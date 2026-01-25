using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Utility;

namespace ProjectSS.Map
{
    /// <summary>
    /// 맵 생성기 (마을/필드/던전 허브 시스템)
    /// Map generator (Town/Field/Dungeon hub system)
    /// </summary>
    public class MapGenerator
    {
        private SeededRandom random;
        private MapGenerationConfig config;

        // 기본 설정값
        private const float NODE_SPACING_X = 2f;
        private const float NODE_SPACING_Y = 1.5f;

        public MapType CurrentMapType => config?.mapType ?? MapType.Field;

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
            // 미리 정의된 레이아웃 사용
            if (config?.usePredefinedLayout == true && config.predefinedLayout != null)
            {
                return LoadFromLayout(config.predefinedLayout);
            }

            MapType mapType = config?.mapType ?? MapType.Field;
            return GenerateMap(mapType);
        }

        /// <summary>
        /// 미리 정의된 레이아웃에서 맵 로드
        /// Load map from predefined layout
        /// </summary>
        public MapData LoadFromLayout(MapLayoutData layout)
        {
            if (layout == null)
            {
                Debug.LogError("MapGenerator: Layout is null");
                return GenerateMap(config?.mapType ?? MapType.Field);
            }

            var mapData = new MapData(layout.mapType);

            // 각 층의 노드 생성
            foreach (var floorLayout in layout.floors)
            {
                var nodes = new List<MapNode>();
                foreach (var nodeLayout in floorLayout.nodes)
                {
                    var node = new MapNode(
                        floorLayout.floorIndex,
                        nodeLayout.column,
                        nodeLayout.nodeType
                    );

                    // 위치 계산
                    Vector2 position = CalculateNodePosition(floorLayout.floorIndex, nodeLayout, floorLayout.nodes.Count);
                    node.SetPosition(position);

                    nodes.Add(node);
                }
                mapData.AddFloor(nodes);
            }

            // 노드 연결 설정
            ConnectNodesFromLayout(mapData, layout);

            // 초기 접근성 설정
            SetInitialAccessibility(mapData);

            return mapData;
        }

        /// <summary>
        /// 레이아웃 기반 노드 위치 계산
        /// Calculate node position from layout
        /// </summary>
        private Vector2 CalculateNodePosition(int floor, NodeLayoutData nodeLayout, int nodesInFloor)
        {
            float startX = -(nodesInFloor - 1) * NODE_SPACING_X / 2f;
            float x = startX + nodeLayout.column * NODE_SPACING_X + nodeLayout.positionOffset.x;
            float y = floor * NODE_SPACING_Y + nodeLayout.positionOffset.y;
            return new Vector2(x, y);
        }

        /// <summary>
        /// 레이아웃 기반 노드 연결
        /// Connect nodes from layout data
        /// </summary>
        private void ConnectNodesFromLayout(MapData mapData, MapLayoutData layout)
        {
            for (int floor = 0; floor < layout.floors.Count - 1; floor++)
            {
                var currentFloorLayout = layout.floors[floor];
                var currentFloorNodes = mapData.GetFloor(floor);

                foreach (var nodeLayout in currentFloorLayout.nodes)
                {
                    var node = mapData.GetNode(nodeLayout.GetNodeId(floor));
                    if (node == null) continue;

                    foreach (int targetColumn in nodeLayout.connectsToColumns)
                    {
                        string targetNodeId = $"node_{floor + 1}_{targetColumn}";
                        var targetNode = mapData.GetNode(targetNodeId);
                        if (targetNode != null)
                        {
                            node.AddConnection(targetNodeId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 초기 접근성 설정
        /// Set initial accessibility
        /// </summary>
        private void SetInitialAccessibility(MapData mapData)
        {
            if (mapData.FloorCount == 0) return;

            // Town 맵은 모든 노드 접근 가능
            if (mapData.MapType == MapType.Town)
            {
                foreach (var node in mapData.GetAllNodes())
                {
                    node.IsAccessible = true;
                }
            }
            else
            {
                // 다른 맵은 첫 층만 접근 가능
                foreach (var node in mapData.GetFloor(0))
                {
                    node.IsAccessible = true;
                }
            }
        }

        /// <summary>
        /// 맵 타입에 따른 맵 생성
        /// Generate map based on map type
        /// </summary>
        public MapData GenerateMap(MapType mapType)
        {
            return mapType switch
            {
                MapType.Town => GenerateTownMap(),
                MapType.Field => GenerateFieldMap(),
                MapType.Dungeon => GenerateDungeonMap(),
                _ => GenerateFieldMap()
            };
        }

        #region Town Map Generation (Fixed Layout)

        /// <summary>
        /// 마을 맵 생성 (고정 레이아웃)
        /// Generate town map (fixed layout)
        /// </summary>
        private MapData GenerateTownMap()
        {
            var mapData = new MapData(MapType.Town);

            // 고정 레이아웃: Shop, Rest, Event
            var nodes = new List<MapNode>
            {
                new MapNode(0, 0, MapNodeType.Shop),
                new MapNode(0, 1, MapNodeType.Rest),
                new MapNode(0, 2, MapNodeType.Event)
            };

            // 고정 위치 설정
            nodes[0].SetPosition(new Vector2(-2f, 0f));
            nodes[1].SetPosition(new Vector2(0f, 1f));
            nodes[2].SetPosition(new Vector2(2f, 0f));

            mapData.AddFloor(nodes);

            // 마을의 모든 노드는 접근 가능
            foreach (var node in mapData.GetAllNodes())
            {
                node.IsAccessible = true;
            }

            return mapData;
        }

        #endregion

        #region Field Map Generation (Fixed Layout)

        /// <summary>
        /// 필드 맵 생성 (고정 레이아웃)
        /// Generate field map (fixed layout)
        /// </summary>
        private MapData GenerateFieldMap()
        {
            var mapData = new MapData(MapType.Field);

            // Floor 0: 2 Combat nodes
            var floor0 = new List<MapNode>
            {
                new MapNode(0, 0, MapNodeType.Combat),
                new MapNode(0, 1, MapNodeType.Combat)
            };
            mapData.AddFloor(floor0);

            // Floor 1: Combat, Event, Combat
            var floor1 = new List<MapNode>
            {
                new MapNode(1, 0, MapNodeType.Combat),
                new MapNode(1, 1, MapNodeType.Event),
                new MapNode(1, 2, MapNodeType.Combat)
            };
            mapData.AddFloor(floor1);

            // Floor 2: Treasure, Combat
            var floor2 = new List<MapNode>
            {
                new MapNode(2, 0, MapNodeType.Treasure),
                new MapNode(2, 1, MapNodeType.Combat)
            };
            mapData.AddFloor(floor2);

            // Floor 3: Combat, Event
            var floor3 = new List<MapNode>
            {
                new MapNode(3, 0, MapNodeType.Combat),
                new MapNode(3, 1, MapNodeType.Event)
            };
            mapData.AddFloor(floor3);

            // Floor 4: Elite
            var floor4 = new List<MapNode>
            {
                new MapNode(4, 0, MapNodeType.Elite)
            };
            mapData.AddFloor(floor4);

            // Floor 5: Rest
            var floor5 = new List<MapNode>
            {
                new MapNode(5, 0, MapNodeType.Rest)
            };
            mapData.AddFloor(floor5);

            // Floor 6: Combat, Shop
            var floor6 = new List<MapNode>
            {
                new MapNode(6, 0, MapNodeType.Combat),
                new MapNode(6, 1, MapNodeType.Shop)
            };
            mapData.AddFloor(floor6);

            // Floor 7: Event (leads to dungeon)
            var floor7 = new List<MapNode>
            {
                new MapNode(7, 0, MapNodeType.Event)
            };
            mapData.AddFloor(floor7);

            // 고정 연결 설정
            ConnectFieldNodesFixed(mapData);

            // 위치 계산
            CalculatePositionsFixed(mapData);

            // 첫 층 접근 가능
            foreach (var node in mapData.GetFloor(0))
            {
                node.IsAccessible = true;
            }

            return mapData;
        }

        /// <summary>
        /// 필드 노드 고정 연결
        /// Connect field nodes (fixed)
        /// </summary>
        private void ConnectFieldNodesFixed(MapData mapData)
        {
            // Floor 0 → Floor 1
            mapData.GetNode("node_0_0")?.AddConnection("node_1_0");
            mapData.GetNode("node_0_0")?.AddConnection("node_1_1");
            mapData.GetNode("node_0_1")?.AddConnection("node_1_1");
            mapData.GetNode("node_0_1")?.AddConnection("node_1_2");

            // Floor 1 → Floor 2
            mapData.GetNode("node_1_0")?.AddConnection("node_2_0");
            mapData.GetNode("node_1_1")?.AddConnection("node_2_0");
            mapData.GetNode("node_1_1")?.AddConnection("node_2_1");
            mapData.GetNode("node_1_2")?.AddConnection("node_2_1");

            // Floor 2 → Floor 3
            mapData.GetNode("node_2_0")?.AddConnection("node_3_0");
            mapData.GetNode("node_2_0")?.AddConnection("node_3_1");
            mapData.GetNode("node_2_1")?.AddConnection("node_3_0");
            mapData.GetNode("node_2_1")?.AddConnection("node_3_1");

            // Floor 3 → Floor 4
            mapData.GetNode("node_3_0")?.AddConnection("node_4_0");
            mapData.GetNode("node_3_1")?.AddConnection("node_4_0");

            // Floor 4 → Floor 5
            mapData.GetNode("node_4_0")?.AddConnection("node_5_0");

            // Floor 5 → Floor 6
            mapData.GetNode("node_5_0")?.AddConnection("node_6_0");
            mapData.GetNode("node_5_0")?.AddConnection("node_6_1");

            // Floor 6 → Floor 7
            mapData.GetNode("node_6_0")?.AddConnection("node_7_0");
            mapData.GetNode("node_6_1")?.AddConnection("node_7_0");
        }

        #endregion

        #region Dungeon Map Generation (Fixed Layout)

        /// <summary>
        /// 던전 맵 생성 (고정 레이아웃)
        /// Generate dungeon map (fixed layout)
        /// </summary>
        private MapData GenerateDungeonMap()
        {
            var mapData = new MapData(MapType.Dungeon);

            // Floor 0: 2 Combat nodes
            var floor0 = new List<MapNode>
            {
                new MapNode(0, 0, MapNodeType.Combat),
                new MapNode(0, 1, MapNodeType.Combat)
            };
            mapData.AddFloor(floor0);

            // Floor 1: Elite
            var floor1 = new List<MapNode>
            {
                new MapNode(1, 0, MapNodeType.Elite)
            };
            mapData.AddFloor(floor1);

            // Floor 2: Combat, Elite
            var floor2 = new List<MapNode>
            {
                new MapNode(2, 0, MapNodeType.Combat),
                new MapNode(2, 1, MapNodeType.Elite)
            };
            mapData.AddFloor(floor2);

            // Floor 3: Rest
            var floor3 = new List<MapNode>
            {
                new MapNode(3, 0, MapNodeType.Rest)
            };
            mapData.AddFloor(floor3);

            // Floor 4: Boss
            var floor4 = new List<MapNode>
            {
                new MapNode(4, 0, MapNodeType.Boss)
            };
            mapData.AddFloor(floor4);

            // 고정 연결 설정
            ConnectDungeonNodesFixed(mapData);

            // 위치 계산
            CalculatePositionsFixed(mapData);

            // 첫 층 접근 가능
            foreach (var node in mapData.GetFloor(0))
            {
                node.IsAccessible = true;
            }

            return mapData;
        }

        /// <summary>
        /// 던전 노드 고정 연결
        /// Connect dungeon nodes (fixed)
        /// </summary>
        private void ConnectDungeonNodesFixed(MapData mapData)
        {
            // Floor 0 → Floor 1
            mapData.GetNode("node_0_0")?.AddConnection("node_1_0");
            mapData.GetNode("node_0_1")?.AddConnection("node_1_0");

            // Floor 1 → Floor 2
            mapData.GetNode("node_1_0")?.AddConnection("node_2_0");
            mapData.GetNode("node_1_0")?.AddConnection("node_2_1");

            // Floor 2 → Floor 3
            mapData.GetNode("node_2_0")?.AddConnection("node_3_0");
            mapData.GetNode("node_2_1")?.AddConnection("node_3_0");

            // Floor 3 → Floor 4
            mapData.GetNode("node_3_0")?.AddConnection("node_4_0");
        }

        #endregion

        #region Common Methods (Fixed Layout)

        /// <summary>
        /// 고정 위치 계산 (랜덤 오프셋 없음)
        /// Calculate fixed positions (no random offset)
        /// </summary>
        private void CalculatePositionsFixed(MapData mapData)
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
                    floorNodes[i].SetPosition(new Vector2(x, y));
                }
            }
        }

        #endregion
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

        /// <summary>
        /// 맵 타입
        /// Map type
        /// </summary>
        public MapType MapType { get; private set; }

        public int FloorCount => floors.Count;

        public MapData() : this(MapType.Field) { }

        public MapData(MapType mapType)
        {
            MapType = mapType;
        }

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
