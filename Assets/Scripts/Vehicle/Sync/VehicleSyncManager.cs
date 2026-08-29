using System;
using System.Collections.Generic;
using System.Linq;
using CY.Player;
using HelloServer;
using UnityEngine;

namespace  CY.Network
{
    public class VehicleSyncManager : MonoBehaviour
    {
        private NetworkSession session;

        private readonly Dictionary<string, VehicleRole> roles = new();
        public IReadOnlyDictionary<string, VehicleRole> Roles => roles;

        public event Action<IReadOnlyDictionary<string, VehicleRole>> RolesChanged;

        private void Awake()
        {
            session = NetworkSession.Instance;
        }

        private void OnEnable()
        {
            session.MessageReceived += HandleMessage;
            RefreshRoles();
        }

        private void OnDisable()
        {
            if (session != null)
            {
                session.MessageReceived -= HandleMessage;
            }
        }

        private void HandleMessage(string type, string json)
        {
            if (type != "welcome" && type != "join" && type != "leave") return;

            RefreshRoles();
        }

        private void RefreshRoles()
        {
            List<User> users = session.Participants.Values
                .OrderBy(User => GetJoinOrder(User.Id)).ToList();

            roles.Clear();

            if (users.Count != 4)
            {
                Session.ClearRole();
                RolesChanged?.Invoke(roles);
                return;
            }

            List<VehicleRole> shuffledRoles = CreateShuffledRoles(users);

            for (int i = 0; i < users.Count; i++)
            {
                roles[users[i].Id] = shuffledRoles[i];
            }

            if (roles.TryGetValue(session.Me.Id, out VehicleRole myRole))
            {
                bool isHost = myRole == VehicleRole.Handle;
                Session.Apply(isHost, myRole);
            }
            
            RolesChanged?.Invoke(roles);
        }
        
        private static int GetJoinOrder(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length < 2)
                return int.MaxValue;

            return int.TryParse(id.Substring(1), out int order)
                ? order
                : int.MaxValue;
        }
        
        private List<VehicleRole> CreateShuffledRoles(List<User> users)
        {
            List<VehicleRole> shuffledRoles = new()
            {
                VehicleRole.Handle,
                VehicleRole.Accel,
                VehicleRole.Brake,
                VehicleRole.Gear
            };

            // 모든 클라이언트가 동일하게 갖는 룸 코드 값으로 시드 생성
            uint state = CreateSeed(session.RoomCode, users);

            // 같은 방 코드 + 같은 4명 = 모든 클라이언트에서 같은 결과
            for (int i = shuffledRoles.Count - 1; i > 0; i--)
            {
                state = state * 1664525u + 1013904223u;

                int swapIndex = (int)(state % (uint)(i + 1));

                (shuffledRoles[i], shuffledRoles[swapIndex]) =
                    (shuffledRoles[swapIndex], shuffledRoles[i]);
            }

            return shuffledRoles;
        }
        
        #region 랜덤 시드 생성

        private static uint CreateSeed(string roomCode, List<User> users)
        {
            uint hash = 2166136261u;

            AddToHash(ref hash, roomCode);

            // users는 이미 접속 순서(ID 숫자순)로 정렬된 상태
            foreach (User user in users)
                AddToHash(ref hash, user.Id);

            return hash;
        }

        private static void AddToHash(ref uint hash, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            foreach (char character in value)
            {
                hash ^= character;
                hash *= 16777619u;
            }
        }

        #endregion
    }
}













