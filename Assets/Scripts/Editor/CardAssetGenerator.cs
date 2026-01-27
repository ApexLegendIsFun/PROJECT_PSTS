// Editor/CardAssetGenerator.cs
// 카드 및 효과 SO 에셋 생성 도구

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Collections.Generic;
using ProjectSS.Core;
using ProjectSS.Data.Cards;
using ProjectSS.Combat;

namespace ProjectSS.Editor
{
    /// <summary>
    /// 카드 및 효과 SO 에셋 생성 도구
    /// </summary>
    public class CardAssetGenerator : EditorWindow
    {
        private const string BASE_PATH = "Assets/Data/Cards";
        private const string EFFECTS_PATH = "Assets/Data/Cards/Effects";
        private const string POOLS_PATH = "Assets/Data/Cards/CharacterPools";

        [MenuItem("PSTS/Card Asset Generator")]
        public static void ShowWindow()
        {
            GetWindow<CardAssetGenerator>("Card Asset Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("카드 에셋 생성 도구", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("폴더 구조 생성", GUILayout.Height(30)))
            {
                CreateFolderStructure();
            }

            GUILayout.Space(10);
            GUILayout.Label("기본 효과 SO 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("기본 효과 에셋 생성 (Damage, Block, Draw)", GUILayout.Height(30)))
            {
                CreateBaseEffectAssets();
            }

            GUILayout.Space(10);
            GUILayout.Label("기본 카드 SO 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("기본 카드 에셋 생성 (Strike, Defend)", GUILayout.Height(30)))
            {
                CreateBaseCardAssets();
            }

            GUILayout.Space(10);
            GUILayout.Label("캐릭터 카드풀 SO 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("캐릭터별 카드풀 생성 (Warrior, Mage, Healer)", GUILayout.Height(30)))
            {
                CreateCharacterCardPools();
            }

            GUILayout.Space(20);
            GUILayout.Label("전체 생성", EditorStyles.boldLabel);

            if (GUILayout.Button("모두 생성 (폴더 + 효과 + 카드 + 카드풀)", GUILayout.Height(40)))
            {
                CreateAll();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("'모두 생성' 버튼을 누르면 씬의 CombatSceneInitializer에 자동으로 연결됩니다.", MessageType.Info);

            GUILayout.Space(10);
            if (GUILayout.Button("씬에 카드풀 연결", GUILayout.Height(30)))
            {
                LinkCardPoolsToScene();
            }
        }

        /// <summary>
        /// 모든 에셋 생성 및 씬 연결
        /// </summary>
        private void CreateAll()
        {
            CreateFolderStructure();
            CreateBaseEffectAssets();
            CreateBaseCardAssets();
            CreateCharacterCardPools();
            LinkCardPoolsToScene();
            Debug.Log("[CardAssetGenerator] 모든 에셋 생성 및 연결 완료!");
        }

        /// <summary>
        /// 폴더 구조 생성
        /// </summary>
        private void CreateFolderStructure()
        {
            CreateFolderIfNotExists(BASE_PATH);
            CreateFolderIfNotExists(EFFECTS_PATH);
            CreateFolderIfNotExists($"{EFFECTS_PATH}/Damage");
            CreateFolderIfNotExists($"{EFFECTS_PATH}/Block");
            CreateFolderIfNotExists($"{EFFECTS_PATH}/Draw");
            CreateFolderIfNotExists($"{EFFECTS_PATH}/Status");
            CreateFolderIfNotExists($"{BASE_PATH}/Starter");
            CreateFolderIfNotExists($"{BASE_PATH}/Common");
            CreateFolderIfNotExists($"{BASE_PATH}/Uncommon");
            CreateFolderIfNotExists($"{BASE_PATH}/Rare");
            CreateFolderIfNotExists(POOLS_PATH);

            AssetDatabase.Refresh();
            Debug.Log("[CardAssetGenerator] 폴더 구조 생성 완료!");
        }

        /// <summary>
        /// 기본 효과 에셋 생성
        /// </summary>
        private void CreateBaseEffectAssets()
        {
            CreateFolderStructure();

            // Damage Effects
            CreateDamageEffect("Damage_6", 6, TargetType.SingleEnemy);
            CreateDamageEffect("Damage_8", 8, TargetType.SingleEnemy);
            CreateDamageEffect("Damage_12", 12, TargetType.SingleEnemy);
            CreateDamageEffect("DamageAll_4", 4, TargetType.AllEnemies);

            // Block Effects
            CreateBlockEffect("Block_5", 5);
            CreateBlockEffect("Block_8", 8);
            CreateBlockEffect("Block_12", 12);

            // Draw Effects
            CreateDrawEffect("Draw_1", 1);
            CreateDrawEffect("Draw_2", 2);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CardAssetGenerator] 기본 효과 에셋 생성 완료!");
        }

        /// <summary>
        /// 기본 카드 에셋 생성
        /// </summary>
        private void CreateBaseCardAssets()
        {
            CreateFolderStructure();

            // 효과 에셋 로드
            var damage6 = AssetDatabase.LoadAssetAtPath<DamageEffectSO>($"{EFFECTS_PATH}/Damage/Damage_6.asset");
            var damage8 = AssetDatabase.LoadAssetAtPath<DamageEffectSO>($"{EFFECTS_PATH}/Damage/Damage_8.asset");
            var block5 = AssetDatabase.LoadAssetAtPath<BlockEffectSO>($"{EFFECTS_PATH}/Block/Block_5.asset");
            var block8 = AssetDatabase.LoadAssetAtPath<BlockEffectSO>($"{EFFECTS_PATH}/Block/Block_8.asset");
            var draw1 = AssetDatabase.LoadAssetAtPath<DrawEffectSO>($"{EFFECTS_PATH}/Draw/Draw_1.asset");

            // 효과가 없으면 먼저 생성
            if (damage6 == null || block5 == null)
            {
                Debug.LogWarning("[CardAssetGenerator] 효과 에셋이 없습니다. 먼저 효과 에셋을 생성합니다.");
                CreateBaseEffectAssets();

                // 다시 로드
                damage6 = AssetDatabase.LoadAssetAtPath<DamageEffectSO>($"{EFFECTS_PATH}/Damage/Damage_6.asset");
                damage8 = AssetDatabase.LoadAssetAtPath<DamageEffectSO>($"{EFFECTS_PATH}/Damage/Damage_8.asset");
                block5 = AssetDatabase.LoadAssetAtPath<BlockEffectSO>($"{EFFECTS_PATH}/Block/Block_5.asset");
                block8 = AssetDatabase.LoadAssetAtPath<BlockEffectSO>($"{EFFECTS_PATH}/Block/Block_8.asset");
                draw1 = AssetDatabase.LoadAssetAtPath<DrawEffectSO>($"{EFFECTS_PATH}/Draw/Draw_1.asset");
            }

            // Strike 카드
            CreateCardData(
                path: $"{BASE_PATH}/Starter/Strike.asset",
                cardId: "strike",
                cardName: "강타",
                description: "적에게 6 데미지를 줍니다.",
                upgradeDescription: "적에게 8 데미지를 줍니다.",
                cardType: CardType.Attack,
                rarity: CardRarity.Starter,
                targetType: TargetType.SingleEnemy,
                energyCost: 1,
                upgradedEnergyCost: 1,
                effects: new CardEffectSO[] { damage6 },
                upgradedEffects: new CardEffectSO[] { damage8 }
            );

            // Defend 카드
            CreateCardData(
                path: $"{BASE_PATH}/Starter/Defend.asset",
                cardId: "defend",
                cardName: "수비",
                description: "5 방어막을 얻습니다.",
                upgradeDescription: "8 방어막을 얻습니다.",
                cardType: CardType.Skill,
                rarity: CardRarity.Starter,
                targetType: TargetType.Self,
                energyCost: 1,
                upgradedEnergyCost: 1,
                effects: new CardEffectSO[] { block5 },
                upgradedEffects: new CardEffectSO[] { block8 }
            );

            // Insight 카드 (드로우)
            if (draw1 != null)
            {
                CreateCardData(
                    path: $"{BASE_PATH}/Common/Insight.asset",
                    cardId: "insight",
                    cardName: "통찰",
                    description: "카드 1장을 뽑습니다.",
                    upgradeDescription: "카드 2장을 뽑습니다.",
                    cardType: CardType.Skill,
                    rarity: CardRarity.Common,
                    targetType: TargetType.Self,
                    energyCost: 1,
                    upgradedEnergyCost: 0,
                    effects: new CardEffectSO[] { draw1 },
                    upgradedEffects: null
                );
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CardAssetGenerator] 기본 카드 에셋 생성 완료!");
        }

        #region Asset Creation Helpers

        private void CreateDamageEffect(string name, int damage, TargetType targetType)
        {
            string path = $"{EFFECTS_PATH}/Damage/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<DamageEffectSO>(path) != null)
            {
                Debug.Log($"[CardAssetGenerator] 이미 존재: {path}");
                return;
            }

            var effect = ScriptableObject.CreateInstance<DamageEffectSO>();
            SetPrivateField(effect, "_baseDamage", damage);
            SetPrivateField(effect, "_targetType", targetType);
            SetPrivateField(effect, "_scalesWithStrength", true);
            SetPrivateField(effect, "_description", $"{damage} 데미지");

            AssetDatabase.CreateAsset(effect, path);
            Debug.Log($"[CardAssetGenerator] 생성: {path}");
        }

        private void CreateBlockEffect(string name, int block)
        {
            string path = $"{EFFECTS_PATH}/Block/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<BlockEffectSO>(path) != null)
            {
                Debug.Log($"[CardAssetGenerator] 이미 존재: {path}");
                return;
            }

            var effect = ScriptableObject.CreateInstance<BlockEffectSO>();
            SetPrivateField(effect, "_baseBlock", block);
            SetPrivateField(effect, "_scalesWithDexterity", true);
            SetPrivateField(effect, "_description", $"{block} 방어막");

            AssetDatabase.CreateAsset(effect, path);
            Debug.Log($"[CardAssetGenerator] 생성: {path}");
        }

        private void CreateDrawEffect(string name, int count)
        {
            string path = $"{EFFECTS_PATH}/Draw/{name}.asset";
            if (AssetDatabase.LoadAssetAtPath<DrawEffectSO>(path) != null)
            {
                Debug.Log($"[CardAssetGenerator] 이미 존재: {path}");
                return;
            }

            var effect = ScriptableObject.CreateInstance<DrawEffectSO>();
            SetPrivateField(effect, "_drawCount", count);
            SetPrivateField(effect, "_description", $"카드 {count}장 드로우");

            AssetDatabase.CreateAsset(effect, path);
            Debug.Log($"[CardAssetGenerator] 생성: {path}");
        }

        private void CreateCardData(
            string path,
            string cardId,
            string cardName,
            string description,
            string upgradeDescription,
            CardType cardType,
            CardRarity rarity,
            TargetType targetType,
            int energyCost,
            int upgradedEnergyCost,
            CardEffectSO[] effects,
            CardEffectSO[] upgradedEffects)
        {
            if (AssetDatabase.LoadAssetAtPath<CardDataSO>(path) != null)
            {
                Debug.Log($"[CardAssetGenerator] 이미 존재: {path}");
                return;
            }

            var card = ScriptableObject.CreateInstance<CardDataSO>();
            SetPrivateField(card, "_cardId", cardId);
            SetPrivateField(card, "_cardName", cardName);
            SetPrivateField(card, "_description", description);
            SetPrivateField(card, "_upgradeDescription", upgradeDescription);
            SetPrivateField(card, "_cardType", cardType);
            SetPrivateField(card, "_rarity", rarity);
            SetPrivateField(card, "_targetType", targetType);
            SetPrivateField(card, "_energyCost", energyCost);
            SetPrivateField(card, "_upgradedEnergyCost", upgradedEnergyCost);

            if (effects != null)
            {
                var effectList = new System.Collections.Generic.List<CardEffectSO>(effects);
                SetPrivateField(card, "_effects", effectList);
            }

            if (upgradedEffects != null)
            {
                var upgradedList = new System.Collections.Generic.List<CardEffectSO>(upgradedEffects);
                SetPrivateField(card, "_upgradedEffects", upgradedList);
            }

            AssetDatabase.CreateAsset(card, path);
            Debug.Log($"[CardAssetGenerator] 생성: {path}");
        }

        private void CreateFolderIfNotExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parentFolder = Path.GetDirectoryName(path).Replace("\\", "/");
                string newFolderName = Path.GetFileName(path);

                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    CreateFolderIfNotExists(parentFolder);
                }

                AssetDatabase.CreateFolder(parentFolder, newFolderName);
                Debug.Log($"[CardAssetGenerator] 폴더 생성: {path}");
            }
        }

        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[CardAssetGenerator] 필드를 찾을 수 없음: {fieldName}");
            }
        }

        #endregion

        #region Character Card Pool Creation

        /// <summary>
        /// 캐릭터별 카드풀 생성
        /// </summary>
        private void CreateCharacterCardPools()
        {
            CreateFolderStructure();

            // 카드 에셋 로드
            var strike = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{BASE_PATH}/Starter/Strike.asset");
            var defend = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{BASE_PATH}/Starter/Defend.asset");

            if (strike == null || defend == null)
            {
                Debug.LogWarning("[CardAssetGenerator] 카드 에셋이 없습니다. 먼저 카드 에셋을 생성합니다.");
                CreateBaseEffectAssets();
                CreateBaseCardAssets();

                strike = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{BASE_PATH}/Starter/Strike.asset");
                defend = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{BASE_PATH}/Starter/Defend.asset");
            }

            // 각 캐릭터별 카드풀 생성
            CreateCharacterCardPool("warrior", "전사", strike, defend, 4, 4);
            CreateCharacterCardPool("mage", "마법사", strike, defend, 3, 5);
            CreateCharacterCardPool("healer", "힐러", strike, defend, 2, 6);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CardAssetGenerator] 캐릭터 카드풀 생성 완료!");
        }

        /// <summary>
        /// 개별 캐릭터 카드풀 생성
        /// </summary>
        private void CreateCharacterCardPool(string characterId, string characterName, CardDataSO strike, CardDataSO defend, int strikeCount, int defendCount)
        {
            string path = $"{POOLS_PATH}/{characterId}_CardPool.asset";

            // 이미 존재하면 업데이트
            var existing = AssetDatabase.LoadAssetAtPath<CharacterCardPoolSO>(path);
            if (existing != null)
            {
                // 기존 에셋 업데이트
                UpdateCardPool(existing, characterId, characterName, strike, defend, strikeCount, defendCount);
                EditorUtility.SetDirty(existing);
                Debug.Log($"[CardAssetGenerator] 업데이트: {path}");
                return;
            }

            var pool = ScriptableObject.CreateInstance<CharacterCardPoolSO>();
            UpdateCardPool(pool, characterId, characterName, strike, defend, strikeCount, defendCount);

            AssetDatabase.CreateAsset(pool, path);
            Debug.Log($"[CardAssetGenerator] 생성: {path}");
        }

        /// <summary>
        /// 카드풀 데이터 설정
        /// </summary>
        private void UpdateCardPool(CharacterCardPoolSO pool, string characterId, string characterName, CardDataSO strike, CardDataSO defend, int strikeCount, int defendCount)
        {
            SetPrivateField(pool, "_characterId", characterId);
            SetPrivateField(pool, "_characterName", characterName);

            // 시작 덱 구성
            var starterDeck = new List<StarterCardEntry>();

            if (strike != null)
            {
                starterDeck.Add(new StarterCardEntry { Card = strike, Count = strikeCount });
            }

            if (defend != null)
            {
                starterDeck.Add(new StarterCardEntry { Card = defend, Count = defendCount });
            }

            SetPrivateField(pool, "_starterDeck", starterDeck);
        }

        #endregion

        #region Static Methods (외부 호출용)

        /// <summary>
        /// 모든 카드 에셋 생성 (ProjectBootstrapper에서 호출)
        /// </summary>
        public static void CreateAllAssetsStatic()
        {
            // 인스턴스 생성하여 메서드 호출
            var generator = CreateInstance<CardAssetGenerator>();
            generator.CreateFolderStructure();
            generator.CreateBaseEffectAssets();
            generator.CreateBaseCardAssets();
            generator.CreateCharacterCardPools();
            DestroyImmediate(generator);

            Debug.Log("[CardAssetGenerator] Static: 모든 에셋 생성 완료");
        }

        /// <summary>
        /// CombatSceneInitializer에 카드풀 연결 (외부 호출용)
        /// </summary>
        public static void LinkToInitializer(CombatSceneInitializer initializer)
        {
            if (initializer == null) return;

            var warriorPool = AssetDatabase.LoadAssetAtPath<CharacterCardPoolSO>($"{POOLS_PATH}/warrior_CardPool.asset");
            var magePool = AssetDatabase.LoadAssetAtPath<CharacterCardPoolSO>($"{POOLS_PATH}/mage_CardPool.asset");
            var healerPool = AssetDatabase.LoadAssetAtPath<CharacterCardPoolSO>($"{POOLS_PATH}/healer_CardPool.asset");
            var strike = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{BASE_PATH}/Starter/Strike.asset");
            var defend = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{BASE_PATH}/Starter/Defend.asset");

            var so = new SerializedObject(initializer);
            var warriorPoolProp = so.FindProperty("_warriorCardPool");
            var magePoolProp = so.FindProperty("_mageCardPool");
            var healerPoolProp = so.FindProperty("_healerCardPool");
            var strikeCardProp = so.FindProperty("_testStrikeCard");
            var defendCardProp = so.FindProperty("_testDefendCard");

            if (warriorPoolProp != null) warriorPoolProp.objectReferenceValue = warriorPool;
            if (magePoolProp != null) magePoolProp.objectReferenceValue = magePool;
            if (healerPoolProp != null) healerPoolProp.objectReferenceValue = healerPool;
            if (strikeCardProp != null) strikeCardProp.objectReferenceValue = strike;
            if (defendCardProp != null) defendCardProp.objectReferenceValue = defend;

            so.ApplyModifiedProperties();
            Debug.Log("[CardAssetGenerator] 카드풀 연결 완료");
        }

        #endregion

        #region Scene Linking

        /// <summary>
        /// 씬의 CombatSceneInitializer에 카드풀 연결
        /// </summary>
        private void LinkCardPoolsToScene()
        {
            // 카드풀 로드
            var warriorPool = AssetDatabase.LoadAssetAtPath<CharacterCardPoolSO>($"{POOLS_PATH}/warrior_CardPool.asset");
            var magePool = AssetDatabase.LoadAssetAtPath<CharacterCardPoolSO>($"{POOLS_PATH}/mage_CardPool.asset");
            var healerPool = AssetDatabase.LoadAssetAtPath<CharacterCardPoolSO>($"{POOLS_PATH}/healer_CardPool.asset");

            // 카드 로드 (폴백용)
            var strike = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{BASE_PATH}/Starter/Strike.asset");
            var defend = AssetDatabase.LoadAssetAtPath<CardDataSO>($"{BASE_PATH}/Starter/Defend.asset");

            if (warriorPool == null || magePool == null || healerPool == null)
            {
                Debug.LogWarning("[CardAssetGenerator] 카드풀이 없습니다. 먼저 카드풀을 생성하세요.");
                return;
            }

            // 씬에서 CombatSceneInitializer 찾기
            var initializer = Object.FindObjectOfType<CombatSceneInitializer>();

            if (initializer == null)
            {
                Debug.LogWarning("[CardAssetGenerator] 씬에 CombatSceneInitializer가 없습니다.");
                return;
            }

            // SerializedObject로 private 필드 설정
            var so = new SerializedObject(initializer);

            var warriorPoolProp = so.FindProperty("_warriorCardPool");
            var magePoolProp = so.FindProperty("_mageCardPool");
            var healerPoolProp = so.FindProperty("_healerCardPool");
            var strikeCardProp = so.FindProperty("_testStrikeCard");
            var defendCardProp = so.FindProperty("_testDefendCard");

            if (warriorPoolProp != null) warriorPoolProp.objectReferenceValue = warriorPool;
            if (magePoolProp != null) magePoolProp.objectReferenceValue = magePool;
            if (healerPoolProp != null) healerPoolProp.objectReferenceValue = healerPool;
            if (strikeCardProp != null) strikeCardProp.objectReferenceValue = strike;
            if (defendCardProp != null) defendCardProp.objectReferenceValue = defend;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(initializer);

            // 씬 변경 표시
            EditorSceneManager.MarkSceneDirty(initializer.gameObject.scene);

            Debug.Log("[CardAssetGenerator] CombatSceneInitializer에 카드풀 연결 완료! 씬을 저장하세요.");
        }

        #endregion
    }
}
#endif
