using UnityEngine;
using UnityEditor;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 상태효과 데이터 생성기
    /// Status effect data generator
    /// </summary>
    public static class StatusEffectGenerator
    {
        #region Menu Items

        [MenuItem("Tools/Project SS/Generators/Generate Missing Status Effects")]
        public static void GenerateMissingStatusEffects()
        {
            int created = 0;

            // Frail (허약) - 누락된 효과
            if (GenerateFrail()) created++;

            // Poison (독) - 누락된 효과
            if (GeneratePoison()) created++;

            // Regeneration (재생) - 누락된 효과
            if (GenerateRegeneration()) created++;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (created > 0)
            {
                Debug.Log($"<color=green>✅ 상태효과 {created}개 생성 완료!</color>");
                EditorUtility.DisplayDialog("완료", $"상태효과 {created}개가 생성되었습니다.", "확인");
            }
            else
            {
                Debug.Log("모든 상태효과가 이미 존재합니다.");
                EditorUtility.DisplayDialog("알림", "모든 상태효과가 이미 존재합니다.", "확인");
            }
        }

        [MenuItem("Tools/Project SS/Generators/Status Effects/Generate Frail")]
        public static bool GenerateFrail()
        {
            return CreateStatusEffect(
                koreanName: "허약",
                englishName: "Frail",
                description: "블록 획득량 25% 감소\nGain 25% less Block",
                effectType: StatusEffectType.Frail,
                isDebuff: true,
                stackable: true,
                stackBehavior: StackBehavior.Duration,
                triggerTime: StatusTrigger.Passive,
                valuePerStack: 0.75f
            );
        }

        [MenuItem("Tools/Project SS/Generators/Status Effects/Generate Poison")]
        public static bool GeneratePoison()
        {
            return CreateStatusEffect(
                koreanName: "독",
                englishName: "Poison",
                description: "턴 종료 시 {0} 데미지를 받고, 1 감소\nTake {0} damage at turn end, then decrease by 1",
                effectType: StatusEffectType.Poison,
                isDebuff: true,
                stackable: true,
                stackBehavior: StackBehavior.Intensity,
                triggerTime: StatusTrigger.TurnEnd,
                valuePerStack: 1f
            );
        }

        [MenuItem("Tools/Project SS/Generators/Status Effects/Generate Regeneration")]
        public static bool GenerateRegeneration()
        {
            return CreateStatusEffect(
                koreanName: "재생",
                englishName: "Regeneration",
                description: "턴 종료 시 {0} 체력 회복, 1 감소\nHeal {0} HP at turn end, then decrease by 1",
                effectType: StatusEffectType.Regeneration,
                isDebuff: false,
                stackable: true,
                stackBehavior: StackBehavior.Intensity,
                triggerTime: StatusTrigger.TurnEnd,
                valuePerStack: 1f
            );
        }

        #endregion

        #region Core Creation Methods

        /// <summary>
        /// 상태효과 생성
        /// Create status effect
        /// </summary>
        /// <returns>true if created, false if already exists</returns>
        public static bool CreateStatusEffect(
            string koreanName,
            string englishName,
            string description,
            StatusEffectType effectType,
            bool isDebuff,
            bool stackable,
            StackBehavior stackBehavior,
            StatusTrigger triggerTime,
            float valuePerStack = 1f)
        {
            string fileName = GeneratorUtility.FormatStatusFileName(koreanName, englishName);
            string fullPath = $"{GeneratorUtility.STATUS_PATH}/{fileName}.asset";

            // 이미 존재하면 스킵
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"상태효과 이미 존재: {fileName}");
                return false;
            }

            // 폴더 확인
            GeneratorUtility.EnsureFolderExists(GeneratorUtility.STATUS_PATH);

            // 새 상태효과 생성
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = fileName.ToLower().Replace(" ", "_");
            effect.statusName = $"{koreanName} ({englishName})";
            effect.description = description;
            effect.effectType = effectType;
            effect.isDebuff = isDebuff;
            effect.stackable = stackable;
            effect.stackBehavior = stackBehavior;
            effect.triggerTime = triggerTime;
            effect.valuePerStack = valuePerStack;

            AssetDatabase.CreateAsset(effect, fullPath);
            Debug.Log($"상태효과 생성: {fileName}");
            return true;
        }

        /// <summary>
        /// 지속시간 기반 디버프 생성
        /// Create duration-based debuff
        /// </summary>
        public static StatusEffectData CreateDurationDebuff(
            string koreanName,
            string englishName,
            string description,
            StatusEffectType effectType,
            float valuePerStack = 1f)
        {
            string fileName = GeneratorUtility.FormatStatusFileName(koreanName, englishName);
            string fullPath = $"{GeneratorUtility.STATUS_PATH}/{fileName}.asset";

            GeneratorUtility.EnsureFolderExists(GeneratorUtility.STATUS_PATH);

            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = fileName.ToLower().Replace(" ", "_");
            effect.statusName = $"{koreanName} ({englishName})";
            effect.description = description;
            effect.effectType = effectType;
            effect.isDebuff = true;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Duration;
            effect.triggerTime = StatusTrigger.Passive;
            effect.valuePerStack = valuePerStack;

            AssetDatabase.CreateAsset(effect, fullPath);
            return effect;
        }

        /// <summary>
        /// 강도 기반 버프 생성
        /// Create intensity-based buff
        /// </summary>
        public static StatusEffectData CreateIntensityBuff(
            string koreanName,
            string englishName,
            string description,
            StatusEffectType effectType,
            float valuePerStack = 1f)
        {
            string fileName = GeneratorUtility.FormatStatusFileName(koreanName, englishName);
            string fullPath = $"{GeneratorUtility.STATUS_PATH}/{fileName}.asset";

            GeneratorUtility.EnsureFolderExists(GeneratorUtility.STATUS_PATH);

            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = fileName.ToLower().Replace(" ", "_");
            effect.statusName = $"{koreanName} ({englishName})";
            effect.description = description;
            effect.effectType = effectType;
            effect.isDebuff = false;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Intensity;
            effect.triggerTime = StatusTrigger.Passive;
            effect.valuePerStack = valuePerStack;

            AssetDatabase.CreateAsset(effect, fullPath);
            return effect;
        }

        /// <summary>
        /// DoT/HoT 효과 생성
        /// Create DoT (Damage over Time) or HoT (Heal over Time) effect
        /// </summary>
        public static StatusEffectData CreateDoTEffect(
            string koreanName,
            string englishName,
            string description,
            StatusEffectType effectType,
            bool isHeal)
        {
            string fileName = GeneratorUtility.FormatStatusFileName(koreanName, englishName);
            string fullPath = $"{GeneratorUtility.STATUS_PATH}/{fileName}.asset";

            GeneratorUtility.EnsureFolderExists(GeneratorUtility.STATUS_PATH);

            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = fileName.ToLower().Replace(" ", "_");
            effect.statusName = $"{koreanName} ({englishName})";
            effect.description = description;
            effect.effectType = effectType;
            effect.isDebuff = !isHeal;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Intensity;
            effect.triggerTime = StatusTrigger.TurnEnd;
            effect.valuePerStack = 1f;

            AssetDatabase.CreateAsset(effect, fullPath);
            return effect;
        }

        #endregion

        #region Validation

        /// <summary>
        /// 모든 상태효과가 존재하는지 확인
        /// Check if all required status effects exist
        /// </summary>
        public static bool ValidateAllStatusEffectsExist()
        {
            string[] requiredEffects = {
                "STF_힘_Strength",
                "STF_민첩_Dexterity",
                "STF_약화_Weak",
                "STF_취약_Vulnerable",
                "STF_허약_Frail",
                "STF_독_Poison",
                "STF_재생_Regeneration"
            };

            bool allExist = true;
            foreach (var fileName in requiredEffects)
            {
                string path = $"{GeneratorUtility.STATUS_PATH}/{fileName}.asset";
                if (!System.IO.File.Exists(path))
                {
                    Debug.LogWarning($"상태효과 누락: {fileName}");
                    allExist = false;
                }
            }

            return allExist;
        }

        #endregion
    }
}
