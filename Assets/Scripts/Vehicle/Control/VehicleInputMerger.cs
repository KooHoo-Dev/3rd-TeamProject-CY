using CY.Player;

namespace CY.Vehicle
{
    public class VehicleInputMerger
    {
        private readonly VehicleInput[] byRole = new VehicleInput[4];

        public void Set(VehicleRole role, VehicleInput input)
        {
            int index = (int)role;
            if (index < 0 || index >= byRole.Length) return;
            
            byRole[index] = input;
        }

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