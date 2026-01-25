#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 통합 월드맵 레이아웃 생성기
    /// Unified world map layout generator
    /// </summary>
    public static class WorldMapLayoutGenerator
    {
        private const string LAYOUT_PATH = "Assets/_Project/Data/Map/Layouts/";
        private const string WORLD_MAP_PATH = "Assets/_Project/Data/Map/";

        #region Menu Items

        [MenuItem("Game/Generators/World Map/Generate Default World Map Layout", false, 100)]
        public static void GenerateDefaultWorldMapLayout()
        {
            EnsureDirectoriesExist();

            // 1. 개별 지역 레이아웃 생성
            var townLayout = CreateTownLayout();
            var fieldLayout = CreateFieldLayout();
            var dungeonLayout = CreateDungeonLayout();

            // 2. 통합 월드맵 레이아웃 생성
            var worldMapLayout = CreateWorldMapLayout(townLayout, fieldLayout, dungeonLayout);

            // 저장
            SaveAsset(townLayout, LAYOUT_PATH + "LAYOUT_Town.asset");
            SaveAsset(fieldLayout, LAYOUT_PATH + "LAYOUT_Field.asset");
            SaveAsset(dungeonLayout, LAYOUT_PATH + "LAYOUT_Dungeon.asset");
            SaveAsset(worldMapLayout, WORLD_MAP_PATH + "WorldMapLayout.asset");

            Debug.Log("[WorldMapLayoutGenerator] Generated default world map layout with 3 regions");
            EditorUtility.DisplayDialog(
                "World Map Layout Generated",
                "통합 월드맵 레이아웃이 생성되었습니다.\n" +
                "- LAYOUT_Town.asset\n" +
                "- LAYOUT_Field.asset\n" +
                "- LAYOUT_Dungeon.asset\n" +
                "- WorldMapLayout.asset",
                "OK"
            );
        }

        [MenuItem("Game/Generators/World Map/Generate Town Layout Only", false, 101)]
        public static void GenerateTownLayoutOnly()
        {
            EnsureDirectoriesExist();
            var layout = CreateTownLayout();
            SaveAsset(layout, LAYOUT_PATH + "LAYOUT_Town.asset");
            Debug.Log("[WorldMapLayoutGenerator] Generated Town layout");
        }

        [MenuItem("Game/Generators/World Map/Generate Field Layout Only", false, 102)]
        public static void GenerateFieldLayoutOnly()
        {
            EnsureDirectoriesExist();
            var layout = CreateFieldLayout();
            SaveAsset(layout, LAYOUT_PATH + "LAYOUT_Field.asset");
            Debug.Log("[WorldMapLayoutGenerator] Generated Field layout");
        }

        [MenuItem("Game/Generators/World Map/Generate Dungeon Layout Only", false, 103)]
        public static void GenerateDungeonLayoutOnly()
        {
            EnsureDirectoriesExist();
            var layout = CreateDungeonLayout();
            SaveAsset(layout, LAYOUT_PATH + "LAYOUT_Dungeon.asset");
            Debug.Log("[WorldMapLayoutGenerator] Generated Dungeon layout");
        }

        #endregion

        #region Layout Creation

        /// <summary>
        /// 통합 월드맵 레이아웃 생성
        /// Create unified world map layout
        /// </summary>
        private static WorldMapLayoutData CreateWorldMapLayout(
            MapLayoutData townLayout,
            MapLayoutData fieldLayout,
            MapLayoutData dungeonLayout)
        {
            var worldMap = ScriptableObject.CreateInstance<WorldMapLayoutData>();
            worldMap.worldMapId = "world_map_default";
            worldMap.worldMapName = "기본 월드맵 (Default World Map)";

            // 지역 엔트리 추가
            worldMap.regions = new List<RegionLayoutEntry>
            {
                new RegionLayoutEntry
                {
                    regionId = "town",
                    displayName = "마을 (Town)",
                    mapType = MapType.Town,
                    layout = townLayout,
                    worldOffset = new Vector2(-6f, 0f),
                    regionColor = new Color(0.2f, 0.7f, 0.3f, 0.2f)
                },
                new RegionLayoutEntry
                {
                    regionId = "field",
                    displayName = "필드 (Field)",
                    mapType = MapType.Field,
                    layout = fieldLayout,
                    worldOffset = new Vector2(0f, 0f),
                    regionColor = new Color(0.3f, 0.5f, 0.8f, 0.2f)
                },
                new RegionLayoutEntry
                {
                    regionId = "dungeon",
                    displayName = "던전 (Dungeon)",
                    mapType = MapType.Dungeon,
                    layout = dungeonLayout,
                    worldOffset = new Vector2(10f, 0f),
                    regionColor = new Color(0.7f, 0.2f, 0.2f, 0.2f)
                }
            };

            // 지역 간 연결 설정
            worldMap.crossRegionConnections = new List<CrossRegionConnection>();

            // Town → Field 연결 (Town의 각 노드 → Field의 첫 층 노드들)
            // Town Floor 0, Column 0 (Shop) → Field Floor 0, Column 0, 1
            worldMap.crossRegionConnections.Add(new CrossRegionConnection
            {
                fromRegionId = "town",
                fromFloor = 0,
                fromColumn = 0,
                toRegionId = "field",
                toFloor = 0,
                toColumn = 0
            });
            worldMap.crossRegionConnections.Add(new CrossRegionConnection
            {
                fromRegionId = "town",
                fromFloor = 0,
                fromColumn = 0,
                toRegionId = "field",
                toFloor = 0,
                toColumn = 1
            });

            // Town Floor 0, Column 1 (Rest) → Field Floor 0, Column 0, 1
            worldMap.crossRegionConnections.Add(new CrossRegionConnection
            {
                fromRegionId = "town",
                fromFloor = 0,
                fromColumn = 1,
                toRegionId = "field",
                toFloor = 0,
                toColumn = 0
            });
            worldMap.crossRegionConnections.Add(new CrossRegionConnection
            {
                fromRegionId = "town",
                fromFloor = 0,
                fromColumn = 1,
                toRegionId = "field",
                toFloor = 0,
                toColumn = 1
            });

            // Town Floor 0, Column 2 (Event) → Field Floor 0, Column 0, 1
            worldMap.crossRegionConnections.Add(new CrossRegionConnection
            {
                fromRegionId = "town",
                fromFloor = 0,
                fromColumn = 2,
                toRegionId = "field",
                toFloor = 0,
                toColumn = 0
            });
            worldMap.crossRegionConnections.Add(new CrossRegionConnection
            {
                fromRegionId = "town",
                fromFloor = 0,
                fromColumn = 2,
                toRegionId = "field",
                toFloor = 0,
                toColumn = 1
            });

            // Field → Dungeon 연결 (Field 마지막 층 → Dungeon 첫 층)
            worldMap.crossRegionConnections.Add(new CrossRegionConnection
            {
                fromRegionId = "field",
                fromFloor = 7,
                fromColumn = 0,
                toRegionId = "dungeon",
                toFloor = 0,
                toColumn = 0
            });
            worldMap.crossRegionConnections.Add(new CrossRegionConnection
            {
                fromRegionId = "field",
                fromFloor = 7,
                fromColumn = 0,
                toRegionId = "dungeon",
                toFloor = 0,
                toColumn = 1
            });

            return worldMap;
        }

        /// <summary>
        /// 마을 레이아웃 생성
        /// Create town layout
        /// </summary>
        private static MapLayoutData CreateTownLayout()
        {
            var layout = ScriptableObject.CreateInstance<MapLayoutData>();
            layout.layoutId = "town_default";
            layout.layoutName = "마을 레이아웃 (Town Layout)";
            layout.mapType = MapType.Town;
            layout.floors = new List<FloorLayoutData>();

            // Floor 0: Shop, Rest, Event
            var floor0 = new FloorLayoutData
            {
                floorIndex = 0,
                nodes = new List<NodeLayoutData>
                {
                    new NodeLayoutData
                    {
                        column = 0,
                        positionOffset = new Vector2(-2f, 0f),
                        nodeType = MapNodeType.Shop,
                        connectsToColumns = new List<int>()  // Town 노드는 지역 간 연결로 처리
                    },
                    new NodeLayoutData
                    {
                        column = 1,
                        positionOffset = new Vector2(0f, 1f),
                        nodeType = MapNodeType.Rest,
                        connectsToColumns = new List<int>()
                    },
                    new NodeLayoutData
                    {
                        column = 2,
                        positionOffset = new Vector2(2f, 0f),
                        nodeType = MapNodeType.Event,
                        connectsToColumns = new List<int>()
                    }
                }
            };
            layout.floors.Add(floor0);

            return layout;
        }

        /// <summary>
        /// 필드 레이아웃 생성
        /// Create field layout
        /// </summary>
        private static MapLayoutData CreateFieldLayout()
        {
            var layout = ScriptableObject.CreateInstance<MapLayoutData>();
            layout.layoutId = "field_default";
            layout.layoutName = "필드 레이아웃 (Field Layout)";
            layout.mapType = MapType.Field;
            layout.floors = new List<FloorLayoutData>();

            // Floor 0: 2 Combat nodes
            layout.floors.Add(CreateFloor(0, new[]
            {
                (0, MapNodeType.Combat, new[] { 0, 1 }),
                (1, MapNodeType.Combat, new[] { 1, 2 })
            }));

            // Floor 1: Combat, Event, Combat
            layout.floors.Add(CreateFloor(1, new[]
            {
                (0, MapNodeType.Combat, new[] { 0 }),
                (1, MapNodeType.Event, new[] { 0, 1 }),
                (2, MapNodeType.Combat, new[] { 1 })
            }));

            // Floor 2: Treasure, Combat
            layout.floors.Add(CreateFloor(2, new[]
            {
                (0, MapNodeType.Treasure, new[] { 0, 1 }),
                (1, MapNodeType.Combat, new[] { 0, 1 })
            }));

            // Floor 3: Combat, Event
            layout.floors.Add(CreateFloor(3, new[]
            {
                (0, MapNodeType.Combat, new[] { 0 }),
                (1, MapNodeType.Event, new[] { 0 })
            }));

            // Floor 4: Elite
            layout.floors.Add(CreateFloor(4, new[]
            {
                (0, MapNodeType.Elite, new[] { 0 })
            }));

            // Floor 5: Rest
            layout.floors.Add(CreateFloor(5, new[]
            {
                (0, MapNodeType.Rest, new[] { 0, 1 })
            }));

            // Floor 6: Combat, Shop
            layout.floors.Add(CreateFloor(6, new[]
            {
                (0, MapNodeType.Combat, new[] { 0 }),
                (1, MapNodeType.Shop, new[] { 0 })
            }));

            // Floor 7: Event (마지막 층 - Dungeon으로 연결)
            layout.floors.Add(CreateFloor(7, new[]
            {
                (0, MapNodeType.Event, new int[] { })  // 지역 간 연결로 처리
            }));

            return layout;
        }

        /// <summary>
        /// 던전 레이아웃 생성
        /// Create dungeon layout
        /// </summary>
        private static MapLayoutData CreateDungeonLayout()
        {
            var layout = ScriptableObject.CreateInstance<MapLayoutData>();
            layout.layoutId = "dungeon_default";
            layout.layoutName = "던전 레이아웃 (Dungeon Layout)";
            layout.mapType = MapType.Dungeon;
            layout.floors = new List<FloorLayoutData>();

            // Floor 0: 2 Combat nodes
            layout.floors.Add(CreateFloor(0, new[]
            {
                (0, MapNodeType.Combat, new[] { 0 }),
                (1, MapNodeType.Combat, new[] { 0 })
            }));

            // Floor 1: Elite
            layout.floors.Add(CreateFloor(1, new[]
            {
                (0, MapNodeType.Elite, new[] { 0, 1 })
            }));

            // Floor 2: Combat, Elite
            layout.floors.Add(CreateFloor(2, new[]
            {
                (0, MapNodeType.Combat, new[] { 0 }),
                (1, MapNodeType.Elite, new[] { 0 })
            }));

            // Floor 3: Rest
            layout.floors.Add(CreateFloor(3, new[]
            {
                (0, MapNodeType.Rest, new[] { 0 })
            }));

            // Floor 4: Boss
            layout.floors.Add(CreateFloor(4, new[]
            {
                (0, MapNodeType.Boss, new int[] { })
            }));

            return layout;
        }

        /// <summary>
        /// 층 레이아웃 생성 헬퍼
        /// Floor layout creation helper
        /// </summary>
        private static FloorLayoutData CreateFloor(int floorIndex, (int column, MapNodeType type, int[] connections)[] nodes)
        {
            var floor = new FloorLayoutData
            {
                floorIndex = floorIndex,
                nodes = new List<NodeLayoutData>()
            };

            foreach (var (column, type, connections) in nodes)
            {
                floor.nodes.Add(new NodeLayoutData
                {
                    column = column,
                    positionOffset = Vector2.zero,
                    nodeType = type,
                    connectsToColumns = new List<int>(connections)
                });
            }

            return floor;
        }

        #endregion

        #region Utility

        private static void EnsureDirectoriesExist()
        {
            if (!Directory.Exists(LAYOUT_PATH))
            {
                Directory.CreateDirectory(LAYOUT_PATH);
            }
            if (!Directory.Exists(WORLD_MAP_PATH))
            {
                Directory.CreateDirectory(WORLD_MAP_PATH);
            }
            AssetDatabase.Refresh();
        }

        private static void SaveAsset(Object asset, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(asset, existing);
                EditorUtility.SetDirty(existing);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path);
            }
            AssetDatabase.SaveAssets();
        }

        #endregion
    }
}
#endif
