// Combat/Cards/CardEffectResolver.cs
// 카드 효과 해결자 - 카드 효과를 실행

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data.Cards;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 카드 효과 해결자
    /// CardInstance의 효과들을 순차적으로 실행
    /// </summary>
    public class CardEffectResolver
    {
        private readonly CombatManager _combatManager;

        public CardEffectResolver(CombatManager combatManager)
        {
            _combatManager = combatManager;
        }

        /// <summary>
        /// 카드의 모든 효과 실행
        /// </summary>
        /// <param name="card">실행할 카드</param>
        /// <param name="source">카드 사용자</param>
        /// <param name="target">타겟 (SingleEnemy 등)</param>
        /// <returns>성공 여부</returns>
        public bool ResolveCard(CardInstance card, ICombatEntity source, ICombatEntity target)
        {
            if (card == null || !card.IsValid())
            {
                Debug.LogWarning("[CardEffectResolver] Invalid card");
                return false;
            }

            if (source == null || !source.IsAlive)
            {
                Debug.LogWarning("[CardEffectResolver] Invalid source");
                return false;
            }

            var effects = card.Effects;
            if (effects == null || effects.Count == 0)
            {
                Debug.LogWarning($"[CardEffectResolver] Card {card.CardName} has no effects");
                return true; // 효과가 없어도 카드 사용은 성공
            }

            // 타겟 유효성 검사 (타겟팅이 필요한 카드)
            if (RequiresTarget(card) && (target == null || !target.IsAlive))
            {
                Debug.LogWarning($"[CardEffectResolver] Card {card.CardName} requires a valid target");
                return false;
            }

            // 효과 컨텍스트 생성
            var context = new CombatEffectContext(source, target, _combatManager);

            Debug.Log($"[CardEffectResolver] Resolving {card.CardName} ({effects.Count} effects)");

            // 모든 효과 순차 실행
            foreach (var effect in effects)
            {
                if (effect == null)
                {
                    Debug.LogWarning("[CardEffectResolver] Null effect in card");
                    continue;
                }

                try
                {
                    effect.Execute(context);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[CardEffectResolver] Error executing effect: {e.Message}");
                }
            }

            return true;
        }

        /// <summary>
        /// 카드가 타겟을 필요로 하는지 확인
        /// </summary>
        private bool RequiresTarget(CardInstance card)
        {
            return card.TargetType switch
            {
                TargetType.SingleEnemy => true,
                TargetType.SingleAlly => true,
                TargetType.Self => false,
                TargetType.AllEnemies => false,
                TargetType.AllAllies => false,
                TargetType.Random => false,
                _ => false
            };
        }

        /// <summary>
        /// 랜덤 타겟 선택 (Random 타겟 타입용)
        /// </summary>
        public ICombatEntity GetRandomTarget(CardInstance card, ICombatEntity source)
        {
            if (card.TargetType != TargetType.Random)
            {
                return null;
            }

            // 적에게 사용하는 카드면 적 중 랜덤
            if (card.CardType == CardType.Attack)
            {
                var enemies = _combatManager?.GetAliveEnemies();
                if (enemies != null && enemies.Count > 0)
                {
                    return enemies[Random.Range(0, enemies.Count)];
                }
            }

            return null;
        }
    }
}
