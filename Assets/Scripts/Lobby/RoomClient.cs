using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using CY.Network;

namespace HelloServer
{
    public class RoomClient : MonoBehaviour
    {
        [Header("장면")]
        [SerializeField] private LocalPlayer localPlayer;
        [SerializeField] private RemotePlayer remotePrefab;

        [Header("보내기")]
        [SerializeField] private float sendPerSecond = 20f;
        
        [Header("캐릭터 에셋")]
        [SerializeField] private Sprite[] characterSprites;
            
        [Header("로비 고정 배치")]
        [SerializeField] private Transform[] lobbySlots;

        private readonly Dictionary<string, int> slotByUserId = new Dictionary<string, int>();
        
        private NetworkSession session;
        
        private readonly Dictionary<string, RemotePlayer> remotes = new Dictionary<string, RemotePlayer>();
        private readonly List<string> chatLog = new List<string>();

        private const int ChatKeep = 12;

        public User Me => session?.Me;
        public string RoomCode => session?.RoomCode;
        public bool IsBusy => session != null && session.IsBusy;
        
        
        public int RemoteCount => remotes.Count;
        public LocalPlayer Local => localPlayer;
        public IReadOnlyList<string> ChatLog => chatLog;

        private void Awake()
        {
            session = NetworkSession.Instance;
        }

        private void OnEnable()
        {
            if (session == null) session = NetworkSession.Instance;
            session.MessageReceived += Handle;
        }

        private void Start()
        {
            SendLoop().Forget();
        }

        private void OnDisable()
        {
            if (session != null) session.MessageReceived -= Handle;
        }

        public void Join(string code, string who)
        {
            session.Join(code, who);
        }
        
        private void Handle(string type, string json)
        {

            switch (type)
            {
                case "welcome": OnWelcome(JsonConvert.DeserializeObject<WelcomeMessage>(json)); break;
                case "join": OnJoin(JsonConvert.DeserializeObject<JoinMessage>(json)); break;
                case "leave": OnLeave(JsonConvert.DeserializeObject<LeaveMessage>(json)); break;
                case "chat": OnChat(JsonConvert.DeserializeObject<ChatMessage>(json)); break;
            }
        }

        private void OnWelcome(WelcomeMessage message)
        {
            localPlayer.Setup(Me);
            localPlayer.CanMove = false;

            foreach (User user in message.Users)
            {
                Spawn(user);
            }

            AssignInitialSlots();

            Debug.Log($"[방] {RoomCode} 에 들어왔다. " + $"내 번호는 {Me.Id}, 이미 {message.Users.Length}명 있다");
        }

        private void OnJoin(JoinMessage message)
        {
            Spawn(message.User);
            AssignNextSlot(message.User);
            
            Debug.Log($"[방] {message.User.NickName}({message.User.Id}) 이(가) 들어왔다");
        }

        private void OnLeave(LeaveMessage message)
        {
            if (remotes.TryGetValue(message.Id, out RemotePlayer remote) == false) return;

            remotes.Remove(message.Id);
            slotByUserId.Remove(message.Id);
            Destroy(remote.gameObject);

            Debug.Log($"[방] {message.Id} 이(가) 나갔다");
        }
        
        // 방에서 누가 말했다. 내 말도 여기로 돌아옴
        private void OnChat(ChatMessage message)
        {
            string mine = Me != null && message.Id == Me.Id ? " (나)" : "";

            chatLog.Add($"{message.NickName}{mine} : {message.Text}");

            if (chatLog.Count > ChatKeep) chatLog.RemoveAt(0);

            Debug.Log($"[방] {message.NickName}({message.Id}) : {message.Text}");
        }
        
        private void Spawn(User user)
        {
            // 이미 있으면 만들지 않는다. welcome 과 join 이 겹칠 수 있다.
            if (remotes.ContainsKey(user.Id)) return;

            RemotePlayer remote = Instantiate(remotePrefab, Vector3.zero, Quaternion.identity);
            remote.Setup(user);
            remotes[user.Id] = remote;
        }

        private async UniTaskVoid SendLoop()
        {
            int delay = (int)(1000f / sendPerSecond);
            var token = destroyCancellationToken;

            while (token.IsCancellationRequested == false)
            {
                await UniTask.Delay(delay, cancellationToken: destroyCancellationToken);

                // 아직 번호를 못 받았으면 보낼 것이 없다.
                if (Me == null || token.IsCancellationRequested) continue;

                Vector2 position = localPlayer.transform.position;

                await session.SendAsync(new MoveMessage { Id = Me.Id, X = position.x, Y = position.y });
            }
        }

        public void Say(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            session.SendAsync(new ChatMessage { Text = text.Trim() }).Forget();
        }

        public void RequestSceneChange(string sceneName)
        {
            if (Me == null) return;
            if (string.IsNullOrEmpty(sceneName)) return;

            session.SendAsync(new SceneChangeRequestMessage { SceneName = sceneName.Trim() }).Forget();
        }
        

        private static Color ColorOf(string id)
        {
            int hash = 0;

            foreach (char c in id) hash = hash * 31 + c;

            return Color.HSVToRGB(Mathf.Abs(hash % 360) / 360f, 0.65f, 0.95f);
        }
        
        private void AssignInitialSlots()
        {
            List<User> users = new List<User>(session.Participants.Values);

            users.Sort((left, right) =>
                GetJoinOrder(left.Id).CompareTo(GetJoinOrder(right.Id)));

            for (int i = 0; i < users.Count; i++)
            {
                slotByUserId[users[i].Id] = i;
                ApplySlot(users[i]);
            }
        }

        private void AssignNextSlot(User user)
        {
            int slotIndex = 0;

            while (slotByUserId.ContainsValue(slotIndex))
            {
                slotIndex++;
            }

            slotByUserId[user.Id] = slotIndex;
            ApplySlot(user);
        }

        private void ApplySlot(User user)
        {
            int slotIndex = slotByUserId[user.Id];
            Sprite sprite = characterSprites[slotIndex];
            Vector3 position = lobbySlots[slotIndex].position;

            if (user.Id == Me.Id)
            {
                localPlayer.SetSprite(sprite);
                localPlayer.transform.position = position;
                return;
            }

            RemotePlayer remote = remotes[user.Id];
            remote.SetSprite(sprite);
            remote.SetPosition(position);
        }

        private static int GetJoinOrder(string userId)
        {
            return int.Parse(userId.Substring(1));
        }
        

    }
}
