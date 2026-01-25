using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 상점 설정 ScriptableObject
    /// Shop configuration ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ShopConfig", menuName = "Game/Shop/Shop Config")]
    public class ShopConfig : ScriptableObject
    {
        [Header("인벤토리 설정 (Inventory Settings)")]
        [Tooltip("판매 카드 슬롯 수 / Number of card slots")]
        public int cardSlotCount = 5;

        [Tooltip("판매 유물 슬롯 수 / Number of relic slots")]
        public int relicSlotCount = 3;

        [Header("카드 희귀도 가중치 (Card Rarity Weights)")]
        [Range(0f, 1f)]
        [Tooltip("Common 카드 확률 / Common card probability")]
        public float commonWeight = 0.55f;

        [Range(0f, 1f)]
        [Tooltip("Uncommon 카드 확률 / Uncommon card probability")]
        public float uncommonWeight = 0.37f;

        [Range(0f, 1f)]
        [Tooltip("Rare 카드 확률 / Rare card probability")]
        public float rareWeight = 0.08f;

        [Header("카드 가격 (Card Pricing)")]
        [Tooltip("Common 카드 기본 가격 / Common card base price")]
        public int commonCardBasePrice = 50;

        [Tooltip("Uncommon 카드 기본 가격 / Uncommon card base price")]
        public int uncommonCardBasePrice = 75;

        [Tooltip("Rare 카드 기본 가격 / Rare card base price")]
        public int rareCardBasePrice = 150;

        [Header("유물 가격 (Relic Pricing)")]
        [Tooltip("Common 유물 가격 / Common relic price")]
        public int commonRelicPrice = 150;

        [Tooltip("Uncommon 유물 가격 / Uncommon relic price")]
        public int uncommonRelicPrice = 250;

        [Tooltip("Rare 유물 가격 / Rare relic price")]
        public int rareRelicPrice = 300;

        [Header("서비스 가격 (Service Pricing)")]
        [Tooltip("카드 제거 기본 가격 / Base card removal price")]
        public int baseRemoveCost = 50;

        [Tooltip("카드 제거 가격 증가량 / Card removal price increment")]
        public int removeIncrementCost = 25;

        [Tooltip("카드 업그레이드 가격 / Card upgrade price")]
        public int upgradeCost = 75;

        [Tooltip("HP당 치료 비용 / Heal cost per HP")]
        public int healCostPerHP = 3;

        [Header("가격 변동 (Price Variation)")]
        [Range(0f, 0.5f)]
        [Tooltip("가격 변동 범위 (0.2 = ±20%) / Price variation range")]
        public float priceVariation = 0.15f;

        [Header("할인 설정 (Discount Settings)")]
        [Tooltip("할인 카드 수 / Number of discounted cards")]
        public int discountCardCount = 1;

        [Range(0f, 1f)]
        [Tooltip("할인율 / Discount rate")]
        public float discountRate = 0.5f;

        /// <summary>
        /// 카드 가격 계산
        /// Calculate card price
        /// </summary>
        public int GetCardPrice(CardRarity rarity, bool isDiscounted = false)
        {
            int basePrice = rarity switch
            {
                CardRarity.Common => commonCardBasePrice,
                CardRarity.Uncommon => uncommonCardBasePrice,
                CardRarity.Rare => rareCardBasePrice,
                _ => commonCardBasePrice
            };

            // 가격 변동 적용
            float variation = Random.Range(-priceVariation, priceVariation);
            int price = Mathf.RoundToInt(basePrice * (1f + variation));

            // 할인 적용
            if (isDiscounted)
            {
                price = Mathf.RoundToInt(price * (1f - discountRate));
            }

            return Mathf.Max(price, 1);
        }

        /// <summary>
        /// 유물 가격 계산
        /// Calculate relic price
        /// </summary>
        public int GetRelicPrice(RelicRarity rarity)
        {
            int basePrice = rarity switch
            {
                RelicRarity.Common => commonRelicPrice,
                RelicRarity.Uncommon => uncommonRelicPrice,
                RelicRarity.Rare => rareRelicPrice,
                _ => commonRelicPrice
            };

            float variation = Random.Range(-priceVariation, priceVariation);
            return Mathf.Max(Mathf.RoundToInt(basePrice * (1f + variation)), 1);
        }

        /// <summary>
        /// 카드 제거 가격 계산 (제거 횟수에 따라 증가)
        /// Calculate card removal price (increases with removal count)
        /// </summary>
        public int GetRemoveCost(int previousRemovals)
        {
            return baseRemoveCost + (removeIncrementCost * previousRemovals);
        }

        /// <summary>
        /// 파티 치료 비용 계산
        /// Calculate party heal cost
        /// </summary>
        public int GetHealCost(int missingHP)
        {
            return healCostPerHP * missingHP;
        }

        /// <summary>
        /// 희귀도 가중치로 희귀도 선택
        /// Select rarity based on weights
        /// </summary>
        public CardRarity RollCardRarity()
        {
            float roll = Random.Range(0f, 1f);
            float normalizedTotal = commonWeight + uncommonWeight + rareWeight;

            float common = commonWeight / normalizedTotal;
            float uncommon = uncommonWeight / normalizedTotal;

            if (roll < common)
                return CardRarity.Common;
            else if (roll < common + uncommon)
                return CardRarity.Uncommon;
            else
                return CardRarity.Rare;
        }

        private void OnValidate()
        {
            // 가중치 합이 1이 되도록 조정하지는 않음 (정규화됨)
            cardSlotCount = Mathf.Max(1, cardSlotCount);
            relicSlotCount = Mathf.Max(0, relicSlotCount);
            commonCardBasePrice = Mathf.Max(1, commonCardBasePrice);
            uncommonCardBasePrice = Mathf.Max(1, uncommonCardBasePrice);
            rareCardBasePrice = Mathf.Max(1, rareCardBasePrice);
            baseRemoveCost = Mathf.Max(0, baseRemoveCost);
            upgradeCost = Mathf.Max(1, upgradeCost);
            healCostPerHP = Mathf.Max(1, healCostPerHP);
        }
    }
}
