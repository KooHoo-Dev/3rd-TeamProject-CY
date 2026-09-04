using System;
using UnityEngine;

namespace CY.Vehicle
{
    /// <summary>
    /// 차 소리용 이벤트 허브. 호스트는 병합 입력(Raise)으로, 비호스트는 상태값 추정(RaiseFromState)으로
    /// 같은 이벤트를 쏘고, SoundManager 는 여기만 구독한다.
    /// </summary>
    public static class VehicleSoundEvents
    {
        public static event Action<bool> SteerChanged;   // 핸들 돌리는 중 여부
        public static event Action<bool> AccelChanged;   // 액셀 밟는 중 여부
        public static event Action<bool> BrakeChanged;   // 브레이크 밟는 중 여부
        public static event Action<Gear> GearChanged;    // 기어 변속

        // ── 상태 추정 민감도. 15Hz 스냅샷 한 칸 기준이라 잘 안 잡히면 여기부터 조절 ──
        private const float AccelSpeedDelta = 0.1f;   // 이만큼 빨라지면 가속 중
        private const float BrakeSpeedDelta = 0.25f;  // 이만큼 느려지면 제동 중
        private const float SteerAngleDelta = 1f;     // 이만큼(도) 돌면 조향 중
        private const float GearSpeedThreshold = 0.3f; // 전/후진 판정 최소 속도

        private static bool lastSteer;
        private static bool lastAccel;
        private static bool lastBrake;
        private static Gear lastGear = Gear.N;

        private static float lastSpeed;
        private static float lastRotation;
        private static bool hasStateSample;

        /// <summary>호스트·로컬용. 물리에 적용되는 병합 입력을 그대로 넘긴다.</summary>
        public static void Raise(VehicleInput input)
        {
            Fire(input.Steer != 0f, input.Accel > 0f, input.Brake > 0f, input.CurrentGear);
        }

        /// <summary>비호스트용. vehicle_state 값으로 무슨 조작이었는지 추정한다.</summary>
        public static void RaiseFromState(float rotation, Vector2 velocity)
        {
            float speed = velocity.magnitude;

            if (hasStateSample == false)
            {
                // 첫 스냅샷은 비교 대상이 없으므로 기록만 한다
                hasStateSample = true;
                lastSpeed = speed;
                lastRotation = rotation;
                return;
            }

            bool accel = speed - lastSpeed > AccelSpeedDelta;
            bool brake = lastSpeed - speed > BrakeSpeedDelta;
            bool steer = Mathf.Abs(Mathf.DeltaAngle(lastRotation, rotation)) > SteerAngleDelta;

            // 차가 향한 방향 대비 속도의 부호로 전/후진을 추정. 느릴 때는 요동치니 이전 기어 유지
            Vector2 forward = Quaternion.Euler(0f, 0f, rotation) * Vector2.up;
            float forwardSpeed = Vector2.Dot(velocity, forward);

            Gear gear = lastGear;
            if (forwardSpeed > GearSpeedThreshold) gear = Gear.D;
            else if (forwardSpeed < -GearSpeedThreshold) gear = Gear.R;

            lastSpeed = speed;
            lastRotation = rotation;

            Fire(steer, accel, brake, gear);
        }

        private static void Fire(bool steer, bool accel, bool brake, Gear gear)
        {
            if (steer != lastSteer) { lastSteer = steer; SteerChanged?.Invoke(steer); }
            if (accel != lastAccel) { lastAccel = accel; AccelChanged?.Invoke(accel); }
            if (brake != lastBrake) { lastBrake = brake; BrakeChanged?.Invoke(brake); }

            if (gear != lastGear)
            {
                lastGear = gear;
                GearChanged?.Invoke(gear);
            }
        }
    }
}