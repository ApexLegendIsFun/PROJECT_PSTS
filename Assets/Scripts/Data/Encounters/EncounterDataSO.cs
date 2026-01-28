// Data/Encounters/EncounterDataSO.cs
// 인카운터 데이터 ScriptableObject

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data.Enemies;

namespace ProjectSS.Data.Encounters
{
    /// <summary>
    /// 인카운터에 등장하는 적 정보
    /// </summary>
    [System.Serializable]
    public class EncounterEnemyEntry
    {
        [Tooltip("적 데이터")]
        public EnemyDataSO EnemyData;

        [Tooltip("고정 등장 수 (RandomCountMin/Max가 0일 때 사용)")]
        [Range(1, 5)]
        public int Count = 1;

        [Tooltip("최소 랜덤 등장 수 (0이면 고정 Count 사용)")]
        [Range(0, 5)]
        public int RandomCountMin = 0;

        [Tooltip("최대 랜덤 등장 수 (0이면 고정 Count 사용)")]
        [Range(0, 5)]
        public int RandomCountMax = 0;

        /// <summary>
        /// 실제 등장할 적 수 계산
        /// </summary>
        public int GetActualCount()
        {
            if (RandomCountMin > 0 && RandomCountMax > 0 && RandomCountMax >= RandomCountMin)
            {
                return Random.Range(RandomCountMin, RandomCountMax + 1);
            }
            return Count;
        }
    }

    /// <summary>
    /// 인카운터 데이터 정의
    /// 전투에서 등장할 적 구성을 설정
    /// </summary>
    [CreateAssetMenu(fileName = "NewEncounter", menuName = "PSTS/Encounter Data")]
    public class EncounterDataSO : ScriptableObject
    {
        #region Identity

        [Header("Identity")]
        [Tooltip("인카운터 고유 ID")]
        [SerializeField] private string _encounterId;

        [Tooltip("인카운터 이름")]
        [SerializeField] private string _encounterName;

        [Tooltip("인카운터 타입")]
        [SerializeField] private TileType _encounterType = TileType.Enemy;

        #endregion

        #region Enemy Composition

        [Header("Enemy Composition")]
        [Tooltip("등장할 적 목록")]
        [SerializeField] private List<EncounterEnemyEntry> _enemies = new List<EncounterEnemyEntry>();

        #endregion

        #region Requirements

        [Header("Requirements")]
        [Tooltip("최소 난이도 (이 난이도 이상에서만 등장)")]
        [Range(1, 20)]
        [SerializeField] private int _minDifficulty = 1;

        [Tooltip("최대 난이도 (이 난이도 이하에서만 등장)")]
        [Range(1, 20)]
        [SerializeField] private int _maxDifficulty = 10;

        [Tooltip("Act 요구사항 (이 Act에서만 등장, 0이면 모든 Act)")]
        [Range(0, 5)]
        [SerializeField] private int _actRequirement = 0;

        #endregion

        #region Rewards

        [Header("Rewards")]
        [Tooltip("기본 골드 보상")]
        [SerializeField] private int _baseGoldReward = 10;

        [Tooltip("카드 보상 희귀도")]
        [SerializeField] private CardRarity _cardRewardRarity = CardRarity.Common;

        #endregion

        #region Properties

        /// <summary>
        /// 인카운터 고유 ID
        /// </summary>
        public string EncounterId => _encounterId;

        /// <summary>
        /// 인카운터 이름
        /// </summary>
        public string EncounterName => _encounterName;

        /// <summary>
        /// 인카운터 타입
        /// </summary>
        public TileType EncounterType => _encounterType;

        /// <summary>
        /// 등장할 적 목록
        /// </summary>
        public IReadOnlyList<EncounterEnemyEntry> Enemies => _enemies;

        /// <summary>
        /// 최소 난이도
        /// </summary>
        public int MinDifficulty => _minDifficulty;

        /// <summary>
        /// 최대 난이도
        /// </summary>
        public int MaxDifficulty => _maxDifficulty;

        /// <summary>
        /// Act 요구사항
        /// </summary>
        public int ActRequirement => _actRequirement;

        /// <summary>
        /// 기본 골드 보상
        /// </summary>
        public int BaseGoldReward => _baseGoldReward;

        /// <summary>
        /// 카드 보상 희귀도
        /// </summary>
        public CardRarity CardRewardRarity => _cardRewardRarity;

        #endregion

        #region Methods

        /// <summary>
        /// 해당 난이도에서 유효한 인카운터인지 확인
        /// </summary>
        public bool IsValidForDifficulty(int difficulty)
        {
            return difficulty >= _minDifficulty && difficulty <= _maxDifficulty;
        }

        /// <summary>
        /// 해당 Act에서 유효한 인카운터인지 확인
        /// </summary>
        public bool IsValidForAct(int act)
        {
            return _actRequirement == 0 || _actRequirement == act;
        }

        /// <summary>
        /// 난이도와 Act 모두에서 유효한지 확인
        /// </summary>
        public bool IsValidFor(int difficulty, int act)
        {
            return IsValidForDifficulty(difficulty) && IsValidForAct(act);
        }

        /// <summary>
        /// 난이도에 따른 골드 보상 계산
        /// </summary>
        public int GetGoldReward(int difficulty)
        {
            float scale = 1f + (0.1f * (difficulty - 1));
            return Mathf.RoundToInt(_baseGoldReward * scale);
        }

        /// <summary>
        /// 적 구성에서 총 적 수 계산
        /// </summary>
        public int GetTotalEnemyCount()
        {
            int total = 0;
            foreach (var entry in _enemies)
            {
                total += entry.GetActualCount();
            }
            return total;
        }

        #endregion

        #region Editor Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            // ID가 비어있으면 에셋 이름으로 설정
            if (string.IsNullOrEmpty(_encounterId))
            {
                _encounterId = name.ToLower().Replace(" ", "_");
            }

            // 이름이 비어있으면 에셋 이름으로 설정
            if (string.IsNullOrEmpty(_encounterName))
            {
                _encounterName = name;
            }

            // 최소/최대 난이도 검증
            if (_maxDifficulty < _minDifficulty)
            {
                _maxDifficulty = _minDifficulty;
            }
        }
#endif

        #endregion
    }
}
