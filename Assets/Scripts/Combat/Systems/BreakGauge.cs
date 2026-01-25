using ProjectSS.Data;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 적의 Break 게이지 상태 추적
    /// Tracks enemy's Break gauge state
    ///
    /// Break 시스템:
    /// - 턴 내 누적 데미지 또는 타격 횟수가 임계값에 도달하면 Break 발생
    /// - Break 발생 시 적은 Groggy 상태가 되어 현재 행동 취소
    /// - Groggy 상태에서는 약화된 행동만 수행
    /// </summary>
    [System.Serializable]
    public class BreakGauge
    {
        /// <summary>
        /// 현재 턴 누적 데미지
        /// Current turn accumulated damage
        /// </summary>
        public int CurrentDamage { get; private set; }

        /// <summary>
        /// 현재 턴 타격 횟수
        /// Current turn hit count
        /// </summary>
        public int CurrentHits { get; private set; }

        /// <summary>
        /// Break 상태 여부
        /// Whether broken
        /// </summary>
        public bool IsBroken { get; private set; }

        /// <summary>
        /// 남은 Groggy 턴 수
        /// Remaining groggy turns
        /// </summary>
        public int GroggyTurnsLeft { get; private set; }

        /// <summary>
        /// Groggy 상태 여부
        /// Whether in groggy state
        /// </summary>
        public bool IsGroggy => GroggyTurnsLeft > 0;

        /// <summary>
        /// Break 조건 데이터 참조
        /// Reference to break condition data
        /// </summary>
        private BreakConditionData conditionData;

        /// <summary>
        /// 생성자
        /// Constructor
        /// </summary>
        /// <param name="condition">Break 조건 데이터 / Break condition data</param>
        public BreakGauge(BreakConditionData condition)
        {
            conditionData = condition;
            Reset();
        }

        /// <summary>
        /// 데미지 기록
        /// Record damage taken
        /// </summary>
        /// <param name="damage">받은 데미지 / Damage taken</param>
        public void RecordDamage(int damage)
        {
            if (conditionData == null || IsGroggy) return;

            CurrentDamage += damage;
        }

        /// <summary>
        /// 타격 기록
        /// Record hit received
        /// </summary>
        public void RecordHit()
        {
            if (conditionData == null || IsGroggy) return;

            CurrentHits++;
        }

        /// <summary>
        /// 데미지와 타격을 동시에 기록
        /// Record both damage and hit
        /// </summary>
        /// <param name="damage">받은 데미지 / Damage taken</param>
        public void RecordDamageAndHit(int damage)
        {
            RecordDamage(damage);
            RecordHit();
        }

        /// <summary>
        /// Break 조건 확인
        /// Check if break condition is met
        /// </summary>
        /// <returns>Break 발생 여부 / Whether break occurred</returns>
        public bool CheckBreak()
        {
            if (conditionData == null || IsBroken || IsGroggy) return false;

            bool conditionMet = conditionData.CheckBreak(CurrentDamage, CurrentHits);

            if (conditionMet)
            {
                TriggerBreak();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Break 발생
        /// Trigger break
        /// </summary>
        private void TriggerBreak()
        {
            IsBroken = true;
            GroggyTurnsLeft = conditionData.groggyTurns;
        }

        /// <summary>
        /// 턴 시작 시 호출 (조건에 따라 데미지/타격 리셋)
        /// Called at turn start (reset damage/hits based on condition)
        /// </summary>
        public void OnTurnStart()
        {
            if (conditionData != null && conditionData.resetOnTurnStart)
            {
                ResetTurnCounters();
            }

            // Groggy 해제 시 Break 상태도 리셋
            // Reset break state when groggy ends
            if (IsBroken && !IsGroggy)
            {
                IsBroken = false;
            }
        }

        /// <summary>
        /// Groggy 턴 감소
        /// Tick down groggy turns
        /// </summary>
        public void TickGroggy()
        {
            if (GroggyTurnsLeft > 0)
            {
                GroggyTurnsLeft--;
            }
        }

        /// <summary>
        /// 턴 카운터 리셋 (데미지/타격)
        /// Reset turn counters (damage/hits)
        /// </summary>
        public void ResetTurnCounters()
        {
            CurrentDamage = 0;
            CurrentHits = 0;
        }

        /// <summary>
        /// 전체 리셋
        /// Full reset
        /// </summary>
        public void Reset()
        {
            CurrentDamage = 0;
            CurrentHits = 0;
            IsBroken = false;
            GroggyTurnsLeft = 0;
        }

        /// <summary>
        /// 데미지 진행률 (0.0 ~ 1.0)
        /// Damage progress (0.0 ~ 1.0)
        /// </summary>
        public float GetDamageProgress()
        {
            if (conditionData == null) return 0f;
            return conditionData.GetDamageProgress(CurrentDamage);
        }

        /// <summary>
        /// 타격 진행률 (0.0 ~ 1.0)
        /// Hit progress (0.0 ~ 1.0)
        /// </summary>
        public float GetHitProgress()
        {
            if (conditionData == null) return 0f;
            return conditionData.GetHitProgress(CurrentHits);
        }

        /// <summary>
        /// 주요 진행률 (조건 타입에 따라)
        /// Primary progress (based on condition type)
        /// </summary>
        public float GetPrimaryProgress()
        {
            if (conditionData == null) return 0f;

            return conditionData.conditionType switch
            {
                BreakConditionType.DamageThreshold => GetDamageProgress(),
                BreakConditionType.HitCount => GetHitProgress(),
                BreakConditionType.Both => UnityEngine.Mathf.Max(GetDamageProgress(), GetHitProgress()),
                _ => 0f
            };
        }

        /// <summary>
        /// Break 가능 여부 (조건 데이터 존재)
        /// Whether breakable (condition data exists)
        /// </summary>
        public bool IsBreakable => conditionData != null;

        /// <summary>
        /// 조건 데이터 반환
        /// Get condition data
        /// </summary>
        public BreakConditionData GetConditionData() => conditionData;
    }
}
