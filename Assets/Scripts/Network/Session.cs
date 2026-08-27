/// <summary>네트워크 세션 정보. 서버 연동 전에는 기본값(호스트)으로 동작</summary>
public static class Session
{
    public static bool IsHost { get; private set; } = true;
    public static VehicleRole MyRole { get; private set; } = VehicleRole.Handle;
    
    public static void Apply(bool isHost, VehicleRole role)
    {
        IsHost = isHost;
        MyRole = role;
    }
}