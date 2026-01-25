using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using ProjectSS.Data;

namespace ProjectSS.Editor
{
    /// <summary>
    /// 프로젝트 에셋 자동 생성기
    /// Project asset auto-generator
    /// </summary>
    public static class ProjectGenerator
    {
        private const string PROJECT_PATH = "Assets/_Project";
        private const string DATA_PATH = "Assets/_Project/Data";
        private const string SCENES_PATH = "Assets/_Project/Scenes";
        private const string PREFABS_PATH = "Assets/_Project/Prefabs";

        #region Menu Items

        [MenuItem("Tools/Project SS/Generate All %&a")] // Ctrl+Alt+A
        public static void GenerateAll()
        {
            if (!EditorUtility.DisplayDialog(
                "Generate All",
                "모든 에셋을 생성합니다.\n(기존 에셋은 덮어쓰지 않습니다)\n\n계속하시겠습니까?",
                "생성", "취소"))
            {
                return;
            }

            EditorUtility.DisplayProgressBar("Generating", "폴더 구조 생성 중...", 0.1f);
            GenerateFolders();

            EditorUtility.DisplayProgressBar("Generating", "씬 생성 중...", 0.3f);
            GenerateScenes();

            EditorUtility.DisplayProgressBar("Generating", "프리팹 생성 중...", 0.5f);
            GeneratePrefabs();

            EditorUtility.DisplayProgressBar("Generating", "스타터 데이터 생성 중...", 0.7f);
            GenerateStarterData();

            EditorUtility.DisplayProgressBar("Generating", "완료 중...", 0.9f);
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();

            EditorUtility.ClearProgressBar();
            Debug.Log("<color=green>✅ 전체 생성 완료!</color>");
            EditorUtility.DisplayDialog("완료", "모든 에셋이 생성되었습니다!", "확인");
        }

        [MenuItem("Tools/Project SS/Generate Folders")]
        public static void GenerateFolders()
        {
            CreateFolder("Assets/_Project");
            CreateFolder("Assets/_Project/Art");
            CreateFolder("Assets/_Project/Art/Sprites");
            CreateFolder("Assets/_Project/Art/Sprites/Cards");
            CreateFolder("Assets/_Project/Art/Sprites/Enemies");
            CreateFolder("Assets/_Project/Art/Sprites/UI");
            CreateFolder("Assets/_Project/Audio");
            CreateFolder("Assets/_Project/Audio/BGM");
            CreateFolder("Assets/_Project/Audio/SFX");
            CreateFolder("Assets/_Project/Data");
            CreateFolder("Assets/_Project/Data/Cards");
            CreateFolder("Assets/_Project/Data/Cards/Attack");
            CreateFolder("Assets/_Project/Data/Cards/Defense");
            CreateFolder("Assets/_Project/Data/Cards/Skill");
            CreateFolder("Assets/_Project/Data/Enemies");
            CreateFolder("Assets/_Project/Data/Enemies/Normal");
            CreateFolder("Assets/_Project/Data/Enemies/Elite");
            CreateFolder("Assets/_Project/Data/Enemies/Boss");
            CreateFolder("Assets/_Project/Data/StatusEffects");
            CreateFolder("Assets/_Project/Data/Relics");
            CreateFolder("Assets/_Project/Data/Events");
            CreateFolder("Assets/_Project/Data/Map");
            CreateFolder("Assets/_Project/Prefabs");
            CreateFolder("Assets/_Project/Prefabs/UI");
            CreateFolder("Assets/_Project/Prefabs/Managers");
            CreateFolder("Assets/_Project/Prefabs/Combat");
            CreateFolder("Assets/_Project/Scenes");
            CreateFolder("Assets/_Project/Settings");

            AssetDatabase.Refresh();
            Debug.Log("✅ 폴더 구조 생성 완료");
        }

        [MenuItem("Tools/Project SS/Generate Scenes")]
        public static void GenerateScenes()
        {
            string[] sceneNames = { "Boot", "MainMenu", "Map", "Combat", "Event", "Shop", "Rest" };

            foreach (var sceneName in sceneNames)
            {
                CreateScene(sceneName);
            }

            AssetDatabase.Refresh();
            Debug.Log("✅ 씬 생성 완료");
        }

        [MenuItem("Tools/Project SS/Generate Prefabs")]
        public static void GeneratePrefabs()
        {
            // UI Prefabs
            CreateCardPrefab();
            CreateEnemyPrefab();
            CreateHealthBarPrefab();
            CreateIntentPrefab();
            CreateMapNodePrefab();

            // Manager Prefabs
            CreateGameManagerPrefab();

            AssetDatabase.Refresh();
            Debug.Log("✅ 프리팹 생성 완료");
        }

        [MenuItem("Tools/Project SS/Generate Starter Data")]
        public static void GenerateStarterData()
        {
            GenerateStatusEffects();
            GenerateStarterCards();
            GenerateStarterEnemies();
            GenerateMapConfig();

            AssetDatabase.Refresh();
            Debug.Log("✅ 스타터 데이터 생성 완료");
        }

        #endregion

        #region Helper Methods

        private static void CreateFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static void CreateScene(string sceneName)
        {
            string scenePath = $"{SCENES_PATH}/{sceneName}.unity";

            if (File.Exists(scenePath))
            {
                Debug.Log($"씬 이미 존재: {sceneName}");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // 씬별 기본 설정
            switch (sceneName)
            {
                case "Boot":
                    SetupBootScene();
                    break;
                case "MainMenu":
                    SetupMainMenuScene();
                    break;
                case "Combat":
                    SetupCombatScene();
                    break;
                case "Map":
                    SetupMapScene();
                    break;
            }

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"씬 생성: {sceneName}");
        }

        private static void SetupBootScene()
        {
            // BootLoader 오브젝트 생성
            var bootLoader = new GameObject("BootLoader");
            bootLoader.AddComponent<ProjectSS.Core.BootLoader>();

            // GameManager 프리팹 연결 (나중에 수동으로 해야 함)
            Debug.Log("[Boot 씬] BootLoader 생성됨. GameManager 프리팹을 수동으로 연결하세요.");
        }

        private static void SetupMainMenuScene()
        {
            // Canvas 생성
            var canvas = CreateCanvas("MainMenuCanvas");

            // 타이틀 텍스트
            var titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(canvas.transform);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 150);
            titleRect.sizeDelta = new Vector2(600, 100);
            var titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
            titleText.text = "Project SS";
            titleText.fontSize = 72;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;

            // 버튼들
            CreateButton(canvas.transform, "NewGameButton", "New Game", new Vector2(0, 0));
            CreateButton(canvas.transform, "ContinueButton", "Continue", new Vector2(0, -80));
            CreateButton(canvas.transform, "QuitButton", "Quit", new Vector2(0, -160));
        }

        private static void SetupCombatScene()
        {
            var canvas = CreateCanvas("CombatCanvas");

            // Player Area
            var playerArea = new GameObject("PlayerArea");
            playerArea.transform.SetParent(canvas.transform);
            var playerRect = playerArea.AddComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0, 0);
            playerRect.anchorMax = new Vector2(0.3f, 0.5f);
            playerRect.offsetMin = Vector2.zero;
            playerRect.offsetMax = Vector2.zero;

            // Enemy Area
            var enemyArea = new GameObject("EnemyArea");
            enemyArea.transform.SetParent(canvas.transform);
            var enemyRect = enemyArea.AddComponent<RectTransform>();
            enemyRect.anchorMin = new Vector2(0.5f, 0.4f);
            enemyRect.anchorMax = new Vector2(1f, 1f);
            enemyRect.offsetMin = Vector2.zero;
            enemyRect.offsetMax = Vector2.zero;

            // Hand Area
            var handArea = new GameObject("HandArea");
            handArea.transform.SetParent(canvas.transform);
            var handRect = handArea.AddComponent<RectTransform>();
            handRect.anchorMin = new Vector2(0.1f, 0);
            handRect.anchorMax = new Vector2(0.9f, 0.25f);
            handRect.offsetMin = Vector2.zero;
            handRect.offsetMax = Vector2.zero;

            // Energy Display
            var energyDisplay = new GameObject("EnergyDisplay");
            energyDisplay.transform.SetParent(canvas.transform);
            var energyRect = energyDisplay.AddComponent<RectTransform>();
            energyRect.anchorMin = new Vector2(0, 0);
            energyRect.anchorMax = new Vector2(0.1f, 0.15f);
            energyRect.offsetMin = Vector2.zero;
            energyRect.offsetMax = Vector2.zero;

            // End Turn Button
            CreateButton(canvas.transform, "EndTurnButton", "End Turn", new Vector2(400, -300));
        }

        private static void SetupMapScene()
        {
            var canvas = CreateCanvas("MapCanvas");

            // Map Container
            var mapContainer = new GameObject("MapContainer");
            mapContainer.transform.SetParent(canvas.transform);
            var mapRect = mapContainer.AddComponent<RectTransform>();
            mapRect.anchorMin = Vector2.zero;
            mapRect.anchorMax = Vector2.one;
            mapRect.offsetMin = new Vector2(50, 50);
            mapRect.offsetMax = new Vector2(-50, -50);

            // Scroll View
            var scrollView = new GameObject("MapScrollView");
            scrollView.transform.SetParent(mapContainer.transform);
            var scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            scrollView.AddComponent<UnityEngine.UI.ScrollRect>();
        }

        private static GameObject CreateCanvas(string name)
        {
            var canvasObj = new GameObject(name);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            return canvasObj;
        }

        private static void CreateButton(Transform parent, string name, string text, Vector2 position)
        {
            var buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);

            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(200, 50);

            var image = buttonObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f);

            buttonObj.AddComponent<UnityEngine.UI.Button>();

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var buttonText = textObj.AddComponent<UnityEngine.UI.Text>();
            buttonText.text = text;
            buttonText.fontSize = 24;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
        }

        #endregion

        #region Prefab Creation

        private static void CreateCardPrefab()
        {
            string path = $"{PREFABS_PATH}/UI/CardPrefab.prefab";
            if (File.Exists(path)) return;

            var cardObj = new GameObject("CardPrefab");

            // RectTransform
            var rect = cardObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 200);

            // Background
            var bg = cardObj.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0.9f, 0.85f, 0.7f);

            // CardUI component (연결은 수동으로)
            // cardObj.AddComponent<ProjectSS.UI.CardUI>();

            // Cost text
            var costObj = CreateTextChild(cardObj.transform, "CostText", new Vector2(-55, 80), new Vector2(40, 40), "1");

            // Name text
            var nameObj = CreateTextChild(cardObj.transform, "NameText", new Vector2(0, 60), new Vector2(130, 30), "Card Name");

            // Description text
            var descObj = CreateTextChild(cardObj.transform, "DescriptionText", new Vector2(0, -40), new Vector2(130, 80), "Description");

            // Type text
            var typeObj = CreateTextChild(cardObj.transform, "TypeText", new Vector2(0, -85), new Vector2(130, 20), "Attack");

            PrefabUtility.SaveAsPrefabAsset(cardObj, path);
            Object.DestroyImmediate(cardObj);
            Debug.Log("프리팹 생성: CardPrefab");
        }

        private static void CreateEnemyPrefab()
        {
            string path = $"{PREFABS_PATH}/UI/EnemyPrefab.prefab";
            if (File.Exists(path)) return;

            var enemyObj = new GameObject("EnemyPrefab");
            var rect = enemyObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 200);

            // Enemy sprite
            var spriteObj = new GameObject("Sprite");
            spriteObj.transform.SetParent(enemyObj.transform);
            var spriteRect = spriteObj.AddComponent<RectTransform>();
            spriteRect.anchoredPosition = Vector2.zero;
            spriteRect.sizeDelta = new Vector2(100, 100);
            var sprite = spriteObj.AddComponent<UnityEngine.UI.Image>();
            sprite.color = Color.red;

            // Intent
            var intentObj = new GameObject("Intent");
            intentObj.transform.SetParent(enemyObj.transform);
            var intentRect = intentObj.AddComponent<RectTransform>();
            intentRect.anchoredPosition = new Vector2(0, 80);
            intentRect.sizeDelta = new Vector2(50, 50);

            // Health bar placeholder
            var healthObj = new GameObject("HealthBar");
            healthObj.transform.SetParent(enemyObj.transform);
            var healthRect = healthObj.AddComponent<RectTransform>();
            healthRect.anchoredPosition = new Vector2(0, -70);
            healthRect.sizeDelta = new Vector2(100, 15);

            PrefabUtility.SaveAsPrefabAsset(enemyObj, path);
            Object.DestroyImmediate(enemyObj);
            Debug.Log("프리팹 생성: EnemyPrefab");
        }

        private static void CreateHealthBarPrefab()
        {
            string path = $"{PREFABS_PATH}/UI/HealthBarPrefab.prefab";
            if (File.Exists(path)) return;

            var healthObj = new GameObject("HealthBarPrefab");
            var rect = healthObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 25);

            // Background
            var bgImage = healthObj.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);

            // Fill
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(healthObj.transform);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(1, 1);
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);
            var fillImage = fillObj.AddComponent<UnityEngine.UI.Image>();
            fillImage.color = new Color(0.8f, 0.2f, 0.2f);

            // Text
            CreateTextChild(healthObj.transform, "HealthText", Vector2.zero, new Vector2(100, 25), "50/50");

            PrefabUtility.SaveAsPrefabAsset(healthObj, path);
            Object.DestroyImmediate(healthObj);
            Debug.Log("프리팹 생성: HealthBarPrefab");
        }

        private static void CreateIntentPrefab()
        {
            string path = $"{PREFABS_PATH}/UI/IntentPrefab.prefab";
            if (File.Exists(path)) return;

            var intentObj = new GameObject("IntentPrefab");
            var rect = intentObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60, 60);

            // Icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(intentObj.transform);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchoredPosition = new Vector2(0, 10);
            iconRect.sizeDelta = new Vector2(40, 40);
            var iconImage = iconObj.AddComponent<UnityEngine.UI.Image>();
            iconImage.color = Color.red;

            // Value text
            CreateTextChild(intentObj.transform, "ValueText", new Vector2(0, -20), new Vector2(50, 20), "12");

            PrefabUtility.SaveAsPrefabAsset(intentObj, path);
            Object.DestroyImmediate(intentObj);
            Debug.Log("프리팹 생성: IntentPrefab");
        }

        private static void CreateMapNodePrefab()
        {
            string path = $"{PREFABS_PATH}/UI/MapNodePrefab.prefab";
            if (File.Exists(path)) return;

            var nodeObj = new GameObject("MapNodePrefab");
            var rect = nodeObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(60, 60);

            // Button
            var image = nodeObj.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f);
            nodeObj.AddComponent<UnityEngine.UI.Button>();

            // Icon
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(nodeObj.transform);
            var iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(10, 10);
            iconRect.offsetMax = new Vector2(-10, -10);
            var iconImage = iconObj.AddComponent<UnityEngine.UI.Image>();
            iconImage.color = Color.white;

            PrefabUtility.SaveAsPrefabAsset(nodeObj, path);
            Object.DestroyImmediate(nodeObj);
            Debug.Log("프리팹 생성: MapNodePrefab");
        }

        private static void CreateGameManagerPrefab()
        {
            string path = $"{PREFABS_PATH}/Managers/GameManager.prefab";
            if (File.Exists(path)) return;

            var gmObj = new GameObject("GameManager");
            // GameManager 컴포넌트는 수동으로 추가해야 함
            // gmObj.AddComponent<ProjectSS.Core.GameManager>();

            PrefabUtility.SaveAsPrefabAsset(gmObj, path);
            Object.DestroyImmediate(gmObj);
            Debug.Log("프리팹 생성: GameManager");
        }

        private static GameObject CreateTextChild(Transform parent, string name, Vector2 position, Vector2 size, string text)
        {
            var textObj = new GameObject(name);
            textObj.transform.SetParent(parent);

            var rect = textObj.AddComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var textComp = textObj.AddComponent<UnityEngine.UI.Text>();
            textComp.text = text;
            textComp.fontSize = 14;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.black;

            return textObj;
        }

        #endregion

        #region Data Generation

        private static void GenerateStatusEffects()
        {
            // Strength
            CreateStatusEffect("STF_힘_Strength", "힘", "Strength",
                "공격 시 데미지 +{0}\nDeal +{0} damage",
                StatusEffectType.Strength, false, true);

            // Weak
            CreateStatusEffect("STF_약화_Weak", "약화", "Weak",
                "공격 데미지 25% 감소\nDeal 25% less damage",
                StatusEffectType.Weak, true, false);

            // Vulnerable
            CreateStatusEffect("STF_취약_Vulnerable", "취약", "Vulnerable",
                "받는 데미지 50% 증가\nTake 50% more damage",
                StatusEffectType.Vulnerable, true, false);

            // Dexterity
            CreateStatusEffect("STF_민첩_Dexterity", "민첩", "Dexterity",
                "블록 획득 시 +{0}\nGain +{0} Block",
                StatusEffectType.Dexterity, false, true);
        }

        private static void CreateStatusEffect(string fileName, string nameKo, string nameEn, string description,
            StatusEffectType effectType, bool isDebuff, bool stackable)
        {
            string path = $"{DATA_PATH}/StatusEffects/{fileName}.asset";
            if (File.Exists(path)) return;

            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = fileName;
            effect.statusName = $"{nameKo} ({nameEn})";
            effect.description = description;
            effect.effectType = effectType;
            effect.isDebuff = isDebuff;
            effect.stackable = stackable;
            effect.triggerTime = StatusTrigger.TurnStart;

            AssetDatabase.CreateAsset(effect, path);
            Debug.Log($"상태이상 생성: {fileName}");
        }

        private static void GenerateStarterCards()
        {
            // Strike - 공격 카드
            CreateCard("ATK_강타_Strike", "강타", "Strike",
                "Deal 6 damage.\n6 데미지를 줍니다.",
                CardType.Attack, CardRarity.Starter, 1,
                new List<CardEffect> { new CardEffect { effectType = CardEffectType.Damage, value = 6 } });

            // Defend - 방어 카드
            CreateCard("DEF_방어_Defend", "방어", "Defend",
                "Gain 5 Block.\n5 블록을 얻습니다.",
                CardType.Defense, CardRarity.Starter, 1,
                new List<CardEffect> { new CardEffect { effectType = CardEffectType.Block, value = 5 } });

            // Bash - 특수 공격
            CreateCard("ATK_강타_Bash", "강타", "Bash",
                "Deal 8 damage. Apply 2 Vulnerable.\n8 데미지를 주고 취약 2를 부여합니다.",
                CardType.Attack, CardRarity.Starter, 2,
                new List<CardEffect>
                {
                    new CardEffect { effectType = CardEffectType.Damage, value = 8 },
                    new CardEffect { effectType = CardEffectType.ApplyStatus, statusStacks = 2 }
                });
        }

        private static void CreateCard(string fileName, string nameKo, string nameEn, string description,
            CardType cardType, CardRarity rarity, int cost, List<CardEffect> effects)
        {
            string subFolder = cardType == CardType.Attack ? "Attack" :
                              cardType == CardType.Defense ? "Defense" : "Skill";
            string path = $"{DATA_PATH}/Cards/{subFolder}/{fileName}.asset";
            if (File.Exists(path)) return;

            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = fileName;
            card.cardName = $"{nameKo} ({nameEn})";
            card.description = description;
            card.cardType = cardType;
            card.rarity = rarity;
            card.energyCost = cost;
            card.effects = effects;

            AssetDatabase.CreateAsset(card, path);
            Debug.Log($"카드 생성: {fileName}");
        }

        private static void GenerateStarterEnemies()
        {
            // Slime - 기본 적
            CreateEnemy("EN_슬라임_Slime", "슬라임", "Slime",
                EnemyType.Normal, 12, 16, 5, 10,
                new List<EnemyAction>
                {
                    new EnemyAction
                    {
                        actionName = "Tackle",
                        intentType = EnemyIntentType.Attack,
                        damage = 5,
                        hitCount = 1
                    }
                });

            // Cultist - 의식 적
            CreateEnemy("EN_광신도_Cultist", "광신도", "Cultist",
                EnemyType.Normal, 48, 54, 10, 15,
                new List<EnemyAction>
                {
                    new EnemyAction
                    {
                        actionName = "Dark Strike",
                        intentType = EnemyIntentType.Attack,
                        damage = 6,
                        hitCount = 1
                    },
                    new EnemyAction
                    {
                        actionName = "Incantation",
                        intentType = EnemyIntentType.Buff,
                        statusStacks = 3
                    }
                });

            // Lagavulin - 엘리트
            CreateEnemy("ELITE_라가불린_Lagavulin", "라가불린", "Lagavulin",
                EnemyType.Elite, 109, 111, 30, 50,
                new List<EnemyAction>
                {
                    new EnemyAction
                    {
                        actionName = "Attack",
                        intentType = EnemyIntentType.Attack,
                        damage = 18,
                        hitCount = 1
                    },
                    new EnemyAction
                    {
                        actionName = "Siphon Soul",
                        intentType = EnemyIntentType.Debuff,
                        statusStacks = 2
                    }
                });
        }

        private static void CreateEnemy(string fileName, string nameKo, string nameEn,
            EnemyType enemyType, int minHp, int maxHp, int minGold, int maxGold,
            List<EnemyAction> actions)
        {
            string subFolder = enemyType == EnemyType.Normal ? "Normal" :
                              enemyType == EnemyType.Elite ? "Elite" : "Boss";
            string path = $"{DATA_PATH}/Enemies/{subFolder}/{fileName}.asset";
            if (File.Exists(path)) return;

            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.enemyId = fileName;
            enemy.enemyName = $"{nameKo} ({nameEn})";
            enemy.enemyType = enemyType;
            enemy.minHealth = minHp;
            enemy.maxHealth = maxHp;
            enemy.goldReward = minGold;
            enemy.actionPattern = actions;

            AssetDatabase.CreateAsset(enemy, path);
            Debug.Log($"적 생성: {fileName}");
        }

        private static void GenerateMapConfig()
        {
            string path = $"{DATA_PATH}/Map/MapConfig_Act1.asset";
            if (File.Exists(path)) return;

            var config = ScriptableObject.CreateInstance<MapGenerationConfig>();
            config.numberOfFloors = 15;
            config.minNodesPerFloor = 3;
            config.maxNodesPerFloor = 4;

            AssetDatabase.CreateAsset(config, path);
            Debug.Log("맵 설정 생성: MapConfig_Act1");
        }

        #endregion
    }
}
