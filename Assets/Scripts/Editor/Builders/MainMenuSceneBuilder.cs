// Editor/Builders/MainMenuSceneBuilder.cs
// MainMenu 씬 빌더 (MainMenu Scene Builder)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using ProjectSS.MainMenu;

namespace ProjectSS.Editor
{
    /// <summary>
    /// MainMenu 씬 생성 빌더
    /// - MainPanel (타이틀, 메뉴 버튼들)
    /// - CharacterSelectPanel (캐릭터 선택 UI)
    /// - SettingsPanel (설정 패널)
    /// - MainMenuController, CharacterSelectPanel 컴포넌트
    /// </summary>
    public class MainMenuSceneBuilder : ISceneBuilder
    {
        public string SceneName => "MainMenu";

        public string Build(string scenesFolder)
        {
            string scenePath = $"{scenesFolder}/{SceneName}.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = SceneName;

            UIComponentFactory.CreateEventSystem();
            var canvas = UIComponentFactory.CreateCanvas("MainMenuCanvas");

            // Main Panel
            var mainPanel = CreateMainPanel(canvas.transform);

            // Character Select Panel
            var charSelectPanelGo = CreateCharacterSelectPanel(canvas.transform);

            // Settings Panel
            var settingsPanel = CreateSettingsPanel(canvas.transform);

            // Controllers
            var (menuController, charSelectPanel) = CreateControllers(
                mainPanel, charSelectPanelGo, settingsPanel);

            // 캐릭터 버튼 생성 및 이벤트 연결
            BindCharacterButtons(charSelectPanelGo, charSelectPanel);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[MainMenuSceneBuilder] MainMenu 씬 생성: {scenePath}");
            return scenePath;
        }

        private GameObject CreateMainPanel(Transform parent)
        {
            var mainPanel = UIComponentFactory.CreatePanel(parent, "MainPanel");
            UIComponentFactory.SetRectTransformFill(mainPanel.GetComponent<RectTransform>());
            mainPanel.GetComponent<Image>().color = UIComponentFactory.PrimaryColor;

            // Title
            var title = UIComponentFactory.CreateText(mainPanel.transform, "TitleText", "PROJECT PSTS", 48);
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.sizeDelta = new Vector2(600, 80);

            // Buttons
            float buttonY = 0.55f;
            float buttonSpacing = 0.12f;

            var newRunBtn = UIComponentFactory.CreateButton(mainPanel.transform, "NewRunButton", "새로운 여정", new Vector2(300, 60));
            UIComponentFactory.PositionButton(newRunBtn, 0.5f, buttonY);

            var continueBtn = UIComponentFactory.CreateButton(mainPanel.transform, "ContinueButton", "이어하기", new Vector2(300, 60));
            UIComponentFactory.PositionButton(continueBtn, 0.5f, buttonY - buttonSpacing);

            var settingsBtn = UIComponentFactory.CreateButton(mainPanel.transform, "SettingsButton", "설정", new Vector2(300, 60));
            UIComponentFactory.PositionButton(settingsBtn, 0.5f, buttonY - buttonSpacing * 2);

            var quitBtn = UIComponentFactory.CreateButton(mainPanel.transform, "QuitButton", "종료", new Vector2(300, 60));
            UIComponentFactory.PositionButton(quitBtn, 0.5f, buttonY - buttonSpacing * 3);

            return mainPanel;
        }

        private GameObject CreateCharacterSelectPanel(Transform parent)
        {
            var charSelectPanelGo = UIComponentFactory.CreatePanel(parent, "CharacterSelectPanelGo");
            UIComponentFactory.SetRectTransformFill(charSelectPanelGo.GetComponent<RectTransform>());
            charSelectPanelGo.GetComponent<Image>().color = UIComponentFactory.PrimaryColor;
            charSelectPanelGo.SetActive(false);

            // Title
            var charSelectTitle = UIComponentFactory.CreateText(charSelectPanelGo.transform, "TitleText", "파티 선택 (0/3)", 36);
            var charSelectTitleRect = charSelectTitle.GetComponent<RectTransform>();
            charSelectTitleRect.anchorMin = new Vector2(0.5f, 0.9f);
            charSelectTitleRect.anchorMax = new Vector2(0.5f, 0.9f);
            charSelectTitleRect.sizeDelta = new Vector2(400, 60);

            // Character List Container
            var characterListContainer = UIComponentFactory.CreatePanel(charSelectPanelGo.transform, "CharacterListContainer");
            var charListRect = characterListContainer.GetComponent<RectTransform>();
            charListRect.anchorMin = new Vector2(0.1f, 0.4f);
            charListRect.anchorMax = new Vector2(0.9f, 0.8f);
            var charListLayout = characterListContainer.AddComponent<HorizontalLayoutGroup>();
            charListLayout.spacing = 20;
            charListLayout.childAlignment = TextAnchor.MiddleCenter;
            charListLayout.childForceExpandWidth = false;
            charListLayout.childForceExpandHeight = false;
            characterListContainer.GetComponent<Image>().color = UIComponentFactory.SecondaryColor;

            // Selected Slots Container
            var selectedSlotsContainer = UIComponentFactory.CreatePanel(charSelectPanelGo.transform, "SelectedSlotsContainer");
            var slotsRect = selectedSlotsContainer.GetComponent<RectTransform>();
            slotsRect.anchorMin = new Vector2(0.3f, 0.25f);
            slotsRect.anchorMax = new Vector2(0.7f, 0.35f);
            var slotsLayout = selectedSlotsContainer.AddComponent<HorizontalLayoutGroup>();
            slotsLayout.spacing = 15;
            slotsLayout.childAlignment = TextAnchor.MiddleCenter;
            slotsLayout.childForceExpandWidth = false;
            slotsLayout.childForceExpandHeight = false;
            selectedSlotsContainer.GetComponent<Image>().color = UIComponentFactory.SecondaryColor;

            // 선택 슬롯 3개 생성
            for (int i = 0; i < 3; i++)
            {
                UIComponentFactory.CreateSlotPanel(selectedSlotsContainer.transform, $"Slot_{i}");
            }

            // Info Text
            var infoText = UIComponentFactory.CreateText(charSelectPanelGo.transform, "InfoText", "파티원을 3명 선택하세요", 20);
            var infoTextRect = infoText.GetComponent<RectTransform>();
            infoTextRect.anchorMin = new Vector2(0.5f, 0.18f);
            infoTextRect.anchorMax = new Vector2(0.5f, 0.18f);
            infoTextRect.sizeDelta = new Vector2(400, 40);

            // Buttons
            var confirmBtn = UIComponentFactory.CreateButton(charSelectPanelGo.transform, "ConfirmButton", "확인", new Vector2(150, 50));
            UIComponentFactory.PositionButton(confirmBtn, 0.6f, 0.08f);

            var backBtn = UIComponentFactory.CreateButton(charSelectPanelGo.transform, "BackButton", "뒤로", new Vector2(150, 50));
            UIComponentFactory.PositionButton(backBtn, 0.4f, 0.08f);

            return charSelectPanelGo;
        }

        private GameObject CreateSettingsPanel(Transform parent)
        {
            var settingsPanel = UIComponentFactory.CreatePanel(parent, "SettingsPanel");
            UIComponentFactory.SetRectTransformFill(settingsPanel.GetComponent<RectTransform>());
            settingsPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            settingsPanel.SetActive(false);

            var settingsTitle = UIComponentFactory.CreateText(settingsPanel.transform, "TitleText", "설정", 36);
            var settingsTitleRect = settingsTitle.GetComponent<RectTransform>();
            settingsTitleRect.anchorMin = new Vector2(0.5f, 0.8f);
            settingsTitleRect.anchorMax = new Vector2(0.5f, 0.8f);
            settingsTitleRect.sizeDelta = new Vector2(300, 60);

            var settingsBackBtn = UIComponentFactory.CreateButton(settingsPanel.transform, "BackButton", "뒤로", new Vector2(200, 50));
            UIComponentFactory.PositionButton(settingsBackBtn, 0.5f, 0.15f);

            return settingsPanel;
        }

        private (MainMenuController, CharacterSelectPanel) CreateControllers(
            GameObject mainPanel, GameObject charSelectPanelGo, GameObject settingsPanel)
        {
            var menuControllerGo = new GameObject("MainMenuController");
            var menuController = menuControllerGo.AddComponent<MainMenuController>();

            var charSelectGo = new GameObject("CharacterSelectPanel");
            var charSelectPanel = charSelectGo.AddComponent<CharacterSelectPanel>();

            // MainMenuController SerializedFields
            UIComponentFactory.SetPrivateField(menuController, "_mainPanel", mainPanel);
            UIComponentFactory.SetPrivateField(menuController, "_characterSelectPanel", charSelectPanelGo);
            UIComponentFactory.SetPrivateField(menuController, "_settingsPanel", settingsPanel);
            UIComponentFactory.SetPrivateField(menuController, "_newRunButton",
                mainPanel.transform.Find("NewRunButton")?.GetComponent<Button>());
            UIComponentFactory.SetPrivateField(menuController, "_continueButton",
                mainPanel.transform.Find("ContinueButton")?.GetComponent<Button>());
            UIComponentFactory.SetPrivateField(menuController, "_settingsButton",
                mainPanel.transform.Find("SettingsButton")?.GetComponent<Button>());
            UIComponentFactory.SetPrivateField(menuController, "_quitButton",
                mainPanel.transform.Find("QuitButton")?.GetComponent<Button>());
            UIComponentFactory.SetPrivateField(menuController, "_characterSelect", charSelectPanel);

            // CharacterSelectPanel SerializedFields
            var characterListContainer = charSelectPanelGo.transform.Find("CharacterListContainer");
            var selectedSlotsContainer = charSelectPanelGo.transform.Find("SelectedSlotsContainer");
            var confirmBtn = charSelectPanelGo.transform.Find("ConfirmButton");
            var backBtn = charSelectPanelGo.transform.Find("BackButton");
            var infoText = charSelectPanelGo.transform.Find("InfoText");

            UIComponentFactory.SetPrivateField(charSelectPanel, "_characterListContainer", characterListContainer);
            UIComponentFactory.SetPrivateField(charSelectPanel, "_selectedSlotsContainer", selectedSlotsContainer);
            UIComponentFactory.SetPrivateField(charSelectPanel, "_confirmButton", confirmBtn?.GetComponent<Button>());
            UIComponentFactory.SetPrivateField(charSelectPanel, "_backButton", backBtn?.GetComponent<Button>());
            UIComponentFactory.SetPrivateField(charSelectPanel, "_infoText", infoText?.GetComponent<Text>());
            UIComponentFactory.SetPrivateField(charSelectPanel, "_menuController", menuController);

            return (menuController, charSelectPanel);
        }

        private void BindCharacterButtons(GameObject charSelectPanelGo, CharacterSelectPanel charSelectPanel)
        {
            var characterData = new[]
            {
                ("warrior_01", "검투사", new Color(0.8f, 0.3f, 0.3f, 1f)),
                ("mage_01", "마법사", new Color(0.3f, 0.3f, 0.8f, 1f)),
                ("rogue_01", "암살자", new Color(0.3f, 0.8f, 0.3f, 1f))
            };

            var characterListContainer = charSelectPanelGo.transform.Find("CharacterListContainer");

            foreach (var (id, charName, color) in characterData)
            {
                var charBtn = UIComponentFactory.CreateCharacterButton(characterListContainer, id, charName, color);
                var button = charBtn.GetComponent<Button>();

                // Persistent listener로 ToggleCharacter(id) 연결
                UnityAction<string> action = charSelectPanel.ToggleCharacter;
                UnityEventTools.AddStringPersistentListener(button.onClick, action, id);
            }
        }
    }
}
