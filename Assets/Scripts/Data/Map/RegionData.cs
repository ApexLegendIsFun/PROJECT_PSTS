using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data
{
    /// <summary>
    /// 지역 데이터 (Town/Field/Dungeon 컨테이너)
    /// Region data (Town/Field/Dungeon container)
    /// </summary>
    [CreateAssetMenu(fileName = "NewRegion", menuName = "Game/Map/Region Data")]
    public class RegionData : ScriptableObject
    {
        [Header("지역 정보 (Region Info)")]
        [Tooltip("지역 ID / Region ID")]
        public string regionId;

        [Tooltip("지역 이름 / Region name")]
        public string regionName;

        [Tooltip("지역 설명 / Region description")]
        [TextArea(2, 4)]
        public string regionDescription;

        [Tooltip("지역 아이콘 / Region icon")]
        public Sprite regionIcon;

        [Header("맵 설정 (Map Configuration)")]
        [Tooltip("맵 타입 / Map type")]
        public MapType mapType;

        [Tooltip("맵 레이아웃 / Map layout")]
        public MapLayoutData mapLayout;

        [Header("진행 설정 (Progression)")]
        [Tooltip("다음 지역 / Next region")]
        public RegionData nextRegion;

        [Tooltip("최종 지역 여부 / Is final region")]
        public bool isFinalRegion;

        /// <summary>
        /// 다음 지역 존재 여부
        /// Check if next region exists
        /// </summary>
        public bool HasNextRegion => nextRegion != null && !isFinalRegion;

        /// <summary>
        /// 지역 정보 문자열 반환
        /// Get region info string
        /// </summary>
        public override string ToString()
        {
            return $"{regionName} ({mapType})";
        }
    }
}
