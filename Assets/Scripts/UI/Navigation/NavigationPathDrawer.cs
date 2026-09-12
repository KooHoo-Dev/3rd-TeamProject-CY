using System.Collections.Generic;
using CY.Stages;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace CY.UI
{
    public enum NavPathMode { Idle = 0, Draw = 1, Erase = 2 }

    public class NavigationPathDrawer : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private RoadGraph graph;
        [SerializeField] private NavigationMapPanel panel;
        [SerializeField] private Camera mapCamera;
        [SerializeField] private RectTransform mapRect;

        [Tooltip("Overlay 캔버스면 비워둔다")]
        [SerializeField] private Camera uiCamera;

        [SerializeField] private Transform vehicle;

        [Header("선")]
        [SerializeField] private LineRenderer confirmedLine;
        [SerializeField] private LineRenderer previewLine;
        [SerializeField] private float lineZ = -1f;
        [SerializeField] private Color drawPreviewColor  = new Color(0.45f, 0.85f, 1f, 0.55f);
        [SerializeField] private Color erasePreviewColor = new Color(1f, 0.35f, 0.30f, 0.75f);
        [SerializeField] private float drawPreviewWidth = 0.5f;

        [Tooltip("확정된 선보다 굵어야 형광펜처럼 보인다")]
        [SerializeField] private float erasePreviewWidth = 1.1f;

        [Header("호버 표시")]
        [SerializeField] private Transform hoverMarker;

        [Header("모드 버튼")]
        [Tooltip("연필·지우개 모드일 때만 보일 것들")]
        [SerializeField] private GameObject[] editOnlyObjects;

        [Header("아이콘")]
        [SerializeField] private GameObject drawIcon;
        [SerializeField] private GameObject doneIcon;
        [SerializeField] private GameObject pencilIcon;
        [SerializeField] private GameObject eraserIcon;

        [Header("설정")]
        [Tooltip("이 거리 안에 교차점이 있어야 잡힌다 (월드 단위)")]
        [SerializeField] private float snapDistance = 4f;

        [Tooltip("차가 이만큼 가까워지면 그 교차점까지 지운다. snapDistance 보다 작게.")]
        [SerializeField] private float eraseDistance = 3f;

        public NavPathMode Mode => mode;

        private readonly List<RoadNode> confirmed = new();

        /// <summary>클릭 한 번마다 그때의 confirmed 길이를 쌓아 둔다. 우클릭 취소가 이걸 되짚는다.</summary>
        private readonly List<int> segmentEnds = new();

        private NavPathMode mode = NavPathMode.Idle;

        private RoadNode lastHovered;
        private RoadNode lastStart;
        private int lastEraseIndex = -1;
        private bool previewDirty = true;

        private void Start()
        {
            ClearLine(confirmedLine);
            ClearLine(previewLine);

            SetMode(NavPathMode.Idle);
        }

        private void Update()
        {
            // 지도를 닫아둔 채 운전해도 지워져야 하므로 지도 상태를 확인하기 전에 돈다
            ConsumePassedNodes();
            UpdateCarAnchor();

            if (IsMapReady() == false)
            {
                if (mode != NavPathMode.Idle) SetMode(NavPathMode.Idle);
                StopHovering();
                return;
            }

            if (mode == NavPathMode.Idle)
            {
                StopHovering();
                return;
            }

            if (TryGetMapWorldPoint(out Vector2 world) == false)
            {
                StopHovering();
                return;
            }

            if (mode == NavPathMode.Draw) UpdateDrawMode(world);
            else UpdateEraseMode(world);
        }

        private bool IsMapReady()
        {
            if (graph == null || mapCamera == null || mapRect == null) return false;
            if (panel == null || panel.IsOpen == false) return false;

            return true;
        }

        public void OnToggleEditModeClicked() =>
            SetMode(mode == NavPathMode.Idle ? NavPathMode.Draw : NavPathMode.Idle);

        public void OnEnterDrawModeClicked() => SetMode(NavPathMode.Draw);

        public void OnExitModeClicked() => SetMode(NavPathMode.Idle);

        public void OnToggleToolClicked() =>
            SetMode(mode == NavPathMode.Erase ? NavPathMode.Draw : NavPathMode.Erase);

        private void SetMode(NavPathMode next)
        {
            mode = next;

            StopHovering();
            RefreshModeUi();
        }

        private void RefreshModeUi()
        {
            bool editing = mode != NavPathMode.Idle;

            SetActive(editOnlyObjects, editing);

            SetActive(drawIcon, editing == false);
            SetActive(doneIcon, editing);

            SetActive(pencilIcon, mode == NavPathMode.Draw);
            SetActive(eraserIcon, mode == NavPathMode.Erase);
        }

        private static void SetActive(GameObject target, bool value)
        {
            if (target != null && target.activeSelf != value) target.SetActive(value);
        }

        private static void SetActive(GameObject[] targets, bool value)
        {
            if (targets == null) return;

            foreach (GameObject target in targets) SetActive(target, value);
        }

        private void UpdateDrawMode(Vector2 world)
        {
            RoadNode hovered = graph.FindNearest(world, snapDistance);
            RoadNode start = CurrentEnd();

            if (hovered != lastHovered || start != lastStart)
            {
                lastHovered = hovered;
                lastStart = start;
                previewDirty = true;
            }

            if (previewDirty)
            {
                RedrawDrawPreview(start, hovered);
                previewDirty = false;
            }

            UpdateHoverMarker(hovered);

            if (IsPointerOverUi()) return;

            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame && hovered != null)
                Confirm(start, hovered);

            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
                UndoLastSegment();

            if (keyboard != null && keyboard.zKey.wasPressedThisFrame)
                ClearPath();
        }

        private void Confirm(RoadNode start, RoadNode target)
        {
            if (start == null) return;

            List<RoadNode> path = graph.FindPath(start, target);
            if (path == null) return;

            // 이어 붙일 때는 path[0] 이 이미 confirmed 의 마지막이라 건너뛴다
            int first = confirmed.Count == 0 ? 0 : 1;

            for (int i = first; i < path.Count; i++)
                confirmed.Add(path[i]);

            if (confirmed.Count == 0) return;

            segmentEnds.Add(confirmed.Count);

            RedrawConfirmed();
            previewDirty = true;
        }

        private void UndoLastSegment()
        {
            if (segmentEnds.Count == 0) return;

            segmentEnds.RemoveAt(segmentEnds.Count - 1);

            int keep = segmentEnds.Count > 0 ? segmentEnds[^1] : 0;
            confirmed.RemoveRange(keep, confirmed.Count - keep);

            RedrawConfirmed();
            previewDirty = true;
        }

        private void ClearPath()
        {
            confirmed.Clear();
            segmentEnds.Clear();

            RedrawConfirmed();
            previewDirty = true;
        }

        private void UpdateEraseMode(Vector2 world)
        {
            int index = FindPathIndexNear(world);

            if (index != lastEraseIndex)
            {
                lastEraseIndex = index;
                previewDirty = true;
            }

            if (previewDirty)
            {
                RedrawErasePreview(index);
                previewDirty = false;
            }

            UpdateHoverMarker(index >= 0 ? confirmed[index] : null);

            if (IsPointerOverUi()) return;

            Mouse mouse = Mouse.current;
            Keyboard keyboard = Keyboard.current;

            if (mouse != null && mouse.leftButton.wasPressedThisFrame && index >= 0)
                EraseFrom(index);

            if (keyboard != null && keyboard.zKey.wasPressedThisFrame)
                ClearPath();
        }

        /// <summary>그려진 경로 위에서 마우스와 가장 가까운 노드의 인덱스. 없으면 -1.</summary>
        private int FindPathIndexNear(Vector2 world)
        {
            int best = -1;
            float bestSqr = snapDistance * snapDistance;

            for (int i = 0; i < confirmed.Count; i++)
            {
                float sqr = (confirmed[i].Position - world).sqrMagnitude;
                if (sqr > bestSqr) continue;

                bestSqr = sqr;
                best = i;
            }

            return best;
        }

        private void EraseFrom(int index)
        {
            if (index < 0 || index >= confirmed.Count) return;

            confirmed.RemoveRange(index, confirmed.Count - index);

            // segmentEnds 는 오름차순이라 뒤에서부터 넘치는 것만 걷어내면 된다
            for (int i = segmentEnds.Count - 1; i >= 0; i--)
            {
                if (segmentEnds[i] <= confirmed.Count) break;
                segmentEnds.RemoveAt(i);
            }

            if (confirmed.Count == 0) segmentEnds.Clear();
            else if (segmentEnds.Count == 0 || segmentEnds[^1] != confirmed.Count)
                segmentEnds.Add(confirmed.Count);

            RedrawConfirmed();
            previewDirty = true;
        }

        private void ConsumePassedNodes()
        {
            if (vehicle == null || confirmed.Count == 0) return;

            Vector2 carPosition = vehicle.position;

            // 지도를 닫고 여러 개를 지나쳤어도 한 번에 걷어내려고 가장 가까운 노드까지 자른다
            int reached = -1;
            float bestSqr = eraseDistance * eraseDistance;

            for (int i = 0; i < confirmed.Count; i++)
            {
                float sqr = (confirmed[i].Position - carPosition).sqrMagnitude;
                if (sqr > bestSqr) continue;

                bestSqr = sqr;
                reached = i;
            }

            if (reached < 0) return;

            RemoveFront(reached + 1);
        }

        private void RemoveFront(int count)
        {
            if (count <= 0) return;

            count = Mathf.Min(count, confirmed.Count);
            confirmed.RemoveRange(0, count);

            // segmentEnds 는 confirmed 의 인덱스라 앞이 잘리면 같이 당겨야 한다
            for (int i = segmentEnds.Count - 1; i >= 0; i--)
            {
                segmentEnds[i] -= count;
                if (segmentEnds[i] <= 0) segmentEnds.RemoveAt(i);
            }

            RedrawConfirmed();
            previewDirty = true;
            lastEraseIndex = -1;
        }

        private bool TryGetMapWorldPoint(out Vector2 world)
        {
            world = default;

            Mouse mouse = Mouse.current;
            if (mouse == null) return false;

            Vector2 screen = mouse.position.ReadValue();

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    mapRect, screen, uiCamera, out Vector2 local) == false) return false;

            Rect rect = mapRect.rect;
            if (rect.Contains(local) == false) return false;

            // 패널 안에서의 위치를 0~1 로 바꾸면 그게 곧 맵 카메라의 뷰포트 좌표다
            Vector3 viewport = new Vector3(
                (local.x - rect.xMin) / rect.width,
                (local.y - rect.yMin) / rect.height,
                0f);

            world = mapCamera.ViewportToWorldPoint(viewport);
            return true;
        }

        /// <summary>버튼 위에서 누른 클릭이 지도 클릭으로도 처리되는 걸 막는다.</summary>
        private static bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>다음 구간이 출발할 지점. 경로가 비어 있으면 차와 가장 가까운 교차점.</summary>
        private RoadNode CurrentEnd()
        {
            if (confirmed.Count > 0) return confirmed[^1];
            if (vehicle == null) return null;

            return graph.FindNearest(vehicle.position, Mathf.Infinity);
        }

        private void RedrawConfirmed()
        {
            if (confirmedLine == null) return;

            if (confirmed.Count == 0)
            {
                confirmedLine.positionCount = 0;
                return;
            }

            // 0번은 차 위치, 1번부터가 남은 교차점들
            bool useCar = vehicle != null;
            int offset = useCar ? 1 : 0;

            confirmedLine.positionCount = confirmed.Count + offset;

            if (useCar)
                confirmedLine.SetPosition(0, new Vector3(vehicle.position.x, vehicle.position.y, lineZ));

            for (int i = 0; i < confirmed.Count; i++)
                confirmedLine.SetPosition(i + offset,
                    new Vector3(confirmed[i].Position.x, confirmed[i].Position.y, lineZ));
        }

        private void UpdateCarAnchor()
        {
            if (confirmedLine == null || vehicle == null) return;
            if (confirmedLine.positionCount == 0) return;

            confirmedLine.SetPosition(0, new Vector3(vehicle.position.x, vehicle.position.y, lineZ));
        }

        private void RedrawDrawPreview(RoadNode start, RoadNode hovered)
        {
            if (hovered == null || start == null || hovered == start)
            {
                ClearLine(previewLine);
                return;
            }

            List<RoadNode> path = graph.FindPath(start, hovered);

            if (path == null) { ClearLine(previewLine); return; }

            SetPreviewStyle(drawPreviewColor, drawPreviewWidth);

            // 새 경로를 시작할 때는 확정선처럼 차 위치를 0번 점으로 넣는다
            bool fromCar = confirmed.Count == 0 && vehicle != null;
            int offset = fromCar ? 1 : 0;

            previewLine.positionCount = path.Count + offset;

            if (fromCar)
                previewLine.SetPosition(0, new Vector3(vehicle.position.x, vehicle.position.y, lineZ));

            for (int i = 0; i < path.Count; i++)
                previewLine.SetPosition(i + offset,
                    new Vector3(path[i].Position.x, path[i].Position.y, lineZ));
        }

        private void RedrawErasePreview(int index)
        {
            if (index < 0 || previewLine == null)
            {
                ClearLine(previewLine);
                return;
            }

            // index 를 지우면 그 앞 지점에서 들어오던 선분도 함께 사라지므로 한 칸 앞에서 시작한다
            bool fromCar = index == 0 && vehicle != null;
            int start = Mathf.Max(0, index - 1);

            SetPreviewStyle(erasePreviewColor, erasePreviewWidth);
            previewLine.positionCount = confirmed.Count - start + (fromCar ? 1 : 0);

            int offset = 0;

            if (fromCar)
            {
                previewLine.SetPosition(0, new Vector3(vehicle.position.x, vehicle.position.y, lineZ));
                offset = 1;
            }

            for (int i = start; i < confirmed.Count; i++)
                previewLine.SetPosition(i - start + offset,
                    new Vector3(confirmed[i].Position.x, confirmed[i].Position.y, lineZ));
        }

        private void SetPreviewStyle(Color color, float width)
        {
            if (previewLine == null) return;

            previewLine.startColor = color;
            previewLine.endColor = color;
            previewLine.widthMultiplier = width;
        }

        private void SetLine(LineRenderer line, IReadOnlyList<RoadNode> nodes, int from)
        {
            if (line == null) return;

            int count = nodes.Count - from;

            if (count <= 0)
            {
                line.positionCount = 0;
                return;
            }

            line.positionCount = count;

            for (int i = 0; i < count; i++)
                line.SetPosition(i, new Vector3(
                    nodes[from + i].Position.x, nodes[from + i].Position.y, lineZ));
        }

        private static void ClearLine(LineRenderer line)
        {
            if (line != null) line.positionCount = 0;
        }

        private void StopHovering()
        {
            ClearLine(previewLine);
            SetMarkerVisible(false);

            lastHovered = null;
            lastStart = null;
            lastEraseIndex = -1;
            previewDirty = true;
        }

        private void UpdateHoverMarker(RoadNode hovered)
        {
            if (hoverMarker == null) return;

            SetMarkerVisible(hovered != null);

            if (hovered != null)
                hoverMarker.position = new Vector3(hovered.Position.x, hovered.Position.y, lineZ);
        }

        private void SetMarkerVisible(bool visible)
        {
            if (hoverMarker != null && hoverMarker.gameObject.activeSelf != visible)
                hoverMarker.gameObject.SetActive(visible);
        }
    }
}
