using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Data;
using ProjectSS.Core;

namespace ProjectSS.Run
{
    /// <summary>
    /// 플레이어 런타임 상태 관리
    /// Player runtime state management
    /// </summary>
    public class PlayerState
    {
        public int CurrentHP { get; private set; }
        public int MaxHP { get; private set; }
        public int Gold { get; private set; }

        private List<CardData> deck = new List<CardData>();
        private List<bool> deckUpgraded = new List<bool>();
        private List<RelicData> relics = new List<RelicData>();

        public IReadOnlyList<CardData> Deck => deck;
        public IReadOnlyList<bool> DeckUpgraded => deckUpgraded;
        public IReadOnlyList<RelicData> Relics => relics;

        public PlayerState(int maxHP = 80, int startGold = 99)
        {
            MaxHP = maxHP;
            CurrentHP = maxHP;
            Gold = startGold;
        }

        /// <summary>
        /// RunState에서 초기화
        /// Initialize from RunState
        /// </summary>
        public void LoadFromRunState(RunState state, CardDatabase cardDb, RelicDatabase relicDb)
        {
            CurrentHP = state.currentHP;
            MaxHP = state.maxHP;
            Gold = state.gold;

            deck.Clear();
            deckUpgraded.Clear();

            for (int i = 0; i < state.deckCardIds.Count; i++)
            {
                var card = cardDb?.GetCard(state.deckCardIds[i]);
                if (card != null)
                {
                    deck.Add(card);
                    deckUpgraded.Add(state.deckUpgraded[i]);
                }
            }

            relics.Clear();
            foreach (var relicId in state.relicIds)
            {
                var relic = relicDb?.GetRelic(relicId);
                if (relic != null)
                {
                    relics.Add(relic);
                }
            }
        }

        /// <summary>
        /// RunState로 저장
        /// Save to RunState
        /// </summary>
        public void SaveToRunState(RunState state)
        {
            state.currentHP = CurrentHP;
            state.maxHP = MaxHP;
            state.gold = Gold;

            state.deckCardIds.Clear();
            state.deckUpgraded.Clear();

            for (int i = 0; i < deck.Count; i++)
            {
                state.deckCardIds.Add(deck[i].cardId);
                state.deckUpgraded.Add(deckUpgraded[i]);
            }

            state.relicIds.Clear();
            foreach (var relic in relics)
            {
                state.relicIds.Add(relic.relicId);
            }
        }

        #region HP Management

        public void TakeDamage(int amount)
        {
            CurrentHP = Mathf.Max(0, CurrentHP - amount);
        }

        public void Heal(int amount)
        {
            CurrentHP = Mathf.Min(MaxHP, CurrentHP + amount);
        }

        public void SetMaxHP(int amount)
        {
            MaxHP = amount;
            CurrentHP = Mathf.Min(CurrentHP, MaxHP);
        }

        public void GainMaxHP(int amount)
        {
            MaxHP += amount;
            CurrentHP += amount; // 최대 체력 증가 시 현재 체력도 증가
        }

        public void LoseMaxHP(int amount)
        {
            MaxHP = Mathf.Max(1, MaxHP - amount);
            CurrentHP = Mathf.Min(CurrentHP, MaxHP);
        }

        public bool IsAlive => CurrentHP > 0;

        #endregion

        #region Gold Management

        public void GainGold(int amount)
        {
            Gold += amount;
            EventBus.Publish(new GoldChangedEvent(amount, Gold));
        }

        public bool SpendGold(int amount)
        {
            if (Gold < amount) return false;
            Gold -= amount;
            EventBus.Publish(new GoldChangedEvent(-amount, Gold));
            return true;
        }

        public bool CanAfford(int amount) => Gold >= amount;

        #endregion

        #region Deck Management

        public void AddCard(CardData card, bool upgraded = false)
        {
            deck.Add(card);
            deckUpgraded.Add(upgraded);
        }

        public bool RemoveCard(int index)
        {
            if (index >= 0 && index < deck.Count)
            {
                deck.RemoveAt(index);
                deckUpgraded.RemoveAt(index);
                return true;
            }
            return false;
        }

        public bool RemoveCard(CardData card)
        {
            int index = deck.IndexOf(card);
            return RemoveCard(index);
        }

        public void UpgradeCard(int index)
        {
            if (index >= 0 && index < deck.Count)
            {
                deckUpgraded[index] = true;
            }
        }

        /// <summary>
        /// 전투용 덱 데이터 반환
        /// Get deck data for combat
        /// </summary>
        public List<CardData> GetDeckForCombat()
        {
            var combatDeck = new List<CardData>();

            for (int i = 0; i < deck.Count; i++)
            {
                // 업그레이드된 카드는 업그레이드 버전 사용
                if (deckUpgraded[i] && deck[i].upgradedVersion != null)
                {
                    combatDeck.Add(deck[i].upgradedVersion);
                }
                else
                {
                    combatDeck.Add(deck[i]);
                }
            }

            return combatDeck;
        }

        #endregion

        #region Relic Management

        public void AddRelic(RelicData relic)
        {
            if (!HasRelic(relic))
            {
                relics.Add(relic);
            }
        }

        public bool HasRelic(RelicData relic)
        {
            return relics.Contains(relic);
        }

        public bool HasRelic(string relicId)
        {
            return relics.Exists(r => r.relicId == relicId);
        }

        #endregion
    }

    /// <summary>
    /// 카드 데이터베이스 인터페이스
    /// Card database interface
    /// </summary>
    public interface CardDatabase
    {
        CardData GetCard(string cardId);
    }

    /// <summary>
    /// 유물 데이터베이스 인터페이스
    /// Relic database interface
    /// </summary>
    public interface RelicDatabase
    {
        RelicData GetRelic(string relicId);
    }
}
