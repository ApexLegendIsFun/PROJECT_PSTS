// Editor/EnemyAssetGenerator.cs
// 적 및 인카운터 SO 에셋 생성 도구

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ProjectSS.Core;
using ProjectSS.Data.Enemies;
using ProjectSS.Data.Encounters;
using ProjectSS.Combat;

namespace ProjectSS.Editor
{
    /// <summary>
    /// 적 및 인카운터 SO 에셋 생성 도구
    /// </summary>
    public class EnemyAssetGenerator : EditorWindow
    {
        private const string ENEMIES_PATH = "Assets/Data/Enemies";
        private const string ENCOUNTERS_PATH = "Assets/Data/Encounters";
        private const string POOLS_PATH = "Assets/Data/Encounters/Pools";

        [MenuItem("PSTS/Enemy Asset Generator")]
        public static void ShowWindow()
        {
            GetWindow<EnemyAssetGenerator>("Enemy Asset Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("적 에셋 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("폴더 구조 생성", GUILayout.Height(30)))
            {
                CreateFolderStructure();
            }

            GUILayout.Space(10);
            GUILayout.Label("기본 적 SO 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("기본 적 에셋 생성 (Slime, Goblin, Skeleton, Elite, Boss)", GUILayout.Height(30)))
            {
                CreateBaseEnemyAssets();
            }

            GUILayout.Space(10);
            GUILayout.Label("인카운터 SO 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("기본 인카운터 에셋 생성", GUILayout.Height(30)))
            {
                CreateBaseEncounterAssets();
            }

            GUILayout.Space(10);
            GUILayout.Label("인카운터 풀 SO 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("Act 1 인카운터 풀 생성", GUILayout.Height(30)))
            {
                CreateEncounterPools();
            }

            GUILayout.Space(20);
            GUILayout.Label("전체 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("모두 생성 (폴더 + 적 + 인카운터 + 풀)", GUILayout.Height(40)))
            {
                CreateAll();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("'모두 생성' 버튼을 누르면 씬의 CombatSceneInitializer에 자동으로 연결됩니다.", MessageType.Info);

            GUILayout.Space(10);
            if (GUILayout.Button("씬에 인카운터 풀 연결", GUILayout.Height(30)))
            {
                LinkEncounterPoolsToScene();
            }
        }

        /// <summary>
        /// 모든 에셋 생성 및 씬 연결
        /// </summary>
        private void CreateAll()
        {
            CreateFolderStructure();
            CreateBaseEnemyAssets();
            CreateBaseEncounterAssets();
            CreateEncounterPools();
            LinkEncounterPoolsToScene();
            Debug.Log("[EnemyAssetGenerator] 모든 에셋 생성 및 연결 완료!");
        }

        /// <summary>
        /// 정적 메서드: ProjectBootstrapper에서 호출
        /// </summary>
        public static void CreateAllAssetsStatic()
        {
            CreateFolderStructureStatic();
            CreateBaseEnemyAssetsStatic();
            CreateBaseEncounterAssetsStatic();
            CreateEncounterPoolsStatic();
            Debug.Log("[EnemyAssetGenerator] 모든 에셋 생성 완료 (Static)");
        }

        #region Folder Structure

        private void CreateFolderStructure()
        {
            CreateFolderStructureStatic();
        }

        private static void CreateFolderStructureStatic()
        {
            CreateFolderIfNotExists("Assets/Data");
            CreateFolderIfNotExists(ENEMIES_PATH);
            CreateFolderIfNotExists(ENCOUNTERS_PATH);
            CreateFolderIfNotExists(POOLS_PATH);
            AssetDatabase.Refresh();
            Debug.Log("[EnemyAssetGenerator] 폴더 구조 생성 완료");
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

        #region Enemy Assets

        private void CreateBaseEnemyAssets()
        {
            CreateBaseEnemyAssetsStatic();
        }

        private static void CreateBaseEnemyAssetsStatic()
        {
            // 일반 적
            CreateEnemyAsset("Slime", "슬라임", "Slime", 25, 6, 6, 3);
            CreateEnemyAsset("Goblin", "고블린", "Goblin", 30, 8, 8, 4);
            CreateEnemyAsset("Skeleton", "스켈레톤", "Skeleton", 35, 7, 10, 5);

            // 엘리트 적
            CreateEnemyAsset("Elite_Guardian", "수호자", "Elite", 50, 9, 12, 6);
            CreateEnemyAsset("Elite_Assassin", "암살자", "Elite", 45, 12, 15, 4);

            // 보스
            CreateEnemyAsset("Boss_Dragon", "드래곤", "Boss", 100, 10, 15, 8);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnemyAssetGenerator] 기본 적 에셋 생성 완료");
        }

        private static void CreateEnemyAsset(string id, string displayName, string enemyType,
            int baseHP, int baseSpeed, int baseDamage, int baseBlock)
        {
            string path = $"{ENEMIES_PATH}/{id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path) != null)
            {
                Debug.Log($"[EnemyAssetGenerator] Enemy already exists: {id}");
                return;
            }

            var enemy = ScriptableObject.CreateInstance<EnemyDataSO>();

            // SerializedObject를 통해 private 필드 설정
            var so = new SerializedObject(enemy);
            so.FindProperty("_enemyId").stringValue = id.ToLower();
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_enemyType").stringValue = enemyType;
            so.FindProperty("_description").stringValue = $"{displayName} - {enemyType} 타입";
            so.FindProperty("_baseHP").intValue = baseHP;
            so.FindProperty("_baseSpeed").intValue = baseSpeed;
            so.FindProperty("_baseAttackDamage").intValue = baseDamage;
            so.FindProperty("_baseBlockAmount").intValue = baseBlock;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(enemy, path);
            Debug.Log($"[EnemyAssetGenerator] Created enemy: {id}");
        }

        #endregion

        #region Encounter Assets

        private void CreateBaseEncounterAssets()
        {
            CreateBaseEncounterAssetsStatic();
        }

        private static void CreateBaseEncounterAssetsStatic()
        {
            // 적 에셋 로드
            var slime = AssetDatabase.LoadAssetAtPath<EnemyDataSO>($"{ENEMIES_PATH}/Slime.asset");
            var goblin = AssetDatabase.LoadAssetAtPath<EnemyDataSO>($"{ENEMIES_PATH}/Goblin.asset");
            var skeleton = AssetDatabase.LoadAssetAtPath<EnemyDataSO>($"{ENEMIES_PATH}/Skeleton.asset");
            var eliteGuardian = AssetDatabase.LoadAssetAtPath<EnemyDataSO>($"{ENEMIES_PATH}/Elite_Guardian.asset");
            var eliteAssassin = AssetDatabase.LoadAssetAtPath<EnemyDataSO>($"{ENEMIES_PATH}/Elite_Assassin.asset");
            var bossDragon = AssetDatabase.LoadAssetAtPath<EnemyDataSO>($"{ENEMIES_PATH}/Boss_Dragon.asset");

            // 일반 인카운터
            CreateEncounterAsset("Normal_SlimeGroup", "슬라임 무리", TileType.Enemy,
                new List<EncounterEnemyConfig>
                {
                    new EncounterEnemyConfig { Enemy = slime, Count = 2, RandomMin = 2, RandomMax = 3 }
                }, 10, CardRarity.Common);

            CreateEncounterAsset("Normal_GoblinRaid", "고블린 습격", TileType.Enemy,
                new List<EncounterEnemyConfig>
                {
                    new EncounterEnemyConfig { Enemy = goblin, Count = 2 },
                    new EncounterEnemyConfig { Enemy = skeleton, Count = 1 }
                }, 15, CardRarity.Common);

            CreateEncounterAsset("Normal_Undead", "언데드", TileType.Enemy,
                new List<EncounterEnemyConfig>
                {
                    new EncounterEnemyConfig { Enemy = skeleton, Count = 2, RandomMin = 2, RandomMax = 3 }
                }, 12, CardRarity.Common);

            // 엘리트 인카운터
            CreateEncounterAsset("Elite_Guardians", "수호자들", TileType.Elite,
                new List<EncounterEnemyConfig>
                {
                    new EncounterEnemyConfig { Enemy = eliteGuardian, Count = 2 }
                }, 30, CardRarity.Uncommon);

            CreateEncounterAsset("Elite_Ambush", "암살자 매복", TileType.Elite,
                new List<EncounterEnemyConfig>
                {
                    new EncounterEnemyConfig { Enemy = eliteAssassin, Count = 1 },
                    new EncounterEnemyConfig { Enemy = goblin, Count = 2 }
                }, 25, CardRarity.Uncommon);

            // 보스 인카운터
            CreateEncounterAsset("Boss_DragonLair", "드래곤의 둥지", TileType.Boss,
                new List<EncounterEnemyConfig>
                {
                    new EncounterEnemyConfig { Enemy = bossDragon, Count = 1 }
                }, 50, CardRarity.Rare);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnemyAssetGenerator] 기본 인카운터 에셋 생성 완료");
        }

        private struct EncounterEnemyConfig
        {
            public EnemyDataSO Enemy;
            public int Count;
            public int RandomMin;
            public int RandomMax;
        }

        private static void CreateEncounterAsset(string id, string name, TileType type,
            List<EncounterEnemyConfig> enemies, int goldReward, CardRarity cardRarity)
        {
            string path = $"{ENCOUNTERS_PATH}/{id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<EncounterDataSO>(path) != null)
            {
                Debug.Log($"[EnemyAssetGenerator] Encounter already exists: {id}");
                return;
            }

            var encounter = ScriptableObject.CreateInstance<EncounterDataSO>();

            var so = new SerializedObject(encounter);
            so.FindProperty("_encounterId").stringValue = id.ToLower();
            so.FindProperty("_encounterName").stringValue = name;
            so.FindProperty("_encounterType").enumValueIndex = (int)type;
            so.FindProperty("_baseGoldReward").intValue = goldReward;
            so.FindProperty("_cardRewardRarity").enumValueIndex = (int)cardRarity;

            // 적 목록 설정
            var enemiesProp = so.FindProperty("_enemies");
            enemiesProp.ClearArray();

            for (int i = 0; i < enemies.Count; i++)
            {
                var config = enemies[i];
                if (config.Enemy == null) continue;

                enemiesProp.InsertArrayElementAtIndex(i);
                var element = enemiesProp.GetArrayElementAtIndex(i);

                element.FindPropertyRelative("EnemyData").objectReferenceValue = config.Enemy;
                element.FindPropertyRelative("Count").intValue = config.Count > 0 ? config.Count : 1;
                element.FindPropertyRelative("RandomCountMin").intValue = config.RandomMin;
                element.FindPropertyRelative("RandomCountMax").intValue = config.RandomMax;
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(encounter, path);
            Debug.Log($"[EnemyAssetGenerator] Created encounter: {id}");
        }

        #endregion

        #region Encounter Pools

        private void CreateEncounterPools()
        {
            CreateEncounterPoolsStatic();
        }

        private static void CreateEncounterPoolsStatic()
        {
            // 인카운터 에셋 로드
            var normalSlime = AssetDatabase.LoadAssetAtPath<EncounterDataSO>($"{ENCOUNTERS_PATH}/Normal_SlimeGroup.asset");
            var normalGoblin = AssetDatabase.LoadAssetAtPath<EncounterDataSO>($"{ENCOUNTERS_PATH}/Normal_GoblinRaid.asset");
            var normalUndead = AssetDatabase.LoadAssetAtPath<EncounterDataSO>($"{ENCOUNTERS_PATH}/Normal_Undead.asset");
            var eliteGuardians = AssetDatabase.LoadAssetAtPath<EncounterDataSO>($"{ENCOUNTERS_PATH}/Elite_Guardians.asset");
            var eliteAmbush = AssetDatabase.LoadAssetAtPath<EncounterDataSO>($"{ENCOUNTERS_PATH}/Elite_Ambush.asset");
            var bossDragon = AssetDatabase.LoadAssetAtPath<EncounterDataSO>($"{ENCOUNTERS_PATH}/Boss_DragonLair.asset");

            // 일반 적 풀
            CreatePoolAsset("Act1_NormalPool", TileType.Enemy, 1,
                new List<EncounterDataSO> { normalSlime, normalGoblin, normalUndead });

            // 엘리트 풀
            CreatePoolAsset("Act1_ElitePool", TileType.Elite, 1,
                new List<EncounterDataSO> { eliteGuardians, eliteAmbush });

            // 보스 풀
            CreatePoolAsset("Act1_BossPool", TileType.Boss, 1,
                new List<EncounterDataSO> { bossDragon });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[EnemyAssetGenerator] 인카운터 풀 생성 완료");
        }

        private static void CreatePoolAsset(string id, TileType type, int actNumber,
            List<EncounterDataSO> encounters)
        {
            string path = $"{POOLS_PATH}/{id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<EncounterPoolSO>(path) != null)
            {
                Debug.Log($"[EnemyAssetGenerator] Pool already exists: {id}");
                return;
            }

            var pool = ScriptableObject.CreateInstance<EncounterPoolSO>();

            var so = new SerializedObject(pool);
            so.FindProperty("_poolId").stringValue = id.ToLower();
            so.FindProperty("_poolType").enumValueIndex = (int)type;
            so.FindProperty("_actNumber").intValue = actNumber;

            // 인카운터 목록 설정
            var encountersProp = so.FindProperty("_encounters");
            encountersProp.ClearArray();

            for (int i = 0; i < encounters.Count; i++)
            {
                if (encounters[i] == null) continue;

                encountersProp.InsertArrayElementAtIndex(i);
                encountersProp.GetArrayElementAtIndex(i).objectReferenceValue = encounters[i];
            }

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(pool, path);
            Debug.Log($"[EnemyAssetGenerator] Created pool: {id}");
        }

        #endregion

        #region Scene Linking

        private void LinkEncounterPoolsToScene()
        {
            LinkEncounterPoolsToSceneStatic();
        }

        public static void LinkEncounterPoolsToSceneStatic()
        {
            // Combat 씬의 CombatSceneInitializer 찾기
            var initializer = Object.FindObjectOfType<CombatSceneInitializer>();
            if (initializer == null)
            {
                Debug.LogWarning("[EnemyAssetGenerator] CombatSceneInitializer not found in scene. Skipping link.");
                return;
            }

            LinkToInitializer(initializer);
        }

        /// <summary>
        /// CombatSceneInitializer에 인카운터 풀 연결 (CombatSceneBuilder에서 호출)
        /// </summary>
        public static void LinkToInitializer(CombatSceneInitializer initializer)
        {
            if (initializer == null)
            {
                Debug.LogWarning("[EnemyAssetGenerator] CombatSceneInitializer is null. Skipping link.");
                return;
            }

            // 인카운터 풀 로드
            var normalPool = AssetDatabase.LoadAssetAtPath<EncounterPoolSO>($"{POOLS_PATH}/Act1_NormalPool.asset");
            var elitePool = AssetDatabase.LoadAssetAtPath<EncounterPoolSO>($"{POOLS_PATH}/Act1_ElitePool.asset");
            var bossPool = AssetDatabase.LoadAssetAtPath<EncounterPoolSO>($"{POOLS_PATH}/Act1_BossPool.asset");

            // SerializedObject를 통해 필드 설정
            var so = new SerializedObject(initializer);
            so.FindProperty("_normalEncounterPool").objectReferenceValue = normalPool;
            so.FindProperty("_eliteEncounterPool").objectReferenceValue = elitePool;
            so.FindProperty("_bossEncounterPool").objectReferenceValue = bossPool;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(initializer);
            Debug.Log("[EnemyAssetGenerator] 인카운터 풀을 CombatSceneInitializer에 연결 완료");
        }

        #endregion
    }
}
#endif
