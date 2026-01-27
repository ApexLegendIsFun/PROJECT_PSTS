// Data/Cards/Effects/HealEffectSO.cs
// 회복 효과 SO

using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Cards;

namespace ProjectSS.Data.Cards
{
    /// <summary>
    /// HP를 회복하는 효과
    /// </summary>
    [CreateAssetMenu(fileName = "HealEffect", menuName = "PSTS/Card Effects/Heal")]
    public class HealEffectSO : CardEffectSO
    {
        [Header("Heal Settings")]
        [Tooltip("회복량")]
        [SerializeField] private int _healAmount = 5;

        [Tooltip("타겟 타입")]
        [SerializeField] private HealTargetType _targetType = HealTargetType.Self;

        /// <summary>
        /// 회복량
        /// </summary>
        public int HealAmount => _healAmount;

        /// <summary>
        /// 힐 대상 타입
        /// </summary>
        public HealTargetType HealTarget => _targetType;

        public override void Execute(IEffectContext context)
        {
            switch (_targetType)
            {
                case HealTargetType.Self:
                    context.HealSource(_healAmount);
                    break;

                case HealTargetType.Target:
                    context.HealTarget(_healAmount);
                    break;
            }
        }

        public override string GetValueString()
        {
            return _healAmount.ToString();
        }
    }

    /// <summary>
    /// 힐 대상 타입
    /// </summary>
    public enum HealTargetType
    {
        Self,       // 사용자
        Target      // 지정 타겟
    }
}
