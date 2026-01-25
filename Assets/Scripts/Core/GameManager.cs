using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSS.Core
{
    /// <summary>
    /// 게임 전체 라이프사이클 관리 싱글톤
    /// Game lifecycle management singleton
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        private void Initialize()
        {
            // 서비스 초기화
            // Initialize services
            ServiceLocator.Initialize();
        }

        #region Scene Management

        /// <summary>
        /// 씬 전환 (비동기)
        /// Load scene asynchronously
        /// </summary>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName);
        }

        public void LoadMainMenu()
        {
            SetState(GameState.MainMenu);
            LoadScene("MainMenu");
        }

        public void LoadMap()
        {
            SetState(GameState.Map);
            LoadScene("Map");
        }

        public void LoadCombat()
        {
            SetState(GameState.Combat);
            LoadScene("Combat");
        }

        public void LoadEvent()
        {
            SetState(GameState.Event);
            LoadScene("Event");
        }

        public void LoadShop()
        {
            SetState(GameState.Shop);
            LoadScene("Shop");
        }

        public void LoadRest()
        {
            SetState(GameState.Rest);
            LoadScene("Rest");
        }

        public void LoadReward()
        {
            SetState(GameState.Reward);
            LoadScene("Reward");
        }

        #endregion

        #region Game State

        public void SetState(GameState newState)
        {
            var previousState = _currentState;
            _currentState = newState;

            EventBus.Publish(new GameStateChangedEvent(previousState, newState));
        }

        public void PauseGame()
        {
            if (IsPaused) return;

            IsPaused = true;
            Time.timeScale = 0f;
            EventBus.Publish(new GamePausedEvent(true));
        }

        public void ResumeGame()
        {
            if (!IsPaused) return;

            IsPaused = false;
            Time.timeScale = 1f;
            EventBus.Publish(new GamePausedEvent(false));
        }

        #endregion

        #region Run Management

        /// <summary>
        /// 새 런 시작
        /// Start a new run
        /// </summary>
        public void StartNewRun()
        {
            SetState(GameState.Map);
            EventBus.Publish(new RunStartedEvent());
            LoadMap();
        }

        /// <summary>
        /// 런 종료
        /// End current run
        /// </summary>
        public void EndRun(bool victory)
        {
            EventBus.Publish(new RunEndedEvent(victory));
            LoadMainMenu();
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    /// <summary>
    /// 게임 상태 enum
    /// Game state enum
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Map,
        Combat,
        Event,
        Shop,
        Rest,
        Reward
    }
}
