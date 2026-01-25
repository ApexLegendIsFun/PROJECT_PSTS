using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 카드 효과 정의 (직렬화 가능)
    /// Card effect definition (serializable)
    /// </summary>
    [System.Serializable]
    public class CardEffect
    {
        [Tooltip("효과 타입 / Effect type")]
        public CardEffectType effectType;

        [Tooltip("효과 수치 / Effect value")]
        public int value;

        [Tooltip("대상 타입 / Target type")]
        public TargetType targetType = TargetType.SingleEnemy;

        [Tooltip("적용할 상태이상 (ApplyStatus 타입일 경우) / Status to apply")]
        public StatusEffectData statusEffect;

        [Tooltip("상태이상 스택 수 / Status effect stacks")]
        public int statusStacks = 1;

        /// <summary>
        /// 효과 설명 텍스트 생성
        /// Generate effect description text
        /// </summary>
        public string GetDescription()
        {
            return effectType switch
            {
                CardEffectType.Damage => $"Deal {value} damage",
                CardEffectType.Block => $"Gain {value} Block",
                CardEffectType.ApplyStatus => statusEffect != null
                    ? $"Apply {statusStacks} {statusEffect.statusName}"
                    : "Apply status",
                CardEffectType.Draw => $"Draw {value} card(s)",
                CardEffectType.GainEnergy => $"Gain {value} Energy",
                CardEffectType.Heal => $"Heal {value} HP",
                CardEffectType.Discard => $"Discard {value} card(s)",
                CardEffectType.Exhaust => "Exhaust",
                _ => ""
            };
        }
    }
}
