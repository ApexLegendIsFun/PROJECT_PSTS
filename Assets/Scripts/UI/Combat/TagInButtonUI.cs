using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Combat;
using ProjectSS.Data;
using ProjectSS.Core;

namespace ProjectSS.UI
{
    /// <summary>
    /// Tag-In 버튼 UI
    /// Tag-In button UI
    ///
    /// TRIAD: Standby 캐릭터를 Active로 교체하는 버튼
    /// Button to swap Standby character to Active
    /// </summary>
    public class TagInButtonUI : MonoBehaviour
    {
        [Header("Button")]
        [SerializeField] private Button button;

        [Header("Display")]
        [SerializeField] private Image buttonBackground;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image classColorAccent;

        [Header("Colors")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.5f, 0.7f);
        [SerializeField] private Color freeColor = new Color(0.2f, 0.7f, 0.3f);
        [SerializeField] private Color disabledColor = new Color(0.4f, 0.4f, 0.4f);
        [SerializeField] private Color highlightColor = new Color(0.4f, 0.6f, 0.8f);

        [Header("Animation")]
        [SerializeField] private bool pulseWhenFree = true;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseIntensity = 0.1f;

        private PartyMemberCombat linkedMember;
        private bool isFreeTagIn;

        /// <summary>
        /// 연결된 파티 멤버
        /// Linked party member
        /// </summary>
        public PartyMemberCombat LinkedMember => linkedMember;

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
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClicked);
            }
        }

        private void Update()
        {
            if (pulseWhenFree && isFreeTagIn && buttonBackground != null)
            {
                AnimateFreeTagIn();
            }
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

            // 클래스 색상 설정
            // Set class color
            if (classColorAccent != null && member.ClassData != null)
            {
                classColorAccent.color = member.ClassData.classColor;
            }

            RefreshDisplay();
        }

        /// <summary>
        /// 디스플레이 새로고침
        /// Refresh display
        /// </summary>
        public void RefreshDisplay()
        {
            if (linkedMember == null)
            {
                gameObject.SetActive(false);
                return;
            }

            // Standby가 아니면 숨김
            // Hide if not Standby
            if (!linkedMember.IsStandby || !linkedMember.CanAct)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            // Tag-In 가능 여부
            // Check Tag-In availability
            bool canTagIn = PartyManager.Instance?.CanTagIn(linkedMember) ?? false;
            int cost = PartyManager.Instance?.GetTagInCost(linkedMember) ?? 1;
            isFreeTagIn = cost == 0;

            // 버튼 상태
            // Button state
            if (button != null)
            {
                button.interactable = canTagIn;
            }

            // 배경 색상
            // Background color
            if (buttonBackground != null)
            {
                if (!canTagIn)
                {
                    buttonBackground.color = disabledColor;
                }
                else if (isFreeTagIn)
                {
                    buttonBackground.color = freeColor;
                }
                else
                {
                    buttonBackground.color = normalColor;
                }
            }

            // 버튼 텍스트
            // Button text
            if (buttonText != null)
            {
                buttonText.text = "TAG IN";
            }

            // 비용 텍스트
            // Cost text
            if (costText != null)
            {
                if (isFreeTagIn)
                {
                    costText.text = "FREE!";
                    costText.color = Color.yellow;
                }
                else
                {
                    costText.text = $"E:{cost}";
                    costText.color = canTagIn ? Color.white : Color.gray;
                }
            }
        }

        private void OnButtonClicked()
        {
            if (linkedMember == null) return;

            if (CombatManager.Instance != null && CombatManager.Instance.TurnManager.IsPlayerTurn)
            {
                bool success = CombatManager.Instance.ExecuteTagIn(linkedMember);
                if (success)
                {
                    // Tag-In 성공 효과
                    // Tag-In success effect
                    PlayTagInEffect();
                }
            }
        }

        /// <summary>
        /// Tag-In 성공 효과
        /// Tag-In success effect
        /// </summary>
        private void PlayTagInEffect()
        {
            StartCoroutine(TagInEffectCoroutine());
        }

        private System.Collections.IEnumerator TagInEffectCoroutine()
        {
            if (buttonBackground != null)
            {
                Color originalColor = buttonBackground.color;
                buttonBackground.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                buttonBackground.color = originalColor;
            }
        }

        /// <summary>
        /// 무료 Tag-In 펄스 애니메이션
        /// Free Tag-In pulse animation
        /// </summary>
        private void AnimateFreeTagIn()
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
            Color baseColor = freeColor;
            buttonBackground.color = new Color(
                Mathf.Min(baseColor.r * pulse, 1f),
                Mathf.Min(baseColor.g * pulse, 1f),
                Mathf.Min(baseColor.b * pulse, 1f),
                baseColor.a
            );
        }

        #region Event Handling

        private void SubscribeEvents()
        {
            EventBus.Subscribe<FocusChangedEvent>(OnFocusChanged);
            EventBus.Subscribe<EnergyChangedEvent>(OnEnergyChanged);
            EventBus.Subscribe<PositionChangedEvent>(OnPositionChanged);
            EventBus.Subscribe<TagInEvent>(OnTagIn);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<FocusChangedEvent>(OnFocusChanged);
            EventBus.Unsubscribe<EnergyChangedEvent>(OnEnergyChanged);
            EventBus.Unsubscribe<PositionChangedEvent>(OnPositionChanged);
            EventBus.Unsubscribe<TagInEvent>(OnTagIn);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        }

        private void OnFocusChanged(FocusChangedEvent evt)
        {
            if (linkedMember != null && evt.CharacterClass == linkedMember.Class)
            {
                RefreshDisplay();
            }
        }

        private void OnEnergyChanged(EnergyChangedEvent evt)
        {
            RefreshDisplay();
        }

        private void OnPositionChanged(PositionChangedEvent evt)
        {
            if (linkedMember != null && evt.CharacterClass == linkedMember.Class)
            {
                RefreshDisplay();
            }
        }

        private void OnTagIn(TagInEvent evt)
        {
            RefreshDisplay();
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
