using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 덱 더미 UI (뽑을 덱 / 버린 덱)
    /// Deck pile UI for draw pile and discard pile display
    /// </summary>
    public class DeckPileUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _deckIcon;
        [SerializeField] private Text _countText;
        [SerializeField] private Text _labelText;

        [Header("Settings")]
        [SerializeField] private bool _isDrawPile = true;
        [SerializeField] private Color _drawPileColor = new Color(0.25f, 0.5f, 0.63f, 1f);    // #285064
        [SerializeField] private Color _discardPileColor = new Color(0.5f, 0.35f, 0.25f, 1f); // Brown-ish

        private int _cardCount;

        private void Start()
        {
            SubscribeToEvents();
            UpdateLabel();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 덱 타입 초기화
        /// </summary>
        public void Initialize(bool isDrawPile)
        {
            _isDrawPile = isDrawPile;
            UpdateLabel();
            UpdateVisuals();
        }

        /// <summary>
        /// 카드 수 업데이트
        /// </summary>
        public void UpdateCount(int count)
        {
            _cardCount = count;
            if (_countText != null)
            {
                _countText.text = $"[{_cardCount}]";
            }
        }

        private void UpdateLabel()
        {
            if (_labelText != null)
            {
                _labelText.text = _isDrawPile ? "뽑을 덱" : "버린 덱";
            }
        }

        private void UpdateVisuals()
        {
            if (_deckIcon != null)
            {
                _deckIcon.color = _isDrawPile ? _drawPileColor : _discardPileColor;
            }
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DeckPileChangedEvent>(OnDeckPileChanged);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<DeckPileChangedEvent>(OnDeckPileChanged);
        }

        private void OnDeckPileChanged(DeckPileChangedEvent e)
        {
            if (_isDrawPile)
            {
                UpdateCount(e.DrawPileCount);
            }
            else
            {
                UpdateCount(e.DiscardPileCount);
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

            // Background panel - check if Image already exists (from scene builder)
            var bgImage = GetComponent<Image>();
            if (bgImage == null)
            {
                bgImage = gameObject.AddComponent<Image>();
            }
            bgImage.color = _isDrawPile ? _drawPileColor : _discardPileColor;

            // Deck Icon (card back representation)
            var iconObj = new GameObject("DeckIcon");
            iconObj.transform.SetParent(transform, false);
            _deckIcon = iconObj.AddComponent<Image>();
            _deckIcon.color = new Color(0.2f, 0.2f, 0.3f, 1f);

            var iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.15f, 0.40f);
            iconRect.anchorMax = new Vector2(0.85f, 0.90f);
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            // Label Text (뽑을 덱 / 버린 덱)
            var labelObj = new GameObject("LabelText");
            labelObj.transform.SetParent(transform, false);
            _labelText = labelObj.AddComponent<Text>();
            _labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _labelText.fontSize = 12;
            _labelText.color = Color.white;
            _labelText.alignment = TextAnchor.MiddleCenter;
            _labelText.text = _isDrawPile ? "뽑을 덱" : "버린 덱";

            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.05f, 0.18f);
            labelRect.anchorMax = new Vector2(0.95f, 0.38f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            // Count Text
            var countObj = new GameObject("CountText");
            countObj.transform.SetParent(transform, false);
            _countText = countObj.AddComponent<Text>();
            _countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _countText.fontSize = 14;
            _countText.fontStyle = FontStyle.Bold;
            _countText.color = Color.white;
            _countText.alignment = TextAnchor.MiddleCenter;
            _countText.text = "[0]";

            var countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.05f, 0.02f);
            countRect.anchorMax = new Vector2(0.95f, 0.18f);
            countRect.offsetMin = Vector2.zero;
            countRect.offsetMax = Vector2.zero;
        }

        public bool IsDrawPile => _isDrawPile;
        public int CardCount => _cardCount;
    }
}
