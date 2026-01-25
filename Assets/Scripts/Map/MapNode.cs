using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Data;

namespace ProjectSS.Map
{
    /// <summary>
    /// 맵 노드 클래스
    /// Map node class
    /// </summary>
    [System.Serializable]
    public class MapNode
    {
        public string NodeId { get; private set; }
        public int Floor { get; private set; }
        public int Column { get; private set; }
        public MapNodeType NodeType { get; private set; }
        public Vector2 Position { get; private set; }
        public bool IsVisited { get; private set; }
        public bool IsAccessible { get; set; }

        private List<string> connectedNodeIds = new List<string>();

        public IReadOnlyList<string> ConnectedNodeIds => connectedNodeIds;

        public MapNode(int floor, int column, MapNodeType type)
        {
            NodeId = $"node_{floor}_{column}";
            Floor = floor;
            Column = column;
            NodeType = type;
            IsVisited = false;
            IsAccessible = false;
        }

        /// <summary>
        /// 노드 위치 설정
        /// Set node position
        /// </summary>
        public void SetPosition(Vector2 pos)
        {
            Position = pos;
        }

        /// <summary>
        /// 연결 추가
        /// Add connection
        /// </summary>
        public void AddConnection(string nodeId)
        {
            if (!connectedNodeIds.Contains(nodeId))
            {
                connectedNodeIds.Add(nodeId);
            }
        }

        /// <summary>
        /// 노드 방문 처리
        /// Mark as visited
        /// </summary>
        public void Visit()
        {
            IsVisited = true;
        }

        /// <summary>
        /// 노드 타입에 따른 색상 반환 (플레이스홀더용)
        /// Get color based on node type (for placeholder)
        /// </summary>
        public Color GetNodeColor()
        {
            return NodeType switch
            {
                MapNodeType.Combat => new Color(0.9f, 0.3f, 0.3f),      // Red
                MapNodeType.Elite => new Color(1f, 0.84f, 0f),          // Gold
                MapNodeType.Boss => new Color(0.8f, 0.1f, 0.1f),        // Dark red
                MapNodeType.Rest => new Color(0.3f, 0.8f, 0.4f),        // Green
                MapNodeType.Event => new Color(0.3f, 0.5f, 0.9f),       // Blue
                MapNodeType.Shop => new Color(0.9f, 0.8f, 0.2f),        // Yellow
                MapNodeType.Treasure => new Color(0.6f, 0.4f, 0.2f),    // Brown
                _ => Color.white
            };
        }

        /// <summary>
        /// 노드 타입 심볼 반환 (플레이스홀더용)
        /// Get symbol based on node type (for placeholder)
        /// </summary>
        public string GetNodeSymbol()
        {
            return NodeType switch
            {
                MapNodeType.Combat => "⚔",
                MapNodeType.Elite => "!",
                MapNodeType.Boss => "☠",
                MapNodeType.Rest => "🔥",
                MapNodeType.Event => "?",
                MapNodeType.Shop => "$",
                MapNodeType.Treasure => "◆",
                _ => "●"
            };
        }
    }
}
