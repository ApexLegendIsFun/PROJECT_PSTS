// Editor/CharacterAssetGenerator.cs
// 캐릭터 SO 에셋 생성 도구

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Editor
{
    /// <summary>
    /// 캐릭터 SO 에셋 생성 도구
    /// </summary>
    public class CharacterAssetGenerator : EditorWindow
    {
        private const string CHARACTERS_PATH = "Assets/Data/Characters";
        private const string DATABASE_PATH = "Assets/Resources/Config";

        [MenuItem("PSTS/Character Asset Generator")]
        public static void ShowWindow()
        {
            GetWindow<CharacterAssetGenerator>("Character Asset Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("캐릭터 에셋 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("폴더 구조 생성", GUILayout.Height(30)))
            {
                CreateFolderStructure();
            }

            GUILayout.Space(10);
            GUILayout.Label("캐릭터 SO 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("기본 캐릭터 에셋 생성 (검투사, 마법사, 암살자, 사제, 수호자)", GUILayout.Height(30)))
            {
                CreateBaseCharacterAssets();
            }

            GUILayout.Space(10);
            GUILayout.Label("캐릭터 데이터베이스", EditorStyles.boldLabel);

            if (GUILayout.Button("CharacterDatabase 에셋 생성 및 연결", GUILayout.Height(30)))
            {
                CreateCharacterDatabase();
            }

            GUILayout.Space(20);
            GUILayout.Label("전체 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("모두 생성 (폴더 + 캐릭터 + 데이터베이스)", GUILayout.Height(40)))
            {
                CreateAll();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("생성된 CharacterDatabase는 Resources/Config에 저장되어 DataService에서 자동 로드됩니다.", MessageType.Info);
        }

        /// <summary>
        /// 모든 에셋 생성
        /// </summary>
        private void CreateAll()
        {
            CreateFolderStructure();
            CreateBaseCharacterAssets();
            CreateCharacterDatabase();
            Debug.Log("[CharacterAssetGenerator] 모든 캐릭터 에셋 생성 완료!");
        }

        /// <summary>
        /// 정적 메서드: ProjectBootstrapper에서 호출
        /// </summary>
        public static void CreateAllAssetsStatic()
        {
            CreateFolderStructure();
            CreateBaseCharacterAssets();
            CreateCharacterDatabase();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CharacterAssetGenerator] 캐릭터 에셋 생성 완료");
        }

        #region Folder Structure

        private static void CreateFolderStructure()
        {
            CreateFolderIfNotExists("Assets/Data");
            CreateFolderIfNotExists(CHARACTERS_PATH);
            CreateFolderIfNotExists("Assets/Resources");
            CreateFolderIfNotExists(DATABASE_PATH);

            AssetDatabase.Refresh();
            Debug.Log("[CharacterAssetGenerator] 폴더 구조 생성 완료");
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

        #region Character Assets

        private static void CreateBaseCharacterAssets()
        {
            // 검투사 (Warrior) - 균형형
            CreateCharacterAsset(
                id: "warrior_01",
                displayName: "검투사",
                characterClass: CharacterClass.Warrior,
                maxHealth: 50,
                baseEnergy: 3,
                baseSpeed: 10,
                themeColor: new Color(0.8f, 0.3f, 0.3f, 1f),
                description: "균형 잡힌 전투 스타일의 전사. 공격과 방어 모두 능숙하다."
            );

            // 마법사 (Mage) - 고에너지, 저체력
            CreateCharacterAsset(
                id: "mage_01",
                displayName: "마법사",
                characterClass: CharacterClass.Mage,
                maxHealth: 40,
                baseEnergy: 4,
                baseSpeed: 8,
                themeColor: new Color(0.3f, 0.3f, 0.8f, 1f),
                description: "강력한 마법을 사용하는 주문술사. 높은 에너지와 다양한 스킬을 보유."
            );

            // 암살자 (Rogue) - 고속, 저체력
            CreateCharacterAsset(
                id: "rogue_01",
                displayName: "암살자",
                characterClass: CharacterClass.Rogue,
                maxHealth: 35,
                baseEnergy: 3,
                baseSpeed: 14,
                themeColor: new Color(0.3f, 0.8f, 0.3f, 1f),
                description: "빠른 속도로 적을 제압하는 암살자. 선제 공격에 강하다."
            );

            // 사제 (Healer) - 지원형
            CreateCharacterAsset(
                id: "healer_01",
                displayName: "사제",
                characterClass: CharacterClass.Healer,
                maxHealth: 45,
                baseEnergy: 3,
                baseSpeed: 12,
                themeColor: new Color(0.9f, 0.8f, 0.3f, 1f),
                description: "파티를 치유하고 보호하는 성직자. 버프와 힐링에 특화."
            );

            // 수호자 (Tank) - 고체력, 저에너지
            CreateCharacterAsset(
                id: "tank_01",
                displayName: "수호자",
                characterClass: CharacterClass.Tank,
                maxHealth: 60,
                baseEnergy: 2,
                baseSpeed: 8,
                themeColor: new Color(0.5f, 0.5f, 0.5f, 1f),
                description: "파티를 보호하는 방패. 높은 체력과 강력한 방어 스킬."
            );

            AssetDatabase.SaveAssets();
            Debug.Log("[CharacterAssetGenerator] 기본 캐릭터 에셋 생성 완료 (5개)");
        }

        private static void CreateCharacterAsset(
            string id,
            string displayName,
            CharacterClass characterClass,
            int maxHealth,
            int baseEnergy,
            int baseSpeed,
            Color themeColor,
            string description)
        {
            string assetPath = $"{CHARACTERS_PATH}/{id}.asset";

            // 이미 존재하면 스킵
            if (AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath) != null)
            {
                Debug.Log($"[CharacterAssetGenerator] 캐릭터 이미 존재: {displayName}");
                return;
            }

            var character = ScriptableObject.CreateInstance<CharacterData>();
            character.Id = id;
            character.Name = displayName;
            character.Class = characterClass;
            character.MaxHealth = maxHealth;
            character.BaseEnergy = baseEnergy;
            character.BaseSpeed = baseSpeed;
            character.ThemeColor = themeColor;
            character.Description = description;

            AssetDatabase.CreateAsset(character, assetPath);
            Debug.Log($"[CharacterAssetGenerator] 캐릭터 생성: {displayName}");
        }

        #endregion

        #region Character Database

        private static void CreateCharacterDatabase()
        {
            string dbPath = $"{DATABASE_PATH}/CharacterDatabase.asset";

            // 이미 존재하면 업데이트
            var existingDb = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(dbPath);
            if (existingDb != null)
            {
                UpdateCharacterDatabase(existingDb);
                return;
            }

            // 새로 생성
            var database = ScriptableObject.CreateInstance<CharacterDatabase>();
            PopulateCharacterDatabase(database);

            AssetDatabase.CreateAsset(database, dbPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[CharacterAssetGenerator] CharacterDatabase 생성 완료");
        }

        private static void UpdateCharacterDatabase(CharacterDatabase database)
        {
            PopulateCharacterDatabase(database);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log("[CharacterAssetGenerator] CharacterDatabase 업데이트 완료");
        }

        private static void PopulateCharacterDatabase(CharacterDatabase database)
        {
            database.Characters.Clear();

            // Characters 폴더의 모든 CharacterData 에셋 로드
            string[] guids = AssetDatabase.FindAssets("t:CharacterData", new[] { CHARACTERS_PATH });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var character = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (character != null)
                {
                    database.Characters.Add(character);
                }
            }

            Debug.Log($"[CharacterAssetGenerator] CharacterDatabase에 {database.Characters.Count}개 캐릭터 등록");
        }

        #endregion
    }
}
#endif
