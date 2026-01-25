using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 유물 데이터 ScriptableObject
    /// Relic data ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewRelic", menuName = "Game/Relics/Relic Data")]
    public class RelicData : ScriptableObject
    {
        [Header("기본 정보 (Basic Info)")]
        [Tooltip("고유 ID / Unique ID")]
        public string relicId;

        [Tooltip("유물 이름 / Relic name")]
        public string relicName;

        [Tooltip("유물 설명 / Relic description")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("유물 아이콘 / Relic icon")]
        public Sprite icon;

        [Tooltip("유물 희귀도 / Relic rarity")]
        public RelicRarity rarity;

        [Header("효과 설정 (Effect Settings)")]
        [Tooltip("발동 시점 / Trigger timing")]
        public RelicTrigger triggerType;

        [Tooltip("유물 효과 목록 / List of relic effects")]
        public List<RelicEffect> effects = new List<RelicEffect>();

        [Header("조건 (Conditions)")]
        [Tooltip("발동 조건 / Trigger condition")]
        public string conditionDescription;

        /// <summary>
        /// 희귀도에 따른 색상 반환
        /// Get color based on rarity
        /// </summary>
        public Color GetRarityColor()
        {
            return rarity switch
            {
                RelicRarity.Starter => Color.gray,
                RelicRarity.Common => Color.white,
                RelicRarity.Uncommon => new Color(0.3f, 0.7f, 1f),   // Light blue
                RelicRarity.Rare => new Color(1f, 0.84f, 0f),        // Gold
                RelicRarity.Boss => new Color(0.9f, 0.3f, 0.3f),     // Red
                RelicRarity.Event => new Color(0.6f, 0.4f, 0.8f),    // Purple
                _ => Color.white
            };
        }

        private void OnValidate()
        {
            // 자동으로 ID 생성 (비어있을 경우)
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(relicId))
            {
                relicId = name.Replace(" ", "_").ToLower();
            }
        }
    }

    /// <summary>
    /// 유물 효과 정의
    /// Relic effect definition
    /// </summary>
    [System.Serializable]
    public class RelicEffect
    {
        [Tooltip("효과 타입 / Effect type")]
        public RelicEffectType effectType;

        [Tooltip("효과 수치 / Effect value")]
        public int value;

        [Tooltip("적용할 상태이상 / Status effect to apply")]
        public StatusEffectData statusEffect;
    }

    /// <summary>
    /// 유물 효과 타입
    /// Relic effect type
    /// </summary>
    public enum RelicEffectType
    {
        GainEnergy,         // 에너지 획득
        DrawCards,          // 카드 드로우
        GainBlock,          // 블록 획득
        GainStrength,       // 힘 획득
        GainDexterity,      // 민첩 획득
        GainGold,           // 골드 획득
        HealHP,             // 체력 회복
        GainMaxHP,          // 최대 체력 증가
        ApplyStatus,        // 상태이상 적용
        ModifyDamage,       // 데미지 증감
        ModifyBlock         // 블록 증감
    }
}
