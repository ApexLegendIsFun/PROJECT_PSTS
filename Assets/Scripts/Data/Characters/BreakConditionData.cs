using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 브레이크 조건 데이터 ScriptableObject
    /// Break condition data ScriptableObject for Pattern Cancel system
    /// </summary>
    [CreateAssetMenu(fileName = "NewBreakCondition", menuName = "Game/Enemies/Break Condition")]
    public class BreakConditionData : ScriptableObject
    {
        [Header("조건 타입 (Condition Type)")]
        [Tooltip("브레이크 조건 타입 / Break condition type")]
        public BreakConditionType conditionType = BreakConditionType.DamageThreshold;

        [Header("임계값 (Thresholds)")]
        [Tooltip("임계 데미지 (DamageThreshold 또는 Both 시 사용) / Damage threshold")]
        [Range(10, 100)]
        public int damageThreshold = 20;

        [Tooltip("타격 횟수 (HitCount 또는 Both 시 사용) / Hit count threshold")]
        [Range(1, 10)]
        public int hitCountThreshold = 5;

        [Header("기절 설정 (Groggy Settings)")]
        [Tooltip("기절 지속 턴 수 / Groggy duration in turns")]
        [Range(1, 3)]
        public int groggyTurns = 1;

        [Tooltip("턴 시작 시 게이지 리셋 여부 / Reset gauge on turn start")]
        public bool resetOnTurnStart = true;

        /// <summary>
        /// 브레이크 조건 충족 여부 확인
        /// Check if break condition is met
        /// </summary>
        public bool CheckBreak(int currentDamage, int currentHits)
        {
            return conditionType switch
            {
                BreakConditionType.DamageThreshold => currentDamage >= damageThreshold,
                BreakConditionType.HitCount => currentHits >= hitCountThreshold,
                BreakConditionType.Both => currentDamage >= damageThreshold || currentHits >= hitCountThreshold,
                _ => false
            };
        }

        /// <summary>
        /// 데미지 진행률 계산 (0~1)
        /// Calculate damage progress (0~1)
        /// </summary>
        public float GetDamageProgress(int currentDamage)
        {
            if (conditionType == BreakConditionType.HitCount)
                return 0f;

            return Mathf.Clamp01((float)currentDamage / damageThreshold);
        }

        /// <summary>
        /// 타격 횟수 진행률 계산 (0~1)
        /// Calculate hit count progress (0~1)
        /// </summary>
        public float GetHitProgress(int currentHits)
        {
            if (conditionType == BreakConditionType.DamageThreshold)
                return 0f;

            return Mathf.Clamp01((float)currentHits / hitCountThreshold);
        }

        /// <summary>
        /// 브레이크 조건 설명 생성
        /// Generate break condition description
        /// </summary>
        public string GetDescription()
        {
            return conditionType switch
            {
                BreakConditionType.DamageThreshold =>
                    $"한 턴에 {damageThreshold} 데미지로 패턴 캔슬 / Deal {damageThreshold} damage in one turn to cancel pattern",
                BreakConditionType.HitCount =>
                    $"한 턴에 {hitCountThreshold}회 타격으로 패턴 캔슬 / Hit {hitCountThreshold} times in one turn to cancel pattern",
                BreakConditionType.Both =>
                    $"{damageThreshold} 데미지 또는 {hitCountThreshold}회 타격으로 패턴 캔슬 / Deal {damageThreshold} damage OR hit {hitCountThreshold} times to cancel pattern",
                _ => "조건 없음 / No condition"
            };
        }
    }
}
