// Map/UI/WorldMapVisualConfig.cs
// 월드맵 비주얼 설정 (ScriptableObject)

using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Map.UI
{
    /// <summary>
    /// 월드맵 비주얼 설정
    /// </summary>
    [CreateAssetMenu(fileName = "WorldMapVisualConfig", menuName = "ProjectSS/Map/Visual Config")]
    public class WorldMapVisualConfig : ScriptableObject
    {
        [System.Serializable]
        public class RegionVisual
        {
            [Tooltip("지역 타입")]
            public RegionType RegionType;

            [Tooltip("배경 색상")]
            public Color BackgroundColor = Color.gray;

            [Tooltip("아이콘")]
            public Sprite Icon;

            [Tooltip("표시 이름")]
            public string DisplayName;

            [Tooltip("설명")]
            [TextArea(1, 3)]
            public string Description;
        }

        [Header("Region Visuals")]
        [SerializeField]
        private RegionVisual[] _regionVisuals = new RegionVisual[]
        {
            new RegionVisual
            {
                RegionType = RegionType.Start,
                BackgroundColor = new Color(0.2f, 0.7f, 0.9f),
                DisplayName = "시작",
                Description = "여정의 시작점"
            },
            new RegionVisual
            {
                RegionType = RegionType.Shelter,
                BackgroundColor = new Color(0.3f, 0.8f, 0.3f),
                DisplayName = "쉼터",
                Description = "휴식을 취하고 회복할 수 있습니다"
            },
            new RegionVisual
            {
                RegionType = RegionType.Dungeon,
                BackgroundColor = new Color(0.8f, 0.3f, 0.3f),
                DisplayName = "던전",
                Description = "적과의 전투가 기다리고 있습니다"
            },
            new RegionVisual
            {
                RegionType = RegionType.Boss,
                BackgroundColor = new Color(0.5f, 0f, 0.8f),
                DisplayName = "보스",
                Description = "강력한 적이 기다리고 있습니다"
            },
            new RegionVisual
            {
                RegionType = RegionType.End,
                BackgroundColor = new Color(1f, 0.84f, 0f),
                DisplayName = "목적지",
                Description = "여정의 끝"
            }
        };

        [Header("Path Colors")]
        [Tooltip("기본 경로 색상")]
        public Color PathDefaultColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        [Tooltip("이동한 경로 색상")]
        public Color PathTraversedColor = new Color(0.9f, 0.8f, 0.3f, 1f);

        [Header("State Colors")]
        [Tooltip("접근 가능 글로우 색상")]
        public Color AccessibleGlowColor = new Color(1f, 1f, 0.5f, 0.5f);

        [Tooltip("클리어된 노드 오버레이 색상")]
        public Color ClearedOverlayColor = new Color(0f, 0f, 0f, 0.5f);

        [Tooltip("플레이어 마커 색상")]
        public Color PlayerMarkerColor = Color.cyan;

        [Tooltip("비활성 노드 알파")]
        [Range(0.1f, 1f)]
        public float InactiveAlpha = 0.5f;

        #region Lookup Methods

        /// <summary>
        /// 지역 타입별 배경 색상 조회
        /// </summary>
        public Color GetBackgroundColor(RegionType type)
        {
            foreach (var visual in _regionVisuals)
            {
                if (visual.RegionType == type)
                    return visual.BackgroundColor;
            }
            return Color.gray;
        }

        /// <summary>
        /// 지역 타입별 아이콘 조회
        /// </summary>
        public Sprite GetIcon(RegionType type)
        {
            foreach (var visual in _regionVisuals)
            {
                if (visual.RegionType == type)
                    return visual.Icon;
            }
            return null;
        }

        /// <summary>
        /// 지역 타입별 표시 이름 조회
        /// </summary>
        public string GetDisplayName(RegionType type)
        {
            foreach (var visual in _regionVisuals)
            {
                if (visual.RegionType == type)
                    return visual.DisplayName;
            }
            return type.ToString();
        }

        /// <summary>
        /// 지역 타입별 설명 조회
        /// </summary>
        public string GetDescription(RegionType type)
        {
            foreach (var visual in _regionVisuals)
            {
                if (visual.RegionType == type)
                    return visual.Description;
            }
            return "";
        }

        /// <summary>
        /// 전체 비주얼 데이터 조회
        /// </summary>
        public RegionVisual GetVisualData(RegionType type)
        {
            foreach (var visual in _regionVisuals)
            {
                if (visual.RegionType == type)
                    return visual;
            }
            return null;
        }

        #endregion
    }
}
