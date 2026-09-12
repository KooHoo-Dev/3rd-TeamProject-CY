using System.Collections.Generic;
using UnityEngine;

namespace CY.Stages
{
    public class ObstaclePath : MonoBehaviour
    {
        [SerializeField] private List<Transform> points = new();
    
        [SerializeField] private Color gizmoColor = new Color(1f, 0.65f, 0.2f, 0.9f);
    
        [Tooltip("켜면 마지막에서 처음으로 돌아감. 끄면 왕복")]
        [SerializeField] private bool loop = true;

        public bool Loop => loop;
        public int Count => points.Count;
        public bool IsValid => points.Count >= 2;

        public Vector2 PointAt(int index) => points[index].position;

        private void Awake()
        {
            // 지운 점이 있으면 건너뜀
            int removed = points.RemoveAll(p => p == null);
            
            if (removed > 0)
                Debug.LogWarning($"[ObstaclePath] {name}: 비어있는 지점 {removed} 개를 건너뜁니다", this);
        }

        /// <summary>
        ///  다음 지점의 인덱스. 왕복 경로면 끝에서 direction 이 뒤집힘
        /// </summary>
        /// <param name="index"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public int Advance(int index, ref int direction)
        {
            int next = index + direction;
            if (next >= 0 && next < points.Count) return next;
            if (loop) return direction > 0 ? 0 : points.Count - 1;
            direction = -direction;
            return index + direction;
        }

        [ContextMenu("자식 오브젝트로 채우기")]
        private void FillFromChildren()
        {
            points.Clear();
            foreach (Transform child in transform) points.Add(child);
        }

        private void OnDrawGizmos()
        {
            if (points.Count < 2) return;
            Gizmos.color = gizmoColor;
            for (int i = 0; i < points.Count - 1; i++) DrawLeg(i, i + 1);
            if (loop) DrawLeg(points.Count - 1, 0);

            foreach (Transform point in points)
            {
                if (point != null) Gizmos.DrawSphere(point.position, 0.35f);
            }
        
#if UNITY_EDITOR
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] == null) continue;
                UnityEditor.Handles.Label(points[i].position + Vector3.up * 0.6f, i.ToString());
            }
#endif
        }

        private void DrawLeg(int from, int to)
        {
            if (points[from] == null || points[to] == null) return;
            Vector3 a = points[from].position;
            Vector3 b = points[to].position;
        
            Gizmos.DrawLine(a, b);

            // 진행 방향 화살표 ( 선 중앙에 화살표 넣기 )
            Vector3 mid = (a + b) * 0.5f;
            Vector3 direction = (b - a).normalized;
            Vector3 side = new Vector3(-direction.y, direction.x, 0f) * 0.4f; // 반시계방향 90도 회전
        
            Gizmos.DrawLine(mid, mid - direction * 0.9f + side);
            Gizmos.DrawLine(mid, mid - direction * 0.9f - side);
        }
    }
}

