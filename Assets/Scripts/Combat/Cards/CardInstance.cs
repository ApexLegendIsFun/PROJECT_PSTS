using System;
using ProjectSS.Data;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 런타임 카드 인스턴스
    /// Runtime card instance
    /// </summary>
    public class CardInstance
    {
        public string InstanceId { get; private set; }
        public CardData Data { get; private set; }
        public bool IsUpgraded { get; private set; }

        /// <summary>
        /// 실제 사용할 데이터 (업그레이드 여부에 따라)
        /// Actual data to use (based on upgrade status)
        /// </summary>
        public CardData EffectiveData => IsUpgraded && Data.upgradedVersion != null
            ? Data.upgradedVersion
            : Data;

        public CardInstance(CardData data, bool upgraded = false)
        {
            InstanceId = Guid.NewGuid().ToString();
            Data = data;
            IsUpgraded = upgraded;
        }

        /// <summary>
        /// 카드 업그레이드
        /// Upgrade the card
        /// </summary>
        public void Upgrade()
        {
            if (!IsUpgraded && Data.upgradedVersion != null)
            {
                IsUpgraded = true;
            }
        }

        /// <summary>
        /// 카드 사용 가능 여부
        /// Check if card can be played
        /// </summary>
        public bool CanPlay(EnergySystem energy, ICombatEntity target = null)
        {
            // 에너지 체크
            if (!energy.CanSpend(EffectiveData.energyCost))
                return false;

            // 타겟 필요 여부 체크
            bool needsTarget = false;
            foreach (var effect in EffectiveData.effects)
            {
                if (effect.targetType == TargetType.SingleEnemy)
                {
                    needsTarget = true;
                    break;
                }
            }

            if (needsTarget && target == null)
                return false;

            return true;
        }

        /// <summary>
        /// 복제본 생성
        /// Create a copy
        /// </summary>
        public CardInstance Clone()
        {
            return new CardInstance(Data, IsUpgraded);
        }
    }
}
