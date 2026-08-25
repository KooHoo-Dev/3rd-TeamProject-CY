using UnityEngine;
using UnityEngine.InputSystem;

public class LocalKeyboardInputSource : MonoBehaviour, IVehicleInputSource
{
    [SerializeField] private VehicleInput current;
    private Gear gear = Gear.N;

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        float steer = 0f;
        if (kb.aKey.isPressed) steer -= 1f;
        if (kb.dKey.isPressed) steer += 1f;

        if (kb.upArrowKey.wasPressedThisFrame) gear = Gear.D;
        if (kb.downArrowKey.wasPressedThisFrame) gear = Gear.R;
        if (kb.leftArrowKey.wasPressedThisFrame) gear = Gear.N;

        current = new VehicleInput
        {
            Steer = steer,
            Accel = kb.spaceKey.isPressed ? 1f : 0f,
            Brake = kb.kKey.isPressed ? 1f : 0f,
            CurrentGear = gear,
        };
    }

    public VehicleInput Read() => current;
}
