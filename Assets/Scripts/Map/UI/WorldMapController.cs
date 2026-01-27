// Map/UI/WorldMapController.cs
// 월드맵 UI 컨트롤러

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core;
using ProjectSS.Core.Events;
using ProjectSS.Map.Data;

namespace ProjectSS.Map.UI
{
    /// <summary>
    /// 월드맵 UI 컨트롤러
    /// 전체 맵 렌더링 및 인터랙션 관리
    /// </summary>
    public class WorldMapController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject _nodePrefab;
        [SerializeField] private GameObject _pathPrefab;

        [Header("Containers")]
        [SerializeField] private RectTransform _nodesContainer;
        [SerializeField] private RectTransform _pathsContainer;

        [Header("Configuration")]
        [SerializeField] private WorldMapVisualConfig _visualConfig;

        [Header("Scroll Settings")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private float _scrollPadding = 100f;

        // 캐시
        private Dictionary<string, WorldMapNodeUI> _nodeUIs = new Dictionary<string, WorldMapNodeUI>();
        private List<WorldMapPathUI> _pathUIs = new List<WorldMapPathUI>();
        private WorldMapData _currentMapData;

        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.Subscribe<WorldMapLoadedEvent>(OnMapLoaded);
            EventBus.Subscribe<MapNodeEnteredEvent>(OnNodeEntered);
            EventBus.Subscribe<MapNodeClearedEvent>(OnNodeCleared);
            EventBus.Subscribe<MapNodeSelectedEvent>(OnNodeSelected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WorldMapLoadedEvent>(OnMapLoaded);
            EventBus.Unsubscribe<MapNodeEnteredEvent>(OnNodeEntered);
            EventBus.Unsubscribe<MapNodeClearedEvent>(OnNodeCleared);
            EventBus.Unsubscribe<MapNodeSelectedEvent>(OnNodeSelected);
        }

        #endregion

        #region Event Handlers

        private void OnMapLoaded(WorldMapLoadedEvent evt)
        {
            var manager = WorldMapManager.Instance;
            if (manager?.CurrentMapData == null) return;

            RenderMap(manager.CurrentMapData);
        }

        private void OnNodeEntered(MapNodeEnteredEvent evt)
        {
            // 이전 노드 플레이어 마커 제거
            if (!string.IsNullOrEmpty(evt.FromNodeId) && _nodeUIs.TryGetValue(evt.FromNodeId, out var prevUI))
            {
                prevUI.SetPlayerHere(false);
            }

            // 현재 노드 플레이어 마커 표시
            if (_nodeUIs.TryGetValue(evt.NodeId, out var currentUI))
            {
                currentUI.SetPlayerHere(true);
            }

            // 경로 하이라이트
            UpdateTraversedPaths();

            // 접근 가능 노드 업데이트
            UpdateAccessibleNodes();

            // 현재 노드로 스크롤
            ScrollToNode(evt.NodeId);
        }

        private void OnNodeCleared(MapNodeClearedEvent evt)
        {
            if (_nodeUIs.TryGetValue(evt.NodeId, out var nodeUI))
            {
                nodeUI.SetCleared(true);
            }

            UpdateAccessibleNodes();
        }

        private void OnNodeSelected(MapNodeSelectedEvent evt)
        {
            var manager = WorldMapManager.Instance;
            if (manager == null) return;

            // 이동 가능한지 확인하고 이동
            if (manager.CanMoveTo(evt.NodeId))
            {
                manager.MoveToNode(evt.NodeId);
            }
            else
            {
                Debug.Log($"[WorldMapController] Cannot move to node: {evt.NodeId}");
            }
        }

        #endregion

        #region Map Rendering

        /// <summary>
        /// 맵 렌더링
        /// </summary>
        public void RenderMap(WorldMapData mapData)
        {
            if (mapData == null) return;

            _currentMapData = mapData;

            ClearUI();
            RenderPaths(mapData);
            RenderNodes(mapData);
            UpdateAccessibleNodes();

            // 현재 위치로 스크롤
            var manager = WorldMapManager.Instance;
            if (manager != null)
            {
                ScrollToNode(manager.CurrentNodeId);

                // 현재 노드 마커 표시
                if (_nodeUIs.TryGetValue(manager.CurrentNodeId, out var currentUI))
                {
                    currentUI.SetPlayerHere(true);
                }
            }

            Debug.Log($"[WorldMapController] Rendered map: {mapData.Nodes.Count} nodes");
        }

        /// <summary>
        /// 노드 렌더링
        /// </summary>
        private void RenderNodes(WorldMapData mapData)
        {
            if (_nodePrefab == null)
            {
                Debug.LogError("[WorldMapController] Node prefab is not assigned!");
                return;
            }

            foreach (var node in mapData.Nodes)
            {
                var go = Instantiate(_nodePrefab, _nodesContainer);
                var nodeUI = go.GetComponent<WorldMapNodeUI>();

                if (nodeUI != null)
                {
                    nodeUI.Initialize(node, _visualConfig);
                    _nodeUIs[node.NodeId] = nodeUI;

                    // 위치 설정
                    var rectTransform = go.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.anchoredPosition = node.Position;
                    }
                }
            }
        }

        /// <summary>
        /// 경로 렌더링
        /// </summary>
        private void RenderPaths(WorldMapData mapData)
        {
            if (_pathPrefab == null)
            {
                Debug.LogWarning("[WorldMapController] Path prefab is not assigned, skipping path rendering");
                return;
            }

            // 연결 정보를 기반으로 경로 생성
            var renderedPaths = new HashSet<string>();

            foreach (var node in mapData.Nodes)
            {
                if (node.ConnectedNodeIds == null) continue;

                foreach (var connectedId in node.ConnectedNodeIds)
                {
                    // 중복 방지 (A->B, B->A)
                    var pathKey = GetPathKey(node.NodeId, connectedId);
                    if (renderedPaths.Contains(pathKey)) continue;
                    renderedPaths.Add(pathKey);

                    var connectedNode = mapData.GetNode(connectedId);
                    if (connectedNode == null) continue;

                    var go = Instantiate(_pathPrefab, _pathsContainer);
                    var pathUI = go.GetComponent<WorldMapPathUI>();

                    if (pathUI != null)
                    {
                        pathUI.Initialize(node.NodeId, connectedId, node.Position, connectedNode.Position, _visualConfig);
                        _pathUIs.Add(pathUI);
                    }
                }
            }
        }

        private string GetPathKey(string nodeA, string nodeB)
        {
            return string.Compare(nodeA, nodeB) < 0 ? $"{nodeA}_{nodeB}" : $"{nodeB}_{nodeA}";
        }

        /// <summary>
        /// UI 클리어
        /// </summary>
        private void ClearUI()
        {
            foreach (var kvp in _nodeUIs)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            _nodeUIs.Clear();

            foreach (var pathUI in _pathUIs)
            {
                if (pathUI != null)
                    Destroy(pathUI.gameObject);
            }
            _pathUIs.Clear();
        }

        #endregion

        #region UI Updates

        /// <summary>
        /// 접근 가능 노드 업데이트
        /// </summary>
        private void UpdateAccessibleNodes()
        {
            var manager = WorldMapManager.Instance;
            if (manager == null) return;

            var accessibleNodes = manager.GetAccessibleNodes();
            var accessibleIds = new HashSet<string>(accessibleNodes.ConvertAll(n => n.NodeId));

            foreach (var kvp in _nodeUIs)
            {
                var node = _currentMapData?.GetNode(kvp.Key);
                if (node == null) continue;

                bool isAccessible = accessibleIds.Contains(kvp.Key);
                kvp.Value.SetAccessible(isAccessible);
                kvp.Value.SetCleared(node.IsCleared);
            }
        }

        /// <summary>
        /// 이동한 경로 업데이트
        /// </summary>
        private void UpdateTraversedPaths()
        {
            var manager = WorldMapManager.Instance;
            if (manager?.Progress == null) return;

            foreach (var pathUI in _pathUIs)
            {
                bool isTraversed = manager.Progress.HasTraversedPath(pathUI.FromNodeId, pathUI.ToNodeId) ||
                                   manager.Progress.HasTraversedPath(pathUI.ToNodeId, pathUI.FromNodeId);
                pathUI.SetTraversed(isTraversed);
            }
        }

        /// <summary>
        /// 특정 노드로 스크롤
        /// </summary>
        private void ScrollToNode(string nodeId)
        {
            if (_scrollRect == null || string.IsNullOrEmpty(nodeId)) return;

            if (_nodeUIs.TryGetValue(nodeId, out var nodeUI))
            {
                // 노드 위치로 스크롤
                var nodeRect = nodeUI.GetComponent<RectTransform>();
                if (nodeRect != null)
                {
                    Canvas.ForceUpdateCanvases();

                    var contentRect = _scrollRect.content;
                    var viewportRect = _scrollRect.viewport;

                    // 노드 위치를 뷰포트 중앙으로
                    var nodePos = nodeRect.anchoredPosition;
                    var contentSize = contentRect.rect.size;
                    var viewportSize = viewportRect.rect.size;

                    var normalizedX = Mathf.Clamp01((nodePos.x + _scrollPadding) / (contentSize.x - viewportSize.x));
                    var normalizedY = Mathf.Clamp01((nodePos.y + _scrollPadding) / (contentSize.y - viewportSize.y));

                    _scrollRect.normalizedPosition = new Vector2(normalizedX, normalizedY);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 맵 새로고침
        /// </summary>
        public void RefreshMap()
        {
            if (_currentMapData != null)
            {
                UpdateAccessibleNodes();
                UpdateTraversedPaths();
            }
        }

        /// <summary>
        /// 특정 노드 UI 가져오기
        /// </summary>
        public WorldMapNodeUI GetNodeUI(string nodeId)
        {
            return _nodeUIs.TryGetValue(nodeId, out var ui) ? ui : null;
        }

        #endregion
    }
}
