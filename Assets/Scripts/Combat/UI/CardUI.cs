// Combat/UI/CardUI.cs
// 개별 카드 UI - 드래그 & 드롭 지원

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectSS.Core;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 개별 카드 UI
    /// 드래그 앤 드롭으로 타겟 선택 및 카드 사용
    /// </summary>
    public class CardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Card Display")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _costText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _typeText;
        [SerializeField] private Image _cardArtImage;

        [Header("Colors")]
        [SerializeField] private Color _attackColor = new Color(0.7f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _skillColor = new Color(0.3f, 0.5f, 0.7f, 1f);
        [SerializeField] private Color _powerColor = new Color(0.6f, 0.4f, 0.7f, 1f);
        [SerializeField] private Color _disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        [SerializeField] private Color _hoverColor = new Color(1f, 1f, 0.8f, 1f);

        // 카드 데이터
        private CardInstance _cardData;
        private PartyMemberCombat _owner;
        private bool _isPlayable = true;

        // 드래그 상태
        private bool _isDragging;
        private Vector3 _originalPosition;
        private Transform _originalParent;
        private int _originalSiblingIndex;
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;

        public CardInstance CardData => _cardData;
        public PartyMemberCombat Owner => _owner;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            _canvas = GetComponentInParent<Canvas>();
        }

        #region Initialization

        /// <summary>
        /// 카드 데이터로 초기화
        /// </summary>
        public void Initialize(CardInstance card, PartyMemberCombat owner)
        {
            _cardData = card;
            _owner = owner;

            UpdateDisplay();
            UpdatePlayableState();
        }

        /// <summary>
        /// 동적으로 UI 생성
        /// </summary>
        public void CreateSimpleUI()
        {
            var rect = GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = gameObject.AddComponent<RectTransform>();
            }
            rect.sizeDelta = new Vector2(100, 140);

            // 배경
            _backgroundImage = gameObject.AddComponent<Image>();
            _backgroundImage.color = _skillColor;

            // 코스트 텍스트 (좌상단)
            var costGo = new GameObject("CostText");
            costGo.transform.SetParent(transform, false);
            var costRect = costGo.AddComponent<RectTransform>();
            costRect.anchorMin = new Vector2(0, 0.85f);
            costRect.anchorMax = new Vector2(0.3f, 1f);
            costRect.offsetMin = Vector2.zero;
            costRect.offsetMax = Vector2.zero;
            var costBg = costGo.AddComponent<Image>();
            costBg.color = new Color(0.2f, 0.2f, 0.4f, 1f);
            _costText = costGo.AddComponent<Text>();
            _costText.alignment = TextAnchor.MiddleCenter;
            _costText.fontSize = 16;
            _costText.fontStyle = FontStyle.Bold;
            _costText.color = Color.white;
            _costText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 이름 텍스트 (상단)
            var nameGo = new GameObject("NameText");
            nameGo.transform.SetParent(transform, false);
            var nameRect = nameGo.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.7f);
            nameRect.anchorMax = new Vector2(1, 0.85f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            _nameText = nameGo.AddComponent<Text>();
            _nameText.alignment = TextAnchor.MiddleCenter;
            _nameText.fontSize = 12;
            _nameText.color = Color.white;
            _nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 카드 타입 (중앙)
            var typeGo = new GameObject("TypeText");
            typeGo.transform.SetParent(transform, false);
            var typeRect = typeGo.AddComponent<RectTransform>();
            typeRect.anchorMin = new Vector2(0, 0.5f);
            typeRect.anchorMax = new Vector2(1, 0.65f);
            typeRect.offsetMin = Vector2.zero;
            typeRect.offsetMax = Vector2.zero;
            _typeText = typeGo.AddComponent<Text>();
            _typeText.alignment = TextAnchor.MiddleCenter;
            _typeText.fontSize = 10;
            _typeText.color = Color.yellow;
            _typeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 설명 텍스트 (하단)
            var descGo = new GameObject("DescText");
            descGo.transform.SetParent(transform, false);
            var descRect = descGo.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.05f, 0.05f);
            descRect.anchorMax = new Vector2(0.95f, 0.45f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;
            _descriptionText = descGo.AddComponent<Text>();
            _descriptionText.alignment = TextAnchor.MiddleCenter;
            _descriptionText.fontSize = 10;
            _descriptionText.color = Color.white;
            _descriptionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            UpdateDisplay();
        }

        #endregion

        #region Display Update

        public void UpdateDisplay()
        {
            if (_cardData == null) return;

            if (_nameText != null)
            {
                _nameText.text = _cardData.CardName;
            }

            if (_costText != null)
            {
                _costText.text = _cardData.EnergyCost.ToString();
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = _cardData.Description;
            }

            if (_typeText != null)
            {
                _typeText.text = GetCardTypeText(_cardData.CardType);
            }

            UpdateCardColor();
        }

        public void UpdatePlayableState()
        {
            if (_owner == null || _cardData == null)
            {
                _isPlayable = false;
            }
            else
            {
                _isPlayable = _owner.CanPlayCard(_cardData) && CombatManager.Instance?.IsPlayerTurn == true;
            }

            // 시각적 피드백
            if (_backgroundImage != null)
            {
                _backgroundImage.color = _isPlayable ? GetCardTypeColor(_cardData.CardType) : _disabledColor;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _isPlayable ? 1f : 0.6f;
            }
        }

        private void UpdateCardColor()
        {
            if (_backgroundImage != null && _cardData != null)
            {
                _backgroundImage.color = GetCardTypeColor(_cardData.CardType);
            }
        }

        private Color GetCardTypeColor(CardType type)
        {
            return type switch
            {
                CardType.Attack => _attackColor,
                CardType.Skill => _skillColor,
                CardType.Power => _powerColor,
                _ => _skillColor
            };
        }

        private string GetCardTypeText(CardType type)
        {
            return type switch
            {
                CardType.Attack => "공격",
                CardType.Skill => "스킬",
                CardType.Power => "파워",
                _ => "???"
            };
        }

        #endregion

        #region Drag Handlers

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isPlayable) return;

            _isDragging = true;
            _originalPosition = transform.position;
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();

            // 드래그 중 최상단에 표시
            transform.SetParent(_canvas.transform);
            transform.SetAsLastSibling();

            _canvasGroup.blocksRaycasts = false;

            // 타겟팅 시스템 활성화
            if (TargetingSystem.Instance != null)
            {
                TargetingSystem.Instance.StartTargeting(_cardData);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            _isDragging = false;
            _canvasGroup.blocksRaycasts = true;

            // 타겟 확인
            ICombatEntity target = null;
            if (TargetingSystem.Instance != null)
            {
                target = TargetingSystem.Instance.GetTargetUnderPointer(eventData);
                TargetingSystem.Instance.EndTargeting();
            }

            // 유효한 타겟에 드롭했으면 카드 사용
            if (target != null && _isPlayable)
            {
                TryPlayCard(target);
            }
            else
            {
                // 원래 위치로 복귀
                ReturnToHand();
            }
        }

        private void ReturnToHand()
        {
            transform.SetParent(_originalParent);
            transform.SetSiblingIndex(_originalSiblingIndex);
            transform.position = _originalPosition;
        }

        #endregion

        #region Card Playing

        private void TryPlayCard(ICombatEntity target)
        {
            if (_owner == null || _cardData == null) return;

            bool success = CombatManager.Instance?.TryPlayCard(_owner, _cardData, target) ?? false;

            if (success)
            {
                // 카드 사용 성공 - CardHandUI에서 제거됨
                Debug.Log($"[CardUI] Card played: {_cardData.CardName} -> {target.DisplayName}");
            }
            else
            {
                // 실패 시 원래 위치로
                ReturnToHand();
            }
        }

        #endregion

        #region Pointer Events

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isDragging && _isPlayable)
            {
                // 호버 효과
                transform.localScale = Vector3.one * 1.1f;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                transform.localScale = Vector3.one;
            }
        }

        #endregion
    }
}
