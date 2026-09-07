using System;
using UnityEngine;

namespace CY.Vehicle
{
    public static class VehicleSoundEvents
    {
        public static event Action<bool> SteerChanged;
        public static event Action<bool> AccelChanged;
        public static event Action<bool> BrakeChanged;
        public static event Action<Gear> GearChanged;

        private static bool lastSteer;
        private static bool lastAccel;
        private static bool lastBrake;
        private static Gear lastGear = Gear.N;

        // 호스트, 비호스트 상관없이 무조건 이 메서드 하나로 통합합니다.
        public static void Raise(VehicleInput input)
        {
            bool steer = input.Steer != 0f;
            bool accel = input.Accel > 0f;
            bool brake = input.Brake > 0f;
            Gear gear = input.CurrentGear;

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