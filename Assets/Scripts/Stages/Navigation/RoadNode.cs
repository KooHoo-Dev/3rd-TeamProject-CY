using System.Collections.Generic;
using UnityEngine;

namespace CY.Stages
{
    public class RoadNode : MonoBehaviour
    {
        [Tooltip("이어진 교차점들. 한쪽에만 넣어도 RoadGraph 가 양방향으로 만든다.")]
        [SerializeField] private List<RoadNode> neighbors = new();

        public IReadOnlyList<RoadNode> Neighbors => neighbors;
        public Vector2 Position => transform.position;

#if UNITY_EDITOR
        public void EditorSetNeighbors(List<RoadNode> value) => neighbors = value;

        public int EditorRemoveMissingNeighbors() => neighbors.RemoveAll(n => n == null);

        public void EditorConnect(RoadNode other)
        {
            if (other == null || other == this) return;
            if (neighbors.Contains(other)) return;

            neighbors.Add(other);
        }
#endif

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.9f);
            Gizmos.DrawSphere(transform.position, 0.4f);

            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.4f);

            foreach (RoadNode neighbor in neighbors)
            {
                if (neighbor == null) continue;
                Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }
}
