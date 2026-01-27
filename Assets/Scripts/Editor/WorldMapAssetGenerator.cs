// Editor/WorldMapAssetGenerator.cs
// 월드맵 에셋 생성 도구 (World Map Asset Generator)

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ProjectSS.Core;
using ProjectSS.Map;
using ProjectSS.Map.Data;
using ProjectSS.Map.UI;

namespace ProjectSS.Editor
{
    /// <summary>
    /// 월드맵 에셋 생성 도구
    /// - 폴더 구조 생성
    /// - WorldMapVisualConfig 생성
    /// - 테스트 맵 데이터 생성
    /// - 노드/경로 프리팹 생성
    /// </summary>
    public class WorldMapAssetGenerator : EditorWindow
    {
        private const string MAPS_PATH = "Assets/Data/Maps";
        private const string PREFABS_PATH = "Assets/Prefabs/Map";
        private const string VISUAL_CONFIG_PATH = "Assets/Data/Maps/WorldMapVisualConfig.asset";
        private const string TEST_MAP_PATH = "Assets/Data/Maps/TestWorldMap.asset";
        private const string NODE_PREFAB_PATH = "Assets/Prefabs/Map/WorldMapNodePrefab.prefab";
        private const string PATH_PREFAB_PATH = "Assets/Prefabs/Map/WorldMapPathPrefab.prefab";

        [MenuItem("PSTS/World Map Asset Generator")]
        public static void ShowWindow()
        {
            GetWindow<WorldMapAssetGenerator>("World Map Asset Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("월드맵 에셋 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("폴더 구조 생성", GUILayout.Height(30)))
            {
                CreateFolderStructure();
            }

            GUILayout.Space(10);
            GUILayout.Label("에셋 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("Visual Config 생성", GUILayout.Height(30)))
            {
                CreateVisualConfig();
            }

            if (GUILayout.Button("테스트 맵 데이터 생성", GUILayout.Height(30)))
            {
                CreateTestWorldMapData();
            }

            GUILayout.Space(10);
            GUILayout.Label("프리팹 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("노드 프리팹 생성", GUILayout.Height(30)))
            {
                CreateNodePrefab();
            }

            if (GUILayout.Button("경로 프리팹 생성", GUILayout.Height(30)))
            {
                CreatePathPrefab();
            }

            GUILayout.Space(20);
            GUILayout.Label("전체 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("모두 생성", GUILayout.Height(40)))
            {
                CreateAll();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "'모두 생성' 버튼을 누르면 월드맵 시스템에 필요한 모든 에셋이 생성됩니다.\n" +
                "- Visual Config: 노드 색상 및 시각적 설정\n" +
                "- Test Map: 6개 노드로 구성된 테스트 맵\n" +
                "- Prefabs: 노드 및 경로 UI 프리팹",
                MessageType.Info);
        }

        /// <summary>
        /// 모든 에셋 생성
        /// </summary>
        private void CreateAll()
        {
            CreateFolderStructure();
            CreateVisualConfig();
            CreateNodePrefab();
            CreatePathPrefab();
            CreateTestWorldMapData();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[WorldMapAssetGenerator] 모든 에셋 생성 완료!");
        }

        /// <summary>
        /// 모든 에셋 생성 (ProjectBootstrapper에서 호출)
        /// </summary>
        public static void CreateAllAssetsStatic()
        {
            var generator = CreateInstance<WorldMapAssetGenerator>();
            generator.CreateFolderStructure();
            generator.CreateVisualConfig();
            generator.CreateNodePrefab();
            generator.CreatePathPrefab();
            generator.CreateTestWorldMapData();
            DestroyImmediate(generator);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[WorldMapAssetGenerator] Static: 모든 에셋 생성 완료");
        }

        #region Folder Structure

        /// <summary>
        /// 폴더 구조 생성
        /// </summary>
        private void CreateFolderStructure()
        {
            CreateFolderIfNotExists(MAPS_PATH);
            CreateFolderIfNotExists(PREFABS_PATH);

            AssetDatabase.Refresh();
            Debug.Log("[WorldMapAssetGenerator] 폴더 구조 생성 완료");
        }

        private void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentFolder = Path.GetDirectoryName(path).Replace("\\", "/");
                string newFolderName = Path.GetFileName(path);

                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    CreateFolderIfNotExists(parentFolder);
                }

                AssetDatabase.CreateFolder(parentFolder, newFolderName);
                Debug.Log($"[WorldMapAssetGenerator] 폴더 생성: {path}");
            }
        }

        #endregion

        #region Visual Config

        /// <summary>
        /// WorldMapVisualConfig 생성
        /// </summary>
        private void CreateVisualConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<WorldMapVisualConfig>(VISUAL_CONFIG_PATH) != null)
            {
                Debug.Log($"[WorldMapAssetGenerator] 이미 존재: {VISUAL_CONFIG_PATH}");
                return;
            }

            CreateFolderStructure();

            var config = ScriptableObject.CreateInstance<WorldMapVisualConfig>();
            // WorldMapVisualConfig has default values defined in the class

            AssetDatabase.CreateAsset(config, VISUAL_CONFIG_PATH);
            Debug.Log($"[WorldMapAssetGenerator] 생성: {VISUAL_CONFIG_PATH}");
        }

        #endregion

        #region Test Map Data

        /// <summary>
        /// 테스트 맵 데이터 생성
        /// </summary>
        private void CreateTestWorldMapData()
        {
            if (AssetDatabase.LoadAssetAtPath<WorldMapData>(TEST_MAP_PATH) != null)
            {
                Debug.Log($"[WorldMapAssetGenerator] 이미 존재: {TEST_MAP_PATH}");
                return;
            }

            CreateFolderStructure();

            var mapData = ScriptableObject.CreateInstance<WorldMapData>();

            mapData.MapId = "act1_test";
            mapData.MapName = "1막 - 테스트 여정";
            mapData.Description = "ProjectBootstrapper에서 생성된 테스트용 월드맵입니다.";

            // 맵 구조:
            //         [Dungeon2] (300, 350)
            //        /
            // [Start] -- [Shelter] -- [Boss]
            //        \
            //         [Dungeon1] -- [Dungeon3]
            //
            // 좌표는 UI 렌더링용 (픽셀 단위)

            mapData.Nodes = new List<WorldMapNode>
            {
                CreateNode("start", RegionType.Start, "시작점", new Vector2(100, 250),
                    new[] { "dungeon1", "shelter" }),

                CreateNode("dungeon1", RegionType.Dungeon, "어둠의 숲", new Vector2(250, 100),
                    new[] { "start", "dungeon3" }, difficulty: 1),

                CreateNode("shelter", RegionType.Shelter, "여행자의 쉼터", new Vector2(300, 250),
                    new[] { "start", "dungeon2", "boss" }),

                CreateNode("dungeon2", RegionType.Dungeon, "고블린 동굴", new Vector2(350, 400),
                    new[] { "shelter" }, difficulty: 2),

                CreateNode("dungeon3", RegionType.Dungeon, "잊혀진 유적", new Vector2(500, 100),
                    new[] { "dungeon1", "boss" }, difficulty: 3),

                CreateNode("boss", RegionType.Boss, "드래곤의 둥지", new Vector2(600, 250),
                    new[] { "shelter", "dungeon3" }, difficulty: 5),
            };

            mapData.StartNodeId = "start";
            mapData.EndNodeId = "boss";

            // 유효성 검증
            if (mapData.Validate(out var errors))
            {
                AssetDatabase.CreateAsset(mapData, TEST_MAP_PATH);
                Debug.Log($"[WorldMapAssetGenerator] 생성: {TEST_MAP_PATH}");
            }
            else
            {
                foreach (var error in errors)
                {
                    Debug.LogError($"[WorldMapAssetGenerator] 맵 검증 실패: {error}");
                }
            }
        }

        /// <summary>
        /// 노드 생성 헬퍼
        /// </summary>
        private WorldMapNode CreateNode(string id, RegionType type, string name,
            Vector2 position, string[] connections, int difficulty = 1)
        {
            return new WorldMapNode
            {
                NodeId = id,
                RegionType = type,
                DisplayName = name,
                Position = position,
                ConnectedNodeIds = new List<string>(connections),
                Difficulty = difficulty
            };
        }

        #endregion

        #region Prefab Creation

        /// <summary>
        /// 노드 프리팹 생성
        /// </summary>
        private void CreateNodePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(NODE_PREFAB_PATH) != null)
            {
                Debug.Log($"[WorldMapAssetGenerator] 이미 존재: {NODE_PREFAB_PATH}");
                return;
            }

            CreateFolderStructure();

            // 루트 오브젝트
            var nodeGo = new GameObject("WorldMapNodePrefab");
            var rect = nodeGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 80);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // AccessibleGlow (가장 뒤에 - 첫 번째로 생성하지만 SetAsFirstSibling으로 이동)
            var glowGo = CreateImageChild(nodeGo.transform, "AccessibleGlow", new Vector2(100, 100));
            var glowImage = glowGo.GetComponent<Image>();
            glowImage.color = new Color(1f, 1f, 0.5f, 0.5f);
            glowGo.SetActive(false);

            // Background
            var bgGo = CreateImageChild(nodeGo.transform, "Background", new Vector2(80, 80));
            var bgImage = bgGo.GetComponent<Image>();
            bgImage.color = new Color(0.4f, 0.4f, 0.5f, 1f);
            UIComponentFactory.SetRectTransformFill(bgGo.GetComponent<RectTransform>());

            // IconImage
            var iconGo = CreateImageChild(nodeGo.transform, "IconImage", new Vector2(40, 40));
            var iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.55f);
            iconRect.anchorMax = new Vector2(0.5f, 0.55f);
            iconRect.anchoredPosition = Vector2.zero;
            iconGo.GetComponent<Image>().color = Color.white;

            // NameText (TMP)
            var nameGo = new GameObject("NameText");
            nameGo.transform.SetParent(nodeGo.transform, false);
            var nameRect = nameGo.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0);
            nameRect.anchorMax = new Vector2(1, 0.35f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            var nameText = nameGo.AddComponent<TextMeshProUGUI>();
            nameText.text = "노드";
            nameText.fontSize = 12;
            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.enableWordWrapping = false;
            nameText.overflowMode = TextOverflowModes.Ellipsis;

            // ClearedOverlay
            var clearedGo = CreateImageChild(nodeGo.transform, "ClearedOverlay", new Vector2(80, 80));
            var clearedImage = clearedGo.GetComponent<Image>();
            clearedImage.color = new Color(0f, 0f, 0f, 0.5f);
            UIComponentFactory.SetRectTransformFill(clearedGo.GetComponent<RectTransform>());
            clearedGo.SetActive(false);

            // PlayerMarker
            var markerGo = CreateImageChild(nodeGo.transform, "PlayerMarker", new Vector2(20, 20));
            var markerRect = markerGo.GetComponent<RectTransform>();
            markerRect.anchorMin = new Vector2(0.5f, 1f);
            markerRect.anchorMax = new Vector2(0.5f, 1f);
            markerRect.anchoredPosition = new Vector2(0, 15);
            markerGo.GetComponent<Image>().color = Color.cyan;
            markerGo.SetActive(false);

            // AccessibleGlow를 가장 뒤로
            glowGo.transform.SetAsFirstSibling();

            // 컴포넌트 추가
            var button = nodeGo.AddComponent<Button>();
            var canvasGroup = nodeGo.AddComponent<CanvasGroup>();
            var nodeUI = nodeGo.AddComponent<WorldMapNodeUI>();

            // 필드 연결
            UIComponentFactory.SetPrivateField(nodeUI, "_backgroundImage", bgImage);
            UIComponentFactory.SetPrivateField(nodeUI, "_iconImage", iconGo.GetComponent<Image>());
            UIComponentFactory.SetPrivateField(nodeUI, "_nameText", nameText);
            UIComponentFactory.SetPrivateField(nodeUI, "_accessibleGlow", glowImage);
            UIComponentFactory.SetPrivateField(nodeUI, "_clearedOverlay", clearedImage);
            UIComponentFactory.SetPrivateField(nodeUI, "_playerMarker", markerGo.GetComponent<Image>());
            UIComponentFactory.SetPrivateField(nodeUI, "_button", button);
            UIComponentFactory.SetPrivateField(nodeUI, "_canvasGroup", canvasGroup);

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(nodeGo, NODE_PREFAB_PATH);
            DestroyImmediate(nodeGo);

            Debug.Log($"[WorldMapAssetGenerator] 생성: {NODE_PREFAB_PATH}");
        }

        /// <summary>
        /// 경로 프리팹 생성
        /// </summary>
        private void CreatePathPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PATH_PREFAB_PATH) != null)
            {
                Debug.Log($"[WorldMapAssetGenerator] 이미 존재: {PATH_PREFAB_PATH}");
                return;
            }

            CreateFolderStructure();

            // 루트 오브젝트
            var pathGo = new GameObject("WorldMapPathPrefab");
            var rect = pathGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 4);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // 라인 이미지 (루트에 직접)
            var lineImage = pathGo.AddComponent<Image>();
            lineImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            // PathUI 컴포넌트
            var pathUI = pathGo.AddComponent<WorldMapPathUI>();
            UIComponentFactory.SetPrivateField(pathUI, "_lineImage", lineImage);
            UIComponentFactory.SetPrivateField(pathUI, "_rectTransform", rect);

            // 프리팹 저장
            PrefabUtility.SaveAsPrefabAsset(pathGo, PATH_PREFAB_PATH);
            DestroyImmediate(pathGo);

            Debug.Log($"[WorldMapAssetGenerator] 생성: {PATH_PREFAB_PATH}");
        }

        /// <summary>
        /// 이미지 자식 오브젝트 생성 헬퍼
        /// </summary>
        private GameObject CreateImageChild(Transform parent, string name, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            go.AddComponent<Image>();
            return go;
        }

        #endregion

        #region Static Helpers for Scene Builder

        /// <summary>
        /// 테스트 맵 데이터 로드
        /// </summary>
        public static WorldMapData GetTestMapData()
        {
            return AssetDatabase.LoadAssetAtPath<WorldMapData>(TEST_MAP_PATH);
        }

        /// <summary>
        /// Visual Config 로드
        /// </summary>
        public static WorldMapVisualConfig GetVisualConfig()
        {
            return AssetDatabase.LoadAssetAtPath<WorldMapVisualConfig>(VISUAL_CONFIG_PATH);
        }

        /// <summary>
        /// 노드 프리팹 로드
        /// </summary>
        public static GameObject GetNodePrefab()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(NODE_PREFAB_PATH);
        }

        /// <summary>
        /// 경로 프리팹 로드
        /// </summary>
        public static GameObject GetPathPrefab()
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(PATH_PREFAB_PATH);
        }

        /// <summary>
        /// WorldMapSceneInitializer에 맵 데이터 연결
        /// </summary>
        public static void LinkToInitializer(WorldMapSceneInitializer initializer)
        {
            if (initializer == null) return;

            var testMapData = GetTestMapData();
            if (testMapData != null)
            {
                var so = new SerializedObject(initializer);
                var mapDataProp = so.FindProperty("_defaultMapData");
                if (mapDataProp != null)
                {
                    mapDataProp.objectReferenceValue = testMapData;
                    so.ApplyModifiedProperties();
                }
            }
        }

        /// <summary>
        /// WorldMapController에 프리팹/설정 연결
        /// </summary>
        public static void LinkToController(WorldMapController controller,
            RectTransform nodesContainer, RectTransform pathsContainer, ScrollRect scrollRect)
        {
            if (controller == null) return;

            var nodePrefab = GetNodePrefab();
            var pathPrefab = GetPathPrefab();
            var visualConfig = GetVisualConfig();

            var so = new SerializedObject(controller);

            var nodePrefabProp = so.FindProperty("_nodePrefab");
            var pathPrefabProp = so.FindProperty("_pathPrefab");
            var nodesContainerProp = so.FindProperty("_nodesContainer");
            var pathsContainerProp = so.FindProperty("_pathsContainer");
            var visualConfigProp = so.FindProperty("_visualConfig");
            var scrollRectProp = so.FindProperty("_scrollRect");

            if (nodePrefabProp != null) nodePrefabProp.objectReferenceValue = nodePrefab;
            if (pathPrefabProp != null) pathPrefabProp.objectReferenceValue = pathPrefab;
            if (nodesContainerProp != null) nodesContainerProp.objectReferenceValue = nodesContainer;
            if (pathsContainerProp != null) pathsContainerProp.objectReferenceValue = pathsContainer;
            if (visualConfigProp != null) visualConfigProp.objectReferenceValue = visualConfig;
            if (scrollRectProp != null) scrollRectProp.objectReferenceValue = scrollRect;

            so.ApplyModifiedProperties();
            Debug.Log("[WorldMapAssetGenerator] WorldMapController 연결 완료");
        }

        #endregion
    }
}
#endif
