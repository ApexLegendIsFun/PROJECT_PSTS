using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 게임 설정 마법사
    /// Game setup wizard
    ///
    /// 한 번의 클릭으로 모든 누락된 에셋과 설정을 완료합니다.
    /// Completes all missing assets and setup with one click.
    /// </summary>
    public static class GameSetupWizard
    {
        private const string SCENES_PATH = "Assets/_Project/Scenes";
        private const string BOOT_SCENE = "Assets/_Project/Scenes/Boot.unity";
        private const string MAINMENU_SCENE = "Assets/_Project/Scenes/MainMenu.unity";
        private const string MAP_SCENE = "Assets/_Project/Scenes/Map.unity";
        private const string COMBAT_SCENE = "Assets/_Project/Scenes/Combat.unity";

        // 빌드에 포함되어야 하는 씬 목록 (순서대로)
        // Scenes that must be included in build (in order)
        private static readonly string[] REQUIRED_SCENES = new string[]
        {
            "Assets/_Project/Scenes/Boot.unity",
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/Map.unity",
            "Assets/_Project/Scenes/Combat.unity",
            "Assets/_Project/Scenes/Event.unity",
            "Assets/_Project/Scenes/Shop.unity",
            "Assets/_Project/Scenes/Rest.unity",
            "Assets/_Project/Scenes/Reward.unity"
        };

        [MenuItem("Tools/Project SS/🎮 Complete Game Setup %&a", priority = 0)]
        public static void CompleteGameSetup()
        {
            if (!EditorUtility.DisplayDialog("게임 설정 마법사",
                "이 작업은 다음을 수행합니다:\n\n" +
                "1. 누락된 Status Effects 생성 (Frail, Poison, Regeneration)\n" +
                "2. TRIAD 캐릭터 클래스 생성 (Warrior, Mage, Rogue)\n" +
                "3. 스타터 카드 생성 (Skill 카드)\n" +
                "4. Act 1 적 생성 (Boss 포함)\n" +
                "5. Prefab 컴포넌트 설정 (GameManager 포함)\n" +
                "6. Resources 폴더 구성\n" +
                "7. 씬 컴포넌트 설정 (Boot, Combat)\n" +
                "8. Build Settings 씬 등록\n\n" +
                "계속하시겠습니까?",
                "시작", "취소"))
            {
                return;
            }

            StringBuilder report = new StringBuilder();
            report.AppendLine("=== 게임 설정 마법사 실행 결과 ===\n");

            try
            {
                // Phase 1: Status Effects
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "1/7: Status Effects 생성 중...", 0.1f);
                report.AppendLine("[Phase 1: Status Effects]");
                try
                {
                    StatusEffectGenerator.GenerateMissingStatusEffects();
                    report.AppendLine("✅ Status Effects 생성 완료");
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ Status Effects 생성 실패: {e.Message}");
                }

                // Phase 2: Character Classes
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "2/7: TRIAD 클래스 생성 중...", 0.2f);
                report.AppendLine("\n[Phase 2: Character Classes]");
                try
                {
                    CharacterClassGenerator.GenerateTriadClasses();
                    report.AppendLine("✅ TRIAD 클래스 생성 완료");
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ TRIAD 클래스 생성 실패: {e.Message}");
                }

                // Phase 3: Starter Cards
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "3/7: 스타터 카드 생성 중...", 0.35f);
                report.AppendLine("\n[Phase 3: Starter Cards]");
                try
                {
                    StarterCardGenerator.GenerateStarterCards();
                    report.AppendLine("✅ 스타터 카드 생성 완료");
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ 스타터 카드 생성 실패: {e.Message}");
                }

                // Phase 4: Enemies
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "4/7: Act 1 적 생성 중...", 0.5f);
                report.AppendLine("\n[Phase 4: Enemies]");
                try
                {
                    EnemyGenerator.GenerateAct1Enemies();
                    report.AppendLine("✅ Act 1 적 생성 완료");
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ Act 1 적 생성 실패: {e.Message}");
                }

                // Phase 5: Prefabs
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "5/7: Prefab 설정 중...", 0.6f);
                report.AppendLine("\n[Phase 5: Prefabs]");
                try
                {
                    PrefabSetupGenerator.SetupPrefabComponents();
                    report.AppendLine("✅ Prefab 설정 완료");
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ Prefab 설정 실패: {e.Message}");
                }

                // Phase 6: Resources
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "6/7: Resources 설정 중...", 0.75f);
                report.AppendLine("\n[Phase 6: Resources]");
                try
                {
                    ResourceSetupGenerator.SetupResourcesFolder();
                    report.AppendLine("✅ Resources 설정 완료");
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ Resources 설정 실패: {e.Message}");
                }

                // Phase 7: Scene Setup
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "7/8: 씬 설정 중...", 0.8f);
                report.AppendLine("\n[Phase 7: Scene Setup]");
                try
                {
                    SetupScenes();
                    report.AppendLine("✅ 씬 설정 완료");
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ 씬 설정 실패: {e.Message}");
                }

                // Phase 7.5: Create Missing Scenes
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "7.5/8: 누락된 씬 생성 중...", 0.85f);
                report.AppendLine("\n[Phase 7.5: Missing Scenes]");
                try
                {
                    ProjectSS.Editor.ProjectGenerator.GenerateScenes();
                    report.AppendLine("✅ 누락된 씬 생성 완료");
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ 씬 생성 실패: {e.Message}");
                }

                // Phase 8: Build Settings
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "8/8: Build Settings 설정 중...", 0.9f);
                report.AppendLine("\n[Phase 8: Build Settings]");
                try
                {
                    int addedScenes = SetupBuildSettings();
                    if (addedScenes > 0)
                    {
                        report.AppendLine($"✅ Build Settings 설정 완료 ({addedScenes}개 씬 추가)");
                    }
                    else
                    {
                        report.AppendLine("✅ Build Settings 이미 설정됨");
                    }
                }
                catch (System.Exception e)
                {
                    report.AppendLine($"⚠️ Build Settings 설정 실패: {e.Message}");
                }

                // Final refresh
                EditorUtility.DisplayProgressBar("게임 설정 마법사", "완료 중...", 0.95f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                report.AppendLine("\n=== 설정 완료 ===");
                report.AppendLine("\n다음 단계:");
                report.AppendLine("1. Unity에서 Boot 씬을 열고 플레이 버튼을 누르세요");
                report.AppendLine("2. 또는 MainMenu 씬에서 New Game을 클릭하세요");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log(report.ToString());

            EditorUtility.DisplayDialog("게임 설정 마법사 완료",
                "모든 설정이 완료되었습니다!\n\n" +
                "이제 Boot 씬을 열고 플레이 버튼을 눌러\n" +
                "게임을 테스트할 수 있습니다.\n\n" +
                "자세한 결과는 Console 창을 확인하세요.",
                "확인");
        }

        #region Build Settings

        /// <summary>
        /// Build Settings에 필요한 씬 등록
        /// Register required scenes to Build Settings
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup Build Settings")]
        public static int SetupBuildSettings()
        {
            var currentScenes = EditorBuildSettings.scenes.ToList();
            var currentPaths = currentScenes.Select(s => s.path).ToHashSet();
            int addedCount = 0;

            // 필요한 씬 확인 및 추가
            foreach (string scenePath in REQUIRED_SCENES)
            {
                if (!File.Exists(scenePath))
                {
                    Debug.LogWarning($"[GameSetupWizard] 씬 파일이 존재하지 않습니다: {scenePath}");
                    continue;
                }

                if (!currentPaths.Contains(scenePath))
                {
                    currentScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    addedCount++;
                    Debug.Log($"[GameSetupWizard] Build Settings에 씬 추가: {scenePath}");
                }
            }

            if (addedCount > 0)
            {
                // 씬 순서 재정렬 (REQUIRED_SCENES 순서대로)
                var orderedScenes = new List<EditorBuildSettingsScene>();

                // 먼저 필수 씬들을 순서대로 추가
                foreach (string requiredPath in REQUIRED_SCENES)
                {
                    var scene = currentScenes.FirstOrDefault(s => s.path == requiredPath);
                    if (scene != null)
                    {
                        orderedScenes.Add(scene);
                    }
                }

                // 그 다음 나머지 씬들 추가 (필수 씬이 아닌 것들)
                foreach (var scene in currentScenes)
                {
                    if (!REQUIRED_SCENES.Contains(scene.path))
                    {
                        orderedScenes.Add(scene);
                    }
                }

                EditorBuildSettings.scenes = orderedScenes.ToArray();
                Debug.Log($"<color=green>✅ Build Settings에 {addedCount}개 씬 추가 완료!</color>");
            }
            else
            {
                Debug.Log("[GameSetupWizard] 모든 필요한 씬이 이미 Build Settings에 등록되어 있습니다.");
            }

            return addedCount;
        }

        /// <summary>
        /// Build Settings 유효성 검사
        /// Validate Build Settings
        /// </summary>
        public static bool ValidateBuildSettings()
        {
            var currentPaths = EditorBuildSettings.scenes.Select(s => s.path).ToHashSet();

            foreach (string scenePath in REQUIRED_SCENES)
            {
                if (!currentPaths.Contains(scenePath))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Scene Setup

        /// <summary>
        /// 모든 필요한 씬 설정
        /// Setup all required scenes
        /// </summary>
        private static void SetupScenes()
        {
            SetupBootScene();
            SetupMainMenuScene();
            SetupMapScene();
            SetupCombatScene();
            SetupEventScene();
            SetupShopScene();
            SetupRestScene();
            SetupRewardScene();
        }

        /// <summary>
        /// Boot 씬에 BootLoader 컴포넌트 확인/추가
        /// Verify/add BootLoader component to Boot scene
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup Boot Scene")]
        public static void SetupBootScene()
        {
            if (!File.Exists(BOOT_SCENE))
            {
                Debug.LogWarning($"Boot 씬을 찾을 수 없습니다: {BOOT_SCENE}");
                return;
            }

            // 현재 씬 저장
            var currentScene = EditorSceneManager.GetActiveScene();
            bool sceneChanged = currentScene.path != BOOT_SCENE;

            if (sceneChanged)
            {
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }
                EditorSceneManager.OpenScene(BOOT_SCENE);
            }

            // BootLoader 찾기
            var bootLoader = Object.FindObjectOfType<ProjectSS.Core.BootLoader>();
            if (bootLoader == null)
            {
                // BootManager 오브젝트 찾기
                var bootManager = GameObject.Find("BootManager");
                if (bootManager == null)
                {
                    bootManager = new GameObject("BootManager");
                }

                bootLoader = bootManager.AddComponent<ProjectSS.Core.BootLoader>();
                Debug.Log("[GameSetupWizard] BootLoader 컴포넌트 추가됨");
            }
            else
            {
                Debug.Log("[GameSetupWizard] BootLoader가 이미 존재합니다");
            }

            // 씬 저장
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            // 원래 씬으로 복귀
            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }

        /// <summary>
        /// Combat 씬에 CombatSceneInitializer 컴포넌트 확인/추가 및 Canvas 설정
        /// Verify/add CombatSceneInitializer component to Combat scene and fix Canvas setup
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup Combat Scene")]
        public static void SetupCombatScene()
        {
            if (!File.Exists(COMBAT_SCENE))
            {
                Debug.LogWarning($"Combat 씬을 찾을 수 없습니다: {COMBAT_SCENE}");
                return;
            }

            // 현재 씬 저장
            var currentScene = EditorSceneManager.GetActiveScene();
            bool sceneChanged = currentScene.path != COMBAT_SCENE;

            if (sceneChanged)
            {
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }
                EditorSceneManager.OpenScene(COMBAT_SCENE);
            }

            bool modified = false;

            // Canvas 찾기 및 수정
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // RectTransform Scale 수정
                var rectTransform = canvas.GetComponent<RectTransform>();
                if (rectTransform.localScale != Vector3.one)
                {
                    rectTransform.localScale = Vector3.one;
                    Debug.Log("[GameSetupWizard] Combat Canvas Scale을 (1,1,1)로 수정");
                    modified = true;
                }

                // GraphicRaycaster 확인/추가
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster == null)
                {
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    Debug.Log("[GameSetupWizard] Combat Canvas에 GraphicRaycaster 추가");
                    modified = true;
                }

                // CanvasScaler 설정 확인
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    if (scaler.uiScaleMode != UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                        scaler.referenceResolution != new Vector2(1920, 1080))
                    {
                        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(1920, 1080);
                        scaler.matchWidthOrHeight = 0.5f;
                        Debug.Log("[GameSetupWizard] Combat CanvasScaler 설정 수정");
                        modified = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[GameSetupWizard] Combat 씬에서 Canvas를 찾을 수 없습니다");
            }

            // CombatSceneInitializer 찾기
            var initializer = Object.FindObjectOfType<ProjectSS.Run.CombatSceneInitializer>();
            if (initializer == null)
            {
                // SceneInitializer 오브젝트 찾거나 생성
                var initializerObj = GameObject.Find("SceneInitializer");
                if (initializerObj == null)
                {
                    initializerObj = new GameObject("SceneInitializer");
                }

                initializer = initializerObj.AddComponent<ProjectSS.Run.CombatSceneInitializer>();
                Debug.Log("[GameSetupWizard] CombatSceneInitializer 컴포넌트 추가됨");
                modified = true;
            }
            else
            {
                Debug.Log("[GameSetupWizard] CombatSceneInitializer가 이미 존재합니다");
            }

            if (modified)
            {
                // 씬 저장
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>✅ Combat 씬 설정 완료!</color>");
            }
            else
            {
                Debug.Log("[GameSetupWizard] Combat 씬이 이미 올바르게 설정되어 있습니다");
            }

            // 원래 씬으로 복귀
            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }

        /// <summary>
        /// MainMenu 씬 Canvas 수정
        /// Fix MainMenu scene Canvas setup
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup MainMenu Scene")]
        public static void SetupMainMenuScene()
        {
            if (!File.Exists(MAINMENU_SCENE))
            {
                Debug.LogWarning($"MainMenu 씬을 찾을 수 없습니다: {MAINMENU_SCENE}");
                return;
            }

            // 현재 씬 저장
            var currentScene = EditorSceneManager.GetActiveScene();
            bool sceneChanged = currentScene.path != MAINMENU_SCENE;

            if (sceneChanged)
            {
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }
                EditorSceneManager.OpenScene(MAINMENU_SCENE);
            }

            bool modified = false;

            // Canvas 찾기
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // RectTransform Scale 수정
                var rectTransform = canvas.GetComponent<RectTransform>();
                if (rectTransform.localScale != Vector3.one)
                {
                    rectTransform.localScale = Vector3.one;
                    Debug.Log("[GameSetupWizard] MainMenu Canvas Scale을 (1,1,1)로 수정");
                    modified = true;
                }

                // GraphicRaycaster 확인/추가
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster == null)
                {
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    Debug.Log("[GameSetupWizard] MainMenu Canvas에 GraphicRaycaster 추가");
                    modified = true;
                }

                // CanvasScaler 설정 확인
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    if (scaler.uiScaleMode != UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                    {
                        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(1920, 1080);
                        scaler.matchWidthOrHeight = 0.5f;
                        Debug.Log("[GameSetupWizard] MainMenu CanvasScaler 설정 수정");
                        modified = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[GameSetupWizard] MainMenu 씬에서 Canvas를 찾을 수 없습니다");
            }

            // MainMenuUI 스크립트 확인
            var mainMenuUI = Object.FindObjectOfType<ProjectSS.UI.MainMenuUI>();
            if (mainMenuUI == null)
            {
                // Canvas에 MainMenuUI 추가
                if (canvas != null)
                {
                    mainMenuUI = canvas.gameObject.AddComponent<ProjectSS.UI.MainMenuUI>();
                    Debug.Log("[GameSetupWizard] MainMenuUI 컴포넌트 추가");
                    modified = true;
                }
            }

            // MainMenuUI 버튼 참조 자동 연결
            if (mainMenuUI != null)
            {
                var so = new SerializedObject(mainMenuUI);

                // 버튼 찾기
                var newGameBtn = GameObject.Find("NewGameButton")?.GetComponent<UnityEngine.UI.Button>();
                var continueBtn = GameObject.Find("ContinueButton")?.GetComponent<UnityEngine.UI.Button>();
                var quitBtn = GameObject.Find("QuitButton")?.GetComponent<UnityEngine.UI.Button>();

                // SerializedProperty로 연결
                var newGameProp = so.FindProperty("newGameButton");
                var continueProp = so.FindProperty("continueButton");
                var quitProp = so.FindProperty("quitButton");

                bool buttonsModified = false;

                if (newGameProp != null && newGameBtn != null && newGameProp.objectReferenceValue == null)
                {
                    newGameProp.objectReferenceValue = newGameBtn;
                    buttonsModified = true;
                }
                if (continueProp != null && continueBtn != null && continueProp.objectReferenceValue == null)
                {
                    continueProp.objectReferenceValue = continueBtn;
                    buttonsModified = true;
                }
                if (quitProp != null && quitBtn != null && quitProp.objectReferenceValue == null)
                {
                    quitProp.objectReferenceValue = quitBtn;
                    buttonsModified = true;
                }

                if (buttonsModified)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(mainMenuUI);
                    Debug.Log("[GameSetupWizard] MainMenuUI 버튼 참조 연결 완료");
                    modified = true;
                }
            }

            if (modified)
            {
                // 씬 저장
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>✅ MainMenu 씬 설정 완료!</color>");
            }
            else
            {
                Debug.Log("[GameSetupWizard] MainMenu 씬이 이미 올바르게 설정되어 있습니다");
            }

            // 원래 씬으로 복귀
            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }

        /// <summary>
        /// Map 씬 설정 수정
        /// Fix Map scene setup
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup Map Scene")]
        public static void SetupMapScene()
        {
            if (!File.Exists(MAP_SCENE))
            {
                Debug.LogWarning($"Map 씬을 찾을 수 없습니다: {MAP_SCENE}");
                return;
            }

            // 현재 씬 저장
            var currentScene = EditorSceneManager.GetActiveScene();
            bool sceneChanged = currentScene.path != MAP_SCENE;

            if (sceneChanged)
            {
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }
                EditorSceneManager.OpenScene(MAP_SCENE);
            }

            bool modified = false;

            // Canvas 찾기
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // RectTransform Scale 수정
                var rectTransform = canvas.GetComponent<RectTransform>();
                if (rectTransform.localScale != Vector3.one)
                {
                    rectTransform.localScale = Vector3.one;
                    Debug.Log("[GameSetupWizard] Map Canvas Scale을 (1,1,1)로 수정");
                    modified = true;
                }

                // GraphicRaycaster 확인/추가
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster == null)
                {
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    Debug.Log("[GameSetupWizard] Map Canvas에 GraphicRaycaster 추가");
                    modified = true;
                }

                // CanvasScaler 설정
                var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                if (scaler != null)
                {
                    if (scaler.uiScaleMode != UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize ||
                        scaler.referenceResolution != new Vector2(1920, 1080))
                    {
                        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                        scaler.referenceResolution = new Vector2(1920, 1080);
                        scaler.matchWidthOrHeight = 0.5f;
                        Debug.Log("[GameSetupWizard] Map CanvasScaler 설정 수정");
                        modified = true;
                    }
                }
            }
            else
            {
                Debug.LogWarning("[GameSetupWizard] Map 씬에서 Canvas를 찾을 수 없습니다");
            }

            // ScrollRect 찾기 및 Content 설정
            var scrollRect = Object.FindObjectOfType<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null)
            {
                // Content가 없으면 생성
                if (scrollRect.content == null)
                {
                    var contentObj = new GameObject("Content");
                    contentObj.transform.SetParent(scrollRect.transform);
                    var contentRect = contentObj.AddComponent<RectTransform>();

                    // Content RectTransform 설정
                    contentRect.anchorMin = new Vector2(0, 0);
                    contentRect.anchorMax = new Vector2(1, 1);
                    contentRect.offsetMin = Vector2.zero;
                    contentRect.offsetMax = Vector2.zero;
                    contentRect.localScale = Vector3.one;

                    scrollRect.content = contentRect;
                    Debug.Log("[GameSetupWizard] ScrollRect Content 생성 및 연결");
                    modified = true;

                    // NodeContainer 생성
                    var nodeContainerObj = new GameObject("NodeContainer");
                    nodeContainerObj.transform.SetParent(contentRect);
                    var nodeContainerRect = nodeContainerObj.AddComponent<RectTransform>();
                    nodeContainerRect.anchorMin = Vector2.zero;
                    nodeContainerRect.anchorMax = Vector2.one;
                    nodeContainerRect.offsetMin = Vector2.zero;
                    nodeContainerRect.offsetMax = Vector2.zero;
                    nodeContainerRect.localScale = Vector3.one;

                    // PathContainer 생성
                    var pathContainerObj = new GameObject("PathContainer");
                    pathContainerObj.transform.SetParent(contentRect);
                    pathContainerObj.transform.SetAsFirstSibling(); // 경로가 노드 뒤에 그려지도록
                    var pathContainerRect = pathContainerObj.AddComponent<RectTransform>();
                    pathContainerRect.anchorMin = Vector2.zero;
                    pathContainerRect.anchorMax = Vector2.one;
                    pathContainerRect.offsetMin = Vector2.zero;
                    pathContainerRect.offsetMax = Vector2.zero;
                    pathContainerRect.localScale = Vector3.one;

                    Debug.Log("[GameSetupWizard] NodeContainer, PathContainer 생성");
                }
            }

            // MapManager 확인 및 설정
            var mapManager = Object.FindObjectOfType<ProjectSS.Map.MapManager>();
            if (mapManager == null)
            {
                var mapManagerObj = new GameObject("MapManager");
                mapManager = mapManagerObj.AddComponent<ProjectSS.Map.MapManager>();
                Debug.Log("[GameSetupWizard] MapManager 오브젝트 및 컴포넌트 추가");
                modified = true;
            }

            // MapGenerationConfig 확인 및 연결
            if (mapManager != null)
            {
                var mapManagerSo = new SerializedObject(mapManager);
                var configProp = mapManagerSo.FindProperty("generationConfig");

                if (configProp != null && configProp.objectReferenceValue == null)
                {
                    // 기존 Config 찾기
                    var existingConfig = AssetDatabase.LoadAssetAtPath<ProjectSS.Data.MapGenerationConfig>(
                        "Assets/_Project/Data/Map/MapConfig.asset");

                    if (existingConfig == null)
                    {
                        // Config 생성
                        var configPath = "Assets/_Project/Data/Map";
                        if (!Directory.Exists(configPath))
                        {
                            Directory.CreateDirectory(configPath);
                        }

                        var newConfig = ScriptableObject.CreateInstance<ProjectSS.Data.MapGenerationConfig>();
                        AssetDatabase.CreateAsset(newConfig, $"{configPath}/MapConfig.asset");
                        AssetDatabase.SaveAssets();
                        existingConfig = newConfig;
                        Debug.Log("[GameSetupWizard] MapGenerationConfig 생성됨");
                    }

                    configProp.objectReferenceValue = existingConfig;
                    mapManagerSo.ApplyModifiedProperties();
                    EditorUtility.SetDirty(mapManager);
                    Debug.Log("[GameSetupWizard] MapManager에 GenerationConfig 연결됨");
                    modified = true;
                }
            }

            // MapUI 확인 및 설정
            var mapUI = Object.FindObjectOfType<ProjectSS.UI.MapUI>();
            if (mapUI == null)
            {
                // Canvas에 MapUI 추가
                if (canvas != null)
                {
                    mapUI = canvas.gameObject.AddComponent<ProjectSS.UI.MapUI>();
                    Debug.Log("[GameSetupWizard] MapUI 컴포넌트 추가");
                    modified = true;
                }
            }

            // MapUI 필드 자동 연결
            if (mapUI != null)
            {
                var so = new SerializedObject(mapUI);

                // nodeContainer 찾기
                var nodeContainer = GameObject.Find("NodeContainer")?.transform;
                var pathContainer = GameObject.Find("PathContainer")?.transform;

                // Prefab 찾기
                var nodeButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Prefabs/UI/MapNodePrefab.prefab");
                var pathLinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/_Project/Prefabs/UI/PathLinePrefab.prefab");

                bool fieldsModified = false;

                var nodeContainerProp = so.FindProperty("nodeContainer");
                if (nodeContainerProp != null && nodeContainer != null && nodeContainerProp.objectReferenceValue == null)
                {
                    nodeContainerProp.objectReferenceValue = nodeContainer;
                    fieldsModified = true;
                }

                var pathContainerProp = so.FindProperty("pathContainer");
                if (pathContainerProp != null && pathContainer != null && pathContainerProp.objectReferenceValue == null)
                {
                    pathContainerProp.objectReferenceValue = pathContainer;
                    fieldsModified = true;
                }

                var nodeButtonProp = so.FindProperty("nodeButtonPrefab");
                if (nodeButtonProp != null && nodeButtonPrefab != null && nodeButtonProp.objectReferenceValue == null)
                {
                    nodeButtonProp.objectReferenceValue = nodeButtonPrefab;
                    fieldsModified = true;
                }

                var pathLineProp = so.FindProperty("pathLinePrefab");
                if (pathLineProp != null && pathLinePrefab != null && pathLineProp.objectReferenceValue == null)
                {
                    pathLineProp.objectReferenceValue = pathLinePrefab;
                    fieldsModified = true;
                }

                if (fieldsModified)
                {
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(mapUI);
                    Debug.Log("[GameSetupWizard] MapUI 필드 연결 완료");
                    modified = true;
                }
            }

            if (modified)
            {
                // 씬 저장
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>✅ Map 씬 설정 완료!</color>");
            }
            else
            {
                Debug.Log("[GameSetupWizard] Map 씬이 이미 올바르게 설정되어 있습니다");
            }

            // 원래 씬으로 복귀
            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }

        /// <summary>
        /// Event 씬 설정
        /// Setup Event scene
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup Event Scene")]
        public static void SetupEventScene()
        {
            string scenePath = "Assets/_Project/Scenes/Event.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"Event 씬을 찾을 수 없습니다: {scenePath}");
                return;
            }

            var currentScene = EditorSceneManager.GetActiveScene();
            bool sceneChanged = currentScene.path != scenePath;

            if (sceneChanged)
            {
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }
                EditorSceneManager.OpenScene(scenePath);
            }

            bool modified = false;

            // Canvas 설정
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("EventCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasObj.GetComponent<RectTransform>().localScale = Vector3.one;

                Debug.Log("[GameSetupWizard] Event Canvas 생성됨");
                modified = true;
            }
            else
            {
                // Canvas Scale 수정
                var rectTransform = canvas.GetComponent<RectTransform>();
                if (rectTransform.localScale != Vector3.one)
                {
                    rectTransform.localScale = Vector3.one;
                    modified = true;
                }
            }

            // EventManager 확인
            var eventManager = Object.FindObjectOfType<ProjectSS.Events.EventManager>();
            if (eventManager == null)
            {
                var eventManagerObj = new GameObject("EventManager");
                eventManager = eventManagerObj.AddComponent<ProjectSS.Events.EventManager>();
                Debug.Log("[GameSetupWizard] EventManager 컴포넌트 추가됨");
                modified = true;
            }

            // EventUI 확인
            var eventUI = Object.FindObjectOfType<ProjectSS.UI.EventUI>();
            if (eventUI == null && canvas != null)
            {
                eventUI = canvas.gameObject.AddComponent<ProjectSS.UI.EventUI>();
                Debug.Log("[GameSetupWizard] EventUI 컴포넌트 추가됨");
                modified = true;
            }

            if (modified)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>✅ Event 씬 설정 완료!</color>");
            }
            else
            {
                Debug.Log("[GameSetupWizard] Event 씬이 이미 올바르게 설정되어 있습니다");
            }

            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }

        /// <summary>
        /// Shop 씬 설정
        /// Setup Shop scene
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup Shop Scene")]
        public static void SetupShopScene()
        {
            string scenePath = "Assets/_Project/Scenes/Shop.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"Shop 씬을 찾을 수 없습니다: {scenePath}");
                return;
            }

            var currentScene = EditorSceneManager.GetActiveScene();
            bool sceneChanged = currentScene.path != scenePath;

            if (sceneChanged)
            {
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }
                EditorSceneManager.OpenScene(scenePath);
            }

            bool modified = false;

            // Canvas 설정
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("ShopCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasObj.GetComponent<RectTransform>().localScale = Vector3.one;

                Debug.Log("[GameSetupWizard] Shop Canvas 생성됨");
                modified = true;
            }
            else
            {
                var rectTransform = canvas.GetComponent<RectTransform>();
                if (rectTransform.localScale != Vector3.one)
                {
                    rectTransform.localScale = Vector3.one;
                    modified = true;
                }
            }

            // ShopManager 확인
            var shopManager = Object.FindObjectOfType<ProjectSS.Shop.ShopManager>();
            if (shopManager == null)
            {
                var shopManagerObj = new GameObject("ShopManager");
                shopManager = shopManagerObj.AddComponent<ProjectSS.Shop.ShopManager>();
                Debug.Log("[GameSetupWizard] ShopManager 컴포넌트 추가됨");
                modified = true;

                // ShopConfig 연결 시도
                var config = AssetDatabase.LoadAssetAtPath<ProjectSS.Data.ShopConfig>(
                    "Assets/_Project/Data/Shop/ShopConfig.asset");
                if (config != null)
                {
                    var so = new SerializedObject(shopManager);
                    var configProp = so.FindProperty("config");
                    if (configProp != null)
                    {
                        configProp.objectReferenceValue = config;
                        so.ApplyModifiedProperties();
                        Debug.Log("[GameSetupWizard] ShopManager에 ShopConfig 연결됨");
                    }
                }
            }

            // ShopUI 확인
            var shopUI = Object.FindObjectOfType<ProjectSS.UI.ShopUI>();
            if (shopUI == null && canvas != null)
            {
                shopUI = canvas.gameObject.AddComponent<ProjectSS.UI.ShopUI>();
                Debug.Log("[GameSetupWizard] ShopUI 컴포넌트 추가됨");
                modified = true;
            }

            if (modified)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>✅ Shop 씬 설정 완료!</color>");
            }
            else
            {
                Debug.Log("[GameSetupWizard] Shop 씬이 이미 올바르게 설정되어 있습니다");
            }

            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }

        /// <summary>
        /// Rest 씬 설정
        /// Setup Rest scene
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup Rest Scene")]
        public static void SetupRestScene()
        {
            string scenePath = "Assets/_Project/Scenes/Rest.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"Rest 씬을 찾을 수 없습니다: {scenePath}");
                return;
            }

            var currentScene = EditorSceneManager.GetActiveScene();
            bool sceneChanged = currentScene.path != scenePath;

            if (sceneChanged)
            {
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }
                EditorSceneManager.OpenScene(scenePath);
            }

            bool modified = false;

            // Canvas 설정
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("RestCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasObj.GetComponent<RectTransform>().localScale = Vector3.one;

                Debug.Log("[GameSetupWizard] Rest Canvas 생성됨");
                modified = true;
            }
            else
            {
                var rectTransform = canvas.GetComponent<RectTransform>();
                if (rectTransform.localScale != Vector3.one)
                {
                    rectTransform.localScale = Vector3.one;
                    modified = true;
                }
            }

            // RestUI 확인
            var restUI = Object.FindObjectOfType<ProjectSS.UI.RestUI>();
            if (restUI == null && canvas != null)
            {
                restUI = canvas.gameObject.AddComponent<ProjectSS.UI.RestUI>();
                Debug.Log("[GameSetupWizard] RestUI 컴포넌트 추가됨");
                modified = true;
            }

            if (modified)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>✅ Rest 씬 설정 완료!</color>");
            }
            else
            {
                Debug.Log("[GameSetupWizard] Rest 씬이 이미 올바르게 설정되어 있습니다");
            }

            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }

        /// <summary>
        /// Reward 씬 설정
        /// Setup Reward scene
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Setup Reward Scene")]
        public static void SetupRewardScene()
        {
            string scenePath = "Assets/_Project/Scenes/Reward.unity";
            if (!File.Exists(scenePath))
            {
                Debug.LogWarning($"Reward 씬을 찾을 수 없습니다: {scenePath}");
                return;
            }

            var currentScene = EditorSceneManager.GetActiveScene();
            bool sceneChanged = currentScene.path != scenePath;

            if (sceneChanged)
            {
                if (currentScene.isDirty)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                }
                EditorSceneManager.OpenScene(scenePath);
            }

            bool modified = false;

            // Canvas 설정
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("RewardCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                canvasObj.GetComponent<RectTransform>().localScale = Vector3.one;

                Debug.Log("[GameSetupWizard] Reward Canvas 생성됨");
                modified = true;
            }
            else
            {
                var rectTransform = canvas.GetComponent<RectTransform>();
                if (rectTransform.localScale != Vector3.one)
                {
                    rectTransform.localScale = Vector3.one;
                    modified = true;
                }
            }

            // RewardManager 확인
            var rewardManager = Object.FindObjectOfType<ProjectSS.Reward.RewardManager>();
            if (rewardManager == null)
            {
                var rewardManagerObj = new GameObject("RewardManager");
                rewardManager = rewardManagerObj.AddComponent<ProjectSS.Reward.RewardManager>();
                Debug.Log("[GameSetupWizard] RewardManager 컴포넌트 추가됨");
                modified = true;
            }

            // RewardUI 확인
            var rewardUI = Object.FindObjectOfType<ProjectSS.UI.RewardUI>();
            if (rewardUI == null && canvas != null)
            {
                rewardUI = canvas.gameObject.AddComponent<ProjectSS.UI.RewardUI>();
                Debug.Log("[GameSetupWizard] RewardUI 컴포넌트 추가됨");
                modified = true;
            }

            if (modified)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>✅ Reward 씬 설정 완료!</color>");
            }
            else
            {
                Debug.Log("[GameSetupWizard] Reward 씬이 이미 올바르게 설정되어 있습니다");
            }

            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
            }
        }

        /// <summary>
        /// 모든 씬의 Canvas Scale 문제를 수정
        /// Fix Canvas Scale issues in all scenes
        /// </summary>
        [MenuItem("Tools/Project SS/Setup/Fix All Canvas Scales")]
        public static void FixAllCanvasScales()
        {
            string[] scenePaths = new string[]
            {
                MAINMENU_SCENE,
                MAP_SCENE,
                COMBAT_SCENE,
                "Assets/_Project/Scenes/Event.unity",
                "Assets/_Project/Scenes/Shop.unity",
                "Assets/_Project/Scenes/Rest.unity",
                "Assets/_Project/Scenes/Reward.unity"
            };

            // 현재 씬 저장
            var originalScene = EditorSceneManager.GetActiveScene();
            if (originalScene.isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            int fixedCount = 0;

            foreach (string scenePath in scenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    continue;
                }

                EditorSceneManager.OpenScene(scenePath);

                // 모든 Canvas 찾기
                var canvases = Object.FindObjectsOfType<Canvas>();
                bool sceneModified = false;

                foreach (var canvas in canvases)
                {
                    var rectTransform = canvas.GetComponent<RectTransform>();
                    if (rectTransform.localScale != Vector3.one)
                    {
                        rectTransform.localScale = Vector3.one;
                        Debug.Log($"[FixAllCanvasScales] {scenePath}: {canvas.name} Scale을 (1,1,1)로 수정");
                        sceneModified = true;
                    }

                    // CanvasScaler 설정 확인
                    var scaler = canvas.GetComponent<UnityEngine.UI.CanvasScaler>();
                    if (scaler != null)
                    {
                        if (scaler.uiScaleMode != UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
                        {
                            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                            scaler.referenceResolution = new Vector2(1920, 1080);
                            scaler.matchWidthOrHeight = 0.5f;
                            sceneModified = true;
                        }
                    }

                    // GraphicRaycaster 확인
                    if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                    {
                        canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                        sceneModified = true;
                    }
                }

                if (sceneModified)
                {
                    EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                    fixedCount++;
                }
            }

            // 원래 씬으로 복귀
            if (!string.IsNullOrEmpty(originalScene.path))
            {
                EditorSceneManager.OpenScene(originalScene.path);
            }

            if (fixedCount > 0)
            {
                Debug.Log($"<color=green>✅ {fixedCount}개 씬의 Canvas Scale 문제가 수정되었습니다!</color>");
                EditorUtility.DisplayDialog("Canvas Scale 수정 완료",
                    $"{fixedCount}개 씬의 Canvas Scale이 수정되었습니다.\n\n" +
                    "이제 UI가 정상적으로 표시됩니다.",
                    "확인");
            }
            else
            {
                Debug.Log("[FixAllCanvasScales] 모든 Canvas가 이미 올바르게 설정되어 있습니다.");
                EditorUtility.DisplayDialog("Canvas Scale 확인",
                    "모든 Canvas가 이미 올바르게 설정되어 있습니다.",
                    "확인");
            }
        }

        #endregion

        [MenuItem("Tools/Project SS/🔍 Validate Game Setup", priority = 1)]
        public static void ValidateGameSetup()
        {
            StringBuilder report = new StringBuilder();
            report.AppendLine("=== 게임 설정 검증 결과 ===\n");

            int totalIssues = 0;

            // Status Effects
            report.AppendLine("[Status Effects]");
            if (!StatusEffectGenerator.ValidateAllStatusEffectsExist())
            {
                report.AppendLine("❌ 일부 Status Effects 누락");
                totalIssues++;
            }
            else
            {
                report.AppendLine("✅ 모든 Status Effects 존재");
            }

            // Character Classes
            report.AppendLine("\n[Character Classes]");
            if (!CharacterClassGenerator.ValidateAllClassesExist())
            {
                report.AppendLine("❌ 일부 Character Classes 누락");
                totalIssues++;
            }
            else
            {
                report.AppendLine("✅ 모든 Character Classes 존재");
            }

            // Starter Cards
            report.AppendLine("\n[Starter Cards]");
            if (!StarterCardGenerator.ValidateStarterCardsExist())
            {
                report.AppendLine("❌ 일부 Starter Cards 누락");
                totalIssues++;
            }
            else
            {
                report.AppendLine("✅ 모든 Starter Cards 존재");
            }

            // Scenes
            report.AppendLine("\n[Scenes]");
            if (!ValidateBootScene())
            {
                report.AppendLine("❌ Boot 씬에 BootLoader 컴포넌트 누락");
                totalIssues++;
            }
            else
            {
                report.AppendLine("✅ Boot 씬 설정 완료");
            }

            // Build Settings
            report.AppendLine("\n[Build Settings]");
            if (!ValidateBuildSettings())
            {
                report.AppendLine("❌ 일부 씬이 Build Settings에 등록되지 않음");
                totalIssues++;
            }
            else
            {
                report.AppendLine("✅ 모든 씬이 Build Settings에 등록됨");
            }

            // GameManager Prefab
            report.AppendLine("\n[GameManager Prefab]");
            if (!PrefabSetupGenerator.ValidateGameManagerPrefab())
            {
                report.AppendLine("❌ GameManager Prefab 설정 누락");
                totalIssues++;
            }
            else
            {
                report.AppendLine("✅ GameManager Prefab 설정 완료");
            }

            // Summary
            report.AppendLine($"\n총 이슈: {totalIssues}개");

            if (totalIssues > 0)
            {
                report.AppendLine("\n권장: Tools > Project SS > 🎮 Complete Game Setup 실행");
            }

            Debug.Log(report.ToString());

            if (totalIssues > 0)
            {
                EditorUtility.DisplayDialog("검증 결과",
                    $"{totalIssues}개 이슈가 발견되었습니다.\n\n" +
                    "Tools > Project SS > 🎮 Complete Game Setup\n" +
                    "메뉴를 실행하여 해결하세요.",
                    "확인");
            }
            else
            {
                EditorUtility.DisplayDialog("검증 결과",
                    "모든 게임 설정이 완료되었습니다!\n\n" +
                    "Boot 씬을 열고 플레이 버튼을 눌러\n" +
                    "게임을 테스트할 수 있습니다.",
                    "확인");
            }
        }

        /// <summary>
        /// Boot 씬 유효성 검사
        /// Validate Boot scene setup
        /// </summary>
        private static bool ValidateBootScene()
        {
            if (!File.Exists(BOOT_SCENE))
            {
                return false;
            }

            // 씬 파일 내용 확인 (BootLoader GUID 검색)
            string sceneContent = File.ReadAllText(BOOT_SCENE);
            // BootLoader의 GUID: 6e24d0d5d948f64459109519b86d1d95
            return sceneContent.Contains("6e24d0d5d948f64459109519b86d1d95");
        }

        [MenuItem("Tools/Project SS/📖 Show Setup Instructions", priority = 2)]
        public static void ShowSetupInstructions()
        {
            string instructions = @"=== Project SS 설정 가이드 ===

【빠른 설정】
1. Tools > Project SS > 🎮 Complete Game Setup 실행
2. Unity에서 Boot 씬 열기
3. 플레이 버튼 클릭

【수동 설정 (필요한 경우)】

1. Status Effects 생성:
   Tools > Project SS > Generators > Generate Missing Status Effects

2. Character Classes 생성:
   Tools > Project SS > Generators > Generate TRIAD Classes

3. 스타터 카드 생성:
   Tools > Project SS > Generators > Generate Starter Cards

4. 적 생성:
   Tools > Project SS > Generators > Generate Act 1 Enemies

5. Prefab 설정:
   Tools > Project SS > Setup > Setup Prefab Components

6. Resources 설정:
   Tools > Project SS > Setup > Setup Resources Folder

7. 씬 설정:
   Tools > Project SS > Setup > Setup Boot Scene
   Tools > Project SS > Setup > Setup Combat Scene

8. Build Settings 설정:
   Tools > Project SS > Setup > Setup Build Settings

【검증】
Tools > Project SS > 🔍 Validate Game Setup

【트러블슈팅】
- Boot 씬에서 다음 씬으로 안 넘어가면: BootLoader 컴포넌트 확인
- 'Scene couldn't be loaded' 에러: Build Settings에 씬 등록 필요
  → Tools > Project SS > Setup > Setup Build Settings 실행
- Combat 씬에서 적이 안 나오면: CombatSceneInitializer 컴포넌트 확인
- 캐릭터 클래스가 로드 안 되면: Resources/CharacterClasses 폴더 확인
- GameManager prefab 할당 안됨: Prefab 설정 확인
- Console 창에서 에러 메시지 확인
";

            Debug.Log(instructions);

            EditorUtility.DisplayDialog("설정 가이드",
                "Console 창에 자세한 설정 가이드가 출력되었습니다.\n\n" +
                "빠른 설정: Tools > Project SS > 🎮 Complete Game Setup",
                "확인");
        }

        [MenuItem("Tools/Project SS/🚀 Open Boot Scene and Play", priority = 3)]
        public static void OpenBootSceneAndPlay()
        {
            if (!File.Exists(BOOT_SCENE))
            {
                EditorUtility.DisplayDialog("오류", "Boot 씬을 찾을 수 없습니다.", "확인");
                return;
            }

            // 현재 씬 저장
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            // Boot 씬 열기
            EditorSceneManager.OpenScene(BOOT_SCENE);

            // 플레이 모드 시작
            EditorApplication.isPlaying = true;
        }
    }
}
