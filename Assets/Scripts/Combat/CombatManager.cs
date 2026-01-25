using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Utility;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 전투 관리자
    /// Combat manager - orchestrates all combat systems
    ///
    /// TRIAD: PartyManager 통합으로 3인 파티 시스템 지원
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        public static CombatManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private EnemyCombat enemyPrefab;
        [SerializeField] private Transform[] enemySpawnPoints;

        private PartyManager partyManager;
        private List<EnemyCombat> enemies = new List<EnemyCombat>();
        private TurnManager turnManager;
        private DeckManager deckManager;
        private EnergySystem energySystem;
        private CardEffectResolver effectResolver;
        private SeededRandom random;

        /// <summary>
        /// TRIAD: 성과 시스템
        /// Performance tracking system
        /// </summary>
        private PerformanceSystem performanceSystem;

        /// <summary>
        /// TRIAD: 파티 매니저 (기존 Player 대체)
        /// TRIAD: Party manager (replaces Player)
        /// </summary>
        public PartyManager Party => partyManager;

        /// <summary>
        /// TRIAD: 현재 Active 캐릭터 (하위 호환성)
        /// TRIAD: Current Active character (backward compatibility)
        /// </summary>
        public PartyMemberCombat ActiveMember => partyManager?.Active;

        public IReadOnlyList<EnemyCombat> Enemies => enemies;
        public TurnManager TurnManager => turnManager;
        public DeckManager DeckManager => deckManager;
        public EnergySystem EnergySystem => energySystem;
        public bool IsCombatActive { get; private set; }

        /// <summary>
        /// TRIAD: 성과 시스템 접근
        /// Access performance system
        /// </summary>
        public PerformanceSystem Performance => performanceSystem;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// TRIAD: 전투 초기화 (파티 시스템)
        /// TRIAD: Initialize combat with party system
        /// </summary>
        /// <param name="enemyDatas">적 데이터 목록 / Enemy data list</param>
        /// <param name="partyState">파티 상태 / Party state</param>
        /// <param name="combinedDeck">통합 덱 (3캐릭터 스타터) / Combined deck</param>
        /// <param name="seed">랜덤 시드 / Random seed</param>
        public void InitializeCombat(List<EnemyData> enemyDatas, PartyState partyState, List<CardData> combinedDeck, int seed)
        {
            random = new SeededRandom(seed);

            // 시스템 초기화
            energySystem = new EnergySystem(3);
            deckManager = new DeckManager(random);
            deckManager.InitializeDeck(combinedDeck);

            // TRIAD: 파티 매니저 초기화
            // Initialize party manager
            InitializeParty(partyState);

            // TRIAD: 성과 시스템 초기화
            // Initialize performance system
            InitializePerformanceSystem(enemyDatas);

            // 적 생성
            SpawnEnemies(enemyDatas);

            // TRIAD: 턴 매니저에 PartyManager 전달
            // Pass PartyManager to TurnManager
            turnManager = new TurnManager(partyManager, enemies, energySystem, deckManager);

            // 효과 해결사 초기화
            effectResolver = new CardEffectResolver(deckManager, energySystem);

            // TRIAD: Tag-In 이벤트 구독
            // Subscribe to Tag-In events
            SubscribeToTagInEvents();

            IsCombatActive = true;
            EventBus.Publish(new CombatStartedEvent());

            // 전투 시작
            turnManager.StartCombat();
        }

        /// <summary>
        /// TRIAD: 파티 매니저 초기화
        /// TRIAD: Initialize party manager
        /// </summary>
        private void InitializeParty(PartyState partyState)
        {
            partyManager = PartyManager.Instance;

            if (partyManager == null)
            {
                Debug.LogError("[CombatManager] PartyManager.Instance is null!");
                return;
            }

            partyManager.Initialize(partyState, energySystem);
        }

        /// <summary>
        /// TRIAD: Tag-In 이벤트 구독
        /// TRIAD: Subscribe to Tag-In events
        /// </summary>
        private void SubscribeToTagInEvents()
        {
            EventBus.Subscribe<TagInDrawCardEvent>(OnTagInDrawCard);
            EventBus.Subscribe<TagInApplyDebuffEvent>(OnTagInApplyDebuff);
            EventBus.Subscribe<TagInEvent>(OnTagIn);
            EventBus.Subscribe<CharacterIncapacitatedEvent>(OnCharacterIncapacitated);
            EventBus.Subscribe<EnemyBrokenEvent>(OnEnemyBroken);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        }

        private void UnsubscribeFromTagInEvents()
        {
            EventBus.Unsubscribe<TagInDrawCardEvent>(OnTagInDrawCard);
            EventBus.Unsubscribe<TagInApplyDebuffEvent>(OnTagInApplyDebuff);
            EventBus.Unsubscribe<TagInEvent>(OnTagIn);
            EventBus.Unsubscribe<CharacterIncapacitatedEvent>(OnCharacterIncapacitated);
            EventBus.Unsubscribe<EnemyBrokenEvent>(OnEnemyBroken);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        }

        private void OnTagIn(TagInEvent evt)
        {
            // RunManager가 TagInEvent를 직접 구독하여 처리
            // (RunManager subscribes to TagInEvent directly)
        }

        private void OnCharacterIncapacitated(CharacterIncapacitatedEvent evt)
        {
            // 성과 시스템에 기절 기록
            performanceSystem?.RecordIncapacitation();
        }

        private void OnEnemyBroken(EnemyBrokenEvent evt)
        {
            // RunManager가 EnemyBrokenEvent를 직접 구독하여 처리
            // (RunManager subscribes to EnemyBrokenEvent directly)
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            // 플레이어 턴 시작 시에만 턴 카운트
            if (evt.IsPlayerTurn)
            {
                performanceSystem?.RecordTurn();
            }
        }

        private void OnTagInDrawCard(TagInDrawCardEvent evt)
        {
            if (deckManager != null)
            {
                deckManager.DrawCards(evt.DrawCount);
            }
        }

        private void OnTagInApplyDebuff(TagInApplyDebuffEvent evt)
        {
            if (evt.StatusEffect == null) return;

            // 모든 적에게 디버프 적용
            // Apply debuff to all enemies
            foreach (var enemy in GetAliveEnemies())
            {
                enemy.GetStatusEffects().ApplyStatus(evt.StatusEffect, evt.Stacks);
            }
        }

        /// <summary>
        /// TRIAD: 성과 시스템 초기화
        /// Initialize performance system
        /// </summary>
        private void InitializePerformanceSystem(List<EnemyData> enemyDatas)
        {
            performanceSystem = new PerformanceSystem();

            // 가장 높은 등급의 적 타입으로 설정
            EnemyType highestType = EnemyType.Normal;
            foreach (var enemyData in enemyDatas)
            {
                if (enemyData.enemyType > highestType)
                {
                    highestType = enemyData.enemyType;
                }
            }

            performanceSystem.Initialize(highestType, enemyDatas.Count);
        }

        private void SpawnEnemies(List<EnemyData> enemyDatas)
        {
            enemies.Clear();

            for (int i = 0; i < enemyDatas.Count; i++)
            {
                Vector3 spawnPos = Vector3.zero;
                if (enemySpawnPoints != null && i < enemySpawnPoints.Length)
                {
                    spawnPos = enemySpawnPoints[i].position;
                }
                else
                {
                    spawnPos = new Vector3(3 + i * 2, 0, 0);
                }

                EnemyCombat enemy;
                if (enemyPrefab != null)
                {
                    enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    var go = new GameObject($"Enemy_{enemyDatas[i].enemyName}");
                    go.transform.position = spawnPos;
                    enemy = go.AddComponent<EnemyCombat>();
                }

                enemy.Initialize(enemyDatas[i], random);
                enemies.Add(enemy);
            }
        }

        /// <summary>
        /// TRIAD: 카드 사용 (Active 캐릭터 기반)
        /// TRIAD: Play a card (Active character based)
        /// </summary>
        public bool PlayCard(CardInstance card, ICombatEntity target = null)
        {
            if (!IsCombatActive || !turnManager.IsPlayerTurn) return false;

            // TRIAD: Active 캐릭터 확인
            // Get Active character
            var active = partyManager?.Active;
            if (active == null || !active.CanAct)
            {
                Debug.LogWarning("[CombatManager] No active party member can act!");
                return false;
            }

            // 기본 카드 플레이 체크 (에너지, 타겟)
            if (!card.CanPlay(energySystem, target)) return false;

            // TRIAD: 클래스 제한 및 Focus 비용 체크
            // Check class restriction and Focus cost
            if (!active.CanUseCard(card.Data))
            {
                Debug.LogWarning($"[CombatManager] Active member {active.Class} cannot use card {card.Data.cardName}");
                return false;
            }

            // 에너지 소모
            energySystem.Spend(card.EffectiveData.energyCost);

            // TRIAD: Focus 비용 소모
            // Consume Focus cost
            if (card.Data.focusCost > 0)
            {
                active.ConsumeFocus(card.Data.focusCost);
            }

            // TRIAD: Active 캐릭터를 source로 효과 해결
            // Resolve effects with Active as source
            effectResolver.ResolveCard(card, active, target, enemies);

            // 카드 처리 (버리기 또는 소멸)
            deckManager.OnCardPlayed(card);

            // 전투 종료 체크
            CheckCombatEnd();

            return true;
        }

        /// <summary>
        /// TRIAD: Tag-In 실행
        /// TRIAD: Execute Tag-In
        /// </summary>
        /// <param name="newActive">새 Active 캐릭터 / New Active character</param>
        /// <returns>성공 여부 / Success</returns>
        public bool ExecuteTagIn(PartyMemberCombat newActive)
        {
            if (!IsCombatActive || !turnManager.IsPlayerTurn) return false;

            return partyManager.ExecuteTagIn(newActive);
        }

        /// <summary>
        /// 턴 종료 버튼
        /// End turn button
        /// </summary>
        public void OnEndTurnClicked()
        {
            if (!IsCombatActive || !turnManager.IsPlayerTurn) return;

            turnManager.EndPlayerTurn();
            StartCoroutine(ExecuteEnemyTurnCoroutine());
        }

        private IEnumerator ExecuteEnemyTurnCoroutine()
        {
            yield return turnManager.ExecuteEnemyActions();
            CheckCombatEnd();
        }

        /// <summary>
        /// 전투 종료 체크
        /// Check combat end conditions
        ///
        /// TRIAD: 파티 전멸 체크로 변경
        /// </summary>
        private void CheckCombatEnd()
        {
            if (!IsCombatActive) return;

            // TRIAD: 파티 전멸 체크
            // Check if party is wiped
            if (partyManager.IsPartyWiped())
            {
                EndCombat(false);
                return;
            }

            // 모든 적 사망 체크
            bool allEnemiesDead = true;
            foreach (var enemy in enemies)
            {
                if (enemy.IsAlive)
                {
                    allEnemiesDead = false;
                    break;
                }
            }

            if (allEnemiesDead)
            {
                EndCombat(true);
            }
        }

        /// <summary>
        /// 전투 종료
        /// End combat
        ///
        /// TRIAD: 승리 시 파티 회복 및 성과 처리
        /// </summary>
        private void EndCombat(bool playerWon)
        {
            IsCombatActive = false;
            turnManager.EndCombat();

            // TRIAD: 이벤트 구독 해제
            UnsubscribeFromTagInEvents();

            // TRIAD: 성과 처리 - RunManager가 CombatPerformanceEvent 구독하여 처리
            if (playerWon && performanceSystem != null)
            {
                var performanceData = performanceSystem.GeneratePerformanceData();

                // 성과 이벤트 발행 (RunManager가 구독)
                EventBus.Publish(new CombatPerformanceEvent(
                    performanceData.speedBonus,
                    performanceData.impactBonus,
                    performanceData.riskBonus,
                    performanceData.turnsUsed,
                    performanceData.overkillDamage,
                    performanceData.incapacitationCount
                ));
            }

            // TRIAD: 승리 시 파티 회복
            if (playerWon && partyManager != null)
            {
                partyManager.ReviveAll();
            }

            EventBus.Publish(new CombatEndedEvent(playerWon));
        }

        /// <summary>
        /// TRIAD: 적 처치 시 오버킬 기록
        /// Record overkill when enemy dies
        /// </summary>
        public void RecordEnemyOverkill(int overkillDamage)
        {
            performanceSystem?.RecordOverkill(overkillDamage);
        }

        /// <summary>
        /// 적 선택 (타겟팅용)
        /// Get enemy by index
        /// </summary>
        public EnemyCombat GetEnemy(int index)
        {
            if (index >= 0 && index < enemies.Count)
                return enemies[index];
            return null;
        }

        /// <summary>
        /// 살아있는 적 목록
        /// Get alive enemies
        /// </summary>
        public List<EnemyCombat> GetAliveEnemies()
        {
            return enemies.FindAll(e => e.IsAlive);
        }
    }
}
