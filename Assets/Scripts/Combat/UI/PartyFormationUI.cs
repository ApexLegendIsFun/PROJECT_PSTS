using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 파티 포메이션 UI (전방/후방 배치 관리)
    /// Party formation UI managing front/back positioning
    /// Position 0 = 후방 (Back, leftmost)
    /// Position 2 = 전방 (Front, rightmost)
    /// </summary>
    public class PartyFormationUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _partyContainer;
        [SerializeField] private GameObject _memberSlotPrefab;

        [Header("Layout Settings")]
        [SerializeField] private int _maxPartySize = 3;

        // Position anchors for 3 party members (normalized within PartyContainer)
        // Back (0) = leftmost, Front (2) = rightmost
        private readonly Vector2[] _slotAnchorsMin = new Vector2[]
        {
            new Vector2(0.00f, 0f),  // Position 0: 후방 (Back)
            new Vector2(0.35f, 0f),  // Position 1: 중앙 (Mid)
            new Vector2(0.70f, 0f),  // Position 2: 전방 (Front)
        };

        private readonly Vector2[] _slotAnchorsMax = new Vector2[]
        {
            new Vector2(0.30f, 1f),  // Position 0: 후방 (Back)
            new Vector2(0.65f, 1f),  // Position 1: 중앙 (Mid)
            new Vector2(1.00f, 1f),  // Position 2: 전방 (Front)
        };

        private List<PartyMemberSlotUI> _memberSlots = new List<PartyMemberSlotUI>();
        private Dictionary<string, PartyMemberSlotUI> _entityToSlotMap = new Dictionary<string, PartyMemberSlotUI>();

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 파티 멤버들로 포메이션 초기화
        /// </summary>
        public void Initialize(IReadOnlyList<PartyMemberCombat> partyMembers)
        {
            ClearSlots();

            if (partyMembers == null) return;

            int memberCount = Mathf.Min(partyMembers.Count, _maxPartySize);

            for (int i = 0; i < memberCount; i++)
            {
                var member = partyMembers[i];
                var slot = CreateMemberSlot(i);
                slot.Initialize(member);

                _memberSlots.Add(slot);
                _entityToSlotMap[member.EntityId] = slot;
            }
        }

        private PartyMemberSlotUI CreateMemberSlot(int positionIndex)
        {
            GameObject slotObj;

            if (_memberSlotPrefab != null)
            {
                slotObj = Instantiate(_memberSlotPrefab, _partyContainer);
            }
            else
            {
                slotObj = new GameObject($"MemberSlot_{positionIndex}");
                slotObj.transform.SetParent(_partyContainer, false);
                var slotUI = slotObj.AddComponent<PartyMemberSlotUI>();
                slotUI.CreateSimpleUI();
            }

            // Set position anchors
            var rect = slotObj.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = slotObj.AddComponent<RectTransform>();
            }

            rect.anchorMin = _slotAnchorsMin[positionIndex];
            rect.anchorMax = _slotAnchorsMax[positionIndex];
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return slotObj.GetComponent<PartyMemberSlotUI>();
        }

        private void ClearSlots()
        {
            foreach (var slot in _memberSlots)
            {
                if (slot != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            _memberSlots.Clear();
            _entityToSlotMap.Clear();
        }

        /// <summary>
        /// 특정 멤버의 HP 업데이트
        /// </summary>
        public void UpdateMemberHP(string entityId, int currentHP, int maxHP)
        {
            if (_entityToSlotMap.TryGetValue(entityId, out var slot))
            {
                slot.UpdateHP(currentHP, maxHP);
            }
        }

        /// <summary>
        /// 활성 멤버 표시 설정
        /// </summary>
        public void SetActiveMember(string entityId)
        {
            foreach (var kvp in _entityToSlotMap)
            {
                kvp.Value.SetActive(kvp.Key == entityId);
            }
        }

        /// <summary>
        /// 모든 멤버 활성 표시 해제
        /// </summary>
        public void ClearActiveMember()
        {
            foreach (var slot in _memberSlots)
            {
                slot.SetActive(false);
            }
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<HealEvent>(OnHeal);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<HealEvent>(OnHeal);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
        }

        private void OnDamageDealt(DamageDealtEvent e)
        {
            if (_entityToSlotMap.TryGetValue(e.TargetId, out var slot))
            {
                // Get current HP from the slot and subtract damage
                int newHP = Mathf.Max(0, slot.CurrentHP - e.Damage);
                slot.UpdateHP(newHP);
            }
        }

        private void OnHeal(HealEvent e)
        {
            if (_entityToSlotMap.TryGetValue(e.EntityId, out var slot))
            {
                int newHP = Mathf.Min(slot.MaxHP, slot.CurrentHP + e.Amount);
                slot.UpdateHP(newHP);
            }
        }

        private void OnTurnStarted(TurnStartedEvent e)
        {
            if (e.IsPlayerCharacter)
            {
                SetActiveMember(e.EntityId);
            }
        }

        private void OnTurnEnded(TurnEndedEvent e)
        {
            ClearActiveMember();
        }

        /// <summary>
        /// 파티 컨테이너 설정
        /// </summary>
        public void SetPartyContainer(Transform container)
        {
            _partyContainer = container;
        }

        public int MemberCount => _memberSlots.Count;
        public IReadOnlyList<PartyMemberSlotUI> MemberSlots => _memberSlots;
    }
}
