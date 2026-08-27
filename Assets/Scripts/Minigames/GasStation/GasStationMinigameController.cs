using UnityEngine;

public class MiniGameStartedMessage { public string MiniGameType; public string OperatorId; }
public class FuelStateMessage { public float GaugePercent; public bool IsPressed; }

[RequireComponent(typeof(GasStationMinigameView))]
public class GasStationMinigameController : MonoBehaviour
{
    private GasStationMinigameView view;
    //private RoomClient roomClient;

    private bool IsOperator;
    private bool IsActive;
    private bool CanSendInput;
    
    private string myUserId = "임시_로컬_아이디";

    private void Awake()
    {
        view = GetComponent<GasStationMinigameView>();
        //roomClient = FindObjectOfType<RoomClient>();
        
        //if (roomClient == null)
        {
            Debug.LogWarning("씬에 RoomClient가 없습니다. 네트워크 연동 시 확인해주세요.");
        }
    }

    private void Start()
    {
        ResetState();
    }
    
    public void SendFuelPress()
    {
        if (!IsActive || !IsOperator || !CanSendInput)
            return;

        //roomClient.SendFuelPress();
    }

    public void SendFuelRelease()
    {
        if (!IsActive || !IsOperator || !CanSendInput)
            return;

        //roomClient.SendFuelRelease();
    }

    public void HandleMiniGameStarted(MiniGameStartedMessage message)
    {
        if (message.MiniGameType != "fuel") return;

        ResetState();
        IsActive = true;
        
        IsOperator = (myUserId == message.OperatorId);
        
        CanSendInput = IsOperator;
        
        view.Begin(IsOperator);
    }

    public void HandleFuelState(FuelStateMessage message)
    {
        if (!IsActive) return;
        
        view.ApplyFuelState(message.GaugePercent, message.IsPressed);

    }

    public void HandleFuelAttemptFailed()
    {
        if (!IsActive) return;
        
        view.ApplyAttemptFailed();

    }

    public void HandleFuelCompleted()
    {
        if (!IsActive) return;
        
        CanSendInput = false;
        view.ApplyCompleted();

    }

    public void HandleGameAborted()
    {
        if (!IsActive) return;
        
        CanSendInput = false; 
        view.ApplyAborted(); 
        ResetState();

    }

    private void ResetState()
    {
        IsActive = false; 
        IsOperator = false; 
        CanSendInput = false; 

        if (view != null)
            view.ResetView(); 
    }
}

