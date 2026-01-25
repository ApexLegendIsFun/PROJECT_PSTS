using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 파티 멤버 전투 엔티티
    /// Party member combat entity
    ///
    /// 기존 PlayerCombat을 대체하며, 3인 파티 시스템에서 각 캐릭터를 나타냅니다.
    /// Replaces PlayerCombat, represents each character in 3-party system.
    /// </summary>
    public class PartyMemberCombat : CombatEntity
    {
        [Header("TRIAD - 캐릭터 정보")]
        [SerializeField] private CharacterClassData classData;

        private CharacterClass characterClass;
        private CharacterPosition position;
        private int focusStacks;
        private bool isIncapacitated;

        #region Properties

        /// <summary>
        /// 캐릭터 클래스 데이터
        /// Character class data
        /// </summary>
        public CharacterClassData ClassData => classData;

        /// <summary>
        /// 캐릭터 클래스
        /// Character class
        /// </summary>
        public CharacterClass Class => characterClass;

        /// <summary>
        /// 현재 위치 (전열/후열)
        /// Current position (Active/Standby)
        /// </summary>
        public CharacterPosition Position => position;

        /// <summary>
        /// Focus 스택
        /// Focus stacks
        /// </summary>
        public int FocusStacks => focusStacks;

        /// <summary>
        /// 전열 여부
        /// Whether in Active (front) position
        /// </summary>
        public bool IsActive => position == CharacterPosition.Active;

        /// <summary>
        /// 후열 여부
        /// Whether in Standby (back) position
        /// </summary>
        public bool IsStandby => position == CharacterPosition.Standby;

        /// <summary>
        /// 기절 상태 여부
        /// Whether incapacitated
        /// </summary>
        public bool IsIncapacitated => isIncapacitated;

        /// <summary>
        /// 행동 가능 여부 (생존 + 비기절)
        /// Whether can act (alive and not incapacitated)
        /// </summary>
        public bool CanAct => IsAlive && !isIncapacitated;

        #endregion

        #region Initialization

        /// <summary>
        /// 클래스 데이터로 초기화
        /// Initialize from class data
        /// </summary>
        /// <param name="data">클래스 데이터 / Class data</param>
        /// <param name="initialPosition">초기 위치 / Initial position</param>
        public void Initialize(CharacterClassData data, CharacterPosition initialPosition = CharacterPosition.Standby)
        {
            classData = data;
            characterClass = data.classType;
            position = initialPosition;

            entityId = $"party_{data.classType}";
            entityName = data.className;
            maxHealth = data.baseMaxHP;
            currentHealth = maxHealth;

            focusStacks = 0;
            isIncapacitated = false;

            // StatusEffectManager 초기화 (base.Awake에서 이미 호출됨)
            if (statusEffects == null)
            {
                statusEffects = new StatusEffectManager(this);
            }
        }

        /// <summary>
        /// 런 상태에서 초기화
        /// Initialize from run state
        /// </summary>
        /// <param name="data">클래스 데이터 / Class data</param>
        /// <param name="hp">현재 체력 / Current HP</param>
        /// <param name="maxHp">최대 체력 / Max HP</param>
        /// <param name="initialPosition">초기 위치 / Initial position</param>
        public void InitializeFromRunState(CharacterClassData data, int hp, int maxHp, CharacterPosition initialPosition)
        {
            Initialize(data, initialPosition);
            maxHealth = maxHp;
            currentHealth = hp;
        }

        #endregion

        #region Focus System

        /// <summary>
        /// Focus 획득
        /// Gain Focus
        /// </summary>
        /// <param name="amount">획득량 / Amount to gain</param>
        public void GainFocus(int amount)
        {
            int previousFocus = focusStacks;
            focusStacks = FocusSystem.GainFocus(focusStacks, amount);

            if (focusStacks != previousFocus)
            {
                EventBus.Publish(new FocusChangedEvent(characterClass, previousFocus, focusStacks));
            }
        }

        /// <summary>
        /// Focus 소모 (Tag-In 시)
        /// Consume Focus (on Tag-In)
        /// </summary>
        /// <param name="amount">소모량 (0이면 전부 소모) / Amount to consume (0 = consume all)</param>
        public void ConsumeFocus(int amount = 0)
        {
            int previousFocus = focusStacks;

            if (amount <= 0)
            {
                focusStacks = 0;
            }
            else
            {
                focusStacks = Mathf.Max(0, focusStacks - amount);
            }

            if (focusStacks != previousFocus)
            {
                EventBus.Publish(new FocusChangedEvent(characterClass, previousFocus, focusStacks));
            }
        }

        /// <summary>
        /// Focus 리셋
        /// Reset Focus
        /// </summary>
        public void ResetFocus()
        {
            if (focusStacks > 0)
            {
                int previousFocus = focusStacks;
                focusStacks = 0;
                EventBus.Publish(new FocusChangedEvent(characterClass, previousFocus, 0));
            }
        }

        #endregion

        #region Position System

        /// <summary>
        /// 위치 설정
        /// Set position
        /// </summary>
        /// <param name="newPosition">새 위치 / New position</param>
        public void SetPosition(CharacterPosition newPosition)
        {
            if (position != newPosition)
            {
                var previousPosition = position;
                position = newPosition;
                EventBus.Publish(new PositionChangedEvent(characterClass, previousPosition, newPosition));
            }
        }

        #endregion

        #region Damage & Death

        public override void TakeDamage(int amount)
        {
            // 후열은 직접 공격을 받지 않음 (특수 능력 제외)
            // Standby doesn't take direct attacks (except special abilities)
            if (IsStandby)
            {
                Debug.LogWarning($"[PartyMemberCombat] Standby character {entityName} was targeted for damage.");
            }

            base.TakeDamage(amount);
        }

        protected override void OnDeath()
        {
            // 즉사 대신 기절 처리
            // Incapacitate instead of death
            Incapacitate();
        }

        /// <summary>
        /// 기절 처리
        /// Handle incapacitation
        /// </summary>
        public void Incapacitate()
        {
            if (!isIncapacitated)
            {
                isIncapacitated = true;
                currentHealth = 0;

                EventBus.Publish(new CharacterIncapacitatedEvent(characterClass));

                // 전열이 기절하면 자동 교체 필요
                // If Active is incapacitated, auto-swap needed
                if (IsActive)
                {
                    EventBus.Publish(new ActiveIncapacitatedEvent(characterClass));
                }
            }
        }

        /// <summary>
        /// 부활 (전투 종료 후)
        /// Revive (after combat ends)
        /// </summary>
        public void Revive()
        {
            if (isIncapacitated)
            {
                isIncapacitated = false;
                currentHealth = maxHealth;
                ResetFocus();

                EventBus.Publish(new CharacterRevivedEvent(characterClass));
            }
        }

        /// <summary>
        /// 전투 종료 후 완전 회복
        /// Full recovery after combat
        /// </summary>
        public void FullRecovery()
        {
            Revive();
            currentHealth = maxHealth;
            ResetFocus();
            statusEffects?.ClearAll();
        }

        #endregion

        #region Turn Events

        public override void OnTurnStart()
        {
            if (!CanAct) return;

            // 전열만 블록 초기화
            // Only Active clears block
            if (IsActive)
            {
                ClearBlock();
            }

            base.OnTurnStart();
        }

        public override void OnTurnEnd()
        {
            if (!CanAct) return;

            base.OnTurnEnd();

            // 후열은 턴 종료 시 Focus +1
            // Standby gains +1 Focus at turn end
            if (IsStandby)
            {
                GainFocus(FocusSystem.FOCUS_PER_TURN);
            }
        }

        #endregion

        #region Combat State

        /// <summary>
        /// 전투 상태 리셋
        /// Reset combat state
        /// </summary>
        public void ResetCombatState()
        {
            ResetFocus();
            ClearBlock();
            statusEffects?.ClearAll();
        }

        /// <summary>
        /// 카드 사용 가능 여부 확인
        /// Check if can use card
        /// </summary>
        /// <param name="card">카드 데이터 / Card data</param>
        /// <returns>사용 가능 여부 / Whether can use</returns>
        public bool CanUseCard(CardData card)
        {
            // 전열만 카드 사용 가능
            // Only Active can use cards
            if (!IsActive) return false;

            // 행동 가능해야 함
            // Must be able to act
            if (!CanAct) return false;

            // 클래스 제한 확인
            // Check class restriction
            if (!card.CanBeUsedBy(characterClass)) return false;

            // Focus 비용 확인
            // Check Focus cost
            if (card.focusCost > focusStacks) return false;

            return true;
        }

        #endregion
    }

    #region TRIAD Events

    /// <summary>
    /// Focus 변경 이벤트
    /// Focus changed event
    /// </summary>
    public struct FocusChangedEvent : IGameEvent
    {
        public CharacterClass CharacterClass;
        public int PreviousFocus;
        public int NewFocus;

        public FocusChangedEvent(CharacterClass characterClass, int previous, int current)
        {
            CharacterClass = characterClass;
            PreviousFocus = previous;
            NewFocus = current;
        }
    }

    /// <summary>
    /// 위치 변경 이벤트
    /// Position changed event
    /// </summary>
    public struct PositionChangedEvent : IGameEvent
    {
        public CharacterClass CharacterClass;
        public CharacterPosition PreviousPosition;
        public CharacterPosition NewPosition;

        public PositionChangedEvent(CharacterClass characterClass, CharacterPosition previous, CharacterPosition current)
        {
            CharacterClass = characterClass;
            PreviousPosition = previous;
            NewPosition = current;
        }
    }

    /// <summary>
    /// 캐릭터 기절 이벤트
    /// Character incapacitated event
    /// </summary>
    public struct CharacterIncapacitatedEvent : IGameEvent
    {
        public CharacterClass CharacterClass;

        public CharacterIncapacitatedEvent(CharacterClass characterClass)
        {
            CharacterClass = characterClass;
        }
    }

    /// <summary>
    /// 전열 캐릭터 기절 이벤트 (자동 교체 트리거)
    /// Active character incapacitated event (triggers auto-swap)
    /// </summary>
    public struct ActiveIncapacitatedEvent : IGameEvent
    {
        public CharacterClass CharacterClass;

        public ActiveIncapacitatedEvent(CharacterClass characterClass)
        {
            CharacterClass = characterClass;
        }
    }

    /// <summary>
    /// 캐릭터 부활 이벤트
    /// Character revived event
    /// </summary>
    public struct CharacterRevivedEvent : IGameEvent
    {
        public CharacterClass CharacterClass;

        public CharacterRevivedEvent(CharacterClass characterClass)
        {
            CharacterClass = characterClass;
        }
    }

    /// <summary>
    /// Tag-In 이벤트
    /// Tag-In event
    /// </summary>
    public struct TagInEvent : IGameEvent
    {
        public CharacterClass NewActiveClass;
        public CharacterClass PreviousActiveClass;
        public int EnergyCost;

        public TagInEvent(CharacterClass newActive, CharacterClass previousActive, int energyCost)
        {
            NewActiveClass = newActive;
            PreviousActiveClass = previousActive;
            EnergyCost = energyCost;
        }
    }

    #endregion
}
