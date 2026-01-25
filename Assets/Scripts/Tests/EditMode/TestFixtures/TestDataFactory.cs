using UnityEngine;
using ProjectSS.Data;

namespace ProjectSS.Tests.EditMode.TestFixtures
{
    /// <summary>
    /// 테스트용 데이터 생성 팩토리
    /// Test data factory for creating test ScriptableObjects
    /// </summary>
    public static class TestDataFactory
    {
        #region CardData Factory

        /// <summary>
        /// 기본 공격 카드 생성
        /// Create basic attack card
        /// </summary>
        public static CardData CreateAttackCard(string name = "TestAttack", int damage = 6, int cost = 1)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = $"test_attack_{name.ToLower()}";
            card.cardName = name;
            card.cardType = CardType.Attack;
            card.energyCost = cost;
            card.rarity = CardRarity.Common;
            card.effects = new System.Collections.Generic.List<CardEffect>
            {
                new CardEffect
                {
                    effectType = CardEffectType.Damage,
                    value = damage,
                    targetType = TargetType.SingleEnemy
                }
            };
            return card;
        }

        /// <summary>
        /// 기본 방어 카드 생성
        /// Create basic defense card
        /// </summary>
        public static CardData CreateDefenseCard(string name = "TestDefense", int block = 5, int cost = 1)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = $"test_defense_{name.ToLower()}";
            card.cardName = name;
            card.cardType = CardType.Defense;
            card.energyCost = cost;
            card.rarity = CardRarity.Common;
            card.effects = new System.Collections.Generic.List<CardEffect>
            {
                new CardEffect
                {
                    effectType = CardEffectType.Block,
                    value = block,
                    targetType = TargetType.Self
                }
            };
            return card;
        }

        /// <summary>
        /// 기본 스킬 카드 생성
        /// Create basic skill card
        /// </summary>
        public static CardData CreateSkillCard(string name = "TestSkill", int drawAmount = 2, int cost = 1)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = $"test_skill_{name.ToLower()}";
            card.cardName = name;
            card.cardType = CardType.Skill;
            card.energyCost = cost;
            card.rarity = CardRarity.Common;
            card.effects = new System.Collections.Generic.List<CardEffect>
            {
                new CardEffect
                {
                    effectType = CardEffectType.Draw,
                    value = drawAmount,
                    targetType = TargetType.Self
                }
            };
            return card;
        }

        /// <summary>
        /// 다중 효과 카드 생성
        /// Create multi-effect card
        /// </summary>
        public static CardData CreateMultiEffectCard(string name, params CardEffect[] effects)
        {
            var card = ScriptableObject.CreateInstance<CardData>();
            card.cardId = $"test_multi_{name.ToLower()}";
            card.cardName = name;
            card.cardType = CardType.Attack;
            card.energyCost = 2;
            card.rarity = CardRarity.Uncommon;
            card.effects = new System.Collections.Generic.List<CardEffect>(effects);
            return card;
        }

        #endregion

        #region StatusEffectData Factory

        /// <summary>
        /// 힘 효과 생성
        /// Create Strength status effect
        /// </summary>
        public static StatusEffectData CreateStrengthEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = "test_strength";
            effect.statusName = "힘";
            effect.effectType = StatusEffectType.Strength;
            effect.isDebuff = false;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Intensity;
            return effect;
        }

        /// <summary>
        /// 민첩 효과 생성
        /// Create Dexterity status effect
        /// </summary>
        public static StatusEffectData CreateDexterityEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = "test_dexterity";
            effect.statusName = "민첩";
            effect.effectType = StatusEffectType.Dexterity;
            effect.isDebuff = false;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Intensity;
            return effect;
        }

        /// <summary>
        /// 약화 효과 생성
        /// Create Weak status effect
        /// </summary>
        public static StatusEffectData CreateWeakEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = "test_weak";
            effect.statusName = "약화";
            effect.effectType = StatusEffectType.Weak;
            effect.isDebuff = true;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Duration;
            return effect;
        }

        /// <summary>
        /// 취약 효과 생성
        /// Create Vulnerable status effect
        /// </summary>
        public static StatusEffectData CreateVulnerableEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = "test_vulnerable";
            effect.statusName = "취약";
            effect.effectType = StatusEffectType.Vulnerable;
            effect.isDebuff = true;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Duration;
            return effect;
        }

        /// <summary>
        /// 허약 효과 생성
        /// Create Frail status effect
        /// </summary>
        public static StatusEffectData CreateFrailEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = "test_frail";
            effect.statusName = "허약";
            effect.effectType = StatusEffectType.Frail;
            effect.isDebuff = true;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Duration;
            return effect;
        }

        /// <summary>
        /// 독 효과 생성
        /// Create Poison status effect
        /// </summary>
        public static StatusEffectData CreatePoisonEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = "test_poison";
            effect.statusName = "독";
            effect.effectType = StatusEffectType.Poison;
            effect.isDebuff = true;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Intensity;
            effect.triggerTime = StatusTrigger.TurnEnd;
            return effect;
        }

        /// <summary>
        /// 재생 효과 생성
        /// Create Regeneration status effect
        /// </summary>
        public static StatusEffectData CreateRegenerationEffect()
        {
            var effect = ScriptableObject.CreateInstance<StatusEffectData>();
            effect.statusId = "test_regeneration";
            effect.statusName = "재생";
            effect.effectType = StatusEffectType.Regeneration;
            effect.isDebuff = false;
            effect.stackable = true;
            effect.stackBehavior = StackBehavior.Intensity;
            effect.triggerTime = StatusTrigger.TurnEnd;
            return effect;
        }

        #endregion

        #region EnemyData Factory

        /// <summary>
        /// 기본 일반 적 생성
        /// Create basic normal enemy
        /// </summary>
        public static EnemyData CreateNormalEnemy(string name = "TestEnemy", int minHp = 10, int maxHp = 15)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.enemyId = $"test_enemy_{name.ToLower()}";
            enemy.enemyName = name;
            enemy.enemyType = EnemyType.Normal;
            enemy.minHealth = minHp;
            enemy.maxHealth = maxHp;
            enemy.goldReward = 10;
            enemy.actionPattern = new System.Collections.Generic.List<EnemyAction>
            {
                CreateEnemyAttackAction("공격", 6)
            };
            return enemy;
        }

        /// <summary>
        /// 엘리트 적 생성
        /// Create elite enemy
        /// </summary>
        public static EnemyData CreateEliteEnemy(string name = "TestElite", int minHp = 30, int maxHp = 40)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.enemyId = $"test_elite_{name.ToLower()}";
            enemy.enemyName = name;
            enemy.enemyType = EnemyType.Elite;
            enemy.minHealth = minHp;
            enemy.maxHealth = maxHp;
            enemy.goldReward = 30;
            enemy.actionPattern = new System.Collections.Generic.List<EnemyAction>
            {
                CreateEnemyAttackAction("강타", 12),
                CreateEnemyDefendAction("방어", 10)
            };
            return enemy;
        }

        /// <summary>
        /// 보스 생성
        /// Create boss enemy
        /// </summary>
        public static EnemyData CreateBoss(string name = "TestBoss", int minHp = 80, int maxHp = 100)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyData>();
            enemy.enemyId = $"test_boss_{name.ToLower()}";
            enemy.enemyName = name;
            enemy.enemyType = EnemyType.Boss;
            enemy.minHealth = minHp;
            enemy.maxHealth = maxHp;
            enemy.goldReward = 100;
            enemy.actionPattern = new System.Collections.Generic.List<EnemyAction>
            {
                CreateEnemyAttackAction("분쇄", 20),
                CreateEnemyDefendAction("철벽", 15),
                CreateEnemyBuffAction("강화", StatusEffectType.Strength)
            };
            return enemy;
        }

        #endregion

        #region EnemyAction Factory

        public static EnemyAction CreateEnemyAttackAction(string name, int damage, int hitCount = 1)
        {
            return new EnemyAction
            {
                actionName = name,
                intentType = EnemyIntentType.Attack,
                damage = damage,
                hitCount = hitCount
            };
        }

        public static EnemyAction CreateEnemyDefendAction(string name, int block)
        {
            return new EnemyAction
            {
                actionName = name,
                intentType = EnemyIntentType.Defend,
                block = block
            };
        }

        public static EnemyAction CreateEnemyBuffAction(string name, StatusEffectType buffType, int stacks = 2)
        {
            // Note: statusEffect should be a StatusEffectData reference
            // For tests, we create the effect data on the fly
            var statusEffectData = CreateStatusEffectByType(buffType);
            return new EnemyAction
            {
                actionName = name,
                intentType = EnemyIntentType.Buff,
                statusEffect = statusEffectData,
                statusStacks = stacks
            };
        }

        public static EnemyAction CreateEnemyDebuffAction(string name, StatusEffectType debuffType, int stacks = 2)
        {
            // Note: statusEffect should be a StatusEffectData reference
            // For tests, we create the effect data on the fly
            var statusEffectData = CreateStatusEffectByType(debuffType);
            return new EnemyAction
            {
                actionName = name,
                intentType = EnemyIntentType.Debuff,
                statusEffect = statusEffectData,
                statusStacks = stacks
            };
        }

        private static StatusEffectData CreateStatusEffectByType(StatusEffectType type)
        {
            return type switch
            {
                StatusEffectType.Strength => CreateStrengthEffect(),
                StatusEffectType.Dexterity => CreateDexterityEffect(),
                StatusEffectType.Weak => CreateWeakEffect(),
                StatusEffectType.Vulnerable => CreateVulnerableEffect(),
                StatusEffectType.Frail => CreateFrailEffect(),
                StatusEffectType.Poison => CreatePoisonEffect(),
                StatusEffectType.Regeneration => CreateRegenerationEffect(),
                _ => null
            };
        }

        #endregion

        #region BreakConditionData Factory

        /// <summary>
        /// 데미지 임계값 브레이크 조건 생성
        /// Create damage threshold break condition
        /// </summary>
        public static BreakConditionData CreateDamageThresholdBreakCondition(int threshold = 20, int groggyTurns = 1)
        {
            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.DamageThreshold;
            condition.damageThreshold = threshold;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = true;
            return condition;
        }

        /// <summary>
        /// 타격 횟수 브레이크 조건 생성
        /// Create hit count break condition
        /// </summary>
        public static BreakConditionData CreateHitCountBreakCondition(int hitCount = 5, int groggyTurns = 1)
        {
            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.HitCount;
            condition.hitCountThreshold = hitCount;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = true;
            return condition;
        }

        /// <summary>
        /// 복합 브레이크 조건 생성
        /// Create combined break condition
        /// </summary>
        public static BreakConditionData CreateCombinedBreakCondition(int damageThreshold = 20, int hitCount = 5, int groggyTurns = 1)
        {
            var condition = ScriptableObject.CreateInstance<BreakConditionData>();
            condition.conditionType = BreakConditionType.Both;
            condition.damageThreshold = damageThreshold;
            condition.hitCountThreshold = hitCount;
            condition.groggyTurns = groggyTurns;
            condition.resetOnTurnStart = true;
            return condition;
        }

        #endregion

        #region CharacterClassData Factory

        /// <summary>
        /// 테스트용 캐릭터 클래스 생성
        /// Create test character class
        /// </summary>
        public static CharacterClassData CreateCharacterClass(string name = "TestClass", int baseHp = 80)
        {
            var characterClass = ScriptableObject.CreateInstance<CharacterClassData>();
            characterClass.classId = $"test_class_{name.ToLower()}";
            characterClass.className = name;
            characterClass.baseMaxHP = baseHp;
            characterClass.tagInBonusType = TagInBonusType.GainBlock;
            characterClass.tagInBonusValue = 5;
            return characterClass;
        }

        #endregion

        #region CardEffect Factory

        public static CardEffect CreateDamageEffect(int damage, TargetType targetType = TargetType.SingleEnemy)
        {
            return new CardEffect
            {
                effectType = CardEffectType.Damage,
                value = damage,
                targetType = targetType
            };
        }

        public static CardEffect CreateBlockEffect(int block)
        {
            return new CardEffect
            {
                effectType = CardEffectType.Block,
                value = block,
                targetType = TargetType.Self
            };
        }

        public static CardEffect CreateDrawEffect(int amount)
        {
            return new CardEffect
            {
                effectType = CardEffectType.Draw,
                value = amount,
                targetType = TargetType.Self
            };
        }

        public static CardEffect CreateApplyStatusEffect(StatusEffectType statusType, int stacks, TargetType targetType = TargetType.SingleEnemy)
        {
            return new CardEffect
            {
                effectType = CardEffectType.ApplyStatus,
                statusEffect = CreateStatusEffectByType(statusType),
                statusStacks = stacks,
                targetType = targetType
            };
        }

        #endregion
    }
}
