// Editor/ProjectBootstrapper.cs
// 프로젝트 부트스트래퍼 - 즉시 플레이 가능한 개발 환경 구축
// 단축키: Ctrl+Alt+A

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ProjectSS.Editor
{
    /// <summary>
    /// 프로젝트 부트스트래퍼
    /// 씬, 에셋, 프리팹을 자동 생성하여 즉시 테스트 가능한 환경 구축
    /// </summary>
    public static class ProjectBootstrapper
    {
        private const string ScenesFolder = "Assets/Scenes";

        /// <summary>
        /// 프로젝트 부트스트랩 실행
        /// 단축키: Ctrl+Alt+A (%&a)
        /// </summary>
        [MenuItem("Tools/PSTS/Bootstrap Project %&a")]
        public static void Bootstrap()
        {
            if (!ShowConfirmDialog())
            {
                return;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Bootstrap", "에셋 리프레시 중...", 0f);
                AssetDatabase.Refresh();
                EnsureScenesFolder();

                // 카드 에셋 생성
                EditorUtility.DisplayProgressBar("Bootstrap", "카드 에셋 생성 중...", 0.1f);
                CardAssetGenerator.CreateAllAssetsStatic();

                // 월드맵 에셋 생성
                EditorUtility.DisplayProgressBar("Bootstrap", "월드맵 에셋 생성 중...", 0.15f);
                WorldMapAssetGenerator.CreateAllAssetsStatic();

                // 씬 빌드
                var builders = CreateBuilders();
                var sceneAssets = BuildAllScenes(builders);

                UpdateBuildSettings(sceneAssets);
                OpenBootScene();

                EditorUtility.ClearProgressBar();
                ShowCompletionDialog();

                Debug.Log("[ProjectBootstrapper] 부트스트랩 완료!");
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                HandleError(e);
            }
        }

        #region Private Methods

        private static bool ShowConfirmDialog()
        {
            return EditorUtility.DisplayDialog(
                "프로젝트 부트스트랩",
                "즉시 플레이 가능한 개발 환경을 구축합니다.\n\n" +
                "생성 항목:\n" +
                "• 카드 에셋 (효과 SO, 카드 SO, 카드풀)\n" +
                "• 월드맵 에셋 (Visual Config, 테스트맵, 프리팹)\n" +
                "• 4개 씬 (Boot, MainMenu, Map, Combat)\n" +
                "• UI 컴포넌트 및 매니저\n\n" +
                "기존 에셋/씬이 있으면 덮어씁니다.",
                "부트스트랩 실행",
                "취소");
        }

        private static void EnsureScenesFolder()
        {
            if (!AssetDatabase.IsValidFolder(ScenesFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        private static ISceneBuilder[] CreateBuilders()
        {
            return new ISceneBuilder[]
            {
                new BootSceneBuilder(),
                new MainMenuSceneBuilder(),
                new MapSceneBuilder(),
                new CombatSceneBuilder()
            };
        }

        private static List<EditorBuildSettingsScene> BuildAllScenes(ISceneBuilder[] builders)
        {
            var sceneAssets = new List<EditorBuildSettingsScene>();

            for (int i = 0; i < builders.Length; i++)
            {
                float progress = 0.2f + (i + 1f) / builders.Length * 0.6f;
                EditorUtility.DisplayProgressBar(
                    "Bootstrap",
                    $"{builders[i].SceneName} 씬 생성 중...",
                    progress);

                string scenePath = builders[i].Build(ScenesFolder);
                sceneAssets.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            return sceneAssets;
        }

        private static void UpdateBuildSettings(List<EditorBuildSettingsScene> sceneAssets)
        {
            EditorUtility.DisplayProgressBar("Bootstrap", "Build Settings 업데이트 중...", 0.9f);
            EditorBuildSettings.scenes = sceneAssets.ToArray();
        }

        private static void OpenBootScene()
        {
            string bootScenePath = $"{ScenesFolder}/Boot.unity";
            if (File.Exists(bootScenePath))
            {
                EditorSceneManager.OpenScene(bootScenePath);
            }
        }

        private static void ShowCompletionDialog()
        {
            EditorUtility.DisplayDialog(
                "부트스트랩 완료",
                "개발 환경 구축이 완료되었습니다!\n\n" +
                "━━━ 생성된 에셋 ━━━\n" +
                "• 카드 효과 SO: 9개\n" +
                "• 카드 SO: 3개\n" +
                "• 캐릭터 카드풀: 3개\n" +
                "• 월드맵 Visual Config: 1개\n" +
                "• 월드맵 테스트 데이터: 1개 (6노드)\n" +
                "• 월드맵 프리팹: 2개 (노드, 경로)\n\n" +
                "━━━ 생성된 씬 ━━━\n" +
                "0: Boot\n" +
                "1: MainMenu\n" +
                "2: Map (월드맵)\n" +
                "3: Combat\n\n" +
                "▶ Map 씬에서 월드맵 테스트 가능",
                "확인");
        }

        private static void HandleError(System.Exception e)
        {
            Debug.LogError($"[ProjectBootstrapper] 오류 발생: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog(
                "오류",
                $"부트스트랩 중 오류가 발생했습니다:\n{e.Message}",
                "확인");
        }

        #endregion
    }
}
