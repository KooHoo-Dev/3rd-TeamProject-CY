using System.Collections.Generic;
using CY.Network;
using Cysharp.Threading.Tasks;
using HelloServer;
using Newtonsoft.Json;
using UnityEngine;

namespace CY.Stages
{
    public class TrafficStateSync : MonoBehaviour
    {
        [SerializeField] private float sendPerSecond = 10f;

        private readonly Dictionary<string, TrafficAgent> agentsById = new();

        private TrafficAgent[] trafficAgents;
        private NetworkSession session;
        private int expectedSceneVersion;
        private float nextSendAt;

        private void Awake()
        {
            session = NetworkSession.Instance;

            trafficAgents =
                FindObjectsByType<TrafficAgent>(FindObjectsSortMode.None);

            foreach (TrafficAgent agent in trafficAgents)
                agentsById.Add(agent.NetworkId, agent);
        }

        private void OnEnable()
        {
            if (session == null)
                session = NetworkSession.Instance;

            if (session == null)
                return;

            expectedSceneVersion = session.CurrentSceneVersion;
            session.MessageReceived += HandleMessage;
        }

        private void OnDisable()
        {
            if (session != null)
                session.MessageReceived -= HandleMessage;
        }

        private void FixedUpdate()
        {
            if (Session.IsHost == false || session == null)
                return;

            if (Time.unscaledTime < nextSendAt)
                return;

            nextSendAt = Time.unscaledTime + 1f / Mathf.Max(1f, sendPerSecond);

            TrafficAgentState[] states =
                new TrafficAgentState[trafficAgents.Length];

            for (int i = 0; i < trafficAgents.Length; i++)
            {
                TrafficAgent agent = trafficAgents[i];

                states[i] = new TrafficAgentState
                {
                    Id = agent.NetworkId,
                    PositionX = agent.transform.position.x,
                    PositionY = agent.transform.position.y,
                    Rotation = agent.transform.eulerAngles.z
                };
            }

            session.SendAsync(new TrafficStateMessage
            {
                SceneVersion = session.CurrentSceneVersion,
                States = states
            }).Forget();
        }

        private void HandleMessage(string type, string json)
        {
            if (type != "traffic_state" || Session.IsHost)
                return;

            TrafficStateMessage message =
                JsonConvert.DeserializeObject<TrafficStateMessage>(json);

            if (message == null || message.SceneVersion != expectedSceneVersion)
                return;

            foreach (TrafficAgentState state in message.States)
            {
                if (agentsById.TryGetValue(state.Id, out TrafficAgent agent))
                {
                    agent.ApplyNetworkState(
                        new Vector2(state.PositionX, state.PositionY),
                        state.Rotation);
                }
            }
        }
    }
}