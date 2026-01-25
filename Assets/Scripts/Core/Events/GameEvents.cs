namespace ProjectSS.Core
{
    #region Game Lifecycle Events

    /// <summary>
    /// 게임 상태 변경 이벤트
    /// Game state changed event
    /// </summary>
    public struct GameStateChangedEvent : IGameEvent
    {
        public GameState PreviousState { get; }
        public GameState NewState { get; }

        public GameStateChangedEvent(GameState previous, GameState newState)
        {
            PreviousState = previous;
            NewState = newState;
        }
    }

    /// <summary>
    /// 게임 일시정지 이벤트
    /// Game paused event
    /// </summary>
    public struct GamePausedEvent : IGameEvent
    {
        public bool IsPaused { get; }

        public GamePausedEvent(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }

    #endregion

    #region Run Events

    /// <summary>
    /// 런 초기화 요청 이벤트 (GameManager → RunManager)
    /// Run init requested event (GameManager → RunManager)
    /// </summary>
    public struct RunInitRequestedEvent : IGameEvent
    {
    }

    /// <summary>
    /// 런 시작 이벤트
    /// Run started event
    /// </summary>
    public struct RunStartedEvent : IGameEvent
    {
    }

    /// <summary>
    /// 런 종료 이벤트
    /// Run ended event
    /// </summary>
    public struct RunEndedEvent : IGameEvent
    {
        public bool Victory { get; }

        public RunEndedEvent(bool victory)
        {
            Victory = victory;
        }
    }

    #endregion

    #region Combat Events

    /// <summary>
    /// 전투 시작 이벤트
    /// Combat started event
    /// </summary>
    public struct CombatStartedEvent : IGameEvent
    {
    }

    /// <summary>
    /// 전투 종료 이벤트
    /// Combat ended event
    /// </summary>
    public struct CombatEndedEvent : IGameEvent
    {
        public bool PlayerWon { get; }

        public CombatEndedEvent(bool playerWon)
        {
            PlayerWon = playerWon;
        }
    }

    /// <summary>
    /// 턴 시작 이벤트
    /// Turn started event
    /// </summary>
    public struct TurnStartedEvent : IGameEvent
    {
        public bool IsPlayerTurn { get; }
        public int TurnNumber { get; }

        public TurnStartedEvent(bool isPlayerTurn, int turnNumber)
        {
            IsPlayerTurn = isPlayerTurn;
            TurnNumber = turnNumber;
        }
    }

    /// <summary>
    /// 턴 종료 이벤트
    /// Turn ended event
    /// </summary>
    public struct TurnEndedEvent : IGameEvent
    {
        public bool IsPlayerTurn { get; }

        public TurnEndedEvent(bool isPlayerTurn)
        {
            IsPlayerTurn = isPlayerTurn;
        }
    }

    #endregion

    #region Card Events

    /// <summary>
    /// 카드 드로우 이벤트
    /// Card drawn event
    /// </summary>
    public struct CardDrawnEvent : IGameEvent
    {
        public int CardCount { get; }

        public CardDrawnEvent(int count)
        {
            CardCount = count;
        }
    }

    /// <summary>
    /// 카드 사용 이벤트
    /// Card played event
    /// </summary>
    public struct CardPlayedEvent : IGameEvent
    {
        public string CardId { get; }

        public CardPlayedEvent(string cardId)
        {
            CardId = cardId;
        }
    }

    /// <summary>
    /// 카드 버림 이벤트
    /// Card discarded event
    /// </summary>
    public struct CardDiscardedEvent : IGameEvent
    {
        public string CardId { get; }

        public CardDiscardedEvent(string cardId)
        {
            CardId = cardId;
        }
    }

    #endregion

    #region Entity Events

    /// <summary>
    /// 데미지 받음 이벤트
    /// Damage taken event
    /// </summary>
    public struct DamageTakenEvent : IGameEvent
    {
        public string EntityId { get; }
        public int Amount { get; }
        public int CurrentHealth { get; }

        public DamageTakenEvent(string entityId, int amount, int currentHealth)
        {
            EntityId = entityId;
            Amount = amount;
            CurrentHealth = currentHealth;
        }
    }

    /// <summary>
    /// 블록 획득 이벤트
    /// Block gained event
    /// </summary>
    public struct BlockGainedEvent : IGameEvent
    {
        public string EntityId { get; }
        public int Amount { get; }

        public BlockGainedEvent(string entityId, int amount)
        {
            EntityId = entityId;
            Amount = amount;
        }
    }

    /// <summary>
    /// 상태이상 적용 이벤트
    /// Status effect applied event
    /// </summary>
    public struct StatusEffectAppliedEvent : IGameEvent
    {
        public string EntityId { get; }
        public string StatusId { get; }
        public int Stacks { get; }

        public StatusEffectAppliedEvent(string entityId, string statusId, int stacks)
        {
            EntityId = entityId;
            StatusId = statusId;
            Stacks = stacks;
        }
    }

    /// <summary>
    /// 엔티티 사망 이벤트
    /// Entity died event
    /// </summary>
    public struct EntityDiedEvent : IGameEvent
    {
        public string EntityId { get; }
        public bool IsPlayer { get; }

        public EntityDiedEvent(string entityId, bool isPlayer)
        {
            EntityId = entityId;
            IsPlayer = isPlayer;
        }
    }

    #endregion

    #region Map Events

    /// <summary>
    /// 맵 노드 선택 이벤트
    /// Map node selected event
    /// </summary>
    public struct MapNodeSelectedEvent : IGameEvent
    {
        public string NodeId { get; }
        public int Floor { get; }

        public MapNodeSelectedEvent(string nodeId, int floor)
        {
            NodeId = nodeId;
            Floor = floor;
        }
    }

    /// <summary>
    /// 맵 생성 요청 이벤트 (GameManager → MapManager)
    /// Map generation requested event (GameManager → MapManager)
    /// </summary>
    public struct MapGenerationRequestedEvent : IGameEvent
    {
        public int Seed { get; }
        public MapType MapType { get; }

        public MapGenerationRequestedEvent(int seed, MapType mapType)
        {
            Seed = seed;
            MapType = mapType;
        }
    }

    /// <summary>
    /// 맵 생성 완료 이벤트
    /// Map generated event
    /// </summary>
    public struct MapGeneratedEvent : IGameEvent
    {
        public int Seed { get; }
        public int FloorCount { get; }
        public MapType MapType { get; }

        public MapGeneratedEvent(int seed, int floorCount, MapType mapType = MapType.Field)
        {
            Seed = seed;
            FloorCount = floorCount;
            MapType = mapType;
        }
    }

    /// <summary>
    /// 맵 완료 이벤트 (필드 완료 → 던전 진입 등)
    /// Map completed event (field complete → enter dungeon, etc.)
    /// </summary>
    public struct MapCompletedEvent : IGameEvent
    {
        public MapType CompletedMapType { get; }

        public MapCompletedEvent(MapType completedMapType)
        {
            CompletedMapType = completedMapType;
        }
    }

    #endregion

    #region Region Events

    /// <summary>
    /// 지역 진입 이벤트
    /// Region entered event
    /// </summary>
    public struct RegionEnteredEvent : IGameEvent
    {
        public string RegionId { get; }
        public MapType MapType { get; }

        public RegionEnteredEvent(string regionId, MapType mapType)
        {
            RegionId = regionId;
            MapType = mapType;
        }
    }

    /// <summary>
    /// 지역 완료 이벤트
    /// Region completed event
    /// </summary>
    public struct RegionCompletedEvent : IGameEvent
    {
        public string RegionId { get; }
        public bool HasNextRegion { get; }

        public RegionCompletedEvent(string regionId, bool hasNextRegion)
        {
            RegionId = regionId;
            HasNextRegion = hasNextRegion;
        }
    }

    #endregion

    #region World Map Events

    /// <summary>
    /// 통합 월드맵 생성 완료 이벤트
    /// World map generated event
    /// </summary>
    public struct WorldMapGeneratedEvent : IGameEvent
    {
        public int Seed { get; }
        public int TotalNodeCount { get; }
        public int RegionCount { get; }

        public WorldMapGeneratedEvent(int seed, int totalNodeCount, int regionCount)
        {
            Seed = seed;
            TotalNodeCount = totalNodeCount;
            RegionCount = regionCount;
        }
    }

    /// <summary>
    /// 월드맵 노드 선택 이벤트 (지역 정보 포함)
    /// World map node selected event (includes region info)
    /// </summary>
    public struct WorldMapNodeSelectedEvent : IGameEvent
    {
        public string NodeId { get; }
        public string RegionId { get; }
        public int Floor { get; }
        public MapNodeType NodeType { get; }

        public WorldMapNodeSelectedEvent(string nodeId, string regionId, int floor, MapNodeType nodeType)
        {
            NodeId = nodeId;
            RegionId = regionId;
            Floor = floor;
            NodeType = nodeType;
        }
    }

    #endregion

    #region TRIAD Performance Events

    /// <summary>
    /// TRIAD: 전투 성과 이벤트
    /// Combat performance event
    /// </summary>
    public struct CombatPerformanceEvent : IGameEvent
    {
        public int SpeedBonus { get; }
        public int ImpactBonus { get; }
        public int RiskBonus { get; }
        public int TurnsUsed { get; }
        public int OverkillDamage { get; }
        public int IncapacitationCount { get; }
        public int TotalBonus { get; }

        public CombatPerformanceEvent(
            int speedBonus,
            int impactBonus,
            int riskBonus,
            int turnsUsed,
            int overkillDamage,
            int incapacitationCount)
        {
            SpeedBonus = speedBonus;
            ImpactBonus = impactBonus;
            RiskBonus = riskBonus;
            TurnsUsed = turnsUsed;
            OverkillDamage = overkillDamage;
            IncapacitationCount = incapacitationCount;
            TotalBonus = speedBonus + impactBonus + riskBonus;
        }
    }

    #endregion

    #region Resource Events

    /// <summary>
    /// 에너지 변경 이벤트
    /// Energy changed event
    /// </summary>
    public struct EnergyChangedEvent : IGameEvent
    {
        public int CurrentEnergy { get; }
        public int MaxEnergy { get; }

        public EnergyChangedEvent(int current, int max)
        {
            CurrentEnergy = current;
            MaxEnergy = max;
        }
    }

    /// <summary>
    /// 골드 변경 이벤트
    /// Gold changed event
    /// </summary>
    public struct GoldChangedEvent : IGameEvent
    {
        public int Amount { get; }
        public int Total { get; }

        public GoldChangedEvent(int amount, int total)
        {
            Amount = amount;
            Total = total;
        }
    }

    #endregion

    #region Event Scene Events

    /// <summary>
    /// 이벤트 씬 시작 이벤트
    /// Event scene started event
    /// </summary>
    public struct EventSceneStartedEvent : IGameEvent
    {
        public string EventId { get; }

        public EventSceneStartedEvent(string eventId)
        {
            EventId = eventId;
        }
    }

    /// <summary>
    /// 이벤트 선택지 선택 이벤트
    /// Event choice selected event
    /// </summary>
    public struct EventChoiceSelectedEvent : IGameEvent
    {
        public string EventId { get; }
        public int ChoiceIndex { get; }

        public EventChoiceSelectedEvent(string eventId, int choiceIndex)
        {
            EventId = eventId;
            ChoiceIndex = choiceIndex;
        }
    }

    /// <summary>
    /// 이벤트 완료 이벤트
    /// Event completed event
    /// </summary>
    public struct EventCompletedEvent : IGameEvent
    {
        public string EventId { get; }

        public EventCompletedEvent(string eventId)
        {
            EventId = eventId;
        }
    }

    #endregion

    #region Shop Scene Events

    /// <summary>
    /// 상점 진입 이벤트
    /// Shop entered event
    /// </summary>
    public struct ShopEnteredEvent : IGameEvent
    {
    }

    /// <summary>
    /// 아이템 구매 이벤트
    /// Item purchased event
    /// </summary>
    public struct ItemPurchasedEvent : IGameEvent
    {
        public string ItemId { get; }
        public int Cost { get; }
        public bool IsCard { get; }

        public ItemPurchasedEvent(string itemId, int cost, bool isCard)
        {
            ItemId = itemId;
            Cost = cost;
            IsCard = isCard;
        }
    }

    /// <summary>
    /// 상점 서비스 사용 이벤트
    /// Shop service used event
    /// </summary>
    public struct ShopServiceUsedEvent : IGameEvent
    {
        public string ServiceType { get; }

        public ShopServiceUsedEvent(string serviceType)
        {
            ServiceType = serviceType;
        }
    }

    #endregion

    #region Rest Scene Events

    /// <summary>
    /// 휴식 시작 이벤트
    /// Rest started event
    /// </summary>
    public struct RestStartedEvent : IGameEvent
    {
    }

    /// <summary>
    /// 휴식 힐 이벤트 (TRIAD 파티용)
    /// Rest healed event (for TRIAD party)
    /// </summary>
    public struct RestHealedEvent : IGameEvent
    {
        public int WarriorHeal { get; }
        public int MageHeal { get; }
        public int RogueHeal { get; }

        public RestHealedEvent(int warriorHeal, int mageHeal, int rogueHeal)
        {
            WarriorHeal = warriorHeal;
            MageHeal = mageHeal;
            RogueHeal = rogueHeal;
        }
    }

    /// <summary>
    /// 카드 업그레이드 이벤트 (휴식/대장장이)
    /// Card upgraded event (rest/smith)
    /// </summary>
    public struct CardUpgradedEvent : IGameEvent
    {
        public string CardId { get; }

        public CardUpgradedEvent(string cardId)
        {
            CardId = cardId;
        }
    }

    /// <summary>
    /// 휴식 완료 이벤트
    /// Rest completed event
    /// </summary>
    public struct RestCompletedEvent : IGameEvent
    {
        public string ActionType { get; }

        public RestCompletedEvent(string actionType)
        {
            ActionType = actionType;
        }
    }

    #endregion

    #region Reward Scene Events

    /// <summary>
    /// 보상 화면 오픈 이벤트
    /// Reward screen opened event
    /// </summary>
    public struct RewardScreenOpenedEvent : IGameEvent
    {
        public RewardType Type { get; }

        public RewardScreenOpenedEvent(RewardType type)
        {
            Type = type;
        }
    }

    /// <summary>
    /// 골드 획득 이벤트 (보상)
    /// Gold claimed event (reward)
    /// </summary>
    public struct GoldClaimedEvent : IGameEvent
    {
        public int Amount { get; }

        public GoldClaimedEvent(int amount)
        {
            Amount = amount;
        }
    }

    /// <summary>
    /// 카드 보상 획득 이벤트
    /// Card reward claimed event
    /// </summary>
    public struct CardRewardClaimedEvent : IGameEvent
    {
        public string CardId { get; }

        public CardRewardClaimedEvent(string cardId)
        {
            CardId = cardId;
        }
    }

    /// <summary>
    /// 카드 보상 스킵 이벤트
    /// Card reward skipped event
    /// </summary>
    public struct CardRewardSkippedEvent : IGameEvent
    {
    }

    /// <summary>
    /// 유물 획득 이벤트
    /// Relic claimed event
    /// </summary>
    public struct RelicClaimedEvent : IGameEvent
    {
        public string RelicId { get; }

        public RelicClaimedEvent(string relicId)
        {
            RelicId = relicId;
        }
    }

    /// <summary>
    /// 보상 완료 이벤트
    /// Reward completed event
    /// </summary>
    public struct RewardCompletedEvent : IGameEvent
    {
    }

    #endregion

    #region Reward Types

    /// <summary>
    /// 보상 타입
    /// Reward type
    /// </summary>
    public enum RewardType
    {
        Combat,     // 일반 전투
        Elite,      // 엘리트 전투 (유물 포함)
        Boss,       // 보스 전투 (유물 포함)
        Treasure    // 보물 노드
    }

    #endregion
}
