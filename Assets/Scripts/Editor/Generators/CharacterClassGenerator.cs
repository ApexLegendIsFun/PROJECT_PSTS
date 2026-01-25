using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// TRIAD 캐릭터 클래스 데이터 생성기
    /// TRIAD character class data generator (Warrior, Mage, Rogue)
    /// </summary>
    public static class CharacterClassGenerator
    {
        #region Menu Items

        [MenuItem("Tools/Project SS/Generators/Generate TRIAD Classes")]
        public static void GenerateTriadClasses()
        {
            int created = 0;

            EditorUtility.DisplayProgressBar("TRIAD 클래스 생성", "전사(Warrior) 생성 중...", 0.1f);
            if (GenerateWarriorClass()) created++;

            EditorUtility.DisplayProgressBar("TRIAD 클래스 생성", "마법사(Mage) 생성 중...", 0.4f);
            if (GenerateMageClass()) created++;

            EditorUtility.DisplayProgressBar("TRIAD 클래스 생성", "도적(Rogue) 생성 중...", 0.7f);
            if (GenerateRogueClass()) created++;

            EditorUtility.DisplayProgressBar("TRIAD 클래스 생성", "완료 중...", 0.9f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            if (created > 0)
            {
                Debug.Log($"<color=green>✅ TRIAD 클래스 {created}개 생성 완료!</color>");
                EditorUtility.DisplayDialog("완료", $"캐릭터 클래스 {created}개가 생성되었습니다.", "확인");
            }
            else
            {
                Debug.Log("모든 TRIAD 클래스가 이미 존재합니다.");
                EditorUtility.DisplayDialog("알림", "모든 캐릭터 클래스가 이미 존재합니다.", "확인");
            }
        }

        [MenuItem("Tools/Project SS/Generators/Character Classes/Generate Warrior")]
        public static bool GenerateWarriorClass()
        {
            return CreateCharacterClass(
                classType: CharacterClass.Warrior,
                koreanName: "전사",
                englishName: "Warrior",
                baseMaxHP: 80,
                classColor: new Color(0.9f, 0.4f, 0.3f),  // Red-orange
                tagInBonusType: TagInBonusType.GainBlock,
                tagInBonusValue: 8,
                description: "튼튼한 방어력과 높은 체력을 가진 전사.\n전열에서 적의 공격을 막아내며 팀을 보호합니다.\n\n" +
                            "Sturdy defender with high HP.\nProtects the team from the frontline."
            );
        }

        [MenuItem("Tools/Project SS/Generators/Character Classes/Generate Mage")]
        public static bool GenerateMageClass()
        {
            return CreateCharacterClass(
                classType: CharacterClass.Mage,
                koreanName: "마법사",
                englishName: "Mage",
                baseMaxHP: 70,
                classColor: new Color(0.4f, 0.5f, 0.9f),  // Blue-purple
                tagInBonusType: TagInBonusType.DrawCard,
                tagInBonusValue: 2,
                description: "강력한 광역 마법과 버프를 사용하는 마법사.\n전략적인 카드 드로우로 팀의 옵션을 늘려줍니다.\n\n" +
                            "Powerful mage with AoE spells and buffs.\nDraws cards to expand team options."
            );
        }

        [MenuItem("Tools/Project SS/Generators/Character Classes/Generate Rogue")]
        public static bool GenerateRogueClass()
        {
            // Weak 상태효과 로드
            var weakEffect = GeneratorUtility.LoadStatusEffect(StatusEffectType.Weak);

            return CreateCharacterClass(
                classType: CharacterClass.Rogue,
                koreanName: "도적",
                englishName: "Rogue",
                baseMaxHP: 75,
                classColor: new Color(0.4f, 0.8f, 0.4f),  // Green
                tagInBonusType: TagInBonusType.ApplyDebuff,
                tagInBonusValue: 0,  // ApplyDebuff uses statusStacks instead
                tagInStatusEffect: weakEffect,
                tagInStatusStacks: 2,
                description: "빠른 연속 공격과 디버프에 특화된 도적.\n적에게 약화를 부여하여 팀의 생존력을 높여줍니다.\n\n" +
                            "Fast attacker specialized in debuffs.\nWeakens enemies to improve team survivability."
            );
        }

        #endregion

        #region Core Creation Methods

        /// <summary>
        /// 캐릭터 클래스 생성
        /// Create character class
        /// </summary>
        /// <returns>true if created, false if already exists</returns>
        public static bool CreateCharacterClass(
            CharacterClass classType,
            string koreanName,
            string englishName,
            int baseMaxHP,
            Color classColor,
            TagInBonusType tagInBonusType,
            int tagInBonusValue,
            StatusEffectData tagInStatusEffect = null,
            int tagInStatusStacks = 1,
            string description = null,
            List<CardData> starterDeck = null)
        {
            string fileName = GeneratorUtility.FormatCharacterClassFileName(koreanName, englishName);
            string fullPath = $"{GeneratorUtility.CHARACTERS_PATH}/{fileName}.asset";

            // 이미 존재하면 스킵
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"캐릭터 클래스 이미 존재: {fileName}");
                return false;
            }

            // 폴더 확인
            GeneratorUtility.EnsureFolderExists(GeneratorUtility.CHARACTERS_PATH);

            // 새 캐릭터 클래스 생성
            var classData = ScriptableObject.CreateInstance<CharacterClassData>();
            classData.classId = classType.ToString().ToLower();
            classData.className = $"{koreanName} ({englishName})";
            classData.classType = classType;
            classData.classColor = classColor;
            classData.baseMaxHP = baseMaxHP;
            classData.tagInBonusType = tagInBonusType;
            classData.tagInBonusValue = tagInBonusValue;
            classData.tagInStatusEffect = tagInStatusEffect;
            classData.tagInStatusStacks = tagInStatusStacks;
            classData.classDescription = description ?? GetDefaultDescription(classType);

            // 스타터 덱 설정
            if (starterDeck != null)
            {
                classData.starterDeck = starterDeck;
            }
            else
            {
                classData.starterDeck = GetDefaultStarterDeck(classType);
            }

            AssetDatabase.CreateAsset(classData, fullPath);
            Debug.Log($"캐릭터 클래스 생성: {fileName}");
            return true;
        }

        #endregion

        #region Starter Deck Configuration

        /// <summary>
        /// 전사 스타터 덱 반환
        /// Get Warrior starter deck
        /// </summary>
        public static List<CardData> GetWarriorStarterDeck()
        {
            var deck = new List<CardData>();

            // 기본 Strike x5
            var strike = LoadCard("ATK_강타_Strike");
            if (strike != null)
            {
                for (int i = 0; i < 5; i++)
                    deck.Add(strike);
            }

            // 기본 Defend x4
            var defend = LoadCard("DEF_방어_Defend");
            if (defend != null)
            {
                for (int i = 0; i < 4; i++)
                    deck.Add(defend);
            }

            // Bash x1
            var bash = LoadCard("ATK_강타_Bash");
            if (bash != null)
            {
                deck.Add(bash);
            }

            return deck;
        }

        /// <summary>
        /// 마법사 스타터 덱 반환
        /// Get Mage starter deck
        /// </summary>
        public static List<CardData> GetMageStarterDeck()
        {
            var deck = new List<CardData>();

            // 기본 Strike x4
            var strike = LoadCard("ATK_강타_Strike");
            if (strike != null)
            {
                for (int i = 0; i < 4; i++)
                    deck.Add(strike);
            }

            // 기본 Defend x5
            var defend = LoadCard("DEF_방어_Defend");
            if (defend != null)
            {
                for (int i = 0; i < 5; i++)
                    deck.Add(defend);
            }

            // TODO: Mage-specific starter card (Zap, etc.)
            // 마법사 전용 스타터 카드 추가 필요

            return deck;
        }

        /// <summary>
        /// 도적 스타터 덱 반환
        /// Get Rogue starter deck
        /// </summary>
        public static List<CardData> GetRogueStarterDeck()
        {
            var deck = new List<CardData>();

            // 기본 Strike x5
            var strike = LoadCard("ATK_강타_Strike");
            if (strike != null)
            {
                for (int i = 0; i < 5; i++)
                    deck.Add(strike);
            }

            // 기본 Defend x4
            var defend = LoadCard("DEF_방어_Defend");
            if (defend != null)
            {
                for (int i = 0; i < 4; i++)
                    deck.Add(defend);
            }

            // TODO: Rogue-specific starter card (Neutralize, etc.)
            // 도적 전용 스타터 카드 추가 필요

            return deck;
        }

        /// <summary>
        /// 기본 스타터 덱 반환
        /// Get default starter deck based on class type
        /// </summary>
        public static List<CardData> GetDefaultStarterDeck(CharacterClass classType)
        {
            return classType switch
            {
                CharacterClass.Warrior => GetWarriorStarterDeck(),
                CharacterClass.Mage => GetMageStarterDeck(),
                CharacterClass.Rogue => GetRogueStarterDeck(),
                _ => new List<CardData>()
            };
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 기본 클래스 설명 반환
        /// Get default class description
        /// </summary>
        private static string GetDefaultDescription(CharacterClass classType)
        {
            return classType switch
            {
                CharacterClass.Warrior => "방어와 도발에 특화된 전사.\nA warrior specialized in defense and taunting.",
                CharacterClass.Mage => "광역 마법과 버프에 특화된 마법사.\nA mage specialized in AoE spells and buffs.",
                CharacterClass.Rogue => "빠른 공격과 디버프에 특화된 도적.\nA rogue specialized in fast attacks and debuffs.",
                _ => ""
            };
        }

        /// <summary>
        /// 카드 로드
        /// Load card by filename
        /// </summary>
        private static CardData LoadCard(string fileName)
        {
            // Attack 폴더에서 먼저 검색
            string[] subfolders = { "Attack", "Defense", "Skill" };

            foreach (var subfolder in subfolders)
            {
                string path = $"{GeneratorUtility.CARDS_PATH}/{subfolder}/{fileName}.asset";
                var card = AssetDatabase.LoadAssetAtPath<CardData>(path);
                if (card != null) return card;
            }

            Debug.LogWarning($"카드를 찾을 수 없음: {fileName}");
            return null;
        }

        #endregion

        #region Validation

        /// <summary>
        /// 모든 TRIAD 클래스가 존재하는지 확인
        /// Check if all TRIAD classes exist
        /// </summary>
        public static bool ValidateAllClassesExist()
        {
            string[] requiredClasses = {
                "CLASS_전사_Warrior",
                "CLASS_마법사_Mage",
                "CLASS_도적_Rogue"
            };

            bool allExist = true;
            foreach (var fileName in requiredClasses)
            {
                string path = $"{GeneratorUtility.CHARACTERS_PATH}/{fileName}.asset";
                if (!System.IO.File.Exists(path))
                {
                    Debug.LogWarning($"캐릭터 클래스 누락: {fileName}");
                    allExist = false;
                }
            }

            return allExist;
        }

        /// <summary>
        /// 캐릭터 클래스 데이터 로드
        /// Load character class data by type
        /// </summary>
        public static CharacterClassData LoadCharacterClass(CharacterClass classType)
        {
            string fileName = classType switch
            {
                CharacterClass.Warrior => "CLASS_전사_Warrior",
                CharacterClass.Mage => "CLASS_마법사_Mage",
                CharacterClass.Rogue => "CLASS_도적_Rogue",
                _ => null
            };

            if (fileName == null) return null;

            string path = $"{GeneratorUtility.CHARACTERS_PATH}/{fileName}.asset";
            return AssetDatabase.LoadAssetAtPath<CharacterClassData>(path);
        }

        #endregion
    }
}
