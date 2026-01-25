using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Combat;

namespace ProjectSS.UI
{
    /// <summary>
    /// 핸드 UI 관리
    /// Hand UI management
    /// </summary>
    public class HandUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private Transform cardContainer;

        [Header("Layout")]
        [SerializeField] private float cardSpacing = 120f;
        [SerializeField] private float arcHeight = 50f;
        [SerializeField] private float arcAngle = 5f;
        [SerializeField] private float maxWidth = 800f;

        private List<CardUI> cardUIs = new List<CardUI>();

        /// <summary>
        /// 핸드 새로고침
        /// Refresh hand display
        /// </summary>
        public void RefreshHand()
        {
            ClearHand();

            if (CombatManager.Instance?.DeckManager == null) return;

            var hand = CombatManager.Instance.DeckManager.Hand;

            foreach (var card in hand)
            {
                CreateCardUI(card);
            }

            ArrangeCards();
        }

        private void ClearHand()
        {
            foreach (var cardUI in cardUIs)
            {
                if (cardUI != null)
                {
                    Destroy(cardUI.gameObject);
                }
            }
            cardUIs.Clear();
        }

        private void CreateCardUI(CardInstance card)
        {
            if (cardPrefab == null || cardContainer == null) return;

            var cardObj = Instantiate(cardPrefab, cardContainer);
            var cardUI = cardObj.GetComponent<CardUI>();

            if (cardUI != null)
            {
                cardUI.Initialize(card, this);
                cardUIs.Add(cardUI);
            }
        }

        /// <summary>
        /// 카드 배열 (아크 형태)
        /// Arrange cards in arc
        /// </summary>
        private void ArrangeCards()
        {
            int count = cardUIs.Count;
            if (count == 0) return;

            // 실제 간격 계산 (최대 너비 고려)
            float totalWidth = (count - 1) * cardSpacing;
            float actualSpacing = cardSpacing;

            if (totalWidth > maxWidth)
            {
                actualSpacing = maxWidth / (count - 1);
                totalWidth = maxWidth;
            }

            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                var cardUI = cardUIs[i];
                var rectTransform = cardUI.GetComponent<RectTransform>();

                if (rectTransform == null) continue;

                // X 위치
                float x = startX + i * actualSpacing;

                // Y 위치 (아크)
                float normalizedPos = count > 1 ? (float)i / (count - 1) : 0.5f;
                float y = -arcHeight * (4f * normalizedPos * (normalizedPos - 1f)); // 포물선

                // 회전 (아크)
                float rotation = 0f;
                if (count > 1)
                {
                    rotation = Mathf.Lerp(arcAngle, -arcAngle, normalizedPos);
                }

                rectTransform.anchoredPosition = new Vector2(x, y);
                rectTransform.rotation = Quaternion.Euler(0, 0, rotation);
            }
        }

        /// <summary>
        /// 카드 사용 가능 여부 업데이트
        /// Update card playability
        /// </summary>
        public void UpdatePlayability()
        {
            foreach (var cardUI in cardUIs)
            {
                cardUI.UpdatePlayability();
            }
        }
    }
}
