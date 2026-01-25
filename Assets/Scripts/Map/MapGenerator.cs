using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Data;
using ProjectSS.Utility;

namespace ProjectSS.Map
{
    /// <summary>
    /// 맵 생성기 (슬더슬 스타일 분기 맵)
    /// Map generator (Slay the Spire style branching map)
    /// </summary>
    public class MapGenerator
    {
        private SeededRandom random;
        private MapGenerationConfig config;

        // 기본 설정값
        private const int DEFAULT_FLOORS = 15;
        private const int MIN_NODES_PER_FLOOR = 2;
        private const int MAX_NODES_PER_FLOOR = 4;
        private const float NODE_SPACING_X = 2f;
        private const float NODE_SPACING_Y = 1.5f;

        public MapGenerator(SeededRandom rng, MapGenerationConfig config = null)
        {
            random = rng;
            this.config = config;
        }

        /// <summary>
        /// 맵 생성
        /// Generate map
        /// </summary>
        public MapData GenerateMap()
        {
            int floors = config != null ? config.numberOfFloors : DEFAULT_FLOORS;
            var mapData = new MapData();

            // 각 층별 노드 생성
            for (int floor = 0; floor < floors; floor++)
            {
                var floorNodes = CreateFloorNodes(floor, floors);
                mapData.AddFloor(floorNodes);
            }

            // 노드 연결
            ConnectNodes(mapData);

            // 노드 타입 할당
            AssignNodeTypes(mapData, floors);

            // 위치 계산
            CalculatePositions(mapData);

            // 첫 번째 층 노드들 접근 가능하게 설정
            foreach (var node in mapData.GetFloor(0))
            {
                node.IsAccessible = true;
            }

            return mapData;
        }

        private List<MapNode> CreateFloorNodes(int floor, int totalFloors)
        {
            var nodes = new List<MapNode>();

            int minNodes = config != null ? config.minNodesPerFloor : MIN_NODES_PER_FLOOR;
            int maxNodes = config != null ? config.maxNodesPerFloor : MAX_NODES_PER_FLOOR;

            // 첫 층과 보스 층은 특수 처리
            int nodeCount;
            if (floor == 0)
            {
                nodeCount = 3; // 시작 층은 3개
            }
            else if (floor == totalFloors - 1)
            {
                nodeCount = 1; // 보스 층은 1개
            }
            else
            {
                nodeCount = random.Next(minNodes, maxNodes + 1);
            }

            for (int col = 0; col < nodeCount; col++)
            {
                var node = new MapNode(floor, col, MapNodeType.Combat);
                nodes.Add(node);
            }

            return nodes;
        }

        private void ConnectNodes(MapData mapData)
        {
            int floorCount = mapData.FloorCount;

            for (int floor = 0; floor < floorCount - 1; floor++)
            {
                var currentFloor = mapData.GetFloor(floor);
                var nextFloor = mapData.GetFloor(floor + 1);

                // 각 노드에서 최소 1개 연결 보장
                foreach (var node in currentFloor)
                {
                    // 1-2개의 연결 생성
                    int connections = random.Next(1, 3);

                    // 가까운 노드에 우선 연결
                    var sortedNext = new List<MapNode>(nextFloor);
                    sortedNext.Sort((a, b) =>
                        Mathf.Abs(a.Column - node.Column).CompareTo(Mathf.Abs(b.Column - node.Column)));

                    for (int i = 0; i < Mathf.Min(connections, sortedNext.Count); i++)
                    {
                        node.AddConnection(sortedNext[i].NodeId);
                    }
                }

                // 다음 층의 모든 노드가 연결되어 있는지 확인
                foreach (var nextNode in nextFloor)
                {
                    bool hasIncoming = false;
                    foreach (var currentNode in currentFloor)
                    {
                        if (currentNode.ConnectedNodeIds.Contains(nextNode.NodeId))
                        {
                            hasIncoming = true;
                            break;
                        }
                    }

                    // 연결이 없으면 가장 가까운 노드에서 연결
                    if (!hasIncoming)
                    {
                        var closest = currentFloor[0];
                        int minDist = Mathf.Abs(closest.Column - nextNode.Column);

                        foreach (var node in currentFloor)
                        {
                            int dist = Mathf.Abs(node.Column - nextNode.Column);
                            if (dist < minDist)
                            {
                                minDist = dist;
                                closest = node;
                            }
                        }

                        closest.AddConnection(nextNode.NodeId);
                    }
                }
            }
        }

        private void AssignNodeTypes(MapData mapData, int totalFloors)
        {
            for (int floor = 0; floor < totalFloors; floor++)
            {
                var floorNodes = mapData.GetFloor(floor);

                foreach (var node in floorNodes)
                {
                    MapNodeType type = DetermineNodeType(floor, totalFloors);

                    // 리플렉션 대신 새 노드 생성으로 타입 변경
                    // MapNode는 immutable하지 않으므로 직접 필드 접근 필요
                    // 여기서는 간단히 SetNodeType 메서드 추가 필요
                    SetNodeType(node, type);
                }
            }
        }

        private void SetNodeType(MapNode node, MapNodeType type)
        {
            // MapNode에 SetType 메서드가 필요하지만, 현재는 리플렉션 사용
            var field = typeof(MapNode).GetProperty("NodeType");
            if (field != null)
            {
                // backing field 직접 수정
                var backingField = typeof(MapNode).GetField("<NodeType>k__BackingField",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                backingField?.SetValue(node, type);
            }
        }

        private MapNodeType DetermineNodeType(int floor, int totalFloors)
        {
            // 보스 층
            if (floor == totalFloors - 1)
                return MapNodeType.Boss;

            // 첫 층은 항상 일반 전투
            if (floor == 0)
                return MapNodeType.Combat;

            // 휴식 보장 층 (8층 근처)
            int restFloor = config != null ? config.restGuaranteeFloor : 8;
            if (floor == restFloor && random.Chance(0.7f))
                return MapNodeType.Rest;

            // 엘리트 최소 층
            int minEliteFloor = config != null ? config.minFloorsBeforeElite : 5;

            // 가중치 기반 랜덤 선택
            float combatWeight = config != null ? config.combatNodeWeight : 0.4f;
            float eventWeight = config != null ? config.eventNodeWeight : 0.2f;
            float eliteWeight = floor >= minEliteFloor ? (config != null ? config.eliteNodeWeight : 0.1f) : 0f;
            float restWeight = floor >= 3 ? (config != null ? config.restNodeWeight : 0.1f) : 0f;
            float shopWeight = floor >= 2 ? (config != null ? config.shopNodeWeight : 0.1f) : 0f;
            float treasureWeight = config != null ? config.treasureNodeWeight : 0.1f;

            float totalWeight = combatWeight + eventWeight + eliteWeight + restWeight + shopWeight + treasureWeight;
            float roll = random.NextFloat() * totalWeight;

            float cumulative = 0;
            cumulative += combatWeight;
            if (roll < cumulative) return MapNodeType.Combat;

            cumulative += eventWeight;
            if (roll < cumulative) return MapNodeType.Event;

            cumulative += eliteWeight;
            if (roll < cumulative) return MapNodeType.Elite;

            cumulative += restWeight;
            if (roll < cumulative) return MapNodeType.Rest;

            cumulative += shopWeight;
            if (roll < cumulative) return MapNodeType.Shop;

            return MapNodeType.Treasure;
        }

        private void CalculatePositions(MapData mapData)
        {
            int floorCount = mapData.FloorCount;

            for (int floor = 0; floor < floorCount; floor++)
            {
                var floorNodes = mapData.GetFloor(floor);
                int nodeCount = floorNodes.Count;

                float startX = -(nodeCount - 1) * NODE_SPACING_X / 2f;
                float y = floor * NODE_SPACING_Y;

                for (int i = 0; i < nodeCount; i++)
                {
                    float x = startX + i * NODE_SPACING_X;
                    // 약간의 랜덤 오프셋 추가
                    x += (random.NextFloat() - 0.5f) * 0.3f;
                    floorNodes[i].SetPosition(new Vector2(x, y));
                }
            }
        }
    }

    /// <summary>
    /// 맵 데이터 (전체 맵 정보)
    /// Map data (entire map information)
    /// </summary>
    [System.Serializable]
    public class MapData
    {
        private List<List<MapNode>> floors = new List<List<MapNode>>();
        private Dictionary<string, MapNode> nodeDict = new Dictionary<string, MapNode>();

        public int FloorCount => floors.Count;

        public void AddFloor(List<MapNode> floorNodes)
        {
            floors.Add(floorNodes);
            foreach (var node in floorNodes)
            {
                nodeDict[node.NodeId] = node;
            }
        }

        public List<MapNode> GetFloor(int floor)
        {
            if (floor >= 0 && floor < floors.Count)
                return floors[floor];
            return new List<MapNode>();
        }

        public MapNode GetNode(string nodeId)
        {
            return nodeDict.TryGetValue(nodeId, out var node) ? node : null;
        }

        public List<MapNode> GetAllNodes()
        {
            var all = new List<MapNode>();
            foreach (var floor in floors)
            {
                all.AddRange(floor);
            }
            return all;
        }
    }
}
