using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 파티 관리 싱글톤
    /// Party management singleton
    ///
    /// 3인 파티 시스템의 핵심 관리자입니다.
    /// Core manager for the 3-character party system.
    /// - 파티 멤버 관리 (Warrior, Mage, Rogue)
    /// - Tag-In 시스템
    /// - 위치 관리 (Active/Standby)
    /// - 타겟팅 로직
    /// </summary>
    public class PartyManager : MonoBehaviour
    {
        public static PartyManager Instance { get; private set; }

        [Header("Party Members")]
        [SerializeField] private PartyMemberCombat warriorCombat;
        [SerializeField] private PartyMemberCombat mageCombat;
        [SerializeField] private PartyMemberCombat rogueCombat;

        [Header("Class Data")]
        [SerializeField] private CharacterClassData warriorData;
        [SerializeField] private CharacterClassData mageData;
        [SerializeField] private CharacterClassData rogueData;

        private Dictionary<CharacterClass, PartyMemberCombat> memberMap;
        private EnergySystem energySystem;

        #region Properties

        /// <summary>
        /// 현재 전열(Active) 캐릭터
        /// Current Active (front) character
        /// </summary>
        public PartyMemberCombat Active => memberMap?.Values.FirstOrDefault(m => m.IsActive && m.CanAct);

        /// <summary>
        /// 후열(Standby) 캐릭터 목록
        /// Standby (back) characters
        /// </summary>
        public IEnumerable<PartyMemberCombat> Standby => memberMap?.Values.Where(m => m.IsStandby && m.CanAct) ?? Enumerable.Empty<PartyMemberCombat>();

        /// <summary>
        /// 생존 캐릭터 목록
        /// Alive characters
        /// </summary>
        public IEnumerable<PartyMemberCombat> Alive => memberMap?.Values.Where(m => m.IsAlive) ?? Enumerable.Empty<PartyMemberCombat>();

        /// <summary>
        /// 행동 가능 캐릭터 목록
        /// Characters that can act
        /// </summary>
        public IEnumerable<PartyMemberCombat> Available => memberMap?.Values.Where(m => m.CanAct) ?? Enumerable.Empty<PartyMemberCombat>();

        /// <summary>
        /// 모든 파티 멤버
        /// All party members
        /// </summary>
        public IEnumerable<PartyMemberCombat> All => memberMap?.Values ?? Enumerable.Empty<PartyMemberCombat>();

        /// <summary>
        /// 워리어
        /// Warrior
        /// </summary>
        public PartyMemberCombat Warrior => warriorCombat;

        /// <summary>
        /// 메이지
        /// Mage
        /// </summary>
        public PartyMemberCombat Mage => mageCombat;

        /// <summary>
        /// 로그
        /// Rogue
        /// </summary>
        public PartyMemberCombat Rogue => rogueCombat;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializeMemberMap();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            UnsubscribeFromEvents();
        }

        #endregion

        #region Initialization

        private void InitializeMemberMap()
        {
            memberMap = new Dictionary<CharacterClass, PartyMemberCombat>();

            if (warriorCombat != null)
                memberMap[CharacterClass.Warrior] = warriorCombat;
            if (mageCombat != null)
                memberMap[CharacterClass.Mage] = mageCombat;
            if (rogueCombat != null)
                memberMap[CharacterClass.Rogue] = rogueCombat;
        }

        /// <summary>
        /// 파티 초기화
        /// Initialize party
        /// </summary>
        /// <param name="partyState">파티 상태 / Party state</param>
        /// <param name="energy">에너지 시스템 참조 / Energy system reference</param>
        public void Initialize(PartyState partyState, EnergySystem energy)
        {
            energySystem = energy;

            // 각 멤버 초기화
            // Initialize each member
            if (warriorCombat != null && warriorData != null)
            {
                warriorCombat.InitializeFromRunState(
                    warriorData,
                    partyState.warrior.currentHP,
                    partyState.warrior.maxHP,
                    partyState.warrior.position
                );
            }

            if (mageCombat != null && mageData != null)
            {
                mageCombat.InitializeFromRunState(
                    mageData,
                    partyState.mage.currentHP,
                    partyState.mage.maxHP,
                    partyState.mage.position
                );
            }

            if (rogueCombat != null && rogueData != null)
            {
                rogueCombat.InitializeFromRunState(
                    rogueData,
                    partyState.rogue.currentHP,
                    partyState.rogue.maxHP,
                    partyState.rogue.position
                );
            }
        }

        /// <summary>
        /// 클래스 데이터로 초기화 (새 런 시작 시)
        /// Initialize from class data (new run start)
        /// </summary>
        public void InitializeNewRun(CharacterClassData warrior, CharacterClassData mage, CharacterClassData rogue, EnergySystem energy)
        {
            energySystem = energy;
            warriorData = warrior;
            mageData = mage;
            rogueData = rogue;

            if (warriorCombat != null)
                warriorCombat.Initialize(warrior, CharacterPosition.Active);
            if (mageCombat != null)
                mageCombat.Initialize(mage, CharacterPosition.Standby);
            if (rogueCombat != null)
                rogueCombat.Initialize(rogue, CharacterPosition.Standby);
        }

        #endregion

        #region Tag-In System

        /// <summary>
        /// Tag-In 가능 여부 확인
        /// Check if Tag-In is possible
        /// </summary>
        /// <param name="standbyMember">교체할 후열 멤버 / Standby member to swap in</param>
        /// <returns>Tag-In 가능 여부 / Whether Tag-In is possible</returns>
        public bool CanTagIn(PartyMemberCombat standbyMember)
        {
            // 후열이고 행동 가능해야 함
            // Must be Standby and able to act
            if (standbyMember == null || !standbyMember.IsStandby || !standbyMember.CanAct)
                return false;

            // 현재 전열이 존재해야 함
            // Current Active must exist
            if (Active == null)
                return false;

            // 에너지 비용 확인
            // Check energy cost
            int cost = GetTagInCost(standbyMember);
            return energySystem == null || energySystem.CanSpend(cost);
        }

        /// <summary>
        /// Tag-In 비용 계산
        /// Calculate Tag-In cost
        /// </summary>
        /// <param name="standbyMember">교체할 후열 멤버 / Standby member to swap in</param>
        /// <returns>에너지 비용 / Energy cost</returns>
        public int GetTagInCost(PartyMemberCombat standbyMember)
        {
            if (standbyMember == null) return 1;
            return FocusSystem.GetTagInCost(standbyMember.FocusStacks);
        }

        /// <summary>
        /// Tag-In 실행
        /// Execute Tag-In
        /// </summary>
        /// <param name="newActive">새 전열 캐릭터 / New Active character</param>
        /// <returns>성공 여부 / Whether successful</returns>
        public bool ExecuteTagIn(PartyMemberCombat newActive)
        {
            if (!CanTagIn(newActive))
                return false;

            var currentActive = Active;
            int cost = GetTagInCost(newActive);

            // 에너지 소모
            // Spend energy
            if (energySystem != null && cost > 0)
            {
                energySystem.Spend(cost);
            }

            // 위치 교체
            // Swap positions
            currentActive.SetPosition(CharacterPosition.Standby);
            newActive.SetPosition(CharacterPosition.Active);

            // Focus 소모 (전부)
            // Consume Focus (all)
            newActive.ConsumeFocus();

            // Tag-In 보너스 적용
            // Apply Tag-In bonus
            ApplyTagInBonus(newActive);

            // 이벤트 발행
            // Publish event
            EventBus.Publish(new TagInEvent(newActive.Class, currentActive.Class, cost));

            return true;
        }

        /// <summary>
        /// Tag-In 보너스 적용
        /// Apply Tag-In bonus
        /// </summary>
        /// <param name="member">Tag-In한 멤버 / Member who tagged in</param>
        private void ApplyTagInBonus(PartyMemberCombat member)
        {
            var classData = member.ClassData;
            if (classData == null) return;

            switch (classData.tagInBonusType)
            {
                case TagInBonusType.GainBlock:
                    member.GainBlock(classData.tagInBonusValue);
                    break;

                case TagInBonusType.DrawCard:
                    // DeckManager를 통해 카드 드로우
                    // Draw cards through DeckManager
                    EventBus.Publish(new TagInDrawCardEvent(classData.tagInBonusValue));
                    break;

                case TagInBonusType.ApplyDebuff:
                    // 모든 적에게 디버프 적용 (CombatManager에서 처리)
                    // Apply debuff to all enemies (handled by CombatManager)
                    if (classData.tagInStatusEffect != null)
                    {
                        EventBus.Publish(new TagInApplyDebuffEvent(
                            classData.tagInStatusEffect,
                            classData.tagInStatusStacks
                        ));
                    }
                    break;

                case TagInBonusType.GainEnergy:
                    if (energySystem != null)
                    {
                        energySystem.Gain(classData.tagInBonusValue);
                    }
                    break;

                case TagInBonusType.Heal:
                    member.Heal(classData.tagInBonusValue);
                    break;
            }
        }

        #endregion

        #region Turn Processing

        /// <summary>
        /// 턴 종료 시 Focus 처리
        /// Process Focus at turn end
        /// </summary>
        public void ProcessTurnEndFocus()
        {
            foreach (var member in Standby)
            {
                member.GainFocus(FocusSystem.FOCUS_PER_TURN);
            }
        }

        /// <summary>
        /// 모든 파티원 턴 시작 처리
        /// Process turn start for all party members
        /// </summary>
        public void OnPartyTurnStart()
        {
            foreach (var member in Available)
            {
                member.OnTurnStart();
            }
        }

        /// <summary>
        /// 모든 파티원 턴 종료 처리
        /// Process turn end for all party members
        /// </summary>
        public void OnPartyTurnEnd()
        {
            foreach (var member in Available)
            {
                member.OnTurnEnd();
            }
        }

        #endregion

        #region Targeting

        /// <summary>
        /// 적의 타겟 선택
        /// Get target for enemy
        /// </summary>
        /// <param name="strategy">타겟팅 전략 / Targeting strategy</param>
        /// <returns>타겟 캐릭터 / Target character</returns>
        public PartyMemberCombat GetTarget(EnemyTargetingStrategy strategy)
        {
            var targets = Available.ToList();
            if (targets.Count == 0) return null;

            return strategy switch
            {
                EnemyTargetingStrategy.ActiveOnly => Active,
                EnemyTargetingStrategy.LowestHP => targets.OrderBy(t => t.CurrentHealth).First(),
                EnemyTargetingStrategy.HighestHP => targets.OrderByDescending(t => t.CurrentHealth).First(),
                EnemyTargetingStrategy.Random => targets[Random.Range(0, targets.Count)],
                EnemyTargetingStrategy.MostFocus => targets.OrderByDescending(t => t.FocusStacks).First(),
                _ => Active
            };
        }

        #endregion

        #region Party State

        /// <summary>
        /// 파티 전멸 여부
        /// Check if party is wiped
        /// </summary>
        public bool IsPartyWiped()
        {
            return !memberMap.Values.Any(m => m.CanAct);
        }

        /// <summary>
        /// 행동 가능 캐릭터 존재 여부
        /// Check if any character can act
        /// </summary>
        public bool AnyCharacterCanAct()
        {
            return memberMap.Values.Any(m => m.CanAct);
        }

        /// <summary>
        /// 클래스로 멤버 찾기
        /// Get member by class
        /// </summary>
        public PartyMemberCombat GetByClass(CharacterClass characterClass)
        {
            return memberMap.TryGetValue(characterClass, out var member) ? member : null;
        }

        /// <summary>
        /// 전투 종료 후 모든 캐릭터 부활
        /// Revive all characters after combat
        /// </summary>
        public void ReviveAll()
        {
            foreach (var member in memberMap.Values)
            {
                member.FullRecovery();
            }
        }

        /// <summary>
        /// 현재 파티 상태를 PartyState로 변환
        /// Convert current party state to PartyState
        /// </summary>
        public PartyState ToPartyState()
        {
            var state = new PartyState();

            if (warriorCombat != null)
            {
                state.warrior = new CharacterState
                {
                    characterClass = CharacterClass.Warrior,
                    currentHP = warriorCombat.CurrentHealth,
                    maxHP = warriorCombat.MaxHealth,
                    focusStacks = warriorCombat.FocusStacks,
                    isIncapacitated = warriorCombat.IsIncapacitated,
                    position = warriorCombat.Position
                };
            }

            if (mageCombat != null)
            {
                state.mage = new CharacterState
                {
                    characterClass = CharacterClass.Mage,
                    currentHP = mageCombat.CurrentHealth,
                    maxHP = mageCombat.MaxHealth,
                    focusStacks = mageCombat.FocusStacks,
                    isIncapacitated = mageCombat.IsIncapacitated,
                    position = mageCombat.Position
                };
            }

            if (rogueCombat != null)
            {
                state.rogue = new CharacterState
                {
                    characterClass = CharacterClass.Rogue,
                    currentHP = rogueCombat.CurrentHealth,
                    maxHP = rogueCombat.MaxHealth,
                    focusStacks = rogueCombat.FocusStacks,
                    isIncapacitated = rogueCombat.IsIncapacitated,
                    position = rogueCombat.Position
                };
            }

            return state;
        }

        #endregion

        #region Event Handling

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<ActiveIncapacitatedEvent>(OnActiveIncapacitated);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<ActiveIncapacitatedEvent>(OnActiveIncapacitated);
        }

        /// <summary>
        /// 전열 기절 시 자동 교체
        /// Auto-swap when Active is incapacitated
        /// </summary>
        private void OnActiveIncapacitated(ActiveIncapacitatedEvent evt)
        {
            // 행동 가능한 후열 중 첫 번째를 자동으로 전열로
            // Auto-swap first available Standby to Active
            var nextActive = Standby.FirstOrDefault();
            if (nextActive != null)
            {
                nextActive.SetPosition(CharacterPosition.Active);
                // 자동 교체는 에너지 소모 없음, Focus도 유지
                // Auto-swap costs no energy, Focus is kept
            }
        }

        #endregion
    }

    #region Tag-In Bonus Events

    /// <summary>
    /// Tag-In 카드 드로우 이벤트
    /// Tag-In draw card event
    /// </summary>
    public struct TagInDrawCardEvent : IGameEvent
    {
        public int DrawCount;

        public TagInDrawCardEvent(int count)
        {
            DrawCount = count;
        }
    }

    /// <summary>
    /// Tag-In 디버프 적용 이벤트
    /// Tag-In apply debuff event
    /// </summary>
    public struct TagInApplyDebuffEvent : IGameEvent
    {
        public StatusEffectData StatusEffect;
        public int Stacks;

        public TagInApplyDebuffEvent(StatusEffectData effect, int stacks)
        {
            StatusEffect = effect;
            Stacks = stacks;
        }
    }

    #endregion
}
