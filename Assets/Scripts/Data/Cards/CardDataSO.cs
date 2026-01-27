// Data/Cards/CardDataSO.cs
// 카드 데이터 ScriptableObject

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data.Cards
{
    /// <summary>
    /// 카드 데이터 정의
    /// 에디터에서 카드의 모든 속성을 설정
    /// </summary>
    [CreateAssetMenu(fileName = "NewCard", menuName = "PSTS/Card Data")]
    public class CardDataSO : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("카드 고유 ID")]
        [SerializeField] private string _cardId;

        [Tooltip("카드 이름")]
        [SerializeField] private string _cardName;

        [Tooltip("카드 설명")]
        [TextArea(2, 4)]
        [SerializeField] private string _description;

        [Tooltip("업그레이드 후 설명")]
        [TextArea(2, 4)]
        [SerializeField] private string _upgradeDescription;

        [Header("Classification")]
        [Tooltip("카드 타입")]
        [SerializeField] private CardType _cardType = CardType.Attack;

        [Tooltip("카드 희귀도")]
        [SerializeField] private CardRarity _rarity = CardRarity.Common;

        [Tooltip("타겟 타입")]
        [SerializeField] private TargetType _targetType = TargetType.SingleEnemy;

        [Header("Cost")]
        [Tooltip("기본 에너지 비용")]
        [SerializeField] private int _energyCost = 1;

        [Tooltip("업그레이드 후 에너지 비용")]
        [SerializeField] private int _upgradedEnergyCost = 1;

        [Header("Effects")]
        [Tooltip("기본 효과 목록")]
        [SerializeField] private List<CardEffectSO> _effects = new List<CardEffectSO>();

        [Tooltip("업그레이드 후 효과 목록 (비어있으면 기본 효과 사용)")]
        [SerializeField] private List<CardEffectSO> _upgradedEffects = new List<CardEffectSO>();

        [Header("Visual")]
        [Tooltip("카드 아트")]
        [SerializeField] private Sprite _cardArt;

        #region Properties

        /// <summary>
        /// 카드 고유 ID
        /// </summary>
        public string CardId => _cardId;

        /// <summary>
        /// 카드 이름
        /// </summary>
        public string CardName => _cardName;

        /// <summary>
        /// 카드 설명
        /// </summary>
        public string Description => _description;

        /// <summary>
        /// 업그레이드 후 설명
        /// </summary>
        public string UpgradeDescription => _upgradeDescription;

        /// <summary>
        /// 카드 타입
        /// </summary>
        public CardType CardType => _cardType;

        /// <summary>
        /// 카드 희귀도
        /// </summary>
        public CardRarity Rarity => _rarity;

        /// <summary>
        /// 타겟 타입
        /// </summary>
        public TargetType TargetType => _targetType;

        /// <summary>
        /// 기본 에너지 비용
        /// </summary>
        public int EnergyCost => _energyCost;

        /// <summary>
        /// 업그레이드 후 에너지 비용
        /// </summary>
        public int UpgradedEnergyCost => _upgradedEnergyCost;

        /// <summary>
        /// 기본 효과 목록
        /// </summary>
        public IReadOnlyList<CardEffectSO> Effects => _effects;

        /// <summary>
        /// 업그레이드 후 효과 목록
        /// </summary>
        public IReadOnlyList<CardEffectSO> UpgradedEffects => _upgradedEffects;

        /// <summary>
        /// 카드 아트
        /// </summary>
        public Sprite CardArt => _cardArt;

        #endregion

        #region Methods

        /// <summary>
        /// 현재 상태에 따른 에너지 비용 반환
        /// </summary>
        public int GetEnergyCost(bool isUpgraded)
        {
            return isUpgraded ? _upgradedEnergyCost : _energyCost;
        }

        /// <summary>
        /// 현재 상태에 따른 설명 반환
        /// </summary>
        public string GetDescription(bool isUpgraded)
        {
            if (isUpgraded && !string.IsNullOrEmpty(_upgradeDescription))
            {
                return _upgradeDescription;
            }
            return _description;
        }

        /// <summary>
        /// 현재 상태에 따른 효과 목록 반환
        /// </summary>
        public IReadOnlyList<CardEffectSO> GetEffects(bool isUpgraded)
        {
            if (isUpgraded && _upgradedEffects != null && _upgradedEffects.Count > 0)
            {
                return _upgradedEffects;
            }
            return _effects;
        }

        /// <summary>
        /// 업그레이드 효과가 있는지 확인
        /// </summary>
        public bool HasUpgradedEffects()
        {
            return _upgradedEffects != null && _upgradedEffects.Count > 0;
        }

        #endregion

        #region Editor Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            // ID가 비어있으면 에셋 이름으로 설정
            if (string.IsNullOrEmpty(_cardId))
            {
                _cardId = name.ToLower().Replace(" ", "_");
            }

            // 이름이 비어있으면 에셋 이름으로 설정
            if (string.IsNullOrEmpty(_cardName))
            {
                _cardName = name;
            }
        }
#endif

        #endregion
    }
}
