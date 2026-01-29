// Core/Events/GameEvents.cs
// 게임 이벤트 정의

using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Core.Events
{
    #region Game State Events

    /// <summary>
    /// 게임 상태 변경 이벤트
    /// </summary>
    public struct GameStateChangedEvent
    {
        public GameState PreviousState;
        public GameState NewState;
    }

    /// <summary>
    /// 씬 로드 시작 이벤트
    /// </summary>
    public struct SceneLoadStartedEvent
    {
        public SceneIndex TargetScene;
    }

    /// <summary>
    /// 씬 로드 완료 이벤트
    /// </summary>
    public struct SceneLoadCompletedEvent
    {
        public SceneIndex LoadedScene;
    }

    #endregion

    #region Run Events

    /// <summary>
    /// 런 시작 이벤트
    /// </summary>
    public struct RunStartedEvent
    {
        public List<string> PartyCharacterIds;
    }

    /// <summary>
    /// 런 종료 이벤트
    /// </summary>
    public struct RunEndedEvent
    {
        public bool Victory;
        public int FloorsCleared;
        public int GoldEarned;
    }

    #endregion

    #region Map Events

    /// <summary>
    /// 플레이어 이동 이벤트
    /// </summary>
    public struct PlayerMovedEvent
    {
        public Vector2Int FromPosition;
        public Vector2Int ToPosition;
        public int RemainingMoves;
    }

    /// <summary>
    /// 타일 진입 이벤트
    /// </summary>
    public struct TileEnteredEvent
    {
        public Vector2Int Position;
        public TileType TileType;
    }

    /// <summary>
    /// 맵 생성 완료 이벤트
    /// </summary>
    public struct MapGeneratedEvent
    {
        public int Width;
        public int Height;
    }

    /// <summary>
    /// 타일 호버 이벤트 (UI용)
    /// </summary>
    public struct TileHoverEvent
    {
        public Vector2Int Position;
        public TileType TileType;
        public bool IsHovering;
    }

    #endregion

    #region Combat Events

    /// <summary>
    /// 전투 시작 이벤트
    /// </summary>
    public struct CombatStartedEvent
    {
        public TileType EncounterType;
    }

    /// <summary>
    /// 전투 종료 이벤트
    /// </summary>
    public struct CombatEndedEvent
    {
        public bool Victory;
    }

    /// <summary>
    /// 라운드 시작 이벤트
    /// </summary>
    public struct RoundStartedEvent
    {
        public int RoundNumber;
    }

    /// <summary>
    /// 라운드 종료 이벤트
    /// </summary>
    public struct RoundEndedEvent
    {
        public int RoundNumber;
    }

    /// <summary>
    /// 턴 시작 이벤트 (개별 유닛)
    /// </summary>
    public struct TurnStartedEvent
    {
        public string EntityId;
        public bool IsPlayerCharacter;
    }

    /// <summary>
    /// 턴 종료 이벤트 (개별 유닛)
    /// </summary>
    public struct TurnEndedEvent
    {
        public string EntityId;
        public bool IsPlayerCharacter;
    }

    /// <summary>
    /// 카드 드로우 이벤트
    /// </summary>
    public struct CardDrawnEvent
    {
        public string CharacterId;
        public string CardId;
        public int CardsInHand;
    }

    /// <summary>
    /// 카드 사용 이벤트
    /// </summary>
    public struct CardPlayedEvent
    {
        public string CharacterId;
        public string CardId;
        public string TargetId;
        public int EnergyCost;
    }

    /// <summary>
    /// 피해 발생 이벤트
    /// </summary>
    public struct DamageDealtEvent
    {
        public string SourceId;
        public string TargetId;
        public int Damage;
        public int BlockedAmount;
        public bool IsCritical;
    }

    /// <summary>
    /// 방어막 획득 이벤트
    /// </summary>
    public struct BlockGainedEvent
    {
        public string EntityId;
        public int Amount;
        public int TotalBlock;
    }

    /// <summary>
    /// 상태 효과 적용 이벤트
    /// </summary>
    public struct StatusEffectAppliedEvent
    {
        public string EntityId;
        public StatusEffectType EffectType;
        public int Stacks;
    }

    /// <summary>
    /// 엔티티 사망 이벤트
    /// </summary>
    public struct EntityDiedEvent
    {
        public string EntityId;
        public bool IsPlayerCharacter;
    }

    /// <summary>
    /// 회복 이벤트
    /// </summary>
    public struct HealEvent
    {
        public string EntityId;
        public int Amount;
        public int CurrentHP;
        public int MaxHP;
    }

    /// <summary>
    /// 에너지 변경 이벤트
    /// </summary>
    public struct EnergyChangedEvent
    {
        public string CharacterId;
        public int CurrentEnergy;
        public int MaxEnergy;
    }

    #endregion

    #region UI Events

    /// <summary>
    /// 카드 선택 이벤트
    /// </summary>
    public struct CardSelectedEvent
    {
        public string CardId;
        public bool IsSelected;
    }

    /// <summary>
    /// 타겟 선택 이벤트
    /// </summary>
    public struct TargetSelectedEvent
    {
        public string TargetId;
        public TargetType TargetType;
    }

    #endregion

    #region World Map Events

    /// <summary>
    /// 월드맵 로드 완료 이벤트
    /// </summary>
    public struct WorldMapLoadedEvent
    {
        public string MapId;
        public string MapName;
        public int TotalNodes;
    }

    /// <summary>
    /// 맵 노드 선택 이벤트 (클릭)
    /// </summary>
    public struct MapNodeSelectedEvent
    {
        public string NodeId;
        public RegionType RegionType;
    }

    /// <summary>
    /// 맵 노드 진입 이벤트
    /// </summary>
    public struct MapNodeEnteredEvent
    {
        public string NodeId;
        public string FromNodeId;
        public RegionType RegionType;
    }

    /// <summary>
    /// 맵 노드 클리어 이벤트
    /// </summary>
    public struct MapNodeClearedEvent
    {
        public string NodeId;
        public RegionType RegionType;
    }

    /// <summary>
    /// 월드맵 완료 이벤트 (최종 노드 클리어)
    /// </summary>
    public struct WorldMapCompletedEvent
    {
        public string MapId;
        public int ClearedNodes;
        public int TotalNodes;
    }

    /// <summary>
    /// 맵 노드 호버 이벤트 (UI용)
    /// </summary>
    public struct MapNodeHoverEvent
    {
        public string NodeId;
        public RegionType RegionType;
        public string DisplayName;
        public bool IsHovering;
    }

    #endregion

    #region Deck Pile Events

    /// <summary>
    /// 덱 더미 변경 이벤트 (뽑을 덱 / 버린 덱)
    /// </summary>
    public struct DeckPileChangedEvent
    {
        public string CharacterId;
        public int DrawPileCount;
        public int DiscardPileCount;
    }

    #endregion
}
