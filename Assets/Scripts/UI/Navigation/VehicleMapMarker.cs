using UnityEngine;

namespace CY.UI
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class VehicleMapMarker : MonoBehaviour
    {
        [Header("따라갈 대상")]
        [SerializeField] private Transform vehicle;
        [SerializeField] private float z = -2f;

        [Header("화살표")]
        [SerializeField] private float size = 3f;
        [SerializeField] private Color arrowColor = new Color(0.25f, 0.95f, 0.45f, 1f);
        [SerializeField] private int sortingOrder = 30;

        [Tooltip("비워두면 런타임에 URP 2D Unlit 으로 자동 생성한다.")]
        [SerializeField] private Material markerMaterial;

        [Header("박동")]
        [SerializeField] private float beatPeriod = 1.1f;

        [SerializeField] private float beatAmount = 0.14f;

        [Header("파동")]
        [SerializeField] private bool showPulse = true;
        [SerializeField] private float pulseStartScale = 0.8f;
        [SerializeField] private float pulseEndScale = 2.8f;
        [SerializeField, Range(0f, 1f)] private float pulseAlpha = 0.45f;
        [SerializeField, Range(0.02f, 0.5f)] private float pulseThickness = 0.16f;

        private MeshRenderer arrowRenderer;
        private Mesh arrowMesh;

        private Transform ring;
        private MeshRenderer ringRenderer;
        private Mesh ringMesh;
        private Color[] ringColors;

        private float phase;

        private void Awake()
        {
            if (markerMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                markerMaterial = new Material(shader) { name = "NavMarker (자동 생성)" };
            }

            BuildArrow();
            if (showPulse) BuildRing();
        }

        private void LateUpdate()
        {
            if (vehicle == null) return;

            transform.position = new Vector3(vehicle.position.x, vehicle.position.y, z);
            transform.rotation = vehicle.rotation;

            phase += Time.unscaledDeltaTime / Mathf.Max(0.05f, beatPeriod);
            phase -= Mathf.Floor(phase);

            float beat = 1f + beatAmount * Beat(phase);
            transform.localScale = new Vector3(beat, beat, 1f);

            UpdateRing();
        }

        private static float Beat(float t)
        {
            float first = Mathf.Exp(-22f * t);
            float second = t < 0.16f ? 0f : Mathf.Exp(-22f * (t - 0.16f)) * 0.65f;

            return Mathf.Clamp01(first + second);
        }

        private void BuildArrow()
        {
            arrowRenderer = GetComponent<MeshRenderer>();
            arrowRenderer.sharedMaterial = markerMaterial;
            arrowRenderer.sortingOrder = sortingOrder;
            arrowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            arrowRenderer.receiveShadows = false;

            // 앞쪽이 +Y. 차의 transform.up 이 전방이라 회전을 그대로 물려받으면 된다.
            Vector3[] v =
            {
                new Vector3( 0.00f,  0.55f, 0f) * size,   // 앞 끝
                new Vector3(-0.42f, -0.45f, 0f) * size,   // 왼쪽 뒤
                new Vector3( 0.00f, -0.16f, 0f) * size,   // 뒤 홈
                new Vector3( 0.42f, -0.45f, 0f) * size,   // 오른쪽 뒤
            };

            arrowMesh = new Mesh { name = "NavArrow" };
            arrowMesh.vertices = v;
            arrowMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            arrowMesh.colors = new[] { arrowColor, arrowColor, arrowColor, arrowColor };
            arrowMesh.uv = new[] { Vector2.one, Vector2.one, Vector2.one, Vector2.one };
            arrowMesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = arrowMesh;
        }

        private void BuildRing()
        {
            GameObject go = new GameObject("PulseRing") { layer = gameObject.layer };
            go.transform.SetParent(transform, false);
            ring = go.transform;

            const int segments = 48;
            float inner = 1f - pulseThickness * 0.5f;
            float outer = 1f + pulseThickness * 0.5f;

            Vector3[] v = new Vector3[segments * 2];
            Vector2[] uv = new Vector2[segments * 2];
            int[] tri = new int[segments * 6];
            ringColors = new Color[segments * 2];

            for (int i = 0; i < segments; i++)
            {
                float a = i / (float)segments * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);

                v[i * 2] = dir * inner;
                v[i * 2 + 1] = dir * outer;
                uv[i * 2] = Vector2.one;
                uv[i * 2 + 1] = Vector2.one;

                int n = (i + 1) % segments;
                int t = i * 6;
                tri[t] = i * 2;         tri[t + 1] = i * 2 + 1;  tri[t + 2] = n * 2;
                tri[t + 3] = n * 2;     tri[t + 4] = i * 2 + 1;  tri[t + 5] = n * 2 + 1;
            }

            ringMesh = new Mesh { name = "NavPulseRing" };
            ringMesh.vertices = v;
            ringMesh.uv = uv;
            ringMesh.triangles = tri;
            ringMesh.RecalculateBounds();

            go.AddComponent<MeshFilter>().sharedMesh = ringMesh;

            ringRenderer = go.AddComponent<MeshRenderer>();
            ringRenderer.sharedMaterial = markerMaterial;
            ringRenderer.sortingOrder = sortingOrder - 1;
            ringRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ringRenderer.receiveShadows = false;
        }

        private void UpdateRing()
        {
            if (ring == null) return;

            float scale = Mathf.Lerp(pulseStartScale, pulseEndScale, phase) * size;

            // 부모가 박동으로 커지므로 그만큼 나눠서 파동은 일정하게 퍼지게 한다
            float parent = Mathf.Max(0.0001f, transform.localScale.x);
            ring.localScale = new Vector3(scale / parent, scale / parent, 1f);

            Color c = arrowColor;
            c.a = pulseAlpha * (1f - phase) * (1f - phase);

            for (int i = 0; i < ringColors.Length; i++) ringColors[i] = c;
            ringMesh.colors = ringColors;
        }

        private void OnDestroy()
        {
            if (arrowMesh != null) Destroy(arrowMesh);
            if (ringMesh != null) Destroy(ringMesh);
        }
    }
}
