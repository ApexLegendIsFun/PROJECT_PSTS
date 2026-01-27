// Combat/UI/CombatUIController.cs
// 전투 UI 메인 컨트롤러

using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 전투 UI 메인 컨트롤러
    /// 모든 전투 UI 컴포넌트 관리 및 이벤트 연결
    /// </summary>
    public class CombatUIController : MonoBehaviour
    {
        public static CombatUIController Instance { get; private set; }

        [Header("Panel References")]
        [SerializeField] private GameObject _topPanel;
        [SerializeField] private GameObject _enemyArea;
        [SerializeField] private GameObject _playerArea;
        [SerializeField] private GameObject _cardHandArea;
        [SerializeField] private GameObject _combatInfoPanel;

        [Header("Containers")]
        [SerializeField] private Transform _enemyContainer;
        [SerializeField] private Transform _partyContainer;
        [SerializeField] private Transform _cardContainer;

        [Header("Text References")]
        [SerializeField] private Text _roundText;
        [SerializeField] private Text _turnIndicatorText;
        [SerializeField] private Text _energyText;

        [Header("Buttons")]
        [SerializeField] private Button _endTurnButton;

        [Header("Prefabs")]
        [SerializeField] private GameObject _cardUIPrefab;
        [SerializeField] private GameObject _entityStatusPrefab;

        [Header("UI Components")]
        [SerializeField] private CardHandUI _cardHandUI;
        [SerializeField] private TargetingSystem _targetingSystem;

        // 현재 상태
        private int _currentRound = 1;
        private string _currentTurnEntityId;
        private bool _isPlayerTurn;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
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

            Debug.Log("[CombatUIController] UI Initialized");
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Subscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Subscribe<RoundStartedEvent>(OnRoundStarted);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Subscribe<EnergyChangedEvent>(OnEnergyChanged);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            EventBus.Unsubscribe<CombatEndedEvent>(OnCombatEnded);
            EventBus.Unsubscribe<RoundStartedEvent>(OnRoundStarted);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Unsubscribe<EnergyChangedEvent>(OnEnergyChanged);
        }

        #endregion

        #region Event Handlers

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            Debug.Log($"[CombatUIController] Combat started: {evt.EncounterType}");
            _currentRound = 1;
            UpdateRoundText();

            // 엔티티 상태 UI 생성
            CreateEntityStatusUIs();
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            Debug.Log($"[CombatUIController] Combat ended. Victory: {evt.Victory}");
            _endTurnButton.interactable = false;

            // TODO: 승리/패배 UI 표시
        }

        private void OnRoundStarted(RoundStartedEvent evt)
        {
            _currentRound = evt.RoundNumber;
            UpdateRoundText();
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            _currentTurnEntityId = evt.EntityId;
            _isPlayerTurn = evt.IsPlayerCharacter;

            UpdateTurnIndicator();

            // 플레이어 턴일 때만 턴 종료 버튼 활성화
            if (_endTurnButton != null)
            {
                _endTurnButton.interactable = _isPlayerTurn;
            }
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            // 턴 종료 시 버튼 비활성화
            if (_endTurnButton != null && evt.EntityId == _currentTurnEntityId)
            {
                _endTurnButton.interactable = false;
            }
        }

        private void OnEnergyChanged(EnergyChangedEvent evt)
        {
            // 현재 턴인 캐릭터의 에너지만 표시
            if (evt.CharacterId == _currentTurnEntityId)
            {
                UpdateEnergyText(evt.CurrentEnergy, evt.MaxEnergy);
            }
        }

        #endregion

        #region UI Updates

        private void UpdateRoundText()
        {
            if (_roundText != null)
            {
                _roundText.text = $"라운드: {_currentRound}";
            }
        }

        private void UpdateTurnIndicator()
        {
            if (_turnIndicatorText != null)
            {
                string entityName = GetEntityDisplayName(_currentTurnEntityId);
                string turnType = _isPlayerTurn ? "플레이어" : "적";
                _turnIndicatorText.text = $"{entityName}의 턴 ({turnType})";
            }
        }

        private void UpdateEnergyText(int current, int max)
        {
            if (_energyText != null)
            {
                _energyText.text = $"에너지: {current}/{max}";
            }
        }

        private string GetEntityDisplayName(string entityId)
        {
            // CombatManager에서 엔티티 이름 조회
            if (CombatManager.Instance != null)
            {
                foreach (var member in CombatManager.Instance.PlayerParty)
                {
                    if (member.EntityId == entityId) return member.DisplayName;
                }
                foreach (var enemy in CombatManager.Instance.Enemies)
                {
                    if (enemy.EntityId == entityId) return enemy.DisplayName;
                }
            }
            return entityId;
        }

        #endregion

        #region Entity Status UI

        private void CreateEntityStatusUIs()
        {
            if (CombatManager.Instance == null) return;

            // 파티원 상태 UI 생성
            if (_partyContainer != null)
            {
                ClearContainer(_partyContainer);
                foreach (var member in CombatManager.Instance.PlayerParty)
                {
                    CreateEntityStatusUI(member, _partyContainer, false);
                }
            }

            // 적 상태 UI 생성
            if (_enemyContainer != null)
            {
                ClearContainer(_enemyContainer);
                foreach (var enemy in CombatManager.Instance.Enemies)
                {
                    CreateEntityStatusUI(enemy, _enemyContainer, true);
                }
            }
        }

        private void CreateEntityStatusUI(ICombatEntity entity, Transform container, bool isEnemy)
        {
            if (_entityStatusPrefab != null)
            {
                var go = Instantiate(_entityStatusPrefab, container);
                var statusUI = go.GetComponent<EntityStatusUI>();
                if (statusUI != null)
                {
                    statusUI.Initialize(entity, isEnemy);
                }
            }
            else
            {
                // 프리팹이 없으면 간단한 UI 동적 생성
                CreateSimpleEntityUI(entity, container, isEnemy);
            }
        }

        private void CreateSimpleEntityUI(ICombatEntity entity, Transform container, bool isEnemy)
        {
            var go = new GameObject(entity.DisplayName + "_Status");
            go.transform.SetParent(container, false);

            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 120);

            // EntityStatusUI 컴포넌트 추가
            var statusUI = go.AddComponent<EntityStatusUI>();
            statusUI.CreateSimpleUI(entity, isEnemy);
        }

        private void ClearContainer(Transform container)
        {
            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
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
            if (CombatManager.Instance == null) return null;

            foreach (var member in CombatManager.Instance.PlayerParty)
            {
                if (member.EntityId == _currentTurnEntityId)
                {
                    return member;
                }
            }
            return null;
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

        #endregion
    }
}
