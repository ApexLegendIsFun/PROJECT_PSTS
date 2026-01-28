// Services/DataService.cs
// 데이터 서비스 구현
// 중앙집중 데이터 액세스 레이어

using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Config;
using ProjectSS.Data;

namespace ProjectSS.Services
{
    /// <summary>
    /// 데이터 서비스 구현
    /// 게임 데이터에 대한 통합 액세스 제공
    /// ServiceLocator에 등록하여 전역 접근 가능
    /// </summary>
    public class DataService : IDataService
    {
        private const string BALANCE_CONFIG_PATH = "Config/GameBalanceConfig";
        private const string CHARACTER_DB_PATH = "Config/CharacterDatabase";

        private GameBalanceConfigSO _balance;
        private CharacterDatabase _characterDatabase;
        private bool _isInitialized;

        #region Properties

        public GameBalanceConfigSO Balance => _balance;
        public CharacterDatabase Characters => _characterDatabase;
        public bool IsInitialized => _isInitialized;

        #endregion

        #region Singleton Access

        /// <summary>
        /// 편의를 위한 정적 접근자
        /// ServiceLocator.Get<IDataService>()의 축약형
        /// </summary>
        public static IDataService Instance => ServiceLocator.TryGet<IDataService>();

        #endregion

        #region Initialization

        /// <summary>
        /// 데이터 서비스 초기화
        /// BootLoader에서 호출
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[DataService] Already initialized.");
                return;
            }

            Debug.Log("[DataService] Initializing...");

            // 게임 밸런스 설정 로드
            LoadBalanceConfig();

            // 캐릭터 데이터베이스 로드
            LoadCharacterDatabase();

            // ServiceLocator에 등록
            ServiceLocator.Register<IDataService>(this);

            _isInitialized = true;
            Debug.Log("[DataService] Initialization complete.");
        }

        /// <summary>
        /// 데이터 서비스 정리
        /// </summary>
        public void Cleanup()
        {
            if (!_isInitialized)
            {
                return;
            }

            Debug.Log("[DataService] Cleaning up...");

            ServiceLocator.Unregister<IDataService>();

            _balance = null;
            _characterDatabase = null;
            _isInitialized = false;

            Debug.Log("[DataService] Cleanup complete.");
        }

        #endregion

        #region Data Loading

        private void LoadBalanceConfig()
        {
            // Resources에서 로드 시도
            _balance = Resources.Load<GameBalanceConfigSO>(BALANCE_CONFIG_PATH);

            if (_balance == null)
            {
                Debug.LogWarning($"[DataService] GameBalanceConfig not found at Resources/{BALANCE_CONFIG_PATH}. Using defaults.");

                // 런타임에 기본값으로 생성 (개발/테스트용)
                _balance = ScriptableObject.CreateInstance<GameBalanceConfigSO>();
            }
            else
            {
                Debug.Log("[DataService] GameBalanceConfig loaded successfully.");
            }
        }

        private void LoadCharacterDatabase()
        {
            // Resources에서 로드 시도
            _characterDatabase = Resources.Load<CharacterDatabase>(CHARACTER_DB_PATH);

            if (_characterDatabase == null)
            {
                Debug.LogWarning($"[DataService] CharacterDatabase not found at Resources/{CHARACTER_DB_PATH}. Character selection may use fallback data.");
            }
            else
            {
                Debug.Log($"[DataService] CharacterDatabase loaded successfully. {_characterDatabase.Count} characters available.");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 밸런스 설정 핫 리로드 (에디터 전용)
        /// </summary>
        public void ReloadBalanceConfig()
        {
            #if UNITY_EDITOR
            LoadBalanceConfig();
            Debug.Log("[DataService] Balance config reloaded.");
            #endif
        }

        #endregion
    }
}
