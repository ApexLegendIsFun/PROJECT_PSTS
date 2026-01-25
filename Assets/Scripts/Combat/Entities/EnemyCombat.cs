using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Utility;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 적 전투 엔티티
    /// Enemy combat entity
    ///
    /// TRIAD: 파티 타겟팅 + Break/Groggy 시스템 지원
    /// </summary>
    public class EnemyCombat : CombatEntity
    {
        private EnemyData enemyData;
        private int currentActionIndex;
        private int groggyActionIndex;
        private EnemyAction currentIntent;
        private SeededRandom random;

        /// <summary>
        /// TRIAD: Break 게이지
        /// TRIAD: Break gauge for Pattern Cancel system
        /// </summary>
        private BreakGauge breakGauge;

        public EnemyData Data => enemyData;
        public EnemyAction CurrentIntent => currentIntent;

        #region TRIAD Break/Groggy Properties

        /// <summary>
        /// Break 게이지 접근자
        /// Break gauge accessor
        /// </summary>
        public BreakGauge BreakGauge => breakGauge;

        /// <summary>
        /// Break 가능 여부
        /// Whether this enemy can be broken
        /// </summary>
        public bool IsBreakable => breakGauge?.IsBreakable ?? false;

        /// <summary>
        /// 현재 Groggy 상태 여부
        /// Whether currently in groggy state
        /// </summary>
        public bool IsGroggy => breakGauge?.IsGroggy ?? false;

        /// <summary>
        /// Break 상태 여부
        /// Whether currently broken
        /// </summary>
        public bool IsBroken => breakGauge?.IsBroken ?? false;

        #endregion

        /// <summary>
        /// TRIAD: 타겟팅 전략
        /// TRIAD: Targeting strategy
        /// </summary>
        public EnemyTargetingStrategy TargetingStrategy =>
            enemyData?.targetingStrategy ?? EnemyTargetingStrategy.ActiveOnly;

        /// <summary>
        /// 적 초기화
        /// Initialize enemy
        /// </summary>
        public void Initialize(EnemyData data, SeededRandom rng)
        {
            enemyData = data;
            random = rng;

            entityId = $"enemy_{data.enemyId}_{GetInstanceID()}";
            entityName = data.enemyName;
            maxHealth = data.GetRandomHealth(rng.GetSystemRandom());
            currentHealth = maxHealth;
            currentActionIndex = 0;
            groggyActionIndex = 0;

            // TRIAD: Break 게이지 초기화
            // Initialize break gauge if enemy has break condition
            if (data.breakCondition != null)
            {
                breakGauge = new BreakGauge(data.breakCondition);
            }

            DecideNextIntent();
        }

        public override void OnTurnStart()
        {
            base.OnTurnStart();
            ClearBlock();

            // TRIAD: Break 게이지 턴 시작 처리
            // Break gauge turn start processing
            breakGauge?.OnTurnStart();
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();

            // TRIAD: Groggy 턴 감소 및 종료 체크
            // Tick down groggy turns and check for end
            if (breakGauge != null && IsGroggy)
            {
                breakGauge.TickGroggy();

                // Groggy 종료 시 이벤트 발행
                // Publish event when groggy ends
                if (!IsGroggy)
                {
                    EventBus.Publish(new EnemyGroggyEndedEvent(this));
                    DecideNextIntent(); // 일반 패턴으로 복귀
                }
            }
        }

        /// <summary>
        /// 다음 인텐트 결정
        /// Decide next intent
        ///
        /// TRIAD: Groggy 상태면 groggyActions 사용
        /// </summary>
        public void DecideNextIntent()
        {
            // TRIAD: Groggy 상태면 groggyActions 사용
            // Use groggyActions when in groggy state
            if (IsGroggy && enemyData.HasGroggyActions)
            {
                currentIntent = enemyData.groggyActions[groggyActionIndex];
                groggyActionIndex = (groggyActionIndex + 1) % enemyData.groggyActions.Count;
                return;
            }

            // 일반 패턴
            // Normal pattern
            if (enemyData.actionPattern == null || enemyData.actionPattern.Count == 0)
            {
                currentIntent = null;
                return;
            }

            currentIntent = enemyData.actionPattern[currentActionIndex];
            currentActionIndex = (currentActionIndex + 1) % enemyData.actionPattern.Count;
        }

        /// <summary>
        /// TRIAD: 현재 인텐트 실행 (파티 멤버 타겟)
        /// TRIAD: Execute current intent on party member target
        /// </summary>
        public void ExecuteIntent(ICombatEntity target)
        {
            if (currentIntent == null || target == null) return;

            switch (currentIntent.intentType)
            {
                case EnemyIntentType.Attack:
                    ExecuteAttack(target);
                    break;
                case EnemyIntentType.Defend:
                    GainBlock(currentIntent.block);
                    break;
                case EnemyIntentType.AttackBuff:
                    ExecuteAttack(target);
                    ApplyBuffToSelf();
                    break;
                case EnemyIntentType.AttackDebuff:
                    ExecuteAttack(target);
                    ApplyDebuffToTarget(target);
                    break;
                case EnemyIntentType.Buff:
                    ApplyBuffToSelf();
                    break;
                case EnemyIntentType.Debuff:
                    ApplyDebuffToTarget(target);
                    break;
                case EnemyIntentType.Unknown:
                    // 알 수 없음 - 아무 행동 안함
                    break;
            }

            DecideNextIntent();
        }

        /// <summary>
        /// 현재 인텐트 실행 (하위 호환성)
        /// Execute current intent (backward compatibility)
        /// </summary>
        [System.Obsolete("Use ExecuteIntent(ICombatEntity) instead for TRIAD party system")]
        public void ExecuteIntent(PlayerCombat player)
        {
            ExecuteIntent((ICombatEntity)player);
        }

        private void ExecuteAttack(ICombatEntity target)
        {
            int damage = DamageCalculator.CalculateDamage(
                currentIntent.damage,
                this,
                target
            );

            for (int i = 0; i < currentIntent.hitCount; i++)
            {
                target.TakeDamage(damage);
            }
        }

        private void ApplyBuffToSelf()
        {
            if (currentIntent.statusEffect != null)
            {
                statusEffects.ApplyStatus(currentIntent.statusEffect, currentIntent.statusStacks);
            }
        }

        private void ApplyDebuffToTarget(ICombatEntity target)
        {
            if (currentIntent.statusEffect != null && target is CombatEntity combatEntity)
            {
                combatEntity.GetStatusEffects().ApplyStatus(currentIntent.statusEffect, currentIntent.statusStacks);
            }
        }

        /// <summary>
        /// 디버프 적용 (하위 호환성)
        /// Apply debuff (backward compatibility)
        /// </summary>
        [System.Obsolete("Use ApplyDebuffToTarget(ICombatEntity) instead")]
        private void ApplyDebuffToPlayer(PlayerCombat player)
        {
            ApplyDebuffToTarget(player);
        }

        #region TRIAD Break System

        /// <summary>
        /// TRIAD: 데미지 받기 (Break 체크 포함)
        /// TRIAD: Take damage with Break check
        /// </summary>
        public override void TakeDamage(int amount)
        {
            // TRIAD: Break 게이지에 데미지 기록
            // Record damage to break gauge
            if (breakGauge != null && !IsGroggy)
            {
                breakGauge.RecordDamageAndHit(amount);

                // Break 조건 체크
                // Check break condition
                if (breakGauge.CheckBreak())
                {
                    OnBreak();
                }
            }

            base.TakeDamage(amount);
        }

        /// <summary>
        /// TRIAD: Break 발생 시 처리
        /// TRIAD: Handle break trigger
        /// </summary>
        private void OnBreak()
        {
            // Groggy 상태 시작 - 현재 인텐트 취소하고 다음 인텐트 결정
            // Start groggy state - cancel current intent and decide next
            groggyActionIndex = 0;
            DecideNextIntent();

            // Break 이벤트 발행
            // Publish break event
            EventBus.Publish(new EnemyBrokenEvent(this));
        }

        /// <summary>
        /// TRIAD: Break 진행률 (UI용)
        /// TRIAD: Break progress for UI
        /// </summary>
        public float GetBreakProgress()
        {
            return breakGauge?.GetPrimaryProgress() ?? 0f;
        }

        /// <summary>
        /// TRIAD: Break 게이지 리셋
        /// TRIAD: Reset break gauge
        /// </summary>
        public void ResetBreakGauge()
        {
            breakGauge?.Reset();
        }

        #endregion

        protected override void OnDeath()
        {
            base.OnDeath();
            // 보상 처리는 CombatManager에서 수행
        }
    }

    #region TRIAD Break Events

    /// <summary>
    /// 적 Break 이벤트
    /// Enemy broken event
    /// </summary>
    public struct EnemyBrokenEvent : IGameEvent
    {
        public EnemyCombat Enemy;

        public EnemyBrokenEvent(EnemyCombat enemy)
        {
            Enemy = enemy;
        }
    }

    /// <summary>
    /// 적 Groggy 종료 이벤트
    /// Enemy groggy ended event
    /// </summary>
    public struct EnemyGroggyEndedEvent : IGameEvent
    {
        public EnemyCombat Enemy;

        public EnemyGroggyEndedEvent(EnemyCombat enemy)
        {
            Enemy = enemy;
        }
    }

    #endregion
}
