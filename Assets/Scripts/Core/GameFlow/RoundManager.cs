using CY.Network;
using CY.Stages;
using CY.Vehicle;
using UnityEngine;
using UnityEngine.Events;

namespace CY.Core
{
    public class RoundManager : MonoBehaviour
    {
        [Header("참조")] 
        [SerializeField] private RoundTimer timer;
        [SerializeField] private VehicleController vehicle;
        [SerializeField] private Transform startPoint;
        [SerializeField] private CheckpointZone[] checkpoints;

        [Header("결과 신호")] 
        [SerializeField] private UnityEvent onRoundSucceeded;
        [SerializeField] private UnityEvent onRoundFailed;

        public int ClearedCount { get; private set; }
        public int TotalCheckpoints => checkpoints.Length;
        public bool IsFinished => finished;
        public RoundPhase Phase { get; private set; } = RoundPhase.Waiting;

        private bool finished;
        private VehicleDamage damage;
        
        // [추가] 인게임 씬에서 시작할 때 네트워크 구독
        private void OnEnable()
        {
            if (NetworkSession.Instance != null)
                NetworkSession.Instance.MessageReceived += OnNetworkMessageReceived;
        }

        // [추가] 네트워크 구독 해제
        private void OnDisable()
        {
            if (NetworkSession.Instance != null)
                NetworkSession.Instance.MessageReceived -= OnNetworkMessageReceived;
        }
        
        // [추가] BGM이 1초에 수십 번씩 다시 시작되는 것을 막아줄 자물쇠
        private bool hasPlayedEndBgm = false;

        // [추가] 비호스트 클라이언트가 성공/실패 패킷을 받아 BGM 재생
        private void OnNetworkMessageReceived(string type, string json)
        {
            // Debug.Log($"[네트워크 수신 테스트] 패킷 도착! 타입: {type} / 내용: {json}");
            
            if (Session.IsHost || type != "round_state") return;
            
            if (Session.IsHost) return;

            // 서버가 이미 잘 보내주고 있는 'round_state'를 활용해서 BGM을 틉니다!
            if (json.Contains("\"Phase\":\"Success\"") ||
                json.Contains("\"Phase\": \"Success\""))
            {
                if (hasPlayedEndBgm)
                    return;

                hasPlayedEndBgm = true;
                SoundManager.Instance?.PlayGameClearBgm();
            }
            else if (json.Contains("\"Phase\":\"Failed\"") ||
                     json.Contains("\"Phase\": \"Failed\""))
            {
                if (hasPlayedEndBgm)
                    return;

                hasPlayedEndBgm = true;
                SoundManager.Instance?.PlayGameOverBgm();
            }
        }

        private void Start()
        {
            // --- [1. BGM 재생 (모든 플레이어 공통)] ---
            // 호스트 여부 검사 전에 호출해야 클라이언트 플레이어도 인게임 BGM을 듣는다.
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.StopBgm();       // 혹시 남아있는 로비 BGM 즉시 정지
                SoundManager.Instance.PlayInGameBgm(); // 인게임 BGM 4종 중 1개 무작위 재생
            }
            
            if (!Session.IsHost)
            {
                //enabled = false;
                return;
            }

            foreach (CheckpointZone zone in checkpoints)
                zone.Bind(this);

            damage = vehicle.GetComponent<VehicleDamage>();
            damage?.Bind(this);

            StartRound();
        }

        public void StartRound()
        {
            finished = false;
            ClearedCount = 0;

            foreach (CheckpointZone zone in checkpoints)
                if (zone != null) zone.ResetZone();

            MoveVehicleToStart();
            damage?.ResetDurability();
            vehicle.SetInputEnabled(true);
            timer.StartRound();
            Phase = RoundPhase.Playing;
            
            Debug.Log("[Round] 라운드 시작", this);
        }

        private void MoveVehicleToStart()
        {
            Rigidbody2D rb = vehicle.GetComponent<Rigidbody2D>();
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = startPoint.position;
            rb.rotation = startPoint.eulerAngles.z;
        }

        /// <summary>CheckpointZone 이 미션 완료 시 호출한다.</summary>
        public void OnCheckpointCleared(CheckpointZone zone)
        {
            if (finished) return;

            ClearedCount++;
            Debug.Log($"[Round] 경유지 {ClearedCount}/{TotalCheckpoints} 완료 ({zone.MissionType})", this);
        }

        /// <summary>DestinationZone 이 차량 진입 시 호출한다.</summary>
        public void OnDestinationReached()
        {
            if (finished) return;

            if (ClearedCount < TotalCheckpoints)
            {
                Debug.Log($"[Round] 경유지 미완료 {ClearedCount}/{TotalCheckpoints} — 도착 무효", this);
                return;
            }

            Finish(true);
        }

        /// <summary>제한 시간 소진, 내구도 0 등 실패 원인에서 호출한다.</summary>
        public void FailRound()
        {
            if (finished) return;
            Finish(false);
        }

        private void Finish(bool success)
        {
            finished = true;
            timer.StopRound();
            vehicle.SetInputEnabled(false);
            
            // 호스트가 비호스트(클라이언트)들에게 게임 종료/성공 여부를 방송
            if (Session.IsHost && NetworkSession.Instance != null)
            {
                string msgType = success ? "round_success" : "round_failed";
                _ = NetworkSession.Instance.SendAsync
                    (new { type = msgType });
            }

            if (success)
            {
                Phase = RoundPhase.Success;
                
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayGameClearBgm();
                
                onRoundSucceeded.Invoke();
                Debug.Log($"[Round] 성공! 소요 시간 {timer.ElapsedTime:0.#}초", this);
            }
            else
            {
                Phase = RoundPhase.Failed;
                
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlayGameOverBgm();
                
                onRoundFailed.Invoke();
                Debug.Log($"[Round] 실패. 경유지 {ClearedCount}/{TotalCheckpoints}", this);
            }
        }
    }
}