using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// Generator 공용 유틸리티
    /// Shared utilities for all generators
    /// </summary>
    public static class GeneratorUtility
    {
        #region Path Constants

        public const string DATA_PATH = "Assets/_Project/Data";
        public const string CARDS_PATH = DATA_PATH + "/Cards";
        public const string ENEMIES_PATH = DATA_PATH + "/Enemies";
        public const string STATUS_PATH = DATA_PATH + "/StatusEffects";
        public const string RELICS_PATH = DATA_PATH + "/Relics";
        public const string EVENTS_PATH = DATA_PATH + "/Events";
        public const string CHARACTERS_PATH = DATA_PATH + "/Characters";
        public const string MAP_PATH = DATA_PATH + "/Map";

        #endregion

        #region File Name Formatters

        /// <summary>
        /// 카드 파일명 포맷
        /// Format card file name: ATK_강타_Strike.asset
        /// </summary>
        public static string FormatCardFileName(CardType type, string koreanName, string englishName)
        {
            string prefix = type switch
            {
                CardType.Attack => "ATK",
                CardType.Defense => "DEF",
                CardType.Skill => "SKL",
                _ => "CRD"
            };
            return $"{prefix}_{koreanName}_{englishName}";
        }

        /// <summary>
        /// 적 파일명 포맷
        /// Format enemy file name: EN_슬라임_Slime.asset
        /// </summary>
        public static string FormatEnemyFileName(EnemyType type, string koreanName, string englishName)
        {
            string prefix = type switch
            {
                EnemyType.Normal => "EN",
                EnemyType.Elite => "ELITE",
                EnemyType.Boss => "BOSS",
                _ => "EN"
            };
            return $"{prefix}_{koreanName}_{englishName}";
        }

        /// <summary>
        /// 상태효과 파일명 포맷
        /// Format status effect file name: STF_힘_Strength.asset
        /// </summary>
        public static string FormatStatusFileName(string koreanName, string englishName)
        {
            return $"STF_{koreanName}_{englishName}";
        }

        /// <summary>
        /// 유물 파일명 포맷
        /// Format relic file name: REL_부러진왕관_BrokenCrown.asset
        /// </summary>
        public static string FormatRelicFileName(string koreanName, string englishName)
        {
            return $"REL_{koreanName}_{englishName}";
        }

        /// <summary>
        /// 이벤트 파일명 포맷
        /// Format event file name: EVT_신비로운상자_MysteriousChest.asset
        /// </summary>
        public static string FormatEventFileName(string koreanName, string englishName)
        {
            return $"EVT_{koreanName}_{englishName}";
        }

        /// <summary>
        /// 캐릭터 클래스 파일명 포맷
        /// Format character class file name: CLASS_전사_Warrior.asset
        /// </summary>
        public static string FormatCharacterClassFileName(string koreanName, string englishName)
        {
            return $"CLASS_{koreanName}_{englishName}";
        }

        #endregion

        #region Subfolder Helpers

        /// <summary>
        /// 카드 타입에 따른 서브폴더 반환
        /// Get subfolder based on card type
        /// </summary>
        public static string GetCardSubfolder(CardType type)
        {
            return type switch
            {
                CardType.Attack => "Attack",
                CardType.Defense => "Defense",
                CardType.Skill => "Skill",
                _ => "Attack"
            };
        }

        /// <summary>
        /// 적 타입에 따른 서브폴더 반환
        /// Get subfolder based on enemy type
        /// </summary>
        public static string GetEnemySubfolder(EnemyType type)
        {
            return type switch
            {
                EnemyType.Normal => "Normal",
                EnemyType.Elite => "Elite",
                EnemyType.Boss => "Boss",
                _ => "Normal"
            };
        }

        #endregion

        #region Asset Creation

        /// <summary>
        /// 에셋이 존재하지 않으면 생성
        /// Create asset if it doesn't exist
        /// </summary>
        public static T CreateAssetIfNotExists<T>(string folderPath, string fileName) where T : ScriptableObject
        {
            string fullPath = $"{folderPath}/{fileName}.asset";

            // 이미 존재하면 기존 에셋 반환
            if (File.Exists(fullPath))
            {
                Debug.Log($"에셋 이미 존재: {fileName}");
                return AssetDatabase.LoadAssetAtPath<T>(fullPath);
            }

            // 폴더가 없으면 생성
            EnsureFolderExists(folderPath);

            // 새 에셋 생성
            var asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, fullPath);
            Debug.Log($"에셋 생성: {fileName}");
            return asset;
        }

        /// <summary>
        /// 에셋 저장 (덮어쓰기 확인 포함)
        /// Save asset with overwrite confirmation
        /// </summary>
        public static void SaveAsset<T>(T asset, string folderPath, string fileName, bool confirmOverwrite = true) where T : ScriptableObject
        {
            string fullPath = $"{folderPath}/{fileName}.asset";

            // 이미 존재하는 경우
            if (File.Exists(fullPath))
            {
                if (confirmOverwrite)
                {
                    if (!EditorUtility.DisplayDialog(
                        "에셋 덮어쓰기",
                        $"{fileName}이(가) 이미 존재합니다.\n덮어쓰시겠습니까?",
                        "덮어쓰기", "취소"))
                    {
                        return;
                    }
                }

                // 기존 에셋 삭제 후 새로 생성
                AssetDatabase.DeleteAsset(fullPath);
            }

            // 폴더가 없으면 생성
            EnsureFolderExists(folderPath);

            // 에셋 저장
            AssetDatabase.CreateAsset(asset, fullPath);
            EditorUtility.SetDirty(asset);
            Debug.Log($"에셋 저장: {fileName}");
        }

        /// <summary>
        /// 폴더가 없으면 생성
        /// Ensure folder exists
        /// </summary>
        public static void EnsureFolderExists(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = Path.GetFileName(path);

            // 부모 폴더도 확인
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolderExists(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }

        #endregion

        #region Color Helpers

        /// <summary>
        /// 카드 타입 색상 반환
        /// Get card type color
        /// </summary>
        public static Color GetCardTypeColor(CardType type)
        {
            return type switch
            {
                CardType.Attack => new Color(0.9f, 0.3f, 0.3f),   // Red
                CardType.Defense => new Color(0.3f, 0.5f, 0.9f), // Blue
                CardType.Skill => new Color(0.3f, 0.8f, 0.4f),   // Green
                _ => Color.white
            };
        }

        /// <summary>
        /// 적 타입 색상 반환
        /// Get enemy type color
        /// </summary>
        public static Color GetEnemyTypeColor(EnemyType type)
        {
            return type switch
            {
                EnemyType.Normal => new Color(0.7f, 0.7f, 0.7f),  // Gray
                EnemyType.Elite => new Color(1f, 0.8f, 0.2f),     // Gold
                EnemyType.Boss => new Color(0.9f, 0.2f, 0.2f),    // Red
                _ => Color.white
            };
        }

        /// <summary>
        /// 카드 희귀도 색상 반환
        /// Get card rarity color
        /// </summary>
        public static Color GetRarityColor(CardRarity rarity)
        {
            return rarity switch
            {
                CardRarity.Starter => new Color(0.6f, 0.6f, 0.6f),   // Gray
                CardRarity.Common => Color.white,                     // White
                CardRarity.Uncommon => new Color(0.3f, 0.7f, 0.9f),  // Blue
                CardRarity.Rare => new Color(1f, 0.8f, 0.2f),        // Gold
                _ => Color.white
            };
        }

        /// <summary>
        /// 유물 희귀도 색상 반환
        /// Get relic rarity color
        /// </summary>
        public static Color GetRelicRarityColor(RelicRarity rarity)
        {
            return rarity switch
            {
                RelicRarity.Starter => new Color(0.6f, 0.6f, 0.6f),   // Gray
                RelicRarity.Common => Color.white,                     // White
                RelicRarity.Uncommon => new Color(0.3f, 0.7f, 0.9f),  // Blue
                RelicRarity.Rare => new Color(1f, 0.8f, 0.2f),        // Gold
                RelicRarity.Boss => new Color(0.9f, 0.2f, 0.2f),      // Red
                RelicRarity.Event => new Color(0.6f, 0.3f, 0.9f),     // Purple
                _ => Color.white
            };
        }

        #endregion

        #region Validation Helpers

        /// <summary>
        /// 한글 이름 유효성 검사
        /// Validate Korean name
        /// </summary>
        public static bool ValidateKoreanName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            // 공백, 특수문자 제외 (한글, 숫자만 허용)
            foreach (char c in name)
            {
                if (!IsKorean(c) && !char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 영문 이름 유효성 검사
        /// Validate English name
        /// </summary>
        public static bool ValidateEnglishName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            // 공백, 특수문자 제외 (영문, 숫자만 허용)
            foreach (char c in name)
            {
                if (!char.IsLetter(c) && !char.IsDigit(c))
                    return false;
            }
            return true;
        }

        private static bool IsKorean(char c)
        {
            // 한글 유니코드 범위
            return (c >= '\uAC00' && c <= '\uD7AF') ||  // 한글 음절
                   (c >= '\u1100' && c <= '\u11FF') ||  // 한글 자모
                   (c >= '\u3130' && c <= '\u318F');    // 호환용 한글 자모
        }

        #endregion

        #region Status Effect Lookup

        /// <summary>
        /// 상태효과 데이터 로드
        /// Load status effect data by type
        /// </summary>
        public static StatusEffectData LoadStatusEffect(StatusEffectType type)
        {
            string fileName = type switch
            {
                StatusEffectType.Strength => "STF_힘_Strength",
                StatusEffectType.Dexterity => "STF_민첩_Dexterity",
                StatusEffectType.Weak => "STF_약화_Weak",
                StatusEffectType.Vulnerable => "STF_취약_Vulnerable",
                StatusEffectType.Frail => "STF_허약_Frail",
                StatusEffectType.Poison => "STF_독_Poison",
                StatusEffectType.Regeneration => "STF_재생_Regeneration",
                _ => null
            };

            if (fileName == null) return null;

            string path = $"{STATUS_PATH}/{fileName}.asset";
            return AssetDatabase.LoadAssetAtPath<StatusEffectData>(path);
        }

        /// <summary>
        /// 모든 상태효과 데이터 로드
        /// Load all status effect data
        /// </summary>
        public static StatusEffectData[] LoadAllStatusEffects()
        {
            string[] guids = AssetDatabase.FindAssets("t:StatusEffectData", new[] { STATUS_PATH });
            StatusEffectData[] effects = new StatusEffectData[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                effects[i] = AssetDatabase.LoadAssetAtPath<StatusEffectData>(path);
            }

            return effects;
        }

        #endregion
    }
}
