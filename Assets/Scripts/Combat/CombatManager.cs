// Combat/CombatManager.cs
// 전투 관리자 - 전투 상태 머신 및 흐름 제어

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Events;
using ProjectSS.Services;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 전투 관리자
    /// 전투 상태 머신 및 전체 흐름 제어
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [Header("Combat State")]
        [SerializeField] private CombatState _currentState = CombatState.NotInCombat;

        [Header("References")]
        [SerializeField] private TurnManager _turnManager;

        [Header("Entities")]
        [SerializeField] private List<PartyMemberCombat> _playerParty = new();
        [SerializeField] private List<EnemyCombat> _enemies = new();

        [Header("Settings")]
        [SerializeField] private float _turnDelay = 0.5f;
        [SerializeField] private float _enemyActionDelay = 1.0f;

        // 현재 전투 데이터
        private TileType _encounterType;

        // 프로퍼티
        public CombatState CurrentState => _currentState;
        public TurnManager TurnManager => _turnManager;
        public IReadOnlyList<PartyMemberCombat> PlayerParty => _playerParty;
        public IReadOnlyList<EnemyCombat> Enemies => _enemies;
        public bool IsPlayerTurn => _turnManager?.IsPlayerTurn ?? false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // DataService에서 기본값 로드
            InitializeFromConfig();

            // TurnManager 확인
            if (_turnManager == null)
            {
                _turnManager = GetComponent<TurnManager>();
                if (_turnManager == null)
                {
                    _turnManager = gameObject.AddComponent<TurnManager>();
                }
            }

            // 서비스 등록
            ServiceLocator.Register(this);
        }

        /// <summary>
        /// DataService에서 기본값 로드
        /// </summary>
        private void InitializeFromConfig()
        {
            var balance = DataService.Instance?.Balance;
            if (balance == null) return;

            // SerializeField 기본값이면 Config에서 로드
            if (_turnDelay == 0.5f)
            {
                _turnDelay = balance.TurnDelay;
            }
            if (_enemyActionDelay == 1.0f)
            {
                _enemyActionDelay = balance.EnemyActionDelay;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<CombatManager>();
                Instance = null;
            }
        }

        #region Combat Flow

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartCombat(TileType encounterType, List<PartyMemberCombat> party, List<EnemyCombat> enemies)
        {
            Debug.Log($"[CombatManager] Starting combat: {encounterType}");

            _encounterType = encounterType;
            _playerParty = party;
            _enemies = enemies;

            SetState(CombatState.Initializing);

            // 전투 시작 이벤트
            EventBus.Publish(new CombatStartedEvent { EncounterType = encounterType });

            // 초기화 후 전투 루프 시작
            StartCoroutine(CombatLoop());
        }

        /// <summary>
        /// 전투 메인 루프
        /// </summary>
        private IEnumerator CombatLoop()
        {
            // 초기화
            InitializeCombat();
            yield return new WaitForSeconds(_turnDelay);

            // 전투 루프
            while (!IsCombatOver())
            {
                // 새 라운드 시작
                SetState(CombatState.RoundStart);
                _turnManager.StartNewRound();
                yield return new WaitForSeconds(_turnDelay);

                // 라운드 내 턴 루프
                while (!_turnManager.IsRoundComplete() && !IsCombatOver())
                {
                    // 다음 턴 시작
                    SetState(CombatState.TurnStart);
                    var currentEntity = _turnManager.StartNextTurn();

                    if (currentEntity == null)
                    {
                        break;
                    }

                    yield return new WaitForSeconds(_turnDelay);

                    // 플레이어 턴 또는 적 턴 처리
                    if (currentEntity.IsPlayerCharacter)
                    {
                        SetState(CombatState.PlayerTurn);
                        yield return StartCoroutine(HandlePlayerTurn(currentEntity as PartyMemberCombat));
                    }
                    else
                    {
                        SetState(CombatState.EnemyTurn);
                        yield return StartCoroutine(HandleEnemyTurn(currentEntity as EnemyCombat));
                    }

                    // 턴 종료
                    SetState(CombatState.TurnEnd);
                    _turnManager.EndCurrentTurn();
                    yield return new WaitForSeconds(_turnDelay);
                }

                // 라운드 종료
                SetState(CombatState.RoundEnd);
                _turnManager.EndRound();
                yield return new WaitForSeconds(_turnDelay);
            }

            // 전투 종료 처리
            EndCombat();
        }

        /// <summary>
        /// 전투 초기화
        /// </summary>
        private void InitializeCombat()
        {
            Debug.Log("[CombatManager] Initializing combat...");

            // 모든 엔티티 수집
            var allEntities = new List<ICombatEntity>();
            allEntities.AddRange(_playerParty);
            allEntities.AddRange(_enemies);

            // 턴 순서 초기화
            _turnManager.InitializeTurnOrder(allEntities);
        }

        /// <summary>
        /// 플레이어 턴 처리
        /// </summary>
        private IEnumerator HandlePlayerTurn(PartyMemberCombat character)
        {
            Debug.Log($"[CombatManager] {character.DisplayName}'s turn (Player)");

            // 플레이어 입력 대기
            // UI에서 EndTurn()이 호출될 때까지 대기
            _waitingForPlayerInput = true;

            while (_waitingForPlayerInput && !IsCombatOver())
            {
                yield return null;
            }
        }

        /// <summary>
        /// 적 턴 처리
        /// </summary>
        private IEnumerator HandleEnemyTurn(EnemyCombat enemy)
        {
            Debug.Log($"[CombatManager] {enemy.DisplayName}'s turn (Enemy)");

            yield return new WaitForSeconds(_enemyActionDelay);

            // 의도 실행
            enemy.ExecuteIntent(_playerParty.ToArray());

            yield return new WaitForSeconds(_turnDelay);
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        private void EndCombat()
        {
            bool victory = _turnManager.IsEnemyTeamDefeated();

            Debug.Log($"[CombatManager] Combat ended. Victory: {victory}");

            SetState(victory ? CombatState.Victory : CombatState.Defeat);

            EventBus.Publish(new CombatEndedEvent { Victory = victory });

            // 맵으로 복귀 또는 게임 오버 처리
            if (victory)
            {
                // 승리 - 보상 처리 후 맵으로
                StartCoroutine(ReturnToMapAfterDelay(2f));
            }
            else
            {
                // 패배 - 런 종료
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.EndRun(false);
                }
            }
        }

        private IEnumerator ReturnToMapAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMap();
            }
        }

        #endregion

        #region Combat State

        private bool _waitingForPlayerInput = false;

        /// <summary>
        /// 상태 변경
        /// </summary>
        private void SetState(CombatState newState)
        {
            if (_currentState != newState)
            {
                Debug.Log($"[CombatManager] State: {_currentState} -> {newState}");
                _currentState = newState;
            }
        }

        /// <summary>
        /// 전투 종료 조건 확인
        /// </summary>
        private bool IsCombatOver()
        {
            return _turnManager.IsPlayerTeamDefeated() || _turnManager.IsEnemyTeamDefeated();
        }

        #endregion

        #region Player Actions

        /// <summary>
        /// 플레이어 턴 종료 (UI에서 호출)
        /// </summary>
        public void EndPlayerTurn()
        {
            if (_currentState == CombatState.PlayerTurn && _waitingForPlayerInput)
            {
                Debug.Log("[CombatManager] Player ended turn");
                _waitingForPlayerInput = false;
            }
        }

        /// <summary>
        /// 카드 사용 (UI에서 호출)
        /// </summary>
        public bool TryPlayCard(PartyMemberCombat character, CardInstance card, ICombatEntity target)
        {
            if (_currentState != CombatState.PlayerTurn)
            {
                Debug.Log("[CombatManager] Not player turn!");
                return false;
            }

            if (_turnManager.CurrentEntity != character)
            {
                Debug.Log("[CombatManager] Not this character's turn!");
                return false;
            }

            return character.PlayCard(card, target);
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 현재 턴의 캐릭터 가져오기
        /// </summary>
        public ICombatEntity GetCurrentTurnEntity()
        {
            return _turnManager?.CurrentEntity;
        }

        /// <summary>
        /// 생존한 적 목록
        /// </summary>
        public List<EnemyCombat> GetAliveEnemies()
        {
            return _enemies.FindAll(e => e.IsAlive);
        }

        /// <summary>
        /// 생존한 파티원 목록
        /// </summary>
        public List<PartyMemberCombat> GetAlivePartyMembers()
        {
            return _playerParty.FindAll(p => p.IsAlive);
        }

        #endregion
    }
}
