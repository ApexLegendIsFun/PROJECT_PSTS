// Combat/Cards/CombatEffectContext.cs
// 전투 효과 컨텍스트 - IEffectContext 구현

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Cards;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 전투 중 카드 효과 실행을 위한 컨텍스트
    /// IEffectContext를 구현하여 실제 전투 엔티티에 효과 적용
    /// </summary>
    public class CombatEffectContext : IEffectContext
    {
        private readonly ICombatEntity _source;
        private readonly ICombatEntity _target;
        private readonly CombatManager _combatManager;
        private readonly PartyMemberCombat _sourceAsPartyMember;

        public string SourceId => _source?.EntityId ?? string.Empty;
        public string TargetId => _target?.EntityId ?? string.Empty;

        /// <summary>
        /// 효과 사용자
        /// </summary>
        public ICombatEntity Source => _source;

        /// <summary>
        /// 효과 타겟
        /// </summary>
        public ICombatEntity Target => _target;

        public CombatEffectContext(ICombatEntity source, ICombatEntity target, CombatManager combatManager)
        {
            _source = source;
            _target = target;
            _combatManager = combatManager;
            _sourceAsPartyMember = source as PartyMemberCombat;
        }

        #region Damage

        public void DealDamage(int amount)
        {
            if (_target == null || !_target.IsAlive)
            {
                Debug.LogWarning("[EffectContext] No valid target for damage");
                return;
            }

            // TODO: Vulnerable 등 상태이상 계산
            int finalDamage = CalculateDamage(amount, _target);

            _target.TakeDamage(finalDamage, _source);

            Debug.Log($"[Effect] {_source?.DisplayName} dealt {finalDamage} damage to {_target.DisplayName}");

            EventBus.Publish(new DamageDealtEvent
            {
                SourceId = _source?.EntityId,
                TargetId = _target.EntityId,
                Damage = finalDamage,
                BlockedAmount = 0, // TakeDamage 내부에서 계산됨
                IsCritical = false
            });
        }

        public void DealDamageToAllEnemies(int amount)
        {
            if (_combatManager == null)
            {
                Debug.LogWarning("[EffectContext] No CombatManager for AOE damage");
                return;
            }

            var enemies = _combatManager.GetAliveEnemies();

            foreach (var enemy in enemies)
            {
                int finalDamage = CalculateDamage(amount, enemy);
                enemy.TakeDamage(finalDamage, _source);

                Debug.Log($"[Effect] {_source?.DisplayName} dealt {finalDamage} AOE damage to {enemy.DisplayName}");

                EventBus.Publish(new DamageDealtEvent
                {
                    SourceId = _source?.EntityId,
                    TargetId = enemy.EntityId,
                    Damage = finalDamage,
                    BlockedAmount = 0,
                    IsCritical = false
                });
            }
        }

        private int CalculateDamage(int baseDamage, ICombatEntity target)
        {
            // TODO: Strength 보너스, Weak 감소, Vulnerable 증가 등 계산
            // StatusEffect 시스템 연동 필요
            return Mathf.Max(0, baseDamage);
        }

        #endregion

        #region Block

        public void GainBlock(int amount)
        {
            if (_source == null || !_source.IsAlive)
            {
                Debug.LogWarning("[EffectContext] No valid source for block");
                return;
            }

            // TODO: Dexterity 보너스 계산
            int finalBlock = CalculateBlock(amount);

            _source.GainBlock(finalBlock);

            Debug.Log($"[Effect] {_source.DisplayName} gained {finalBlock} block");

            EventBus.Publish(new BlockGainedEvent
            {
                EntityId = _source.EntityId,
                Amount = finalBlock,
                TotalBlock = _source.CurrentBlock
            });
        }

        private int CalculateBlock(int baseBlock)
        {
            // TODO: Dexterity 보너스 계산
            return Mathf.Max(0, baseBlock);
        }

        #endregion

        #region Draw

        public void DrawCards(int count)
        {
            if (_sourceAsPartyMember == null)
            {
                Debug.LogWarning("[EffectContext] Source is not a party member, cannot draw cards");
                return;
            }

            var deckManager = _sourceAsPartyMember.DeckManager;
            if (deckManager == null)
            {
                Debug.LogWarning("[EffectContext] No deck manager for card draw");
                return;
            }

            deckManager.DrawCards(count);

            Debug.Log($"[Effect] {_source.DisplayName} drew {count} cards");
        }

        #endregion

        #region Heal

        public void HealSource(int amount)
        {
            if (_source == null || !_source.IsAlive)
            {
                Debug.LogWarning("[EffectContext] No valid source for heal");
                return;
            }

            _source.Heal(amount);

            Debug.Log($"[Effect] {_source.DisplayName} healed for {amount}");

            EventBus.Publish(new HealEvent
            {
                EntityId = _source.EntityId,
                Amount = amount,
                CurrentHP = _source.CurrentHP,
                MaxHP = _source.MaxHP
            });
        }

        public void HealTarget(int amount)
        {
            if (_target == null || !_target.IsAlive)
            {
                Debug.LogWarning("[EffectContext] No valid target for heal");
                return;
            }

            _target.Heal(amount);

            Debug.Log($"[Effect] {_target.DisplayName} healed for {amount}");

            EventBus.Publish(new HealEvent
            {
                EntityId = _target.EntityId,
                Amount = amount,
                CurrentHP = _target.CurrentHP,
                MaxHP = _target.MaxHP
            });
        }

        #endregion

        #region Status Effects

        public void ApplyStatusToTarget(StatusEffectType type, int stacks)
        {
            if (_target == null || !_target.IsAlive)
            {
                Debug.LogWarning("[EffectContext] No valid target for status effect");
                return;
            }

            // TODO: StatusEffectManager 연동
            Debug.Log($"[Effect] Applied {stacks} {type} to {_target.DisplayName}");

            EventBus.Publish(new StatusEffectAppliedEvent
            {
                EntityId = _target.EntityId,
                EffectType = type,
                Stacks = stacks
            });
        }

        public void ApplyStatusToSource(StatusEffectType type, int stacks)
        {
            if (_source == null || !_source.IsAlive)
            {
                Debug.LogWarning("[EffectContext] No valid source for status effect");
                return;
            }

            // TODO: StatusEffectManager 연동
            Debug.Log($"[Effect] Applied {stacks} {type} to {_source.DisplayName}");

            EventBus.Publish(new StatusEffectAppliedEvent
            {
                EntityId = _source.EntityId,
                EffectType = type,
                Stacks = stacks
            });
        }

        public void ApplyStatusToAllEnemies(StatusEffectType type, int stacks)
        {
            if (_combatManager == null)
            {
                Debug.LogWarning("[EffectContext] No CombatManager for AOE status");
                return;
            }

            var enemies = _combatManager.GetAliveEnemies();

            foreach (var enemy in enemies)
            {
                // TODO: StatusEffectManager 연동
                Debug.Log($"[Effect] Applied {stacks} {type} to {enemy.DisplayName}");

                EventBus.Publish(new StatusEffectAppliedEvent
                {
                    EntityId = enemy.EntityId,
                    EffectType = type,
                    Stacks = stacks
                });
            }
        }

        #endregion
    }
}
