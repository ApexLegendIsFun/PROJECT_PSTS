using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Map
{
    /// <summary>
    /// 지역 관리자 (Town/Field/Dungeon 진행 관리)
    /// Region manager (Town/Field/Dungeon progression management)
    /// </summary>
    public class RegionManager : MonoBehaviour
    {
        public static RegionManager Instance { get; private set; }

        [Header("지역 설정 (Region Configuration)")]
        [SerializeField] private RegionData[] allRegions;
        [SerializeField] private RegionData startingRegion;

        private RegionData currentRegion;
        private int currentRegionIndex;
        private List<string> completedRegionIds = new List<string>();

        #region Properties

        /// <summary>
        /// 현재 지역
        /// Current region
        /// </summary>
        public RegionData CurrentRegion => currentRegion;

        /// <summary>
        /// 현재 지역 인덱스
        /// Current region index
        /// </summary>
        public int CurrentRegionIndex => currentRegionIndex;

        /// <summary>
        /// 완료된 지역 ID 목록
        /// Completed region IDs
        /// </summary>
        public IReadOnlyList<string> CompletedRegionIds => completedRegionIds;

        /// <summary>
        /// 시작 지역
        /// Starting region
        /// </summary>
        public RegionData StartingRegion => startingRegion;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<MapCompletedEvent>(OnMapCompleted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MapCompletedEvent>(OnMapCompleted);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 런 초기화 (시작 지역으로 설정)
        /// Initialize run (set to starting region)
        /// </summary>
        public void InitializeRun()
        {
            InitializeRun(startingRegion);
        }

        /// <summary>
        /// 런 초기화 (지정된 지역으로 설정)
        /// Initialize run (set to specified region)
        /// </summary>
        public void InitializeRun(RegionData region)
        {
            currentRegion = region ?? startingRegion;
            currentRegionIndex = GetRegionIndex(currentRegion);
            completedRegionIds.Clear();

            Debug.Log($"RegionManager: Initialized run at region '{currentRegion?.regionName}'");
        }

        /// <summary>
        /// 지역 진입
        /// Enter region
        /// </summary>
        public void EnterRegion(RegionData region)
        {
            if (region == null)
            {
                Debug.LogError("RegionManager: Cannot enter null region");
                return;
            }

            currentRegion = region;
            currentRegionIndex = GetRegionIndex(region);

            Debug.Log($"RegionManager: Entered region '{region.regionName}'");

            EventBus.Publish(new RegionEnteredEvent(region.regionId, region.mapType));
        }

        /// <summary>
        /// 현재 지역 완료
        /// Complete current region
        /// </summary>
        public void CompleteCurrentRegion()
        {
            if (currentRegion == null) return;

            if (!completedRegionIds.Contains(currentRegion.regionId))
            {
                completedRegionIds.Add(currentRegion.regionId);
            }

            bool hasNext = currentRegion.HasNextRegion;

            Debug.Log($"RegionManager: Completed region '{currentRegion.regionName}', hasNext: {hasNext}");

            EventBus.Publish(new RegionCompletedEvent(currentRegion.regionId, hasNext));

            // 다음 지역으로 자동 진행
            if (hasNext)
            {
                EnterRegion(currentRegion.nextRegion);
            }
        }

        /// <summary>
        /// 다음 지역 가져오기
        /// Get next region
        /// </summary>
        public RegionData GetNextRegion()
        {
            return currentRegion?.nextRegion;
        }

        /// <summary>
        /// 최종 지역 여부
        /// Check if at final region
        /// </summary>
        public bool IsAtFinalRegion()
        {
            return currentRegion?.isFinalRegion ?? false;
        }

        /// <summary>
        /// 지역 완료 여부
        /// Check if region is completed
        /// </summary>
        public bool IsRegionCompleted(string regionId)
        {
            return completedRegionIds.Contains(regionId);
        }

        #endregion

        #region State Management (for RunManager integration)

        /// <summary>
        /// 현재 지역 상태 가져오기 (저장용)
        /// Get current region state (for save)
        /// </summary>
        public RegionStateData GetRegionState()
        {
            return new RegionStateData
            {
                currentRegionId = currentRegion?.regionId,
                currentRegionIndex = currentRegionIndex,
                completedRegionIds = new List<string>(completedRegionIds)
            };
        }

        /// <summary>
        /// 지역 상태 로드
        /// Load region state
        /// </summary>
        public void LoadRegionState(RegionStateData state)
        {
            if (state == null) return;

            currentRegionIndex = state.currentRegionIndex;
            completedRegionIds = state.completedRegionIds ?? new List<string>();

            // 지역 ID로 지역 데이터 찾기
            currentRegion = FindRegionById(state.currentRegionId);

            if (currentRegion == null && !string.IsNullOrEmpty(state.currentRegionId))
            {
                Debug.LogWarning($"RegionManager: Could not find region '{state.currentRegionId}', using starting region");
                currentRegion = startingRegion;
            }
        }

        #endregion

        #region Private Methods

        private void OnMapCompleted(MapCompletedEvent evt)
        {
            // 맵 완료 시 지역 완료 체크
            if (currentRegion != null && currentRegion.mapType == evt.CompletedMapType)
            {
                CompleteCurrentRegion();
            }
        }

        private int GetRegionIndex(RegionData region)
        {
            if (region == null || allRegions == null) return 0;

            for (int i = 0; i < allRegions.Length; i++)
            {
                if (allRegions[i] == region)
                    return i;
            }
            return 0;
        }

        private RegionData FindRegionById(string regionId)
        {
            if (string.IsNullOrEmpty(regionId) || allRegions == null)
                return null;

            foreach (var region in allRegions)
            {
                if (region != null && region.regionId == regionId)
                    return region;
            }
            return null;
        }

        #endregion
    }
}
