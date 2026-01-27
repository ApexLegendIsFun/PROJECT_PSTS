// Core/Cards/IEffectContext.cs
// 카드 효과 실행 컨텍스트 인터페이스

using ProjectSS.Core;

namespace ProjectSS.Core.Cards
{
    /// <summary>
    /// 카드 효과 실행을 위한 컨텍스트 인터페이스
    /// Combat에서 구현하여 실제 엔티티에 효과 적용
    /// </summary>
    public interface IEffectContext
    {
        /// <summary>
        /// 효과 사용자 ID
        /// </summary>
        string SourceId { get; }

        /// <summary>
        /// 타겟 ID (단일 타겟)
        /// </summary>
        string TargetId { get; }

        /// <summary>
        /// 타겟에게 데미지
        /// </summary>
        void DealDamage(int amount);

        /// <summary>
        /// 모든 적에게 데미지
        /// </summary>
        void DealDamageToAllEnemies(int amount);

        /// <summary>
        /// 사용자에게 방어막
        /// </summary>
        void GainBlock(int amount);

        /// <summary>
        /// 카드 드로우
        /// </summary>
        void DrawCards(int count);

        /// <summary>
        /// 사용자 회복
        /// </summary>
        void HealSource(int amount);

        /// <summary>
        /// 타겟 회복
        /// </summary>
        void HealTarget(int amount);

        /// <summary>
        /// 타겟에게 상태이상 적용
        /// </summary>
        void ApplyStatusToTarget(StatusEffectType type, int stacks);

        /// <summary>
        /// 사용자에게 상태이상 적용
        /// </summary>
        void ApplyStatusToSource(StatusEffectType type, int stacks);

        /// <summary>
        /// 모든 적에게 상태이상 적용
        /// </summary>
        void ApplyStatusToAllEnemies(StatusEffectType type, int stacks);
    }
}
