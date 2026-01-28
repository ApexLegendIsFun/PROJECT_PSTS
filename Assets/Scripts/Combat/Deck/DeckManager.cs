// Combat/Deck/DeckManager.cs
// 개별 덱 관리자 - 캐릭터별 덱/핸드/버리기 관리

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core.Events;
using ProjectSS.Services;
using ProjectSS.Data.Cards;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 개별 덱 관리자
    /// 각 캐릭터가 자신만의 덱/핸드/버리기 더미를 가짐
    /// </summary>
    public class DeckManager : MonoBehaviour
    {
        [Header("Owner")]
        [SerializeField] private string _ownerId;

        [Header("Deck State")]
        [SerializeField] private List<CardInstance> _drawPile = new();
        [SerializeField] private List<CardInstance> _hand = new();
        [SerializeField] private List<CardInstance> _discardPile = new();
        [SerializeField] private List<CardInstance> _exhaustPile = new();

        [Header("Settings")]
        [SerializeField] private int _maxHandSize = 10;

        // 프로퍼티
        public string OwnerId => _ownerId;
        public IReadOnlyList<CardInstance> DrawPile => _drawPile;
        public IReadOnlyList<CardInstance> Hand => _hand;
        public IReadOnlyList<CardInstance> DiscardPile => _discardPile;
        public IReadOnlyList<CardInstance> ExhaustPile => _exhaustPile;
        public int DrawPileCount => _drawPile.Count;
        public int HandCount => _hand.Count;
        public int DiscardPileCount => _discardPile.Count;

        private void Awake()
        {
            InitializeFromConfig();
        }

        /// <summary>
        /// DataService에서 기본값 로드
        /// </summary>
        private void InitializeFromConfig()
        {
            var balance = DataService.Instance?.Balance;
            if (balance == null) return;

            // SerializeField 기본값(10)이면 Config에서 로드
            if (_maxHandSize == 10)
            {
                _maxHandSize = balance.MaxHandSize;
            }
        }

        #region Initialization

        /// <summary>
        /// 덱 초기화 (런 시작 시) - CardInstance 리스트
        /// </summary>
        public void InitializeDeck(string ownerId, List<CardInstance> starterCards)
        {
            _ownerId = ownerId;

            _drawPile.Clear();
            _hand.Clear();
            _discardPile.Clear();
            _exhaustPile.Clear();

            // 시작 덱 설정
            foreach (var card in starterCards)
            {
                _drawPile.Add(card);
            }

            // 셔플
            ShuffleDrawPile();

            Debug.Log($"[DeckManager] {_ownerId} - Deck initialized with {_drawPile.Count} cards");
        }

        /// <summary>
        /// 덱 초기화 (런 시작 시) - CardDataSO 리스트에서 인스턴스 생성
        /// </summary>
        public void InitializeDeck(string ownerId, List<CardDataSO> starterCardData)
        {
            _ownerId = ownerId;

            _drawPile.Clear();
            _hand.Clear();
            _discardPile.Clear();
            _exhaustPile.Clear();

            // 시작 덱 설정 (SO에서 인스턴스 생성)
            foreach (var cardData in starterCardData)
            {
                if (cardData != null)
                {
                    _drawPile.Add(new CardInstance(cardData));
                }
            }

            // 셔플
            ShuffleDrawPile();

            Debug.Log($"[DeckManager] {_ownerId} - Deck initialized with {_drawPile.Count} cards from SO");
        }

        /// <summary>
        /// 덱 초기화 - CharacterCardPoolSO에서 시작 덱 로드
        /// </summary>
        public void InitializeDeck(string ownerId, CharacterCardPoolSO cardPool)
        {
            if (cardPool == null)
            {
                Debug.LogError($"[DeckManager] Card pool is null for {ownerId}");
                return;
            }

            var starterCards = cardPool.GetStarterDeckCards();
            InitializeDeck(ownerId, starterCards);
        }

        /// <summary>
        /// 전투 시작 시 리셋
        /// </summary>
        public void ResetForCombat()
        {
            // 모든 카드를 드로우 더미로
            _drawPile.AddRange(_hand);
            _drawPile.AddRange(_discardPile);

            _hand.Clear();
            _discardPile.Clear();
            // 소멸 카드는 제외

            ShuffleDrawPile();

            Debug.Log($"[DeckManager] {_ownerId} - Reset for combat. Draw pile: {_drawPile.Count}");
        }

        #endregion

        #region Card Drawing

        /// <summary>
        /// 카드 드로우
        /// </summary>
        public List<CardInstance> DrawCards(int count)
        {
            var drawnCards = new List<CardInstance>();

            for (int i = 0; i < count; i++)
            {
                var card = DrawSingleCard();
                if (card != null)
                {
                    drawnCards.Add(card);
                }
                else
                {
                    break; // 더 이상 드로우할 카드가 없음
                }
            }

            Debug.Log($"[DeckManager] {_ownerId} - Drew {drawnCards.Count} cards. Hand: {_hand.Count}");
            return drawnCards;
        }

        /// <summary>
        /// 단일 카드 드로우
        /// </summary>
        private CardInstance DrawSingleCard()
        {
            // 핸드가 가득 차면 드로우 불가
            if (_hand.Count >= _maxHandSize)
            {
                Debug.Log($"[DeckManager] {_ownerId} - Hand is full!");
                return null;
            }

            // 드로우 더미가 비었으면 버리기 더미 셔플
            if (_drawPile.Count == 0)
            {
                if (_discardPile.Count == 0)
                {
                    Debug.Log($"[DeckManager] {_ownerId} - No cards to draw!");
                    return null;
                }

                ReshuffleDiscardIntoDraw();
            }

            // 드로우
            var card = _drawPile[0];
            _drawPile.RemoveAt(0);
            _hand.Add(card);

            EventBus.Publish(new CardDrawnEvent
            {
                CharacterId = _ownerId,
                CardId = card.CardId,
                CardsInHand = _hand.Count
            });

            return card;
        }

        /// <summary>
        /// 버리기 더미를 드로우 더미로 셔플
        /// </summary>
        private void ReshuffleDiscardIntoDraw()
        {
            Debug.Log($"[DeckManager] {_ownerId} - Reshuffling discard pile ({_discardPile.Count} cards)");

            _drawPile.AddRange(_discardPile);
            _discardPile.Clear();
            ShuffleDrawPile();
        }

        /// <summary>
        /// 드로우 더미 셔플
        /// </summary>
        public void ShuffleDrawPile()
        {
            for (int i = _drawPile.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_drawPile[i], _drawPile[j]) = (_drawPile[j], _drawPile[i]);
            }
        }

        #endregion

        #region Card Playing

        /// <summary>
        /// 핸드에서 카드 사용 (버리기 더미로 이동)
        /// </summary>
        public bool PlayCardFromHand(CardInstance card)
        {
            if (!_hand.Contains(card))
            {
                Debug.LogWarning($"[DeckManager] {_ownerId} - Card not in hand: {card.CardName}");
                return false;
            }

            _hand.Remove(card);

            // 소멸 카드인 경우
            if (card.IsExhausted)
            {
                _exhaustPile.Add(card);
            }
            else
            {
                _discardPile.Add(card);
            }

            return true;
        }

        /// <summary>
        /// 핸드의 모든 카드 버리기
        /// </summary>
        public void DiscardHand()
        {
            Debug.Log($"[DeckManager] {_ownerId} - Discarding hand ({_hand.Count} cards)");

            _discardPile.AddRange(_hand);
            _hand.Clear();
        }

        /// <summary>
        /// 특정 카드 버리기
        /// </summary>
        public void DiscardCard(CardInstance card)
        {
            if (_hand.Remove(card))
            {
                _discardPile.Add(card);
                Debug.Log($"[DeckManager] {_ownerId} - Discarded: {card.CardName}");
            }
        }

        /// <summary>
        /// 카드 소멸
        /// </summary>
        public void ExhaustCard(CardInstance card)
        {
            card.Exhaust();

            if (_hand.Remove(card) || _discardPile.Remove(card) || _drawPile.Remove(card))
            {
                _exhaustPile.Add(card);
                Debug.Log($"[DeckManager] {_ownerId} - Exhausted: {card.CardName}");
            }
        }

        #endregion

        #region Deck Modification

        /// <summary>
        /// 덱에 카드 추가 (런 중 획득) - CardInstance
        /// </summary>
        public void AddCardToDeck(CardInstance card)
        {
            _discardPile.Add(card);
            Debug.Log($"[DeckManager] {_ownerId} - Added to deck: {card.CardName}");
        }

        /// <summary>
        /// 덱에 카드 추가 (런 중 획득) - CardDataSO에서 인스턴스 생성
        /// </summary>
        public void AddCardToDeck(CardDataSO cardData, bool upgraded = false)
        {
            if (cardData == null)
            {
                Debug.LogWarning($"[DeckManager] {_ownerId} - Cannot add null card data");
                return;
            }

            var card = new CardInstance(cardData, upgraded);
            _discardPile.Add(card);
            Debug.Log($"[DeckManager] {_ownerId} - Added to deck from SO: {card.CardName}");
        }

        /// <summary>
        /// 덱에서 카드 제거 (영구 삭제)
        /// </summary>
        public bool RemoveCardFromDeck(CardInstance card)
        {
            if (_drawPile.Remove(card) || _discardPile.Remove(card) || _hand.Remove(card))
            {
                Debug.Log($"[DeckManager] {_ownerId} - Removed from deck: {card.CardName}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 전체 덱 목록 (런 중 확인용)
        /// </summary>
        public List<CardInstance> GetFullDeck()
        {
            var fullDeck = new List<CardInstance>();
            fullDeck.AddRange(_drawPile);
            fullDeck.AddRange(_hand);
            fullDeck.AddRange(_discardPile);
            return fullDeck;
        }

        #endregion

        #region Query

        /// <summary>
        /// 핸드에서 카드 찾기
        /// </summary>
        public CardInstance GetCardFromHand(string cardId)
        {
            return _hand.Find(c => c.CardId == cardId);
        }

        /// <summary>
        /// 핸드에서 인덱스로 카드 가져오기
        /// </summary>
        public CardInstance GetCardFromHand(int index)
        {
            if (index >= 0 && index < _hand.Count)
            {
                return _hand[index];
            }
            return null;
        }

        #endregion
    }
}
