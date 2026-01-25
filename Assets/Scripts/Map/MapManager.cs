using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Utility;
using MapType = ProjectSS.Core.MapType;

namespace ProjectSS.Map
{
    /// <summary>
    /// 맵 관리자 (마을/필드/던전 허브 시스템 + 통합 월드맵)
    /// Map manager (Town/Field/Dungeon hub system + unified world map)
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [Header("Map Configs")]
        [SerializeField] private MapGenerationConfig generationConfig;
        [SerializeField] private MapGenerationConfig townConfig;
        [SerializeField] private MapGenerationConfig fieldConfig;
        [SerializeField] private MapGenerationConfig dungeonConfig;

        [Header("World Map Config")]
        [SerializeField] private WorldMapLayoutData worldMapLayout;

        [Header("Region Integration")]
        [SerializeField] private RegionManager regionManager;

        // 기존 단일 맵 데이터 (하위 호환용)
        private MapData currentMap;
        private MapNode currentNode;
        private int currentFloor;
        private SeededRandom random;

        // 통합 월드맵 데이터
        private WorldMapData worldMap;
        private WorldMapGenerator worldMapGenerator;

        // 기존 프로퍼티
        public MapData CurrentMap => currentMap;
        public MapNode CurrentNode => currentNode;
        public int CurrentFloor => currentFloor;
        public MapType CurrentMapType => currentMap?.MapType ?? MapType.Town;

        // 통합 월드맵 프로퍼티
        public WorldMapData WorldMap => worldMap;
        public bool IsWorldMapActive => worldMap != null;

        /// <summary>
        /// 현재 지역
        /// Current region
        /// </summary>
        public RegionData CurrentRegion => regionManager?.CurrentRegion;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            // 맵 생성 요청 이벤트 구독
            // Subscribe to map generation request event
            EventBus.Subscribe<MapGenerationRequestedEvent>(OnMapGenerationRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MapGenerationRequestedEvent>(OnMapGenerationRequested);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 맵 생성 요청 이벤트 핸들러
        /// Map generation requested event handler
        /// </summary>
        private void OnMapGenerationRequested(MapGenerationRequestedEvent evt)
        {
            GenerateNewMap(evt.Seed, evt.MapType);
        }

        /// <summary>
        /// 새 맵 생성 (기본: generationConfig의 맵 타입)
        /// Generate new map (default: generationConfig's map type)
        /// </summary>
        public void GenerateNewMap(int seed)
        {
            MapType mapType = generationConfig?.mapType ?? MapType.Field;
            GenerateNewMap(seed, mapType);
        }

        /// <summary>
        /// 맵 타입 지정하여 새 맵 생성
        /// Generate new map with specified map type
        /// </summary>
        public void GenerateNewMap(int seed, MapType mapType)
        {
            random = new SeededRandom(seed);

            // 맵 타입에 맞는 Config 선택
            MapGenerationConfig config = GetConfigForMapType(mapType);
            if (config != null)
            {
                config.mapType = mapType;
            }

            var generator = new MapGenerator(random, config);

            // 지역의 레이아웃이 있으면 사용
            // Use region's layout if available
            var region = regionManager?.CurrentRegion;
            if (region != null && region.mapLayout != null)
            {
                currentMap = generator.LoadFromLayout(region.mapLayout);
            }
            else
            {
                currentMap = generator.GenerateMap(mapType);
            }

            currentFloor = -1;
            currentNode = null;

            // 첫 번째 층 노드들 접근 가능 설정
            // Set first floor nodes as accessible
            SetInitialAccessibleNodes();

            // 이벤트 발행
            // Publish event
            EventBus.Publish(new MapGeneratedEvent(seed, currentMap?.FloorCount ?? 0, mapType));

            Debug.Log($"[MapManager] Generated {mapType} map with {currentMap?.FloorCount ?? 0} floors");
        }

        /// <summary>
        /// 지역 기반 맵 생성
        /// Generate map from region
        /// </summary>
        public void GenerateMapFromRegion(int seed, RegionData region)
        {
            if (region == null)
            {
                Debug.LogError("[MapManager] Cannot generate map: region is null");
                return;
            }

            random = new SeededRandom(seed);

            var config = GetConfigForMapType(region.mapType);
            var generator = new MapGenerator(random, config);

            if (region.mapLayout != null)
            {
                currentMap = generator.LoadFromLayout(region.mapLayout);
            }
            else
            {
                currentMap = generator.GenerateMap(region.mapType);
            }

            currentFloor = -1;
            currentNode = null;

            SetInitialAccessibleNodes();

            EventBus.Publish(new MapGeneratedEvent(seed, currentMap?.FloorCount ?? 0, region.mapType));

            Debug.Log($"[MapManager] Generated map from region '{region.regionName}' with {currentMap?.FloorCount ?? 0} floors");
        }

        /// <summary>
        /// 맵 타입에 맞는 Config 반환
        /// Get config for map type
        /// </summary>
        private MapGenerationConfig GetConfigForMapType(MapType mapType)
        {
            return mapType switch
            {
                MapType.Town => townConfig ?? generationConfig,
                MapType.Field => fieldConfig ?? generationConfig,
                MapType.Dungeon => dungeonConfig ?? generationConfig,
                _ => generationConfig
            };
        }

        /// <summary>
        /// 마을 맵 생성 (허브)
        /// Generate town map (hub)
        /// </summary>
        public void GenerateTownMap(int seed)
        {
            GenerateNewMap(seed, MapType.Town);
        }

        /// <summary>
        /// 필드 맵 생성
        /// Generate field map
        /// </summary>
        public void GenerateFieldMap(int seed)
        {
            GenerateNewMap(seed, MapType.Field);
        }

        /// <summary>
        /// 던전 맵 생성
        /// Generate dungeon map
        /// </summary>
        public void GenerateDungeonMap(int seed)
        {
            GenerateNewMap(seed, MapType.Dungeon);
        }

        /// <summary>
        /// 초기 접근 가능 노드 설정 (첫 번째 층)
        /// Set initial accessible nodes (first floor)
        /// </summary>
        private void SetInitialAccessibleNodes()
        {
            if (currentMap == null) return;

            foreach (var node in currentMap.GetAllNodes())
            {
                node.IsAccessible = (node.Floor == 0);
            }
        }

        /// <summary>
        /// 저장된 맵 로드
        /// Load saved map
        /// </summary>
        public void LoadMap(MapData map, int floor, string currentNodeId)
        {
            currentMap = map;
            currentFloor = floor;
            currentNode = map.GetNode(currentNodeId);
        }

        /// <summary>
        /// 노드 선택
        /// Select node
        /// </summary>
        public bool SelectNode(string nodeId)
        {
            var node = currentMap.GetNode(nodeId);
            if (node == null || !node.IsAccessible)
                return false;

            // 현재 노드 방문 처리
            if (currentNode != null)
            {
                currentNode.Visit();
            }

            currentNode = node;
            currentFloor = node.Floor;

            // 다음 층 노드들 접근 가능하게 설정
            UpdateAccessibleNodes();

            // 이벤트 발행
            EventBus.Publish(new MapNodeSelectedEvent(nodeId, node.Floor));

            return true;
        }

        /// <summary>
        /// 접근 가능한 노드 업데이트
        /// Update accessible nodes
        /// </summary>
        private void UpdateAccessibleNodes()
        {
            if (currentNode == null) return;

            // 모든 노드 접근 불가로 초기화
            foreach (var node in currentMap.GetAllNodes())
            {
                node.IsAccessible = false;
            }

            // 현재 노드에서 연결된 노드들만 접근 가능
            foreach (var connectedId in currentNode.ConnectedNodeIds)
            {
                var connectedNode = currentMap.GetNode(connectedId);
                if (connectedNode != null)
                {
                    connectedNode.IsAccessible = true;
                }
            }
        }

        /// <summary>
        /// 현재 노드 완료 처리
        /// Complete current node
        /// </summary>
        public void CompleteCurrentNode()
        {
            if (currentNode != null)
            {
                currentNode.Visit();
            }
        }

        /// <summary>
        /// 보스 노드인지 확인
        /// Check if current node is boss
        /// </summary>
        public bool IsBossNode()
        {
            return currentNode != null && currentNode.NodeType == MapNodeType.Boss;
        }

        /// <summary>
        /// 접근 가능한 노드 목록
        /// Get accessible nodes
        /// </summary>
        public System.Collections.Generic.List<MapNode> GetAccessibleNodes()
        {
            var accessible = new System.Collections.Generic.List<MapNode>();

            if (currentMap == null) return accessible;

            foreach (var node in currentMap.GetAllNodes())
            {
                if (node.IsAccessible)
                {
                    accessible.Add(node);
                }
            }

            return accessible;
        }

        #region World Map Methods

        /// <summary>
        /// 통합 월드맵 생성
        /// Generate unified world map
        /// </summary>
        public void GenerateWorldMap(int seed)
        {
            random = new SeededRandom(seed);
            worldMapGenerator = new WorldMapGenerator(random);

            if (worldMapLayout != null)
            {
                worldMap = worldMapGenerator.GenerateWorldMap(worldMapLayout);
            }
            else
            {
                worldMap = worldMapGenerator.GenerateDefaultWorldMap();
            }

            currentNode = null;
            currentFloor = -1;

            // 기존 맵 초기화 (호환성)
            currentMap = null;

            // 이벤트 발행
            EventBus.Publish(new WorldMapGeneratedEvent(
                seed,
                worldMap.TotalNodeCount,
                worldMap.RegionCount
            ));

            Debug.Log($"[MapManager] Generated world map with {worldMap.TotalNodeCount} nodes across {worldMap.RegionCount} regions");
        }

        /// <summary>
        /// 월드맵 노드 선택
        /// Select world map node
        /// </summary>
        public bool SelectWorldMapNode(string nodeId)
        {
            if (worldMap == null)
            {
                Debug.LogWarning("[MapManager] World map is not generated");
                return false;
            }

            var node = worldMap.GetNode(nodeId);
            if (node == null)
            {
                Debug.LogWarning($"[MapManager] Node not found: {nodeId}");
                return false;
            }

            if (!node.IsAccessible)
            {
                Debug.LogWarning($"[MapManager] Node not accessible: {nodeId}");
                return false;
            }

            // 현재 노드 방문 처리
            if (currentNode != null)
            {
                currentNode.Visit();
            }

            currentNode = node;
            currentFloor = node.Floor;

            // 접근 가능한 노드 업데이트
            UpdateWorldMapAccessibleNodes();

            // 이벤트 발행
            EventBus.Publish(new WorldMapNodeSelectedEvent(
                nodeId,
                node.RegionId,
                node.Floor,
                node.NodeType
            ));

            Debug.Log($"[MapManager] Selected world map node: {nodeId} (Region: {node.RegionId}, Type: {node.NodeType})");

            return true;
        }

        /// <summary>
        /// 월드맵 접근 가능 노드 업데이트
        /// Update world map accessible nodes
        /// </summary>
        private void UpdateWorldMapAccessibleNodes()
        {
            if (worldMap == null) return;

            // 모든 노드 접근 불가로 초기화
            worldMap.ResetAllAccessibility();

            if (currentNode == null)
            {
                // 시작 노드들만 접근 가능
                worldMap.SetInitialAccessibility();
            }
            else
            {
                // 현재 노드에서 연결된 노드들만 접근 가능
                foreach (var connectedId in currentNode.ConnectedNodeIds)
                {
                    var connectedNode = worldMap.GetNode(connectedId);
                    if (connectedNode != null)
                    {
                        connectedNode.IsAccessible = true;
                    }
                }
            }
        }

        /// <summary>
        /// 월드맵 접근 가능 노드 목록
        /// Get world map accessible nodes
        /// </summary>
        public System.Collections.Generic.List<MapNode> GetWorldMapAccessibleNodes()
        {
            var accessible = new System.Collections.Generic.List<MapNode>();

            if (worldMap == null) return accessible;

            foreach (var node in worldMap.GetAllNodes())
            {
                if (node.IsAccessible)
                {
                    accessible.Add(node);
                }
            }

            return accessible;
        }

        /// <summary>
        /// 현재 노드가 보스인지 확인 (월드맵용)
        /// Check if current node is boss (for world map)
        /// </summary>
        public bool IsWorldMapBossNode()
        {
            return currentNode != null && currentNode.NodeType == MapNodeType.Boss;
        }

        /// <summary>
        /// 현재 노드의 지역 정보
        /// Get current node's region info
        /// </summary>
        public RegionMapSection GetCurrentRegionSection()
        {
            if (worldMap == null || currentNode == null) return null;
            return worldMap.GetRegion(currentNode.RegionId);
        }

        #endregion
    }
}
