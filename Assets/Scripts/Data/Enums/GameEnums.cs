namespace ProjectSS.Data
{
    #region Card Enums

    /// <summary>
    /// 카드 타입
    /// Card type
    /// </summary>
    public enum CardType
    {
        Attack,     // 공격
        Defense,    // 방어
        Skill       // 스킬
    }

    /// <summary>
    /// 카드 희귀도
    /// Card rarity
    /// </summary>
    public enum CardRarity
    {
        Starter,    // 시작 카드
        Common,     // 일반
        Uncommon,   // 고급
        Rare        // 희귀
    }

    /// <summary>
    /// 카드 효과 타입
    /// Card effect type
    /// </summary>
    public enum CardEffectType
    {
        Damage,         // 데미지
        Block,          // 블록
        ApplyStatus,    // 상태이상 부여
        Draw,           // 카드 드로우
        GainEnergy,     // 에너지 획득
        Heal,           // 체력 회복
        Discard,        // 카드 버리기
        Exhaust         // 카드 소멸
    }

    /// <summary>
    /// 대상 타입
    /// Target type
    /// </summary>
    public enum TargetType
    {
        Self,           // 자신
        SingleEnemy,    // 단일 적
        AllEnemies,     // 모든 적
        RandomEnemy     // 랜덤 적
    }

    #endregion

    #region Enemy Enums

    /// <summary>
    /// 적 타입
    /// Enemy type
    /// </summary>
    public enum EnemyType
    {
        Normal,     // 일반
        Elite,      // 엘리트
        Boss        // 보스
    }

    /// <summary>
    /// 적 인텐트 타입
    /// Enemy intent type
    /// </summary>
    public enum EnemyIntentType
    {
        Attack,         // 공격
        Defend,         // 방어
        Buff,           // 버프
        Debuff,         // 디버프
        AttackBuff,     // 공격 + 버프
        AttackDebuff,   // 공격 + 디버프
        Unknown         // 알 수 없음
    }

    #endregion

    #region Status Effect Enums

    /// <summary>
    /// 상태이상 타입
    /// Status effect type
    /// </summary>
    public enum StatusEffectType
    {
        // 버프 (Buffs)
        Strength,       // 힘 (공격력 증가)
        Dexterity,      // 민첩 (블록 증가)

        // 디버프 (Debuffs)
        Weak,           // 약화 (데미지 25% 감소)
        Vulnerable,     // 취약 (받는 데미지 50% 증가)
        Frail,          // 허약 (블록 25% 감소)

        // 기타 (Others)
        Poison,         // 독 (턴 종료시 데미지)
        Regeneration    // 재생 (턴 종료시 회복)
    }

    /// <summary>
    /// 상태이상 스택 방식
    /// Status effect stack behavior
    /// </summary>
    public enum StackBehavior
    {
        Intensity,      // 강도 (스택 수 = 효과 수치)
        Duration        // 지속시간 (스택 수 = 남은 턴)
    }

    /// <summary>
    /// 상태이상 발동 시점
    /// Status effect trigger timing
    /// </summary>
    public enum StatusTrigger
    {
        TurnStart,      // 턴 시작
        TurnEnd,        // 턴 종료
        OnDamageDealt,  // 데미지 줄 때
        OnDamageTaken,  // 데미지 받을 때
        OnBlock,        // 블록 시
        OnCardPlayed,   // 카드 사용 시
        Passive         // 항상 (패시브)
    }

    #endregion

    #region Relic Enums

    /// <summary>
    /// 유물 희귀도
    /// Relic rarity
    /// </summary>
    public enum RelicRarity
    {
        Starter,    // 시작 유물
        Common,     // 일반
        Uncommon,   // 고급
        Rare,       // 희귀
        Boss,       // 보스
        Event       // 이벤트
    }

    /// <summary>
    /// 유물 발동 시점
    /// Relic trigger timing
    /// </summary>
    public enum RelicTrigger
    {
        Passive,        // 패시브 (항상)
        CombatStart,    // 전투 시작
        CombatEnd,      // 전투 종료
        TurnStart,      // 턴 시작
        TurnEnd,        // 턴 종료
        OnPickup,       // 획득 시
        OnCardPlayed,   // 카드 사용 시
        OnDamageDealt,  // 데미지 줄 때
        OnDamageTaken,  // 데미지 받을 때
        OnRest,         // 휴식 시
        OnShop          // 상점에서
    }

    #endregion

    #region Map Enums

    /// <summary>
    /// 맵 노드 타입
    /// Map node type
    /// </summary>
    public enum MapNodeType
    {
        Combat,     // 일반 전투
        Elite,      // 엘리트 전투
        Boss,       // 보스 전투
        Rest,       // 휴식
        Event,      // 이벤트
        Shop,       // 상점
        Treasure    // 보물
    }

    #endregion

    #region TRIAD Enums

    /// <summary>
    /// 캐릭터 클래스
    /// Character class
    /// </summary>
    public enum CharacterClass
    {
        Warrior,    // 워리어 - 방어/도발 특화
        Mage,       // 메이지 - 광역/버프 특화
        Rogue       // 로그 - 단일타겟/디버프 특화
    }

    /// <summary>
    /// 캐릭터 위치 (전열/후열)
    /// Character position (Active/Standby)
    /// </summary>
    public enum CharacterPosition
    {
        Active,     // 전열 (1명) - 카드 사용 가능, 적의 공격 대상
        Standby     // 후열 (2명) - Focus 축적, 직접 공격받지 않음
    }

    /// <summary>
    /// 태그인 보너스 타입
    /// Tag-in bonus type
    /// </summary>
    public enum TagInBonusType
    {
        GainBlock,      // 방어도 획득 (Warrior)
        ApplyDebuff,    // 디버프 부여 (Rogue)
        DrawCard,       // 카드 드로우 (Mage)
        GainEnergy,     // 에너지 획득
        Heal            // 체력 회복
    }

    /// <summary>
    /// 브레이크 조건 타입
    /// Break condition type
    /// </summary>
    public enum BreakConditionType
    {
        DamageThreshold,    // 임계 데미지 조건
        HitCount,           // 타격 횟수 조건
        Both                // 둘 중 하나 충족 시
    }

    /// <summary>
    /// 적 타겟팅 전략
    /// Enemy targeting strategy
    /// </summary>
    public enum EnemyTargetingStrategy
    {
        ActiveOnly,     // 전열만 공격 (기본)
        LowestHP,       // 최저 HP 타겟
        HighestHP,      // 최고 HP 타겟
        Random,         // 랜덤 타겟
        MostFocus       // Focus 최다 타겟
    }

    #endregion
}
