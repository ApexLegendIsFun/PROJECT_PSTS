using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Combat
{
    /// <summary>
    /// TRIAD: 성과 보상 시스템
    /// Performance reward system
    ///
    /// Speed: 빠른 클리어 보너스
    /// Impact: 오버킬 데미지 보너스
    /// Risk: 기절 없이 클리어 보너스
    /// </summary>
    public class PerformanceSystem
    {
        #region Constants

        /// <summary>
        /// Speed 보너스 - 턴당 기준치
        /// Speed bonus - base turns per enemy type
        /// </summary>
        public const int SPEED_NORMAL_BASE_TURNS = 4;
        public const int SPEED_ELITE_BASE_TURNS = 6;
        public const int SPEED_BOSS_BASE_TURNS = 10;
        public const int SPEED_BONUS_PER_TURN = 5;
        public const int SPEED_MAX_BONUS = 50;

        /// <summary>
        /// Impact 보너스 - 오버킬당 포인트
        /// Impact bonus - points per overkill damage
        /// </summary>
        public const int IMPACT_BONUS_PER_OVERKILL = 1;
        public const int IMPACT_MAX_BONUS = 30;

        /// <summary>
        /// Risk 보너스 - 무기절 클리어
        /// Risk bonus - no incapacitation clear
        /// </summary>
        public const int RISK_NO_INCAP_BONUS = 25;
        public const int RISK_ONE_INCAP_BONUS = 10;
        public const int RISK_PENALTY_PER_INCAP = 10;

        #endregion

        private int turnsTaken;
        private int totalOverkillDamage;
        private int incapacitationCount;
        private EnemyType combatEnemyType;
        private int enemyCount;

        /// <summary>
        /// 현재 턴 수
        /// Current turns taken
        /// </summary>
        public int TurnsTaken => turnsTaken;

        /// <summary>
        /// 총 오버킬 데미지
        /// Total overkill damage
        /// </summary>
        public int TotalOverkillDamage => totalOverkillDamage;

        /// <summary>
        /// 기절 횟수
        /// Incapacitation count
        /// </summary>
        public int IncapacitationCount => incapacitationCount;

        /// <summary>
        /// 전투 시작 시 초기화
        /// Initialize at combat start
        /// </summary>
        public void Initialize(EnemyType enemyType, int numberOfEnemies)
        {
            turnsTaken = 0;
            totalOverkillDamage = 0;
            incapacitationCount = 0;
            combatEnemyType = enemyType;
            enemyCount = numberOfEnemies;
        }

        /// <summary>
        /// 턴 기록
        /// Record turn
        /// </summary>
        public void RecordTurn()
        {
            turnsTaken++;
        }

        /// <summary>
        /// 오버킬 데미지 기록
        /// Record overkill damage
        /// </summary>
        public void RecordOverkill(int overkillAmount)
        {
            if (overkillAmount > 0)
            {
                totalOverkillDamage += overkillAmount;
            }
        }

        /// <summary>
        /// 캐릭터 기절 기록
        /// Record character incapacitation
        /// </summary>
        public void RecordIncapacitation()
        {
            incapacitationCount++;
        }

        /// <summary>
        /// Speed 보너스 계산
        /// Calculate speed bonus (fast clear)
        /// </summary>
        public int CalculateSpeedBonus()
        {
            int baseTurns = GetBaseTurns(combatEnemyType) * enemyCount;

            // 기준 턴보다 빠르면 보너스
            int turnsSaved = baseTurns - turnsTaken;

            if (turnsSaved <= 0)
                return 0;

            int bonus = turnsSaved * SPEED_BONUS_PER_TURN;
            return Mathf.Min(bonus, SPEED_MAX_BONUS);
        }

        /// <summary>
        /// Impact 보너스 계산
        /// Calculate impact bonus (overkill damage)
        /// </summary>
        public int CalculateImpactBonus()
        {
            int bonus = totalOverkillDamage * IMPACT_BONUS_PER_OVERKILL;
            return Mathf.Min(bonus, IMPACT_MAX_BONUS);
        }

        /// <summary>
        /// Risk 보너스 계산
        /// Calculate risk bonus (no incapacitation)
        /// </summary>
        public int CalculateRiskBonus()
        {
            if (incapacitationCount == 0)
            {
                return RISK_NO_INCAP_BONUS;
            }
            else if (incapacitationCount == 1)
            {
                return RISK_ONE_INCAP_BONUS;
            }
            else
            {
                // 2명 이상 기절 시 보너스 없음
                return 0;
            }
        }

        /// <summary>
        /// 전체 성과 데이터 생성
        /// Generate total performance data
        /// </summary>
        public CombatPerformanceData GeneratePerformanceData()
        {
            return new CombatPerformanceData
            {
                speedBonus = CalculateSpeedBonus(),
                impactBonus = CalculateImpactBonus(),
                riskBonus = CalculateRiskBonus(),
                turnsUsed = turnsTaken,
                overkillDamage = totalOverkillDamage,
                incapacitationCount = incapacitationCount
            };
        }

        /// <summary>
        /// 적 타입별 기준 턴 수
        /// Get base turns by enemy type
        /// </summary>
        private int GetBaseTurns(EnemyType enemyType)
        {
            return enemyType switch
            {
                EnemyType.Normal => SPEED_NORMAL_BASE_TURNS,
                EnemyType.Elite => SPEED_ELITE_BASE_TURNS,
                EnemyType.Boss => SPEED_BOSS_BASE_TURNS,
                _ => SPEED_NORMAL_BASE_TURNS
            };
        }

        #region Static Helpers

        /// <summary>
        /// 성과 등급 반환
        /// Get performance grade
        /// </summary>
        public static string GetPerformanceGrade(int totalBonus)
        {
            if (totalBonus >= 80)
                return "S";
            else if (totalBonus >= 60)
                return "A";
            else if (totalBonus >= 40)
                return "B";
            else if (totalBonus >= 20)
                return "C";
            else
                return "D";
        }

        /// <summary>
        /// 성과 등급 색상 반환
        /// Get performance grade color
        /// </summary>
        public static Color GetGradeColor(string grade)
        {
            return grade switch
            {
                "S" => new Color(1f, 0.85f, 0.2f),      // Gold
                "A" => new Color(0.6f, 0.4f, 0.9f),    // Purple
                "B" => new Color(0.3f, 0.7f, 0.9f),    // Blue
                "C" => new Color(0.4f, 0.8f, 0.4f),    // Green
                "D" => new Color(0.6f, 0.6f, 0.6f),    // Gray
                _ => Color.white
            };
        }

        /// <summary>
        /// 성과 요약 텍스트 생성
        /// Generate performance summary text
        /// </summary>
        public static string GetPerformanceSummary(CombatPerformanceData data)
        {
            string grade = GetPerformanceGrade(data.TotalBonus);
            return $"[{grade}등급 / Grade {grade}]\n" +
                   $"Speed: +{data.speedBonus} ({data.turnsUsed}턴)\n" +
                   $"Impact: +{data.impactBonus} ({data.overkillDamage} 오버킬)\n" +
                   $"Risk: +{data.riskBonus} ({data.incapacitationCount}명 기절)\n" +
                   $"Total: +{data.TotalBonus}";
        }

        #endregion
    }

    /// <summary>
    /// TRIAD: 전투 성과 데이터
    /// Combat performance data
    /// </summary>
    public class CombatPerformanceData
    {
        public int speedBonus;
        public int impactBonus;
        public int riskBonus;
        public int turnsUsed;
        public int overkillDamage;
        public int incapacitationCount;

        public int TotalBonus => speedBonus + impactBonus + riskBonus;
    }
}
