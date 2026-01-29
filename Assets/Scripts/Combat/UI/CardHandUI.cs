// Combat/UI/CardHandUI.cs
// 카드 핸드 UI - 현재 턴 캐릭터의 핸드 표시

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 카드 핸드 UI
    /// 현재 턴 캐릭터의 핸드에 있는 카드들을 표시
    /// </summary>
    public class CardHandUI : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform _cardContainer;
        [SerializeField] private GameObject _cardUIPrefab;

        [Header("Layout")]
        [SerializeField] private float _cardSpacing = -30f;    // 겹치는 효과
        [SerializeField] private float _maxFanAngle = 10f;     // 최대 부채꼴 각도 (±10°)
        [SerializeField] private float _fanYOffset = 20f;      // 부채꼴 Y 오프셋 (포물선 높이)
        [SerializeField] private float _cardWidth = 100f;      // 카드 너비
        [SerializeField] private bool _useFanLayout = true;

        // 현재 표시 중인 카드 UI들
        private Dictionary<string, CardUI> _cardUIMap = new();
        private PartyMemberCombat _currentCharacter;

        private void Start()
        {
            if (_cardContainer == null)
            {
                _cardContainer = transform;
            }

            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region Event Subscription

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<EnergyChangedEvent>(OnEnergyChanged);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<EnergyChangedEvent>(OnEnergyChanged);
        }

        #endregion

        #region Event Handlers

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            // 플레이어 캐릭터 턴일 때만 핸드 표시
            if (!evt.IsPlayerCharacter) return;

            _currentCharacter = GetPartyMember(evt.EntityId);
            if (_currentCharacter != null)
            {
                RefreshHand();
            }
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            // 턴 종료 시 핸드 비우기
            if (evt.EntityId == _currentCharacter?.EntityId)
            {
                ClearHand();
                _currentCharacter = null;
            }
        }

        private void OnCardDrawn(CardDrawnEvent evt)
        {
            // 현재 캐릭터의 카드만 처리
            if (_currentCharacter == null || evt.CharacterId != _currentCharacter.EntityId)
            {
                return;
            }

            // 전체 핸드 새로고침 (간단한 구현)
            RefreshHand();
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            // 현재 캐릭터의 카드만 처리
            if (_currentCharacter == null || evt.CharacterId != _currentCharacter.EntityId)
            {
                return;
            }

            // 해당 카드 UI 제거
            RemoveCard(evt.CardId);

            // 레이아웃 업데이트
            UpdateLayout();
        }

        private void OnEnergyChanged(EnergyChangedEvent evt)
        {
            // 에너지 변경 시 카드 플레이 가능 상태 업데이트
            if (_currentCharacter != null && evt.CharacterId == _currentCharacter.EntityId)
            {
                UpdateAllCardsPlayableState();
            }
        }

        #endregion

        #region Hand Management

        /// <summary>
        /// 핸드 전체 새로고침
        /// </summary>
        public void RefreshHand()
        {
            ClearHand();

            if (_currentCharacter == null || _currentCharacter.DeckManager == null)
            {
                return;
            }

            var hand = _currentCharacter.DeckManager.Hand;
            Debug.Log($"[CardHandUI] Refreshing hand for {_currentCharacter.DisplayName}. Cards: {hand.Count}");

            foreach (var card in hand)
            {
                CreateCardUI(card);
            }

            UpdateLayout();
        }

        /// <summary>
        /// 카드 UI 생성
        /// </summary>
        private void CreateCardUI(CardInstance card)
        {
            if (card == null || _cardUIMap.ContainsKey(card.InstanceId))
            {
                return;
            }

            GameObject cardGo;
            CardUI cardUI;

            if (_cardUIPrefab != null)
            {
                cardGo = Instantiate(_cardUIPrefab, _cardContainer);
                cardUI = cardGo.GetComponent<CardUI>();
            }
            else
            {
                // 프리팹 없으면 동적 생성
                cardGo = new GameObject($"Card_{card.CardName}");
                cardGo.transform.SetParent(_cardContainer, false);
                cardUI = cardGo.AddComponent<CardUI>();
                cardUI.CreateSimpleUI();
            }

            cardUI.Initialize(card, _currentCharacter);
            _cardUIMap[card.InstanceId] = cardUI;

            Debug.Log($"[CardHandUI] Created card UI: {card.CardName}");
        }

        /// <summary>
        /// 카드 UI 제거
        /// </summary>
        private void RemoveCard(string cardId)
        {
            if (_cardUIMap.TryGetValue(cardId, out var cardUI))
            {
                _cardUIMap.Remove(cardId);
                Destroy(cardUI.gameObject);
            }
        }

        /// <summary>
        /// 핸드 비우기
        /// </summary>
        private void ClearHand()
        {
            foreach (var kvp in _cardUIMap)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            _cardUIMap.Clear();
        }

        /// <summary>
        /// 모든 카드 플레이 가능 상태 업데이트
        /// </summary>
        private void UpdateAllCardsPlayableState()
        {
            foreach (var kvp in _cardUIMap)
            {
                kvp.Value?.UpdatePlayableState();
            }
        }

        #endregion

        #region Layout

        /// <summary>
        /// 카드 레이아웃 업데이트
        /// </summary>
        private void UpdateLayout()
        {
            var cards = new List<CardUI>(_cardUIMap.Values);
            int cardCount = cards.Count;

            if (cardCount == 0) return;

            if (_useFanLayout)
            {
                ApplyFanLayout(cards);
            }
            else
            {
                ApplyLinearLayout(cards);
            }
        }

        /// <summary>
        /// 부채꼴 레이아웃 적용 (±10° 범위, 포물선 Y 오프셋)
        /// Fan layout with ±10° angle range and parabolic Y offset
        /// </summary>
        private void ApplyFanLayout(List<CardUI> cards)
        {
            int count = cards.Count;
            if (count == 0) return;

            // 카드가 1장일 때는 중앙에 0도로 배치
            if (count == 1)
            {
                var rect = cards[0].GetComponent<RectTransform>();
                rect.localRotation = Quaternion.identity;
                rect.localPosition = Vector3.zero;
                return;
            }

            // 각도 계산: -maxAngle ~ +maxAngle 범위로 균등 분배
            float angleStep = (_maxFanAngle * 2f) / (count - 1);

            for (int i = 0; i < count; i++)
            {
                var cardUI = cards[i];
                var rect = cardUI.GetComponent<RectTransform>();

                // 각도: 왼쪽 카드 = 음수 각도, 오른쪽 카드 = 양수 각도
                float angle = -_maxFanAngle + (angleStep * i);
                rect.localRotation = Quaternion.Euler(0, 0, angle);

                // X 위치: 좌우로 균등 분배
                float normalizedX = (float)i / (count - 1) - 0.5f;  // -0.5 ~ 0.5
                float xOffset = normalizedX * (count - 1) * (_cardWidth * 0.6f);

                // Y 위치: 포물선 곡선 (중앙 카드가 가장 높음)
                // 포물선: y = a * (1 - x²) 형태
                float yOffset = (1f - (normalizedX * normalizedX * 4f)) * _fanYOffset;

                rect.localPosition = new Vector3(xOffset, yOffset, 0);

                // Z 순서: 중앙 카드가 위에 오도록 (가장 최근에 그려짐)
                // sibling index로 순서 조정 - 중앙 카드가 가장 마지막
                int siblingOrder = Mathf.Abs(i - count / 2);
                rect.SetSiblingIndex(count - 1 - siblingOrder);
            }
        }

        private void ApplyLinearLayout(List<CardUI> cards)
        {
            int count = cards.Count;

            for (int i = 0; i < count; i++)
            {
                var cardUI = cards[i];
                var rect = cardUI.GetComponent<RectTransform>();

                float xOffset = (i - (count - 1) / 2f) * (100f + _cardSpacing);
                rect.localPosition = new Vector3(xOffset, 0, 0);
                rect.localRotation = Quaternion.identity;
            }
        }

        #endregion

        #region Utility

        private PartyMemberCombat GetPartyMember(string entityId)
        {
            if (CombatManager.Instance == null || string.IsNullOrEmpty(entityId))
                return null;

            foreach (var member in CombatManager.Instance.PlayerParty)
            {
                if (member != null && member.EntityId == entityId)
                {
                    return member;
                }
            }
            return null;
        }

        #endregion
    }
}
