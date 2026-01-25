using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Map;

namespace ProjectSS.Run
{
    /// <summary>
    /// 층 진행 관리자
    /// Floor progression manager
    /// </summary>
    public class FloorManager
    {
        private RunState runState;
        private MapManager mapManager;

        public int CurrentFloor => runState.currentFloor;

        public FloorManager(RunState state, MapManager map)
        {
            runState = state;
            mapManager = map;
        }

        /// <summary>
        /// 노드 선택 시 처리
        /// Handle node selection
        /// </summary>
        public void OnNodeSelected(MapNode node)
        {
            runState.currentFloor = node.Floor;
            runState.currentNodeId = node.NodeId;
            runState.visitedNodeIds.Add(node.NodeId);
        }

        /// <summary>
        /// 현재 노드 완료 처리
        /// Complete current node
        /// </summary>
        public void CompleteCurrentNode()
        {
            mapManager.CompleteCurrentNode();
        }

        /// <summary>
        /// 보스 전 확인
        /// Check if at boss
        /// </summary>
        public bool IsAtBoss()
        {
            return mapManager.IsBossNode();
        }

        /// <summary>
        /// 다음 노드 타입 반환
        /// Get current node type
        /// </summary>
        public MapNodeType GetCurrentNodeType()
        {
            return mapManager.CurrentNode?.NodeType ?? MapNodeType.Combat;
        }
    }
}
