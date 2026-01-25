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

        [MenuItem("Tools/Project SS/🎮 Complete Game Setup", priority = 0)]
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
            SetupCombatScene();
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
        /// Combat 씬에 CombatSceneInitializer 컴포넌트 확인/추가
        /// Verify/add CombatSceneInitializer component to Combat scene
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
            }
            else
            {
                Debug.Log("[GameSetupWizard] CombatSceneInitializer가 이미 존재합니다");
            }

            // 씬 저장
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            // 원래 씬으로 복귀
            if (sceneChanged && !string.IsNullOrEmpty(currentScene.path))
            {
                EditorSceneManager.OpenScene(currentScene.path);
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
