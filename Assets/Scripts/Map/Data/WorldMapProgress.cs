// Map/Data/WorldMapProgress.cs
// 월드맵 런타임 진행 상태

using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Map.Data
{
    /// <summary>
    /// 월드맵 진행 상태
    /// 세이브/로드 및 런타임 상태 추적용
    /// </summary>
    [System.Serializable]
    public class WorldMapProgress
    {
        [Header("맵 식별")]
        [Tooltip("현재 진행 중인 맵 ID")]
        public string MapId;

        [Header("현재 위치")]
        [Tooltip("현재 플레이어가 위치한 노드 ID")]
        public string CurrentNodeId;

        [Header("진행 상태")]
        [Tooltip("방문한 노드 ID 목록")]
        public List<string> VisitedNodeIds = new List<string>();

        [Tooltip("클리어한 노드 ID 목록")]
        public List<string> ClearedNodeIds = new List<string>();

        [Tooltip("이동한 경로 (From_To 형식)")]
        public List<string> TraversedPaths = new List<string>();

        #region Factory Methods

        /// <summary>
        /// 새 맵 진행 상태 생성
        /// </summary>
        public static WorldMapProgress CreateNew(string mapId, string startNodeId)
        {
            var progress = new WorldMapProgress
            {
                MapId = mapId,
                CurrentNodeId = startNodeId,
                VisitedNodeIds = new List<string> { startNodeId },
                ClearedNodeIds = new List<string>(),
                TraversedPaths = new List<string>()
            };

            return progress;
        }

        /// <summary>
        /// WorldMapData에서 진행 상태 추출
        /// </summary>
        public static WorldMapProgress FromMapData(WorldMapData mapData, string currentNodeId)
        {
            var progress = new WorldMapProgress
            {
                MapId = mapData.MapId,
                CurrentNodeId = currentNodeId,
                VisitedNodeIds = new List<string>(),
                ClearedNodeIds = new List<string>(),
                TraversedPaths = new List<string>()
            };

            // 현재 상태 수집
            foreach (var node in mapData.Nodes)
            {
                if (node.IsVisited)
                    progress.VisitedNodeIds.Add(node.NodeId);

                if (node.IsCleared)
                    progress.ClearedNodeIds.Add(node.NodeId);
            }

            return progress;
        }

        #endregion

        #region State Management

        /// <summary>
        /// 노드 방문 기록
        /// </summary>
        public void RecordVisit(string nodeId)
        {
            if (!VisitedNodeIds.Contains(nodeId))
                VisitedNodeIds.Add(nodeId);
        }

        /// <summary>
        /// 노드 클리어 기록
        /// </summary>
        public void RecordClear(string nodeId)
        {
            if (!ClearedNodeIds.Contains(nodeId))
                ClearedNodeIds.Add(nodeId);
        }

        /// <summary>
        /// 이동 경로 기록
        /// </summary>
        public void RecordPath(string fromNodeId, string toNodeId)
        {
            var pathKey = $"{fromNodeId}_to_{toNodeId}";
            if (!TraversedPaths.Contains(pathKey))
                TraversedPaths.Add(pathKey);
        }

        /// <summary>
        /// 특정 경로를 이동했는지 확인
        /// </summary>
        public bool HasTraversedPath(string fromNodeId, string toNodeId)
        {
            var pathKey = $"{fromNodeId}_to_{toNodeId}";
            return TraversedPaths.Contains(pathKey);
        }

        /// <summary>
        /// 노드 방문 여부 확인
        /// </summary>
        public bool HasVisited(string nodeId)
        {
            return VisitedNodeIds.Contains(nodeId);
        }

        /// <summary>
        /// 노드 클리어 여부 확인
        /// </summary>
        public bool HasCleared(string nodeId)
        {
            return ClearedNodeIds.Contains(nodeId);
        }

        /// <summary>
        /// 현재 위치 업데이트
        /// </summary>
        public void SetCurrentNode(string nodeId)
        {
            CurrentNodeId = nodeId;
            RecordVisit(nodeId);
        }

        #endregion

        #region Serialization

        /// <summary>
        /// JSON 직렬화
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        /// <summary>
        /// JSON 역직렬화
        /// </summary>
        public static WorldMapProgress FromJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<WorldMapProgress>(json);
        }

        #endregion

        #region Statistics

        /// <summary>
        /// 진행률 계산 (클리어한 노드 / 전체 노드)
        /// </summary>
        public float GetProgressPercentage(int totalNodes)
        {
            if (totalNodes <= 0) return 0f;
            return (float)ClearedNodeIds.Count / totalNodes * 100f;
        }

        /// <summary>
        /// 맵 완료 여부 확인
        /// </summary>
        public bool IsMapCompleted(string endNodeId)
        {
            return ClearedNodeIds.Contains(endNodeId);
        }

        #endregion
    }
}
