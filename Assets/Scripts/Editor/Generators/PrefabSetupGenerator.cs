using UnityEngine;
using UnityEditor;
using ProjectSS.Combat;
using ProjectSS.UI;
using ProjectSS.Core;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// Prefab 설정 도우미
    /// Prefab setup helper
    ///
    /// Prefab에 필요한 컴포넌트를 추가합니다.
    /// Adds required components to prefabs.
    /// </summary>
    public static class PrefabSetupGenerator
    {
        private const string ENEMY_PREFAB_PATH = "Assets/_Project/Prefabs/UI/EnemyPrefab.prefab";
        private const string CARD_PREFAB_PATH = "Assets/_Project/Prefabs/UI/CardPrefab.prefab";
        private const string GAME_MANAGER_PREFAB_PATH = "Assets/_Project/Prefabs/Managers/GameManager.prefab";

        [MenuItem("Tools/Project SS/Setup/Setup Prefab Components")]
        public static void SetupPrefabComponents()
        {
            int updated = 0;

            EditorUtility.DisplayProgressBar("Prefab 설정", "GameManager 설정 중...", 0.2f);
            if (SetupGameManagerPrefab())
                updated++;

            EditorUtility.DisplayProgressBar("Prefab 설정", "EnemyPrefab 설정 중...", 0.5f);
            if (SetupEnemyPrefab())
                updated++;

            EditorUtility.DisplayProgressBar("Prefab 설정", "CardPrefab 설정 중...", 0.8f);
            if (SetupCardPrefab())
                updated++;

            EditorUtility.DisplayProgressBar("Prefab 설정", "완료 중...", 0.95f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            if (updated > 0)
            {
                Debug.Log($"<color=green>✅ Prefab 설정 완료! {updated}개 Prefab 업데이트됨</color>");
                EditorUtility.DisplayDialog("완료", $"{updated}개 Prefab이 업데이트되었습니다.", "확인");
            }
            else
            {
                Debug.Log("모든 Prefab이 이미 설정되어 있습니다.");
                EditorUtility.DisplayDialog("알림", "모든 Prefab이 이미 설정되어 있습니다.", "확인");
            }
        }

        [MenuItem("Tools/Project SS/Setup/Setup GameManager Prefab")]
        public static bool SetupGameManagerPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GAME_MANAGER_PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogWarning($"GameManager Prefab not found at: {GAME_MANAGER_PREFAB_PATH}");
                // Prefab 생성 시도
                return CreateGameManagerPrefab();
            }

            // GameManager 컴포넌트 확인
            var gameManager = prefab.GetComponent<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("GameManager Prefab already has GameManager component");
                return false;
            }

            // Prefab 편집 시작
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var root = editScope.prefabContentsRoot;

                // GameManager 컴포넌트 추가
                root.AddComponent<GameManager>();
                Debug.Log("Added GameManager component to GameManager Prefab");
            }

            return true;
        }

        /// <summary>
        /// GameManager Prefab이 없으면 생성
        /// Create GameManager Prefab if not exists
        /// </summary>
        private static bool CreateGameManagerPrefab()
        {
            // 폴더 확인
            string folderPath = "Assets/_Project/Prefabs/Managers";
            GeneratorUtility.EnsureFolderExists(folderPath);

            // 새 GameObject 생성
            var gameObject = new GameObject("GameManager");
            gameObject.AddComponent<GameManager>();

            // Prefab으로 저장
            string prefabPath = $"{folderPath}/GameManager.prefab";
            PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
            Object.DestroyImmediate(gameObject);

            Debug.Log($"Created GameManager Prefab at: {prefabPath}");
            return true;
        }

        [MenuItem("Tools/Project SS/Setup/Setup Enemy Prefab")]
        public static bool SetupEnemyPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ENEMY_PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogWarning($"EnemyPrefab not found at: {ENEMY_PREFAB_PATH}");
                return false;
            }

            // EnemyCombat 컴포넌트 확인
            var enemyCombat = prefab.GetComponent<EnemyCombat>();
            if (enemyCombat != null)
            {
                Debug.Log("EnemyPrefab already has EnemyCombat component");
                return false;
            }

            // Prefab 편집 시작
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var root = editScope.prefabContentsRoot;

                // EnemyCombat 컴포넌트 추가
                root.AddComponent<EnemyCombat>();
                Debug.Log("Added EnemyCombat component to EnemyPrefab");
            }

            return true;
        }

        [MenuItem("Tools/Project SS/Setup/Setup Card Prefab")]
        public static bool SetupCardPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CARD_PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogWarning($"CardPrefab not found at: {CARD_PREFAB_PATH}");
                return false;
            }

            // CardUI 컴포넌트 확인
            var cardUI = prefab.GetComponent<CardUI>();
            if (cardUI != null)
            {
                Debug.Log("CardPrefab already has CardUI component");
                return false;
            }

            // Prefab 편집 시작
            string assetPath = AssetDatabase.GetAssetPath(prefab);
            using (var editScope = new PrefabUtility.EditPrefabContentsScope(assetPath))
            {
                var root = editScope.prefabContentsRoot;

                // CardUI 컴포넌트 추가
                var cardUIComponent = root.AddComponent<CardUI>();

                // UI 요소 자동 연결 시도
                var nameText = root.transform.Find("NameText")?.GetComponent<UnityEngine.UI.Text>();
                var costText = root.transform.Find("CostText")?.GetComponent<UnityEngine.UI.Text>();
                var descText = root.transform.Find("DescriptionText")?.GetComponent<UnityEngine.UI.Text>();
                var typeText = root.transform.Find("TypeText")?.GetComponent<UnityEngine.UI.Text>();

                // 리플렉션으로 private SerializeField 설정
                var cardUIType = typeof(CardUI);

                if (nameText != null)
                {
                    var field = cardUIType.GetField("cardNameText",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(cardUIComponent, nameText);
                }

                if (costText != null)
                {
                    var field = cardUIType.GetField("costText",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(cardUIComponent, costText);
                }

                if (descText != null)
                {
                    var field = cardUIType.GetField("descriptionText",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(cardUIComponent, descText);
                }

                Debug.Log("Added CardUI component to CardPrefab");
            }

            return true;
        }

        [MenuItem("Tools/Project SS/Setup/Validate Prefab Components")]
        public static void ValidatePrefabComponents()
        {
            int issues = 0;
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.AppendLine("=== Prefab 검증 결과 ===\n");

            // GameManager Prefab 검증
            report.AppendLine("[GameManager Prefab]");
            var gameManagerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(GAME_MANAGER_PREFAB_PATH);
            if (gameManagerPrefab == null)
            {
                report.AppendLine("❌ Prefab 파일 없음");
                issues++;
            }
            else
            {
                if (gameManagerPrefab.GetComponent<GameManager>() == null)
                {
                    report.AppendLine("❌ GameManager 컴포넌트 없음");
                    issues++;
                }
                else
                {
                    report.AppendLine("✅ GameManager 컴포넌트 있음");
                }
            }

            // EnemyPrefab 검증
            report.AppendLine("\n[EnemyPrefab]");
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ENEMY_PREFAB_PATH);
            if (enemyPrefab == null)
            {
                report.AppendLine("❌ Prefab 파일 없음");
                issues++;
            }
            else
            {
                if (enemyPrefab.GetComponent<EnemyCombat>() == null)
                {
                    report.AppendLine("❌ EnemyCombat 컴포넌트 없음");
                    issues++;
                }
                else
                {
                    report.AppendLine("✅ EnemyCombat 컴포넌트 있음");
                }
            }

            // CardPrefab 검증
            report.AppendLine("\n[CardPrefab]");
            var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CARD_PREFAB_PATH);
            if (cardPrefab == null)
            {
                report.AppendLine("❌ Prefab 파일 없음");
                issues++;
            }
            else
            {
                if (cardPrefab.GetComponent<CardUI>() == null)
                {
                    report.AppendLine("❌ CardUI 컴포넌트 없음");
                    issues++;
                }
                else
                {
                    report.AppendLine("✅ CardUI 컴포넌트 있음");
                }
            }

            report.AppendLine($"\n총 이슈: {issues}개");

            Debug.Log(report.ToString());

            if (issues > 0)
            {
                EditorUtility.DisplayDialog("검증 결과",
                    $"{issues}개 이슈가 발견되었습니다.\n\n" +
                    "Tools > Project SS > Setup > Setup Prefab Components\n" +
                    "메뉴를 실행하여 수정하세요.", "확인");
            }
            else
            {
                EditorUtility.DisplayDialog("검증 결과",
                    "모든 Prefab이 올바르게 설정되어 있습니다!", "확인");
            }
        }

        /// <summary>
        /// GameManager Prefab 유효성 확인
        /// Check if GameManager Prefab is valid
        /// </summary>
        public static bool ValidateGameManagerPrefab()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(GAME_MANAGER_PREFAB_PATH);
            return prefab != null && prefab.GetComponent<GameManager>() != null;
        }
    }
}
