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
        [SerializeField] private float _cardSpacing = -30f; // 겹치는 효과
        [SerializeField] private float _fanAngle = 5f;      // 부채꼴 각도
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
            if (card == null || _cardUIMap.ContainsKey(card.CardId))
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
            _cardUIMap[card.CardId] = cardUI;

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

        private void ApplyFanLayout(List<CardUI> cards)
        {
            int count = cards.Count;
            float totalAngle = _fanAngle * (count - 1);
            float startAngle = totalAngle / 2;

            for (int i = 0; i < count; i++)
            {
                var cardUI = cards[i];
                var rect = cardUI.GetComponent<RectTransform>();

                float angle = startAngle - (_fanAngle * i);
                rect.localRotation = Quaternion.Euler(0, 0, angle);

                // 위치 조정
                float xOffset = (i - (count - 1) / 2f) * 80f;
                rect.localPosition = new Vector3(xOffset, 0, 0);
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
            if (CombatManager.Instance == null) return null;

            foreach (var member in CombatManager.Instance.PlayerParty)
            {
                if (member.EntityId == entityId)
                {
                    return member;
                }
            }
            return null;
        }

        #endregion
    }
}
