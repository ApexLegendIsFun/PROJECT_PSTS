using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Combat;
using ProjectSS.Data;
using ProjectSS.Core;

namespace ProjectSS.UI
{
    /// <summary>
    /// 개별 캐릭터 슬롯 UI
    /// Individual character slot UI
    ///
    /// TRIAD: 파티원 1명의 정보 표시 (초상화, HP, Focus, 상태)
    /// </summary>
    public class CharacterSlotUI : MonoBehaviour
    {
        [Header("Character Info")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Image classColorBorder;
        [SerializeField] private TextMeshProUGUI characterNameText;

        [Header("Health Display")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFill;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Block Display")]
        [SerializeField] private GameObject blockContainer;
        [SerializeField] private TextMeshProUGUI blockText;

        [Header("Focus Display")]
        [SerializeField] private FocusIndicatorUI focusIndicator;

        [Header("Position Indicator")]
        [SerializeField] private GameObject activeIndicator;
        [SerializeField] private GameObject standbyIndicator;
        [SerializeField] private Image incapacitatedOverlay;

        [Header("Tag-In Button")]
        [SerializeField] private Button tagInButton;
        [SerializeField] private TextMeshProUGUI tagInCostText;

        [Header("Colors")]
        [SerializeField] private Color healthyColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color damagedColor = new Color(0.9f, 0.6f, 0.1f);
        [SerializeField] private Color criticalColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color incapacitatedColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);

        private PartyMemberCombat linkedMember;
        private CharacterClass characterClass;

        /// <summary>
        /// 연결된 파티 멤버
        /// Linked party member
        /// </summary>
        public PartyMemberCombat LinkedMember => linkedMember;

        /// <summary>
        /// 캐릭터 클래스
        /// Character class
        /// </summary>
        public CharacterClass CharacterClass => characterClass;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        private void Start()
        {
            SetupTagInButton();
        }

        /// <summary>
        /// 파티 멤버와 연결
        /// Link to party member
        /// </summary>
        public void SetMember(PartyMemberCombat member)
        {
            linkedMember = member;

            if (member == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            characterClass = member.Class;

            // 초상화 및 클래스 색상 설정
            // Set portrait and class color
            if (member.ClassData != null)
            {
                if (portraitImage != null && member.ClassData.portrait != null)
                {
                    portraitImage.sprite = member.ClassData.portrait;
                }

                if (classColorBorder != null)
                {
                    classColorBorder.color = member.ClassData.classColor;
                }

                if (characterNameText != null)
                {
                    characterNameText.text = member.ClassData.className;
                }
            }

            RefreshDisplay();
        }

        /// <summary>
        /// 전체 디스플레이 새로고침
        /// Refresh entire display
        /// </summary>
        public void RefreshDisplay()
        {
            if (linkedMember == null) return;

            UpdateHealthDisplay();
            UpdateBlockDisplay();
            UpdateFocusDisplay();
            UpdatePositionDisplay();
            UpdateTagInButton();
        }

        /// <summary>
        /// 체력 표시 업데이트
        /// Update health display
        /// </summary>
        private void UpdateHealthDisplay()
        {
            if (linkedMember == null) return;

            int current = linkedMember.CurrentHealth;
            int max = linkedMember.MaxHealth;

            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }

            if (healthText != null)
            {
                healthText.text = $"{current}/{max}";
            }

            if (healthFill != null)
            {
                float ratio = max > 0 ? (float)current / max : 0f;
                if (ratio > 0.5f)
                    healthFill.color = healthyColor;
                else if (ratio > 0.25f)
                    healthFill.color = damagedColor;
                else
                    healthFill.color = criticalColor;
            }
        }

        /// <summary>
        /// 블록 표시 업데이트
        /// Update block display
        /// </summary>
        private void UpdateBlockDisplay()
        {
            if (linkedMember == null) return;

            int block = linkedMember.Block;

            if (blockContainer != null)
            {
                blockContainer.SetActive(block > 0);
            }

            if (blockText != null)
            {
                blockText.text = block.ToString();
            }
        }

        /// <summary>
        /// Focus 표시 업데이트
        /// Update Focus display
        /// </summary>
        private void UpdateFocusDisplay()
        {
            if (focusIndicator != null && linkedMember != null)
            {
                focusIndicator.UpdateFocus(linkedMember.FocusStacks);
            }
        }

        /// <summary>
        /// 위치 표시 업데이트 (Active/Standby)
        /// Update position display
        /// </summary>
        private void UpdatePositionDisplay()
        {
            if (linkedMember == null) return;

            bool isActive = linkedMember.IsActive;
            bool isIncapacitated = linkedMember.IsIncapacitated;

            if (activeIndicator != null)
            {
                activeIndicator.SetActive(isActive && !isIncapacitated);
            }

            if (standbyIndicator != null)
            {
                standbyIndicator.SetActive(!isActive && !isIncapacitated);
            }

            if (incapacitatedOverlay != null)
            {
                incapacitatedOverlay.gameObject.SetActive(isIncapacitated);
                incapacitatedOverlay.color = incapacitatedColor;
            }
        }

        /// <summary>
        /// Tag-In 버튼 업데이트
        /// Update Tag-In button
        /// </summary>
        private void UpdateTagInButton()
        {
            if (tagInButton == null || linkedMember == null) return;

            // Tag-In 버튼은 Standby + 행동 가능 상태에서만 표시
            // Tag-In button only shown for Standby + can act
            bool showButton = linkedMember.IsStandby && linkedMember.CanAct;
            tagInButton.gameObject.SetActive(showButton);

            if (!showButton) return;

            // Tag-In 가능 여부 확인
            // Check if Tag-In is possible
            bool canTagIn = PartyManager.Instance?.CanTagIn(linkedMember) ?? false;
            tagInButton.interactable = canTagIn;

            // 비용 표시
            // Show cost
            if (tagInCostText != null)
            {
                int cost = PartyManager.Instance?.GetTagInCost(linkedMember) ?? 1;
                tagInCostText.text = cost > 0 ? $"E:{cost}" : "FREE";
            }
        }

        private void SetupTagInButton()
        {
            if (tagInButton != null)
            {
                tagInButton.onClick.AddListener(OnTagInClicked);
            }
        }

        private void OnTagInClicked()
        {
            if (linkedMember == null) return;

            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.ExecuteTagIn(linkedMember);
            }
        }

        #region Event Handling

        private void SubscribeEvents()
        {
            EventBus.Subscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Subscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Subscribe<FocusChangedEvent>(OnFocusChanged);
            EventBus.Subscribe<PositionChangedEvent>(OnPositionChanged);
            EventBus.Subscribe<CharacterIncapacitatedEvent>(OnCharacterIncapacitated);
            EventBus.Subscribe<CharacterRevivedEvent>(OnCharacterRevived);
            EventBus.Subscribe<TagInEvent>(OnTagIn);
            EventBus.Subscribe<EnergyChangedEvent>(OnEnergyChanged);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Unsubscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Unsubscribe<FocusChangedEvent>(OnFocusChanged);
            EventBus.Unsubscribe<PositionChangedEvent>(OnPositionChanged);
            EventBus.Unsubscribe<CharacterIncapacitatedEvent>(OnCharacterIncapacitated);
            EventBus.Unsubscribe<CharacterRevivedEvent>(OnCharacterRevived);
            EventBus.Unsubscribe<TagInEvent>(OnTagIn);
            EventBus.Unsubscribe<EnergyChangedEvent>(OnEnergyChanged);
        }

        private void OnDamageTaken(DamageTakenEvent evt)
        {
            if (linkedMember != null && evt.EntityId == linkedMember.EntityId)
            {
                UpdateHealthDisplay();
                UpdateBlockDisplay();
            }
        }

        private void OnBlockGained(BlockGainedEvent evt)
        {
            if (linkedMember != null && evt.EntityId == linkedMember.EntityId)
            {
                UpdateBlockDisplay();
            }
        }

        private void OnFocusChanged(FocusChangedEvent evt)
        {
            if (linkedMember != null && evt.CharacterClass == characterClass)
            {
                UpdateFocusDisplay();
                UpdateTagInButton();
            }
        }

        private void OnPositionChanged(PositionChangedEvent evt)
        {
            if (linkedMember != null && evt.CharacterClass == characterClass)
            {
                UpdatePositionDisplay();
                UpdateTagInButton();
            }
        }

        private void OnCharacterIncapacitated(CharacterIncapacitatedEvent evt)
        {
            if (linkedMember != null && evt.CharacterClass == characterClass)
            {
                RefreshDisplay();
            }
        }

        private void OnCharacterRevived(CharacterRevivedEvent evt)
        {
            if (linkedMember != null && evt.CharacterClass == characterClass)
            {
                RefreshDisplay();
            }
        }

        private void OnTagIn(TagInEvent evt)
        {
            // Tag-In 발생 시 전체 새로고침
            // Refresh all on Tag-In
            RefreshDisplay();
        }

        private void OnEnergyChanged(EnergyChangedEvent evt)
        {
            // 에너지 변경 시 Tag-In 버튼 상태 업데이트
            // Update Tag-In button state on energy change
            UpdateTagInButton();
        }

        #endregion
    }
}
