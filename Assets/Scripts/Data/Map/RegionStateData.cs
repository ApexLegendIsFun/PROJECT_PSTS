using System.Collections.Generic;

namespace ProjectSS.Data
{
    /// <summary>
    /// 지역 상태 데이터 (저장/로드용)
    /// Region state data (for save/load)
    ///
    /// Map 어셈블리 내부에서 사용, Run 어셈블리와의 순환 참조 방지
    /// Used within Map assembly, prevents circular reference with Run assembly
    /// </summary>
    [System.Serializable]
    public class RegionStateData
    {
        /// <summary>
        /// 현재 지역 ID
        /// Current region ID
        /// </summary>
        public string currentRegionId;

        /// <summary>
        /// 현재 지역 인덱스
        /// Current region index
        /// </summary>
        public int currentRegionIndex;

        /// <summary>
        /// 완료된 지역 ID 목록
        /// Completed region IDs
        /// </summary>
        public List<string> completedRegionIds = new List<string>();

        public RegionStateData()
        {
            completedRegionIds = new List<string>();
        }

        public RegionStateData(string regionId, int regionIndex, List<string> completedIds)
        {
            currentRegionId = regionId;
            currentRegionIndex = regionIndex;
            completedRegionIds = completedIds ?? new List<string>();
        }

        /// <summary>
        /// 복제본 생성
        /// Create copy
        /// </summary>
        public RegionStateData Clone()
        {
            return new RegionStateData
            {
                currentRegionId = currentRegionId,
                currentRegionIndex = currentRegionIndex,
                completedRegionIds = new List<string>(completedRegionIds)
            };
        }
    }
}
