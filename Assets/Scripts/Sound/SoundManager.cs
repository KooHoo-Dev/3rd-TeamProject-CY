using UnityEngine;

namespace CY.Vehicle
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource accelLoopSource;
        [SerializeField] private AudioSource brakeLoopSource;
        [SerializeField] private AudioSource miniGameLoopSource; // [추가] 미니게임 루프용 소스

        [Header("Audio Clips")]
        [SerializeField] private AudioClip accelClip;
        [SerializeField] private AudioClip brakeClip;
        [SerializeField] private AudioClip uiClickClip; // 공용 버튼 클릭음
        
        [Header("Audio Clips - Minigame 1: Fuel (주유)")]
        [SerializeField] private AudioClip fuelLoopClip;
        [SerializeField] private AudioClip fuelSuccessClip;
        [SerializeField] private AudioClip fuelFailClip;

        [Header("Audio Clips - Minigame 2: Barricade (바리케이드)")]
        [SerializeField] private AudioClip barricadeButtonClickClip;
        [SerializeField] private AudioClip barricadeSuccessClip;
        [SerializeField] private AudioClip barricadeFailClip;

        [Header("Audio Clips - Minigame 3 (예정)")]
        [SerializeField] private AudioClip minigame3LoopClip;
        [SerializeField] private AudioClip minigame3SuccessClip;
        [SerializeField] private AudioClip minigame3FailClip;

        [Header("Crash Clips (Random)")]
        [SerializeField] private AudioClip[] middleCrashClips;
        [SerializeField] private AudioClip[] hardCrashClips;
        
        [Header("BGM Clips")]
        [SerializeField] private AudioClip lobbyBgm;             // 로비 BGM
        [SerializeField] private AudioClip[] inGameBgms;         // 인게임 BGM 배열 (4개)
        [SerializeField] private AudioClip gameClearBgm;         // 게임 클리어 BGM
        [SerializeField] private AudioClip gameOverBgm;          // 게임 오버 BGM

        private bool isPlayingInGameBgmList = false;
        private int lastInGameBgmIndex = -1;
        
        // [추가] 동시 입력 중인 플레이어 수 카운팅 변수
        private int activeAccelCount = 0;
        private int activeBrakeCount = 0;

        private void Awake()
        {
            // 중복 방지 및 씬 이동 시 유지 처리
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject); // 이미 존재하면 새로 생긴 것은 파괴
                return;
            }

            if (accelLoopSource != null) accelLoopSource.loop = true;
            if (brakeLoopSource != null) brakeLoopSource.loop = true;
            if (miniGameLoopSource != null) miniGameLoopSource.loop = true;
        }
        
        private void Update()
        {
            // 인게임 BGM 모드일 때, 재생 중인 곡이 끝나면 다음 곡을 랜덤 재생
            if (isPlayingInGameBgmList && bgmSource != null)
            {
                if (!bgmSource.isPlaying)
                {
                    PlayNextRandomInGameBgm();
                }
            }
        }

        private void OnEnable()
        {
            // static 이벤트 구독
            Vehicle.OnAccelStart += StartAccelSound;
            Vehicle.OnAccelStop += StopAccelSound;
            Vehicle.OnBrakeStart += StartBrakeSound;
            Vehicle.OnBrakeStop += StopBrakeSound;

            VehicleDamage.OnMiddleCrash += PlayMiddleCrashSound;
            VehicleDamage.OnHardCrash += PlayHardCrashSound;
        }

        private void OnDisable()
        {
            // static 이벤트 해제
            Vehicle.OnAccelStart -= StartAccelSound;
            Vehicle.OnAccelStop -= StopAccelSound;
            Vehicle.OnBrakeStart -= StartBrakeSound;
            Vehicle.OnBrakeStop -= StopBrakeSound;

            VehicleDamage.OnMiddleCrash -= PlayMiddleCrashSound;
            VehicleDamage.OnHardCrash -= PlayHardCrashSound;
        }
        
        // --- BGM 제어 메서드 ---

        /// <summary> 로비 BGM 재생 (반복 재생) </summary>
        public void PlayLobbyBgm()
        {
            isPlayingInGameBgmList = false;
            PlayBgmSingle(lobbyBgm, isLoop: true);
        }

        /// <summary> 인게임 BGM 랜덤 순환 재생 시작 </summary>
        public void PlayInGameBgm()
        {
            // 1. 기존 재생 중이던 로비 BGM을 무조건 먼저 강제 정지
            if (bgmSource != null)
            {
                bgmSource.Stop();
            }

            // 2. 인게임 BGM 배열 예외 검사
            if (inGameBgms == null || inGameBgms.Length == 0)
            {
                Debug.LogError("[SoundManager] InGameBgms 배열이 비어있습니다! SoundManager 인스펙터에서 4개의 BGM을 등록했는지 확인하세요.");
                return;
            }
            
            isPlayingInGameBgmList = true;
            lastInGameBgmIndex = -1;
            PlayNextRandomInGameBgm();
        }

        /// <summary> 게임 클리어 BGM 재생 </summary>
        public void PlayGameClearBgm()
        {
            isPlayingInGameBgmList = false;
            PlayBgmSingle(gameClearBgm, isLoop: false);
        }

        /// <summary> 게임 오버 BGM 재생 </summary>
        public void PlayGameOverBgm()
        {
            isPlayingInGameBgmList = false;
            PlayBgmSingle(gameOverBgm, isLoop: false);
        }

        /// <summary> BGM 정지 </summary>
        public void StopBgm()
        {
            isPlayingInGameBgmList = false;
            if (bgmSource != null) bgmSource.Stop();
        }

        // --- 내부 BGM 처리 로직 ---

        private void PlayBgmSingle(AudioClip clip, bool isLoop)
        {
            if (bgmSource == null || clip == null) return;

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.loop = isLoop;
            bgmSource.Play();
        }

        private void PlayNextRandomInGameBgm()
        {
            if (inGameBgms == null || inGameBgms.Length == 0 || bgmSource == null) return;

            int randomIndex = Random.Range(0, inGameBgms.Length);

            // 2개 이상일 때 이전 곡 연속 재생 방지
            if (inGameBgms.Length > 1 && randomIndex == lastInGameBgmIndex)
            {
                randomIndex = (randomIndex + 1) % inGameBgms.Length;
            }

            lastInGameBgmIndex = randomIndex;

            bgmSource.Stop();
            bgmSource.clip = inGameBgms[randomIndex];
            bgmSource.loop = false; // 끝나고 다음 곡을 틀기 위해 loop는 false
            bgmSource.Play();
        }

        // --- 공용 UI 클릭음 ---
        public void PlayUIClick()
        {
            if (uiClickClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(uiClickClip);
            }
        }
        
        // --- 범용 미니게임 제어 메서드 ---

        public void StartMiniGameLoop(AudioClip clip)
        {
            if (miniGameLoopSource == null || clip == null) return;

            // 클립이 바뀌었거나, 현재 재생 중이 아니면 무조건 리셋 후 다시 재생
            if (miniGameLoopSource.clip != clip)
            {
                miniGameLoopSource.clip = clip;
                miniGameLoopSource.time = 0f; // 재생 위치 초기화
            }

            if (!miniGameLoopSource.isPlaying)
            {
                miniGameLoopSource.time = 0f; // 항상 시작점부터 재생
                miniGameLoopSource.Play();
            }
        }

        public void StopMiniGameLoop()
        {
            if (miniGameLoopSource != null && miniGameLoopSource.isPlaying)
            {
                miniGameLoopSource.Stop();
            }
        }

        // --- 1. 주유 미니게임 전용 호출 메서드 ---

        public void StartFuelLoopSound() => StartMiniGameLoop(fuelLoopClip);
        
        public void StopFuelLoopSound() => StopMiniGameLoop();

        public void PlayFuelSuccessSound()
        {
            StopMiniGameLoop();
            if (fuelSuccessClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(fuelSuccessClip);
            }
        }

        public void PlayFuelFailSound()
        {
            StopMiniGameLoop();
            if (fuelFailClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(fuelFailClip);
            }
        }
        
        // --- 2. 바리케이드 미니게임 전용 호출 메서드 ---

        /// <summary> 바리케이드 버튼 클릭 효과음 </summary>
        public void PlayBarricadeButtonClickSound()
        {
            if (barricadeButtonClickClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(barricadeButtonClickClip);
            }
        }

        /// <summary> 바리케이드 해체 성공 효과음 </summary>
        public void PlayBarricadeSuccessSound()
        {
            if (barricadeSuccessClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(barricadeSuccessClip);
            }
        }

        /// <summary> 바리케이드 해체 실패 효과음 </summary>
        public void PlayBarricadeFailSound()
        {
            if (barricadeFailClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(barricadeFailClip);
            }
        }

        private void StartAccelSound()
        {
            if (accelLoopSource == null || accelClip == null) return;
    
            activeAccelCount++; // 카운트 증가 추가
            if (accelLoopSource.clip != accelClip) accelLoopSource.clip = accelClip;
            if (!accelLoopSource.isPlaying) accelLoopSource.Play();
        }

        private void StopAccelSound()
        {
            activeAccelCount = Mathf.Max(0, activeAccelCount - 1);

            if (activeAccelCount == 0 && accelLoopSource != null)
            {
                accelLoopSource.Stop();
            }
        }

        private void StartBrakeSound()
        {
            if (brakeLoopSource == null || brakeClip == null) return;
    
            activeBrakeCount++; // 카운트 증가 추가
            if (brakeLoopSource.clip != brakeClip) brakeLoopSource.clip = brakeClip;
            if (!brakeLoopSource.isPlaying) brakeLoopSource.Play();
        }

        private void StopBrakeSound()
        {
            activeBrakeCount = Mathf.Max(0, activeBrakeCount - 1);

            if (activeBrakeCount == 0 && brakeLoopSource != null)
            {
                brakeLoopSource.Stop();
            }
        }

        private void PlayMiddleCrashSound() => PlayRandomClip(middleCrashClips);
        private void PlayHardCrashSound() => PlayRandomClip(hardCrashClips);

        private void PlayRandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0 || sfxSource == null) return;

            int randomIndex = Random.Range(0, clips.Length);
            AudioClip selectedClip = clips[randomIndex];

            if (selectedClip != null)
            {
                sfxSource.PlayOneShot(selectedClip);
            }
        }
    }
}