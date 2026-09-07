using UnityEngine;
using UnityEngine.UIElements;

namespace CY.Stages
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class TrafficAgent : MonoBehaviour
    {
        [Header("경로")] 
        [SerializeField] private ObstaclePath path;
        [SerializeField] private float speed = 6f;
        [Tooltip("이만큼 가까워지면 다음 지점으로 넘어간다")] 
        [SerializeField] private float arriveRadius = 1.5f;
        [Tooltip("여러 대를 같은 경로에 놓을 때 출발 지점 출발 지점을 어긋나게 세팅")] 
        [SerializeField] private int startIndex;

        [Header("회전")] [Tooltip("스프라이트가 그려진 방향 보정. 위를 바라보는 스프라이트면 -90, 오른쪽 바라보는 스프라이트면 0")] 
        [SerializeField] private float spriteAngleOffset = -90f;
        [Tooltip("차가 가는 쪽을 바라보게 할 것인지")]
        [SerializeField] private bool faceMoveDirection = true;
        [Tooltip("초당 회전 각도. 낮을 수록 크게 돈다")] 
        [SerializeField] private float turnRate = 200f;

        [Header("기물 회피")] 
        [SerializeField] private bool avoidProps = true;
        [Tooltip("앞을 살피는 거리")] 
        [SerializeField] private float lookAhead = 4.5f;
        [Tooltip("차폭의 절반. 이 반경으로 앞을 훑는다")] 
        [SerializeField] private float bodyRadius = 0.8f;
        [Tooltip("좌우로 벌린 탐지선 각도")]
        [SerializeField] private float whiskerAngle = 32f;
        [Tooltip("클수록 기물을 크게 돌아감")]
        [SerializeField] private float avoidStrength = 2.5f;
        
        private Rigidbody2D rb;
        private Vector2 heading = Vector2.up;
        private int index;
        private int direction = 1;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 경로읽기는 Start 에서. ObstaclePath 가 Awake 에서 빈 지점을 걸러낸 뒤여야 하므로
        private void Start()
        {
            if (path == null || path.IsValid == false)
            {
                Debug.LogError($"{name}: Path 가 없거나 지점이 2개 미만임", this);
                enabled = false;
                return;
            }

            if (startIndex < 0 || startIndex >= path.Count)
            {
                Debug.LogWarning($"{name}: Start Index {startIndex} 가 범위 밖임. 0으로 시작합니다", this);
                startIndex = 0;
            }

            index = startIndex;

            Vector2 first = path.PointAt(index) - (Vector2)transform.position;
            if (first.sqrMagnitude > 0.001f) heading = first.normalized;
        }

        private void FixedUpdate()
        {
            Vector2 position = rb.position;
            Vector2 target = path.PointAt(index);

            if (Vector2.Distance(position, target) <= arriveRadius)
            {
                index = path.Advance(index, ref direction);
                target = path.PointAt(index); // 이미 도착한 점을 향해 가려고 한 프레임을 낭비하는 것 방지
            }
            
            Vector2 desired = (target - position).normalized;
            if (avoidProps) desired = (desired + Avoid(position) * avoidStrength).normalized;
            
            // 급하게 꺾이지 않도록 각속도를 제한함
            float step = turnRate * Time.fixedDeltaTime;
            heading = Vector3.RotateTowards(heading, desired, step * Mathf.Deg2Rad, 0f);

            rb.MovePosition(position + heading * (speed * Time.fixedDeltaTime));

            if (faceMoveDirection)
            {
                // 스프라이트의 방향에 맞춰 보정
                float angle = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg + spriteAngleOffset;
                rb.MoveRotation(Mathf.MoveTowardsAngle(rb.rotation, angle, step));
            }
        }

        /// <summary>
        /// 앞의 정지 기물에서 멀어지는 방향을 구한다. Rigidbody2D 가 없는 콜라이더가 정지 기물임.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector2 Avoid(Vector2 position)
        {
            Vector2 away = Vector2.zero;
            
            // 정면과 좌우로 앞을 살핀다. 정면이 더 급하니 좌우는 약하게 반영함
            away += Whisker(position, heading);
            away += Whisker(position, Rotate(heading, whiskerAngle)) * 0.6f;
            away += Whisker(position, Rotate(heading, -whiskerAngle)) * 0.6f;
            
            return away;
        }

        /// <summary>
        /// 한 방향으로 앞을 살펴서 정지 기물을 만나면 그 기물에서 멀어지는 방향을 돌려준다
        /// </summary>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private Vector2 Whisker(Vector2 position, Vector2 direction)
        {
            RaycastHit2D hit = Physics2D.CircleCast(position, bodyRadius, direction, lookAhead);

            if (hit.collider == null) return Vector2.zero;
            if (hit.rigidbody != null) return Vector2.zero; // 플레이어나 다른 AI 는 여기서 피하지 않음. IsBlockedByMover 에서 처리

            // 가까울수록 크게 비껴감
            float weight = 1f - Mathf.Clamp01(hit.distance / lookAhead);

            // hit.normal 은 부딪힌 곳에서 벗어나는 쪽
            return hit.normal * weight;
        }

        private static Vector2 Rotate(Vector2 v, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad;
            float c = Mathf.Cos(r);
            float s = Mathf.Sin(r);
            
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.4f, 0.3f, 0.8f);
            Vector3 h = Application.isPlaying ? (Vector3)heading : transform.up;
            Gizmos.DrawLine(transform.position, transform.position + h * lookAhead);
            Gizmos.DrawWireSphere(transform.position + h * lookAhead, bodyRadius);
        }
    }
}

