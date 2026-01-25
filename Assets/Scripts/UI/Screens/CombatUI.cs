using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Core;
using ProjectSS.Combat;
using ProjectSS.Data;

namespace ProjectSS.UI
{
    /// <summary>
    /// 전투 UI 관리자
    /// Combat UI manager
    ///
    /// TRIAD: 파티 시스템 통합
    /// </summary>
    public class CombatUI : MonoBehaviour
    {
        [Header("TRIAD - Party UI")]
        [Tooltip("파티 표시 UI / Party display UI")]
        [SerializeField] private PartyDisplayUI partyDisplayUI;

        [Header("Player UI (Legacy - 하위 호환)")]
        [SerializeField] private HealthBarUI playerHealthBar;
        [SerializeField] private EnergyUI energyUI;

        [Header("Hand UI")]
        [SerializeField] private HandUI handUI;

        [Header("Buttons")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private TextMeshProUGUI endTurnText;

        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI discardCountText;
        [SerializeField] private TextMeshProUGUI exhaustCountText;

        [Header("Enemy Area")]
        [SerializeField] private Transform enemyUIContainer;
        [SerializeField] private IntentUI intentUIPrefab;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Start()
        {
            SetupButtons();
        }

        private void SetupButtons()
        {
            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }
        }

        private void SubscribeEvents()
        {
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Subscribe<EnergyChangedEvent>(OnEnergyChanged);
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Subscribe<TagInEvent>(OnTagIn);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Unsubscribe<EnergyChangedEvent>(OnEnergyChanged);
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Unsubscribe<TagInEvent>(OnTagIn);
        }

        /// <summary>
        /// TRIAD: 전투 시작 시 파티 UI 초기화
        /// TRIAD: Initialize party UI on combat start
        /// </summary>
        private void OnCombatStarted(CombatStartedEvent e)
        {
            if (partyDisplayUI != null)
            {
                partyDisplayUI.Initialize();
            }
        }

        /// <summary>
        /// TRIAD: Tag-In 발생 시 UI 업데이트
        /// TRIAD: Update UI on Tag-In
        /// </summary>
        private void OnTagIn(TagInEvent e)
        {
            // 파티 UI 새로고침
            if (partyDisplayUI != null)
            {
                partyDisplayUI.RefreshAllSlots();
            }

            // 핸드 UI 새로고침 (클래스 제한 카드 표시 업데이트)
            RefreshHandUI();
        }

        private void OnTurnStarted(TurnStartedEvent e)
        {
            // 플레이어 턴이면 버튼 활성화
            if (endTurnButton != null)
            {
                endTurnButton.interactable = e.IsPlayerTurn;
            }

            if (endTurnText != null)
            {
                endTurnText.text = e.IsPlayerTurn ? "End Turn" : "Enemy Turn";
            }

            UpdateDeckCounts();
        }

        private void OnTurnEnded(TurnEndedEvent e)
        {
            UpdateDeckCounts();
        }

        private void OnEnergyChanged(EnergyChangedEvent e)
        {
            if (energyUI != null)
            {
                energyUI.UpdateEnergy(e.CurrentEnergy, e.MaxEnergy);
            }
        }

        private void OnCardDrawn(CardDrawnEvent e)
        {
            UpdateDeckCounts();
            RefreshHandUI();
        }

        private void OnCombatEnded(CombatEndedEvent e)
        {
            if (endTurnButton != null)
            {
                endTurnButton.interactable = false;
            }
        }

        private void OnEndTurnClicked()
        {
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.OnEndTurnClicked();
            }
        }

        /// <summary>
        /// 덱 카운트 업데이트
        /// Update deck counts
        /// </summary>
        private void UpdateDeckCounts()
        {
            if (CombatManager.Instance?.DeckManager == null) return;

            var deck = CombatManager.Instance.DeckManager;

            if (deckCountText != null)
                deckCountText.text = deck.DrawPileSize.ToString();

            if (discardCountText != null)
                discardCountText.text = deck.DiscardPileSize.ToString();

            if (exhaustCountText != null)
                exhaustCountText.text = deck.ExhaustPile.Count.ToString();
        }

        /// <summary>
        /// 핸드 UI 새로고침
        /// Refresh hand UI
        /// </summary>
        public void RefreshHandUI()
        {
            if (handUI != null)
            {
                handUI.RefreshHand();
            }
        }

        /// <summary>
        /// 플레이어 체력바 업데이트 (Legacy)
        /// Update player health bar (Legacy)
        /// </summary>
        public void UpdatePlayerHealth(int current, int max, int block)
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.UpdateHealth(current, max, block);
            }
        }

        #region TRIAD Party UI

        /// <summary>
        /// TRIAD: 파티 표시 UI 가져오기
        /// TRIAD: Get party display UI
        /// </summary>
        public PartyDisplayUI PartyDisplay => partyDisplayUI;

        /// <summary>
        /// TRIAD: 특정 캐릭터 슬롯 새로고침
        /// TRIAD: Refresh specific character slot
        /// </summary>
        public void RefreshCharacterSlot(CharacterClass characterClass)
        {
            if (partyDisplayUI != null)
            {
                partyDisplayUI.RefreshSlot(characterClass);
            }
        }

        /// <summary>
        /// TRIAD: 모든 파티 슬롯 새로고침
        /// TRIAD: Refresh all party slots
        /// </summary>
        public void RefreshAllPartySlots()
        {
            if (partyDisplayUI != null)
            {
                partyDisplayUI.RefreshAllSlots();
            }
        }

        #endregion
    }
}
