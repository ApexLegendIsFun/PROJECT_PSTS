// Editor/ConfigAssetGenerator.cs
// 게임 설정 SO 에셋 생성 도구
// GameBalanceConfig 및 EnemyAIConfig 자동 생성

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectSS.Core;
using ProjectSS.Core.Config;
using ProjectSS.Data.Enemies;

namespace ProjectSS.Editor
{
    /// <summary>
    /// 게임 설정 SO 에셋 생성 도구
    /// GameBalanceConfig, EnemyAIConfig 자동 생성
    /// </summary>
    public class ConfigAssetGenerator : EditorWindow
    {
        private const string CONFIG_PATH = "Assets/Resources/Config";
        private const string ENEMY_AI_PATH = "Assets/Data/Enemies/AI";

        [MenuItem("PSTS/Config Asset Generator")]
        public static void ShowWindow()
        {
            GetWindow<ConfigAssetGenerator>("Config Asset Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("설정 에셋 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("폴더 구조 생성", GUILayout.Height(30)))
            {
                CreateFolderStructure();
            }

            GUILayout.Space(10);
            GUILayout.Label("게임 밸런스 설정", EditorStyles.boldLabel);

            if (GUILayout.Button("GameBalanceConfig 생성", GUILayout.Height(30)))
            {
                CreateGameBalanceConfig();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GUILayout.Space(10);
            GUILayout.Label("적 AI 설정", EditorStyles.boldLabel);

            if (GUILayout.Button("기본 EnemyAIConfig 생성 (Default, Aggressive, Defensive)", GUILayout.Height(30)))
            {
                CreateDefaultEnemyAIConfigs();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            GUILayout.Space(20);
            GUILayout.Label("전체 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("모두 생성", GUILayout.Height(40)))
            {
                CreateAllAssetsStatic();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox(
                "생성되는 에셋:\n" +
                "• GameBalanceConfig (Resources/Config/)\n" +
                "• DefaultAI, AggressiveAI, DefensiveAI (Data/Enemies/AI/)",
                MessageType.Info);
        }

        /// <summary>
        /// 정적 메서드: ProjectBootstrapper에서 호출
        /// </summary>
        public static void CreateAllAssetsStatic()
        {
            CreateFolderStructure();
            CreateGameBalanceConfig();
            CreateDefaultEnemyAIConfigs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ConfigAssetGenerator] 모든 설정 에셋 생성 완료");
        }

        #region Folder Structure

        private static void CreateFolderStructure()
        {
            CreateFolderIfNotExists("Assets/Resources");
            CreateFolderIfNotExists(CONFIG_PATH);
            CreateFolderIfNotExists("Assets/Data");
            CreateFolderIfNotExists("Assets/Data/Enemies");
            CreateFolderIfNotExists(ENEMY_AI_PATH);
            Debug.Log("[ConfigAssetGenerator] 폴더 구조 생성 완료");
        }

        private static void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace("\\", "/");
                string folderName = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        #endregion

        #region GameBalanceConfig

        private static void CreateGameBalanceConfig()
        {
            string path = $"{CONFIG_PATH}/GameBalanceConfig.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<GameBalanceConfigSO>(path) != null)
            {
                Debug.Log("[ConfigAssetGenerator] GameBalanceConfig already exists");
                return;
            }

            var config = ScriptableObject.CreateInstance<GameBalanceConfigSO>();
            AssetDatabase.CreateAsset(config, path);
            Debug.Log("[ConfigAssetGenerator] Created GameBalanceConfig");
        }

        #endregion

        #region EnemyAIConfig

        private static void CreateDefaultEnemyAIConfigs()
        {
            // 기본 AI (공격 70%, 방어 30%)
            CreateEnemyAIConfig("DefaultAI",
                EnemyIntentType.Attack, 0.7f,
                EnemyIntentType.Defend, 0.3f);

            // 공격적 AI (공격 90%, 방어 10%)
            CreateEnemyAIConfig("AggressiveAI",
                EnemyIntentType.Attack, 0.9f,
                EnemyIntentType.Defend, 0.1f);

            // 방어적 AI (공격 40%, 방어 60%)
            CreateEnemyAIConfig("DefensiveAI",
                EnemyIntentType.Attack, 0.4f,
                EnemyIntentType.Defend, 0.6f);

            Debug.Log("[ConfigAssetGenerator] 기본 EnemyAIConfig 생성 완료");
        }

        private static void CreateEnemyAIConfig(string id,
            EnemyIntentType intent1, float weight1,
            EnemyIntentType intent2, float weight2)
        {
            string path = $"{ENEMY_AI_PATH}/{id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<EnemyAIConfigSO>(path) != null)
            {
                Debug.Log($"[ConfigAssetGenerator] EnemyAIConfig already exists: {id}");
                return;
            }

            var config = ScriptableObject.CreateInstance<EnemyAIConfigSO>();

            // SerializedObject를 통해 private 필드 설정
            var so = new SerializedObject(config);
            var weights = so.FindProperty("_defaultWeights");
            weights.ClearArray();

            // Intent 1
            weights.InsertArrayElementAtIndex(0);
            var elem0 = weights.GetArrayElementAtIndex(0);
            elem0.FindPropertyRelative("Intent").enumValueIndex = (int)intent1;
            elem0.FindPropertyRelative("Weight").floatValue = weight1;

            // Intent 2
            weights.InsertArrayElementAtIndex(1);
            var elem1 = weights.GetArrayElementAtIndex(1);
            elem1.FindPropertyRelative("Intent").enumValueIndex = (int)intent2;
            elem1.FindPropertyRelative("Weight").floatValue = weight2;

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(config, path);
            Debug.Log($"[ConfigAssetGenerator] Created EnemyAIConfig: {id}");
        }

        #endregion

        #region Link to EnemyData

        /// <summary>
        /// 기존 EnemyDataSO에 AI Config 연결
        /// </summary>
        public static void LinkAIConfigsToEnemies()
        {
            var defaultAI = AssetDatabase.LoadAssetAtPath<EnemyAIConfigSO>($"{ENEMY_AI_PATH}/DefaultAI.asset");
            var aggressiveAI = AssetDatabase.LoadAssetAtPath<EnemyAIConfigSO>($"{ENEMY_AI_PATH}/AggressiveAI.asset");
            var defensiveAI = AssetDatabase.LoadAssetAtPath<EnemyAIConfigSO>($"{ENEMY_AI_PATH}/DefensiveAI.asset");

            if (defaultAI == null)
            {
                Debug.LogWarning("[ConfigAssetGenerator] DefaultAI not found. Run CreateAllAssetsStatic first.");
                return;
            }

            // 모든 EnemyDataSO 찾기
            string[] guids = AssetDatabase.FindAssets("t:EnemyDataSO", new[] { "Assets/Data/Enemies" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var enemyData = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);

                if (enemyData == null) continue;

                var so = new SerializedObject(enemyData);
                var aiConfigProp = so.FindProperty("_aiConfig");

                // AI Config가 없으면 적 타입에 따라 할당
                if (aiConfigProp.objectReferenceValue == null)
                {
                    var enemyTypeProp = so.FindProperty("_enemyType");
                    string enemyType = enemyTypeProp.stringValue;

                    // 보스는 공격적, 엘리트는 기본, 일반은 기본
                    if (enemyType == "Boss")
                    {
                        aiConfigProp.objectReferenceValue = aggressiveAI;
                    }
                    else
                    {
                        aiConfigProp.objectReferenceValue = defaultAI;
                    }

                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(enemyData);
                    Debug.Log($"[ConfigAssetGenerator] Linked AI Config to {enemyData.DisplayName}");
                }
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[ConfigAssetGenerator] AI Config 연결 완료");
        }

        #endregion
    }
}
#endif
