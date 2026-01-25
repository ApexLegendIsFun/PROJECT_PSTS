using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 스타터 카드 생성기
    /// Starter card generator for initial deck setup
    /// </summary>
    public static class StarterCardGenerator
    {
        #region Menu Items

        [MenuItem("Tools/Project SS/Generators/Generate Starter Cards")]
        public static void GenerateStarterCards()
        {
            int created = 0;

            EditorUtility.DisplayProgressBar("스타터 카드 생성", "Skill 카드 생성 중...", 0.2f);

            // Skill Cards
            if (GenerateFlashCard()) created++;
            if (GeneratePrepareCard()) created++;
            if (GenerateSurgeCard()) created++;

            EditorUtility.DisplayProgressBar("스타터 카드 생성", "완료 중...", 0.9f);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            if (created > 0)
            {
                Debug.Log($"<color=green>✅ 스타터 카드 {created}개 생성 완료!</color>");
                EditorUtility.DisplayDialog("완료", $"스타터 카드 {created}개가 생성되었습니다.", "확인");
            }
            else
            {
                Debug.Log("모든 스타터 카드가 이미 존재합니다.");
                EditorUtility.DisplayDialog("알림", "모든 스타터 카드가 이미 존재합니다.", "확인");
            }
        }

        [MenuItem("Tools/Project SS/Generators/Skill Cards/Generate Flash")]
        public static bool GenerateFlashCard()
        {
            var effects = new List<CardEffect>
            {
                CardGenerator.CreateDrawEffect(2)
            };

            var card = CardGenerator.CreateSkillCard(
                koreanName: "섬광",
                englishName: "Flash",
                effects: effects,
                cost: 1,
                rarity: CardRarity.Starter
            );

            return card != null && !System.IO.File.Exists(GetSkillCardPath("섬광", "Flash"));
        }

        [MenuItem("Tools/Project SS/Generators/Skill Cards/Generate Prepare")]
        public static bool GeneratePrepareCard()
        {
            var effects = new List<CardEffect>
            {
                CardGenerator.CreateBlockEffect(5),
                CardGenerator.CreateDrawEffect(1)
            };

            var card = CardGenerator.CreateSkillCard(
                koreanName: "준비",
                englishName: "Prepare",
                effects: effects,
                cost: 1,
                rarity: CardRarity.Starter
            );

            return card != null && !System.IO.File.Exists(GetSkillCardPath("준비", "Prepare"));
        }

        [MenuItem("Tools/Project SS/Generators/Skill Cards/Generate Surge")]
        public static bool GenerateSurgeCard()
        {
            var effects = new List<CardEffect>
            {
                CardGenerator.CreateGainEnergyEffect(2)
            };

            var card = CardGenerator.CreateSkillCard(
                koreanName: "급류",
                englishName: "Surge",
                effects: effects,
                cost: 0,
                rarity: CardRarity.Common,
                exhaust: true
            );

            return card != null && !System.IO.File.Exists(GetSkillCardPath("급류", "Surge"));
        }

        #endregion

        #region Helper Methods

        private static string GetSkillCardPath(string koreanName, string englishName)
        {
            string fileName = GeneratorUtility.FormatCardFileName(CardType.Skill, koreanName, englishName);
            return $"{GeneratorUtility.CARDS_PATH}/Skill/{fileName}.asset";
        }

        #endregion

        #region Validation

        /// <summary>
        /// 모든 스타터 카드가 존재하는지 확인
        /// Check if all starter cards exist
        /// </summary>
        public static bool ValidateStarterCardsExist()
        {
            string[] requiredCards = {
                "ATK_강타_Strike",
                "ATK_강타_Bash",
                "DEF_방어_Defend",
                "SKL_섬광_Flash"
            };

            bool allExist = true;
            foreach (var fileName in requiredCards)
            {
                string subfolder = fileName.StartsWith("ATK") ? "Attack" :
                                  fileName.StartsWith("DEF") ? "Defense" : "Skill";
                string path = $"{GeneratorUtility.CARDS_PATH}/{subfolder}/{fileName}.asset";
                if (!System.IO.File.Exists(path))
                {
                    Debug.LogWarning($"스타터 카드 누락: {fileName}");
                    allExist = false;
                }
            }

            return allExist;
        }

        #endregion
    }
}
