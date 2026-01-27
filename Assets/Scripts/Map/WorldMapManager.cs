// Map/WorldMapManager.cs
// 월드맵 관리자 - 노드 기반 여행 맵 시스템

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Events;
using ProjectSS.Map.Data;

namespace ProjectSS.Map
{
    /// <summary>
    /// 월드맵 관리자
    /// 노드-경로 기반 여행 맵 시스템 관리
    /// </summary>
    public class WorldMapManager : MonoBehaviour
    {
        public static WorldMapManager Instance { get; private set; }

        [Header("Map Data")]
        [SerializeField] private WorldMapData _currentMapData;

        [Header("Debug")]
        [SerializeField] private bool _debugMode = false;

        // 런타임 상태
        private WorldMapProgress _progress;
        private string _currentNodeId;

        #region Properties

        /// <summary>
        /// 현재 맵 데이터
        /// </summary>
        public WorldMapData CurrentMapData => _currentMapData;

        /// <summary>
        /// 현재 진행 상태
        /// </summary>
        public WorldMapProgress Progress => _progress;

        /// <summary>
        /// 현재 노드 ID
        /// </summary>
        public string CurrentNodeId => _currentNodeId;

        /// <summary>
        /// 현재 노드
        /// </summary>
        public WorldMapNode CurrentNode => _currentMapData?.GetNode(_currentNodeId);

        /// <summary>
        /// 맵 로드 여부
        /// </summary>
        public bool IsMapLoaded => _currentMapData != null && _progress != null;

        /// <summary>
        /// 맵 완료 여부
        /// </summary>
        public bool IsMapCompleted => _progress?.IsMapCompleted(_currentMapData?.EndNodeId) ?? false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ServiceLocator.Register(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<WorldMapManager>();
                Instance = null;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        #endregion

        #region Map Loading

        /// <summary>
        /// 새 맵 로드
        /// </summary>
        public void LoadMap(WorldMapData mapData)
        {
            if (mapData == null)
            {
                Debug.LogError("[WorldMapManager] Cannot load null map data!");
                return;
            }

            // 유효성 검증
            if (!mapData.Validate(out var errors))
            {
                foreach (var error in errors)
                    Debug.LogError($"[WorldMapManager] Map validation error: {error}");
                return;
            }

            _currentMapData = mapData;
            _currentMapData.ResetAllRuntimeStates();

            // 시작 노드 설정
            var startNode = _currentMapData.GetStartNode();
            _currentNodeId = startNode?.NodeId;

            // 진행 상태 초기화
            _progress = WorldMapProgress.CreateNew(mapData.MapId, _currentNodeId);

            // 시작 노드 방문 처리
            if (startNode != null)
            {
                startNode.Visit();
                startNode.IsAccessible = true;

                // 시작 노드가 Start 타입이면 자동 클리어
                if (startNode.RegionType == RegionType.Start)
                {
                    startNode.Clear();
                    _progress.RecordClear(_currentNodeId);
                    UpdateAccessibleNodes();
                }
            }

            Log($"Map loaded: {mapData.MapName} ({mapData.Nodes.Count} nodes)");

            // 이벤트 발행
            EventBus.Publish(new WorldMapLoadedEvent
            {
                MapId = mapData.MapId,
                MapName = mapData.MapName,
                TotalNodes = mapData.Nodes.Count
            });
        }

        /// <summary>
        /// 저장된 진행 상태로 맵 로드
        /// </summary>
        public void LoadMapWithProgress(WorldMapData mapData, WorldMapProgress progress)
        {
            if (mapData == null || progress == null)
            {
                Debug.LogError("[WorldMapManager] Cannot load map with null data or progress!");
                return;
            }

            _currentMapData = mapData;
            _progress = progress;
            _currentNodeId = progress.CurrentNodeId;

            // 진행 상태 적용
            _currentMapData.ApplyProgress(progress);

            Log($"Map loaded with progress: {mapData.MapName}");

            EventBus.Publish(new WorldMapLoadedEvent
            {
                MapId = mapData.MapId,
                MapName = mapData.MapName,
                TotalNodes = mapData.Nodes.Count
            });
        }

        #endregion

        #region Node Movement

        /// <summary>
        /// 특정 노드로 이동 가능 여부 확인
        /// </summary>
        public bool CanMoveTo(string nodeId)
        {
            if (!IsMapLoaded) return false;

            var targetNode = _currentMapData.GetNode(nodeId);
            if (targetNode == null) return false;

            // 이미 현재 위치면 불가
            if (nodeId == _currentNodeId) return false;

            // 접근 가능 상태 확인
            if (!targetNode.IsAccessible) return false;

            // 이미 방문한 노드는 재방문 불가 (선택적)
            if (targetNode.IsVisited && !targetNode.IsCleared) return false;

            // 현재 노드가 클리어되어야 이동 가능
            var currentNode = CurrentNode;
            if (currentNode != null && !currentNode.IsCleared)
            {
                // 시작 노드는 예외
                if (currentNode.RegionType != RegionType.Start)
                    return false;
            }

            // 연결 확인
            return _currentMapData.AreNodesConnected(_currentNodeId, nodeId);
        }

        /// <summary>
        /// 특정 노드로 이동
        /// </summary>
        public bool MoveToNode(string nodeId)
        {
            if (!CanMoveTo(nodeId))
            {
                Log($"Cannot move to node: {nodeId}");
                return false;
            }

            var previousNodeId = _currentNodeId;
            var targetNode = _currentMapData.GetNode(nodeId);

            // 이동 실행
            _currentNodeId = nodeId;
            targetNode.Visit();

            // 진행 상태 업데이트
            _progress.SetCurrentNode(nodeId);
            _progress.RecordPath(previousNodeId, nodeId);

            Log($"Moved to node: {targetNode.DisplayName} ({targetNode.RegionType})");

            // 노드 진입 이벤트
            EventBus.Publish(new MapNodeEnteredEvent
            {
                NodeId = nodeId,
                FromNodeId = previousNodeId,
                RegionType = targetNode.RegionType
            });

            // 노드 타입별 처리
            ProcessNodeEntry(targetNode);

            return true;
        }

        /// <summary>
        /// 접근 가능한 노드 목록 조회
        /// </summary>
        public List<WorldMapNode> GetAccessibleNodes()
        {
            if (!IsMapLoaded) return new List<WorldMapNode>();

            var currentNode = CurrentNode;
            if (currentNode == null) return new List<WorldMapNode>();

            // 현재 노드가 클리어되지 않았으면 빈 목록
            if (!currentNode.IsCleared && currentNode.RegionType != RegionType.Start)
                return new List<WorldMapNode>();

            // 연결된 노드 중 접근 가능한 것들
            return _currentMapData.GetConnectedNodes(_currentNodeId)
                .Where(n => n.IsAccessible && !n.IsVisited)
                .ToList();
        }

        #endregion

        #region Node Processing

        /// <summary>
        /// 노드 진입 처리
        /// </summary>
        private void ProcessNodeEntry(WorldMapNode node)
        {
            switch (node.RegionType)
            {
                case RegionType.Start:
                    // 시작 노드는 자동 클리어
                    CompleteCurrentNode();
                    break;

                case RegionType.Shelter:
                    // 쉼터 - Rest 상태로 전환
                    Log("Entering shelter - switching to Rest state");
                    GameManager.Instance?.SetState(GameState.Rest);
                    break;

                case RegionType.Dungeon:
                case RegionType.Boss:
                    // 던전/보스 - 전투 시작
                    Log($"Entering {node.RegionType} - starting combat");
                    StartCombat(node);
                    break;

                case RegionType.End:
                    // 종료 노드
                    CompleteCurrentNode();
                    OnMapCompleted();
                    break;
            }
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        private void StartCombat(WorldMapNode node)
        {
            TileType encounterType = node.RegionType switch
            {
                RegionType.Boss => TileType.Boss,
                _ => TileType.Enemy
            };

            EventBus.Publish(new CombatStartedEvent
            {
                EncounterType = encounterType
            });

            GameManager.Instance?.GoToCombat();
        }

        /// <summary>
        /// 전투 종료 이벤트 처리
        /// </summary>
        private void OnCombatEnded(CombatEndedEvent evt)
        {
            if (!evt.Victory)
            {
                Log("Combat lost - ending run");
                GameManager.Instance?.EndRun(false);
                return;
            }

            Log("Combat won - completing node");
            CompleteCurrentNode();

            // 맵으로 복귀
            GameManager.Instance?.GoToMap();
        }

        /// <summary>
        /// 현재 노드 클리어 처리
        /// </summary>
        public void CompleteCurrentNode()
        {
            var currentNode = CurrentNode;
            if (currentNode == null || currentNode.IsCleared) return;

            currentNode.Clear();
            _progress.RecordClear(_currentNodeId);

            Log($"Node cleared: {currentNode.DisplayName}");

            // 클리어 이벤트
            EventBus.Publish(new MapNodeClearedEvent
            {
                NodeId = _currentNodeId,
                RegionType = currentNode.RegionType
            });

            // 연결된 노드들 접근 가능 설정
            UpdateAccessibleNodes();

            // 보스 클리어 시 맵 완료 체크
            if (currentNode.RegionType == RegionType.Boss || _currentNodeId == _currentMapData.EndNodeId)
            {
                OnMapCompleted();
            }
        }

        /// <summary>
        /// 쉼터 완료 처리 (외부 호출용)
        /// </summary>
        public void CompleteShelter()
        {
            var currentNode = CurrentNode;
            if (currentNode?.RegionType != RegionType.Shelter) return;

            CompleteCurrentNode();

            // 맵으로 복귀
            GameManager.Instance?.GoToMap();
        }

        /// <summary>
        /// 접근 가능 노드 업데이트
        /// </summary>
        private void UpdateAccessibleNodes()
        {
            if (!IsMapLoaded) return;

            // 현재 노드의 연결 노드들 접근 가능 설정
            foreach (var connectedNode in _currentMapData.GetConnectedNodes(_currentNodeId))
            {
                connectedNode.IsAccessible = true;
            }
        }

        /// <summary>
        /// 맵 완료 처리
        /// </summary>
        private void OnMapCompleted()
        {
            if (IsMapCompleted)
            {
                Log($"Map completed: {_currentMapData.MapName}");

                EventBus.Publish(new WorldMapCompletedEvent
                {
                    MapId = _currentMapData.MapId,
                    ClearedNodes = _progress.ClearedNodeIds.Count,
                    TotalNodes = _currentMapData.Nodes.Count
                });
            }
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// 현재 진행 상태 저장
        /// </summary>
        public string SaveProgress()
        {
            if (_progress == null) return null;
            return _progress.ToJson();
        }

        /// <summary>
        /// 진행 상태 로드
        /// </summary>
        public void LoadProgress(string json)
        {
            var progress = WorldMapProgress.FromJson(json);
            if (progress != null && _currentMapData != null)
            {
                _progress = progress;
                _currentNodeId = progress.CurrentNodeId;
                _currentMapData.ApplyProgress(progress);
            }
        }

        #endregion

        #region Debug

        private void Log(string message)
        {
            if (_debugMode)
                Debug.Log($"[WorldMapManager] {message}");
        }

        /// <summary>
        /// 디버그용: 모든 노드 클리어
        /// </summary>
        [ContextMenu("Debug: Clear All Nodes")]
        public void DebugClearAllNodes()
        {
            if (!IsMapLoaded) return;

            foreach (var node in _currentMapData.Nodes)
            {
                node.Clear();
                node.IsAccessible = true;
                _progress.RecordClear(node.NodeId);
            }

            Debug.Log("[WorldMapManager] Debug: All nodes cleared");
        }

        #endregion
    }
}
