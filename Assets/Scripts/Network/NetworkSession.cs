using System;
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
        [SerializeField] private string url = "ws://localhost:5000/room";
        
         private string nickName = "";
         private string roomCode = "ABCD";
         
         public User Me { get; private set; }
         public string RoomCode { get; private set; }
         public bool IsBusy { get; private set; }
         
         public bool IsConnected => 
             socket != null 
             && socket.State == WebSocketState.Open 
             && Me != null;
         
         private ClientWebSocket socket;
         private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);
         
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

             Debug.LogWarning($"[방] 수신 루프 종료. socket={socket?.State}, me={Me?.Id ?? "null"}");
             
             IsBusy = false;
             Me = null;
             participants.Clear();
         }

         private async UniTask ReceiveLoopAsync()
         {
             ClientWebSocket receivingSocket = socket;
             if (receivingSocket == null) return;
             
             byte[] buffer = new byte[4096];
             StringBuilder builder = new StringBuilder();

             while (receivingSocket.State == WebSocketState.Open)
             {
                 WebSocketReceiveResult result;

                 try
                 {
                     result = await receivingSocket
                         .ReceiveAsync(new ArraySegment<byte>(buffer), destroyCancellationToken)
                         .AsUniTask();
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

                 Handle(json);
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

             SceneManager.LoadScene(message.SceneName);
         }

         public async UniTask SendAsync(object message)
         {
             if (socket == null || socket.State != WebSocketState.Open) return;

             byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

             await sendLock.WaitAsync();
             
             try
             {
                 await socket
                     .SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, destroyCancellationToken)
                     .AsUniTask();
             }
             catch (OperationCanceledException)
             {
                 // 이 오브젝트가 사라졌다.
             }
             catch (WebSocketException)
             {
                 // 보내는 순간 끊겼다. 듣는 쪽 루프가 알아서 빠져나온다.
             }
             finally
             {
                 sendLock.Release();
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



















