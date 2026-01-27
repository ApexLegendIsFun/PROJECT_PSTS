// Core/GameManager.cs
// 게임 매니저 - 전역 게임 상태 관리

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectSS.Core.Events;

namespace ProjectSS.Core
{
    /// <summary>
    /// 게임 매니저 - 싱글톤, 전역 게임 상태 관리
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] private GameState _currentState = GameState.Boot;

        [Header("Settings")]
        [SerializeField] private bool _debugMode = true;

        // 프로퍼티
        public GameState CurrentState => _currentState;
        public bool IsInCombat => _currentState == GameState.Combat;
        public bool IsInRun => _currentState == GameState.InRun || IsInCombat;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 서비스 등록
            ServiceLocator.Register(this);

            Log("GameManager initialized.");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<GameManager>();
                Instance = null;
            }
        }

        #region State Management

        /// <summary>
        /// 게임 상태 변경
        /// </summary>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            var previousState = _currentState;
            _currentState = newState;

            Log($"State changed: {previousState} -> {newState}");

            EventBus.Publish(new GameStateChangedEvent
            {
                PreviousState = previousState,
                NewState = newState
            });
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// 씬 로드 (비동기)
        /// </summary>
        public void LoadScene(SceneIndex sceneIndex)
        {
            StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        private IEnumerator LoadSceneAsync(SceneIndex sceneIndex)
        {
            Log($"Loading scene: {sceneIndex}");

            EventBus.Publish(new SceneLoadStartedEvent { TargetScene = sceneIndex });

            var asyncOp = SceneManager.LoadSceneAsync((int)sceneIndex);

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            EventBus.Publish(new SceneLoadCompletedEvent { LoadedScene = sceneIndex });

            // 씬별 상태 설정
            switch (sceneIndex)
            {
                case SceneIndex.MainMenu:
                    SetState(GameState.MainMenu);
                    break;
                case SceneIndex.Map:
                    SetState(GameState.InRun);
                    break;
                case SceneIndex.Combat:
                    SetState(GameState.Combat);
                    break;
            }

            Log($"Scene loaded: {sceneIndex}");
        }

        /// <summary>
        /// 메인 메뉴로 이동
        /// </summary>
        public void GoToMainMenu()
        {
            LoadScene(SceneIndex.MainMenu);
        }

        /// <summary>
        /// 전투 씬으로 이동
        /// </summary>
        public void GoToCombat()
        {
            LoadScene(SceneIndex.Combat);
        }

        /// <summary>
        /// 맵 씬으로 이동
        /// </summary>
        public void GoToMap()
        {
            LoadScene(SceneIndex.Map);
        }

        #endregion

        #region Game Flow

        /// <summary>
        /// 새 런 시작
        /// </summary>
        public void StartNewRun()
        {
            Log("Starting new run...");
            LoadScene(SceneIndex.Map);
        }

        /// <summary>
        /// 런 종료 (패배 또는 승리)
        /// </summary>
        public void EndRun(bool victory)
        {
            Log($"Run ended. Victory: {victory}");

            SetState(victory ? GameState.Victory : GameState.GameOver);

            EventBus.Publish(new RunEndedEvent
            {
                Victory = victory,
                FloorsCleared = 0, // TODO: RunManager에서 가져오기
                GoldEarned = 0
            });

            // 메인 메뉴로 복귀
            GoToMainMenu();
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            Log("Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region Utility

        private void Log(string message)
        {
            if (_debugMode)
            {
                Debug.Log($"[GameManager] {message}");
            }
        }

        #endregion
    }
}
