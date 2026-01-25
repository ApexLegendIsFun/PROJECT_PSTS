using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Utility;

namespace ProjectSS.Map
{
    /// <summary>
    /// 통합 월드맵 생성기
    /// Unified world map generator
    /// </summary>
    public class WorldMapGenerator
    {
        private SeededRandom random;

        // 기본 설정값
        private const float NODE_SPACING_X = 2f;
        private const float NODE_SPACING_Y = 1.5f;

        public WorldMapGenerator(SeededRandom rng)
        {
            random = rng;
        }

        /// <summary>
        /// 통합 월드맵 생성
        /// Generate unified world map
        /// </summary>
        public WorldMapData GenerateWorldMap(WorldMapLayoutData layoutData)
        {
            if (layoutData == null)
            {
                Debug.LogError("WorldMapGenerator: Layout data is null");
                return null;
            }

            var worldMap = new WorldMapData();

            // 1. 각 지역 생성
            foreach (var regionEntry in layoutData.regions)
            {
                var regionSection = GenerateRegionSection(regionEntry);
                if (regionSection != null)
                {
                    worldMap.AddRegion(regionSection);
                }
            }

            // 2. 지역 내 노드 연결 설정
            foreach (var regionEntry in layoutData.regions)
            {
                if (regionEntry.layout != null)
                {
                    ConnectNodesWithinRegion(worldMap, regionEntry);
                }
            }

            // 3. 지역 간 연결 설정
            foreach (var connection in layoutData.crossRegionConnections)
            {
                worldMap.ConnectNodes(connection.GetFromNodeId(), connection.GetToNodeId());
            }

            // 4. 초기 접근성 설정
            worldMap.SetInitialAccessibility();

            return worldMap;
        }

        /// <summary>
        /// 기본 월드맵 생성 (레이아웃 데이터 없이)
        /// Generate default world map (without layout data)
        /// </summary>
        public WorldMapData GenerateDefaultWorldMap()
        {
            var worldMap = new WorldMapData();

            // Town 지역 생성
            var townSection = GenerateDefaultTownSection();
            worldMap.AddRegion(townSection);

            // Field 지역 생성
            var fieldSection = GenerateDefaultFieldSection();
            worldMap.AddRegion(fieldSection);

            // Dungeon 지역 생성
            var dungeonSection = GenerateDefaultDungeonSection();
            worldMap.AddRegion(dungeonSection);

            // 지역 간 연결 설정
            ConnectDefaultRegions(worldMap);

            // 초기 접근성 설정
            worldMap.SetInitialAccessibility();

            return worldMap;
        }

        /// <summary>
        /// 지역 섹션 생성
        /// Generate region section
        /// </summary>
        private RegionMapSection GenerateRegionSection(RegionLayoutEntry regionEntry)
        {
            if (regionEntry == null)
            {
                Debug.LogError("WorldMapGenerator: Region entry is null");
                return null;
            }

            var section = new RegionMapSection(
                regionEntry.regionId,
                regionEntry.mapType,
                regionEntry.displayName,
                regionEntry.worldOffset,
                regionEntry.regionColor
            );

            // 레이아웃 데이터가 있으면 그것을 사용
            if (regionEntry.layout != null)
            {
                GenerateNodesFromLayout(section, regionEntry);
            }
            else
            {
                // 레이아웃이 없으면 기본 생성
                GenerateDefaultNodes(section, regionEntry.mapType, regionEntry.regionId);
            }

            return section;
        }

        /// <summary>
        /// 레이아웃에서 노드 생성
        /// Generate nodes from layout
        /// </summary>
        private void GenerateNodesFromLayout(RegionMapSection section, RegionLayoutEntry regionEntry)
        {
            var layout = regionEntry.layout;

            foreach (var floorLayout in layout.floors)
            {
                var nodes = new List<MapNode>();

                foreach (var nodeLayout in floorLayout.nodes)
                {
                    var node = new MapNode(
                        floorLayout.floorIndex,
                        nodeLayout.column,
                        nodeLayout.nodeType,
                        regionEntry.regionId
                    );

                    // 위치 계산
                    Vector2 position = CalculateNodePosition(
                        floorLayout.floorIndex,
                        nodeLayout,
                        floorLayout.nodes.Count
                    );
                    node.SetPosition(position);

                    nodes.Add(node);
                }

                section.AddFloor(nodes);
            }
        }

        /// <summary>
        /// 지역 내 노드 연결
        /// Connect nodes within region
        /// </summary>
        private void ConnectNodesWithinRegion(WorldMapData worldMap, RegionLayoutEntry regionEntry)
        {
            var layout = regionEntry.layout;
            var regionId = regionEntry.regionId;

            for (int floor = 0; floor < layout.floors.Count - 1; floor++)
            {
                var currentFloorLayout = layout.floors[floor];

                foreach (var nodeLayout in currentFloorLayout.nodes)
                {
                    string fromNodeId = $"{regionId}_{floor}_{nodeLayout.column}";

                    foreach (int targetColumn in nodeLayout.connectsToColumns)
                    {
                        string toNodeId = $"{regionId}_{floor + 1}_{targetColumn}";
                        worldMap.ConnectNodes(fromNodeId, toNodeId);
                    }
                }
            }
        }

        /// <summary>
        /// 노드 위치 계산
        /// Calculate node position
        /// </summary>
        private Vector2 CalculateNodePosition(int floor, NodeLayoutData nodeLayout, int nodesInFloor)
        {
            float startX = -(nodesInFloor - 1) * NODE_SPACING_X / 2f;
            float x = startX + nodeLayout.column * NODE_SPACING_X + nodeLayout.positionOffset.x;
            float y = floor * NODE_SPACING_Y + nodeLayout.positionOffset.y;
            return new Vector2(x, y);
        }

        #region Default Region Generation

        /// <summary>
        /// 기본 마을 섹션 생성
        /// Generate default town section
        /// </summary>
        private RegionMapSection GenerateDefaultTownSection()
        {
            var section = new RegionMapSection(
                "town",
                MapType.Town,
                "마을 (Town)",
                new Vector2(-6f, 0f),
                new Color(0.2f, 0.7f, 0.3f, 0.2f)
            );

            GenerateDefaultNodes(section, MapType.Town, "town");
            return section;
        }

        /// <summary>
        /// 기본 필드 섹션 생성
        /// Generate default field section
        /// </summary>
        private RegionMapSection GenerateDefaultFieldSection()
        {
            var section = new RegionMapSection(
                "field",
                MapType.Field,
                "필드 (Field)",
                new Vector2(0f, 0f),
                new Color(0.3f, 0.5f, 0.8f, 0.2f)
            );

            GenerateDefaultNodes(section, MapType.Field, "field");
            return section;
        }

        /// <summary>
        /// 기본 던전 섹션 생성
        /// Generate default dungeon section
        /// </summary>
        private RegionMapSection GenerateDefaultDungeonSection()
        {
            var section = new RegionMapSection(
                "dungeon",
                MapType.Dungeon,
                "던전 (Dungeon)",
                new Vector2(10f, 0f),
                new Color(0.7f, 0.2f, 0.2f, 0.2f)
            );

            GenerateDefaultNodes(section, MapType.Dungeon, "dungeon");
            return section;
        }

        /// <summary>
        /// 기본 노드 생성
        /// Generate default nodes
        /// </summary>
        private void GenerateDefaultNodes(RegionMapSection section, MapType mapType, string regionId)
        {
            switch (mapType)
            {
                case MapType.Town:
                    GenerateTownNodes(section, regionId);
                    break;
                case MapType.Field:
                    GenerateFieldNodes(section, regionId);
                    break;
                case MapType.Dungeon:
                    GenerateDungeonNodes(section, regionId);
                    break;
            }
        }

        /// <summary>
        /// 마을 노드 생성
        /// Generate town nodes
        /// </summary>
        private void GenerateTownNodes(RegionMapSection section, string regionId)
        {
            var nodes = new List<MapNode>
            {
                CreateNode(0, 0, MapNodeType.Shop, regionId, new Vector2(-2f, 0f)),
                CreateNode(0, 1, MapNodeType.Rest, regionId, new Vector2(0f, 1f)),
                CreateNode(0, 2, MapNodeType.Event, regionId, new Vector2(2f, 0f))
            };

            // 마을 노드는 모두 Field의 첫 층으로 연결됨 (나중에 설정)
            section.AddFloor(nodes);
        }

        /// <summary>
        /// 필드 노드 생성
        /// Generate field nodes
        /// </summary>
        private void GenerateFieldNodes(RegionMapSection section, string regionId)
        {
            // Floor 0
            var floor0 = new List<MapNode>
            {
                CreateNode(0, 0, MapNodeType.Combat, regionId),
                CreateNode(0, 1, MapNodeType.Combat, regionId)
            };
            ConnectNodesInList(floor0);
            section.AddFloor(floor0);

            // Floor 1
            var floor1 = new List<MapNode>
            {
                CreateNode(1, 0, MapNodeType.Combat, regionId),
                CreateNode(1, 1, MapNodeType.Event, regionId),
                CreateNode(1, 2, MapNodeType.Combat, regionId)
            };
            section.AddFloor(floor1);

            // Floor 2
            var floor2 = new List<MapNode>
            {
                CreateNode(2, 0, MapNodeType.Treasure, regionId),
                CreateNode(2, 1, MapNodeType.Combat, regionId)
            };
            section.AddFloor(floor2);

            // Floor 3
            var floor3 = new List<MapNode>
            {
                CreateNode(3, 0, MapNodeType.Combat, regionId),
                CreateNode(3, 1, MapNodeType.Event, regionId)
            };
            section.AddFloor(floor3);

            // Floor 4
            var floor4 = new List<MapNode>
            {
                CreateNode(4, 0, MapNodeType.Elite, regionId)
            };
            section.AddFloor(floor4);

            // Floor 5
            var floor5 = new List<MapNode>
            {
                CreateNode(5, 0, MapNodeType.Rest, regionId)
            };
            section.AddFloor(floor5);

            // Floor 6
            var floor6 = new List<MapNode>
            {
                CreateNode(6, 0, MapNodeType.Combat, regionId),
                CreateNode(6, 1, MapNodeType.Shop, regionId)
            };
            section.AddFloor(floor6);

            // Floor 7
            var floor7 = new List<MapNode>
            {
                CreateNode(7, 0, MapNodeType.Event, regionId)
            };
            section.AddFloor(floor7);

            // 위치 계산
            CalculatePositionsForSection(section);
        }

        /// <summary>
        /// 던전 노드 생성
        /// Generate dungeon nodes
        /// </summary>
        private void GenerateDungeonNodes(RegionMapSection section, string regionId)
        {
            // Floor 0
            var floor0 = new List<MapNode>
            {
                CreateNode(0, 0, MapNodeType.Combat, regionId),
                CreateNode(0, 1, MapNodeType.Combat, regionId)
            };
            section.AddFloor(floor0);

            // Floor 1
            var floor1 = new List<MapNode>
            {
                CreateNode(1, 0, MapNodeType.Elite, regionId)
            };
            section.AddFloor(floor1);

            // Floor 2
            var floor2 = new List<MapNode>
            {
                CreateNode(2, 0, MapNodeType.Combat, regionId),
                CreateNode(2, 1, MapNodeType.Elite, regionId)
            };
            section.AddFloor(floor2);

            // Floor 3
            var floor3 = new List<MapNode>
            {
                CreateNode(3, 0, MapNodeType.Rest, regionId)
            };
            section.AddFloor(floor3);

            // Floor 4
            var floor4 = new List<MapNode>
            {
                CreateNode(4, 0, MapNodeType.Boss, regionId)
            };
            section.AddFloor(floor4);

            // 위치 계산
            CalculatePositionsForSection(section);
        }

        /// <summary>
        /// 노드 생성 헬퍼
        /// Create node helper
        /// </summary>
        private MapNode CreateNode(int floor, int column, MapNodeType type, string regionId, Vector2? position = null)
        {
            var node = new MapNode(floor, column, type, regionId);
            if (position.HasValue)
            {
                node.SetPosition(position.Value);
            }
            return node;
        }

        /// <summary>
        /// 섹션 내 위치 계산
        /// Calculate positions for section
        /// </summary>
        private void CalculatePositionsForSection(RegionMapSection section)
        {
            for (int floor = 0; floor < section.FloorCount; floor++)
            {
                var floorNodes = section.GetFloor(floor);
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

        /// <summary>
        /// 기본 지역 간 연결
        /// Connect default regions
        /// </summary>
        private void ConnectDefaultRegions(WorldMapData worldMap)
        {
            // Town → Field 연결 (모든 Town 노드 → Field 0층 노드들)
            var townSection = worldMap.GetRegion("town");
            var fieldSection = worldMap.GetRegion("field");
            var dungeonSection = worldMap.GetRegion("dungeon");

            if (townSection != null && fieldSection != null)
            {
                // Town의 각 노드에서 Field의 첫 층으로 연결
                foreach (var townNode in townSection.GetAllNodes())
                {
                    foreach (var fieldNode in fieldSection.GetFloor(0))
                    {
                        townNode.AddConnection(fieldNode.NodeId);
                    }
                }
            }

            // Field 내부 연결
            if (fieldSection != null)
            {
                ConnectFieldNodesDefault(worldMap, "field");
            }

            // Dungeon 내부 연결
            if (dungeonSection != null)
            {
                ConnectDungeonNodesDefault(worldMap, "dungeon");
            }

            // Field → Dungeon 연결 (Field 마지막 층 → Dungeon 첫 층)
            if (fieldSection != null && dungeonSection != null)
            {
                foreach (var fieldNode in fieldSection.GetLastFloor())
                {
                    foreach (var dungeonNode in dungeonSection.GetFloor(0))
                    {
                        fieldNode.AddConnection(dungeonNode.NodeId);
                    }
                }
            }
        }

        /// <summary>
        /// 필드 노드 기본 연결
        /// Connect field nodes (default)
        /// </summary>
        private void ConnectFieldNodesDefault(WorldMapData worldMap, string regionId)
        {
            // Floor 0 → Floor 1
            worldMap.ConnectNodes($"{regionId}_0_0", $"{regionId}_1_0");
            worldMap.ConnectNodes($"{regionId}_0_0", $"{regionId}_1_1");
            worldMap.ConnectNodes($"{regionId}_0_1", $"{regionId}_1_1");
            worldMap.ConnectNodes($"{regionId}_0_1", $"{regionId}_1_2");

            // Floor 1 → Floor 2
            worldMap.ConnectNodes($"{regionId}_1_0", $"{regionId}_2_0");
            worldMap.ConnectNodes($"{regionId}_1_1", $"{regionId}_2_0");
            worldMap.ConnectNodes($"{regionId}_1_1", $"{regionId}_2_1");
            worldMap.ConnectNodes($"{regionId}_1_2", $"{regionId}_2_1");

            // Floor 2 → Floor 3
            worldMap.ConnectNodes($"{regionId}_2_0", $"{regionId}_3_0");
            worldMap.ConnectNodes($"{regionId}_2_0", $"{regionId}_3_1");
            worldMap.ConnectNodes($"{regionId}_2_1", $"{regionId}_3_0");
            worldMap.ConnectNodes($"{regionId}_2_1", $"{regionId}_3_1");

            // Floor 3 → Floor 4
            worldMap.ConnectNodes($"{regionId}_3_0", $"{regionId}_4_0");
            worldMap.ConnectNodes($"{regionId}_3_1", $"{regionId}_4_0");

            // Floor 4 → Floor 5
            worldMap.ConnectNodes($"{regionId}_4_0", $"{regionId}_5_0");

            // Floor 5 → Floor 6
            worldMap.ConnectNodes($"{regionId}_5_0", $"{regionId}_6_0");
            worldMap.ConnectNodes($"{regionId}_5_0", $"{regionId}_6_1");

            // Floor 6 → Floor 7
            worldMap.ConnectNodes($"{regionId}_6_0", $"{regionId}_7_0");
            worldMap.ConnectNodes($"{regionId}_6_1", $"{regionId}_7_0");
        }

        /// <summary>
        /// 던전 노드 기본 연결
        /// Connect dungeon nodes (default)
        /// </summary>
        private void ConnectDungeonNodesDefault(WorldMapData worldMap, string regionId)
        {
            // Floor 0 → Floor 1
            worldMap.ConnectNodes($"{regionId}_0_0", $"{regionId}_1_0");
            worldMap.ConnectNodes($"{regionId}_0_1", $"{regionId}_1_0");

            // Floor 1 → Floor 2
            worldMap.ConnectNodes($"{regionId}_1_0", $"{regionId}_2_0");
            worldMap.ConnectNodes($"{regionId}_1_0", $"{regionId}_2_1");

            // Floor 2 → Floor 3
            worldMap.ConnectNodes($"{regionId}_2_0", $"{regionId}_3_0");
            worldMap.ConnectNodes($"{regionId}_2_1", $"{regionId}_3_0");

            // Floor 3 → Floor 4
            worldMap.ConnectNodes($"{regionId}_3_0", $"{regionId}_4_0");
        }

        /// <summary>
        /// 리스트 내 노드들 연결 (같은 층 내에서의 연결은 사용하지 않지만, 유틸리티용)
        /// Connect nodes in list (utility method)
        /// </summary>
        private void ConnectNodesInList(List<MapNode> nodes)
        {
            // 현재 사용하지 않음 - 필요시 구현
        }

        #endregion
    }
}
