using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
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

        /// <summary>
        /// 소속 지역 ID (통합 월드맵용)
        /// Region ID this node belongs to (for unified world map)
        /// </summary>
        public string RegionId { get; private set; }

        private List<string> connectedNodeIds = new List<string>();

        public IReadOnlyList<string> ConnectedNodeIds => connectedNodeIds;

        /// <summary>
        /// 기본 생성자 (기존 호환용)
        /// Default constructor (for backward compatibility)
        /// </summary>
        public MapNode(int floor, int column, MapNodeType type)
            : this(floor, column, type, null)
        {
        }

        /// <summary>
        /// 지역 ID 포함 생성자 (통합 월드맵용)
        /// Constructor with region ID (for unified world map)
        /// </summary>
        public MapNode(int floor, int column, MapNodeType type, string regionId)
        {
            Floor = floor;
            Column = column;
            NodeType = type;
            RegionId = regionId;
            IsVisited = false;
            IsAccessible = false;

            // 노드 ID 형식: 지역 ID가 있으면 "regionId_floor_column", 없으면 "node_floor_column"
            NodeId = string.IsNullOrEmpty(regionId)
                ? $"node_{floor}_{column}"
                : $"{regionId}_{floor}_{column}";
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
