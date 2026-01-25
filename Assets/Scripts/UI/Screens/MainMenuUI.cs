using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Core;

namespace ProjectSS.UI
{
    /// <summary>
    /// 메인 메뉴 UI
    /// Main menu UI
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private GameObject settingsPanel;

        private void Start()
        {
            SetupButtons();
            UpdateContinueButton();
        }

        private void SetupButtons()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void UpdateContinueButton()
        {
            // 저장된 게임이 있는지 확인
            // TODO: 실제 저장 시스템 연동
            bool hasSaveGame = false;

            if (continueButton != null)
            {
                continueButton.interactable = hasSaveGame;
            }
        }

        private void OnNewGameClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewRun();
            }
        }

        private void OnContinueClicked()
        {
            // TODO: 저장된 게임 로드
            Debug.Log("Continue game");
        }

        private void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
