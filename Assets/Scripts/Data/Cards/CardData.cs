using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 카드 데이터 ScriptableObject
    /// Card data ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "Game/Cards/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("기본 정보 (Basic Info)")]
        [Tooltip("고유 ID / Unique ID")]
        public string cardId;

        [Tooltip("카드 이름 / Card name")]
        public string cardName;

        [Tooltip("카드 설명 / Card description")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("카드 아트워크 / Card artwork")]
        public Sprite artwork;

        [Tooltip("카드 타입 / Card type")]
        public CardType cardType;

        [Tooltip("카드 희귀도 / Card rarity")]
        public CardRarity rarity;

        [Header("비용 (Cost)")]
        [Tooltip("에너지 비용 / Energy cost")]
        [Range(0, 5)]
        public int energyCost = 1;

        [Tooltip("사용 후 소멸 여부 / Exhaust on use")]
        public bool exhaustOnUse;

        [Header("효과 (Effects)")]
        [Tooltip("카드 효과 목록 / List of card effects")]
        public List<CardEffect> effects = new List<CardEffect>();

        [Header("업그레이드 (Upgrade)")]
        [Tooltip("업그레이드된 버전 / Upgraded version")]
        public CardData upgradedVersion;

        [Tooltip("업그레이드 여부 / Is upgraded")]
        public bool isUpgraded;

        [Header("TRIAD - 클래스 제한 (Class Restriction)")]
        [Tooltip("사용 가능한 클래스 (비어있으면 모든 클래스 사용 가능) / Allowed classes (empty = all classes)")]
        public List<CharacterClass> allowedClasses = new List<CharacterClass>();

        [Tooltip("Focus 소모량 (0 = Focus 불필요) / Focus cost (0 = no focus required)")]
        [Range(0, 3)]
        public int focusCost = 0;

        [Tooltip("Focus 보너스 효과 (Focus 있을 때 추가 적용) / Focus bonus effects (applied when Focus > 0)")]
        public List<CardEffect> focusBonusEffects = new List<CardEffect>();

        /// <summary>
        /// 카드 타입에 따른 색상 반환
        /// Get color based on card type
        /// </summary>
        public Color GetTypeColor()
        {
            return cardType switch
            {
                CardType.Attack => new Color(0.9f, 0.3f, 0.3f),   // Red
                CardType.Defense => new Color(0.3f, 0.5f, 0.9f), // Blue
                CardType.Skill => new Color(0.3f, 0.8f, 0.4f),   // Green
                _ => Color.white
            };
        }

        /// <summary>
        /// 희귀도에 따른 색상 반환
        /// Get color based on rarity
        /// </summary>
        public Color GetRarityColor()
        {
            return rarity switch
            {
                CardRarity.Starter => Color.gray,
                CardRarity.Common => Color.white,
                CardRarity.Uncommon => new Color(0.3f, 0.7f, 1f),   // Light blue
                CardRarity.Rare => new Color(1f, 0.84f, 0f),        // Gold
                _ => Color.white
            };
        }

        /// <summary>
        /// 전체 설명 텍스트 생성
        /// Generate full description text
        /// </summary>
        public string GetFullDescription()
        {
            if (!string.IsNullOrEmpty(description))
                return description;

            var parts = new List<string>();
            foreach (var effect in effects)
            {
                parts.Add(effect.GetDescription());
            }
            return string.Join(". ", parts);
        }

        /// <summary>
        /// 특정 클래스가 이 카드를 사용할 수 있는지 확인
        /// Check if a specific class can use this card
        /// </summary>
        public bool CanBeUsedBy(CharacterClass characterClass)
        {
            // 클래스 제한이 없으면 모든 클래스 사용 가능
            // No restriction = all classes can use
            if (allowedClasses == null || allowedClasses.Count == 0)
                return true;

            return allowedClasses.Contains(characterClass);
        }

        /// <summary>
        /// 클래스 제한 여부 확인
        /// Check if this card has class restrictions
        /// </summary>
        public bool HasClassRestriction => allowedClasses != null && allowedClasses.Count > 0;

        /// <summary>
        /// Focus 필요 여부 확인
        /// Check if this card requires Focus
        /// </summary>
        public bool RequiresFocus => focusCost > 0;

        /// <summary>
        /// Focus 보너스 여부 확인
        /// Check if this card has Focus bonus effects
        /// </summary>
        public bool HasFocusBonus => focusBonusEffects != null && focusBonusEffects.Count > 0;

        private void OnValidate()
        {
            // 자동으로 ID 생성 (비어있을 경우)
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(cardId))
            {
                cardId = name.Replace(" ", "_").ToLower();
            }
        }
    }
}
