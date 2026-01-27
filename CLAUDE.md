# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PROJECT_PSTS is a 2D turn-based deck-building roguelike game built in **Unity 2022.3.62 LTS** with **C# 9.0** (.NET Standard 2.1). The game features card-based combat, party management (3-member "TRIAD" system), procedural map generation, and Korean localization.

## Build & Test Commands

```bash
# Build via MSBuild (command line)
msbuild PROJECT_PSTS.sln /p:Configuration=Release /p:Platform=AnyCPU

# Build in Unity Editor
# File → Build Settings → Build (target: Windows x64)

# Run tests in Unity
# Window → General → Test Runner → Run All (Edit Mode tests)
```

## Architecture

### Modular Assembly Structure

The codebase uses Assembly Definition Files (.asmdef) for modular compilation:

```
Utility (no dependencies)
    ↑
Core, Data (depend on Utility)
    ↑
Combat, Map, Run, UI, Events (depend on Core)
    ↑
Shop, Reward (depend on Core, Data)
    ↑
Editor (development only, references all)
```

**Namespaces follow**: `ProjectSS.[System]` pattern (e.g., `ProjectSS.Combat`, `ProjectSS.Core`)

### Event-Driven Architecture

Central EventBus using pub/sub pattern with generics:
- All events implement `IGameEvent` marker interface
- Event naming: `[Action]Event` (e.g., `CombatStartedEvent`, `RunEndedEvent`)
- EventBus handles errors with try-catch and Debug.LogError

### Singleton Managers

- **GameManager**: Main lifecycle manager (DontDestroyOnLoad)
- **CombatManager**: Combat orchestration
- **ServiceLocator**: Dependency injection pattern

### Scene Flow

```
Boot(0) → MainMenu(1) → Map(2) → Combat(3)/Event(4)/Shop(5)/Rest(6) → Reward(7)
```

## Key Systems

| System | Purpose | Key Types |
|--------|---------|-----------|
| Combat | Turn-based card combat | CombatManager, TurnManager, ICombatEntity |
| Deck | Card collection & hand management | DeckManager, CardInstance, CardEffectResolver |
| World Map | Node-graph based map progression | WorldMapManager, WorldMapController, WorldMapData |
| Run | Expedition progress tracking | RunManager |

### Card System

```csharp
CardType: Attack, Defense, Skill
CardRarity: Starter, Common, Uncommon, Rare
TargetType: Self, SingleEnemy, AllEnemies, RandomEnemy
```

### Combat Mechanics

- Energy management (limited actions per turn)
- Status effects: Strength, Dexterity, Poison, Burn, Stun, Vulnerable, Weak
- Break Gauge, Focus System, Performance System
- Enemy intent preview system

## Naming Conventions

- **Managers**: `[System]Manager`
- **Private fields**: `_camelCase` prefix
- **Events**: `[Action]Event` structs
- **Comments**: Bilingual (Korean/English)

## Editor Tools

Custom generators in Editor assembly:
- **ProjectBootstrapper**: Full project bootstrap (Ctrl+Alt+A) - creates all assets and scenes
- **CardAssetGenerator**: Card/effect SO and card pool generation
- **WorldMapAssetGenerator**: World map assets (visual config, test map, prefabs)
- **Scene Builders**: ISceneBuilder implementations for each scene (Boot, MainMenu, Map, Combat)

## Scene Modification Rules

**IMPORTANT**: Never modify Unity scene files (.unity) directly. All scene changes must be made through Scene Generator scripts in the Editor assembly.

- Use existing generators (e.g., `GameSetupWizard`) to modify scene structure
- Create new generator scripts in `Assets/Scripts/Editor/` for new scene setup requirements
- Scene generators ensure reproducible, version-control-friendly scene configurations

## Implementation Patterns

### Manager Singleton Pattern
```csharp
public class NewManager : MonoBehaviour
{
    public static NewManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        ServiceLocator.Register(this);
    }
}
```

### EventBus Usage
```csharp
// Publish event
EventBus.Publish(new CustomEvent { Data = value });

// Subscribe to event (in Start or OnEnable)
EventBus.Subscribe<CustomEvent>(OnCustomEvent);

// Handler method
private void OnCustomEvent(CustomEvent e) { /* handle */ }
```

### Event Definition
```csharp
// Add to Events/GameEvents.cs
public struct CustomEvent : IGameEvent
{
    public string EntityId;
    public int Value;
}
```

## Enum Reference

| Enum | Values | Location |
|------|--------|----------|
| GameState | Boot, MainMenu, InRun, Combat, Event, Shop, Rest, GameOver | Core/CoreEnums.cs |
| CombatState | NotInCombat, Initializing, RoundStart, TurnStart, PlayerAction, TurnEnd, RoundEnd, Victory, Defeat | Core/CoreEnums.cs |
| CardType | Attack, Skill, Power | Core/CoreEnums.cs |
| CardRarity | Starter, Common, Uncommon, Rare | Core/CoreEnums.cs |
| TargetType | Self, SingleEnemy, AllEnemies, SingleAlly, AllAllies, Random | Core/CoreEnums.cs |
| TileType | Empty, Enemy, Elite, Boss, Event, Shop, Rest, Treasure | Core/CoreEnums.cs |
| StatusEffectType | Strength, Dexterity, Regen, Poison, Burn, Weak, Vulnerable, Stun | Core/CoreEnums.cs |
| CharacterClass | None, Warrior, Mage, Rogue, Healer | Core/CoreEnums.cs |
| EnemyIntentType | Attack, Defend, Buff, Debuff | Combat/Entities/EnemyCombat.cs |

## Event Reference

| Event | Properties | Publisher |
|-------|------------|-----------|
| CombatStartedEvent | EncounterType | CombatManager, MapManager |
| CombatEndedEvent | Victory | CombatManager |
| TurnStartedEvent | EntityId, IsPlayerTurn | PartyMemberCombat, EnemyCombat |
| TurnEndedEvent | EntityId | PartyMemberCombat, EnemyCombat |
| RoundStartedEvent | RoundNumber | TurnManager |
| RoundEndedEvent | RoundNumber | TurnManager |
| CardPlayedEvent | CharacterId, CardData | PartyMemberCombat |
| CardDrawnEvent | CharacterId, CardData | DeckManager |
| DamageDealtEvent | SourceId, TargetId, Amount | CombatEntity |
| BlockGainedEvent | EntityId, Amount | CombatEntity |
| EntityDiedEvent | EntityId, IsEnemy | CombatEntity |
| EnergyChangedEvent | CharacterId, Current, Max | PartyMemberCombat |
| GameStateChangedEvent | PreviousState, NewState | GameManager |
| SceneLoadStartedEvent | TargetScene | GameManager |
| SceneLoadCompletedEvent | LoadedScene | GameManager |
| WorldMapLoadedEvent | MapId, MapName, TotalNodes | WorldMapManager |
| MapNodeEnteredEvent | NodeId, FromNodeId, RegionType | WorldMapManager |
| MapNodeClearedEvent | NodeId, RegionType | WorldMapManager |
| MapNodeSelectedEvent | NodeId, RegionType | WorldMapNodeUI |
| MapNodeHoverEvent | NodeId, RegionType, DisplayName, IsHovering | WorldMapNodeUI |

## File Location Reference

| Category | Path | Key Files |
|----------|------|-----------|
| Core Managers | Scripts/Core/ | GameManager, ServiceLocator, EventBus |
| Combat System | Scripts/Combat/ | CombatManager, TurnManager |
| Combat Entities | Scripts/Combat/Entities/ | CombatEntity, PartyMemberCombat, EnemyCombat |
| Deck System | Scripts/Combat/Deck/ | DeckManager, CardEffectResolver |
| World Map | Scripts/Map/ | WorldMapManager, WorldMapData, WorldMapNode |
| Data Classes | Scripts/Data/ | CardData, EnemyData, CharacterData |
| Combat UI | Scripts/Combat/UI/ | CombatUIController, CardHandUI, EntityStatusUI |
| World Map UI | Scripts/Map/UI/ | WorldMapController, WorldMapNodeUI, WorldMapPathUI |
| Events | Scripts/Events/ | GameEvents (IGameEvent implementations) |
| Enums | Scripts/Core/CoreEnums.cs | All game enums |

## Documentation Reference

Refer to documents in [Docs/](Docs/) folder during implementation:
- **Design/**: Game design (combat, cards, progression)
- **Technical/**: Technical implementation (architecture, conventions)
- **Reference/**: Reference (Slay the Spire)
- **Troubleshooting/**: Unity issue resolution records
