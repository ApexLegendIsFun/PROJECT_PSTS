// Core/BootLoader.cs
// 부팅 시퀀스 관리자

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectSS.Core.Events;

namespace ProjectSS.Core
{
    /// <summary>
    /// 부팅 시퀀스 관리
    /// Boot Scene에 배치하여 게임 초기화 처리
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [Header("Boot Settings")]
        [SerializeField] private float _logoDisplayTime = 1.5f;
        [SerializeField] private float _minimumLoadTime = 0.5f;

        [Header("References")]
        [SerializeField] private GameObject _logoObject;
        [SerializeField] private GameObject _loadingIndicator;

        [Header("Prefabs")]
        [SerializeField] private GameObject _gameManagerPrefab;

        [Header("Debug")]
        [SerializeField] private bool _skipLogo = false;

        private void Start()
        {
            StartCoroutine(BootSequence());
        }

        private IEnumerator BootSequence()
        {
            Debug.Log("[BootLoader] Starting boot sequence...");

            // 1. 로고 표시
            if (!_skipLogo && _logoObject != null)
            {
                _logoObject.SetActive(true);
                yield return new WaitForSeconds(_logoDisplayTime);
                _logoObject.SetActive(false);
            }

            // 2. 로딩 인디케이터 표시
            if (_loadingIndicator != null)
            {
                _loadingIndicator.SetActive(true);
            }

            float loadStartTime = Time.time;

            // 3. 매니저 초기화
            yield return InitializeManagers();

            // 4. 필수 에셋 로드
            yield return LoadEssentialAssets();

            // 5. 최소 로딩 시간 보장 (UX)
            float elapsed = Time.time - loadStartTime;
            if (elapsed < _minimumLoadTime)
            {
                yield return new WaitForSeconds(_minimumLoadTime - elapsed);
            }

            // 6. 서비스 로케이터 초기화 완료 표시
            ServiceLocator.IsInitialized = true;

            Debug.Log("[BootLoader] Boot sequence completed.");

            // 7. 메인 메뉴로 전환
            LoadMainMenu();
        }

        private IEnumerator InitializeManagers()
        {
            Debug.Log("[BootLoader] Initializing managers...");

            // GameManager 생성 (이미 있으면 스킵)
            if (GameManager.Instance == null)
            {
                if (_gameManagerPrefab != null)
                {
                    Instantiate(_gameManagerPrefab);
                }
                else
                {
                    // 프리팹이 없으면 새 GameObject 생성
                    var go = new GameObject("GameManager");
                    go.AddComponent<GameManager>();
                }
            }

            yield return null;

            // 다른 매니저들 초기화 (추후 추가)
            // - AudioManager
            // - SaveManager
            // - RunManager 등

            Debug.Log("[BootLoader] Managers initialized.");
        }

        private IEnumerator LoadEssentialAssets()
        {
            Debug.Log("[BootLoader] Loading essential assets...");

            // TODO: Addressables 또는 Resources에서 필수 에셋 로드
            // - 카드 데이터
            // - 캐릭터 데이터
            // - UI 프리팹 등

            yield return null;

            Debug.Log("[BootLoader] Essential assets loaded.");
        }

        private void LoadMainMenu()
        {
            Debug.Log("[BootLoader] Loading Main Menu...");

            // GameManager를 통해 씬 로드
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadScene(SceneIndex.MainMenu);
            }
            else
            {
                // 폴백: 직접 씬 로드
                SceneManager.LoadScene((int)SceneIndex.MainMenu);
            }
        }
    }
}
