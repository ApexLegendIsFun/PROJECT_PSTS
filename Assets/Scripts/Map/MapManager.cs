using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Utility;

namespace ProjectSS.Map
{
    /// <summary>
    /// 맵 관리자
    /// Map manager
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [SerializeField] private MapGenerationConfig generationConfig;

        private MapData currentMap;
        private MapNode currentNode;
        private int currentFloor;
        private SeededRandom random;

        public MapData CurrentMap => currentMap;
        public MapNode CurrentNode => currentNode;
        public int CurrentFloor => currentFloor;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 새 맵 생성
        /// Generate new map
        /// </summary>
        public void GenerateNewMap(int seed)
        {
            random = new SeededRandom(seed);
            var generator = new MapGenerator(random, generationConfig);
            currentMap = generator.GenerateMap();
            currentFloor = -1;
            currentNode = null;
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
    }
}
