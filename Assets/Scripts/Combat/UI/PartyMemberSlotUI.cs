using UnityEngine;
using UnityEngine.UI;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 파티원 개별 슬롯 UI (스프라이트 + HP + 이름)
    /// Individual party member slot UI
    /// </summary>
    public class PartyMemberSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private HeartHPDisplay _heartHP;
        [SerializeField] private Image _characterSprite;
        [SerializeField] private Text _memberNameText;
        [SerializeField] private Image _activeIndicator;

        [Header("Settings")]
        [SerializeField] private Color _activeColor = new Color(1f, 1f, 0f, 0.5f);
        [SerializeField] private Color _inactiveColor = new Color(0f, 0f, 0f, 0f);

        [Header("Targeting Highlight")]
        [SerializeField] private Image _targetHighlight;
        private bool _isTargetHighlighted;

        private string _entityId;
        private int _currentHP;
        private int _maxHP;

        /// <summary>
        /// 파티 멤버 정보로 초기화
        /// </summary>
        public void Initialize(PartyMemberCombat member)
        {
            if (member == null) return;

            _entityId = member.EntityId;
            _currentHP = member.CurrentHP;
            _maxHP = member.MaxHP;

            // Update name
            if (_memberNameText != null)
            {
                _memberNameText.text = member.DisplayName;
            }

            // Update sprite placeholder (CharacterData not available on PartyMemberCombat)
            // Sprite will use default placeholder color set in CreateSimpleUI()

            // Update HP display
            if (_heartHP != null)
            {
                _heartHP.Initialize(_currentHP, _maxHP);
            }

            // Default to inactive
            SetActive(false);
        }

        /// <summary>
        /// HP 업데이트
        /// </summary>
        public void UpdateHP(int currentHP, int maxHP)
        {
            _currentHP = currentHP;
            _maxHP = maxHP;

            if (_heartHP != null)
            {
                _heartHP.UpdateHP(_currentHP, _maxHP);
            }
        }

        /// <summary>
        /// HP만 업데이트 (최대 HP 유지)
        /// </summary>
        public void UpdateHP(int currentHP)
        {
            _currentHP = currentHP;

            if (_heartHP != null)
            {
                _heartHP.UpdateHP(_currentHP);
            }
        }

        /// <summary>
        /// 활성 상태 설정 (현재 턴인 캐릭터 표시)
        /// </summary>
        public void SetActive(bool isActive)
        {
            if (_activeIndicator != null)
            {
                _activeIndicator.color = isActive ? _activeColor : _inactiveColor;
            }
        }

        /// <summary>
        /// 타겟팅 하이라이트 설정 (수비/지원 카드 드래그 시)
        /// Set targeting highlight for defense/support card dragging
        /// </summary>
        public void SetTargetHighlight(bool highlight, Color color)
        {
            _isTargetHighlighted = highlight;

            if (_targetHighlight != null)
            {
                _targetHighlight.enabled = highlight;
                if (highlight)
                {
                    _targetHighlight.color = color;
                }
            }
            else if (_activeIndicator != null)
            {
                // Fallback: _targetHighlight가 없으면 activeIndicator 재사용
                if (highlight)
                {
                    _activeIndicator.color = color;
                }
                else if (!_isTargetHighlighted)
                {
                    _activeIndicator.color = _inactiveColor;
                }
            }
        }

        /// <summary>
        /// UI가 없을 때 동적으로 생성
        /// </summary>
        public void CreateSimpleUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            // Active Indicator (background glow)
            var indicatorObj = new GameObject("ActiveIndicator");
            indicatorObj.transform.SetParent(transform, false);
            _activeIndicator = indicatorObj.AddComponent<Image>();
            _activeIndicator.color = _inactiveColor;

            var indicatorRect = indicatorObj.GetComponent<RectTransform>();
            indicatorRect.anchorMin = Vector2.zero;
            indicatorRect.anchorMax = Vector2.one;
            indicatorRect.offsetMin = new Vector2(-5, -5);
            indicatorRect.offsetMax = new Vector2(5, 5);

            // Target Highlight (타겟팅 시 표시되는 오버레이)
            var targetHighlightObj = new GameObject("TargetHighlight");
            targetHighlightObj.transform.SetParent(transform, false);
            _targetHighlight = targetHighlightObj.AddComponent<Image>();
            _targetHighlight.color = Color.clear;
            _targetHighlight.enabled = false;
            _targetHighlight.raycastTarget = false;

            var targetRect = targetHighlightObj.GetComponent<RectTransform>();
            targetRect.anchorMin = Vector2.zero;
            targetRect.anchorMax = Vector2.one;
            targetRect.offsetMin = new Vector2(-5, -5);
            targetRect.offsetMax = new Vector2(5, 5);

            // TargetHighlight가 ActiveIndicator 위에 표시되도록 sibling order 설정
            targetHighlightObj.transform.SetAsLastSibling();

            // Heart HP Display (top 15%)
            var hpObj = new GameObject("HeartHP");
            hpObj.transform.SetParent(transform, false);
            _heartHP = hpObj.AddComponent<HeartHPDisplay>();
            _heartHP.CreateSimpleUI();

            var hpRect = hpObj.GetComponent<RectTransform>();
            hpRect.anchorMin = new Vector2(0.1f, 0.85f);
            hpRect.anchorMax = new Vector2(0.9f, 0.98f);
            hpRect.offsetMin = Vector2.zero;
            hpRect.offsetMax = Vector2.zero;

            // Character Sprite (center 65%)
            var spriteObj = new GameObject("CharacterSprite");
            spriteObj.transform.SetParent(transform, false);
            _characterSprite = spriteObj.AddComponent<Image>();
            _characterSprite.color = new Color(0.5f, 0.5f, 0.6f, 1f); // Placeholder color
            _characterSprite.preserveAspect = true;

            var spriteRect = spriteObj.GetComponent<RectTransform>();
            spriteRect.anchorMin = new Vector2(0.1f, 0.18f);
            spriteRect.anchorMax = new Vector2(0.9f, 0.82f);
            spriteRect.offsetMin = Vector2.zero;
            spriteRect.offsetMax = Vector2.zero;

            // Member Name (bottom 15%)
            var nameObj = new GameObject("MemberName");
            nameObj.transform.SetParent(transform, false);
            _memberNameText = nameObj.AddComponent<Text>();
            _memberNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _memberNameText.fontSize = 14;
            _memberNameText.color = Color.white;
            _memberNameText.alignment = TextAnchor.MiddleCenter;
            _memberNameText.text = "캐릭터";

            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0f, 0.02f);
            nameRect.anchorMax = new Vector2(1f, 0.16f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
        }

        public string EntityId => _entityId;
        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
    }
}
