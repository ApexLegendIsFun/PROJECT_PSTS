using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 적 데이터 ScriptableObject
    /// Enemy data ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemies/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("기본 정보 (Basic Info)")]
        [Tooltip("고유 ID / Unique ID")]
        public string enemyId;

        [Tooltip("적 이름 / Enemy name")]
        public string enemyName;

        [Tooltip("적 스프라이트 / Enemy sprite")]
        public Sprite sprite;

        [Tooltip("적 타입 / Enemy type")]
        public EnemyType enemyType;

        [Header("스탯 (Stats)")]
        [Tooltip("최소 체력 / Minimum health")]
        public int minHealth = 10;

        [Tooltip("최대 체력 / Maximum health")]
        public int maxHealth = 15;

        [Header("AI 패턴 (AI Pattern)")]
        [Tooltip("행동 패턴 / Action pattern")]
        public List<EnemyAction> actionPattern = new List<EnemyAction>();

        [Header("보상 (Rewards)")]
        [Tooltip("골드 보상 / Gold reward")]
        public int goldReward = 10;

        [Tooltip("카드 보상 확률 / Card reward chance")]
        [Range(0f, 1f)]
        public float cardRewardChance = 0.5f;

        [Header("TRIAD - 브레이크 시스템 (Break System)")]
        [Tooltip("브레이크 조건 (null이면 브레이크 불가) / Break condition (null = unbreakable)")]
        public BreakConditionData breakCondition;

        [Tooltip("기절 상태 행동 (기절 시 사용) / Groggy state actions")]
        public List<EnemyAction> groggyActions = new List<EnemyAction>();

        [Header("TRIAD - 타겟팅 (Targeting)")]
        [Tooltip("타겟 선택 전략 / Target selection strategy")]
        public EnemyTargetingStrategy targetingStrategy = EnemyTargetingStrategy.ActiveOnly;

        /// <summary>
        /// 랜덤 체력 계산
        /// Calculate random health
        /// </summary>
        public int GetRandomHealth(System.Random random = null)
        {
            random ??= new System.Random();
            return random.Next(minHealth, maxHealth + 1);
        }

        /// <summary>
        /// 타입에 따른 색상 반환
        /// Get color based on enemy type
        /// </summary>
        public Color GetTypeColor()
        {
            return enemyType switch
            {
                EnemyType.Normal => Color.white,
                EnemyType.Elite => new Color(1f, 0.84f, 0f),    // Gold
                EnemyType.Boss => new Color(0.8f, 0.2f, 0.2f),  // Dark red
                _ => Color.white
            };
        }

        /// <summary>
        /// 브레이크 가능 여부 확인
        /// Check if this enemy can be broken
        /// </summary>
        public bool CanBebroken => breakCondition != null;

        /// <summary>
        /// 기절 상태 행동 여부 확인
        /// Check if this enemy has groggy state actions
        /// </summary>
        public bool HasGroggyActions => groggyActions != null && groggyActions.Count > 0;

        private void OnValidate()
        {
            // 자동으로 ID 생성 (비어있을 경우)
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(enemyId))
            {
                enemyId = name.Replace(" ", "_").ToLower();
            }

            // 최소 체력이 최대 체력보다 크지 않도록
            // Ensure min health is not greater than max health
            if (minHealth > maxHealth)
            {
                minHealth = maxHealth;
            }
        }
    }

    /// <summary>
    /// 적 행동 정의
    /// Enemy action definition
    /// </summary>
    [System.Serializable]
    public class EnemyAction
    {
        [Tooltip("인텐트 타입 / Intent type")]
        public EnemyIntentType intentType;

        [Tooltip("행동 이름 / Action name")]
        public string actionName;

        [Tooltip("데미지 (공격 시) / Damage amount")]
        public int damage;

        [Tooltip("블록 (방어 시) / Block amount")]
        public int block;

        [Tooltip("적용할 상태이상 / Status effect to apply")]
        public StatusEffectData statusEffect;

        [Tooltip("상태이상 스택 수 / Status effect stacks")]
        public int statusStacks = 1;

        [Tooltip("히트 횟수 (다중 공격 시) / Number of hits")]
        [Min(1)]
        public int hitCount = 1;

        /// <summary>
        /// 인텐트 설명 생성
        /// Generate intent description
        /// </summary>
        public string GetIntentDescription()
        {
            var parts = new List<string>();

            if (damage > 0)
            {
                string dmgText = hitCount > 1 ? $"{damage}x{hitCount}" : damage.ToString();
                parts.Add($"Attack for {dmgText}");
            }

            if (block > 0)
            {
                parts.Add($"Block for {block}");
            }

            if (statusEffect != null)
            {
                parts.Add($"Apply {statusStacks} {statusEffect.statusName}");
            }

            return parts.Count > 0 ? string.Join(", ", parts) : actionName;
        }
    }
}
