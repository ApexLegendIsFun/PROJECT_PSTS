using System.Collections.Generic;
using ProjectSS.Data;
using ProjectSS.Core;
using ProjectSS.Utility;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 덱 관리자 (드로우 파일, 핸드, 버린 카드, 소멸 관리)
    /// Deck manager (draw pile, hand, discard pile, exhaust pile)
    /// </summary>
    public class DeckManager
    {
        private List<CardInstance> drawPile = new List<CardInstance>();
        private List<CardInstance> hand = new List<CardInstance>();
        private List<CardInstance> discardPile = new List<CardInstance>();
        private List<CardInstance> exhaustPile = new List<CardInstance>();
        private SeededRandom random;

        public IReadOnlyList<CardInstance> DrawPile => drawPile;
        public IReadOnlyList<CardInstance> Hand => hand;
        public IReadOnlyList<CardInstance> DiscardPile => discardPile;
        public IReadOnlyList<CardInstance> ExhaustPile => exhaustPile;

        public int HandSize => hand.Count;
        public int DrawPileSize => drawPile.Count;
        public int DiscardPileSize => discardPile.Count;

        public const int MAX_HAND_SIZE = 10;
        public const int DEFAULT_DRAW_COUNT = 5;

        public DeckManager(SeededRandom rng)
        {
            random = rng;
        }

        /// <summary>
        /// 덱 초기화 (전투 시작 시)
        /// Initialize deck (at combat start)
        /// </summary>
        public void InitializeDeck(List<CardData> deckCards)
        {
            drawPile.Clear();
            hand.Clear();
            discardPile.Clear();
            exhaustPile.Clear();

            foreach (var cardData in deckCards)
            {
                drawPile.Add(new CardInstance(cardData));
            }

            ShuffleDeck();
        }

        /// <summary>
        /// 드로우 파일 셔플
        /// Shuffle draw pile
        /// </summary>
        public void ShuffleDeck()
        {
            drawPile.Shuffle(random.GetSystemRandom());
        }

        /// <summary>
        /// 카드 드로우
        /// Draw cards
        /// </summary>
        public List<CardInstance> DrawCards(int count)
        {
            var drawnCards = new List<CardInstance>();

            for (int i = 0; i < count; i++)
            {
                if (hand.Count >= MAX_HAND_SIZE) break;

                // 드로우 파일이 비었으면 버린 카드 파일을 섞어서 가져옴
                if (drawPile.Count == 0)
                {
                    ShuffleDiscardIntoDraw();
                    if (drawPile.Count == 0) break;
                }

                var card = drawPile.PopLast();
                hand.Add(card);
                drawnCards.Add(card);
            }

            if (drawnCards.Count > 0)
            {
                EventBus.Publish(new CardDrawnEvent(drawnCards.Count));
            }

            return drawnCards;
        }

        /// <summary>
        /// 버린 카드를 드로우 파일로 섞음
        /// Shuffle discard pile into draw pile
        /// </summary>
        public void ShuffleDiscardIntoDraw()
        {
            drawPile.AddRange(discardPile);
            discardPile.Clear();
            ShuffleDeck();
        }

        /// <summary>
        /// 카드를 버린 카드 더미로
        /// Discard a card
        /// </summary>
        public void DiscardCard(CardInstance card)
        {
            if (hand.Remove(card))
            {
                discardPile.Add(card);
                EventBus.Publish(new CardDiscardedEvent(card.EffectiveData.cardId));
            }
        }

        /// <summary>
        /// 핸드 전체 버리기
        /// Discard entire hand
        /// </summary>
        public void DiscardHand()
        {
            foreach (var card in hand)
            {
                discardPile.Add(card);
            }
            hand.Clear();
        }

        /// <summary>
        /// 카드 소멸
        /// Exhaust a card
        /// </summary>
        public void ExhaustCard(CardInstance card)
        {
            if (hand.Remove(card))
            {
                exhaustPile.Add(card);
            }
        }

        /// <summary>
        /// 카드 사용 처리
        /// Process card after playing
        /// </summary>
        public void OnCardPlayed(CardInstance card)
        {
            hand.Remove(card);

            if (card.EffectiveData.exhaustOnUse)
            {
                exhaustPile.Add(card);
            }
            else
            {
                discardPile.Add(card);
            }
        }

        /// <summary>
        /// 특정 카드 덱에 추가
        /// Add card to draw pile
        /// </summary>
        public void AddCardToDrawPile(CardInstance card, bool shuffle = true)
        {
            drawPile.Add(card);
            if (shuffle)
            {
                ShuffleDeck();
            }
        }

        /// <summary>
        /// 특정 카드 핸드에 추가
        /// Add card to hand
        /// </summary>
        public void AddCardToHand(CardInstance card)
        {
            if (hand.Count < MAX_HAND_SIZE)
            {
                hand.Add(card);
            }
            else
            {
                discardPile.Add(card);
            }
        }
    }
}
