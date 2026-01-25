using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Run;

namespace ProjectSS.Reward
{
    /// <summary>
    /// 보상 관리자
    /// Reward manager
    /// </summary>
    public class RewardManager : MonoBehaviour
    {
        public static RewardManager Instance { get; private set; }

        [Header("Card Database")]
        [SerializeField] private CardData[] allCards;

        [Header("Relic Database")]
        [SerializeField] private RelicData[] eliteRelics;
        [SerializeField] private RelicData[] bossRelics;
        [SerializeField] private RelicData[] allRelics;

        [Header("Reward Settings")]
        [SerializeField] private int baseGoldRewardNormal = 15;
        [SerializeField] private int baseGoldRewardElite = 30;
        [SerializeField] private int baseGoldRewardBoss = 100;
        [SerializeField] private int goldVariation = 5;

        [Header("Card Reward Settings")]
        [SerializeField] private int cardChoiceCount = 3;
        [SerializeField] private float commonWeight = 0.55f;
        [SerializeField] private float uncommonWeight = 0.37f;
        [SerializeField] private float rareWeight = 0.08f;

        // 캐시된 보상 데이터
        private RewardType currentRewardType;
        private int cachedGoldReward;
        private List<CardData> cachedCardChoices;
        private RelicData cachedRelicReward;
        private CombatPerformanceEvent? cachedPerformance;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // 카드/유물 데이터 로드
            LoadDatabases();
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

            Debug.Log($"[RewardManager] Loaded {allCards?.Length ?? 0} cards, {allRelics?.Length ?? 0} relics");
        }

        /// <summary>
        /// 보상 초기화
        /// Initialize rewards
        /// </summary>
        public void InitializeRewards(RewardType type, CombatPerformanceEvent? performance = null)
        {
            currentRewardType = type;
            cachedPerformance = performance;

            // 보상 생성
            cachedGoldReward = CalculateGoldReward(type, performance);
            cachedCardChoices = GenerateCardChoices(cardChoiceCount);

            if (type == RewardType.Elite || type == RewardType.Boss)
            {
                cachedRelicReward = GenerateRelicReward(type == RewardType.Boss);
            }
            else
            {
                cachedRelicReward = null;
            }

            Debug.Log($"[RewardManager] Rewards initialized - Type: {type}, Gold: {cachedGoldReward}, Cards: {cachedCardChoices.Count}");
        }

        /// <summary>
        /// 금화 보상 계산
        /// Calculate gold reward
        /// </summary>
        public int CalculateGoldReward(RewardType type, CombatPerformanceEvent? performance = null)
        {
            int baseGold = type switch
            {
                RewardType.Combat => baseGoldRewardNormal,
                RewardType.Elite => baseGoldRewardElite,
                RewardType.Boss => baseGoldRewardBoss,
                RewardType.Treasure => baseGoldRewardNormal * 2,
                _ => baseGoldRewardNormal
            };

            // 층 보너스 (층당 +5%)
            int floor = RunManager.Instance?.CurrentRun?.currentFloor ?? 1;
            float floorBonus = 1f + (floor * 0.05f);

            // 변동
            int variation = Random.Range(-goldVariation, goldVariation + 1);

            int totalGold = Mathf.RoundToInt(baseGold * floorBonus) + variation;

            // TRIAD 성과 보너스
            if (performance.HasValue && performance.Value.TotalBonus > 0)
            {
                int perfBonus = performance.Value.TotalBonus / 10;
                totalGold += perfBonus;
            }

            return Mathf.Max(totalGold, 1);
        }

        /// <summary>
        /// 카드 선택지 생성
        /// Generate card choices
        /// </summary>
        public List<CardData> GenerateCardChoices(int count = 3)
        {
            var choices = new List<CardData>();

            if (allCards == null || allCards.Length == 0)
            {
                Debug.LogWarning("[RewardManager] No cards available for rewards");
                return choices;
            }

            // 스타터 카드 제외
            var eligibleCards = allCards
                .Where(c => c.rarity != CardRarity.Starter)
                .ToList();

            if (eligibleCards.Count == 0)
            {
                eligibleCards = allCards.ToList();
            }

            // 희귀도별 카드 분류
            var byRarity = eligibleCards.GroupBy(c => c.rarity)
                .ToDictionary(g => g.Key, g => g.ToList());

            for (int i = 0; i < count; i++)
            {
                CardRarity rarity = RollRarity();
                CardData selected = null;

                // 해당 희귀도에서 선택
                if (byRarity.TryGetValue(rarity, out var cards) && cards.Count > 0)
                {
                    // 이미 선택된 카드 제외
                    var available = cards.Where(c => !choices.Contains(c)).ToList();
                    if (available.Count > 0)
                    {
                        selected = available[Random.Range(0, available.Count)];
                    }
                }

                // 해당 희귀도 없으면 다른 카드
                if (selected == null)
                {
                    var available = eligibleCards.Where(c => !choices.Contains(c)).ToList();
                    if (available.Count > 0)
                    {
                        selected = available[Random.Range(0, available.Count)];
                    }
                }

                if (selected != null)
                {
                    choices.Add(selected);
                }
            }

            return choices;
        }

        /// <summary>
        /// 희귀도 롤
        /// Roll rarity
        /// </summary>
        private CardRarity RollRarity()
        {
            float roll = Random.Range(0f, 1f);
            float total = commonWeight + uncommonWeight + rareWeight;

            float common = commonWeight / total;
            float uncommon = uncommonWeight / total;

            if (roll < common)
                return CardRarity.Common;
            else if (roll < common + uncommon)
                return CardRarity.Uncommon;
            else
                return CardRarity.Rare;
        }

        /// <summary>
        /// 유물 보상 생성
        /// Generate relic reward
        /// </summary>
        public RelicData GenerateRelicReward(bool isBoss)
        {
            RelicData[] pool = isBoss ? bossRelics : eliteRelics;

            // 풀이 비어있으면 전체 풀 사용
            if (pool == null || pool.Length == 0)
            {
                pool = allRelics;
            }

            if (pool == null || pool.Length == 0)
            {
                Debug.LogWarning("[RewardManager] No relics available for rewards");
                return null;
            }

            // 이미 보유한 유물 제외
            var ownedRelics = RunManager.Instance?.CurrentRun?.relicIds ?? new List<string>();
            var available = pool.Where(r => !ownedRelics.Contains(r.relicId)).ToArray();

            if (available.Length == 0)
            {
                available = pool;
            }

            return available[Random.Range(0, available.Length)];
        }

        // 캐시된 데이터 접근자
        public int GetCachedGoldReward() => cachedGoldReward;
        public List<CardData> GetCachedCardChoices() => cachedCardChoices;
        public RelicData GetCachedRelicReward() => cachedRelicReward;
        public CombatPerformanceEvent? GetCachedPerformance() => cachedPerformance;
        public RewardType GetCurrentRewardType() => currentRewardType;

        /// <summary>
        /// 금화 획득 처리
        /// Claim gold
        /// </summary>
        public void ClaimGold()
        {
            if (RunManager.Instance != null && RunManager.Instance.CurrentRun != null)
            {
                RunManager.Instance.CurrentRun.gold += cachedGoldReward;
                EventBus.Publish(new GoldClaimedEvent(cachedGoldReward));
                EventBus.Publish(new GoldChangedEvent(cachedGoldReward, RunManager.Instance.CurrentRun.gold));

                Debug.Log($"[RewardManager] Gold claimed: +{cachedGoldReward}");
            }
        }

        /// <summary>
        /// 카드 획득 처리
        /// Claim card
        /// </summary>
        public void ClaimCard(CardData card)
        {
            if (RunManager.Instance != null && RunManager.Instance.IsPartyMode)
            {
                RunManager.Instance.PartyState.AddCard(card, false);
                EventBus.Publish(new CardRewardClaimedEvent(card.cardId));

                Debug.Log($"[RewardManager] Card claimed: {card.cardName}");
            }
        }

        /// <summary>
        /// 유물 획득 처리
        /// Claim relic
        /// </summary>
        public void ClaimRelic(RelicData relic)
        {
            if (relic == null) return;

            if (RunManager.Instance != null && RunManager.Instance.CurrentRun != null)
            {
                RunManager.Instance.CurrentRun.relicIds.Add(relic.relicId);
                EventBus.Publish(new RelicClaimedEvent(relic.relicId));

                Debug.Log($"[RewardManager] Relic claimed: {relic.relicName}");
            }
        }
    }
}
