// Combat/Cards/CardInstance.cs
// 카드 인스턴스 - 런타임 카드 상태

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data.Cards;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 카드 인스턴스
    /// CardDataSO를 참조하고 런타임 상태만 관리
    /// </summary>
    [System.Serializable]
    public class CardInstance
    {
        [Header("Data Reference")]
        [SerializeField] private CardDataSO _data;

        [Header("Runtime State")]
        [SerializeField] private bool _isUpgraded;
        [SerializeField] private bool _isExhausted;

        /// <summary>
        /// 카드 데이터 SO 참조
        /// </summary>
        public CardDataSO Data => _data;

        /// <summary>
        /// 업그레이드 여부
        /// </summary>
        public bool IsUpgraded => _isUpgraded;

        /// <summary>
        /// 소멸 여부
        /// </summary>
        public bool IsExhausted => _isExhausted;

        #region Delegated Properties (SO에서 읽기)

        /// <summary>
        /// 카드 ID
        /// </summary>
        public string CardId => _data?.CardId ?? string.Empty;

        /// <summary>
        /// 카드 이름 (업그레이드 시 + 추가)
        /// </summary>
        public string CardName => _isUpgraded ? $"{_data?.CardName}+" : _data?.CardName ?? string.Empty;

        /// <summary>
        /// 카드 설명 (업그레이드 상태 반영)
        /// </summary>
        public string Description => _data?.GetDescription(_isUpgraded) ?? string.Empty;

        /// <summary>
        /// 카드 타입
        /// </summary>
        public CardType CardType => _data?.CardType ?? CardType.Attack;

        /// <summary>
        /// 카드 희귀도
        /// </summary>
        public CardRarity Rarity => _data?.Rarity ?? CardRarity.Common;

        /// <summary>
        /// 타겟 타입
        /// </summary>
        public TargetType TargetType => _data?.TargetType ?? TargetType.SingleEnemy;

        /// <summary>
        /// 에너지 비용 (업그레이드 상태 반영)
        /// </summary>
        public int EnergyCost => _data?.GetEnergyCost(_isUpgraded) ?? 0;

        /// <summary>
        /// 카드 아트
        /// </summary>
        public Sprite CardArt => _data?.CardArt;

        /// <summary>
        /// 현재 효과 목록 (업그레이드 상태 반영)
        /// </summary>
        public IReadOnlyList<CardEffectSO> Effects => _data?.GetEffects(_isUpgraded);

        #endregion

        #region Legacy Properties (호환성 유지용, 향후 제거)

        /// <summary>
        /// 데미지 (첫 번째 DamageEffect에서 추출)
        /// </summary>
        public int Damage
        {
            get
            {
                if (_data == null) return 0;
                var effects = _data.GetEffects(_isUpgraded);
                foreach (var effect in effects)
                {
                    if (effect is DamageEffectSO damageEffect)
                    {
                        return damageEffect.BaseDamage;
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// 방어막 (첫 번째 BlockEffect에서 추출)
        /// </summary>
        public int Block
        {
            get
            {
                if (_data == null) return 0;
                var effects = _data.GetEffects(_isUpgraded);
                foreach (var effect in effects)
                {
                    if (effect is BlockEffectSO blockEffect)
                    {
                        return blockEffect.BaseBlock;
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// 드로우 수 (첫 번째 DrawEffect에서 추출)
        /// </summary>
        public int DrawAmount
        {
            get
            {
                if (_data == null) return 0;
                var effects = _data.GetEffects(_isUpgraded);
                foreach (var effect in effects)
                {
                    if (effect is DrawEffectSO drawEffect)
                    {
                        return drawEffect.DrawCount;
                    }
                }
                return 0;
            }
        }

        /// <summary>
        /// 회복량 (첫 번째 HealEffect에서 추출)
        /// </summary>
        public int HealAmount
        {
            get
            {
                if (_data == null) return 0;
                var effects = _data.GetEffects(_isUpgraded);
                foreach (var effect in effects)
                {
                    if (effect is HealEffectSO healEffect)
                    {
                        return healEffect.HealAmount;
                    }
                }
                return 0;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// SO에서 카드 인스턴스 생성
        /// </summary>
        public CardInstance(CardDataSO data, bool upgraded = false)
        {
            _data = data;
            _isUpgraded = upgraded;
            _isExhausted = false;
        }

        /// <summary>
        /// 기본 생성자 (직렬화용)
        /// </summary>
        public CardInstance()
        {
            _isUpgraded = false;
            _isExhausted = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 카드 업그레이드
        /// </summary>
        public void Upgrade()
        {
            if (_isUpgraded) return;

            _isUpgraded = true;
            Debug.Log($"[Card] Upgraded: {CardName}");
        }

        /// <summary>
        /// 소멸 처리
        /// </summary>
        public void Exhaust()
        {
            _isExhausted = true;
            Debug.Log($"[Card] Exhausted: {CardName}");
        }

        /// <summary>
        /// 복제 (같은 SO 참조, 새로운 런타임 상태)
        /// </summary>
        public CardInstance Clone()
        {
            return new CardInstance(_data, _isUpgraded);
        }

        /// <summary>
        /// 유효성 검사
        /// </summary>
        public bool IsValid()
        {
            return _data != null;
        }

        public override string ToString()
        {
            return $"{CardName} ({EnergyCost} Energy) - {CardType}";
        }

        #endregion

        #region Static Factory Methods (호환성 유지용, 향후 제거 예정)

        /// <summary>
        /// [Deprecated] 테스트용 공격 카드 생성
        /// 실제 사용 시 CardDataSO 에셋 사용 권장
        /// </summary>
        [System.Obsolete("Use CardDataSO assets instead")]
        public static CardInstance CreateAttack(string id, string name, int cost, int damage, TargetType targetType = TargetType.SingleEnemy)
        {
            Debug.LogWarning($"[CardInstance] CreateAttack is deprecated. Use CardDataSO assets instead. Card: {name}");
            // 임시로 null data 반환, 실제로는 SO 사용해야 함
            return new CardInstance(null, false);
        }

        /// <summary>
        /// [Deprecated] 테스트용 방어 카드 생성
        /// </summary>
        [System.Obsolete("Use CardDataSO assets instead")]
        public static CardInstance CreateDefend(string id, string name, int cost, int block)
        {
            Debug.LogWarning($"[CardInstance] CreateDefend is deprecated. Use CardDataSO assets instead. Card: {name}");
            return new CardInstance(null, false);
        }

        /// <summary>
        /// [Deprecated] 테스트용 드로우 카드 생성
        /// </summary>
        [System.Obsolete("Use CardDataSO assets instead")]
        public static CardInstance CreateDraw(string id, string name, int cost, int drawAmount)
        {
            Debug.LogWarning($"[CardInstance] CreateDraw is deprecated. Use CardDataSO assets instead. Card: {name}");
            return new CardInstance(null, false);
        }

        #endregion
    }
}
