using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Utilities;
using HelloServer;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace CY.Network
{
    public class NetworkSession : SingletonBase<NetworkSession>
    {
        [Header("서버")]
        // 웹소켓 주소. http 가 아니라 ws로 시작.
        [SerializeField] private string url = "ws://34.50.63.204:5001/room";
        
         private string nickName = "";
         private string roomCode = "ABCD";
         
         public User Me { get; private set; }
         public string RoomCode { get; private set; }
         public bool IsBusy { get; private set; }
         
         public bool IsConnected => 
             socket != null 
             && socket.State == WebSocketState.Open 
             && Me != null;
         
         public int CurrentSceneVersion { get; private set; }
         
         private ClientWebSocket socket;
         private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);
         private readonly object stateSendGate = new object();
         private readonly Dictionary<string, string> latestStateMessages = new Dictionary<string, string>();
         private readonly ConcurrentQueue<ReceivedMessage> eventMessages = new();
         private readonly ConcurrentDictionary<string, string> latestReceivedStates = new();
         private struct ReceivedMessage
         {
             public string Type;
             public string Json;
         }
         private bool stateSendLoopRunning;
         private float sendTimeoutSeconds = 2f;
         
         private readonly Dictionary<String, User> participants = new Dictionary<string, User>();
         public IReadOnlyDictionary<String, User> Participants => participants;
         
         public event Action<string, string> MessageReceived;

         protected override void OnInitialize()
         {
             base.OnInitialize();
             Application.runInBackground = true;
         }
         
         public void Join(string code, string who)
         {
             if (IsBusy) return;

             roomCode = code;
             nickName = who;

             RunAsync().Forget();
         }

         private async UniTaskVoid RunAsync()
         {
             IsBusy = true;
             socket = new ClientWebSocket();
             
             string full = $"{url}?code={UnityWebRequest.EscapeURL(roomCode)}";

             try
             {
                 await socket.ConnectAsync(new Uri(full), destroyCancellationToken).AsUniTask();
             }
             catch (Exception e)
             { 
                 Debug.LogWarning($"[방] 접속하지 못했다. 서버가 켜져 있는지, 방 코드가 맞는지 확인 : {e.Message}");
                 
                 IsBusy = false;
                 return;
             }
             
             Debug.Log($"[방] {full} 에 접속.  : {nickName}");
             await SendAsync(new HelloMessage { NickName = nickName });
             await ReceiveLoopAsync();
             await UniTask.SwitchToMainThread();

             Debug.LogWarning($"[방] 수신 루프 종료. socket={socket?.State}, me={Me?.Id ?? "null"}");
             
             IsBusy = false;
             Me = null;
             participants.Clear();
         }

         private async UniTask ReceiveLoopAsync()
         {
             ClientWebSocket receivingSocket = socket;
             if (receivingSocket == null) return;
             
             CancellationToken token = destroyCancellationToken;
             
             byte[] buffer = new byte[4096 * 4];
             StringBuilder builder = new StringBuilder();

             while (receivingSocket.State == WebSocketState.Open)
             {
                 WebSocketReceiveResult result;

                 try
                 {
                     result = await receivingSocket
                         .ReceiveAsync(new ArraySegment<byte>(buffer), token)
                         .ConfigureAwait(false);
                 }
                 catch (OperationCanceledException)
                 {
                     return;
                 }
                 catch (WebSocketException)
                 {
                     Debug.LogWarning("[방] 연결이 끊겼다");
                     return;
                 }
                 catch (ObjectDisposedException)
                 {
                     return;
                 }
                 
                 if (result.MessageType == WebSocketMessageType.Close) return;

                 builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                 
                 if (result.EndOfMessage == false) continue;

                 string json = builder.ToString();
                 builder.Clear();

                 QueueReceivedMessage(json);
             }
         }
         
         private void QueueReceivedMessage(string json)
         {
             TypeOnly header = JsonConvert.DeserializeObject<TypeOnly>(json);

             if (header == null || string.IsNullOrEmpty(header.Type))
                 return;

             string stateKey = GetReceivedStateKey(header.Type, json);

             if (stateKey != null)
             {
                 latestReceivedStates[stateKey] = json;
                 return;
             }

             eventMessages.Enqueue(new ReceivedMessage
             {
                 Type = header.Type,
                 Json = json
             });
         }

         private static string GetReceivedStateKey(string type, string json)
         {
             switch (type)
             {
                 case "vehicle_input":
                 {
                     VehicleInputMessage message =
                         JsonConvert.DeserializeObject<VehicleInputMessage>(json);

                     return $"vehicle_input:{message.UserId}";
                 }

                 case "vehicle_state":
                     return "vehicle_state";

                 case "round_state":
                     return "round_state";

                 case "traffic_state":
                     return "traffic_state";

                 case "state":
                     return "state";

                 case "fuel_state":
                     return "fuel_state";

                 case "barricade_progress":
                     return "barricade_progress";

                 case "license_plate_drag":
                 {
                     LicensePlateDragMessage message =
                         JsonConvert.DeserializeObject<LicensePlateDragMessage>(json);

                     return $"license_plate_drag:{message.IsOldPlate}";
                 }

                 default:
                     return null;
             }
         }

         private void Update()
         {
             while (eventMessages.TryDequeue(out ReceivedMessage message))
             {
                 if (message.Type == "scene_change")
                 {
                     latestReceivedStates.Clear();
                     Handle(message.Json);
                     return;
                 }

                 Handle(message.Json);
             }

             foreach (string stateKey in latestReceivedStates.Keys)
             {
                 if (latestReceivedStates.TryRemove(stateKey, out string json))
                 {
                     Handle(json);
                 }
             }
         }
         
         private void Handle(string json)
         {
             string type = JsonConvert.DeserializeObject<TypeOnly>(json).Type;

             switch (type)
             {
                 case "welcome": OnWelcome(JsonConvert.DeserializeObject<WelcomeMessage>(json)); break;
                 case "join": OnJoin(JsonConvert.DeserializeObject<JoinMessage>(json)); break;
                 case "leave": OnLeave(JsonConvert.DeserializeObject<LeaveMessage>(json)); break;
                 case "scene_change" : OnSceneChange(JsonConvert.DeserializeObject<SceneChangeMessage>(json)); break;
             }
             
             MessageReceived?.Invoke(type, json);
         }
         
         private void OnWelcome(WelcomeMessage message)
         {
             Me = message.User;
             RoomCode = message.RoomCode;
             
             participants.Clear();
             participants[Me.Id] = Me;

             foreach (User user in message.Users)
             {
                 participants[user.Id] = user;
             }
         }

         private void OnJoin(JoinMessage message)
         {
             participants[message.User.Id] = message.User;
         }

         private void OnLeave(LeaveMessage message)
         {
             participants.Remove(message.Id);
         }
         
         private void OnSceneChange(SceneChangeMessage message)
         {
             if (message == null || string.IsNullOrEmpty(message.SceneName)) return;
             if (IsConnected == false)
             {
                 Debug.LogWarning(
                     $"[방] 씬 전환 취소: 세션이 유효하지 않습니다. " +
                     $"socket={socket?.State}, me={Me?.Id ?? "null"}");
                 return;
             }
             // Build Setting에 등록되지 않은 씬의 경우
             if (Application.CanStreamedLevelBeLoaded(message.SceneName) == false) return;

             CurrentSceneVersion = message.SceneVersion;
             SceneManager.LoadScene(message.SceneName);
         }

         public UniTask SendAsync(object message)
         {
             if (socket == null || socket.State != WebSocketState.Open) return UniTask.CompletedTask;

             string json = JsonConvert.SerializeObject(message);
             string stateKey = GetStateKey(message);

             if (stateKey == null)
                 return SendRawAsync(json);

             QueueLatestState(stateKey, json);
             return UniTask.CompletedTask;
         }

         private static string GetStateKey(object message)
         {
             return message switch
             {
                 MoveMessage => "move",
                 VehicleInputMessage => "vehicle_input",
                 VehicleStateMessage => "vehicle_state",
                 TrafficStateMessage => "traffic_state",
                 RoundStateMessage => "round_state",
                 LicensePlateDragMessage drag => $"license_plate_drag:{drag.IsOldPlate}",
                 _ => null
             };
         }

         private void QueueLatestState(string stateKey, string json)
         {
             bool startLoop = false;

             lock (stateSendGate)
             {
                 latestStateMessages[stateKey] = json;

                 if (stateSendLoopRunning == false)
                 {
                     stateSendLoopRunning = true;
                     startLoop = true;
                 }
             }

             if (startLoop) SendLatestStatesAsync().Forget();
         }

         private async UniTaskVoid SendLatestStatesAsync()
         {
             while (true)
             {
                 List<string> messages;

                 lock (stateSendGate)
                 {
                     if (latestStateMessages.Count == 0)
                     {
                         stateSendLoopRunning = false;
                         return;
                     }

                     messages = new List<string>(latestStateMessages.Values);
                     latestStateMessages.Clear();
                 }

                 foreach (string message in messages)
                     await SendRawAsync(message);
             }
         }

         private async UniTask SendRawAsync(string json)
         {
             ClientWebSocket sendingSocket = socket;

             
             if (sendingSocket == null || sendingSocket.State != WebSocketState.Open) return;

             byte[] bytes = Encoding.UTF8.GetBytes(json);
             bool locked = false;

             try
             {
                 await sendLock.WaitAsync(destroyCancellationToken);
                 locked = true;

                 if (socket != sendingSocket || sendingSocket.State != WebSocketState.Open) return;

                 // 게임 종료 토큰과 송신 시간 제한을 함께 사용한다.
                 using CancellationTokenSource timeout =
                     CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
                 
                 timeout.CancelAfter(TimeSpan.FromSeconds(sendTimeoutSeconds));
                 
                 await sendingSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, timeout.Token);
             }
             catch (OperationCanceledException)
             {
                 // 게임 종료가 아닌 송신 타임아웃이면,
                 // 이 WebSocket은 정상 동작하지 않는 것으로 보고 끊는다.
                 if (!destroyCancellationToken.IsCancellationRequested)
                 {
                     Debug.LogWarning(
                         $"[방] 송신 {sendTimeoutSeconds:0.#}초 초과. 연결을 재시작합니다.");

                     sendingSocket.Abort();
                 }
             }
             catch (WebSocketException)
             {
                 sendingSocket.Abort();
             }
             catch (ObjectDisposedException)
             {
                 // 세션이 정리되는 중이다.
             }
             finally
             {
                 if (locked) sendLock.Release();
             }
         }

         public void RequestSceneChange(string sceneName)
         {
             if (Me == null) return;
             if (string.IsNullOrEmpty(sceneName)) return;

             SendAsync(new SceneChangeRequestMessage { SceneName = sceneName.Trim() }).Forget();
         }

         protected override void OnDispose()
         {
             base.OnDispose();
             
             socket?.Abort();
             socket?.Dispose();
             socket = null;
         }
    }
}



















