using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Run;

namespace ProjectSS.Shop
{
    /// <summary>
    /// 상점 관리자
    /// Shop manager
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private ShopConfig config;

        [Header("Card Database")]
        [SerializeField] private CardData[] allCards;

        [Header("Relic Database")]
        [SerializeField] private RelicData[] allRelics;

        // 현재 상점 인벤토리
        private List<ShopItem> shopCards = new List<ShopItem>();
        private List<ShopItem> shopRelics = new List<ShopItem>();

        // 제거 횟수 (가격 증가용)
        private int cardRemovalCount = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadDatabases();
            LoadConfig();
        }

        /// <summary>
        /// 데이터베이스 로드
        /// Load databases
        /// </summary>
        private void LoadDatabases()
        {
            if (allCards == null || allCards.Length == 0)
            {
                allCards = Resources.LoadAll<CardData>("Data/Cards");
            }

            if (allRelics == null || allRelics.Length == 0)
            {
                allRelics = Resources.LoadAll<RelicData>("Data/Relics");
            }

            Debug.Log($"[ShopManager] Loaded {allCards?.Length ?? 0} cards, {allRelics?.Length ?? 0} relics");
        }

        /// <summary>
        /// 설정 로드
        /// Load config
        /// </summary>
        private void LoadConfig()
        {
            if (config == null)
            {
                config = Resources.Load<ShopConfig>("Data/Shop/ShopConfig");
            }

            if (config == null)
            {
                Debug.LogWarning("[ShopManager] ShopConfig not found, using defaults");
            }
        }

        /// <summary>
        /// 상점 인벤토리 생성
        /// Generate shop inventory
        /// </summary>
        public void GenerateShopInventory()
        {
            shopCards.Clear();
            shopRelics.Clear();

            int floor = RunManager.Instance?.CurrentRun?.currentFloor ?? 1;

            // 카드 생성
            GenerateShopCards(floor);

            // 유물 생성
            GenerateShopRelics(floor);

            EventBus.Publish(new ShopEnteredEvent());

            Debug.Log($"[ShopManager] Generated shop: {shopCards.Count} cards, {shopRelics.Count} relics");
        }

        /// <summary>
        /// 상점 카드 생성
        /// Generate shop cards
        /// </summary>
        private void GenerateShopCards(int floor)
        {
            if (allCards == null || allCards.Length == 0) return;

            int cardCount = config?.cardSlotCount ?? 5;
            int discountCount = config?.discountCardCount ?? 1;

            // 스타터 카드 제외
            var eligibleCards = allCards.Where(c => c.rarity != CardRarity.Starter).ToList();

            if (eligibleCards.Count == 0)
                eligibleCards = allCards.ToList();

            // 희귀도별 분류
            var byRarity = eligibleCards.GroupBy(c => c.rarity)
                .ToDictionary(g => g.Key, g => g.ToList());

            HashSet<string> selected = new HashSet<string>();

            for (int i = 0; i < cardCount; i++)
            {
                CardRarity rarity = config?.RollCardRarity() ?? CardRarity.Common;
                CardData card = null;

                // 해당 희귀도에서 선택
                if (byRarity.TryGetValue(rarity, out var cards) && cards.Count > 0)
                {
                    var available = cards.Where(c => !selected.Contains(c.cardId)).ToList();
                    if (available.Count > 0)
                    {
                        card = available[Random.Range(0, available.Count)];
                    }
                }

                // 없으면 아무 카드
                if (card == null)
                {
                    var available = eligibleCards.Where(c => !selected.Contains(c.cardId)).ToList();
                    if (available.Count > 0)
                    {
                        card = available[Random.Range(0, available.Count)];
                    }
                }

                if (card != null)
                {
                    selected.Add(card.cardId);

                    bool isDiscounted = i < discountCount;
                    int price = config?.GetCardPrice(card.rarity, isDiscounted) ?? 50;

                    shopCards.Add(new ShopItem
                    {
                        itemId = card.cardId,
                        displayName = card.cardName,
                        price = price,
                        originalPrice = isDiscounted ? config?.GetCardPrice(card.rarity, false) ?? price : price,
                        isDiscounted = isDiscounted,
                        isSold = false,
                        itemType = ShopItemType.Card,
                        cardData = card
                    });
                }
            }
        }

        /// <summary>
        /// 상점 유물 생성
        /// Generate shop relics
        /// </summary>
        private void GenerateShopRelics(int floor)
        {
            if (allRelics == null || allRelics.Length == 0) return;

            int relicCount = config?.relicSlotCount ?? 3;

            // 이미 보유한 유물 제외
            var ownedRelics = RunManager.Instance?.CurrentRun?.relicIds ?? new List<string>();
            var available = allRelics.Where(r => !ownedRelics.Contains(r.relicId)).ToList();

            if (available.Count == 0) return;

            HashSet<string> selected = new HashSet<string>();

            for (int i = 0; i < relicCount && available.Count > 0; i++)
            {
                var eligibleRelics = available.Where(r => !selected.Contains(r.relicId)).ToList();
                if (eligibleRelics.Count == 0) break;

                var relic = eligibleRelics[Random.Range(0, eligibleRelics.Count)];
                selected.Add(relic.relicId);

                int price = config?.GetRelicPrice(relic.rarity) ?? 200;

                shopRelics.Add(new ShopItem
                {
                    itemId = relic.relicId,
                    displayName = relic.relicName,
                    price = price,
                    originalPrice = price,
                    isDiscounted = false,
                    isSold = false,
                    itemType = ShopItemType.Relic,
                    relicData = relic
                });
            }
        }

        // 인벤토리 접근자
        public List<ShopItem> GetShopCards() => shopCards;
        public List<ShopItem> GetShopRelics() => shopRelics;

        /// <summary>
        /// 아이템 구매
        /// Purchase item
        /// </summary>
        public bool PurchaseItem(ShopItem item)
        {
            if (item == null || item.isSold) return false;

            var run = RunManager.Instance?.CurrentRun;
            if (run == null) return false;

            // 골드 확인
            if (run.gold < item.price) return false;

            // 골드 차감
            run.gold -= item.price;

            // 아이템 추가
            if (item.itemType == ShopItemType.Card && item.cardData != null)
            {
                if (RunManager.Instance.IsPartyMode)
                {
                    RunManager.Instance.PartyState.AddCard(item.cardData, false);
                }
            }
            else if (item.itemType == ShopItemType.Relic && item.relicData != null)
            {
                run.relicIds.Add(item.relicData.relicId);
            }

            // 판매 완료 표시
            item.isSold = true;

            // 이벤트 발행
            EventBus.Publish(new ItemPurchasedEvent(item.itemId, item.price, item.itemType == ShopItemType.Card));
            EventBus.Publish(new GoldChangedEvent(-item.price, run.gold));

            Debug.Log($"[ShopManager] Purchased: {item.displayName} for {item.price}G");
            return true;
        }

        /// <summary>
        /// 카드 제거 서비스
        /// Card removal service
        /// </summary>
        public int GetRemoveCost()
        {
            return config?.GetRemoveCost(cardRemovalCount) ?? (50 + cardRemovalCount * 25);
        }

        public bool RemoveCard(int cardIndex)
        {
            var run = RunManager.Instance?.CurrentRun;
            if (run == null) return false;

            int cost = GetRemoveCost();
            if (run.gold < cost) return false;

            if (!RunManager.Instance.IsPartyMode) return false;

            // 카드 제거
            bool removed = RunManager.Instance.PartyState.RemoveCard(cardIndex);
            if (!removed) return false;

            // 골드 차감
            run.gold -= cost;
            cardRemovalCount++;

            EventBus.Publish(new ShopServiceUsedEvent("remove"));
            EventBus.Publish(new GoldChangedEvent(-cost, run.gold));

            Debug.Log($"[ShopManager] Card removed for {cost}G");
            return true;
        }

        /// <summary>
        /// 카드 업그레이드 서비스
        /// Card upgrade service
        /// </summary>
        public int GetUpgradeCost()
        {
            return config?.upgradeCost ?? 75;
        }

        public bool UpgradeCard(int cardIndex)
        {
            var run = RunManager.Instance?.CurrentRun;
            if (run == null) return false;

            int cost = GetUpgradeCost();
            if (run.gold < cost) return false;

            if (!RunManager.Instance.IsPartyMode) return false;

            var party = RunManager.Instance.PartyState;

            // 이미 업그레이드 됐는지 확인
            if (cardIndex < 0 || cardIndex >= party.deckUpgraded.Count) return false;
            if (party.deckUpgraded[cardIndex]) return false;

            // 업그레이드 적용
            party.deckUpgraded[cardIndex] = true;

            // 골드 차감
            run.gold -= cost;

            EventBus.Publish(new ShopServiceUsedEvent("upgrade"));
            EventBus.Publish(new GoldChangedEvent(-cost, run.gold));

            Debug.Log($"[ShopManager] Card upgraded for {cost}G");
            return true;
        }

        /// <summary>
        /// 파티 치료 서비스
        /// Party heal service
        /// </summary>
        public int GetHealCost()
        {
            if (!RunManager.Instance.IsPartyMode) return 0;

            var party = RunManager.Instance.PartyState;
            int missingHP = (party.warrior.maxHP - party.warrior.currentHP)
                          + (party.mage.maxHP - party.mage.currentHP)
                          + (party.rogue.maxHP - party.rogue.currentHP);

            return config?.GetHealCost(missingHP) ?? (missingHP * 3);
        }

        public bool HealParty()
        {
            var run = RunManager.Instance?.CurrentRun;
            if (run == null) return false;

            if (!RunManager.Instance.IsPartyMode) return false;

            var party = RunManager.Instance.PartyState;

            int cost = GetHealCost();
            if (cost == 0) return false; // 이미 만피
            if (run.gold < cost) return false;

            // 치료 적용
            party.warrior.currentHP = party.warrior.maxHP;
            party.mage.currentHP = party.mage.maxHP;
            party.rogue.currentHP = party.rogue.maxHP;

            // 골드 차감
            run.gold -= cost;

            EventBus.Publish(new ShopServiceUsedEvent("heal"));
            EventBus.Publish(new GoldChangedEvent(-cost, run.gold));

            Debug.Log($"[ShopManager] Party healed for {cost}G");
            return true;
        }

        /// <summary>
        /// 플레이어 골드 확인
        /// Check player gold
        /// </summary>
        public int GetPlayerGold()
        {
            return RunManager.Instance?.CurrentRun?.gold ?? 0;
        }

        /// <summary>
        /// 구매 가능 여부 확인
        /// Check if can afford
        /// </summary>
        public bool CanAfford(int price)
        {
            return GetPlayerGold() >= price;
        }
    }

    /// <summary>
    /// 상점 아이템 타입
    /// Shop item type
    /// </summary>
    public enum ShopItemType
    {
        Card,
        Relic
    }

    /// <summary>
    /// 상점 아이템
    /// Shop item
    /// </summary>
    [System.Serializable]
    public class ShopItem
    {
        public string itemId;
        public string displayName;
        public int price;
        public int originalPrice;
        public bool isDiscounted;
        public bool isSold;
        public ShopItemType itemType;
        public CardData cardData;
        public RelicData relicData;
    }
}
