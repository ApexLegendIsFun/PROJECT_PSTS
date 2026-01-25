using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Reward;
using ProjectSS.Run;

namespace ProjectSS.UI
{
    /// <summary>
    /// 보상 씬 UI 관리자
    /// Reward scene UI manager
    /// </summary>
    public class RewardUI : MonoBehaviour
    {
        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Gold Reward")]
        [SerializeField] private GameObject goldRewardPanel;
        [SerializeField] private TextMeshProUGUI goldAmountText;
        [SerializeField] private Button claimGoldButton;

        [Header("Card Reward")]
        [SerializeField] private GameObject cardRewardPanel;
        [SerializeField] private Transform cardChoicesContainer;
        [SerializeField] private GameObject cardChoicePrefab;
        [SerializeField] private Button skipCardButton;

        [Header("Relic Reward")]
        [SerializeField] private GameObject relicRewardPanel;
        [SerializeField] private Image relicIcon;
        [SerializeField] private TextMeshProUGUI relicNameText;
        [SerializeField] private TextMeshProUGUI relicDescText;
        [SerializeField] private Button claimRelicButton;

        [Header("Performance Bonus (TRIAD)")]
        [SerializeField] private GameObject performancePanel;
        [SerializeField] private TextMeshProUGUI speedBonusText;
        [SerializeField] private TextMeshProUGUI impactBonusText;
        [SerializeField] private TextMeshProUGUI riskBonusText;
        [SerializeField] private TextMeshProUGUI totalBonusText;

        [Header("Navigation")]
        [SerializeField] private Button proceedButton;
        [SerializeField] private TextMeshProUGUI proceedButtonText;

        // 상태
        private bool goldClaimed = false;
        private bool cardClaimed = false;
        private bool relicClaimed = false;
        private List<GameObject> cardChoiceButtons = new List<GameObject>();

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        private void Initialize()
        {
            // 버튼 이벤트 연결
            if (claimGoldButton != null)
            {
                claimGoldButton.onClick.RemoveAllListeners();
                claimGoldButton.onClick.AddListener(OnClaimGold);
            }

            if (skipCardButton != null)
            {
                skipCardButton.onClick.RemoveAllListeners();
                skipCardButton.onClick.AddListener(OnSkipCard);
            }

            if (claimRelicButton != null)
            {
                claimRelicButton.onClick.RemoveAllListeners();
                claimRelicButton.onClick.AddListener(OnClaimRelic);
            }

            if (proceedButton != null)
            {
                proceedButton.onClick.RemoveAllListeners();
                proceedButton.onClick.AddListener(OnProceed);
                proceedButton.interactable = false;
            }

            // 보상 표시
            DisplayRewards();
        }

        /// <summary>
        /// 보상 표시
        /// Display rewards
        /// </summary>
        private void DisplayRewards()
        {
            if (RewardManager.Instance == null)
            {
                Debug.LogWarning("[RewardUI] RewardManager not found");
                SetupTestRewards();
                return;
            }

            var rewardType = RewardManager.Instance.GetCurrentRewardType();

            // 타이틀 설정
            UpdateTitle(rewardType);

            // 성과 보너스 표시
            DisplayPerformanceBonus();

            // 금화 보상
            DisplayGoldReward();

            // 카드 보상
            DisplayCardReward();

            // 유물 보상 (엘리트/보스만)
            DisplayRelicReward(rewardType);

            // 이벤트 발행
            EventBus.Publish(new RewardScreenOpenedEvent(rewardType));
        }

        /// <summary>
        /// 테스트 보상 설정 (에디터 테스트용)
        /// Setup test rewards for editor testing
        /// </summary>
        private void SetupTestRewards()
        {
#if UNITY_EDITOR
            Debug.Log("[RewardUI] Setting up test rewards for editor");

            if (titleText != null)
                titleText.text = "전투 승리! (Test)";

            // 금화
            if (goldRewardPanel != null)
            {
                goldRewardPanel.SetActive(true);
                if (goldAmountText != null)
                    goldAmountText.text = "+25 G";
            }

            // 카드 (비활성화)
            if (cardRewardPanel != null)
                cardRewardPanel.SetActive(false);

            // 유물 (비활성화)
            if (relicRewardPanel != null)
                relicRewardPanel.SetActive(false);

            // 성과 (비활성화)
            if (performancePanel != null)
                performancePanel.SetActive(false);

            // 진행 버튼 활성화
            if (proceedButton != null)
                proceedButton.interactable = true;
#endif
        }

        /// <summary>
        /// 타이틀 업데이트
        /// Update title
        /// </summary>
        private void UpdateTitle(RewardType type)
        {
            if (titleText == null) return;

            titleText.text = type switch
            {
                RewardType.Combat => "전투 승리!",
                RewardType.Elite => "엘리트 격파!",
                RewardType.Boss => "보스 격파!",
                RewardType.Treasure => "보물 발견!",
                _ => "보상"
            };
        }

        /// <summary>
        /// 성과 보너스 표시
        /// Display performance bonus
        /// </summary>
        private void DisplayPerformanceBonus()
        {
            var perf = RewardManager.Instance?.GetCachedPerformance();

            if (!perf.HasValue || perf.Value.TotalBonus == 0)
            {
                if (performancePanel != null)
                    performancePanel.SetActive(false);
                return;
            }

            if (performancePanel != null)
            {
                performancePanel.SetActive(true);

                if (speedBonusText != null)
                    speedBonusText.text = $"Speed: +{perf.Value.SpeedBonus}";

                if (impactBonusText != null)
                    impactBonusText.text = $"Impact: +{perf.Value.ImpactBonus}";

                if (riskBonusText != null)
                    riskBonusText.text = $"Risk: +{perf.Value.RiskBonus}";

                int bonusGold = perf.Value.TotalBonus / 10;
                if (totalBonusText != null)
                    totalBonusText.text = $"보너스 골드: +{bonusGold}G";
            }
        }

        /// <summary>
        /// 금화 보상 표시
        /// Display gold reward
        /// </summary>
        private void DisplayGoldReward()
        {
            int goldReward = RewardManager.Instance?.GetCachedGoldReward() ?? 0;

            if (goldRewardPanel != null)
            {
                goldRewardPanel.SetActive(true);

                if (goldAmountText != null)
                    goldAmountText.text = $"+{goldReward} G";
            }
        }

        /// <summary>
        /// 카드 보상 표시
        /// Display card reward
        /// </summary>
        private void DisplayCardReward()
        {
            var cardChoices = RewardManager.Instance?.GetCachedCardChoices();

            if (cardChoices == null || cardChoices.Count == 0)
            {
                if (cardRewardPanel != null)
                    cardRewardPanel.SetActive(false);
                return;
            }

            if (cardRewardPanel != null)
            {
                cardRewardPanel.SetActive(true);
                PopulateCardChoices(cardChoices);
            }
        }

        /// <summary>
        /// 카드 선택지 생성
        /// Populate card choices
        /// </summary>
        private void PopulateCardChoices(List<CardData> cards)
        {
            // 기존 버튼 제거
            foreach (var btn in cardChoiceButtons)
            {
                if (btn != null) Destroy(btn);
            }
            cardChoiceButtons.Clear();

            if (cardChoicePrefab == null || cardChoicesContainer == null) return;

            foreach (var card in cards)
            {
                var buttonObj = Instantiate(cardChoicePrefab, cardChoicesContainer);

                // 텍스트 설정
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string typeStr = card.cardType switch
                    {
                        CardType.Attack => "[ATK]",
                        CardType.Defense => "[DEF]",
                        CardType.Skill => "[SKL]",
                        _ => ""
                    };

                    string rarityStr = card.rarity switch
                    {
                        CardRarity.Common => "",
                        CardRarity.Uncommon => "★",
                        CardRarity.Rare => "★★",
                        _ => ""
                    };

                    text.text = $"{rarityStr}{typeStr}\n{card.cardName}\n({card.energyCost})";
                }

                // 색상 설정
                var image = buttonObj.GetComponent<Image>();
                if (image != null)
                {
                    image.color = card.cardType switch
                    {
                        CardType.Attack => new Color(0.9f, 0.3f, 0.3f),
                        CardType.Defense => new Color(0.3f, 0.5f, 0.9f),
                        CardType.Skill => new Color(0.3f, 0.8f, 0.3f),
                        _ => Color.gray
                    };
                }

                // 클릭 이벤트
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    CardData selectedCard = card;
                    button.onClick.AddListener(() => OnCardSelected(selectedCard));
                }

                cardChoiceButtons.Add(buttonObj);
            }
        }

        /// <summary>
        /// 유물 보상 표시
        /// Display relic reward
        /// </summary>
        private void DisplayRelicReward(RewardType type)
        {
            if (type != RewardType.Elite && type != RewardType.Boss)
            {
                if (relicRewardPanel != null)
                    relicRewardPanel.SetActive(false);
                return;
            }

            var relic = RewardManager.Instance?.GetCachedRelicReward();

            if (relic == null)
            {
                if (relicRewardPanel != null)
                    relicRewardPanel.SetActive(false);
                return;
            }

            if (relicRewardPanel != null)
            {
                relicRewardPanel.SetActive(true);

                if (relicIcon != null && relic.icon != null)
                    relicIcon.sprite = relic.icon;

                if (relicNameText != null)
                    relicNameText.text = relic.relicName;

                if (relicDescText != null)
                    relicDescText.text = relic.description;
            }
        }

        /// <summary>
        /// 금화 획득
        /// Claim gold
        /// </summary>
        private void OnClaimGold()
        {
            if (goldClaimed) return;
            goldClaimed = true;

            RewardManager.Instance?.ClaimGold();

            // 버튼 비활성화 및 텍스트 변경
            if (claimGoldButton != null)
                claimGoldButton.interactable = false;

            if (goldAmountText != null)
                goldAmountText.text += " ✓";

            CheckAllClaimed();

            Debug.Log("[RewardUI] Gold claimed");
        }

        /// <summary>
        /// 카드 선택
        /// Card selected
        /// </summary>
        private void OnCardSelected(CardData card)
        {
            if (cardClaimed) return;
            cardClaimed = true;

            RewardManager.Instance?.ClaimCard(card);

            // 카드 패널 숨기기
            if (cardRewardPanel != null)
            {
                // 선택된 카드만 표시
                foreach (var btn in cardChoiceButtons)
                {
                    if (btn != null)
                    {
                        var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                        if (text != null && !text.text.Contains(card.cardName))
                        {
                            btn.SetActive(false);
                        }
                        else
                        {
                            var button = btn.GetComponent<Button>();
                            if (button != null)
                                button.interactable = false;

                            if (text != null)
                                text.text += "\n✓ 획득!";
                        }
                    }
                }

                if (skipCardButton != null)
                    skipCardButton.gameObject.SetActive(false);
            }

            CheckAllClaimed();

            Debug.Log($"[RewardUI] Card selected: {card.cardName}");
        }

        /// <summary>
        /// 카드 스킵
        /// Skip card
        /// </summary>
        private void OnSkipCard()
        {
            if (cardClaimed) return;
            cardClaimed = true;

            EventBus.Publish(new CardRewardSkippedEvent());

            // 카드 패널 숨기기
            if (cardRewardPanel != null)
                cardRewardPanel.SetActive(false);

            CheckAllClaimed();

            Debug.Log("[RewardUI] Card skipped");
        }

        /// <summary>
        /// 유물 획득
        /// Claim relic
        /// </summary>
        private void OnClaimRelic()
        {
            if (relicClaimed) return;
            relicClaimed = true;

            var relic = RewardManager.Instance?.GetCachedRelicReward();
            if (relic != null)
            {
                RewardManager.Instance?.ClaimRelic(relic);
            }

            // 버튼 비활성화
            if (claimRelicButton != null)
                claimRelicButton.interactable = false;

            if (relicNameText != null)
                relicNameText.text += " ✓";

            CheckAllClaimed();

            Debug.Log("[RewardUI] Relic claimed");
        }

        /// <summary>
        /// 모든 보상 획득 확인
        /// Check if all rewards claimed
        /// </summary>
        private void CheckAllClaimed()
        {
            bool allClaimed = goldClaimed;

            // 카드가 있으면 카드도 처리되어야 함
            var cards = RewardManager.Instance?.GetCachedCardChoices();
            if (cards != null && cards.Count > 0)
            {
                allClaimed = allClaimed && cardClaimed;
            }

            // 유물이 있으면 유물도 처리되어야 함
            var relic = RewardManager.Instance?.GetCachedRelicReward();
            if (relic != null)
            {
                allClaimed = allClaimed && relicClaimed;
            }

            // 진행 버튼 활성화
            if (proceedButton != null)
            {
                proceedButton.interactable = allClaimed;
            }

            if (proceedButtonText != null)
            {
                proceedButtonText.text = allClaimed ? "계속하기" : "보상을 획득하세요";
            }
        }

        /// <summary>
        /// 진행 (맵으로 복귀)
        /// Proceed (return to map)
        /// </summary>
        private void OnProceed()
        {
            EventBus.Publish(new RewardCompletedEvent());

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMap();
            }

            Debug.Log("[RewardUI] Returning to map");
        }

        private void OnDisable()
        {
            // 카드 버튼 정리
            foreach (var btn in cardChoiceButtons)
            {
                if (btn != null) Destroy(btn);
            }
            cardChoiceButtons.Clear();
        }
    }
}
