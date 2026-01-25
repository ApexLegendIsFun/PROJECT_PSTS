using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ProjectSS.Data;
using ProjectSS.Combat;

namespace ProjectSS.UI
{
    /// <summary>
    /// 개별 카드 UI
    /// Individual card UI
    /// </summary>
    public class CardUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [Header("Card Elements")]
        [SerializeField] private Image cardBackground;
        [SerializeField] private Image artworkImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI typeText;

        [Header("Visual Settings")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float hoverYOffset = 30f;

        private CardInstance cardInstance;
        private RectTransform rectTransform;
        private Vector3 originalPosition;
        private Vector3 originalScale;
        private int originalSiblingIndex;
        private bool isDragging;
        private HandUI handUI;

        public CardInstance CardInstance => cardInstance;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = transform.localScale;
        }

        /// <summary>
        /// 카드 데이터로 초기화
        /// Initialize with card data
        /// </summary>
        public void Initialize(CardInstance card, HandUI hand)
        {
            cardInstance = card;
            handUI = hand;
            UpdateVisuals();
        }

        /// <summary>
        /// 비주얼 업데이트
        /// Update visuals
        /// </summary>
        public void UpdateVisuals()
        {
            if (cardInstance == null) return;

            var data = cardInstance.EffectiveData;

            if (nameText != null)
                nameText.text = data.cardName;

            if (costText != null)
                costText.text = data.energyCost.ToString();

            if (descriptionText != null)
                descriptionText.text = data.GetFullDescription();

            if (typeText != null)
                typeText.text = data.cardType.ToString();

            if (cardBackground != null)
                cardBackground.color = data.GetTypeColor();

            if (artworkImage != null && data.artwork != null)
                artworkImage.sprite = data.artwork;

            // 사용 가능 여부에 따른 시각적 표시
            UpdatePlayability();
        }

        /// <summary>
        /// 사용 가능 여부 업데이트
        /// Update playability visual
        /// </summary>
        public void UpdatePlayability()
        {
            if (CombatManager.Instance?.EnergySystem == null) return;

            bool canPlay = cardInstance.CanPlay(CombatManager.Instance.EnergySystem);

            if (cardBackground != null)
            {
                var color = cardBackground.color;
                color.a = canPlay ? 1f : 0.5f;
                cardBackground.color = color;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // 클릭 시 카드 상세 정보 표시 (우클릭)
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                ShowCardDetail();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (isDragging) return;

            originalPosition = rectTransform.anchoredPosition;
            originalSiblingIndex = transform.GetSiblingIndex();

            // 호버 효과
            transform.localScale = originalScale * hoverScale;
            rectTransform.anchoredPosition = originalPosition + new Vector3(0, hoverYOffset, 0);
            transform.SetAsLastSibling();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (isDragging) return;

            // 원래대로 복구
            transform.localScale = originalScale;
            rectTransform.anchoredPosition = originalPosition;
            transform.SetSiblingIndex(originalSiblingIndex);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            originalPosition = rectTransform.anchoredPosition;

            // 드래그 중 반투명하게
            if (cardBackground != null)
            {
                var color = cardBackground.color;
                color.a = 0.7f;
                cardBackground.color = color;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;

            // 타겟 확인 및 카드 사용 시도
            TryPlayCard(eventData);

            // 원래 위치로 복구
            rectTransform.anchoredPosition = originalPosition;
            transform.localScale = originalScale;

            UpdateVisuals();
        }

        private void TryPlayCard(PointerEventData eventData)
        {
            if (CombatManager.Instance == null || !CombatManager.Instance.IsCombatActive) return;
            if (!CombatManager.Instance.TurnManager.IsPlayerTurn) return;

            // 드래그 위치에 적이 있는지 확인
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            ICombatEntity target = null;

            foreach (var result in results)
            {
                var enemyUI = result.gameObject.GetComponent<EnemyUIComponent>();
                if (enemyUI != null)
                {
                    target = enemyUI.Enemy;
                    break;
                }
            }

            // 카드 사용 시도
            bool played = CombatManager.Instance.PlayCard(cardInstance, target);

            if (played && handUI != null)
            {
                handUI.RefreshHand();
            }
        }

        private void ShowCardDetail()
        {
            // TODO: 카드 상세 팝업 표시
            Debug.Log($"Show detail for: {cardInstance?.EffectiveData?.cardName}");
        }
    }

    /// <summary>
    /// 적 UI 컴포넌트 (타겟팅용)
    /// Enemy UI component (for targeting)
    /// </summary>
    public class EnemyUIComponent : MonoBehaviour
    {
        public EnemyCombat Enemy { get; set; }
    }
}
