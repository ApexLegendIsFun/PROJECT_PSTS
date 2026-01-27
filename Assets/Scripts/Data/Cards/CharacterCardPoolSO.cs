// Data/Cards/CharacterCardPoolSO.cs
// 캐릭터별 카드풀 ScriptableObject

using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Data.Cards
{
    /// <summary>
    /// 캐릭터별 카드풀 정의
    /// 시작 덱과 보상으로 획득 가능한 카드 목록 관리
    /// </summary>
    [CreateAssetMenu(fileName = "NewCardPool", menuName = "PSTS/Character Card Pool")]
    public class CharacterCardPoolSO : ScriptableObject
    {
        [Header("Character Info")]
        [Tooltip("캐릭터 ID")]
        [SerializeField] private string _characterId;

        [Tooltip("캐릭터 표시 이름")]
        [SerializeField] private string _characterName;

        [Header("Starter Deck")]
        [Tooltip("시작 덱 카드 목록")]
        [SerializeField] private List<StarterCardEntry> _starterDeck = new List<StarterCardEntry>();

        [Header("Card Pools")]
        [Tooltip("일반 카드풀")]
        [SerializeField] private List<CardDataSO> _commonPool = new List<CardDataSO>();

        [Tooltip("고급 카드풀")]
        [SerializeField] private List<CardDataSO> _uncommonPool = new List<CardDataSO>();

        [Tooltip("희귀 카드풀")]
        [SerializeField] private List<CardDataSO> _rarePool = new List<CardDataSO>();

        #region Properties

        /// <summary>
        /// 캐릭터 ID
        /// </summary>
        public string CharacterId => _characterId;

        /// <summary>
        /// 캐릭터 표시 이름
        /// </summary>
        public string CharacterName => _characterName;

        /// <summary>
        /// 시작 덱 항목
        /// </summary>
        public IReadOnlyList<StarterCardEntry> StarterDeck => _starterDeck;

        /// <summary>
        /// 일반 카드풀
        /// </summary>
        public IReadOnlyList<CardDataSO> CommonPool => _commonPool;

        /// <summary>
        /// 고급 카드풀
        /// </summary>
        public IReadOnlyList<CardDataSO> UncommonPool => _uncommonPool;

        /// <summary>
        /// 희귀 카드풀
        /// </summary>
        public IReadOnlyList<CardDataSO> RarePool => _rarePool;

        #endregion

        #region Methods

        /// <summary>
        /// 시작 덱 카드 목록 생성
        /// </summary>
        public List<CardDataSO> GetStarterDeckCards()
        {
            var cards = new List<CardDataSO>();

            foreach (var entry in _starterDeck)
            {
                if (entry.Card != null)
                {
                    for (int i = 0; i < entry.Count; i++)
                    {
                        cards.Add(entry.Card);
                    }
                }
            }

            return cards;
        }

        /// <summary>
        /// 희귀도별 랜덤 카드 선택
        /// </summary>
        public CardDataSO GetRandomCard(Core.CardRarity rarity)
        {
            var pool = GetPoolByRarity(rarity);

            if (pool == null || pool.Count == 0)
            {
                return null;
            }

            return pool[Random.Range(0, pool.Count)];
        }

        /// <summary>
        /// 희귀도별 카드풀 반환
        /// </summary>
        public IReadOnlyList<CardDataSO> GetPoolByRarity(Core.CardRarity rarity)
        {
            return rarity switch
            {
                Core.CardRarity.Common => _commonPool,
                Core.CardRarity.Uncommon => _uncommonPool,
                Core.CardRarity.Rare => _rarePool,
                _ => _commonPool
            };
        }

        /// <summary>
        /// 모든 카드풀 합산 카드 수
        /// </summary>
        public int TotalPoolCount => _commonPool.Count + _uncommonPool.Count + _rarePool.Count;

        #endregion

        #region Editor Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            // ID가 비어있으면 에셋 이름으로 설정
            if (string.IsNullOrEmpty(_characterId))
            {
                _characterId = name.ToLower().Replace(" ", "_").Replace("cardpool", "");
            }
        }
#endif

        #endregion
    }

    /// <summary>
    /// 시작 덱 카드 항목
    /// </summary>
    [System.Serializable]
    public class StarterCardEntry
    {
        [Tooltip("카드 데이터")]
        public CardDataSO Card;

        [Tooltip("장수")]
        [Range(1, 10)]
        public int Count = 1;
    }
}
