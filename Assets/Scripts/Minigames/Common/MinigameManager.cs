using System;
using UnityEngine;

public class MinigameManager : MonoBehaviour
{
    //private RoomClient roomClient;
    private bool isInsideFuelZone = false;
    
    public string CurrentMiniGameType { get; private set; }

    public void Awake()
    {
        //roomClient = FindObjectOfType<RoomClient>();
    }
    
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FuelZone"))
        {
            isInsideFuelZone = true;
            Debug.Log("주유 구역 진입! 중립(N) 기어를 넣으면 게임이 시작됩니다.");
        }
    }
    
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("FuelZone"))
        {
            isInsideFuelZone = false;
        }
    }
    
    // private void Update()
    // {
    //     // 기어를 N단으로 바꾸는 상황을 'N' 키 입력으로 시뮬레이션
    //     if (isInsideFuelZone && Input.GetKeyDown(KeyCode.N))
    //     {
    //         RequestMiniGameStart("fuel");
    //     }
    // }
    // 
    // public void RequestMiniGameStart(string minigameType)
    // {
    //     CurrentMiniGameType = minigameType;
    //     Debug.Log($"서버로 {minigameType} 미니게임 시작 요청 보냄 (minigame_start_request)[cite: 1]");
    //     
    //     // 실제 네트워크 연동 시에는 아래 코드를 사용
    //     // roomClient.SendMinigameStartRequest(minigameType);
    // 
    //     // [가짜 서버 연동] 테스트 모드가 켜져 있다면 가짜 서버에게 바로 시작을 지시
    //     var controller = FindObjectOfType<GasStationMinigameController>();
    //     if (controller != null && controller.useMockServer)
    //     {
    //         controller.MockServer_StartGame();
    //     }
    // }
}
