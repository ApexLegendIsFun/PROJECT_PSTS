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
}
