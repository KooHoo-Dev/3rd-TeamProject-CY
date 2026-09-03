using System.Collections.Generic;
using UnityEngine;

namespace CY.Stages
{
    [DefaultExecutionOrder(-50)]
    public class RoadGraph : MonoBehaviour
    {
        [SerializeField] private RoadNode[] nodes;

        private readonly Dictionary<RoadNode, List<RoadNode>> adjacency = new();

        private void Awake() => Build();

        [ContextMenu("자식 노드 다시 모으기")]
        public void CollectNodes()
        {
            nodes = GetComponentsInChildren<RoadNode>(true);
            Debug.Log($"[RoadGraph] 노드 {nodes.Length}개를 찾았습니다.", this);
        }

#if UNITY_EDITOR
        [ContextMenu("빈 이웃 칸 정리")]
        private void CleanupNeighbors()
        {
            int removed = 0;

            foreach (RoadNode node in GetComponentsInChildren<RoadNode>(true))
                removed += node.EditorRemoveMissingNeighbors();

            CollectNodes();
            Debug.Log($"[RoadGraph] 빈 이웃 칸 {removed}개를 정리했습니다.", this);
        }

        [ContextMenu("연결 상태 검사")]
        private void Inspect()
        {
            Build();

            int edges = 0;
            List<string> lonely = new();

            foreach (KeyValuePair<RoadNode, List<RoadNode>> pair in adjacency)
            {
                edges += pair.Value.Count;
                if (pair.Value.Count == 0) lonely.Add(pair.Key.name);
            }

            Debug.Log($"[RoadGraph] 노드 {adjacency.Count}개, 연결 {edges / 2}개. " +
                      $"이웃이 없는 노드: {(lonely.Count == 0 ? "없음" : string.Join(", ", lonely))}", this);
        }
#endif

        private void Build()
        {
            if (nodes == null || nodes.Length == 0)
                nodes = GetComponentsInChildren<RoadNode>(true);

            adjacency.Clear();

            foreach (RoadNode node in nodes)
            {
                if (node == null) continue;
                adjacency[node] = new List<RoadNode>();
            }

            // 한쪽만 연결해도 양방향이 되도록 대칭으로 만든다
            foreach (RoadNode node in nodes)
            {
                if (node == null) continue;

                foreach (RoadNode neighbor in node.Neighbors)
                {
                    if (neighbor == null || neighbor == node) continue;
                    if (adjacency.ContainsKey(neighbor) == false) continue;

                    if (adjacency[node].Contains(neighbor) == false)
                        adjacency[node].Add(neighbor);

                    if (adjacency[neighbor].Contains(node) == false)
                        adjacency[neighbor].Add(node);
                }
            }
        }

        /// <summary>maxDistance 안에서 가장 가까운 노드. 없으면 null.</summary>
        public RoadNode FindNearest(Vector2 worldPoint, float maxDistance)
        {
            if (nodes == null) return null;

            RoadNode best = null;
            float bestSqr = maxDistance * maxDistance;

            foreach (RoadNode node in nodes)
            {
                if (node == null) continue;

                float sqr = (node.Position - worldPoint).sqrMagnitude;
                if (sqr > bestSqr) continue;

                bestSqr = sqr;
                best = node;
            }

            return best;
        }

        /// <summary>거리가 가장 짧은 노드 순서. 이어져 있지 않으면 null.</summary>
        public List<RoadNode> FindPath(RoadNode from, RoadNode to)
        {
            if (from == null || to == null) return null;
            if (from == to) return new List<RoadNode> { from };
            if (adjacency.ContainsKey(from) == false) return null;
            if (adjacency.ContainsKey(to) == false) return null;

            Dictionary<RoadNode, float> cost = new();
            Dictionary<RoadNode, RoadNode> cameFrom = new();
            HashSet<RoadNode> visited = new();

            foreach (RoadNode node in adjacency.Keys) cost[node] = float.MaxValue;

            cost[from] = 0f;
            cameFrom[from] = null;

            while (true)
            {
                RoadNode current = null;
                float currentCost = float.MaxValue;

                foreach (KeyValuePair<RoadNode, float> pair in cost)
                {
                    if (visited.Contains(pair.Key)) continue;
                    if (pair.Value >= currentCost) continue;

                    currentCost = pair.Value;
                    current = pair.Key;
                }

                if (current == null) return null;
                if (current == to) return Retrace(cameFrom, to);

                visited.Add(current);

                foreach (RoadNode next in adjacency[current])
                {
                    if (visited.Contains(next)) continue;

                    float candidate = currentCost + Vector2.Distance(current.Position, next.Position);
                    if (candidate >= cost[next]) continue;

                    cost[next] = candidate;
                    cameFrom[next] = current;
                }
            }
        }

        private static List<RoadNode> Retrace(Dictionary<RoadNode, RoadNode> cameFrom, RoadNode to)
        {
            List<RoadNode> path = new();

            for (RoadNode node = to; node != null; node = cameFrom[node])
                path.Add(node);

            path.Reverse();
            return path;
        }
    }
}
