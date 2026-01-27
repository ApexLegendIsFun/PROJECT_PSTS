// MainMenu/MainMenuController.cs
// 메인 메뉴 UI 컨트롤러

using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core;
using ProjectSS.Core.Events;

namespace ProjectSS.MainMenu
{
    /// <summary>
    /// 메인 메뉴 UI 관리
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _characterSelectPanel;
        [SerializeField] private GameObject _settingsPanel;

        [Header("Buttons")]
        [SerializeField] private Button _newRunButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        [Header("Character Select")]
        [SerializeField] private CharacterSelectPanel _characterSelect;

        private void Awake()
        {
            // 버튼 이벤트 연결
            if (_newRunButton != null)
                _newRunButton.onClick.AddListener(OnNewRunClicked);

            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinueClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);

            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void Start()
        {
            // 메인 패널 표시
            ShowMainPanel();

            // 이어하기 버튼 상태 (저장된 런이 있는지 확인)
            UpdateContinueButton();
        }

        private void OnDestroy()
        {
            // 버튼 이벤트 해제
            if (_newRunButton != null)
                _newRunButton.onClick.RemoveListener(OnNewRunClicked);

            if (_continueButton != null)
                _continueButton.onClick.RemoveListener(OnContinueClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettingsClicked);

            if (_quitButton != null)
                _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        #region Panel Management

        private void ShowMainPanel()
        {
            SetPanelActive(_mainPanel, true);
            SetPanelActive(_characterSelectPanel, false);
            SetPanelActive(_settingsPanel, false);
        }

        private void ShowCharacterSelect()
        {
            SetPanelActive(_mainPanel, false);
            SetPanelActive(_characterSelectPanel, true);
            SetPanelActive(_settingsPanel, false);
        }

        private void ShowSettings()
        {
            SetPanelActive(_settingsPanel, true);
        }

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
            {
                panel.SetActive(active);
            }
        }

        #endregion

        #region Button Handlers

        private void OnNewRunClicked()
        {
            Debug.Log("[MainMenu] New Run clicked");
            ShowCharacterSelect();

            if (_characterSelect != null)
            {
                _characterSelect.Initialize();
            }
        }

        private void OnContinueClicked()
        {
            Debug.Log("[MainMenu] Continue clicked");

            // TODO: 저장된 런 로드 후 맵으로 이동
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMap();
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("[MainMenu] Settings clicked");
            ShowSettings();
        }

        private void OnQuitClicked()
        {
            Debug.Log("[MainMenu] Quit clicked");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        #endregion

        #region State Updates

        private void UpdateContinueButton()
        {
            // TODO: SaveManager에서 저장된 런 존재 여부 확인
            bool hasSavedRun = false;

            if (_continueButton != null)
            {
                _continueButton.interactable = hasSavedRun;
            }
        }

        #endregion

        #region Public Methods (Character Select에서 호출)

        /// <summary>
        /// 캐릭터 선택 완료 시 런 시작
        /// </summary>
        public void StartRunWithParty(string[] characterIds)
        {
            Debug.Log($"[MainMenu] Starting run with {characterIds.Length} characters");

            // 이벤트 발행
            EventBus.Publish(new RunStartedEvent
            {
                PartyCharacterIds = new System.Collections.Generic.List<string>(characterIds)
            });

            // 맵 씬으로 이동
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewRun();
            }
        }

        /// <summary>
        /// 캐릭터 선택 취소
        /// </summary>
        public void CancelCharacterSelect()
        {
            ShowMainPanel();
        }

        /// <summary>
        /// 설정 패널 닫기
        /// </summary>
        public void CloseSettings()
        {
            SetPanelActive(_settingsPanel, false);
        }

        #endregion
    }
}
