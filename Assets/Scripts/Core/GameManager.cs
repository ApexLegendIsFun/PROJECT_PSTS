using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSS.Core
{
    /// <summary>
    /// 게임 전체 라이프사이클 관리 싱글톤 (마을/필드/던전 허브 시스템)
    /// Game lifecycle management singleton (Town/Field/Dungeon hub system)
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;

        /// <summary>
        /// 현재 맵 타입 (MapGeneratedEvent로 업데이트됨)
        /// Current map type (updated via MapGeneratedEvent)
        /// </summary>
        private MapType _currentMapType = MapType.Town;
        public MapType CurrentMapType => _currentMapType;

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

        private void OnEnable()
        {
            // 맵 생성 완료 이벤트 구독
            // Subscribe to map generated event
            EventBus.Subscribe<MapGeneratedEvent>(OnMapGenerated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MapGeneratedEvent>(OnMapGenerated);
        }

        private void Initialize()
        {
            // 서비스 초기화
            // Initialize services
            ServiceLocator.Initialize();
        }

        /// <summary>
        /// 맵 생성 완료 이벤트 핸들러
        /// Map generated event handler
        /// </summary>
        private void OnMapGenerated(MapGeneratedEvent evt)
        {
            _currentMapType = evt.MapType;
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
        /// 새 런 시작 (마을에서 시작)
        /// Start a new run (starts from town)
        /// </summary>
        public void StartNewRun()
        {
            // 런 초기화 요청 이벤트 발행 (RunManager가 구독)
            // Publish run init request event (RunManager subscribes)
            EventBus.Publish(new RunInitRequestedEvent());

            // 마을 맵 생성 요청 이벤트 발행 (MapManager가 구독)
            // Publish map generation request event (MapManager subscribes)
            int seed = System.Environment.TickCount;
            EventBus.Publish(new MapGenerationRequestedEvent(seed, MapType.Town));

            SetState(GameState.Map);
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

        /// <summary>
        /// 마을로 복귀
        /// Return to town
        /// </summary>
        public void ReturnToTown()
        {
            int seed = System.Environment.TickCount;
            EventBus.Publish(new MapGenerationRequestedEvent(seed, MapType.Town));

            SetState(GameState.Map);
            LoadMap();
        }

        /// <summary>
        /// 필드 진입
        /// Enter field
        /// </summary>
        public void EnterField()
        {
            int seed = System.Environment.TickCount;
            EventBus.Publish(new MapGenerationRequestedEvent(seed, MapType.Field));

            SetState(GameState.Map);
            LoadMap();
        }

        /// <summary>
        /// 던전 진입
        /// Enter dungeon
        /// </summary>
        public void EnterDungeon()
        {
            int seed = System.Environment.TickCount;
            EventBus.Publish(new MapGenerationRequestedEvent(seed, MapType.Dungeon));

            SetState(GameState.Map);
            LoadMap();
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
