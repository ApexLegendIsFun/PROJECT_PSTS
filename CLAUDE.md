# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity 2022.3 LTS 기반 **슬레이 더 스파이어 스타일 덱빌딩 로그라이크** 게임 프로젝트.
- 플랫폼: Windows Standalone (1920x1080)
- 언어: C# (.NET Standard 2.1)

## Build & Development Commands

```bash
# Unity 솔루션 빌드
msbuild Project_SS.sln /p:Configuration=Debug

# 특정 어셈블리만 빌드
msbuild Assembly-CSharp.csproj

# Unity Test Runner (에디터 내에서 실행)
# Window → General → Test Runner → Run All
```

## Architecture

### Folder Structure
```
Assets/
├── _Project/                    # 게임 에셋 (Art, Audio, Data, Prefabs, Scenes, Settings)
│   ├── Data/                    # ScriptableObject 인스턴스
│   │   ├── Cards/{Attack,Defense,Skill}/
│   │   ├── Enemies/{Normal,Elite,Boss}/
│   │   ├── Relics/, Events/, StatusEffects/
│   └── Scenes/                  # Boot, MainMenu, Map, Combat, Event, Shop, Rest, Reward
│
└── Scripts/                     # C# 스크립트 (Assembly Definition 구분)
    ├── Core/                    # GameManager, EventBus, ServiceLocator
    ├── Data/                    # ScriptableObject 정의 (CardData, EnemyData, etc.)
    ├── Combat/                  # CombatManager, TurnManager, DeckManager, EnergySystem
    ├── Map/                     # MapGenerator, MapNode, MapManager
    ├── Run/                     # RunManager, RunState, PlayerState
    ├── UI/                      # 모든 UI 시스템
    └── Utility/                 # 공용 유틸리티
```

### Assembly Definition Dependencies
```
Utility (의존성 없음)
    ↑
Core (Utility)
    ↑
Data (Core, Utility)
    ↑
Combat, Map (Core, Data, Utility)
    ↑
Run (Core, Data, Combat, Map, Utility)
    ↑
UI (전체)
```

### Core Systems

**GameManager** (싱글톤): 게임 라이프사이클, 씬 전환, 런 관리
**EventBus**: Pub/Sub 패턴으로 시스템 간 디커플링
**ServiceLocator**: 의존성 주입 없이 서비스 접근

### Combat Flow
```
1. PLAYER_TURN_START → 에너지 충전, 카드 드로우
2. PLAYER_ACTION     → 카드 사용 대기
3. PLAYER_TURN_END   → 핸드 버리기
4. ENEMY_TURN        → 인텐트 실행
5. 반복
```

### Damage Calculation
```
baseDamage + Strength × Weak(0.75) × Vulnerable(1.5) - Block = finalDamage
```

## ScriptableObject Data Model

### CardData
```csharp
- cardId, cardName, description, artwork
- cardType: Attack/Defense/Skill
- energyCost, exhaustOnUse
- List<CardEffect> effects  // Damage, Block, ApplyStatus, Draw, GainEnergy, Heal
- upgradedVersion
```

### EnemyData
```csharp
- enemyId, enemyName, sprite
- enemyType: Normal/Elite/Boss
- maxHealth, minHealth
- List<EnemyAction> actionPattern
- goldReward, cardRewardChance
```

### StatusEffectData
```csharp
- statusId, statusName, description, icon
- effectType, isDebuff, stackable
- triggerTime: TurnStart/TurnEnd/OnDamage
```

## Naming Conventions

| 타입 | 패턴 | 예시 |
|------|------|------|
| Attack 카드 | ATK_한글명_EnglishName.asset | ATK_강타_Strike.asset |
| Defense 카드 | DEF_한글명_EnglishName.asset | DEF_방어_Defend.asset |
| Skill 카드 | SKL_한글명_EnglishName.asset | SKL_격노_Anger.asset |
| 일반 적 | EN_한글명_EnglishName.asset | EN_슬라임_Slime.asset |
| 엘리트 적 | ELITE_한글명_EnglishName.asset | ELITE_센티넬_Sentinel.asset |
| 보스 | BOSS_한글명_EnglishName.asset | BOSS_슬라임왕_SlimeKing.asset |
| 유물 | REL_한글명_EnglishName.asset | REL_부러진왕관_BrokenCrown.asset |

## Code Style

- 주석: 한글 + 영문 병기
  ```csharp
  /// <summary>
  /// 카드 효과를 해결하는 클래스
  /// Card effect resolver class
  /// </summary>
  ```
- ScriptableObject 메뉴: `[CreateAssetMenu(menuName = "Game/Cards/Card Data")]`
- 이벤트 네이밍: `On{Event}` (OnCardPlayed, OnTurnStart, OnDamageTaken)

## Key Interfaces

```csharp
ICombatEntity     // 전투 참여자 (플레이어, 적)
INodeType         // 맵 노드 타입 (CombatNode, EventNode, ShopNode, etc.)
```

## Placeholder Visual Rules

- Attack: 빨간 사각형 + "ATK"
- Defense: 파란 사각형 + "DEF"
- Skill: 초록 사각형 + "SKL"
- 맵 노드: 전투(빨간원), 엘리트(금색!), 보스(해골), 휴식(모닥불), 이벤트(?), 상점(동전)
