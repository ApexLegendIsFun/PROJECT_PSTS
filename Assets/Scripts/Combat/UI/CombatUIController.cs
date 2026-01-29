// Combat/UI/CombatUIController.cs
// 전투 UI 메인 컨트롤러 - New Layout

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 전투 UI 메인 컨트롤러 - 새 레이아웃
    /// - 보스전/일반전 분기 처리
    /// - PartyFormationUI 연동
    /// - DeckPileUI 연동
    /// </summary>
    public class CombatUIController : MonoBehaviour
    {
        public static CombatUIController Instance { get; private set; }

        [Header("Top Panel (Boss Health)")]
        [SerializeField] private GameObject _topPanel;
        [SerializeField] private GameObject _bossHealthPanel;
        private BossHealthBarUI _bossHealthBarUI;

        [Header("Party Area")]
        [SerializeField] private GameObject _partyArea;
        [SerializeField] private PartyFormationUI _partyFormationUI;
        [SerializeField] private Transform _partyContainer;

        [Header("Enemy Area")]
        [SerializeField] private GameObject _enemyArea;
        [SerializeField] private Transform _enemyContainer;

        [Header("Card Hand Area")]
        [SerializeField] private GameObject _cardHandArea;
        [SerializeField] private Transform _cardContainer;

        [Header("Deck Pile UI")]
        [SerializeField] private DeckPileUI _drawPileUI;
        [SerializeField] private DeckPileUI _discardPileUI;

        [Header("Text References")]
        [SerializeField] private Text _energyText;

        [Header("Buttons")]
        [SerializeField] private Button _endTurnButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject _entityStatusPrefab;

        [Header("UI Components")]
        [SerializeField] private CardHandUI _cardHandUI;
        [SerializeField] private TargetingSystem _targetingSystem;

        // 현재 상태
        private int _currentRound = 1;
        private string _currentTurnEntityId;
        private bool _isPlayerTurn;
        private bool _isBossFight;

        // 적 상태 UI 매핑
        private Dictionary<string, EntityStatusUI> _enemyStatusUIs = new Dictionary<string, EntityStatusUI>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            SubscribeToEvents();
        }

        private void Start()
        {
            InitializeUI();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            if (Instance == this) Instance = null;
        }

        #region Initialization

        private void InitializeUI()
        {
            // 턴 종료 버튼 연결
            if (_endTurnButton != null)
            {
                _endTurnButton.onClick.AddListener(OnEndTurnClicked);
                _endTurnButton.interactable = false;
            }

            // BossHealthBarUI 참조 가져오기
            if (_bossHealthPanel != null)
            {
                _bossHealthBarUI = _bossHealthPanel.GetComponent<BossHealthBarUI>();
            }

            // CardHandUI 초기화
            if (_cardHandUI == null)
            {
                _cardHandUI = GetComponentInChildren<CardHandUI>();
            }

            // TargetingSystem 초기화
            if (_targetingSystem == null)
            {
                _targetingSystem = GetComponentInChildren<TargetingSystem>();
            }

            // PartyFormationUI 초기화
            if (_partyFormationUI == null && _partyArea != null)
            {
                _partyFormationUI = _partyArea.GetComponent<PartyFormationUI>();
            }

            Debug.Log("[CombatUIController] UI Initialized (New Layout)");
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<RoundStartedEvent>(OnRoundStarted);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Subscribe<EnergyChangedEvent>(OnEnergyChanged);
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<RoundStartedEvent>(OnRoundStarted);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Unsubscribe<EnergyChangedEvent>(OnEnergyChanged);
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
        }

        #endregion

        #region Event Handlers

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            Debug.Log($"[CombatUIController] Combat started: {evt.EncounterType}");
            _currentRound = 1;

            // 보스전 여부 확인
            _isBossFight = evt.EncounterType == TileType.Boss;
            SetupBossFightUI();

            // 파티 UI 생성
            CreatePartyUI();

            // 적 상태 UI 생성
            CreateEnemyStatusUIs();

            // 덱 파일 카운트 초기화
            UpdateDeckPileCounts();
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            Debug.Log($"[CombatUIController] Combat ended. Victory: {evt.Victory}");
            if (_endTurnButton != null)
            {
                _endTurnButton.interactable = false;
            }
        }

        private void OnRoundStarted(RoundStartedEvent evt)
        {
            _currentRound = evt.RoundNumber;
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            _currentTurnEntityId = evt.EntityId;
            _isPlayerTurn = evt.IsPlayerCharacter;

            // 플레이어 턴일 때 에너지 표시 업데이트
            if (_isPlayerTurn)
            {
                var character = GetPlayerCharacter(evt.EntityId);
                if (character != null)
                {
                    UpdateEnergyText(character.CurrentEnergy, character.MaxEnergy);
                }

                // PartyFormationUI에서 활성 멤버 표시
                if (_partyFormationUI != null)
                {
                    _partyFormationUI.SetActiveMember(evt.EntityId);
                }
            }

            // 플레이어 턴일 때만 턴 종료 버튼 활성화
            if (_endTurnButton != null)
            {
                _endTurnButton.interactable = _isPlayerTurn;
            }
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            if (_endTurnButton != null && evt.EntityId == _currentTurnEntityId)
            {
                _endTurnButton.interactable = false;
            }

            // 활성 멤버 표시 해제
            if (_partyFormationUI != null)
            {
                _partyFormationUI.ClearActiveMember();
            }
        }

        private void OnEnergyChanged(EnergyChangedEvent evt)
        {
            if (evt.CharacterId == _currentTurnEntityId)
            {
                UpdateEnergyText(evt.CurrentEnergy, evt.MaxEnergy);
            }
        }

        private void OnCardDrawn(CardDrawnEvent evt)
        {
            UpdateDeckPileCounts();
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            UpdateDeckPileCounts();
        }

        #endregion

        #region Boss Fight UI

        /// <summary>
        /// 보스전/일반전 UI 설정
        /// </summary>
        private void SetupBossFightUI()
        {
            if (_isBossFight)
            {
                // 보스전: 상단 보스 체력바 표시, 적 개별 체력바 숨김
                if (_bossHealthPanel != null)
                {
                    _bossHealthPanel.SetActive(true);

                    // 보스 정보로 체력바 초기화
                    if (_bossHealthBarUI != null && CombatManager.Instance != null)
                    {
                        var boss = CombatManager.Instance.Enemies.Count > 0
                            ? CombatManager.Instance.Enemies[0]
                            : null;

                        if (boss != null)
                        {
                            _bossHealthBarUI.Initialize(boss);
                        }
                    }
                }
            }
            else
            {
                // 일반전: 상단 보스 체력바 숨김, 적 개별 체력바 표시
                if (_bossHealthPanel != null)
                {
                    _bossHealthPanel.SetActive(false);
                }
            }
        }

        #endregion

        #region Party UI

        /// <summary>
        /// 파티 UI 생성 (PartyFormationUI 사용)
        /// </summary>
        private void CreatePartyUI()
        {
            if (CombatManager.Instance == null) return;

            if (_partyFormationUI != null)
            {
                // PartyFormationUI를 사용하여 전방/후방 배치
                _partyFormationUI.Initialize(CombatManager.Instance.PlayerParty);
            }
            else if (_partyContainer != null)
            {
                // Fallback: 기존 방식으로 생성
                ClearContainer(_partyContainer);
                foreach (var member in CombatManager.Instance.PlayerParty)
                {
                    CreateEntityStatusUI(member, _partyContainer, false);
                }
            }
        }

        #endregion

        #region Enemy Status UI

        private void CreateEnemyStatusUIs()
        {
            if (CombatManager.Instance == null || _enemyContainer == null) return;

            ClearContainer(_enemyContainer);
            _enemyStatusUIs.Clear();

            foreach (var enemy in CombatManager.Instance.Enemies)
            {
                var statusUI = CreateEntityStatusUI(enemy, _enemyContainer, true);
                if (statusUI != null)
                {
                    _enemyStatusUIs[enemy.EntityId] = statusUI;

                    // 보스전일 때는 개별 체력바 숨김
                    if (_isBossFight)
                    {
                        statusUI.HideHealthBar();
                    }
                }
            }
        }

        private EntityStatusUI CreateEntityStatusUI(ICombatEntity entity, Transform container, bool isEnemy)
        {
            EntityStatusUI statusUI = null;

            if (_entityStatusPrefab != null)
            {
                var go = Instantiate(_entityStatusPrefab, container);
                statusUI = go.GetComponent<EntityStatusUI>();
                if (statusUI != null)
                {
                    statusUI.Initialize(entity, isEnemy);
                }
            }
            else
            {
                // 프리팹이 없으면 간단한 UI 동적 생성
                var go = new GameObject(entity.DisplayName + "_Status");
                go.transform.SetParent(container, false);

                var rect = go.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(150, 180);

                statusUI = go.AddComponent<EntityStatusUI>();
                statusUI.CreateSimpleUI(entity, isEnemy);
            }

            return statusUI;
        }

        private void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }

        #endregion

        #region Deck Pile Updates

        /// <summary>
        /// 덱 파일 카운트 업데이트
        /// </summary>
        private void UpdateDeckPileCounts()
        {
            if (CombatManager.Instance == null) return;

            // 현재 턴 캐릭터의 덱 정보 가져오기
            var currentMember = GetPlayerCharacter(_currentTurnEntityId);
            if (currentMember == null && CombatManager.Instance.PlayerParty.Count > 0)
            {
                currentMember = CombatManager.Instance.PlayerParty[0];
            }

            if (currentMember?.DeckManager != null)
            {
                int drawCount = currentMember.DeckManager.DrawPile.Count;
                int discardCount = currentMember.DeckManager.DiscardPile.Count;

                if (_drawPileUI != null)
                {
                    _drawPileUI.UpdateCount(drawCount);
                }

                if (_discardPileUI != null)
                {
                    _discardPileUI.UpdateCount(discardCount);
                }

                // 이벤트 발행
                EventBus.Publish(new DeckPileChangedEvent
                {
                    CharacterId = currentMember.EntityId,
                    DrawPileCount = drawCount,
                    DiscardPileCount = discardCount
                });
            }
        }

        #endregion

        #region UI Updates

        private void UpdateEnergyText(int current, int max)
        {
            if (_energyText != null)
            {
                _energyText.text = $"{current}";
            }
        }

        private PartyMemberCombat GetPlayerCharacter(string entityId)
        {
            if (CombatManager.Instance == null || string.IsNullOrEmpty(entityId)) return null;

            foreach (var member in CombatManager.Instance.PlayerParty)
            {
                if (member != null && member.EntityId == entityId)
                {
                    return member;
                }
            }
            return null;
        }

        #endregion

        #region Button Handlers

        private void OnEndTurnClicked()
        {
            if (CombatManager.Instance != null && _isPlayerTurn)
            {
                Debug.Log("[CombatUIController] End Turn clicked");
                CombatManager.Instance.EndPlayerTurn();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 현재 턴의 파티원 가져오기
        /// </summary>
        public PartyMemberCombat GetCurrentTurnPartyMember()
        {
            return GetPlayerCharacter(_currentTurnEntityId);
        }

        /// <summary>
        /// 카드 컨테이너 반환
        /// </summary>
        public Transform GetCardContainer() => _cardContainer;

        /// <summary>
        /// 적 컨테이너 반환
        /// </summary>
        public Transform GetEnemyContainer() => _enemyContainer;

        /// <summary>
        /// 파티 컨테이너 반환
        /// </summary>
        public Transform GetPartyContainer() => _partyContainer;

        /// <summary>
        /// PartyFormationUI 반환 (타겟팅 시스템용)
        /// </summary>
        public PartyFormationUI GetPartyFormationUI() => _partyFormationUI;

        /// <summary>
        /// 보스전 여부
        /// </summary>
        public bool IsBossFight => _isBossFight;

        #endregion
    }
}
