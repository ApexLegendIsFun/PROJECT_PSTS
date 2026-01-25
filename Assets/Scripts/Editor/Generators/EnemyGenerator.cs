using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProjectSS.Data;

namespace ProjectSS.Editor.Generators
{
    /// <summary>
    /// 적 데이터 생성기
    /// Enemy data generator
    /// </summary>
    public static class EnemyGenerator
    {
        #region Menu Items

        [MenuItem("Tools/Project SS/Generators/Enemy Generator Window")]
        public static void OpenWindow()
        {
            EnemyGeneratorWindow.ShowWindow();
        }

        [MenuItem("Tools/Project SS/Generators/Generate Act 1 Enemies")]
        public static void GenerateAct1Enemies()
        {
            int created = 0;

            EditorUtility.DisplayProgressBar("Act 1 적 생성", "일반 적 생성 중...", 0.2f);

            // 일반 적
            if (CreateNormalEnemy("슬라임", "Slime", 12, 16,
                new List<EnemyAction> { CreateAttackAction("Tackle", 5) }, 10) != null) created++;

            if (CreateNormalEnemy("광신도", "Cultist", 48, 54,
                new List<EnemyAction>
                {
                    CreateAttackAction("DarkStrike", 6),
                    CreateBuffAction("Incantation", StatusEffectType.Strength, 3)
                }, 15) != null) created++;

            if (CreateNormalEnemy("산적", "JawWorm", 40, 44,
                new List<EnemyAction>
                {
                    CreateAttackAction("Chomp", 11),
                    CreateDefendAction("Bellow", 6),
                    CreateAttackBuffAction("Thrash", 7, StatusEffectType.Strength, 3)
                }, 12) != null) created++;

            EditorUtility.DisplayProgressBar("Act 1 적 생성", "엘리트 적 생성 중...", 0.5f);

            // 엘리트
            if (CreateEliteEnemy("라가불린", "Lagavulin", 109, 111,
                new List<EnemyAction>
                {
                    CreateAttackAction("Attack", 18),
                    CreateDebuffAction("SiphonSoul", StatusEffectType.Weak, 2)
                }, 30,
                CreateDamageThresholdBreak(25, 1),
                new List<EnemyAction> { CreateAttackAction("WeakAttack", 8) }) != null) created++;

            if (CreateEliteEnemy("센트리", "Sentry", 38, 42,
                new List<EnemyAction>
                {
                    CreateAttackAction("Beam", 9),
                    CreateDefendAction("Shield", 10)
                }, 25) != null) created++;

            EditorUtility.DisplayProgressBar("Act 1 적 생성", "보스 생성 중...", 0.8f);

            // 보스
            if (CreateBoss("슬라임보스", "SlimeBoss", 140, 150,
                new List<EnemyAction>
                {
                    CreateAttackAction("Slam", 35),
                    CreateMultiAction("Preparing", 0, 0, null, 0),
                    CreateDebuffAction("GoopSpray", StatusEffectType.Weak, 2)
                }, 100,
                CreateHitCountBreak(8, 2),
                new List<EnemyAction>
                {
                    CreateDefendAction("Split", 5),
                    CreateAttackAction("WeakSlam", 15)
                }) != null) created++;

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>✅ Act 1 적 {created}개 생성 완료!</color>");
            EditorUtility.DisplayDialog("완료", $"Act 1 적 {created}개가 생성되었습니다.", "확인");
        }

        #endregion

        #region Core Creation Methods

        /// <summary>
        /// 일반 적 생성
        /// Create normal enemy
        /// </summary>
        public static EnemyData CreateNormalEnemy(
            string koreanName,
            string englishName,
            int minHP,
            int maxHP,
            List<EnemyAction> actions,
            int goldReward,
            float cardRewardChance = 0.5f,
            EnemyTargetingStrategy targetingStrategy = EnemyTargetingStrategy.ActiveOnly)
        {
            return CreateEnemy(
                enemyType: EnemyType.Normal,
                koreanName: koreanName,
                englishName: englishName,
                minHP: minHP,
                maxHP: maxHP,
                actions: actions,
                goldReward: goldReward,
                cardRewardChance: cardRewardChance,
                targetingStrategy: targetingStrategy
            );
        }

        /// <summary>
        /// 엘리트 적 생성
        /// Create elite enemy
        /// </summary>
        public static EnemyData CreateEliteEnemy(
            string koreanName,
            string englishName,
            int minHP,
            int maxHP,
            List<EnemyAction> actions,
            int goldReward,
            BreakConditionData breakCondition = null,
            List<EnemyAction> groggyActions = null,
            float cardRewardChance = 0.75f,
            EnemyTargetingStrategy targetingStrategy = EnemyTargetingStrategy.ActiveOnly)
        {
            return CreateEnemy(
                enemyType: EnemyType.Elite,
                koreanName: koreanName,
                englishName: englishName,
                minHP: minHP,
                maxHP: maxHP,
                actions: actions,
                goldReward: goldReward,
                cardRewardChance: cardRewardChance,
                breakCondition: breakCondition,
                groggyActions: groggyActions,
                targetingStrategy: targetingStrategy
            );
        }

        /// <summary>
        /// 보스 생성
        /// Create boss enemy
        /// </summary>
        public static EnemyData CreateBoss(
            string koreanName,
            string englishName,
            int minHP,
            int maxHP,
            List<EnemyAction> actions,
            int goldReward,
            BreakConditionData breakCondition,
            List<EnemyAction> groggyActions,
            float cardRewardChance = 1f,
            EnemyTargetingStrategy targetingStrategy = EnemyTargetingStrategy.ActiveOnly)
        {
            return CreateEnemy(
                enemyType: EnemyType.Boss,
                koreanName: koreanName,
                englishName: englishName,
                minHP: minHP,
                maxHP: maxHP,
                actions: actions,
                goldReward: goldReward,
                cardRewardChance: cardRewardChance,
                breakCondition: breakCondition,
                groggyActions: groggyActions,
                targetingStrategy: targetingStrategy
            );
        }

        /// <summary>
        /// 적 생성 (전체 파라미터)
        /// Create enemy with full parameters
        /// </summary>
        public static EnemyData CreateEnemy(
            EnemyType enemyType,
            string koreanName,
            string englishName,
            int minHP,
            int maxHP,
            List<EnemyAction> actions,
            int goldReward,
            float cardRewardChance = 0.5f,
            BreakConditionData breakCondition = null,
            List<EnemyAction> groggyActions = null,
            EnemyTargetingStrategy targetingStrategy = EnemyTargetingStrategy.ActiveOnly)
        {
            string fileName = GeneratorUtility.FormatEnemyFileName(enemyType, koreanName, englishName);
            string subfolder = GeneratorUtility.GetEnemySubfolder(enemyType);
            string fullPath = $"{GeneratorUtility.ENEMIES_PATH}/{subfolder}/{fileName}.asset";

            // 이미 존재하면 반환
            if (System.IO.File.Exists(fullPath))
            {
                Debug.Log($"적 이미 존재: {fileName}");
                return AssetDatabase.LoadAssetAtPath<EnemyData>(fullPath);
            }

            // 폴더 확인
            GeneratorUtility.EnsureFolderExists($"{GeneratorUtility.ENEMIES_PATH}/{subfolder}");

            // 새 적 생성
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.enemyId = fileName.ToLower().Replace(" ", "_");
            enemy.enemyName = $"{koreanName} ({englishName})";
            enemy.enemyType = enemyType;
            enemy.minHealth = minHP;
            enemy.maxHealth = maxHP;
            enemy.actionPattern = actions ?? new List<EnemyAction>();
            enemy.goldReward = goldReward;
            enemy.cardRewardChance = cardRewardChance;
            enemy.breakCondition = breakCondition;
            enemy.groggyActions = groggyActions ?? new List<EnemyAction>();
            enemy.targetingStrategy = targetingStrategy;

            AssetDatabase.CreateAsset(enemy, fullPath);
            Debug.Log($"적 생성: {fileName}");
            return enemy;
        }

        #endregion

        #region Action Builders

        /// <summary>
        /// 공격 액션 생성
        /// Create attack action
        /// </summary>
        public static EnemyAction CreateAttackAction(string name, int damage, int hitCount = 1)
        {
            return new EnemyAction
            {
                intentType = EnemyIntentType.Attack,
                actionName = name,
                damage = damage,
                hitCount = hitCount
            };
        }

        /// <summary>
        /// 방어 액션 생성
        /// Create defend action
        /// </summary>
        public static EnemyAction CreateDefendAction(string name, int block)
        {
            return new EnemyAction
            {
                intentType = EnemyIntentType.Defend,
                actionName = name,
                block = block
            };
        }

        /// <summary>
        /// 버프 액션 생성
        /// Create buff action
        /// </summary>
        public static EnemyAction CreateBuffAction(string name, StatusEffectType type, int stacks)
        {
            var statusEffect = GeneratorUtility.LoadStatusEffect(type);
            return new EnemyAction
            {
                intentType = EnemyIntentType.Buff,
                actionName = name,
                statusEffect = statusEffect,
                statusStacks = stacks
            };
        }

        /// <summary>
        /// 디버프 액션 생성
        /// Create debuff action
        /// </summary>
        public static EnemyAction CreateDebuffAction(string name, StatusEffectType type, int stacks)
        {
            var statusEffect = GeneratorUtility.LoadStatusEffect(type);
            return new EnemyAction
            {
                intentType = EnemyIntentType.Debuff,
                actionName = name,
                statusEffect = statusEffect,
                statusStacks = stacks
            };
        }

        /// <summary>
        /// 공격 + 버프 액션 생성
        /// Create attack + buff action
        /// </summary>
        public static EnemyAction CreateAttackBuffAction(string name, int damage, StatusEffectType buffType, int stacks, int hitCount = 1)
        {
            var statusEffect = GeneratorUtility.LoadStatusEffect(buffType);
            return new EnemyAction
            {
                intentType = EnemyIntentType.AttackBuff,
                actionName = name,
                damage = damage,
                hitCount = hitCount,
                statusEffect = statusEffect,
                statusStacks = stacks
            };
        }

        /// <summary>
        /// 공격 + 디버프 액션 생성
        /// Create attack + debuff action
        /// </summary>
        public static EnemyAction CreateAttackDebuffAction(string name, int damage, StatusEffectType debuffType, int stacks, int hitCount = 1)
        {
            var statusEffect = GeneratorUtility.LoadStatusEffect(debuffType);
            return new EnemyAction
            {
                intentType = EnemyIntentType.AttackDebuff,
                actionName = name,
                damage = damage,
                hitCount = hitCount,
                statusEffect = statusEffect,
                statusStacks = stacks
            };
        }

        /// <summary>
        /// 복합 액션 생성 (공격 + 방어 + 상태효과)
        /// Create multi-action (attack + defend + status)
        /// </summary>
        public static EnemyAction CreateMultiAction(string name, int damage, int block,
            StatusEffectData statusEffect = null, int stacks = 0, int hitCount = 1)
        {
            var intentType = EnemyIntentType.Unknown;
            if (damage > 0 && statusEffect != null && !statusEffect.isDebuff)
                intentType = EnemyIntentType.AttackBuff;
            else if (damage > 0 && statusEffect != null && statusEffect.isDebuff)
                intentType = EnemyIntentType.AttackDebuff;
            else if (damage > 0)
                intentType = EnemyIntentType.Attack;
            else if (block > 0)
                intentType = EnemyIntentType.Defend;
            else if (statusEffect != null && !statusEffect.isDebuff)
                intentType = EnemyIntentType.Buff;
            else if (statusEffect != null)
                intentType = EnemyIntentType.Debuff;

            return new EnemyAction
            {
                intentType = intentType,
                actionName = name,
                damage = damage,
                block = block,
                hitCount = hitCount,
                statusEffect = statusEffect,
                statusStacks = stacks
            };
        }

        #endregion

        #region Break Condition Builders

        /// <summary>
        /// 데미지 임계값 브레이크 조건 생성
        /// Create damage threshold break condition
        /// </summary>
        public static BreakConditionData CreateDamageThresholdBreak(int threshold, int groggyTurns = 1, bool resetOnTurnStart = true)
        {
            string fileName = $"BREAK_Damage{threshold}";
            string fullPath = $"{GeneratorUtility.ENEMIES_PATH}/{fileName}.asset";

            // 이미 존재하면 반환
            if (System.IO.File.Exists(fullPath))
            {
                return AssetDatabase.LoadAssetAtPath<BreakConditionData>(fullPath);
            }

            GeneratorUtility.EnsureFolderExists(GeneratorUtility.ENEMIES_PATH);

            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.DamageThreshold;
            condition.damageThreshold = threshold;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = resetOnTurnStart;

            AssetDatabase.CreateAsset(condition, fullPath);
            Debug.Log($"브레이크 조건 생성: {fileName}");
            return condition;
        }

        /// <summary>
        /// 타격 횟수 브레이크 조건 생성
        /// Create hit count break condition
        /// </summary>
        public static BreakConditionData CreateHitCountBreak(int hitCount, int groggyTurns = 1, bool resetOnTurnStart = true)
        {
            string fileName = $"BREAK_Hit{hitCount}";
            string fullPath = $"{GeneratorUtility.ENEMIES_PATH}/{fileName}.asset";

            // 이미 존재하면 반환
            if (System.IO.File.Exists(fullPath))
            {
                return AssetDatabase.LoadAssetAtPath<BreakConditionData>(fullPath);
            }

            GeneratorUtility.EnsureFolderExists(GeneratorUtility.ENEMIES_PATH);

            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.HitCount;
            condition.hitCountThreshold = hitCount;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = resetOnTurnStart;

            AssetDatabase.CreateAsset(condition, fullPath);
            Debug.Log($"브레이크 조건 생성: {fileName}");
            return condition;
        }

        /// <summary>
        /// 복합 브레이크 조건 생성 (데미지 OR 타격)
        /// Create combined break condition (damage OR hits)
        /// </summary>
        public static BreakConditionData CreateCombinedBreak(int damageThreshold, int hitCount, int groggyTurns = 1, bool resetOnTurnStart = true)
        {
            string fileName = $"BREAK_Damage{damageThreshold}_Hit{hitCount}";
            string fullPath = $"{GeneratorUtility.ENEMIES_PATH}/{fileName}.asset";

            // 이미 존재하면 반환
            if (System.IO.File.Exists(fullPath))
            {
                return AssetDatabase.LoadAssetAtPath<BreakConditionData>(fullPath);
            }

            GeneratorUtility.EnsureFolderExists(GeneratorUtility.ENEMIES_PATH);

            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.Both;
            condition.damageThreshold = damageThreshold;
            condition.hitCountThreshold = hitCount;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = resetOnTurnStart;

            AssetDatabase.CreateAsset(condition, fullPath);
            Debug.Log($"브레이크 조건 생성: {fileName}");
            return condition;
        }

        #endregion
    }
}
