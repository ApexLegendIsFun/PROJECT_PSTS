// Combat/Entities/PartyMemberCombat.cs
// 파티원 (플레이어 캐릭터) 전투 엔티티

using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Events;
using ProjectSS.Data.Cards;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 파티원 전투 엔티티
    /// 개별 덱과 에너지를 가짐
    /// </summary>
    public class PartyMemberCombat : CombatEntity
    {
        [Header("Character Info")]
        [SerializeField] private CharacterClass _characterClass;

        [Header("Energy")]
        [SerializeField] private int _maxEnergy = 3;
        [SerializeField] private int _currentEnergy = 3;

        [Header("Deck Settings")]
        [SerializeField] private int _drawPerTurn = 5;

        // 덱 매니저 참조
        private DeckManager _deckManager;

        // 카드 효과 해결자
        private CardEffectResolver _effectResolver;

        // 프로퍼티
        public override bool IsPlayerCharacter => true;
        public CharacterClass CharacterClass => _characterClass;
        public int CurrentEnergy => _currentEnergy;
        public int MaxEnergy => _maxEnergy;
        public int DrawPerTurn => _drawPerTurn;
        public DeckManager DeckManager => _deckManager;

        protected override void Awake()
        {
            base.Awake();

            // 덱 매니저 초기화
            _deckManager = GetComponent<DeckManager>();
            if (_deckManager == null)
            {
                _deckManager = gameObject.AddComponent<DeckManager>();
            }

            // 효과 해결자 초기화 (CombatManager는 나중에 설정)
            _effectResolver = new CardEffectResolver(null);
        }

        private void Start()
        {
            // CombatManager 연결 (Awake 이후 참조 가능)
            if (CombatManager.Instance != null)
            {
                _effectResolver = new CardEffectResolver(CombatManager.Instance);
            }
        }

        #region Energy Management

        /// <summary>
        /// 에너지 사용
        /// </summary>
        public bool SpendEnergy(int amount)
        {
            if (_currentEnergy < amount)
            {
                Debug.Log($"[{DisplayName}] Not enough energy! ({_currentEnergy}/{amount})");
                return false;
            }

            _currentEnergy -= amount;

            Debug.Log($"[{DisplayName}] Spent {amount} energy. Remaining: {_currentEnergy}/{_maxEnergy}");

            EventBus.Publish(new EnergyChangedEvent
            {
                CharacterId = EntityId,
                CurrentEnergy = _currentEnergy,
                MaxEnergy = _maxEnergy
            });

            return true;
        }

        /// <summary>
        /// 에너지 회복
        /// </summary>
        public void GainEnergy(int amount)
        {
            _currentEnergy = Mathf.Min(_maxEnergy, _currentEnergy + amount);

            Debug.Log($"[{DisplayName}] Gained {amount} energy. Current: {_currentEnergy}/{_maxEnergy}");

            EventBus.Publish(new EnergyChangedEvent
            {
                CharacterId = EntityId,
                CurrentEnergy = _currentEnergy,
                MaxEnergy = _maxEnergy
            });
        }

        /// <summary>
        /// 에너지 리셋 (턴 시작 시)
        /// </summary>
        public void ResetEnergy()
        {
            _currentEnergy = _maxEnergy;

            EventBus.Publish(new EnergyChangedEvent
            {
                CharacterId = EntityId,
                CurrentEnergy = _currentEnergy,
                MaxEnergy = _maxEnergy
            });
        }

        #endregion

        #region Turn Events

        public override void OnTurnStart()
        {
            base.OnTurnStart();

            // 에너지 리셋
            ResetEnergy();

            // 카드 드로우
            if (_deckManager != null)
            {
                _deckManager.DrawCards(_drawPerTurn);
            }

            EventBus.Publish(new TurnStartedEvent
            {
                EntityId = EntityId,
                IsPlayerCharacter = true
            });
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();

            // 핸드의 카드를 버리기 더미로
            if (_deckManager != null)
            {
                _deckManager.DiscardHand();
            }

            EventBus.Publish(new TurnEndedEvent
            {
                EntityId = EntityId,
                IsPlayerCharacter = true
            });
        }

        #endregion

        #region Card Playing

        /// <summary>
        /// 카드 사용 가능 여부 확인
        /// </summary>
        public bool CanPlayCard(CardInstance card)
        {
            if (card == null) return false;
            return _currentEnergy >= card.EnergyCost;
        }

        /// <summary>
        /// 카드 사용
        /// </summary>
        public bool PlayCard(CardInstance card, ICombatEntity target)
        {
            if (!CanPlayCard(card))
            {
                Debug.Log($"[{DisplayName}] Cannot play card: {card.CardName}");
                return false;
            }

            // 에너지 소모
            SpendEnergy(card.EnergyCost);

            // 카드 효과 실행
            if (_effectResolver != null && card.IsValid())
            {
                _effectResolver.ResolveCard(card, this, target);
            }
            else
            {
                Debug.LogWarning($"[{DisplayName}] Cannot resolve card effects: {card.CardName}");
            }

            // 핸드에서 제거
            if (_deckManager != null)
            {
                _deckManager.PlayCardFromHand(card);
            }

            Debug.Log($"[{DisplayName}] Played: {card.CardName}");

            EventBus.Publish(new CardPlayedEvent
            {
                CharacterId = EntityId,
                CardId = card.CardId,
                TargetId = target?.EntityId,
                EnergyCost = card.EnergyCost
            });

            return true;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 캐릭터 데이터로 초기화
        /// </summary>
        public void Initialize(string characterId, string name, CharacterClass charClass, int maxHP, int maxEnergy, int speed)
        {
            _entityId = characterId;
            _displayName = name;
            _characterClass = charClass;
            _maxHP = maxHP;
            _currentHP = maxHP;
            _maxEnergy = maxEnergy;
            _currentEnergy = maxEnergy;
            _speed = speed;
        }

        #endregion
    }
}
