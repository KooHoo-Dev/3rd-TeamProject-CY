using System.Collections.Generic;
using UnityEngine;

namespace CY.Stages
{
    public class RoadGraphBuilder : MonoBehaviour
    {
        [Tooltip("Level 1 안의 Level_0 오브젝트. 여기서부터의 오프셋으로 좌표를 잡는다.")]
        [SerializeField] private Transform levelOrigin;

        // Level_0__Road_Area-int.png 의 도로 중심선에서 뽑아냈다.
        // 갈림길·코너·막다른 길 99곳, 도로 연결 119개.
        // 모든 연결이 실제 도로 위 직선임을 한 칸씩 검증했다.

        // Level_0 로부터의 오프셋 (타일 1칸 = 월드 1)
        private static readonly Vector2[] NodeOffsets =
        {
            new(  10.5f,  57.5f),   // 0
            new(  10.5f,  39.5f),   // 1
            new(  10.5f,  36.5f),   // 2
            new(  10.5f,  28.5f),   // 3
            new(  10.5f,  21.5f),   // 4
            new(  10.5f,  12.5f),   // 5
            new(  15.5f,  51.5f),   // 6
            new(  15.5f,  38.5f),   // 7
            new(  15.5f,  21.5f),   // 8
            new(  20.5f,  21.5f),   // 9
            new(  23.5f,  57.5f),   // 10
            new(  23.5f,  47.5f),   // 11
            new(  23.5f,  38.5f),   // 12
            new(  23.5f,  28.5f),   // 13
            new(  23.5f,  21.5f),   // 14
            new(  23.5f,  12.5f),   // 15
            new(  28.5f,  38.5f),   // 16
            new(  28.5f,  28.5f),   // 17
            new(  29.5f,  24.5f),   // 18
            new(  29.5f,  21.5f),   // 19
            new(  29.5f,  17.5f),   // 20
            new(  36.5f,  57.5f),   // 21
            new(  36.5f,  47.5f),   // 22
            new(  36.5f,  43.5f),   // 23
            new(  36.5f,  28.5f),   // 24
            new(  36.5f,  21.5f),   // 25
            new(  36.5f,  12.5f),   // 26
            new(  40.5f,  35.5f),   // 27
            new(  42.5f,  36.5f),   // 28
            new(  42.5f,  35.5f),   // 29
            new(  42.5f,  32.5f),   // 30
            new(  42.5f,  28.5f),   // 31
            new(  43.5f,  32.5f),   // 32
            new(  48.5f,  49.5f),   // 33
            new(  48.5f,  43.5f),   // 34
            new(  49.5f,  39.5f),   // 35
            new(  49.5f,  36.5f),   // 36
            new(  49.5f,  35.5f),   // 37
            new(  51.5f,  36.5f),   // 38
            new(  51.5f,  16.5f),   // 39
            new(  51.5f,  12.5f),   // 40
            new(  53.5f,  39.5f),   // 41
            new(  56.5f,  36.5f),   // 42
            new(  57.5f,  57.5f),   // 43
            new(  57.5f,  54.5f),   // 44
            new(  57.5f,  49.5f),   // 45
            new(  58.5f,  54.5f),   // 46
            new(  59.5f,  39.5f),   // 47
            new(  59.5f,  36.5f),   // 48
            new(  59.5f,  28.5f),   // 49
            new(  63.5f,  36.5f),   // 50
            new(  67.5f,  57.5f),   // 51
            new(  67.5f,  43.5f),   // 52
            new(  67.5f,  28.5f),   // 53
            new(  67.5f,  12.5f),   // 54
            new(  73.5f,  31.5f),   // 55
            new(  73.5f,  27.5f),   // 56
            new(  73.5f,  24.5f),   // 57
            new(  73.5f,  23.5f),   // 58
            new(  74.5f,  43.5f),   // 59
            new(  74.5f,  35.5f),   // 60
            new(  77.5f,  57.5f),   // 61
            new(  77.5f,  47.5f),   // 62
            new(  78.5f,  48.5f),   // 63
            new(  78.5f,  47.5f),   // 64
            new(  78.5f,  43.5f),   // 65
            new(  78.5f,  39.5f),   // 66
            new(  78.5f,  35.5f),   // 67
            new(  80.5f,  47.5f),   // 68
            new(  81.5f,  27.5f),   // 69
            new(  82.5f,  39.5f),   // 70
            new(  82.5f,  31.5f),   // 71
            new(  83.5f,  27.5f),   // 72
            new(  83.5f,  23.5f),   // 73
            new(  86.5f,  23.5f),   // 74
            new(  86.5f,  12.5f),   // 75
            new(  90.5f,  57.5f),   // 76
            new(  90.5f,  50.5f),   // 77
            new(  90.5f,  43.5f),   // 78
            new(  90.5f,  30.5f),   // 79
            new(  92.5f,  27.5f),   // 80
            new(  92.5f,  23.5f),   // 81
            new( 100.5f,  43.5f),   // 82
            new( 100.5f,  31.5f),   // 83
            new( 100.5f,  30.5f),   // 84
            new( 100.5f,  23.5f),   // 85
            new( 100.5f,  12.5f),   // 86
            new( 102.5f,  43.5f),   // 87
            new( 103.5f,  50.5f),   // 88
            new( 103.5f,  43.5f),   // 89
            new( 106.5f,  37.5f),   // 90
            new( 106.5f,  31.5f),   // 91
            new( 110.5f,  37.5f),   // 92
            new( 110.5f,  36.5f),   // 93
            new( 112.5f,  57.5f),   // 94
            new( 112.5f,  43.5f),   // 95
            new( 112.5f,  37.5f),   // 96
            new( 112.5f,  12.5f),   // 97
            new( 117.5f,  57.5f),   // 98
        };

        // 도로로 이어진 지점 쌍
        private static readonly int[] EdgePairs =
        {
              0,  1,   0, 10,   2,  3,   3,  4,   3, 13,   4,  5,
              4,  8,   5, 15,   6,  7,   7, 12,   9, 14,  10, 11,
             10, 21,  11, 12,  11, 22,  12, 16,  13, 14,  13, 17,
             14, 15,  14, 19,  15, 26,  16, 17,  17, 24,  18, 19,
             19, 20,  19, 25,  21, 22,  21, 43,  22, 23,  23, 24,
             23, 34,  24, 25,  24, 31,  25, 26,  26, 40,  27, 29,
             28, 29,  28, 36,  29, 30,  29, 37,  30, 31,  30, 32,
             31, 49,  33, 34,  33, 45,  34, 52,  35, 36,  35, 41,
             36, 37,  36, 38,  39, 40,  40, 54,  42, 48,  43, 44,
             43, 51,  44, 45,  44, 46,  47, 48,  48, 49,  48, 50,
             49, 53,  51, 52,  51, 61,  52, 53,  52, 59,  53, 54,
             54, 75,  55, 56,  55, 71,  56, 57,  56, 58,  56, 69,
             57, 58,  58, 73,  59, 60,  59, 65,  60, 67,  61, 62,
             61, 76,  62, 64,  63, 64,  64, 65,  64, 68,  66, 67,
             66, 70,  70, 71,  72, 73,  72, 80,  74, 75,  74, 81,
             75, 86,  76, 77,  76, 94,  77, 78,  77, 88,  78, 79,
             78, 82,  79, 84,  80, 81,  81, 85,  82, 83,  82, 87,
             82, 89,  83, 84,  83, 91,  84, 85,  85, 86,  86, 97,
             87, 89,  88, 89,  89, 95,  90, 91,  90, 92,  92, 93,
             92, 96,  94, 95,  94, 98,  95, 96,  96, 97,
        };

        [ContextMenu("교차점 자동 생성")]
        private void Generate()
        {
            if (levelOrigin == null)
            {
                Debug.LogError($"{name}: Level Origin 을 지정하세요 (Level 1 안의 Level_0)", this);
                return;
            }

            ClearChildren();

            Vector3 origin = levelOrigin.position;
            List<RoadNode> created = new();

            for (int i = 0; i < NodeOffsets.Length; i++)
            {
                GameObject go = new GameObject($"Node_{i:00}");
                go.transform.SetParent(transform, false);
                go.transform.position = origin + new Vector3(NodeOffsets[i].x, NodeOffsets[i].y, 0f);

                created.Add(go.AddComponent<RoadNode>());
            }

            // 한쪽 방향만 넣는다. 반대 방향은 RoadGraph 가 채운다.
            List<List<RoadNode>> links = new();
            for (int i = 0; i < created.Count; i++) links.Add(new List<RoadNode>());

            for (int i = 0; i + 1 < EdgePairs.Length; i += 2)
            {
                int a = EdgePairs[i];
                int b = EdgePairs[i + 1];

                if (a < 0 || a >= created.Count) continue;
                if (b < 0 || b >= created.Count) continue;

                links[a].Add(created[b]);
            }

#if UNITY_EDITOR
            for (int i = 0; i < created.Count; i++)
                created[i].EditorSetNeighbors(links[i]);
#endif

            RoadGraph graph = GetComponent<RoadGraph>();
            if (graph != null) graph.CollectNodes();

            Debug.Log($"[RoadGraphBuilder] 교차점 {created.Count}개, 연결 {EdgePairs.Length / 2}개 생성", this);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(gameObject);
#endif
        }

        [ContextMenu("교차점 전부 지우기")]
        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
