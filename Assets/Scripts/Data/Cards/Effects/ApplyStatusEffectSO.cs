// Data/Cards/Effects/ApplyStatusEffectSO.cs
// 상태이상 적용 효과 SO

using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Cards;

namespace ProjectSS.Data.Cards
{
    /// <summary>
    /// 상태이상을 적용하는 효과
    /// </summary>
    [CreateAssetMenu(fileName = "StatusEffect", menuName = "PSTS/Card Effects/Apply Status")]
    public class ApplyStatusEffectSO : CardEffectSO
    {
        [Header("Status Settings")]
        [Tooltip("적용할 상태이상 타입")]
        [SerializeField] private StatusEffectType _statusType = StatusEffectType.Weak;

        [Tooltip("스택 수")]
        [SerializeField] private int _stacks = 1;

        [Tooltip("적용 대상")]
        [SerializeField] private StatusTargetType _targetType = StatusTargetType.Target;

        /// <summary>
        /// 상태이상 타입
        /// </summary>
        public StatusEffectType StatusType => _statusType;

        /// <summary>
        /// 스택 수
        /// </summary>
        public int Stacks => _stacks;

        /// <summary>
        /// 적용 대상 타입
        /// </summary>
        public StatusTargetType Target => _targetType;

        public override void Execute(IEffectContext context)
        {
            switch (_targetType)
            {
                case StatusTargetType.Self:
                    context.ApplyStatusToSource(_statusType, _stacks);
                    break;

                case StatusTargetType.Target:
                    context.ApplyStatusToTarget(_statusType, _stacks);
                    break;

                case StatusTargetType.AllEnemies:
                    context.ApplyStatusToAllEnemies(_statusType, _stacks);
                    break;
            }
        }

        public override string GetValueString()
        {
            return _stacks.ToString();
        }
    }

    /// <summary>
    /// 상태이상 적용 대상 타입
    /// </summary>
    public enum StatusTargetType
    {
        Self,           // 사용자
        Target,         // 지정 타겟
        AllEnemies      // 모든 적
    }
}
