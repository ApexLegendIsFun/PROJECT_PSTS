// Data/Encounters/EncounterPoolSO.cs
// 인카운터 풀 ScriptableObject

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Data.Encounters
{
    /// <summary>
    /// 인카운터 풀 정의
    /// 특정 타입/Act의 인카운터 목록을 관리
    /// </summary>
    [CreateAssetMenu(fileName = "NewEncounterPool", menuName = "PSTS/Encounter Pool")]
    public class EncounterPoolSO : ScriptableObject
    {
        #region Identity

        [Header("Pool Info")]
        [Tooltip("풀 고유 ID")]
        [SerializeField] private string _poolId;

        [Tooltip("풀 타입 (Enemy, Elite, Boss)")]
        [SerializeField] private TileType _poolType = TileType.Enemy;

        [Tooltip("Act 번호 (0이면 모든 Act)")]
        [Range(0, 5)]
        [SerializeField] private int _actNumber = 1;

        #endregion

        #region Encounters

        [Header("Encounters")]
        [Tooltip("이 풀에 포함된 인카운터 목록")]
        [SerializeField] private List<EncounterDataSO> _encounters = new List<EncounterDataSO>();

        #endregion

        #region Properties

        /// <summary>
        /// 풀 고유 ID
        /// </summary>
        public string PoolId => _poolId;

        /// <summary>
        /// 풀 타입
        /// </summary>
        public TileType PoolType => _poolType;

        /// <summary>
        /// Act 번호
        /// </summary>
        public int ActNumber => _actNumber;

        /// <summary>
        /// 인카운터 목록
        /// </summary>
        public IReadOnlyList<EncounterDataSO> Encounters => _encounters;

        /// <summary>
        /// 인카운터 수
        /// </summary>
        public int Count => _encounters.Count;

        #endregion

        #region Methods

        /// <summary>
        /// 난이도에 맞는 랜덤 인카운터 반환
        /// </summary>
        /// <param name="difficulty">현재 난이도</param>
        /// <returns>랜덤 선택된 인카운터, 없으면 null</returns>
        public EncounterDataSO GetRandomEncounter(int difficulty)
        {
            var validEncounters = GetEncountersForDifficulty(difficulty);

            if (validEncounters.Count == 0)
            {
                // 유효한 인카운터가 없으면 전체에서 랜덤 선택
                if (_encounters.Count > 0)
                {
                    return _encounters[Random.Range(0, _encounters.Count)];
                }
                return null;
            }

            return validEncounters[Random.Range(0, validEncounters.Count)];
        }

        /// <summary>
        /// 난이도에 맞는 인카운터 목록 반환
        /// </summary>
        /// <param name="difficulty">현재 난이도</param>
        /// <returns>유효한 인카운터 목록</returns>
        public List<EncounterDataSO> GetEncountersForDifficulty(int difficulty)
        {
            var result = new List<EncounterDataSO>();

            foreach (var encounter in _encounters)
            {
                if (encounter != null && encounter.IsValidForDifficulty(difficulty))
                {
                    result.Add(encounter);
                }
            }

            return result;
        }

        /// <summary>
        /// 난이도와 Act에 맞는 인카운터 목록 반환
        /// </summary>
        /// <param name="difficulty">현재 난이도</param>
        /// <param name="act">현재 Act</param>
        /// <returns>유효한 인카운터 목록</returns>
        public List<EncounterDataSO> GetEncountersFor(int difficulty, int act)
        {
            var result = new List<EncounterDataSO>();

            foreach (var encounter in _encounters)
            {
                if (encounter != null && encounter.IsValidFor(difficulty, act))
                {
                    result.Add(encounter);
                }
            }

            return result;
        }

        /// <summary>
        /// 특정 ID의 인카운터 반환
        /// </summary>
        /// <param name="encounterId">인카운터 ID</param>
        /// <returns>해당 인카운터, 없으면 null</returns>
        public EncounterDataSO GetEncounterById(string encounterId)
        {
            foreach (var encounter in _encounters)
            {
                if (encounter != null && encounter.EncounterId == encounterId)
                {
                    return encounter;
                }
            }
            return null;
        }

        /// <summary>
        /// 인카운터 추가
        /// </summary>
        public void AddEncounter(EncounterDataSO encounter)
        {
            if (encounter != null && !_encounters.Contains(encounter))
            {
                _encounters.Add(encounter);
            }
        }

        /// <summary>
        /// 인카운터 제거
        /// </summary>
        public bool RemoveEncounter(EncounterDataSO encounter)
        {
            return _encounters.Remove(encounter);
        }

        #endregion

        #region Editor Validation

#if UNITY_EDITOR
        private void OnValidate()
        {
            // ID가 비어있으면 에셋 이름으로 설정
            if (string.IsNullOrEmpty(_poolId))
            {
                _poolId = name.ToLower().Replace(" ", "_");
            }

            // null 인카운터 제거
            _encounters.RemoveAll(e => e == null);
        }
#endif

        #endregion
    }
}
