using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Map
{
    /// <summary>
    /// 통합 월드맵 런타임 데이터
    /// Unified world map runtime data
    /// </summary>
    [System.Serializable]
    public class WorldMapData
    {
        private Dictionary<string, MapNode> allNodes = new Dictionary<string, MapNode>();
        private Dictionary<string, RegionMapSection> regions = new Dictionary<string, RegionMapSection>();
        private List<string> regionOrder = new List<string>();

        /// <summary>
        /// 시작 지역 ID
        /// Starting region ID
        /// </summary>
        public string StartingRegionId { get; private set; }

        /// <summary>
        /// 총 노드 수
        /// Total node count
        /// </summary>
        public int TotalNodeCount => allNodes.Count;

        /// <summary>
        /// 지역 수
        /// Region count
        /// </summary>
        public int RegionCount => regions.Count;

        public WorldMapData()
        {
        }

        /// <summary>
        /// 지역 추가
        /// Add region section
        /// </summary>
        public void AddRegion(RegionMapSection region)
        {
            if (region == null || string.IsNullOrEmpty(region.RegionId))
            {
                Debug.LogError("WorldMapData: Invalid region");
                return;
            }

            regions[region.RegionId] = region;
            regionOrder.Add(region.RegionId);

            // 첫 번째 지역을 시작 지역으로 설정
            if (string.IsNullOrEmpty(StartingRegionId))
            {
                StartingRegionId = region.RegionId;
            }

            // 모든 노드를 전역 딕셔너리에 추가
            foreach (var node in region.GetAllNodes())
            {
                allNodes[node.NodeId] = node;
            }
        }

        /// <summary>
        /// ID로 노드 가져오기
        /// Get node by ID
        /// </summary>
        public MapNode GetNode(string nodeId)
        {
            return allNodes.TryGetValue(nodeId, out var node) ? node : null;
        }

        /// <summary>
        /// ID로 지역 가져오기
        /// Get region by ID
        /// </summary>
        public RegionMapSection GetRegion(string regionId)
        {
            return regions.TryGetValue(regionId, out var region) ? region : null;
        }

        /// <summary>
        /// 모든 노드 반환
        /// Get all nodes
        /// </summary>
        public IEnumerable<MapNode> GetAllNodes()
        {
            return allNodes.Values;
        }

        /// <summary>
        /// 모든 지역 반환 (순서대로)
        /// Get all regions in order
        /// </summary>
        public IEnumerable<RegionMapSection> GetAllRegions()
        {
            foreach (var regionId in regionOrder)
            {
                if (regions.TryGetValue(regionId, out var region))
                {
                    yield return region;
                }
            }
        }

        /// <summary>
        /// 노드 간 연결 추가
        /// Add connection between nodes
        /// </summary>
        public void ConnectNodes(string fromNodeId, string toNodeId)
        {
            var fromNode = GetNode(fromNodeId);
            var toNode = GetNode(toNodeId);

            if (fromNode != null && toNode != null)
            {
                fromNode.AddConnection(toNodeId);
            }
            else
            {
                Debug.LogWarning($"WorldMapData: Cannot connect {fromNodeId} -> {toNodeId}. Node not found.");
            }
        }

        /// <summary>
        /// 시작 지역의 첫 층 노드들
        /// Get starting nodes (first floor of starting region)
        /// </summary>
        public IEnumerable<MapNode> GetStartingNodes()
        {
            var startRegion = GetRegion(StartingRegionId);
            if (startRegion != null && startRegion.FloorCount > 0)
            {
                return startRegion.GetFloor(0);
            }
            return Enumerable.Empty<MapNode>();
        }

        /// <summary>
        /// 모든 노드 접근성 초기화
        /// Reset all nodes accessibility
        /// </summary>
        public void ResetAllAccessibility()
        {
            foreach (var node in allNodes.Values)
            {
                node.IsAccessible = false;
            }
        }

        /// <summary>
        /// 시작 노드들만 접근 가능하게 설정
        /// Set only starting nodes as accessible
        /// </summary>
        public void SetInitialAccessibility()
        {
            ResetAllAccessibility();
            foreach (var node in GetStartingNodes())
            {
                node.IsAccessible = true;
            }
        }
    }

    /// <summary>
    /// 지역별 맵 섹션
    /// Region map section
    /// </summary>
    [System.Serializable]
    public class RegionMapSection
    {
        private List<List<MapNode>> floors = new List<List<MapNode>>();
        private Dictionary<string, MapNode> nodeDict = new Dictionary<string, MapNode>();

        /// <summary>
        /// 지역 ID
        /// Region ID
        /// </summary>
        public string RegionId { get; private set; }

        /// <summary>
        /// 맵 타입
        /// Map type
        /// </summary>
        public MapType MapType { get; private set; }

        /// <summary>
        /// 지역 표시 이름
        /// Region display name
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// 월드맵 내 위치 오프셋
        /// World offset in unified map
        /// </summary>
        public Vector2 WorldOffset { get; private set; }

        /// <summary>
        /// 지역 배경 색상
        /// Region background color
        /// </summary>
        public Color RegionColor { get; private set; }

        /// <summary>
        /// 총 층 수
        /// Floor count
        /// </summary>
        public int FloorCount => floors.Count;

        public RegionMapSection(string regionId, MapType mapType, string displayName, Vector2 worldOffset, Color regionColor)
        {
            RegionId = regionId;
            MapType = mapType;
            DisplayName = displayName;
            WorldOffset = worldOffset;
            RegionColor = regionColor;
        }

        /// <summary>
        /// 층 추가
        /// Add floor
        /// </summary>
        public void AddFloor(List<MapNode> floorNodes)
        {
            floors.Add(floorNodes);
            foreach (var node in floorNodes)
            {
                nodeDict[node.NodeId] = node;
            }
        }

        /// <summary>
        /// 특정 층의 노드들 반환
        /// Get nodes at floor
        /// </summary>
        public List<MapNode> GetFloor(int floorIndex)
        {
            if (floorIndex >= 0 && floorIndex < floors.Count)
                return floors[floorIndex];
            return new List<MapNode>();
        }

        /// <summary>
        /// 마지막 층의 노드들 반환
        /// Get nodes at last floor
        /// </summary>
        public List<MapNode> GetLastFloor()
        {
            if (floors.Count > 0)
                return floors[floors.Count - 1];
            return new List<MapNode>();
        }

        /// <summary>
        /// ID로 노드 찾기
        /// Get node by ID
        /// </summary>
        public MapNode GetNode(string nodeId)
        {
            return nodeDict.TryGetValue(nodeId, out var node) ? node : null;
        }

        /// <summary>
        /// 모든 노드 반환
        /// Get all nodes
        /// </summary>
        public IEnumerable<MapNode> GetAllNodes()
        {
            foreach (var floor in floors)
            {
                foreach (var node in floor)
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// 지역 경계 계산 (노드 기반)
        /// Calculate region bounds based on nodes
        /// </summary>
        public Rect CalculateBounds(float padding = 1f)
        {
            if (floors.Count == 0) return Rect.zero;

            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var node in GetAllNodes())
            {
                Vector2 worldPos = node.Position + WorldOffset;
                if (worldPos.x < minX) minX = worldPos.x;
                if (worldPos.x > maxX) maxX = worldPos.x;
                if (worldPos.y < minY) minY = worldPos.y;
                if (worldPos.y > maxY) maxY = worldPos.y;
            }

            return new Rect(
                minX - padding,
                minY - padding,
                (maxX - minX) + padding * 2,
                (maxY - minY) + padding * 2
            );
        }
    }
}
