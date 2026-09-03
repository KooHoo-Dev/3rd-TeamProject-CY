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
    [SerializeField] private AudioClip crashClip; // 단일 충돌 클립

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (accelLoopSource != null) accelLoopSource.loop = true;
        if (brakeLoopSource != null) brakeLoopSource.loop = true;
    }

    private void OnEnable()
    {
        Vehicle.OnAccelStart += StartAccelSound;
        Vehicle.OnAccelStop += StopAccelSound;
        Vehicle.OnBrakeStart += StartBrakeSound;
        Vehicle.OnBrakeStop += StopBrakeSound;

        // 충돌은 VehicleDamage의 static 이벤트를 구독
        CY.Vehicle.VehicleDamage.OnCrash += PlayCrashSound;
    }

    private void OnDisable()
    {
        Vehicle.OnAccelStart -= StartAccelSound;
        Vehicle.OnAccelStop -= StopAccelSound;
        Vehicle.OnBrakeStart -= StartBrakeSound;
        Vehicle.OnBrakeStop -= StopBrakeSound;

        CY.Vehicle.VehicleDamage.OnCrash -= PlayCrashSound;
    }

    private void StartAccelSound()
    {
        if (accelLoopSource.clip != accelClip) accelLoopSource.clip = accelClip;
        if (!accelLoopSource.isPlaying) accelLoopSource.Play();
    }

    private void StopAccelSound() { accelLoopSource.Stop(); }

    private void StartBrakeSound()
    {
        if (brakeLoopSource.clip != brakeClip) brakeLoopSource.clip = brakeClip;
        if (!brakeLoopSource.isPlaying) brakeLoopSource.Play();
    }

    private void StopBrakeSound() { brakeLoopSource.Stop(); }

    // 충돌 사운드 재생
    private void PlayCrashSound()
    {
        Debug.Log("[SoundManager] 충돌 사운드 재생 시도");

        if (crashClip == null)
        {
            Debug.LogError("[SoundManager] Crash Clip이 인스펙터에 할당되지 않았습니다!");
            return;
        }

        sfxSource.PlayOneShot(crashClip);
    }
}
}

