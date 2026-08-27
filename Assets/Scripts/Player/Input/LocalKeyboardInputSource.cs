using UnityEngine;
using UnityEngine.InputSystem;

public class LocalKeyboardInputSource : MonoBehaviour, IVehicleInputSource
{
    [Header("역할")]
    [SerializeField] private VehicleRole role = VehicleRole.Handle;
    [SerializeField] private bool readAllRoles = true;
    
    public VehicleRole Role => role;
    public VehicleInput Read() => current;
    
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

        VehicleInput full = new VehicleInput
        {
            Steer = steer,
            Accel = kb.spaceKey.isPressed ? 1f : 0f,
            Brake = kb.kKey.isPressed ? 1f : 0f,
            CurrentGear = gear,
        };
        
        current = readAllRoles ? full : MaskToRole(full);
    }

    /// <summary>자기 역할이 담당하는 필드만 남기고 나머지는 비운다.</summary>
    private VehicleInput MaskToRole(VehicleInput full)
    {
        VehicleInput masked = default;

        switch (role)
        {
            case VehicleRole.Handle: masked.Steer       = full.Steer;       break;
            case VehicleRole.Accel:  masked.Accel       = full.Accel;       break;
            case VehicleRole.Brake:  masked.Brake       = full.Brake;       break;
            case VehicleRole.Gear:   masked.CurrentGear = full.CurrentGear; break;
        }
        return masked;
    }
}
