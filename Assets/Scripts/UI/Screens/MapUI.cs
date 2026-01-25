using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Map;
using ProjectSS.Run;
using MapType = ProjectSS.Core.MapType;

namespace ProjectSS.UI
{
    /// <summary>
    /// 맵 UI 관리자 (통합 월드맵 + 기존 허브 시스템 지원)
    /// Map UI manager (unified world map + legacy hub system support)
    /// </summary>
    public class MapUI : MonoBehaviour
    {
        [Header("Node Display")]
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private GameObject nodeButtonPrefab;

        [Header("Path Display")]
        [SerializeField] private Transform pathContainer;
        [SerializeField] private GameObject pathLinePrefab;

        [Header("Region Display (World Map)")]
        [SerializeField] private Transform regionContainer;
        [SerializeField] private GameObject regionBackgroundPrefab;
        [SerializeField] private GameObject regionLabelPrefab;

        [Header("Town UI (Legacy)")]
        [SerializeField] private GameObject townPanel;
        [SerializeField] private Button enterFieldButton;
        [SerializeField] private TMPro.TextMeshProUGUI mapTypeLabel;

        [Header("Layout Settings")]
        [SerializeField] private float nodeScale = 50f;
        [SerializeField] private Vector2 offset = new Vector2(400f, 100f);
        [SerializeField] private Vector2 townOffset = new Vector2(400f, 300f);

        [Header("World Map Settings")]
        [SerializeField] private bool useWorldMap = true;
        [SerializeField] private float regionPadding = 1.5f;

        private Dictionary<string, GameObject> nodeObjects = new Dictionary<string, GameObject>();
        private List<GameObject> pathObjects = new List<GameObject>();
        private Dictionary<string, GameObject> regionBackgrounds = new Dictionary<string, GameObject>();
        private Dictionary<string, GameObject> regionLabels = new Dictionary<string, GameObject>();

        private void OnEnable()
        {
            // 이벤트 구독
            EventBus.Subscribe<MapGeneratedEvent>(OnMapGenerated);
            EventBus.Subscribe<WorldMapGeneratedEvent>(OnWorldMapGenerated);

            // 버튼 이벤트 연결
            if (enterFieldButton != null)
            {
                enterFieldButton.onClick.AddListener(OnEnterFieldClicked);
            }
        }

        private void Start()
        {
            // 월드맵 모드인지 확인
            if (useWorldMap && MapManager.Instance != null)
            {
                if (MapManager.Instance.IsWorldMapActive)
                {
                    RefreshWorldMap();
                }
                else if (MapManager.Instance.CurrentMap != null)
                {
                    RefreshMap();
                }
#if UNITY_EDITOR
                else
                {
                    GenerateEditorTestMap();
                }
#endif
            }
            else if (MapManager.Instance?.CurrentMap != null)
            {
                RefreshMap();
            }
#if UNITY_EDITOR
            else
            {
                GenerateEditorTestMap();
            }
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 테스트용 맵 생성
        /// Generate test map for editor testing
        /// </summary>
        private void GenerateEditorTestMap()
        {
            if (MapManager.Instance == null)
            {
                Debug.LogWarning("[MapUI] MapManager not found. Cannot generate test map.");
                return;
            }

            int testSeed = UnityEngine.Random.Range(0, 99999);

            if (useWorldMap)
            {
                Debug.Log($"[MapUI] Generating editor test world map with seed: {testSeed}");
                MapManager.Instance.GenerateWorldMap(testSeed);
            }
            else
            {
                Debug.Log($"[MapUI] Generating editor test map with seed: {testSeed}");
                MapManager.Instance.GenerateNewMap(testSeed, MapType.Town);
            }
        }
#endif

        private void OnDisable()
        {
            EventBus.Unsubscribe<MapGeneratedEvent>(OnMapGenerated);
            EventBus.Unsubscribe<WorldMapGeneratedEvent>(OnWorldMapGenerated);

            if (enterFieldButton != null)
            {
                enterFieldButton.onClick.RemoveListener(OnEnterFieldClicked);
            }
        }

        /// <summary>
        /// 기존 맵 생성 이벤트 핸들러
        /// Legacy map generated event handler
        /// </summary>
        private void OnMapGenerated(MapGeneratedEvent evt)
        {
            if (!useWorldMap)
            {
                RefreshMap();
            }
        }

        /// <summary>
        /// 월드맵 생성 이벤트 핸들러
        /// World map generated event handler
        /// </summary>
        private void OnWorldMapGenerated(WorldMapGeneratedEvent evt)
        {
            RefreshWorldMap();
        }

        #region World Map Display

        /// <summary>
        /// 통합 월드맵 새로고침
        /// Refresh unified world map display
        /// </summary>
        public void RefreshWorldMap()
        {
            ClearAll();

            if (MapManager.Instance?.WorldMap == null)
            {
                Debug.LogWarning("[MapUI] World map not available");
                return;
            }

            var worldMap = MapManager.Instance.WorldMap;

            // Town 패널 숨김 (월드맵 모드에서는 사용 안함)
            if (townPanel != null)
            {
                townPanel.SetActive(false);
            }

            // 맵 타입 라벨 숨김
            if (mapTypeLabel != null)
            {
                mapTypeLabel.text = "월드맵 (World Map)";
            }

            // 1. 지역 배경 생성
            foreach (var region in worldMap.GetAllRegions())
            {
                CreateRegionBackground(region);
                CreateRegionLabel(region);
            }

            // 2. 모든 노드 생성
            foreach (var node in worldMap.GetAllNodes())
            {
                CreateWorldMapNodeButton(node);
            }

            // 3. 모든 경로 생성
            CreateWorldMapPaths(worldMap);

            // 4. 접근성 업데이트
            UpdateWorldMapAccessibleNodes();

            Debug.Log($"[MapUI] World map refreshed: {worldMap.TotalNodeCount} nodes, {worldMap.RegionCount} regions");
        }

        /// <summary>
        /// 지역 배경 생성
        /// Create region background
        /// </summary>
        private void CreateRegionBackground(RegionMapSection region)
        {
            if (regionBackgroundPrefab == null || regionContainer == null) return;

            var bgObj = Instantiate(regionBackgroundPrefab, regionContainer);
            var rectTransform = bgObj.GetComponent<RectTransform>();
            var image = bgObj.GetComponent<Image>();

            if (rectTransform != null)
            {
                // 지역 경계 계산
                Rect bounds = region.CalculateBounds(regionPadding);

                // 월드 오프셋 적용
                Vector2 worldOffset = region.WorldOffset * nodeScale;
                Vector2 position = new Vector2(
                    bounds.x * nodeScale + worldOffset.x + offset.x,
                    bounds.y * nodeScale + worldOffset.y + offset.y
                );

                rectTransform.anchoredPosition = position;
                rectTransform.sizeDelta = new Vector2(
                    bounds.width * nodeScale,
                    bounds.height * nodeScale
                );
                rectTransform.pivot = new Vector2(0, 0);
            }

            if (image != null)
            {
                image.color = region.RegionColor;
            }

            regionBackgrounds[region.RegionId] = bgObj;
        }

        /// <summary>
        /// 지역 라벨 생성
        /// Create region label
        /// </summary>
        private void CreateRegionLabel(RegionMapSection region)
        {
            if (regionLabelPrefab == null || regionContainer == null) return;

            var labelObj = Instantiate(regionLabelPrefab, regionContainer);
            var rectTransform = labelObj.GetComponent<RectTransform>();
            var text = labelObj.GetComponent<TMPro.TextMeshProUGUI>();

            if (rectTransform != null)
            {
                // 지역 상단 중앙에 배치
                Rect bounds = region.CalculateBounds(regionPadding);
                Vector2 worldOffset = region.WorldOffset * nodeScale;
                Vector2 position = new Vector2(
                    (bounds.x + bounds.width / 2) * nodeScale + worldOffset.x + offset.x,
                    (bounds.y + bounds.height + 0.5f) * nodeScale + worldOffset.y + offset.y
                );

                rectTransform.anchoredPosition = position;
            }

            if (text != null)
            {
                text.text = region.DisplayName;
            }

            regionLabels[region.RegionId] = labelObj;
        }

        /// <summary>
        /// 월드맵 노드 버튼 생성
        /// Create world map node button
        /// </summary>
        private void CreateWorldMapNodeButton(MapNode node)
        {
            if (nodeButtonPrefab == null || nodeContainer == null) return;

            var worldMap = MapManager.Instance.WorldMap;
            var region = worldMap.GetRegion(node.RegionId);
            if (region == null) return;

            var nodeObj = Instantiate(nodeButtonPrefab, nodeContainer);
            var rectTransform = nodeObj.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                // 노드 위치 + 지역 오프셋
                Vector2 worldOffset = region.WorldOffset * nodeScale;
                Vector2 screenPos = node.Position * nodeScale + worldOffset + offset;
                rectTransform.anchoredPosition = screenPos;
            }

            // 버튼 설정
            var button = nodeObj.GetComponent<Button>();
            if (button != null)
            {
                string nodeId = node.NodeId;
                button.onClick.AddListener(() => OnWorldMapNodeClicked(nodeId));
            }

            // 색상 및 텍스트 설정
            var image = nodeObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = node.GetNodeColor();
            }

            var text = nodeObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = node.GetNodeSymbol();
            }

            // 방문한 노드 표시
            if (node.IsVisited)
            {
                if (image != null)
                {
                    var color = image.color;
                    color.a = 0.5f;
                    image.color = color;
                }
            }

            nodeObjects[node.NodeId] = nodeObj;
        }

        /// <summary>
        /// 월드맵 경로 생성
        /// Create world map paths
        /// </summary>
        private void CreateWorldMapPaths(WorldMapData worldMap)
        {
            if (pathLinePrefab == null || pathContainer == null) return;

            foreach (var node in worldMap.GetAllNodes())
            {
                var fromRegion = worldMap.GetRegion(node.RegionId);
                if (fromRegion == null) continue;

                foreach (var connectedId in node.ConnectedNodeIds)
                {
                    var connectedNode = worldMap.GetNode(connectedId);
                    if (connectedNode == null) continue;

                    var toRegion = worldMap.GetRegion(connectedNode.RegionId);
                    if (toRegion == null) continue;

                    var pathObj = Instantiate(pathLinePrefab, pathContainer);

                    // 시작점과 끝점 계산 (지역 오프셋 포함)
                    Vector2 fromWorldOffset = fromRegion.WorldOffset * nodeScale;
                    Vector2 toWorldOffset = toRegion.WorldOffset * nodeScale;

                    Vector2 startPos = node.Position * nodeScale + fromWorldOffset + offset;
                    Vector2 endPos = connectedNode.Position * nodeScale + toWorldOffset + offset;

                    // LineRenderer 사용
                    var line = pathObj.GetComponent<LineRenderer>();
                    if (line != null)
                    {
                        line.SetPosition(0, new Vector3(startPos.x, startPos.y, 0));
                        line.SetPosition(1, new Vector3(endPos.x, endPos.y, 0));

                        // 지역 간 연결은 다른 색상
                        if (node.RegionId != connectedNode.RegionId)
                        {
                            line.startColor = new Color(1f, 0.8f, 0.2f, 0.8f);
                            line.endColor = new Color(1f, 0.8f, 0.2f, 0.8f);
                        }
                    }

                    // UI Image로 선 그리기 (대안)
                    var rectTransform = pathObj.GetComponent<RectTransform>();
                    if (rectTransform != null && line == null)
                    {
                        DrawWorldMapLineUI(rectTransform, startPos, endPos, node.RegionId != connectedNode.RegionId);
                    }

                    pathObjects.Add(pathObj);
                }
            }
        }

        /// <summary>
        /// 월드맵 선 그리기 (UI)
        /// Draw world map line (UI)
        /// </summary>
        private void DrawWorldMapLineUI(RectTransform rect, Vector2 startPos, Vector2 endPos, bool isCrossRegion)
        {
            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            rect.anchoredPosition = startPos;
            rect.sizeDelta = new Vector2(distance, isCrossRegion ? 3f : 2f);
            rect.rotation = Quaternion.Euler(0, 0, angle);
            rect.pivot = new Vector2(0, 0.5f);

            // 지역 간 연결 색상
            var image = rect.GetComponent<Image>();
            if (image != null && isCrossRegion)
            {
                image.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            }
        }

        /// <summary>
        /// 월드맵 접근 가능 노드 업데이트
        /// Update world map accessible nodes
        /// </summary>
        private void UpdateWorldMapAccessibleNodes()
        {
            if (MapManager.Instance?.WorldMap == null) return;

            foreach (var kvp in nodeObjects)
            {
                var node = MapManager.Instance.WorldMap.GetNode(kvp.Key);
                var button = kvp.Value.GetComponent<Button>();

                if (button != null && node != null)
                {
                    button.interactable = node.IsAccessible;

                    // 접근 불가 노드 시각적 표시
                    var image = kvp.Value.GetComponent<Image>();
                    if (image != null && !node.IsAccessible && !node.IsVisited)
                    {
                        var color = image.color;
                        color.a = 0.3f;
                        image.color = color;
                    }
                }
            }
        }

        /// <summary>
        /// 월드맵 노드 클릭 핸들러
        /// World map node click handler
        /// </summary>
        private void OnWorldMapNodeClicked(string nodeId)
        {
            if (MapManager.Instance.SelectWorldMapNode(nodeId))
            {
                var node = MapManager.Instance.CurrentNode;

                // 접근성 업데이트
                UpdateWorldMapAccessibleNodes();

                // RunManager에 알림
                if (RunManager.Instance != null)
                {
                    RunManager.Instance.OnNodeSelected(node);
                }

                // 노드 타입에 따라 씬 전환
                if (GameManager.Instance != null)
                {
                    switch (node.NodeType)
                    {
                        case MapNodeType.Combat:
                        case MapNodeType.Elite:
                            GameManager.Instance.LoadCombat();
                            break;
                        case MapNodeType.Boss:
                            GameManager.Instance.LoadCombat();
                            break;
                        case MapNodeType.Event:
                            GameManager.Instance.LoadEvent();
                            break;
                        case MapNodeType.Shop:
                            GameManager.Instance.LoadShop();
                            break;
                        case MapNodeType.Rest:
                            GameManager.Instance.LoadRest();
                            break;
                        case MapNodeType.Treasure:
                            GameManager.Instance.LoadReward();
                            break;
                    }
                }
            }
        }

        #endregion

        #region Legacy Map Display (Backward Compatibility)

        /// <summary>
        /// 기존 맵 새로고침 (하위 호환용)
        /// Refresh legacy map display (backward compatibility)
        /// </summary>
        public void RefreshMap()
        {
            ClearAll();

            if (MapManager.Instance?.CurrentMap == null) return;

            var mapData = MapManager.Instance.CurrentMap;

            // 맵 타입 라벨 업데이트
            UpdateMapTypeLabel(mapData.MapType);

            // 마을 패널 표시/숨김
            UpdateTownPanel(mapData.MapType);

            // 마을인 경우 다른 레이아웃 사용
            Vector2 currentOffset = mapData.MapType == MapType.Town ? townOffset : offset;

            // 노드 생성
            foreach (var node in mapData.GetAllNodes())
            {
                CreateNodeButton(node, currentOffset);
            }

            // 경로 생성 (마을은 경로 없음)
            if (mapData.MapType != MapType.Town)
            {
                CreatePaths(mapData, currentOffset);
            }

            // 접근 가능한 노드 업데이트
            UpdateAccessibleNodes();
        }

        private void UpdateMapTypeLabel(MapType mapType)
        {
            if (mapTypeLabel == null) return;

            mapTypeLabel.text = mapType switch
            {
                MapType.Town => "마을 (Town)",
                MapType.Field => "필드 (Field)",
                MapType.Dungeon => "던전 (Dungeon)",
                _ => ""
            };
        }

        private void UpdateTownPanel(MapType mapType)
        {
            if (townPanel != null)
            {
                townPanel.SetActive(mapType == MapType.Town);
            }
        }

        /// <summary>
        /// 필드 진입 버튼 클릭
        /// Enter field button clicked
        /// </summary>
        private void OnEnterFieldClicked()
        {
            if (MapManager.Instance == null || GameManager.Instance == null) return;

            // 월드맵 모드면 월드맵 생성
            if (useWorldMap)
            {
                int seed = System.Environment.TickCount;
                MapManager.Instance.GenerateWorldMap(seed);
            }
            else
            {
                int seed = System.Environment.TickCount;
                MapManager.Instance.GenerateNewMap(seed, MapType.Field);
            }

            Debug.Log("[MapUI] Entering Field...");
        }

        private void ClearAll()
        {
            // 노드 정리
            foreach (var obj in nodeObjects.Values)
            {
                if (obj != null) Destroy(obj);
            }
            nodeObjects.Clear();

            // 경로 정리
            foreach (var obj in pathObjects)
            {
                if (obj != null) Destroy(obj);
            }
            pathObjects.Clear();

            // 지역 배경 정리
            foreach (var obj in regionBackgrounds.Values)
            {
                if (obj != null) Destroy(obj);
            }
            regionBackgrounds.Clear();

            // 지역 라벨 정리
            foreach (var obj in regionLabels.Values)
            {
                if (obj != null) Destroy(obj);
            }
            regionLabels.Clear();
        }

        private void CreateNodeButton(MapNode node, Vector2 currentOffset)
        {
            if (nodeButtonPrefab == null || nodeContainer == null) return;

            var nodeObj = Instantiate(nodeButtonPrefab, nodeContainer);
            var rectTransform = nodeObj.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                Vector2 screenPos = node.Position * nodeScale + currentOffset;
                rectTransform.anchoredPosition = screenPos;
            }

            // 버튼 설정
            var button = nodeObj.GetComponent<Button>();
            if (button != null)
            {
                string nodeId = node.NodeId;
                button.onClick.AddListener(() => OnNodeClicked(nodeId));
            }

            // 색상 및 텍스트 설정
            var image = nodeObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = node.GetNodeColor();
            }

            var text = nodeObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = node.GetNodeSymbol();
            }

            // 방문한 노드 표시
            if (node.IsVisited)
            {
                if (image != null)
                {
                    var color = image.color;
                    color.a = 0.5f;
                    image.color = color;
                }
            }

            nodeObjects[node.NodeId] = nodeObj;
        }

        private void CreatePaths(MapData mapData, Vector2 currentOffset)
        {
            if (pathLinePrefab == null || pathContainer == null) return;

            foreach (var node in mapData.GetAllNodes())
            {
                foreach (var connectedId in node.ConnectedNodeIds)
                {
                    var connectedNode = mapData.GetNode(connectedId);
                    if (connectedNode == null) continue;

                    var pathObj = Instantiate(pathLinePrefab, pathContainer);
                    var line = pathObj.GetComponent<LineRenderer>();

                    if (line != null)
                    {
                        Vector3 start = new Vector3(
                            node.Position.x * nodeScale + currentOffset.x,
                            node.Position.y * nodeScale + currentOffset.y,
                            0);
                        Vector3 end = new Vector3(
                            connectedNode.Position.x * nodeScale + currentOffset.x,
                            connectedNode.Position.y * nodeScale + currentOffset.y,
                            0);

                        line.SetPosition(0, start);
                        line.SetPosition(1, end);
                    }

                    // UI Image로 선 그리기 (대안)
                    var rectTransform = pathObj.GetComponent<RectTransform>();
                    if (rectTransform != null && line == null)
                    {
                        DrawLineUI(rectTransform, node.Position, connectedNode.Position, currentOffset);
                    }

                    pathObjects.Add(pathObj);
                }
            }
        }

        private void DrawLineUI(RectTransform rect, Vector2 from, Vector2 to, Vector2 currentOffset)
        {
            Vector2 startPos = from * nodeScale + currentOffset;
            Vector2 endPos = to * nodeScale + currentOffset;

            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            rect.anchoredPosition = startPos;
            rect.sizeDelta = new Vector2(distance, 2f);
            rect.rotation = Quaternion.Euler(0, 0, angle);
            rect.pivot = new Vector2(0, 0.5f);
        }

        private void UpdateAccessibleNodes()
        {
            if (MapManager.Instance?.CurrentMap == null) return;

            foreach (var kvp in nodeObjects)
            {
                var node = MapManager.Instance.CurrentMap.GetNode(kvp.Key);
                var button = kvp.Value.GetComponent<Button>();

                if (button != null && node != null)
                {
                    button.interactable = node.IsAccessible;
                }
            }
        }

        private void OnNodeClicked(string nodeId)
        {
            if (MapManager.Instance.SelectNode(nodeId))
            {
                var node = MapManager.Instance.CurrentNode;
                var mapType = MapManager.Instance.CurrentMapType;

                // RunManager에 알림
                if (RunManager.Instance != null)
                {
                    RunManager.Instance.OnNodeSelected(node);
                }

                // 노드 타입에 따라 씬 전환
                if (GameManager.Instance != null)
                {
                    switch (node.NodeType)
                    {
                        case MapNodeType.Combat:
                        case MapNodeType.Elite:
                            GameManager.Instance.LoadCombat();
                            break;
                        case MapNodeType.Boss:
                            GameManager.Instance.LoadCombat();
                            break;
                        case MapNodeType.Event:
                            GameManager.Instance.LoadEvent();
                            break;
                        case MapNodeType.Shop:
                            GameManager.Instance.LoadShop();
                            break;
                        case MapNodeType.Rest:
                            GameManager.Instance.LoadRest();
                            break;
                        case MapNodeType.Treasure:
                            GameManager.Instance.LoadReward();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 필드 완료 → 던전 진입
        /// Field complete → Enter dungeon
        /// </summary>
        public void OnFieldComplete()
        {
            if (MapManager.Instance == null) return;

            int seed = System.Environment.TickCount;
            MapManager.Instance.GenerateNewMap(seed, MapType.Dungeon);

            Debug.Log("[MapUI] Field completed. Entering Dungeon...");
        }

        /// <summary>
        /// 던전 완료 → 마을 복귀
        /// Dungeon complete → Return to town
        /// </summary>
        public void OnDungeonComplete()
        {
            if (MapManager.Instance == null) return;

            int seed = System.Environment.TickCount;
            MapManager.Instance.GenerateNewMap(seed, MapType.Town);

            Debug.Log("[MapUI] Dungeon completed. Returning to Town...");
        }

        #endregion
    }
}
