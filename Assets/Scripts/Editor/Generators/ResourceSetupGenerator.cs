using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// Resources 폴더 설정 도우미
    /// Resources folder setup helper
    ///
    /// 필요한 에셋을 Resources 폴더로 복사합니다.
    /// Copies required assets to Resources folder for runtime loading.
    /// </summary>
    public static class ResourceSetupGenerator
    {
        private const string RESOURCES_PATH = "Assets/Resources";
        private const string CHARACTER_CLASSES_PATH = "Assets/Resources/CharacterClasses";
        private const string CARDS_PATH = "Assets/Resources/Cards";
        private const string ENEMIES_PATH = "Assets/Resources/Enemies";
        private const string PREFABS_PATH = "Assets/Resources/Prefabs";
        private const string SHOP_PATH = "Assets/Resources/Data/Shop";
        private const string EVENTS_PATH = "Assets/Resources/Data/Events";
        private const string RELICS_PATH = "Assets/Resources/Data/Relics";

        [MenuItem("Tools/Project SS/Setup/Setup Resources Folder")]
        public static void SetupResourcesFolder()
        {
            int copied = 0;

            EditorUtility.DisplayProgressBar("Resources 설정", "폴더 생성 중...", 0.1f);
            CreateFolders();

            EditorUtility.DisplayProgressBar("Resources 설정", "CharacterClasses 복사 중...", 0.2f);
            copied += CopyCharacterClasses();

            EditorUtility.DisplayProgressBar("Resources 설정", "Cards 복사 중...", 0.35f);
            copied += CopyCards();

            EditorUtility.DisplayProgressBar("Resources 설정", "Enemies 복사 중...", 0.5f);
            copied += CopyEnemies();

            EditorUtility.DisplayProgressBar("Resources 설정", "Shop 설정 복사 중...", 0.65f);
            copied += CopyShopConfig();

            EditorUtility.DisplayProgressBar("Resources 설정", "Events 복사 중...", 0.75f);
            copied += CopyEvents();

            EditorUtility.DisplayProgressBar("Resources 설정", "Relics 복사 중...", 0.85f);
            copied += CopyRelics();

            EditorUtility.DisplayProgressBar("Resources 설정", "완료 중...", 0.95f);
            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();

            if (copied > 0)
            {
                Debug.Log($"<color=green>✅ Resources 설정 완료! {copied}개 에셋 복사됨</color>");
                EditorUtility.DisplayDialog("완료", $"{copied}개 에셋이 Resources 폴더로 복사되었습니다.", "확인");
            }
            else
            {
                Debug.Log("모든 에셋이 이미 Resources 폴더에 존재합니다.");
                EditorUtility.DisplayDialog("알림", "모든 에셋이 이미 Resources 폴더에 존재합니다.", "확인");
            }
        }

        [MenuItem("Tools/Project SS/Setup/Validate Resources")]
        public static void ValidateResources()
        {
            int missing = 0;
            System.Text.StringBuilder report = new System.Text.StringBuilder();
            report.AppendLine("=== Resources 검증 결과 ===\n");

            // CharacterClasses
            report.AppendLine("[CharacterClasses]");
            if (!ValidateAsset<CharacterClassData>("CharacterClasses/CLASS_전사_Warrior", ref missing))
                report.AppendLine("❌ CLASS_전사_Warrior 누락");
            else
                report.AppendLine("✅ CLASS_전사_Warrior");

            if (!ValidateAsset<CharacterClassData>("CharacterClasses/CLASS_마법사_Mage", ref missing))
                report.AppendLine("❌ CLASS_마법사_Mage 누락");
            else
                report.AppendLine("✅ CLASS_마법사_Mage");

            if (!ValidateAsset<CharacterClassData>("CharacterClasses/CLASS_도적_Rogue", ref missing))
                report.AppendLine("❌ CLASS_도적_Rogue 누락");
            else
                report.AppendLine("✅ CLASS_도적_Rogue");

            // Cards
            report.AppendLine("\n[Cards]");
            if (!ValidateAsset<CardData>("Cards/ATK_강타_Strike", ref missing))
                report.AppendLine("❌ ATK_강타_Strike 누락");
            else
                report.AppendLine("✅ ATK_강타_Strike");

            if (!ValidateAsset<CardData>("Cards/DEF_방어_Defend", ref missing))
                report.AppendLine("❌ DEF_방어_Defend 누락");
            else
                report.AppendLine("✅ DEF_방어_Defend");

            // Enemies
            report.AppendLine("\n[Enemies]");
            if (!ValidateAsset<EnemyData>("Enemies/EN_슬라임_Slime", ref missing))
                report.AppendLine("❌ EN_슬라임_Slime 누락");
            else
                report.AppendLine("✅ EN_슬라임_Slime");

            report.AppendLine($"\n총 누락: {missing}개");

            Debug.Log(report.ToString());

            if (missing > 0)
            {
                EditorUtility.DisplayDialog("검증 결과",
                    $"{missing}개 에셋이 누락되었습니다.\n\n" +
                    "Tools > Project SS > Setup > Setup Resources Folder\n" +
                    "메뉴를 실행하여 복사하세요.", "확인");
            }
            else
            {
                EditorUtility.DisplayDialog("검증 결과",
                    "모든 필수 에셋이 Resources에 존재합니다!", "확인");
            }
        }

        private static bool ValidateAsset<T>(string path, ref int missing) where T : Object
        {
            var asset = Resources.Load<T>(path);
            if (asset == null)
            {
                missing++;
                return false;
            }
            return true;
        }

        private static void CreateFolders()
        {
            GeneratorUtility.EnsureFolderExists(RESOURCES_PATH);
            GeneratorUtility.EnsureFolderExists(CHARACTER_CLASSES_PATH);
            GeneratorUtility.EnsureFolderExists(CARDS_PATH);
            GeneratorUtility.EnsureFolderExists(ENEMIES_PATH);
            GeneratorUtility.EnsureFolderExists(PREFABS_PATH);
            GeneratorUtility.EnsureFolderExists("Assets/Resources/Data");
            GeneratorUtility.EnsureFolderExists(SHOP_PATH);
            GeneratorUtility.EnsureFolderExists(EVENTS_PATH);
            GeneratorUtility.EnsureFolderExists(RELICS_PATH);
        }

        private static int CopyCharacterClasses()
        {
            int copied = 0;

            // Warrior
            if (CopyAsset(
                $"{GeneratorUtility.CHARACTERS_PATH}/CLASS_전사_Warrior.asset",
                $"{CHARACTER_CLASSES_PATH}/CLASS_전사_Warrior.asset"))
                copied++;

            // Mage
            if (CopyAsset(
                $"{GeneratorUtility.CHARACTERS_PATH}/CLASS_마법사_Mage.asset",
                $"{CHARACTER_CLASSES_PATH}/CLASS_마법사_Mage.asset"))
                copied++;

            // Rogue
            if (CopyAsset(
                $"{GeneratorUtility.CHARACTERS_PATH}/CLASS_도적_Rogue.asset",
                $"{CHARACTER_CLASSES_PATH}/CLASS_도적_Rogue.asset"))
                copied++;

            return copied;
        }

        private static int CopyCards()
        {
            int copied = 0;

            // Attack cards
            string[] attackCards = { "ATK_강타_Strike", "ATK_강타_Bash" };
            foreach (var card in attackCards)
            {
                if (CopyAsset(
                    $"{GeneratorUtility.CARDS_PATH}/Attack/{card}.asset",
                    $"{CARDS_PATH}/{card}.asset"))
                    copied++;
            }

            // Defense cards
            string[] defenseCards = { "DEF_방어_Defend" };
            foreach (var card in defenseCards)
            {
                if (CopyAsset(
                    $"{GeneratorUtility.CARDS_PATH}/Defense/{card}.asset",
                    $"{CARDS_PATH}/{card}.asset"))
                    copied++;
            }

            // Skill cards
            string[] skillCards = { "SKL_섬광_Flash", "SKL_준비_Prepare", "SKL_급류_Surge" };
            foreach (var card in skillCards)
            {
                string sourcePath = $"{GeneratorUtility.CARDS_PATH}/Skill/{card}.asset";
                if (File.Exists(sourcePath))
                {
                    if (CopyAsset(sourcePath, $"{CARDS_PATH}/{card}.asset"))
                        copied++;
                }
            }

            return copied;
        }

        private static int CopyEnemies()
        {
            int copied = 0;

            // Normal enemies
            string[] normalEnemies = { "EN_슬라임_Slime", "EN_광신도_Cultist", "EN_산적_JawWorm" };
            foreach (var enemy in normalEnemies)
            {
                string sourcePath = $"{GeneratorUtility.ENEMIES_PATH}/Normal/{enemy}.asset";
                if (File.Exists(sourcePath))
                {
                    if (CopyAsset(sourcePath, $"{ENEMIES_PATH}/{enemy}.asset"))
                        copied++;
                }
            }

            // Elite enemies
            string[] eliteEnemies = { "ELITE_라가불린_Lagavulin", "ELITE_센트리_Sentry" };
            foreach (var enemy in eliteEnemies)
            {
                string sourcePath = $"{GeneratorUtility.ENEMIES_PATH}/Elite/{enemy}.asset";
                if (File.Exists(sourcePath))
                {
                    if (CopyAsset(sourcePath, $"{ENEMIES_PATH}/{enemy}.asset"))
                        copied++;
                }
            }

            // Boss enemies
            string[] bossEnemies = { "BOSS_슬라임보스_SlimeBoss" };
            foreach (var enemy in bossEnemies)
            {
                string sourcePath = $"{GeneratorUtility.ENEMIES_PATH}/Boss/{enemy}.asset";
                if (File.Exists(sourcePath))
                {
                    if (CopyAsset(sourcePath, $"{ENEMIES_PATH}/{enemy}.asset"))
                        copied++;
                }
            }

            return copied;
        }

        private static bool CopyAsset(string sourcePath, string destPath)
        {
            // 대상이 이미 존재하면 스킵
            if (File.Exists(destPath))
            {
                return false;
            }

            // 소스가 없으면 스킵
            if (!File.Exists(sourcePath))
            {
                Debug.LogWarning($"Source asset not found: {sourcePath}");
                return false;
            }

            // 복사
            if (AssetDatabase.CopyAsset(sourcePath, destPath))
            {
                Debug.Log($"Copied: {sourcePath} → {destPath}");
                return true;
            }

            return false;
        }

        [MenuItem("Tools/Project SS/Setup/Create All Missing Assets")]
        public static void CreateAllMissingAssets()
        {
            EditorUtility.DisplayProgressBar("에셋 생성", "Status Effects...", 0.2f);
            StatusEffectGenerator.GenerateMissingStatusEffects();

            EditorUtility.DisplayProgressBar("에셋 생성", "TRIAD Classes...", 0.4f);
            CharacterClassGenerator.GenerateTriadClasses();

            EditorUtility.DisplayProgressBar("에셋 생성", "Starter Cards...", 0.6f);
            StarterCardGenerator.GenerateStarterCards();

            EditorUtility.DisplayProgressBar("에셋 생성", "Act 1 Enemies...", 0.8f);
            EnemyGenerator.GenerateAct1Enemies();

            EditorUtility.DisplayProgressBar("에셋 생성", "Resources 설정...", 0.9f);
            SetupResourcesFolder();

            EditorUtility.ClearProgressBar();

            Debug.Log("<color=green>✅ 모든 에셋 생성 및 Resources 설정 완료!</color>");
            EditorUtility.DisplayDialog("완료",
                "모든 누락된 에셋이 생성되고 Resources 폴더가 설정되었습니다.\n\n" +
                "이제 게임을 플레이할 수 있습니다!", "확인");
        }
    }
}
