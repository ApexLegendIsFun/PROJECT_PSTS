// Map/Data/WorldMapData.cs
// 월드맵 전체 데이터 (ScriptableObject)

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Map.Data
{
    /// <summary>
    /// 월드맵 전체 데이터
    /// 에디터에서 맵을 디자인하여 저장하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "WorldMap", menuName = "ProjectSS/Map/World Map Data")]
    public class WorldMapData : ScriptableObject
    {
        [Header("맵 기본 정보")]
        [Tooltip("맵 고유 ID")]
        public string MapId;

        [Tooltip("맵 표시 이름 (예: 1막 - 여정의 시작)")]
        public string MapName;

        [Tooltip("맵 설명")]
        [TextArea(2, 4)]
        public string Description;

        [Header("노드 데이터")]
        [Tooltip("맵의 모든 노드")]
        public List<WorldMapNode> Nodes = new List<WorldMapNode>();

        [Header("시작/종료 설정")]
        [Tooltip("시작 노드 ID")]
        public string StartNodeId;

        [Tooltip("최종 노드 ID (보스 또는 종료 지점)")]
        public string EndNodeId;

        // 캐시
        private Dictionary<string, WorldMapNode> _nodeCache;

        #region Node Access Methods

        /// <summary>
        /// 노드 캐시 초기화
        /// </summary>
        private void EnsureCache()
        {
            if (_nodeCache == null || _nodeCache.Count != Nodes.Count)
            {
                _nodeCache = new Dictionary<string, WorldMapNode>();
                foreach (var node in Nodes)
                {
                    if (!string.IsNullOrEmpty(node.NodeId))
                        _nodeCache[node.NodeId] = node;
                }
            }
        }

        /// <summary>
        /// ID로 노드 조회
        /// </summary>
        public WorldMapNode GetNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;

            EnsureCache();
            return _nodeCache.TryGetValue(nodeId, out var node) ? node : null;
        }

        /// <summary>
        /// 시작 노드 조회
        /// </summary>
        public WorldMapNode GetStartNode()
        {
            return GetNode(StartNodeId);
        }

        /// <summary>
        /// 종료 노드 조회
        /// </summary>
        public WorldMapNode GetEndNode()
        {
            return GetNode(EndNodeId);
        }

        /// <summary>
        /// 특정 노드와 연결된 노드들 조회
        /// </summary>
        public List<WorldMapNode> GetConnectedNodes(string nodeId)
        {
            var node = GetNode(nodeId);
            if (node == null || node.ConnectedNodeIds == null)
                return new List<WorldMapNode>();

            return node.ConnectedNodeIds
                .Select(id => GetNode(id))
                .Where(n => n != null)
                .ToList();
        }

        /// <summary>
        /// 두 노드가 연결되어 있는지 확인
        /// </summary>
        public bool AreNodesConnected(string fromId, string toId)
        {
            var fromNode = GetNode(fromId);
            if (fromNode == null) return false;

            return fromNode.IsConnectedTo(toId);
        }

        /// <summary>
        /// 특정 타입의 노드들 조회
        /// </summary>
        public List<WorldMapNode> GetNodesByType(RegionType type)
        {
            return Nodes.Where(n => n.RegionType == type).ToList();
        }

        #endregion

        #region Validation

        /// <summary>
        /// 맵 데이터 유효성 검증
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            // 기본 검증
            if (string.IsNullOrEmpty(MapId))
                errors.Add("MapId가 비어있습니다.");

            if (Nodes == null || Nodes.Count == 0)
                errors.Add("노드가 없습니다.");

            if (string.IsNullOrEmpty(StartNodeId))
                errors.Add("시작 노드가 지정되지 않았습니다.");

            if (string.IsNullOrEmpty(EndNodeId))
                errors.Add("종료 노드가 지정되지 않았습니다.");

            // 시작/종료 노드 존재 확인
            if (GetNode(StartNodeId) == null)
                errors.Add($"시작 노드 '{StartNodeId}'를 찾을 수 없습니다.");

            if (GetNode(EndNodeId) == null)
                errors.Add($"종료 노드 '{EndNodeId}'를 찾을 수 없습니다.");

            // 노드 ID 중복 확인
            var ids = Nodes.Select(n => n.NodeId).ToList();
            var duplicates = ids.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var dup in duplicates)
                errors.Add($"노드 ID '{dup}'가 중복되었습니다.");

            // 연결 유효성 확인
            foreach (var node in Nodes)
            {
                if (node.ConnectedNodeIds == null) continue;

                foreach (var connId in node.ConnectedNodeIds)
                {
                    if (GetNode(connId) == null)
                        errors.Add($"노드 '{node.NodeId}'의 연결 대상 '{connId}'를 찾을 수 없습니다.");
                }
            }

            return errors.Count == 0;
        }

        #endregion

        #region Runtime State Management

        /// <summary>
        /// 모든 노드의 런타임 상태 초기화
        /// </summary>
        public void ResetAllRuntimeStates()
        {
            foreach (var node in Nodes)
            {
                node.ResetRuntimeState();
            }

            // 시작 노드는 기본 접근 가능
            var startNode = GetStartNode();
            if (startNode != null)
            {
                startNode.IsAccessible = true;
            }
        }

        /// <summary>
        /// 진행 상태 적용
        /// </summary>
        public void ApplyProgress(WorldMapProgress progress)
        {
            if (progress == null) return;

            ResetAllRuntimeStates();

            // 방문/클리어 상태 복원
            foreach (var visitedId in progress.VisitedNodeIds)
            {
                var node = GetNode(visitedId);
                if (node != null) node.IsVisited = true;
            }

            foreach (var clearedId in progress.ClearedNodeIds)
            {
                var node = GetNode(clearedId);
                if (node != null)
                {
                    node.IsCleared = true;
                    // 클리어한 노드의 연결 노드들 접근 가능 설정
                    foreach (var connectedNode in GetConnectedNodes(clearedId))
                    {
                        connectedNode.IsAccessible = true;
                    }
                }
            }
        }

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            // 캐시 초기화
            _nodeCache = null;
        }

        private void OnValidate()
        {
            // 에디터에서 수정 시 캐시 무효화
            _nodeCache = null;
        }

        #endregion
    }
}
