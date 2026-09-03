using UnityEngine;

namespace CY.Vehicle
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource accelLoopSource;
        [SerializeField] private AudioSource brakeLoopSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip accelClip;
        [SerializeField] private AudioClip brakeClip;
        [SerializeField] private AudioClip uiClickClip; // 공용 버튼 클릭음

        [Header("Crash Clips (Random)")]
        [SerializeField] private AudioClip[] middleCrashClips;
        [SerializeField] private AudioClip[] hardCrashClips;

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

        // --- 공용 UI 클릭음 ---
        public void PlayUIClick()
        {
            if (uiClickClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(uiClickClip);
            }
        }

        private void StartAccelSound()
        {
            if (accelLoopSource == null || accelClip == null) return;
            if (accelLoopSource.clip != accelClip) accelLoopSource.clip = accelClip;
            if (!accelLoopSource.isPlaying) accelLoopSource.Play();
        }

        private void StopAccelSound()
        {
            if (accelLoopSource != null) accelLoopSource.Stop();
        }

        private void StartBrakeSound()
        {
            if (brakeLoopSource == null || brakeClip == null) return;
            if (brakeLoopSource.clip != brakeClip) brakeLoopSource.clip = brakeClip;
            if (!brakeLoopSource.isPlaying) brakeLoopSource.Play();
        }

        private void StopBrakeSound()
        {
            if (brakeLoopSource != null) brakeLoopSource.Stop();
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