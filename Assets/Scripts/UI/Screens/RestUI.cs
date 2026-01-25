using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Run;

namespace ProjectSS.UI
{
    /// <summary>
    /// 휴식 씬 UI 관리자
    /// Rest scene UI manager
    /// </summary>
    public class RestUI : MonoBehaviour
    {
        [Header("Main Options")]
        [SerializeField] private Button restButton;
        [SerializeField] private Button smithButton;
        [SerializeField] private TextMeshProUGUI restButtonText;
        [SerializeField] private TextMeshProUGUI smithButtonText;

        [Header("Heal Preview")]
        [SerializeField] private TextMeshProUGUI healPreviewText;
        [SerializeField] private float healPercent = 0.30f;

        [Header("Party Status")]
        [SerializeField] private PartyStatusDisplay partyStatusDisplay;

        [Header("Card Selection (Smith)")]
        [SerializeField] private GameObject cardSelectionPanel;
        [SerializeField] private Transform cardGrid;
        [SerializeField] private GameObject cardButtonPrefab;
        [SerializeField] private Button cancelSmithButton;

        [Header("Result Panel")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button continueButton;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;

        // 상태
        private bool hasActed = false;
        private List<GameObject> cardButtons = new List<GameObject>();

        private void OnEnable()
        {
            EventBus.Publish(new RestStartedEvent());
        }

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
            hasActed = false;

            // UI 초기화
            if (resultPanel != null)
                resultPanel.SetActive(false);

            if (cardSelectionPanel != null)
                cardSelectionPanel.SetActive(false);

            // 버튼 이벤트 연결
            if (restButton != null)
            {
                restButton.onClick.RemoveAllListeners();
                restButton.onClick.AddListener(OnRestSelected);
            }

            if (smithButton != null)
            {
                smithButton.onClick.RemoveAllListeners();
                smithButton.onClick.AddListener(OnSmithSelected);
            }

            if (cancelSmithButton != null)
            {
                cancelSmithButton.onClick.RemoveAllListeners();
                cancelSmithButton.onClick.AddListener(OnCancelSmith);
            }

            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinue);
            }

            // 타이틀 설정
            if (titleText != null)
                titleText.text = "휴식 (Rest)";

            // 파티 상태 표시
            if (partyStatusDisplay != null)
                partyStatusDisplay.Refresh();

            // 힐 프리뷰 계산
            UpdateHealPreview();

            // 업그레이드 가능 카드 체크
            UpdateSmithButton();
        }

        /// <summary>
        /// 힐 프리뷰 업데이트
        /// Update heal preview
        /// </summary>
        private void UpdateHealPreview()
        {
            if (RunManager.Instance == null || !RunManager.Instance.IsPartyMode)
                return;

            var party = RunManager.Instance.PartyState;

            int warriorHeal = CalculateHeal(party.warrior);
            int mageHeal = CalculateHeal(party.mage);
            int rogueHeal = CalculateHeal(party.rogue);
            int totalHeal = warriorHeal + mageHeal + rogueHeal;

            if (restButtonText != null)
            {
                restButtonText.text = $"휴식\n(전체 +{totalHeal} HP)";
            }

            if (healPreviewText != null)
            {
                healPreviewText.text = $"전사 +{warriorHeal} / 마법사 +{mageHeal} / 도적 +{rogueHeal}";
            }

            // 파티 상태 프리뷰
            if (partyStatusDisplay != null)
                partyStatusDisplay.ShowHealPreview(healPercent);
        }

        /// <summary>
        /// 힐량 계산
        /// Calculate heal amount
        /// </summary>
        private int CalculateHeal(CharacterState character)
        {
            int healAmount = Mathf.RoundToInt(character.maxHP * healPercent);
            return Mathf.Min(healAmount, character.maxHP - character.currentHP);
        }

        /// <summary>
        /// 대장장이 버튼 업데이트
        /// Update smith button
        /// </summary>
        private void UpdateSmithButton()
        {
            if (smithButton == null) return;

            int upgradeableCount = GetUpgradeableCardCount();
            smithButton.interactable = upgradeableCount > 0;

            if (smithButtonText != null)
            {
                if (upgradeableCount > 0)
                    smithButtonText.text = $"대장장이\n(카드 업그레이드)";
                else
                    smithButtonText.text = "대장장이\n(업그레이드 불가)";
            }
        }

        /// <summary>
        /// 업그레이드 가능한 카드 수
        /// Get upgradeable card count
        /// </summary>
        private int GetUpgradeableCardCount()
        {
            if (RunManager.Instance == null || !RunManager.Instance.IsPartyMode)
                return 0;

            var party = RunManager.Instance.PartyState;
            int count = 0;

            for (int i = 0; i < party.deckCardIds.Count; i++)
            {
                if (!party.deckUpgraded[i])
                    count++;
            }

            return count;
        }

        /// <summary>
        /// 휴식 선택
        /// Rest selected
        /// </summary>
        private void OnRestSelected()
        {
            if (hasActed) return;
            hasActed = true;

            // 파티 전체 힐
            var party = RunManager.Instance.PartyState;

            int warriorHeal = CalculateHeal(party.warrior);
            int mageHeal = CalculateHeal(party.mage);
            int rogueHeal = CalculateHeal(party.rogue);

            party.warrior.Heal(warriorHeal);
            party.mage.Heal(mageHeal);
            party.rogue.Heal(rogueHeal);

            // 이벤트 발행
            EventBus.Publish(new RestHealedEvent(warriorHeal, mageHeal, rogueHeal));

            // 결과 표시
            ShowResult($"휴식 완료!\n\n전사: +{warriorHeal} HP\n마법사: +{mageHeal} HP\n도적: +{rogueHeal} HP");

            Debug.Log($"[RestUI] Party healed - Warrior: +{warriorHeal}, Mage: +{mageHeal}, Rogue: +{rogueHeal}");
        }

        /// <summary>
        /// 대장장이 선택
        /// Smith selected
        /// </summary>
        private void OnSmithSelected()
        {
            if (hasActed) return;

            // 카드 선택 패널 표시
            if (cardSelectionPanel != null)
            {
                cardSelectionPanel.SetActive(true);
                PopulateCardSelection();
            }
        }

        /// <summary>
        /// 카드 선택 목록 생성
        /// Populate card selection list
        /// </summary>
        private void PopulateCardSelection()
        {
            // 기존 버튼 제거
            foreach (var btn in cardButtons)
            {
                if (btn != null) Destroy(btn);
            }
            cardButtons.Clear();

            if (cardButtonPrefab == null || cardGrid == null) return;

            var party = RunManager.Instance.PartyState;
            var cardDatabase = Resources.LoadAll<CardData>("Data/Cards");

            for (int i = 0; i < party.deckCardIds.Count; i++)
            {
                // 이미 업그레이드된 카드는 스킵
                if (party.deckUpgraded[i]) continue;

                string cardId = party.deckCardIds[i];
                CardData cardData = FindCard(cardDatabase, cardId);

                if (cardData == null) continue;

                // 업그레이드 버전이 있는지 확인
                if (cardData.upgradedVersion == null) continue;

                int cardIndex = i;
                var buttonObj = Instantiate(cardButtonPrefab, cardGrid);

                // 버튼 텍스트 설정
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{cardData.cardName}\n→ {cardData.upgradedVersion.cardName}";
                }

                // 버튼 클릭 이벤트
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnCardSelectedForUpgrade(cardIndex, cardData));
                }

                cardButtons.Add(buttonObj);
            }
        }

        /// <summary>
        /// 카드 데이터 찾기
        /// Find card data
        /// </summary>
        private CardData FindCard(CardData[] database, string cardId)
        {
            foreach (var card in database)
            {
                if (card.cardId == cardId)
                    return card;
            }
            return null;
        }

        /// <summary>
        /// 업그레이드할 카드 선택
        /// Card selected for upgrade
        /// </summary>
        private void OnCardSelectedForUpgrade(int cardIndex, CardData cardData)
        {
            hasActed = true;

            // 업그레이드 적용
            var party = RunManager.Instance.PartyState;
            party.deckUpgraded[cardIndex] = true;

            // 이벤트 발행
            EventBus.Publish(new CardUpgradedEvent(cardData.cardId));

            // 카드 선택 패널 숨기기
            if (cardSelectionPanel != null)
                cardSelectionPanel.SetActive(false);

            // 결과 표시
            string upgradedName = cardData.upgradedVersion != null
                ? cardData.upgradedVersion.cardName
                : $"{cardData.cardName}+";

            ShowResult($"대장장이 완료!\n\n{cardData.cardName}\n→ {upgradedName}");

            Debug.Log($"[RestUI] Card upgraded: {cardData.cardName} at index {cardIndex}");
        }

        /// <summary>
        /// 대장장이 취소
        /// Cancel smith
        /// </summary>
        private void OnCancelSmith()
        {
            if (cardSelectionPanel != null)
                cardSelectionPanel.SetActive(false);
        }

        /// <summary>
        /// 결과 표시
        /// Show result
        /// </summary>
        private void ShowResult(string message)
        {
            // 옵션 버튼 비활성화
            if (restButton != null)
                restButton.interactable = false;

            if (smithButton != null)
                smithButton.interactable = false;

            // 파티 상태 새로고침
            if (partyStatusDisplay != null)
            {
                partyStatusDisplay.HideHealPreview();
                partyStatusDisplay.Refresh();
            }

            // 결과 패널 표시
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);

                if (resultText != null)
                    resultText.text = message;
            }
        }

        /// <summary>
        /// 계속하기 (맵으로 복귀)
        /// Continue (return to map)
        /// </summary>
        private void OnContinue()
        {
            // 휴식 완료 이벤트 발행
            string actionType = hasActed ? "rest" : "skip";
            EventBus.Publish(new RestCompletedEvent(actionType));

            // 맵으로 복귀
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMap();
            }

            Debug.Log("[RestUI] Returning to map");
        }

        private void OnDisable()
        {
            // 카드 버튼 정리
            foreach (var btn in cardButtons)
            {
                if (btn != null) Destroy(btn);
            }
            cardButtons.Clear();
        }
    }
}
