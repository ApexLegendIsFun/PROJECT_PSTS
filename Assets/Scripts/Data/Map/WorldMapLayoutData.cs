using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data
{
    /// <summary>
    /// 통합 월드맵 레이아웃 데이터
    /// Unified world map layout data
    /// </summary>
    [CreateAssetMenu(fileName = "NewWorldMapLayout", menuName = "Game/Map/World Map Layout")]
    public class WorldMapLayoutData : ScriptableObject
    {
        [Header("월드맵 정보 (World Map Info)")]
        [Tooltip("월드맵 ID / World map ID")]
        public string worldMapId;

        [Tooltip("월드맵 이름 / World map name")]
        public string worldMapName;

        [Header("지역 설정 (Region Configuration)")]
        [Tooltip("지역 목록 (Town, Field, Dungeon 순서) / Region list")]
        public List<RegionLayoutEntry> regions = new List<RegionLayoutEntry>();

        [Header("지역 간 연결 (Cross-Region Connections)")]
        [Tooltip("지역 간 노드 연결 / Connections between regions")]
        public List<CrossRegionConnection> crossRegionConnections = new List<CrossRegionConnection>();

        /// <summary>
        /// 지역 수 반환
        /// Get region count
        /// </summary>
        public int RegionCount => regions.Count;

        /// <summary>
        /// ID로 지역 찾기
        /// Find region by ID
        /// </summary>
        public RegionLayoutEntry GetRegion(string regionId)
        {
            return regions.Find(r => r.regionId == regionId);
        }

        /// <summary>
        /// 인덱스로 지역 찾기
        /// Get region by index
        /// </summary>
        public RegionLayoutEntry GetRegionAt(int index)
        {
            if (index >= 0 && index < regions.Count)
                return regions[index];
            return null;
        }

        private void OnValidate()
        {
            // 지역 ID 자동 설정
            for (int i = 0; i < regions.Count; i++)
            {
                if (string.IsNullOrEmpty(regions[i].regionId))
                {
                    regions[i].regionId = regions[i].mapType.ToString().ToLower();
                }
            }
        }
    }

    /// <summary>
    /// 지역 레이아웃 엔트리
    /// Region layout entry
    /// </summary>
    [System.Serializable]
    public class RegionLayoutEntry
    {
        [Tooltip("지역 ID (예: town, field, dungeon) / Region ID")]
        public string regionId;

        [Tooltip("지역 표시 이름 / Region display name")]
        public string displayName;

        [Tooltip("맵 타입 / Map type")]
        public MapType mapType;

        [Tooltip("지역 레이아웃 데이터 / Region layout data")]
        public MapLayoutData layout;

        [Tooltip("월드맵 내 위치 오프셋 / Position offset in world map")]
        public Vector2 worldOffset;

        [Tooltip("지역 배경 색상 / Region background color")]
        public Color regionColor = new Color(0.5f, 0.5f, 0.5f, 0.2f);

        /// <summary>
        /// 레이아웃 유효성 검사
        /// Check if layout is valid
        /// </summary>
        public bool HasValidLayout => layout != null && layout.FloorCount > 0;
    }

    /// <summary>
    /// 지역 간 연결 데이터
    /// Cross-region connection data
    /// </summary>
    [System.Serializable]
    public class CrossRegionConnection
    {
        [Header("출발 지역 (Source Region)")]
        [Tooltip("출발 지역 ID / Source region ID")]
        public string fromRegionId;

        [Tooltip("출발 층 인덱스 / Source floor index")]
        public int fromFloor;

        [Tooltip("출발 열 인덱스 / Source column index")]
        public int fromColumn;

        [Header("도착 지역 (Target Region)")]
        [Tooltip("도착 지역 ID / Target region ID")]
        public string toRegionId;

        [Tooltip("도착 층 인덱스 / Target floor index")]
        public int toFloor;

        [Tooltip("도착 열 인덱스 / Target column index")]
        public int toColumn;

        /// <summary>
        /// 출발 노드 ID 생성
        /// Generate source node ID
        /// </summary>
        public string GetFromNodeId()
        {
            return $"{fromRegionId}_{fromFloor}_{fromColumn}";
        }

        /// <summary>
        /// 도착 노드 ID 생성
        /// Generate target node ID
        /// </summary>
        public string GetToNodeId()
        {
            return $"{toRegionId}_{toFloor}_{toColumn}";
        }
    }
}
