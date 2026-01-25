using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Map;
using ProjectSS.Combat;

namespace ProjectSS.Run
{
    /// <summary>
    /// 런 관리자 (로그라이크 한 판 전체 관리)
    /// Run manager (manages entire roguelike run)
    ///
    /// TRIAD: 파티 시스템 통합
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [Header("Initial Setup (Legacy)")]
        [SerializeField] private List<CardData> starterDeck;
        [SerializeField] private RelicData starterRelic;
        [SerializeField] private int startHP = 80;
        [SerializeField] private int startGold = 99;

        [Header("TRIAD Party Setup")]
        [Tooltip("파티 모드 활성화 / Enable party mode")]
        [SerializeField] private bool usePartyMode = true;

        [Tooltip("워리어 클래스 데이터 / Warrior class data")]
        [SerializeField] private CharacterClassData warriorClassData;

        [Tooltip("메이지 클래스 데이터 / Mage class data")]
        [SerializeField] private CharacterClassData mageClassData;

        [Tooltip("로그 클래스 데이터 / Rogue class data")]
        [SerializeField] private CharacterClassData rogueClassData;

        private RunState currentRunState;
        private PlayerState playerState;
        private FloorManager floorManager;

        public RunState CurrentRun => currentRunState;
        public PlayerState Player => playerState;
        public bool IsRunActive { get; private set; }

        /// <summary>
        /// TRIAD: 파티 모드 여부
        /// Is party mode active
        /// </summary>
        public bool IsPartyMode => currentRunState?.isPartyMode ?? usePartyMode;

        /// <summary>
        /// TRIAD: 파티 상태 (파티 모드일 때)
        /// Party state (in party mode)
        /// </summary>
        public PartyState PartyState => currentRunState?.partyState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // TRIAD: 이벤트 구독
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                UnsubscribeFromEvents();
                Instance = null;
            }
        }

        #region TRIAD Event Handling

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<TagInEvent>(OnTagInEvent);
            EventBus.Subscribe<EnemyBrokenEvent>(OnEnemyBrokenEvent);
            EventBus.Subscribe<CombatPerformanceEvent>(OnCombatPerformanceEvent);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<TagInEvent>(OnTagInEvent);
            EventBus.Unsubscribe<EnemyBrokenEvent>(OnEnemyBrokenEvent);
            EventBus.Unsubscribe<CombatPerformanceEvent>(OnCombatPerformanceEvent);
        }

        private void OnTagInEvent(TagInEvent evt)
        {
            OnTagIn();
        }

        private void OnEnemyBrokenEvent(EnemyBrokenEvent evt)
        {
            OnEnemyBroken();
        }

        private void OnCombatPerformanceEvent(CombatPerformanceEvent evt)
        {
            if (currentRunState == null) return;

            currentRunState.speedBonus += evt.SpeedBonus;
            currentRunState.impactBonus += evt.ImpactBonus;
            currentRunState.riskBonus += evt.RiskBonus;

            // 총 성과 보너스 골드
            int bonusGold = evt.TotalBonus / 10;
            if (bonusGold > 0)
            {
                playerState?.GainGold(bonusGold);
            }
        }

        #endregion

        /// <summary>
        /// 새 런 시작
        /// Start new run
        /// </summary>
        public void StartNewRun(int? seed = null)
        {
            int runSeed = seed ?? System.Environment.TickCount;

            currentRunState = new RunState
            {
                runSeed = runSeed,
                mapSeed = runSeed,
                isPartyMode = usePartyMode,
                currentHP = startHP,
                maxHP = startHP,
                gold = startGold
            };

            if (usePartyMode)
            {
                // TRIAD 파티 모드 초기화
                InitializePartyMode();
            }
            else
            {
                // 레거시 모드
                InitializeLegacyMode();
            }

            // 초기 유물 설정
            if (starterRelic != null)
            {
                currentRunState.AddRelic(starterRelic);
            }

            // 맵 생성
            if (MapManager.Instance != null)
            {
                MapManager.Instance.GenerateNewMap(runSeed);
                floorManager = new FloorManager(currentRunState, MapManager.Instance);
            }

            IsRunActive = true;
            EventBus.Publish(new RunStartedEvent());
        }

        /// <summary>
        /// TRIAD: 파티 모드 초기화
        /// Initialize party mode
        /// </summary>
        private void InitializePartyMode()
        {
            currentRunState.partyState = new PartyState();

            // 클래스 데이터로 파티 초기화
            if (warriorClassData != null && mageClassData != null && rogueClassData != null)
            {
                currentRunState.partyState.Initialize(warriorClassData, mageClassData, rogueClassData);
            }
            else
            {
                // 클래스 데이터가 없으면 기본값 사용
                Debug.LogWarning("[RunManager] Character class data not set. Using defaults.");
            }

            // 레거시 PlayerState도 호환을 위해 초기화 (totalHP = 파티 전체 체력)
            int totalPartyHP = (warriorClassData?.baseMaxHP ?? 75) +
                               (mageClassData?.baseMaxHP ?? 75) +
                               (rogueClassData?.baseMaxHP ?? 75);
            playerState = new PlayerState(totalPartyHP, startGold);

            // 레거시 유물
            if (starterRelic != null)
            {
                playerState.AddRelic(starterRelic);
            }
        }

        /// <summary>
        /// 레거시 모드 초기화
        /// Initialize legacy mode
        /// </summary>
        private void InitializeLegacyMode()
        {
            // 초기 덱 설정
            if (starterDeck != null && starterDeck.Count > 0)
            {
                currentRunState.SetInitialDeck(starterDeck);
            }

            // 플레이어 상태 초기화
            playerState = new PlayerState(startHP, startGold);
            foreach (var card in starterDeck)
            {
                playerState.AddCard(card);
            }
            if (starterRelic != null)
            {
                playerState.AddRelic(starterRelic);
            }
        }

        /// <summary>
        /// 런 로드
        /// Load run
        /// </summary>
        public void LoadRun(RunState savedState, CardDatabase cardDb, RelicDatabase relicDb)
        {
            currentRunState = savedState.Clone();

            playerState = new PlayerState();
            playerState.LoadFromRunState(currentRunState, cardDb, relicDb);

            // 맵 로드
            if (MapManager.Instance != null)
            {
                MapManager.Instance.GenerateNewMap(savedState.mapSeed);
                // TODO: 방문한 노드들 복원
                floorManager = new FloorManager(currentRunState, MapManager.Instance);
            }

            IsRunActive = true;
        }

        /// <summary>
        /// 런 저장
        /// Save run
        /// </summary>
        public RunState SaveRun()
        {
            if (playerState != null)
            {
                playerState.SaveToRunState(currentRunState);
            }
            return currentRunState.Clone();
        }

        /// <summary>
        /// 런 종료
        /// End run
        /// </summary>
        public void EndRun(bool victory)
        {
            IsRunActive = false;
            EventBus.Publish(new RunEndedEvent(victory));
        }

        /// <summary>
        /// 전투 완료 처리
        /// Handle combat completion
        /// </summary>
        public void OnCombatComplete(bool victory, int goldReward)
        {
            if (!victory)
            {
                EndRun(false);
                return;
            }

            currentRunState.combatCount++;
            playerState.GainGold(goldReward);

            // TRIAD: 파티 모드 시 전투 후 회복 (기절 해제)
            if (IsPartyMode && currentRunState.partyState != null)
            {
                currentRunState.partyState.FullRecovery();
            }

            // 노드 완료
            floorManager?.CompleteCurrentNode();

            // 보스 클리어 시 런 종료
            if (floorManager != null && floorManager.IsAtBoss())
            {
                currentRunState.bossKills++;
                EndRun(true);
            }
        }

        #region TRIAD Performance Tracking

        /// <summary>
        /// TRIAD: Tag-In 기록
        /// Record Tag-In
        /// </summary>
        public void OnTagIn()
        {
            if (currentRunState != null)
            {
                currentRunState.totalTagIns++;
            }
        }

        /// <summary>
        /// TRIAD: Break 기록
        /// Record Break
        /// </summary>
        public void OnEnemyBroken()
        {
            if (currentRunState != null)
            {
                currentRunState.totalBreaks++;
            }
        }

        /// <summary>
        /// TRIAD: 성과 보너스 추가
        /// Add performance bonus
        /// </summary>
        public void AddPerformanceBonus(int speed, int impact, int risk)
        {
            if (currentRunState == null) return;

            currentRunState.speedBonus += speed;
            currentRunState.impactBonus += impact;
            currentRunState.riskBonus += risk;

            // 골드 보상으로 변환 (10포인트당 1골드)
            int bonusGold = (speed + impact + risk) / 10;
            if (bonusGold > 0)
            {
                playerState?.GainGold(bonusGold);
            }
        }

        #endregion

        /// <summary>
        /// 엘리트 처치 기록
        /// Record elite kill
        /// </summary>
        public void OnEliteKill()
        {
            currentRunState.eliteKills++;
        }

        /// <summary>
        /// 휴식 처리
        /// Handle rest
        /// </summary>
        public void Rest(bool heal)
        {
            if (heal)
            {
                int healAmount = Mathf.RoundToInt(playerState.MaxHP * 0.3f);
                playerState.Heal(healAmount);
                currentRunState.currentHP = playerState.CurrentHP;
            }
        }

        /// <summary>
        /// 노드 선택 처리
        /// Handle node selection
        /// </summary>
        public void OnNodeSelected(MapNode node)
        {
            floorManager?.OnNodeSelected(node);
        }

        /// <summary>
        /// 현재 런 통계
        /// Get run statistics
        /// </summary>
        public RunStatistics GetStatistics()
        {
            int deckSize = IsPartyMode
                ? currentRunState?.partyState?.deckCardIds.Count ?? 0
                : playerState?.Deck.Count ?? 0;

            return new RunStatistics
            {
                Floor = currentRunState?.currentFloor ?? 0,
                TurnCount = currentRunState?.turnCount ?? 0,
                CombatCount = currentRunState?.combatCount ?? 0,
                EliteKills = currentRunState?.eliteKills ?? 0,
                BossKills = currentRunState?.bossKills ?? 0,
                Gold = playerState?.Gold ?? 0,
                DeckSize = deckSize,
                RelicCount = playerState?.Relics.Count ?? 0,

                // TRIAD Stats
                IsPartyMode = IsPartyMode,
                SpeedBonus = currentRunState?.speedBonus ?? 0,
                ImpactBonus = currentRunState?.impactBonus ?? 0,
                RiskBonus = currentRunState?.riskBonus ?? 0,
                TotalTagIns = currentRunState?.totalTagIns ?? 0,
                TotalBreaks = currentRunState?.totalBreaks ?? 0
            };
        }

        /// <summary>
        /// TRIAD: 전투용 덱 데이터 반환
        /// Get combined deck for combat (party mode)
        /// </summary>
        public List<CardData> GetCombatDeck(CardDatabase cardDb)
        {
            var combatDeck = new List<CardData>();

            if (IsPartyMode && currentRunState?.partyState != null)
            {
                var partyState = currentRunState.partyState;
                for (int i = 0; i < partyState.deckCardIds.Count; i++)
                {
                    var card = cardDb?.GetCard(partyState.deckCardIds[i]);
                    if (card != null)
                    {
                        // 업그레이드 상태 확인
                        if (partyState.deckUpgraded[i] && card.upgradedVersion != null)
                        {
                            combatDeck.Add(card.upgradedVersion);
                        }
                        else
                        {
                            combatDeck.Add(card);
                        }
                    }
                }
            }
            else if (playerState != null)
            {
                combatDeck = playerState.GetDeckForCombat();
            }

            return combatDeck;
        }

        /// <summary>
        /// TRIAD: 클래스 데이터 반환
        /// Get class data by type
        /// </summary>
        public CharacterClassData GetClassData(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => warriorClassData,
                CharacterClass.Mage => mageClassData,
                CharacterClass.Rogue => rogueClassData,
                _ => null
            };
        }

        /// <summary>
        /// TRIAD: 모든 클래스 데이터 반환
        /// Get all class data
        /// </summary>
        public CharacterClassData[] GetAllClassData()
        {
            return new[] { warriorClassData, mageClassData, rogueClassData };
        }
    }

    /// <summary>
    /// 런 통계
    /// Run statistics
    /// </summary>
    public struct RunStatistics
    {
        public int Floor;
        public int TurnCount;
        public int CombatCount;
        public int EliteKills;
        public int BossKills;
        public int Gold;
        public int DeckSize;
        public int RelicCount;

        // TRIAD Stats
        public bool IsPartyMode;
        public int SpeedBonus;
        public int ImpactBonus;
        public int RiskBonus;
        public int TotalTagIns;
        public int TotalBreaks;

        public int TotalPerformanceBonus => SpeedBonus + ImpactBonus + RiskBonus;
    }

}
