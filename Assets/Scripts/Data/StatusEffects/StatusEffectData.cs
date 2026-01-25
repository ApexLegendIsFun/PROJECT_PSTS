using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 상태이상 데이터 ScriptableObject
    /// Status effect data ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewStatus", menuName = "Game/Status Effects/Status Effect Data")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("기본 정보 (Basic Info)")]
        [Tooltip("고유 ID / Unique ID")]
        public string statusId;

        [Tooltip("상태이상 이름 / Status effect name")]
        public string statusName;

        [Tooltip("상태이상 설명 / Status effect description")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("아이콘 / Icon")]
        public Sprite icon;

        [Header("타입 설정 (Type Settings)")]
        [Tooltip("상태이상 타입 / Status effect type")]
        public StatusEffectType effectType;

        [Tooltip("디버프 여부 / Is debuff")]
        public bool isDebuff;

        [Tooltip("스택 가능 여부 / Is stackable")]
        public bool stackable = true;

        [Tooltip("스택 방식 / Stack behavior")]
        public StackBehavior stackBehavior = StackBehavior.Duration;

        [Header("발동 설정 (Trigger Settings)")]
        [Tooltip("발동 시점 / Trigger timing")]
        public StatusTrigger triggerTime = StatusTrigger.TurnEnd;

        [Header("수치 (Values)")]
        [Tooltip("스택당 효과 수치 (해당되는 경우) / Value per stack")]
        public float valuePerStack = 1f;

        /// <summary>
        /// 상태이상 색상 반환
        /// Get status effect color
        /// </summary>
        public Color GetColor()
        {
            if (isDebuff)
                return new Color(0.9f, 0.4f, 0.4f); // Red for debuff

            return new Color(0.4f, 0.9f, 0.5f); // Green for buff
        }

        /// <summary>
        /// 스택 수에 따른 설명 생성
        /// Generate description based on stack count
        /// </summary>
        public string GetDescription(int stacks)
        {
            return effectType switch
            {
                StatusEffectType.Strength => $"Deals {stacks} additional damage",
                StatusEffectType.Dexterity => $"Gains {stacks} additional Block",
                StatusEffectType.Weak => $"Deals 25% less damage for {stacks} turn(s)",
                StatusEffectType.Vulnerable => $"Takes 50% more damage for {stacks} turn(s)",
                StatusEffectType.Frail => $"Gains 25% less Block for {stacks} turn(s)",
                StatusEffectType.Poison => $"Takes {stacks} damage at turn end, then decreases by 1",
                StatusEffectType.Regeneration => $"Heals {stacks} HP at turn end, then decreases by 1",
                _ => description
            };
        }

        private void OnValidate()
        {
            // 자동으로 ID 생성 (비어있을 경우)
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(statusId))
            {
                statusId = name.Replace(" ", "_").ToLower();
            }
        }
    }
}
