using System.Collections.Generic;
using System.Linq;

namespace ProjectSS.Data
{
    /// <summary>
    /// 3인 파티 상태 관리
    /// Party state management for 3 characters
    /// </summary>
    [System.Serializable]
    public class PartyState
    {
        /// <summary>
        /// 워리어 상태
        /// Warrior state
        /// </summary>
        public CharacterState warrior;

        /// <summary>
        /// 메이지 상태
        /// Mage state
        /// </summary>
        public CharacterState mage;

        /// <summary>
        /// 로그 상태
        /// Rogue state
        /// </summary>
        public CharacterState rogue;

        /// <summary>
        /// 공유 덱 (3캐릭터 스타터 통합)
        /// Shared deck (combined from all 3 character starters)
        /// </summary>
        public List<string> deckCardIds = new List<string>();

        /// <summary>
        /// 덱 업그레이드 상태
        /// Deck upgrade status
        /// </summary>
        public List<bool> deckUpgraded = new List<bool>();

        /// <summary>
        /// 기본 생성자
        /// Default constructor
        /// </summary>
        public PartyState()
        {
            warrior = new CharacterState { characterClass = CharacterClass.Warrior, position = CharacterPosition.Active };
            mage = new CharacterState { characterClass = CharacterClass.Mage, position = CharacterPosition.Standby };
            rogue = new CharacterState { characterClass = CharacterClass.Rogue, position = CharacterPosition.Standby };
        }

        /// <summary>
        /// 클래스 데이터로 초기화
        /// Initialize from class data
        /// </summary>
        public void Initialize(CharacterClassData warriorData, CharacterClassData mageData, CharacterClassData rogueData)
        {
            warrior = new CharacterState(warriorData, CharacterPosition.Active);
            mage = new CharacterState(mageData, CharacterPosition.Standby);
            rogue = new CharacterState(rogueData, CharacterPosition.Standby);

            // 스타터 덱 통합
            // Combine starter decks
            deckCardIds.Clear();
            deckUpgraded.Clear();

            AddStarterDeck(warriorData.starterDeck);
            AddStarterDeck(mageData.starterDeck);
            AddStarterDeck(rogueData.starterDeck);
        }

        private void AddStarterDeck(List<CardData> cards)
        {
            if (cards == null) return;
            foreach (var card in cards)
            {
                if (card != null)
                {
                    deckCardIds.Add(card.cardId);
                    deckUpgraded.Add(false);
                }
            }
        }

        /// <summary>
        /// 전열(Active) 캐릭터 반환
        /// Get Active character
        /// </summary>
        public CharacterState GetActive()
        {
            if (warrior.IsActive) return warrior;
            if (mage.IsActive) return mage;
            if (rogue.IsActive) return rogue;
            return warrior; // 기본값
        }

        /// <summary>
        /// 후열(Standby) 캐릭터 목록 반환
        /// Get Standby characters
        /// </summary>
        public List<CharacterState> GetStandby()
        {
            var result = new List<CharacterState>();
            if (warrior.IsStandby) result.Add(warrior);
            if (mage.IsStandby) result.Add(mage);
            if (rogue.IsStandby) result.Add(rogue);
            return result;
        }

        /// <summary>
        /// 모든 캐릭터 반환
        /// Get all characters
        /// </summary>
        public List<CharacterState> GetAll()
        {
            return new List<CharacterState> { warrior, mage, rogue };
        }

        /// <summary>
        /// 생존 캐릭터 반환
        /// Get alive characters
        /// </summary>
        public List<CharacterState> GetAlive()
        {
            return GetAll().Where(c => c.IsAlive).ToList();
        }

        /// <summary>
        /// 행동 가능 캐릭터 반환
        /// Get characters that can act
        /// </summary>
        public List<CharacterState> GetAvailable()
        {
            return GetAll().Where(c => c.CanAct).ToList();
        }

        /// <summary>
        /// 클래스로 캐릭터 반환
        /// Get character by class
        /// </summary>
        public CharacterState GetByClass(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Warrior => warrior,
                CharacterClass.Mage => mage,
                CharacterClass.Rogue => rogue,
                _ => null
            };
        }

        /// <summary>
        /// 위치 교체 (태그인)
        /// Swap positions (tag-in)
        /// </summary>
        public void SwapPosition(CharacterClass toActive)
        {
            // 현재 Active를 Standby로
            var currentActive = GetActive();
            currentActive.position = CharacterPosition.Standby;

            // 지정된 캐릭터를 Active로
            var newActive = GetByClass(toActive);
            if (newActive != null)
            {
                newActive.position = CharacterPosition.Active;
            }
        }

        /// <summary>
        /// 파티 전멸 여부 확인
        /// Check if party is wiped
        /// </summary>
        public bool IsPartyWiped()
        {
            return !warrior.CanAct && !mage.CanAct && !rogue.CanAct;
        }

        /// <summary>
        /// 행동 가능 캐릭터 존재 여부
        /// Check if any character can act
        /// </summary>
        public bool AnyCharacterCanAct()
        {
            return warrior.CanAct || mage.CanAct || rogue.CanAct;
        }

        /// <summary>
        /// 전투 종료 후 전체 회복
        /// Full recovery after combat (revive all)
        /// </summary>
        public void FullRecovery()
        {
            warrior.FullRecovery();
            mage.FullRecovery();
            rogue.FullRecovery();
        }

        /// <summary>
        /// 전투 상태 리셋 (Focus 초기화)
        /// Reset combat state
        /// </summary>
        public void ResetCombatState()
        {
            warrior.ResetCombatState();
            mage.ResetCombatState();
            rogue.ResetCombatState();
        }

        /// <summary>
        /// 카드 추가
        /// Add card to deck
        /// </summary>
        public void AddCard(CardData card, bool upgraded = false)
        {
            deckCardIds.Add(card.cardId);
            deckUpgraded.Add(upgraded);
        }

        /// <summary>
        /// 카드 제거
        /// Remove card from deck
        /// </summary>
        public bool RemoveCard(int index)
        {
            if (index >= 0 && index < deckCardIds.Count)
            {
                deckCardIds.RemoveAt(index);
                deckUpgraded.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 복제본 생성
        /// Create copy
        /// </summary>
        public PartyState Clone()
        {
            var copy = new PartyState
            {
                warrior = warrior.Clone(),
                mage = mage.Clone(),
                rogue = rogue.Clone()
            };

            copy.deckCardIds.AddRange(deckCardIds);
            copy.deckUpgraded.AddRange(deckUpgraded);

            return copy;
        }
    }
}
