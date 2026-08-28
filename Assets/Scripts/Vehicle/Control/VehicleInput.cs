using System;

namespace CY.Vehicle
{
    public enum Gear
    {
        N = 0,
        D = 1,
        R = 2
    }

    [Serializable]
    public struct VehicleInput
    {
        public float Steer; // -1 (좌) ~ +1 (우)
        public float Accel; // 0 ~ 1
        public float Brake; // 0 ~ 1
        public Gear CurrentGear;
    }
}