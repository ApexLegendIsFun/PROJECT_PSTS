using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 캐릭터 클래스 데이터 ScriptableObject
    /// Character class data ScriptableObject (Warrior, Mage, Rogue)
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterClass", menuName = "Game/Characters/Character Class Data")]
    public class CharacterClassData : ScriptableObject
    {
        [Header("기본 정보 (Basic Info)")]
        [Tooltip("고유 ID / Unique ID")]
        public string classId;

        [Tooltip("클래스 이름 / Class name")]
        public string className;

        [Tooltip("클래스 타입 / Class type")]
        public CharacterClass classType;

        [Tooltip("클래스 색상 / Class color")]
        public Color classColor = Color.white;

        [Header("기본 스탯 (Base Stats)")]
        [Tooltip("기본 최대 체력 / Base max HP")]
        [Range(50, 100)]
        public int baseMaxHP = 75;

        [Header("태그인 보너스 (Tag-In Bonus)")]
        [Tooltip("태그인 보너스 타입 / Tag-in bonus type")]
        public TagInBonusType tagInBonusType;

        [Tooltip("태그인 보너스 수치 / Tag-in bonus value")]
        public int tagInBonusValue = 5;

        [Tooltip("태그인 시 부여할 상태이상 (ApplyDebuff 타입일 경우) / Status to apply on tag-in")]
        public StatusEffectData tagInStatusEffect;

        [Tooltip("태그인 상태이상 스택 수 / Tag-in status stacks")]
        public int tagInStatusStacks = 1;

        [Header("스타터 덱 (Starter Deck)")]
        [Tooltip("스타터 카드 목록 / Starter card list")]
        public List<CardData> starterDeck = new List<CardData>();

        [Header("비주얼 (Visuals)")]
        [Tooltip("초상화 / Portrait")]
        public Sprite portrait;

        [Tooltip("전투 스프라이트 / Combat sprite")]
        public Sprite combatSprite;

        [TextArea(2, 4)]
        [Tooltip("클래스 설명 / Class description")]
        public string classDescription;

        /// <summary>
        /// 클래스 타입에 따른 기본 색상 반환
        /// Get default color based on class type
        /// </summary>
        public Color GetDefaultClassColor()
        {
            return classType switch
            {
                CharacterClass.Warrior => new Color(0.9f, 0.4f, 0.3f),   // Red-orange
                CharacterClass.Mage => new Color(0.4f, 0.5f, 0.9f),     // Blue-purple
                CharacterClass.Rogue => new Color(0.4f, 0.8f, 0.4f),    // Green
                _ => Color.white
            };
        }

        /// <summary>
        /// 태그인 보너스 설명 생성
        /// Generate tag-in bonus description
        /// </summary>
        public string GetTagInBonusDescription()
        {
            return tagInBonusType switch
            {
                TagInBonusType.GainBlock => $"방어도 {tagInBonusValue} 획득 / Gain {tagInBonusValue} Block",
                TagInBonusType.ApplyDebuff => tagInStatusEffect != null
                    ? $"적에게 {tagInStatusEffect.statusName} {tagInStatusStacks} 부여 / Apply {tagInStatusStacks} {tagInStatusEffect.statusName} to enemy"
                    : "디버프 부여 / Apply debuff",
                TagInBonusType.DrawCard => $"카드 {tagInBonusValue}장 드로우 / Draw {tagInBonusValue} card(s)",
                TagInBonusType.GainEnergy => $"에너지 {tagInBonusValue} 획득 / Gain {tagInBonusValue} Energy",
                TagInBonusType.Heal => $"체력 {tagInBonusValue} 회복 / Heal {tagInBonusValue} HP",
                _ => "보너스 없음 / No bonus"
            };
        }

        private void OnValidate()
        {
            // 자동으로 ID 생성 (비어있을 경우)
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(classId))
            {
                classId = classType.ToString().ToLower();
            }

            // 클래스 색상이 기본값이면 자동 설정
            // Auto-set class color if default
            if (classColor == Color.white)
            {
                classColor = GetDefaultClassColor();
            }
        }
    }
}
