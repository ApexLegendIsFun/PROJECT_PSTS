using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectSS.Core
{
    /// <summary>
    /// 부트 씬 초기화 및 자동 전환
    /// Boot scene initializer and auto-transition
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("GameManager 프리팹 / GameManager prefab")]
        [SerializeField] private GameObject gameManagerPrefab;

        [Tooltip("초기화 후 로드할 씬 / Scene to load after initialization")]
        [SerializeField] private string nextScene = "MainMenu";

        [Tooltip("초기화 지연 시간 (초) / Initialization delay in seconds")]
        [SerializeField] private float loadDelay = 0.5f;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // GameManager가 없으면 생성
            // Create GameManager if it doesn't exist
            if (GameManager.Instance == null)
            {
                if (gameManagerPrefab != null)
                {
                    Instantiate(gameManagerPrefab);
                    Debug.Log("[BootLoader] GameManager 인스턴스 생성");
                }
                else
                {
                    // 프리팹이 없으면 새 GameObject로 생성
                    // Create new GameObject if no prefab assigned
                    var go = new GameObject("GameManager");
                    go.AddComponent<GameManager>();
                    Debug.Log("[BootLoader] GameManager 직접 생성");
                }
            }

            // 다음 씬으로 전환
            // Transition to next scene
            Invoke(nameof(LoadNextScene), loadDelay);
        }

        private void LoadNextScene()
        {
            Debug.Log($"[BootLoader] {nextScene} 씬 로드 중...");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMainMenu();
            }
            else
            {
                // 폴백: 직접 씬 로드
                // Fallback: direct scene load
                SceneManager.LoadScene(nextScene);
            }
        }
    }
}
