// Editor/Builders/CombatSceneBuilder.cs
// Combat 씬 빌더 (Combat Scene Builder)

using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using ProjectSS.Combat;
using ProjectSS.Combat.UI;

namespace ProjectSS.Editor
{
    /// <summary>
    /// Combat 씬 생성 빌더
    /// - TopPanel (라운드, 턴 정보)
    /// - EnemyArea, PlayerArea (유닛 배치)
    /// - CardHandArea (카드 핸드)
    /// - CombatInfoPanel (에너지, 턴 종료 버튼)
    /// - CombatManager, TurnManager, CombatUIController 등 컴포넌트
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

            // Top Panel (라운드, 턴 정보)
            var topPanel = CreateTopPanel(canvas.transform);

            // Enemy Area
            var enemyArea = CreateEnemyArea(canvas.transform);

            // Player Area
            var playerArea = CreatePlayerArea(canvas.transform);

            // Card Hand Area
            var cardHandArea = CreateCardHandArea(canvas.transform);

            // Combat Info Panel (우측)
            var combatInfoPanel = CreateCombatInfoPanel(canvas.transform);

            // Combat Log Panel (좌측)
            CreateCombatLogPanel(canvas.transform);

            // Managers & Controllers
            CreateManagers(topPanel, enemyArea, playerArea, cardHandArea, combatInfoPanel);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[CombatSceneBuilder] Combat 씬 생성: {scenePath}");
            return scenePath;
        }

        private GameObject CreateTopPanel(Transform parent)
        {
            var topPanel = UIComponentFactory.CreatePanel(parent, "TopPanel");
            var topRect = topPanel.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 0.92f);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.offsetMin = Vector2.zero;
            topRect.offsetMax = Vector2.zero;
            topPanel.GetComponent<Image>().color = UIComponentFactory.SecondaryColor;

            var roundText = UIComponentFactory.CreateText(topPanel.transform, "RoundText", "라운드: 1", 24);
            var roundRect = roundText.GetComponent<RectTransform>();
            roundRect.anchorMin = new Vector2(0.05f, 0.5f);
            roundRect.anchorMax = new Vector2(0.2f, 0.5f);
            roundRect.sizeDelta = new Vector2(0, 40);

            var turnIndicator = UIComponentFactory.CreateText(topPanel.transform, "TurnIndicator", "플레이어 턴", 24);
            var turnRect = turnIndicator.GetComponent<RectTransform>();
            turnRect.anchorMin = new Vector2(0.4f, 0.5f);
            turnRect.anchorMax = new Vector2(0.6f, 0.5f);
            turnRect.sizeDelta = new Vector2(0, 40);

            return topPanel;
        }

        private GameObject CreateEnemyArea(Transform parent)
        {
            var enemyArea = UIComponentFactory.CreatePanel(parent, "EnemyArea");
            var enemyRect = enemyArea.GetComponent<RectTransform>();
            enemyRect.anchorMin = new Vector2(0.1f, 0.6f);
            enemyRect.anchorMax = new Vector2(0.9f, 0.9f);
            enemyRect.offsetMin = Vector2.zero;
            enemyRect.offsetMax = Vector2.zero;
            enemyArea.GetComponent<Image>().color = new Color(0.3f, 0.15f, 0.15f, 0.5f);

            var enemyContainer = new GameObject("EnemyContainer");
            enemyContainer.transform.SetParent(enemyArea.transform, false);
            var enemyContainerRect = enemyContainer.AddComponent<RectTransform>();
            UIComponentFactory.SetRectTransformFill(enemyContainerRect);
            var enemyLayout = enemyContainer.AddComponent<HorizontalLayoutGroup>();
            enemyLayout.childAlignment = TextAnchor.MiddleCenter;
            enemyLayout.spacing = 20;

            return enemyArea;
        }

        private GameObject CreatePlayerArea(Transform parent)
        {
            var playerArea = UIComponentFactory.CreatePanel(parent, "PlayerArea");
            var playerRect = playerArea.GetComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0.1f, 0.35f);
            playerRect.anchorMax = new Vector2(0.9f, 0.55f);
            playerRect.offsetMin = Vector2.zero;
            playerRect.offsetMax = Vector2.zero;
            playerArea.GetComponent<Image>().color = new Color(0.15f, 0.25f, 0.15f, 0.5f);

            var partyContainer = new GameObject("PartyContainer");
            partyContainer.transform.SetParent(playerArea.transform, false);
            var partyContainerRect = partyContainer.AddComponent<RectTransform>();
            UIComponentFactory.SetRectTransformFill(partyContainerRect);
            var partyLayout = partyContainer.AddComponent<HorizontalLayoutGroup>();
            partyLayout.childAlignment = TextAnchor.MiddleCenter;
            partyLayout.spacing = 30;

            return playerArea;
        }

        private GameObject CreateCardHandArea(Transform parent)
        {
            var cardHandArea = UIComponentFactory.CreatePanel(parent, "CardHandArea");
            var cardHandRect = cardHandArea.GetComponent<RectTransform>();
            cardHandRect.anchorMin = new Vector2(0.05f, 0.02f);
            cardHandRect.anchorMax = new Vector2(0.75f, 0.3f);
            cardHandRect.offsetMin = Vector2.zero;
            cardHandRect.offsetMax = Vector2.zero;
            cardHandArea.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.8f);

            var cardContainer = new GameObject("CardContainer");
            cardContainer.transform.SetParent(cardHandArea.transform, false);
            var cardContainerRect = cardContainer.AddComponent<RectTransform>();
            UIComponentFactory.SetRectTransformFill(cardContainerRect);
            var cardLayout = cardContainer.AddComponent<HorizontalLayoutGroup>();
            cardLayout.childAlignment = TextAnchor.MiddleCenter;
            cardLayout.spacing = -30; // 겹치는 카드 효과

            return cardHandArea;
        }

        private GameObject CreateCombatInfoPanel(Transform parent)
        {
            var combatInfoPanel = UIComponentFactory.CreatePanel(parent, "CombatInfoPanel");
            var combatInfoRect = combatInfoPanel.GetComponent<RectTransform>();
            combatInfoRect.anchorMin = new Vector2(0.8f, 0.02f);
            combatInfoRect.anchorMax = new Vector2(0.98f, 0.3f);
            combatInfoRect.offsetMin = Vector2.zero;
            combatInfoRect.offsetMax = Vector2.zero;
            combatInfoPanel.GetComponent<Image>().color = UIComponentFactory.SecondaryColor;

            var energyText = UIComponentFactory.CreateText(combatInfoPanel.transform, "EnergyText", "에너지: 3/3", 22);
            var energyRect = energyText.GetComponent<RectTransform>();
            energyRect.anchorMin = new Vector2(0.5f, 0.7f);
            energyRect.anchorMax = new Vector2(0.5f, 0.7f);
            energyRect.sizeDelta = new Vector2(150, 40);

            var endTurnBtn = UIComponentFactory.CreateButton(combatInfoPanel.transform, "EndTurnButton", "턴 종료", new Vector2(120, 50));
            var endTurnRect = endTurnBtn.GetComponent<RectTransform>();
            endTurnRect.anchorMin = new Vector2(0.5f, 0.3f);
            endTurnRect.anchorMax = new Vector2(0.5f, 0.3f);

            return combatInfoPanel;
        }

        private void CreateCombatLogPanel(Transform parent)
        {
            var combatLogPanel = UIComponentFactory.CreatePanel(parent, "CombatLogPanel");
            var logRect = combatLogPanel.GetComponent<RectTransform>();
            logRect.anchorMin = new Vector2(0.02f, 0.35f);
            logRect.anchorMax = new Vector2(0.08f, 0.55f);
            logRect.offsetMin = Vector2.zero;
            logRect.offsetMax = Vector2.zero;
            combatLogPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        }

        private void CreateManagers(GameObject topPanel, GameObject enemyArea, GameObject playerArea,
            GameObject cardHandArea, GameObject combatInfoPanel)
        {
            var combatManagerGo = new GameObject("CombatManager");
            var combatManager = combatManagerGo.AddComponent<CombatManager>();

            var turnManagerGo = new GameObject("TurnManager");
            var turnManager = turnManagerGo.AddComponent<TurnManager>();

            // CombatManager의 TurnManager 참조 연결
            UIComponentFactory.SetPrivateField(combatManager, "_turnManager", turnManager);

            // CombatSceneInitializer (Boot 씬 없이 직접 실행 지원)
            var initializerGo = new GameObject("CombatSceneInitializer");
            var initializer = initializerGo.AddComponent<CombatSceneInitializer>();

            // 카드풀 자동 연결
            CardAssetGenerator.LinkToInitializer(initializer);

            // Combat UI Controller
            var uiControllerGo = new GameObject("CombatUIController");
            var uiController = uiControllerGo.AddComponent<CombatUIController>();

            // UI 참조 연결
            var enemyContainer = enemyArea.transform.Find("EnemyContainer");
            var partyContainer = playerArea.transform.Find("PartyContainer");
            var cardContainer = cardHandArea.transform.Find("CardContainer");
            var roundText = topPanel.transform.Find("RoundText");
            var turnIndicator = topPanel.transform.Find("TurnIndicator");
            var energyText = combatInfoPanel.transform.Find("EnergyText");
            var endTurnBtn = combatInfoPanel.transform.Find("EndTurnButton");

            UIComponentFactory.SetPrivateField(uiController, "_topPanel", topPanel);
            UIComponentFactory.SetPrivateField(uiController, "_enemyArea", enemyArea);
            UIComponentFactory.SetPrivateField(uiController, "_playerArea", playerArea);
            UIComponentFactory.SetPrivateField(uiController, "_cardHandArea", cardHandArea);
            UIComponentFactory.SetPrivateField(uiController, "_combatInfoPanel", combatInfoPanel);
            UIComponentFactory.SetPrivateField(uiController, "_enemyContainer", enemyContainer);
            UIComponentFactory.SetPrivateField(uiController, "_partyContainer", partyContainer);
            UIComponentFactory.SetPrivateField(uiController, "_cardContainer", cardContainer);
            UIComponentFactory.SetPrivateField(uiController, "_roundText", roundText?.GetComponent<Text>());
            UIComponentFactory.SetPrivateField(uiController, "_turnIndicatorText", turnIndicator?.GetComponent<Text>());
            UIComponentFactory.SetPrivateField(uiController, "_energyText", energyText?.GetComponent<Text>());
            UIComponentFactory.SetPrivateField(uiController, "_endTurnButton", endTurnBtn?.GetComponent<Button>());

            // CardHandUI 추가
            var cardHandUI = cardHandArea.AddComponent<CardHandUI>();
            UIComponentFactory.SetPrivateField(cardHandUI, "_cardContainer", cardContainer);
            UIComponentFactory.SetPrivateField(uiController, "_cardHandUI", cardHandUI);

            // TargetingSystem 추가
            var targetingSystemGo = new GameObject("TargetingSystem");
            var targetingSystem = targetingSystemGo.AddComponent<TargetingSystem>();
            UIComponentFactory.SetPrivateField(uiController, "_targetingSystem", targetingSystem);
        }
    }
}
