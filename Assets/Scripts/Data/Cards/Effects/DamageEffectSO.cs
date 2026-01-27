// Data/Cards/Effects/DamageEffectSO.cs
// 데미지 효과 SO

using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Cards;

namespace ProjectSS.Data.Cards
{
    /// <summary>
    /// 데미지를 주는 효과
    /// </summary>
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "PSTS/Card Effects/Damage")]
    public class DamageEffectSO : CardEffectSO
    {
        [Header("Damage Settings")]
        [Tooltip("기본 데미지")]
        [SerializeField] private int _baseDamage = 6;

        [Tooltip("타겟 타입")]
        [SerializeField] private TargetType _targetType = TargetType.SingleEnemy;

        [Tooltip("힘(Strength) 스케일링 적용 여부")]
        [SerializeField] private bool _scalesWithStrength = true;

        /// <summary>
        /// 기본 데미지 값
        /// </summary>
        public int BaseDamage => _baseDamage;

        /// <summary>
        /// 타겟 타입
        /// </summary>
        public TargetType TargetType => _targetType;

        /// <summary>
        /// 힘 스케일링 여부
        /// </summary>
        public bool ScalesWithStrength => _scalesWithStrength;

        public override void Execute(IEffectContext context)
        {
            // TODO: Strength 보너스 계산 (StatusEffect 시스템 연동 후)
            int finalDamage = _baseDamage;

            switch (_targetType)
            {
                case TargetType.SingleEnemy:
                    context.DealDamage(finalDamage);
                    break;

                case TargetType.AllEnemies:
                    context.DealDamageToAllEnemies(finalDamage);
                    break;

                // 다른 타겟 타입은 필요시 추가
                default:
                    context.DealDamage(finalDamage);
                    break;
            }
        }

        public override string GetValueString()
        {
            return _baseDamage.ToString();
        }
    }
}
