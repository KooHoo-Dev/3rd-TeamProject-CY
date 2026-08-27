using CY.Player;

namespace CY.Vehicle
{
    public class VehicleInputMerger
    {
        private readonly VehicleInput[] byRole = new VehicleInput[4];

        /// <summary>한 역할의 최신 입력을 갱신한다. 갱신되지 않은 역할은 직전 값을 유지한다.</summary>
        public void Set(VehicleRole role, VehicleInput input)
        {
            byRole[(int)role] = input;
        }

        /// <summary>각 역할이 담당하는 필드만 뽑아 하나로 합친다.</summary>
        public VehicleInput Merge()
        {
            return new VehicleInput
            {
                Steer = byRole[(int)VehicleRole.Handle].Steer,
                Accel = byRole[(int)VehicleRole.Accel].Accel,
                Brake = byRole[(int)VehicleRole.Brake].Brake,
                CurrentGear = byRole[(int)VehicleRole.Gear].CurrentGear,
            };
        }
    }
}