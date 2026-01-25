using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Combat;
using ProjectSS.Data;
using ProjectSS.Core;

namespace ProjectSS.UI
{
    /// <summary>
    /// 3인 파티 표시 UI
    /// 3-character party display UI
    ///
    /// TRIAD: Active(전열) 1명 + Standby(후열) 2명 표시
    /// Displays Active (front) 1 + Standby (back) 2
    /// </summary>
    public class PartyDisplayUI : MonoBehaviour
    {
        [Header("Character Slots")]
        [Tooltip("전열(Active) 캐릭터 슬롯 / Active character slot")]
        [SerializeField] private CharacterSlotUI activeSlot;

        [Tooltip("후열(Standby) 캐릭터 슬롯들 / Standby character slots")]
        [SerializeField] private CharacterSlotUI[] standbySlots = new CharacterSlotUI[2];

        [Header("Layout")]
        [Tooltip("슬롯 간격 / Slot spacing")]
        [SerializeField] private float slotSpacing = 120f;

        [Header("Animation")]
        [SerializeField] private float swapAnimationDuration = 0.3f;

        private Dictionary<CharacterClass, CharacterSlotUI> slotMap = new Dictionary<CharacterClass, CharacterSlotUI>();
        private bool isInitialized;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// 파티 UI 초기화
        /// Initialize party UI
        /// </summary>
        public void Initialize()
        {
            if (PartyManager.Instance == null)
            {
                Debug.LogWarning("[PartyDisplayUI] PartyManager.Instance is null!");
                return;
            }

            slotMap.Clear();

            // 모든 파티 멤버에 대해 슬롯 할당
            // Assign slots for all party members
            AssignSlots();

            isInitialized = true;
            RefreshAllSlots();
        }

        /// <summary>
        /// 슬롯 할당
        /// Assign slots
        /// </summary>
        private void AssignSlots()
        {
            var party = PartyManager.Instance;
            if (party == null) return;

            int standbyIndex = 0;

            // 워리어
            if (party.Warrior != null)
            {
                if (party.Warrior.IsActive)
                {
                    AssignToActiveSlot(party.Warrior);
                }
                else
                {
                    AssignToStandbySlot(party.Warrior, standbyIndex++);
                }
            }

            // 메이지
            if (party.Mage != null)
            {
                if (party.Mage.IsActive)
                {
                    AssignToActiveSlot(party.Mage);
                }
                else
                {
                    AssignToStandbySlot(party.Mage, standbyIndex++);
                }
            }

            // 로그
            if (party.Rogue != null)
            {
                if (party.Rogue.IsActive)
                {
                    AssignToActiveSlot(party.Rogue);
                }
                else
                {
                    AssignToStandbySlot(party.Rogue, standbyIndex++);
                }
            }
        }

        private void AssignToActiveSlot(PartyMemberCombat member)
        {
            if (activeSlot != null)
            {
                activeSlot.SetMember(member);
                slotMap[member.Class] = activeSlot;
            }
        }

        private void AssignToStandbySlot(PartyMemberCombat member, int index)
        {
            if (index < standbySlots.Length && standbySlots[index] != null)
            {
                standbySlots[index].SetMember(member);
                slotMap[member.Class] = standbySlots[index];
            }
        }

        /// <summary>
        /// 모든 슬롯 새로고침
        /// Refresh all slots
        /// </summary>
        public void RefreshAllSlots()
        {
            if (!isInitialized) return;

            if (activeSlot != null)
            {
                activeSlot.RefreshDisplay();
            }

            foreach (var slot in standbySlots)
            {
                if (slot != null)
                {
                    slot.RefreshDisplay();
                }
            }
        }

        /// <summary>
        /// 특정 클래스의 슬롯 새로고침
        /// Refresh slot for specific class
        /// </summary>
        public void RefreshSlot(CharacterClass characterClass)
        {
            if (slotMap.TryGetValue(characterClass, out var slot))
            {
                slot.RefreshDisplay();
            }
        }

        /// <summary>
        /// 슬롯 가져오기
        /// Get slot for character class
        /// </summary>
        public CharacterSlotUI GetSlot(CharacterClass characterClass)
        {
            return slotMap.TryGetValue(characterClass, out var slot) ? slot : null;
        }

        #region Event Handling

        private void SubscribeEvents()
        {
            EventBus.Subscribe<TagInEvent>(OnTagIn);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<CombatStartedEvent>(OnCombatStarted);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<TagInEvent>(OnTagIn);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<CombatStartedEvent>(OnCombatStarted);
        }

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            Initialize();
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            if (evt.IsPlayerTurn)
            {
                RefreshAllSlots();
            }
        }

        private void OnTagIn(TagInEvent evt)
        {
            // Tag-In 발생 시 슬롯 재할당
            // Reassign slots on Tag-In
            ReassignSlotsAfterTagIn(evt.NewActiveClass, evt.PreviousActiveClass);
        }

        /// <summary>
        /// Tag-In 후 슬롯 재할당
        /// Reassign slots after Tag-In
        /// </summary>
        private void ReassignSlotsAfterTagIn(CharacterClass newActive, CharacterClass previousActive)
        {
            var party = PartyManager.Instance;
            if (party == null) return;

            // 새 Active 멤버
            var newActiveMember = party.GetByClass(newActive);
            // 이전 Active (이제 Standby)
            var previousActiveMember = party.GetByClass(previousActive);

            if (newActiveMember == null || previousActiveMember == null) return;

            // 슬롯 교체 애니메이션 (향후 구현)
            // Slot swap animation (future implementation)

            // 슬롯 재할당
            // Reassign slots
            if (activeSlot != null)
            {
                activeSlot.SetMember(newActiveMember);
                slotMap[newActive] = activeSlot;
            }

            // 이전 Active를 Standby 슬롯에
            // Put previous Active in Standby slot
            for (int i = 0; i < standbySlots.Length; i++)
            {
                if (standbySlots[i] != null)
                {
                    var currentMember = standbySlots[i].LinkedMember;
                    if (currentMember != null && currentMember.Class == newActive)
                    {
                        // 이 슬롯이 새 Active가 있던 곳
                        // This slot had the new Active
                        standbySlots[i].SetMember(previousActiveMember);
                        slotMap[previousActive] = standbySlots[i];
                        break;
                    }
                }
            }

            RefreshAllSlots();
        }

        #endregion

        #region Public Utilities

        /// <summary>
        /// Active 캐릭터 슬롯 가져오기
        /// Get Active character slot
        /// </summary>
        public CharacterSlotUI GetActiveSlot()
        {
            return activeSlot;
        }

        /// <summary>
        /// Standby 캐릭터 슬롯들 가져오기
        /// Get Standby character slots
        /// </summary>
        public CharacterSlotUI[] GetStandbySlots()
        {
            return standbySlots;
        }

        /// <summary>
        /// 모든 Tag-In 버튼 활성화/비활성화
        /// Enable/disable all Tag-In buttons
        /// </summary>
        public void SetTagInButtonsInteractable(bool interactable)
        {
            foreach (var slot in standbySlots)
            {
                if (slot != null)
                {
                    slot.RefreshDisplay();
                }
            }
        }

        #endregion
    }
}
