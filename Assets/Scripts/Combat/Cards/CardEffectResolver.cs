using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Data;
using ProjectSS.Core;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 카드 효과 해결 클래스
    /// Card effect resolver class
    ///
    /// TRIAD: Focus 보너스 적용 지원
    /// </summary>
    public class CardEffectResolver
    {
        private readonly DeckManager deckManager;
        private readonly EnergySystem energySystem;

        public CardEffectResolver(DeckManager deck, EnergySystem energy)
        {
            deckManager = deck;
            energySystem = energy;
        }

        /// <summary>
        /// 카드 효과 해결
        /// Resolve card effects
        ///
        /// TRIAD: Focus 보너스 효과 적용 지원
        /// </summary>
        public void ResolveCard(CardInstance card, ICombatEntity source, ICombatEntity target, List<EnemyCombat> allEnemies)
        {
            var data = card.EffectiveData;

            // TRIAD: 카드 사용 시점의 Focus 스택 저장 (보너스 계산용)
            // Store Focus stacks at card play time for bonus calculation
            int focusAtPlay = GetFocusStacks(source);

            foreach (var effect in data.effects)
            {
                ResolveEffect(effect, source, target, allEnemies, focusAtPlay);
            }

            // TRIAD: Focus 보너스 효과 적용 (Focus가 있었던 경우)
            // Apply Focus bonus effects if Focus was available
            if (focusAtPlay > 0 && data.focusBonusEffects != null && data.focusBonusEffects.Count > 0)
            {
                foreach (var bonus in data.focusBonusEffects)
                {
                    ResolveEffect(bonus, source, target, allEnemies, focusAtPlay);
                }
            }

            // 카드 사용 이벤트 발행
            EventBus.Publish(new CardPlayedEvent(data.cardId));
        }

        /// <summary>
        /// TRIAD: 엔티티의 Focus 스택 가져오기
        /// TRIAD: Get Focus stacks from entity
        /// </summary>
        private int GetFocusStacks(ICombatEntity source)
        {
            if (source is PartyMemberCombat partyMember)
            {
                return partyMember.FocusStacks;
            }
            return 0;
        }

        /// <summary>
        /// 개별 효과 해결
        /// Resolve individual effect
        ///
        /// TRIAD: Focus 보너스 적용
        /// </summary>
        private void ResolveEffect(CardEffect effect, ICombatEntity source, ICombatEntity target, List<EnemyCombat> allEnemies, int focusStacks = 0)
        {
            switch (effect.effectType)
            {
                case CardEffectType.Damage:
                    ResolveDamage(effect, source, target, allEnemies, focusStacks);
                    break;

                case CardEffectType.Block:
                    ResolveBlock(effect, source, focusStacks);
                    break;

                case CardEffectType.ApplyStatus:
                    ResolveApplyStatus(effect, source, target, allEnemies);
                    break;

                case CardEffectType.Draw:
                    deckManager.DrawCards(effect.value);
                    break;

                case CardEffectType.GainEnergy:
                    energySystem.Gain(effect.value);
                    break;

                case CardEffectType.Heal:
                    source.Heal(effect.value);
                    break;

                case CardEffectType.Discard:
                    // UI에서 선택 필요 - 랜덤 버림 구현
                    for (int i = 0; i < effect.value && deckManager.Hand.Count > 0; i++)
                    {
                        var randomCard = deckManager.Hand[UnityEngine.Random.Range(0, deckManager.Hand.Count)];
                        deckManager.DiscardCard(randomCard);
                    }
                    break;

                case CardEffectType.Exhaust:
                    // 이 효과는 카드 자체에 적용되므로 여기서 처리하지 않음
                    // Handled by DeckManager when card is played
                    break;
            }
        }

        /// <summary>
        /// 데미지 효과 해결
        /// Resolve damage effect
        ///
        /// TRIAD: Focus 보너스 데미지 적용
        /// </summary>
        private void ResolveDamage(CardEffect effect, ICombatEntity source, ICombatEntity target, List<EnemyCombat> allEnemies, int focusStacks = 0)
        {
            int baseDamage = effect.value;

            switch (effect.targetType)
            {
                case TargetType.SingleEnemy:
                    if (target != null)
                    {
                        int damage = CalculateDamageWithFocus(baseDamage, source, target, focusStacks);
                        target.TakeDamage(damage);
                    }
                    break;

                case TargetType.AllEnemies:
                    foreach (var enemy in allEnemies)
                    {
                        if (enemy.IsAlive)
                        {
                            int damage = CalculateDamageWithFocus(baseDamage, source, enemy, focusStacks);
                            enemy.TakeDamage(damage);
                        }
                    }
                    break;

                case TargetType.Self:
                    int selfDamage = CalculateDamageWithFocus(baseDamage, source, source, focusStacks);
                    source.TakeDamage(selfDamage);
                    break;

                case TargetType.RandomEnemy:
                    var aliveEnemies = allEnemies.FindAll(e => e.IsAlive);
                    if (aliveEnemies.Count > 0)
                    {
                        var randomTarget = aliveEnemies[Random.Range(0, aliveEnemies.Count)];
                        int damage = CalculateDamageWithFocus(baseDamage, source, randomTarget, focusStacks);
                        randomTarget.TakeDamage(damage);
                    }
                    break;
            }
        }

        /// <summary>
        /// TRIAD: Focus 보너스가 적용된 데미지 계산
        /// TRIAD: Calculate damage with Focus bonus
        /// </summary>
        private int CalculateDamageWithFocus(int baseDamage, ICombatEntity source, ICombatEntity target, int focusStacks)
        {
            int damage = DamageCalculator.CalculateDamage(baseDamage, source, target);

            // TRIAD: Focus 보너스 적용 (source가 파티원인 경우)
            // Apply Focus bonus if source is party member
            if (focusStacks > 0)
            {
                damage = FocusSystem.ApplyFocusDamageBonus(damage, focusStacks);
            }

            return damage;
        }

        /// <summary>
        /// 블록 효과 해결
        /// Resolve block effect
        ///
        /// TRIAD: Focus 보너스 블록 적용
        /// </summary>
        private void ResolveBlock(CardEffect effect, ICombatEntity source, int focusStacks = 0)
        {
            int block = DamageCalculator.CalculateBlock(effect.value, source);

            // TRIAD: Focus 보너스 적용 (source가 파티원인 경우)
            // Apply Focus bonus if source is party member
            if (focusStacks > 0)
            {
                block = FocusSystem.ApplyFocusBlockBonus(block, focusStacks);
            }

            source.GainBlock(block);
        }

        private void ResolveApplyStatus(CardEffect effect, ICombatEntity source, ICombatEntity target, List<EnemyCombat> allEnemies)
        {
            if (effect.statusEffect == null) return;

            switch (effect.targetType)
            {
                case TargetType.Self:
                    ApplyStatusTo(source, effect);
                    break;

                case TargetType.SingleEnemy:
                    if (target != null)
                        ApplyStatusTo(target, effect);
                    break;

                case TargetType.AllEnemies:
                    foreach (var enemy in allEnemies)
                    {
                        if (enemy.IsAlive)
                            ApplyStatusTo(enemy, effect);
                    }
                    break;

                case TargetType.RandomEnemy:
                    var aliveEnemies = allEnemies.FindAll(e => e.IsAlive);
                    if (aliveEnemies.Count > 0)
                    {
                        var randomTarget = aliveEnemies[UnityEngine.Random.Range(0, aliveEnemies.Count)];
                        ApplyStatusTo(randomTarget, effect);
                    }
                    break;
            }
        }

        private void ApplyStatusTo(ICombatEntity entity, CardEffect effect)
        {
            if (entity is CombatEntity combatEntity)
            {
                combatEntity.GetStatusEffects().ApplyStatus(effect.statusEffect, effect.statusStacks);
            }
        }
    }
}
