// Core/CoreEnums.cs
// 게임 전역 열거형 정의

namespace ProjectSS.Core
{
    /// <summary>
    /// 게임 상태
    /// </summary>
    public enum GameState
    {
        Boot,           // 부팅 중
        MainMenu,       // 메인 메뉴
        InRun,          // 런 진행 중 (맵)
        Combat,         // 전투 중
        Event,          // 이벤트 진행 중
        Shop,           // 상점
        Rest,           // 휴식
        Reward,         // 보상 선택
        GameOver,       // 게임 오버
        Victory         // 승리
    }

    /// <summary>
    /// 씬 인덱스
    /// </summary>
    public enum SceneIndex
    {
        Boot = 0,
        MainMenu = 1,
        Map = 2,
        Combat = 3
    }

    /// <summary>
    /// 캐릭터 클래스 (파티원 타입)
    /// </summary>
    public enum CharacterClass
    {
        None,
        Warrior,        // 전사
        Mage,           // 마법사
        Rogue,          // 도적
        Healer,         // 힐러
        Tank            // 탱커
    }

    /// <summary>
    /// 카드 타입
    /// </summary>
    public enum CardType
    {
        Attack,         // 공격
        Skill,          // 스킬
        Power           // 파워 (영구 효과)
    }

    /// <summary>
    /// 카드 희귀도
    /// </summary>
    public enum CardRarity
    {
        Starter,        // 시작 카드
        Common,         // 일반
        Uncommon,       // 고급
        Rare            // 희귀
    }

    /// <summary>
    /// 맵 타일 타입
    /// </summary>
    public enum TileType
    {
        Empty,          // 빈 타일
        Enemy,          // 일반 적
        Elite,          // 엘리트
        Boss,           // 보스
        Shop,           // 상점
        Rest,           // 휴식
        Event,          // 이벤트
        Treasure        // 보물
    }

    /// <summary>
    /// 전투 상태
    /// </summary>
    public enum CombatState
    {
        NotInCombat,    // 전투 아님
        Initializing,   // 초기화 중
        RoundStart,     // 라운드 시작
        TurnStart,      // 턴 시작
        PlayerTurn,     // 플레이어 턴
        EnemyTurn,      // 적 턴
        TurnEnd,        // 턴 종료
        RoundEnd,       // 라운드 종료
        Victory,        // 승리
        Defeat          // 패배
    }

    /// <summary>
    /// 상태 효과 타입
    /// </summary>
    public enum StatusEffectType
    {
        // 버프
        Strength,       // 공격력 증가
        Dexterity,      // 방어력 증가
        Regen,          // 재생
        Block,          // 방어막

        // 디버프
        Weak,           // 약화 (공격력 감소)
        Vulnerable,     // 취약 (받는 피해 증가)
        Poison,         // 독 (턴마다 피해)
        Burn,           // 화상 (턴마다 피해)
        Stun            // 기절 (행동 불가)
    }

    /// <summary>
    /// 타겟 타입
    /// </summary>
    public enum TargetType
    {
        Self,           // 자신
        SingleEnemy,    // 단일 적
        AllEnemies,     // 모든 적
        SingleAlly,     // 단일 아군
        AllAllies,      // 모든 아군
        Random          // 무작위
    }

    /// <summary>
    /// 월드맵 지역 타입
    /// </summary>
    public enum RegionType
    {
        Start,          // 시작점
        Shelter,        // 쉼터 (회복)
        Dungeon,        // 던전 (전투)
        Boss,           // 보스 던전
        End             // 종료점
    }
}
