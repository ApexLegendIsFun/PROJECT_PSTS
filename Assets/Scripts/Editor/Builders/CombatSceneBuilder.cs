// Editor/Builders/CombatSceneBuilder.cs
// Combat 씬 빌더 (Combat Scene Builder) - New Layout

using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using ProjectSS.Combat;
using ProjectSS.Combat.UI;

namespace ProjectSS.Editor
{
    /// <summary>
    /// Combat 씬 생성 빌더 - 새 레이아웃
    /// - TopPanel: 보스 체력바 (보스전 전용)
    /// - CombatArea: PartyArea (전방/후방) + EnemyArea
    /// - BottomPanel: DrawPile + CardHand + DiscardPile
    /// </summary>
    public class CombatSceneBuilder : ISceneBuilder
    {
        public string SceneName => "Combat";

        public string Build(string scenesFolder)
        {
            string scenePath = $"{scenesFolder}/{SceneName}.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = SceneName;

            UIComponentFactory.CreateEventSystem();
            var canvas = UIComponentFactory.CreateCanvas("CombatCanvas");

            // Top Panel - 보스 체력바 (보스전 전용)
            var topPanel = CreateTopPanel(canvas.transform);

            // Combat Area - 전투 영역
            var combatArea = CreateCombatArea(canvas.transform);
            var partyArea = CreatePartyArea(combatArea.transform);
            var enemyArea = CreateEnemyArea(combatArea.transform);

            // Text Effect Area (연출용 공간)
            CreateTextEffectArea(partyArea.transform);

            // Bottom Panel - 카드 핸드 영역
            var bottomPanel = CreateBottomPanel(canvas.transform);
            var drawPileUI = CreateDrawPileUI(bottomPanel.transform);
            var cardHandArea = CreateCardHandArea(bottomPanel.transform);
            var discardPileUI = CreateDiscardPileUI(bottomPanel.transform);
            var energyDisplay = CreateEnergyDisplay(bottomPanel.transform);

            // Managers & Controllers
            CreateManagers(topPanel, partyArea, enemyArea, cardHandArea, drawPileUI, discardPileUI, energyDisplay);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[CombatSceneBuilder] Combat 씬 생성 (새 레이아웃): {scenePath}");
            return scenePath;
        }

        #region Top Panel (Boss Health Bar)

        private GameObject CreateTopPanel(Transform parent)
        {
            var topPanel = UIComponentFactory.CreatePanel(parent, "TopPanel");
            var topRect = topPanel.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 0.92f);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.offsetMin = Vector2.zero;
            topRect.offsetMax = Vector2.zero;
            topPanel.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Boss Health Bar Panel (보스전에만 활성화)
            var bossHealthPanel = UIComponentFactory.CreatePanel(topPanel.transform, "BossHealthPanel");
            var bossRect = bossHealthPanel.GetComponent<RectTransform>();
            bossRect.anchorMin = new Vector2(0.20f, 0.10f);
            bossRect.anchorMax = new Vector2(0.80f, 0.90f);
            bossRect.offsetMin = Vector2.zero;
            bossRect.offsetMax = Vector2.zero;
            bossHealthPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0);

            // Add BossHealthBarUI component
            var bossHealthBarUI = bossHealthPanel.AddComponent<BossHealthBarUI>();
            bossHealthBarUI.CreateSimpleUI();
            bossHealthPanel.SetActive(false); // 기본값 비활성화

            return topPanel;
        }

        #endregion

        #region Combat Area

        private GameObject CreateCombatArea(Transform parent)
        {
            var combatArea = UIComponentFactory.CreatePanel(parent, "CombatArea");
            var combatRect = combatArea.GetComponent<RectTransform>();
            combatRect.anchorMin = new Vector2(0, 0.22f);
            combatRect.anchorMax = new Vector2(1, 0.92f);
            combatRect.offsetMin = Vector2.zero;
            combatRect.offsetMax = Vector2.zero;
            combatArea.GetComponent<Image>().color = new Color(0, 0, 0, 0); // 투명

            return combatArea;
        }

        private GameObject CreatePartyArea(Transform parent)
        {
            var partyArea = UIComponentFactory.CreatePanel(parent, "PartyArea");
            var partyRect = partyArea.GetComponent<RectTransform>();
            partyRect.anchorMin = new Vector2(0, 0);
            partyRect.anchorMax = new Vector2(0.55f, 1);
            partyRect.offsetMin = Vector2.zero;
            partyRect.offsetMax = Vector2.zero;
            partyArea.GetComponent<Image>().color = new Color(0, 0, 0, 0); // 투명

            // Party Container (캐릭터 배치 영역)
            var partyContainer = new GameObject("PartyContainer");
            partyContainer.transform.SetParent(partyArea.transform, false);
            var containerRect = partyContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.15f, 0.15f);
            containerRect.anchorMax = new Vector2(0.95f, 0.85f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Add PartyFormationUI
            var formationUI = partyArea.AddComponent<PartyFormationUI>();
            UIComponentFactory.SetPrivateField(formationUI, "_partyContainer", partyContainer.transform);

            return partyArea;
        }

        private void CreateTextEffectArea(Transform parent)
        {
            // 텍스트 연출 공간 (좌측)
            var textEffectArea = UIComponentFactory.CreatePanel(parent, "TextEffectArea");
            var textRect = textEffectArea.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0.3f);
            textRect.anchorMax = new Vector2(0.15f, 0.9f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            textEffectArea.GetComponent<Image>().color = new Color(0, 0, 0, 0); // 투명
        }

        private GameObject CreateEnemyArea(Transform parent)
        {
            var enemyArea = UIComponentFactory.CreatePanel(parent, "EnemyArea");
            var enemyRect = enemyArea.GetComponent<RectTransform>();
            enemyRect.anchorMin = new Vector2(0.55f, 0);
            enemyRect.anchorMax = new Vector2(1, 1);
            enemyRect.offsetMin = Vector2.zero;
            enemyRect.offsetMax = Vector2.zero;
            enemyArea.GetComponent<Image>().color = new Color(0, 0, 0, 0); // 투명

            // Enemy Container
            var enemyContainer = new GameObject("EnemyContainer");
            enemyContainer.transform.SetParent(enemyArea.transform, false);
            var containerRect = enemyContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.05f, 0.10f);
            containerRect.anchorMax = new Vector2(0.95f, 0.90f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            var layout = enemyContainer.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 20;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            return enemyArea;
        }

        #endregion

        #region Bottom Panel (Card Hand)

        private GameObject CreateBottomPanel(Transform parent)
        {
            var bottomPanel = UIComponentFactory.CreatePanel(parent, "BottomPanel");
            var bottomRect = bottomPanel.GetComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0.22f);
            bottomRect.offsetMin = Vector2.zero;
            bottomRect.offsetMax = Vector2.zero;
            bottomPanel.GetComponent<Image>().color = new Color(0.12f, 0.15f, 0.2f, 0.95f);

            return bottomPanel;
        }

        private GameObject CreateDrawPileUI(Transform parent)
        {
            var drawPile = new GameObject("DrawPileUI");
            drawPile.transform.SetParent(parent, false);
            var drawRect = drawPile.AddComponent<RectTransform>();
            drawRect.anchorMin = new Vector2(0.02f, 0.15f);
            drawRect.anchorMax = new Vector2(0.10f, 0.85f);
            drawRect.offsetMin = Vector2.zero;
            drawRect.offsetMax = Vector2.zero;

            var deckPileUI = drawPile.AddComponent<DeckPileUI>();
            UIComponentFactory.SetPrivateField(deckPileUI, "_isDrawPile", true);
            deckPileUI.CreateSimpleUI();

            return drawPile;
        }

        private GameObject CreateCardHandArea(Transform parent)
        {
            var cardHandArea = UIComponentFactory.CreatePanel(parent, "CardHandArea");
            var cardHandRect = cardHandArea.GetComponent<RectTransform>();
            cardHandRect.anchorMin = new Vector2(0.12f, 0.08f);
            cardHandRect.anchorMax = new Vector2(0.88f, 0.92f);
            cardHandRect.offsetMin = Vector2.zero;
            cardHandRect.offsetMax = Vector2.zero;
            cardHandArea.GetComponent<Image>().color = new Color(0, 0, 0, 0); // 투명

            // Card Container (중앙)
            var cardContainer = new GameObject("CardContainer");
            cardContainer.transform.SetParent(cardHandArea.transform, false);
            var cardContainerRect = cardContainer.AddComponent<RectTransform>();
            cardContainerRect.anchorMin = new Vector2(0.1f, 0);
            cardContainerRect.anchorMax = new Vector2(0.9f, 1);
            cardContainerRect.offsetMin = Vector2.zero;
            cardContainerRect.offsetMax = Vector2.zero;

            return cardHandArea;
        }

        private GameObject CreateDiscardPileUI(Transform parent)
        {
            var discardPile = new GameObject("DiscardPileUI");
            discardPile.transform.SetParent(parent, false);
            var discardRect = discardPile.AddComponent<RectTransform>();
            discardRect.anchorMin = new Vector2(0.90f, 0.15f);
            discardRect.anchorMax = new Vector2(0.98f, 0.85f);
            discardRect.offsetMin = Vector2.zero;
            discardRect.offsetMax = Vector2.zero;

            var deckPileUI = discardPile.AddComponent<DeckPileUI>();
            UIComponentFactory.SetPrivateField(deckPileUI, "_isDrawPile", false);
            deckPileUI.CreateSimpleUI();

            return discardPile;
        }

        private GameObject CreateEnergyDisplay(Transform parent)
        {
            var energyDisplay = UIComponentFactory.CreatePanel(parent, "EnergyDisplay");
            var energyRect = energyDisplay.GetComponent<RectTransform>();
            energyRect.anchorMin = new Vector2(0.46f, 0.02f);
            energyRect.anchorMax = new Vector2(0.54f, 0.22f);
            energyRect.offsetMin = Vector2.zero;
            energyRect.offsetMax = Vector2.zero;
            energyDisplay.GetComponent<Image>().color = new Color(0.25f, 0.5f, 0.63f, 1f);

            var energyText = UIComponentFactory.CreateText(energyDisplay.transform, "EnergyText", "3", 28);
            energyText.GetComponent<Text>().fontStyle = FontStyle.Bold;
            energyText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            var textRect = energyText.GetComponent<RectTransform>();
            UIComponentFactory.SetRectTransformFill(textRect);

            return energyDisplay;
        }

        #endregion

        #region Managers

        private void CreateManagers(GameObject topPanel, GameObject partyArea, GameObject enemyArea,
            GameObject cardHandArea, GameObject drawPileUI, GameObject discardPileUI, GameObject energyDisplay)
        {
            // Combat Manager
            var combatManagerGo = new GameObject("CombatManager");
            var combatManager = combatManagerGo.AddComponent<CombatManager>();

            // Turn Manager
            var turnManagerGo = new GameObject("TurnManager");
            var turnManager = turnManagerGo.AddComponent<TurnManager>();
            UIComponentFactory.SetPrivateField(combatManager, "_turnManager", turnManager);

            // Combat Scene Initializer
            var initializerGo = new GameObject("CombatSceneInitializer");
            var initializer = initializerGo.AddComponent<CombatSceneInitializer>();
            CardAssetGenerator.LinkToInitializer(initializer);
            EnemyAssetGenerator.LinkToInitializer(initializer);

            // Combat UI Controller
            var uiControllerGo = new GameObject("CombatUIController");
            var uiController = uiControllerGo.AddComponent<CombatUIController>();

            // Get references
            var bossHealthPanel = topPanel.transform.Find("BossHealthPanel");
            var partyContainer = partyArea.transform.Find("PartyContainer");
            var partyFormationUI = partyArea.GetComponent<PartyFormationUI>();
            var enemyContainer = enemyArea.transform.Find("EnemyContainer");
            var cardContainer = cardHandArea.transform.Find("CardContainer");
            var energyText = energyDisplay.transform.Find("EnergyText");

            // Connect references to UI Controller
            UIComponentFactory.SetPrivateField(uiController, "_topPanel", topPanel);
            UIComponentFactory.SetPrivateField(uiController, "_bossHealthPanel", bossHealthPanel?.gameObject);
            UIComponentFactory.SetPrivateField(uiController, "_partyArea", partyArea);
            UIComponentFactory.SetPrivateField(uiController, "_partyFormationUI", partyFormationUI);
            UIComponentFactory.SetPrivateField(uiController, "_partyContainer", partyContainer);
            UIComponentFactory.SetPrivateField(uiController, "_enemyArea", enemyArea);
            UIComponentFactory.SetPrivateField(uiController, "_enemyContainer", enemyContainer);
            UIComponentFactory.SetPrivateField(uiController, "_cardHandArea", cardHandArea);
            UIComponentFactory.SetPrivateField(uiController, "_cardContainer", cardContainer);
            UIComponentFactory.SetPrivateField(uiController, "_drawPileUI", drawPileUI.GetComponent<DeckPileUI>());
            UIComponentFactory.SetPrivateField(uiController, "_discardPileUI", discardPileUI.GetComponent<DeckPileUI>());
            UIComponentFactory.SetPrivateField(uiController, "_energyText", energyText?.GetComponent<Text>());

            // CardHandUI
            var cardHandUI = cardHandArea.AddComponent<CardHandUI>();
            UIComponentFactory.SetPrivateField(cardHandUI, "_cardContainer", cardContainer);
            UIComponentFactory.SetPrivateField(uiController, "_cardHandUI", cardHandUI);

            // TargetingSystem
            var targetingSystemGo = new GameObject("TargetingSystem");
            var targetingSystem = targetingSystemGo.AddComponent<TargetingSystem>();
            UIComponentFactory.SetPrivateField(uiController, "_targetingSystem", targetingSystem);

            // End Turn Button (floating at bottom right)
            CreateEndTurnButton(uiController, cardHandArea.transform.parent);
        }

        private void CreateEndTurnButton(CombatUIController uiController, Transform parent)
        {
            var endTurnBtn = UIComponentFactory.CreateButton(parent, "EndTurnButton", "턴 종료", new Vector2(100, 45));
            var btnRect = endTurnBtn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.88f, 0.05f);
            btnRect.anchorMax = new Vector2(0.88f, 0.05f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);

            UIComponentFactory.SetPrivateField(uiController, "_endTurnButton", endTurnBtn.GetComponent<Button>());
        }

        #endregion
    }
}
