// Map/WorldMapSceneInitializer.cs
// 월드맵 씬 초기화 (World Map Scene Initializer)

using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Map.Data;

namespace ProjectSS.Map
{
    /// <summary>
    /// 월드맵 씬 초기화
    /// 씬 로드 시 WorldMapManager에 맵 데이터를 로드
    /// </summary>
    public class WorldMapSceneInitializer : MonoBehaviour
    {
        [Header("Map Data")]
        [Tooltip("기본 맵 데이터 (없으면 RunManager에서 가져옴)")]
        [SerializeField] private WorldMapData _defaultMapData;

        [Header("Debug")]
        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private bool _useDefaultMapForTesting = true;

        private void Start()
        {
            SceneBootstrapper.EnsureInitialized();

            if (_autoInitialize)
            {
                InitializeWorldMap();
            }
        }

        /// <summary>
        /// 월드맵 초기화
        /// </summary>
        public void InitializeWorldMap()
        {
            var manager = WorldMapManager.Instance;

            if (manager == null)
            {
                Debug.LogError("[WorldMapSceneInitializer] WorldMapManager가 없습니다!");
                return;
            }

            // 맵 데이터 결정
            WorldMapData mapData = GetMapData();

            if (mapData == null)
            {
                Debug.LogError("[WorldMapSceneInitializer] 로드할 맵 데이터가 없습니다!");
                return;
            }

            // 맵 로드
            manager.LoadMap(mapData);
            Debug.Log($"[WorldMapSceneInitializer] 맵 로드 완료: {mapData.MapName}");
        }

        /// <summary>
        /// 맵 데이터 가져오기
        /// RunManager에서 현재 맵을 가져오거나, 테스트용 기본 맵 사용
        /// </summary>
        private WorldMapData GetMapData()
        {
            // TODO: RunManager에서 현재 맵 데이터 가져오기
            // var runManager = ServiceLocator.TryGet<RunManager>();
            // if (runManager?.CurrentMapData != null)
            //     return runManager.CurrentMapData;

            // 테스트 모드이거나 RunManager 없으면 기본 맵 사용
            if (_useDefaultMapForTesting && _defaultMapData != null)
            {
                return _defaultMapData;
            }

            return null;
        }

        /// <summary>
        /// 외부에서 맵 데이터 설정
        /// </summary>
        public void SetMapData(WorldMapData mapData)
        {
            _defaultMapData = mapData;
        }
    }
}
