using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Shop;
using ProjectSS.Run;

namespace ProjectSS.UI
{
    /// <summary>
    /// 상점 씬 UI 관리자
    /// Shop scene UI manager
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button cardsTabButton;
        [SerializeField] private Button relicsTabButton;
        [SerializeField] private Button servicesTabButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject cardsPanel;
        [SerializeField] private GameObject relicsPanel;
        [SerializeField] private GameObject servicesPanel;

        [Header("Cards Tab")]
        [SerializeField] private Transform cardGrid;
        [SerializeField] private GameObject shopCardPrefab;

        [Header("Relics Tab")]
        [SerializeField] private Transform relicGrid;
        [SerializeField] private GameObject shopRelicPrefab;

        [Header("Services Tab")]
        [SerializeField] private Button removeCardButton;
        [SerializeField] private TextMeshProUGUI removeCardCostText;
        [SerializeField] private Button upgradeCardButton;
        [SerializeField] private TextMeshProUGUI upgradeCardCostText;
        [SerializeField] private Button healPartyButton;
        [SerializeField] private TextMeshProUGUI healPartyCostText;

        [Header("Card Selection Modal")]
        [SerializeField] private GameObject cardSelectionModal;
        [SerializeField] private Transform deckCardGrid;
        [SerializeField] private GameObject deckCardPrefab;
        [SerializeField] private Button cancelSelectionButton;
        [SerializeField] private TextMeshProUGUI modalTitleText;

        [Header("Player Status")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private PartyStatusDisplay partyStatusDisplay;

        [Header("Navigation")]
        [SerializeField] private Button leaveButton;

        [Header("Shopkeeper")]
        [SerializeField] private TextMeshProUGUI shopkeeperDialogue;

        // 상태
        private ShopTab currentTab = ShopTab.Cards;
        private CardSelectionMode selectionMode = CardSelectionMode.None;
        private List<GameObject> cardItems = new List<GameObject>();
        private List<GameObject> relicItems = new List<GameObject>();
        private List<GameObject> deckCardButtons = new List<GameObject>();

        private enum ShopTab { Cards, Relics, Services }
        private enum CardSelectionMode { None, Remove, Upgrade }

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
            // 상점 인벤토리 생성
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.GenerateShopInventory();
            }

            // 탭 버튼 이벤트
            if (cardsTabButton != null)
            {
                cardsTabButton.onClick.RemoveAllListeners();
                cardsTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Cards));
            }

            if (relicsTabButton != null)
            {
                relicsTabButton.onClick.RemoveAllListeners();
                relicsTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Relics));
            }

            if (servicesTabButton != null)
            {
                servicesTabButton.onClick.RemoveAllListeners();
                servicesTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Services));
            }

            // 서비스 버튼 이벤트
            if (removeCardButton != null)
            {
                removeCardButton.onClick.RemoveAllListeners();
                removeCardButton.onClick.AddListener(OnRemoveCardService);
            }

            if (upgradeCardButton != null)
            {
                upgradeCardButton.onClick.RemoveAllListeners();
                upgradeCardButton.onClick.AddListener(OnUpgradeCardService);
            }

            if (healPartyButton != null)
            {
                healPartyButton.onClick.RemoveAllListeners();
                healPartyButton.onClick.AddListener(OnHealPartyService);
            }

            // 모달 취소 버튼
            if (cancelSelectionButton != null)
            {
                cancelSelectionButton.onClick.RemoveAllListeners();
                cancelSelectionButton.onClick.AddListener(CloseCardSelection);
            }

            // 나가기 버튼
            if (leaveButton != null)
            {
                leaveButton.onClick.RemoveAllListeners();
                leaveButton.onClick.AddListener(OnLeaveShop);
            }

            // 모달 숨기기
            if (cardSelectionModal != null)
                cardSelectionModal.SetActive(false);

            // 초기 탭
            SwitchTab(ShopTab.Cards);

            // 상태 업데이트
            UpdatePlayerStatus();
            UpdateShopkeeperDialogue("어서오세요! 좋은 물건 많습니다.");
        }

        /// <summary>
        /// 탭 전환
        /// Switch tab
        /// </summary>
        private void SwitchTab(ShopTab tab)
        {
            currentTab = tab;

            // 패널 활성화/비활성화
            if (cardsPanel != null)
                cardsPanel.SetActive(tab == ShopTab.Cards);

            if (relicsPanel != null)
                relicsPanel.SetActive(tab == ShopTab.Relics);

            if (servicesPanel != null)
                servicesPanel.SetActive(tab == ShopTab.Services);

            // 탭 버튼 색상
            UpdateTabButtonColors();

            // 탭별 콘텐츠 갱신
            switch (tab)
            {
                case ShopTab.Cards:
                    RefreshCardsTab();
                    break;
                case ShopTab.Relics:
                    RefreshRelicsTab();
                    break;
                case ShopTab.Services:
                    RefreshServicesTab();
                    break;
            }
        }

        /// <summary>
        /// 탭 버튼 색상 업데이트
        /// Update tab button colors
        /// </summary>
        private void UpdateTabButtonColors()
        {
            Color activeColor = new Color(0.3f, 0.6f, 0.9f);
            Color inactiveColor = Color.gray;

            if (cardsTabButton != null)
            {
                var colors = cardsTabButton.colors;
                colors.normalColor = currentTab == ShopTab.Cards ? activeColor : inactiveColor;
                cardsTabButton.colors = colors;
            }

            if (relicsTabButton != null)
            {
                var colors = relicsTabButton.colors;
                colors.normalColor = currentTab == ShopTab.Relics ? activeColor : inactiveColor;
                relicsTabButton.colors = colors;
            }

            if (servicesTabButton != null)
            {
                var colors = servicesTabButton.colors;
                colors.normalColor = currentTab == ShopTab.Services ? activeColor : inactiveColor;
                servicesTabButton.colors = colors;
            }
        }

        /// <summary>
        /// 카드 탭 갱신
        /// Refresh cards tab
        /// </summary>
        private void RefreshCardsTab()
        {
            // 기존 아이템 제거
            foreach (var item in cardItems)
            {
                if (item != null) Destroy(item);
            }
            cardItems.Clear();

            if (shopCardPrefab == null || cardGrid == null) return;

            var shopCards = ShopManager.Instance?.GetShopCards();
            if (shopCards == null) return;

            foreach (var shopItem in shopCards)
            {
                var itemObj = Instantiate(shopCardPrefab, cardGrid);
                SetupShopCardItem(itemObj, shopItem);
                cardItems.Add(itemObj);
            }
        }

        /// <summary>
        /// 상점 카드 아이템 설정
        /// Setup shop card item
        /// </summary>
        private void SetupShopCardItem(GameObject itemObj, ShopItem shopItem)
        {
            // 텍스트 설정
            var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text.name.Contains("Name") || text.name.Contains("name"))
                {
                    text.text = shopItem.displayName;
                }
                else if (text.name.Contains("Price") || text.name.Contains("price"))
                {
                    if (shopItem.isDiscounted)
                    {
                        text.text = $"<s>{shopItem.originalPrice}G</s> {shopItem.price}G";
                        text.color = Color.yellow;
                    }
                    else
                    {
                        text.text = $"{shopItem.price}G";
                    }
                }
                else
                {
                    // 기본 텍스트
                    string rarityStr = shopItem.cardData?.rarity switch
                    {
                        CardRarity.Common => "",
                        CardRarity.Uncommon => "★",
                        CardRarity.Rare => "★★",
                        _ => ""
                    };
                    text.text = $"{rarityStr}{shopItem.displayName}\n{shopItem.price}G";
                }
            }

            // 버튼 설정
            var button = itemObj.GetComponent<Button>();
            if (button != null)
            {
                bool canAfford = ShopManager.Instance?.CanAfford(shopItem.price) ?? false;
                button.interactable = !shopItem.isSold && canAfford;

                ShopItem item = shopItem;
                button.onClick.AddListener(() => OnPurchaseCard(item));
            }

            // 판매 완료 표시
            if (shopItem.isSold)
            {
                var image = itemObj.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
        }

        /// <summary>
        /// 유물 탭 갱신
        /// Refresh relics tab
        /// </summary>
        private void RefreshRelicsTab()
        {
            // 기존 아이템 제거
            foreach (var item in relicItems)
            {
                if (item != null) Destroy(item);
            }
            relicItems.Clear();

            if (shopRelicPrefab == null || relicGrid == null) return;

            var shopRelics = ShopManager.Instance?.GetShopRelics();
            if (shopRelics == null) return;

            foreach (var shopItem in shopRelics)
            {
                var itemObj = Instantiate(shopRelicPrefab, relicGrid);
                SetupShopRelicItem(itemObj, shopItem);
                relicItems.Add(itemObj);
            }
        }

        /// <summary>
        /// 상점 유물 아이템 설정
        /// Setup shop relic item
        /// </summary>
        private void SetupShopRelicItem(GameObject itemObj, ShopItem shopItem)
        {
            var texts = itemObj.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                text.text = $"{shopItem.displayName}\n{shopItem.price}G";
            }

            // 아이콘 설정
            if (shopItem.relicData?.icon != null)
            {
                var images = itemObj.GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    if (img.name.Contains("Icon") || img.name.Contains("icon"))
                    {
                        img.sprite = shopItem.relicData.icon;
                    }
                }
            }

            // 버튼 설정
            var button = itemObj.GetComponent<Button>();
            if (button != null)
            {
                bool canAfford = ShopManager.Instance?.CanAfford(shopItem.price) ?? false;
                button.interactable = !shopItem.isSold && canAfford;

                ShopItem item = shopItem;
                button.onClick.AddListener(() => OnPurchaseRelic(item));
            }

            if (shopItem.isSold)
            {
                var image = itemObj.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }
            }
        }

        /// <summary>
        /// 서비스 탭 갱신
        /// Refresh services tab
        /// </summary>
        private void RefreshServicesTab()
        {
            int playerGold = ShopManager.Instance?.GetPlayerGold() ?? 0;

            // 카드 제거
            int removeCost = ShopManager.Instance?.GetRemoveCost() ?? 50;
            if (removeCardCostText != null)
                removeCardCostText.text = $"{removeCost}G";
            if (removeCardButton != null)
                removeCardButton.interactable = playerGold >= removeCost;

            // 카드 업그레이드
            int upgradeCost = ShopManager.Instance?.GetUpgradeCost() ?? 75;
            if (upgradeCardCostText != null)
                upgradeCardCostText.text = $"{upgradeCost}G";
            if (upgradeCardButton != null)
            {
                bool hasUpgradeable = GetUpgradeableCardCount() > 0;
                upgradeCardButton.interactable = playerGold >= upgradeCost && hasUpgradeable;
            }

            // 파티 치료
            int healCost = ShopManager.Instance?.GetHealCost() ?? 0;
            if (healPartyCostText != null)
            {
                if (healCost > 0)
                    healPartyCostText.text = $"{healCost}G";
                else
                    healPartyCostText.text = "만피 상태";
            }
            if (healPartyButton != null)
                healPartyButton.interactable = healCost > 0 && playerGold >= healCost;
        }

        /// <summary>
        /// 업그레이드 가능 카드 수
        /// Get upgradeable card count
        /// </summary>
        private int GetUpgradeableCardCount()
        {
            if (!RunManager.Instance?.IsPartyMode ?? true) return 0;

            var party = RunManager.Instance.PartyState;
            int count = 0;
            for (int i = 0; i < party.deckUpgraded.Count; i++)
            {
                if (!party.deckUpgraded[i])
                    count++;
            }
            return count;
        }

        /// <summary>
        /// 카드 구매
        /// Purchase card
        /// </summary>
        private void OnPurchaseCard(ShopItem item)
        {
            if (ShopManager.Instance?.PurchaseItem(item) ?? false)
            {
                UpdatePlayerStatus();
                RefreshCardsTab();
                UpdateShopkeeperDialogue("좋은 선택이에요!");
            }
            else
            {
                UpdateShopkeeperDialogue("골드가 부족해요...");
            }
        }

        /// <summary>
        /// 유물 구매
        /// Purchase relic
        /// </summary>
        private void OnPurchaseRelic(ShopItem item)
        {
            if (ShopManager.Instance?.PurchaseItem(item) ?? false)
            {
                UpdatePlayerStatus();
                RefreshRelicsTab();
                UpdateShopkeeperDialogue("훌륭한 유물이에요!");
            }
            else
            {
                UpdateShopkeeperDialogue("골드가 부족해요...");
            }
        }

        /// <summary>
        /// 카드 제거 서비스
        /// Remove card service
        /// </summary>
        private void OnRemoveCardService()
        {
            selectionMode = CardSelectionMode.Remove;
            OpenCardSelection("제거할 카드 선택");
        }

        /// <summary>
        /// 카드 업그레이드 서비스
        /// Upgrade card service
        /// </summary>
        private void OnUpgradeCardService()
        {
            selectionMode = CardSelectionMode.Upgrade;
            OpenCardSelection("업그레이드할 카드 선택");
        }

        /// <summary>
        /// 파티 치료 서비스
        /// Heal party service
        /// </summary>
        private void OnHealPartyService()
        {
            if (ShopManager.Instance?.HealParty() ?? false)
            {
                UpdatePlayerStatus();
                RefreshServicesTab();
                UpdateShopkeeperDialogue("모두 회복됐어요!");
            }
        }

        /// <summary>
        /// 카드 선택 모달 열기
        /// Open card selection modal
        /// </summary>
        private void OpenCardSelection(string title)
        {
            if (cardSelectionModal == null) return;

            cardSelectionModal.SetActive(true);

            if (modalTitleText != null)
                modalTitleText.text = title;

            PopulateDeckCards();
        }

        /// <summary>
        /// 덱 카드 목록 생성
        /// Populate deck cards
        /// </summary>
        private void PopulateDeckCards()
        {
            // 기존 버튼 제거
            foreach (var btn in deckCardButtons)
            {
                if (btn != null) Destroy(btn);
            }
            deckCardButtons.Clear();

            if (deckCardPrefab == null || deckCardGrid == null) return;
            if (!RunManager.Instance?.IsPartyMode ?? true) return;

            var party = RunManager.Instance.PartyState;
            var cardDatabase = Resources.LoadAll<CardData>("Data/Cards");

            for (int i = 0; i < party.deckCardIds.Count; i++)
            {
                // 업그레이드 모드에서는 이미 업그레이드된 카드 스킵
                if (selectionMode == CardSelectionMode.Upgrade && party.deckUpgraded[i])
                    continue;

                string cardId = party.deckCardIds[i];
                CardData cardData = null;

                foreach (var card in cardDatabase)
                {
                    if (card.cardId == cardId)
                    {
                        cardData = card;
                        break;
                    }
                }

                if (cardData == null) continue;

                int cardIndex = i;
                var buttonObj = Instantiate(deckCardPrefab, deckCardGrid);

                // 텍스트 설정
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string upgraded = party.deckUpgraded[i] ? "+" : "";
                    text.text = $"{cardData.cardName}{upgraded}";
                }

                // 버튼 클릭
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnDeckCardSelected(cardIndex));
                }

                deckCardButtons.Add(buttonObj);
            }
        }

        /// <summary>
        /// 덱 카드 선택
        /// Deck card selected
        /// </summary>
        private void OnDeckCardSelected(int cardIndex)
        {
            bool success = false;

            switch (selectionMode)
            {
                case CardSelectionMode.Remove:
                    success = ShopManager.Instance?.RemoveCard(cardIndex) ?? false;
                    if (success)
                        UpdateShopkeeperDialogue("카드가 제거됐어요.");
                    break;

                case CardSelectionMode.Upgrade:
                    success = ShopManager.Instance?.UpgradeCard(cardIndex) ?? false;
                    if (success)
                        UpdateShopkeeperDialogue("카드가 강해졌어요!");
                    break;
            }

            if (success)
            {
                CloseCardSelection();
                UpdatePlayerStatus();
                RefreshServicesTab();
            }
        }

        /// <summary>
        /// 카드 선택 모달 닫기
        /// Close card selection modal
        /// </summary>
        private void CloseCardSelection()
        {
            selectionMode = CardSelectionMode.None;

            if (cardSelectionModal != null)
                cardSelectionModal.SetActive(false);
        }

        /// <summary>
        /// 플레이어 상태 업데이트
        /// Update player status
        /// </summary>
        private void UpdatePlayerStatus()
        {
            int gold = ShopManager.Instance?.GetPlayerGold() ?? 0;

            if (goldText != null)
                goldText.text = $"{gold}G";

            if (partyStatusDisplay != null)
                partyStatusDisplay.Refresh();

            // 현재 탭 갱신 (구매 가능 여부 업데이트)
            switch (currentTab)
            {
                case ShopTab.Cards:
                    RefreshCardsTab();
                    break;
                case ShopTab.Relics:
                    RefreshRelicsTab();
                    break;
                case ShopTab.Services:
                    RefreshServicesTab();
                    break;
            }
        }

        /// <summary>
        /// 상인 대사 업데이트
        /// Update shopkeeper dialogue
        /// </summary>
        private void UpdateShopkeeperDialogue(string dialogue)
        {
            if (shopkeeperDialogue != null)
                shopkeeperDialogue.text = dialogue;
        }

        /// <summary>
        /// 상점 나가기
        /// Leave shop
        /// </summary>
        private void OnLeaveShop()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMap();
            }

            Debug.Log("[ShopUI] Leaving shop");
        }

        private void OnDisable()
        {
            // 정리
            foreach (var item in cardItems)
            {
                if (item != null) Destroy(item);
            }
            cardItems.Clear();

            foreach (var item in relicItems)
            {
                if (item != null) Destroy(item);
            }
            relicItems.Clear();

            foreach (var btn in deckCardButtons)
            {
                if (btn != null) Destroy(btn);
            }
            deckCardButtons.Clear();
        }
    }
}
