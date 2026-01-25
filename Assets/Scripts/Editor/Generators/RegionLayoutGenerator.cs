#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 지역 및 맵 레이아웃 생성기
    /// Region and map layout generator
    /// </summary>
    public class RegionLayoutGenerator : EditorWindow
    {
        private const string LAYOUTS_PATH = GeneratorUtility.MAP_PATH + "/Layouts";
        private const string REGIONS_PATH = GeneratorUtility.MAP_PATH + "/Regions";

        [MenuItem("Game/Generators/Region & Layout Generator")]
        public static void ShowWindow()
        {
            GetWindow<RegionLayoutGenerator>("Region Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("지역 및 레이아웃 생성기", EditorStyles.boldLabel);
            GUILayout.Label("Region & Layout Generator", EditorStyles.miniLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "기본 레이아웃 및 지역 에셋을 생성합니다.\n" +
                "Generates default layout and region assets.",
                MessageType.Info);

            GUILayout.Space(20);

            if (GUILayout.Button("기본 레이아웃 생성\nGenerate Default Layouts", GUILayout.Height(50)))
            {
                GenerateDefaultLayouts();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("기본 지역 생성\nGenerate Default Regions", GUILayout.Height(50)))
            {
                GenerateDefaultRegions();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("전체 생성 (레이아웃 + 지역)\nGenerate All", GUILayout.Height(50)))
            {
                GenerateDefaultLayouts();
                GenerateDefaultRegions();
            }
        }

        /// <summary>
        /// 기본 레이아웃 생성
        /// Generate default layouts
        /// </summary>
        private void GenerateDefaultLayouts()
        {
            GeneratorUtility.EnsureFolderExists(LAYOUTS_PATH);

            // Town Layout
            CreateTownLayout();

            // Field Layout
            CreateFieldLayout();

            // Dungeon Layout
            CreateDungeonLayout();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("기본 레이아웃 생성 완료 / Default layouts generated");
            EditorUtility.DisplayDialog("완료", "기본 레이아웃이 생성되었습니다.\nDefault layouts have been created.", "확인");
        }

        /// <summary>
        /// 기본 지역 생성
        /// Generate default regions
        /// </summary>
        private void GenerateDefaultRegions()
        {
            GeneratorUtility.EnsureFolderExists(REGIONS_PATH);

            // 레이아웃 로드
            var townLayout = AssetDatabase.LoadAssetAtPath<MapLayoutData>($"{LAYOUTS_PATH}/LAYOUT_Town.asset");
            var fieldLayout = AssetDatabase.LoadAssetAtPath<MapLayoutData>($"{LAYOUTS_PATH}/LAYOUT_Field.asset");
            var dungeonLayout = AssetDatabase.LoadAssetAtPath<MapLayoutData>($"{LAYOUTS_PATH}/LAYOUT_Dungeon.asset");

            // Dungeon Region (마지막)
            var dungeonRegion = CreateRegion(
                "dungeon",
                "던전",
                "Dungeon",
                "위험한 던전. 보스가 기다리고 있다.",
                MapType.Dungeon,
                dungeonLayout,
                null,
                true
            );

            // Field Region
            var fieldRegion = CreateRegion(
                "field",
                "필드",
                "Field",
                "모험이 시작되는 필드. 다양한 전투와 이벤트가 기다린다.",
                MapType.Field,
                fieldLayout,
                dungeonRegion,
                false
            );

            // Town Region (시작)
            var townRegion = CreateRegion(
                "town",
                "마을",
                "Town",
                "평화로운 마을. 휴식하고 준비하자.",
                MapType.Town,
                townLayout,
                fieldRegion,
                false
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("기본 지역 생성 완료 / Default regions generated");
            EditorUtility.DisplayDialog("완료", "기본 지역이 생성되었습니다.\nDefault regions have been created.", "확인");
        }

        #region Layout Creation

        private void CreateTownLayout()
        {
            var layout = ScriptableObject.CreateInstance<MapLayoutData>();
            layout.layoutId = "layout_town";
            layout.layoutName = "Town Layout";
            layout.mapType = MapType.Town;

            // 마을: 단일 층에 3개 노드 (Shop, Rest, Event)
            var floor0 = new FloorLayoutData { floorIndex = 0 };
            floor0.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Shop,
                positionOffset = new Vector2(-2f, 0f),
                connectsToColumns = new List<int>()
            });
            floor0.nodes.Add(new NodeLayoutData
            {
                column = 1,
                nodeType = MapNodeType.Rest,
                positionOffset = new Vector2(0f, 1f),
                connectsToColumns = new List<int>()
            });
            floor0.nodes.Add(new NodeLayoutData
            {
                column = 2,
                nodeType = MapNodeType.Event,
                positionOffset = new Vector2(2f, 0f),
                connectsToColumns = new List<int>()
            });

            layout.floors.Add(floor0);

            GeneratorUtility.SaveAsset(layout, LAYOUTS_PATH, "LAYOUT_Town", false);
        }

        private void CreateFieldLayout()
        {
            var layout = ScriptableObject.CreateInstance<MapLayoutData>();
            layout.layoutId = "layout_field";
            layout.layoutName = "Field Layout";
            layout.mapType = MapType.Field;

            // Floor 0: 2 Combat nodes (starting)
            var floor0 = new FloorLayoutData { floorIndex = 0 };
            floor0.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0, 1 }
            });
            floor0.nodes.Add(new NodeLayoutData
            {
                column = 1,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 1, 2 }
            });
            layout.floors.Add(floor0);

            // Floor 1: 3 nodes (Combat, Event, Combat)
            var floor1 = new FloorLayoutData { floorIndex = 1 };
            floor1.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            floor1.nodes.Add(new NodeLayoutData
            {
                column = 1,
                nodeType = MapNodeType.Event,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0, 1 }
            });
            floor1.nodes.Add(new NodeLayoutData
            {
                column = 2,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 1 }
            });
            layout.floors.Add(floor1);

            // Floor 2: 2 nodes (Treasure, Combat)
            var floor2 = new FloorLayoutData { floorIndex = 2 };
            floor2.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Treasure,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0, 1 }
            });
            floor2.nodes.Add(new NodeLayoutData
            {
                column = 1,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0, 1 }
            });
            layout.floors.Add(floor2);

            // Floor 3: 2 nodes (Combat, Event)
            var floor3 = new FloorLayoutData { floorIndex = 3 };
            floor3.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            floor3.nodes.Add(new NodeLayoutData
            {
                column = 1,
                nodeType = MapNodeType.Event,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            layout.floors.Add(floor3);

            // Floor 4: Elite
            var floor4 = new FloorLayoutData { floorIndex = 4 };
            floor4.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Elite,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            layout.floors.Add(floor4);

            // Floor 5: Rest
            var floor5 = new FloorLayoutData { floorIndex = 5 };
            floor5.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Rest,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0, 1 }
            });
            layout.floors.Add(floor5);

            // Floor 6: 2 nodes (Combat, Shop)
            var floor6 = new FloorLayoutData { floorIndex = 6 };
            floor6.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            floor6.nodes.Add(new NodeLayoutData
            {
                column = 1,
                nodeType = MapNodeType.Shop,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            layout.floors.Add(floor6);

            // Floor 7: Final (leads to dungeon)
            var floor7 = new FloorLayoutData { floorIndex = 7 };
            floor7.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Event,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int>() // No next floor
            });
            layout.floors.Add(floor7);

            GeneratorUtility.SaveAsset(layout, LAYOUTS_PATH, "LAYOUT_Field", false);
        }

        private void CreateDungeonLayout()
        {
            var layout = ScriptableObject.CreateInstance<MapLayoutData>();
            layout.layoutId = "layout_dungeon";
            layout.layoutName = "Dungeon Layout";
            layout.mapType = MapType.Dungeon;

            // Floor 0: 2 Combat nodes
            var floor0 = new FloorLayoutData { floorIndex = 0 };
            floor0.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            floor0.nodes.Add(new NodeLayoutData
            {
                column = 1,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            layout.floors.Add(floor0);

            // Floor 1: Elite
            var floor1 = new FloorLayoutData { floorIndex = 1 };
            floor1.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Elite,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0, 1 }
            });
            layout.floors.Add(floor1);

            // Floor 2: 2 nodes (Combat, Elite)
            var floor2 = new FloorLayoutData { floorIndex = 2 };
            floor2.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Combat,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            floor2.nodes.Add(new NodeLayoutData
            {
                column = 1,
                nodeType = MapNodeType.Elite,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            layout.floors.Add(floor2);

            // Floor 3: Rest
            var floor3 = new FloorLayoutData { floorIndex = 3 };
            floor3.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Rest,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int> { 0 }
            });
            layout.floors.Add(floor3);

            // Floor 4: Boss
            var floor4 = new FloorLayoutData { floorIndex = 4 };
            floor4.nodes.Add(new NodeLayoutData
            {
                column = 0,
                nodeType = MapNodeType.Boss,
                positionOffset = Vector2.zero,
                connectsToColumns = new List<int>() // No next floor
            });
            layout.floors.Add(floor4);

            GeneratorUtility.SaveAsset(layout, LAYOUTS_PATH, "LAYOUT_Dungeon", false);
        }

        #endregion

        #region Region Creation

        private RegionData CreateRegion(
            string id,
            string koreanName,
            string englishName,
            string description,
            MapType mapType,
            MapLayoutData layout,
            RegionData nextRegion,
            bool isFinal)
        {
            var region = ScriptableObject.CreateInstance<RegionData>();
            region.regionId = id;
            region.regionName = koreanName;
            region.regionDescription = description;
            region.mapType = mapType;
            region.mapLayout = layout;
            region.nextRegion = nextRegion;
            region.isFinalRegion = isFinal;

            string fileName = $"REG_{koreanName}_{englishName}";
            GeneratorUtility.SaveAsset(region, REGIONS_PATH, fileName, false);

            return region;
        }

        #endregion
    }
}
#endif
