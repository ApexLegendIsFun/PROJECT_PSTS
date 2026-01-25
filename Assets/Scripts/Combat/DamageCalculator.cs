using UnityEngine;
using ProjectSS.Data;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 데미지 계산기
    /// Damage calculator with modifiers
    /// </summary>
    public static class DamageCalculator
    {
        private const float WEAK_MODIFIER = 0.75f;
        private const float VULNERABLE_MODIFIER = 1.5f;

        /// <summary>
        /// 최종 데미지 계산
        /// Calculate final damage with all modifiers
        /// baseDamage + Strength × Weak(0.75) × Vulnerable(1.5) = finalDamage
        /// </summary>
        public static int CalculateDamage(int baseDamage, ICombatEntity attacker, ICombatEntity target)
        {
            float damage = baseDamage;

            // 힘 보너스 추가
            // Add Strength bonus
            if (attacker is CombatEntity attackerEntity)
            {
                int strength = attackerEntity.GetStatusEffects().GetStacks(StatusEffectType.Strength);
                damage += strength;
            }

            // 약화 적용 (공격자)
            // Apply Weak (attacker)
            if (attacker is CombatEntity weakCheck)
            {
                if (weakCheck.GetStatusEffects().HasStatus(StatusEffectType.Weak))
                {
                    damage *= WEAK_MODIFIER;
                }
            }

            // 취약 적용 (대상)
            // Apply Vulnerable (target)
            if (target is CombatEntity targetEntity)
            {
                if (targetEntity.GetStatusEffects().HasStatus(StatusEffectType.Vulnerable))
                {
                    damage *= VULNERABLE_MODIFIER;
                }
            }

            return Mathf.Max(0, Mathf.RoundToInt(damage));
        }

        /// <summary>
        /// 블록 계산 (민첩 보너스 포함)
        /// Calculate block with Dexterity bonus
        /// </summary>
        public static int CalculateBlock(int baseBlock, ICombatEntity entity)
        {
            float block = baseBlock;

            // 민첩 보너스 추가
            // Add Dexterity bonus
            if (entity is CombatEntity combatEntity)
            {
                int dexterity = combatEntity.GetStatusEffects().GetStacks(StatusEffectType.Dexterity);
                block += dexterity;

                // 허약 적용
                // Apply Frail
                if (combatEntity.GetStatusEffects().HasStatus(StatusEffectType.Frail))
                {
                    block *= WEAK_MODIFIER;
                }
            }

            return Mathf.Max(0, Mathf.RoundToInt(block));
        }
    }
}
