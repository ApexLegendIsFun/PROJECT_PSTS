// Combat/TurnManager.cs
// 개별 턴 관리자 - 도래관 스타일 턴 순서

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 개별 턴 관리자
    /// 각 유닛이 속도 기반으로 개별 턴을 가짐
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("Turn State")]
        [SerializeField] private int _currentRound = 0;
        [SerializeField] private int _currentTurnIndex = 0;

        // 턴 순서 큐
        private List<ICombatEntity> _turnOrder = new();

        // 현재 턴 유닛
        private ICombatEntity _currentEntity;

        // 프로퍼티
        public int CurrentRound => _currentRound;
        public ICombatEntity CurrentEntity => _currentEntity;
        public IReadOnlyList<ICombatEntity> TurnOrder => _turnOrder;
        public bool IsPlayerTurn => _currentEntity?.IsPlayerCharacter ?? false;

        #region Turn Order Setup

        /// <summary>
        /// 전투 시작 시 턴 순서 초기화
        /// </summary>
        public void InitializeTurnOrder(IEnumerable<ICombatEntity> allEntities)
        {
            _turnOrder.Clear();
            _currentRound = 0;
            _currentTurnIndex = 0;

            // 생존한 엔티티만 추가
            foreach (var entity in allEntities)
            {
                if (entity.IsAlive)
                {
                    _turnOrder.Add(entity);
                }
            }

            // 속도 기준 내림차순 정렬 (빠른 순서대로)
            SortBySpeed();

            Debug.Log($"[TurnManager] Turn order initialized with {_turnOrder.Count} entities");
            LogTurnOrder();
        }

        /// <summary>
        /// 속도 기준 정렬
        /// </summary>
        private void SortBySpeed()
        {
            _turnOrder = _turnOrder
                .OrderByDescending(e => e.Speed)
                .ThenBy(e => e.IsPlayerCharacter ? 0 : 1) // 동일 속도시 플레이어 우선
                .ToList();
        }

        /// <summary>
        /// 사망한 엔티티 제거
        /// </summary>
        public void RemoveDeadEntities()
        {
            _turnOrder.RemoveAll(e => !e.IsAlive);
        }

        #endregion

        #region Round Management

        /// <summary>
        /// 새 라운드 시작
        /// </summary>
        public void StartNewRound()
        {
            _currentRound++;
            _currentTurnIndex = 0;

            Debug.Log($"[TurnManager] ===== Round {_currentRound} Started =====");

            // 사망한 엔티티 제거 후 재정렬
            RemoveDeadEntities();
            SortBySpeed();

            // 모든 엔티티 라운드 시작 처리
            foreach (var entity in _turnOrder)
            {
                entity.OnRoundStart();
            }

            EventBus.Publish(new RoundStartedEvent { RoundNumber = _currentRound });

            LogTurnOrder();
        }

        /// <summary>
        /// 라운드 종료
        /// </summary>
        public void EndRound()
        {
            Debug.Log($"[TurnManager] ===== Round {_currentRound} Ended =====");

            // 모든 엔티티 라운드 종료 처리
            foreach (var entity in _turnOrder)
            {
                entity.OnRoundEnd();
            }

            EventBus.Publish(new RoundEndedEvent { RoundNumber = _currentRound });
        }

        /// <summary>
        /// 라운드가 완료되었는지 확인
        /// </summary>
        public bool IsRoundComplete()
        {
            return _currentTurnIndex >= _turnOrder.Count;
        }

        #endregion

        #region Turn Management

        /// <summary>
        /// 다음 턴 시작
        /// </summary>
        public ICombatEntity StartNextTurn()
        {
            // 사망한 엔티티 스킵
            while (_currentTurnIndex < _turnOrder.Count && !_turnOrder[_currentTurnIndex].IsAlive)
            {
                _currentTurnIndex++;
            }

            if (_currentTurnIndex >= _turnOrder.Count)
            {
                _currentEntity = null;
                return null;
            }

            _currentEntity = _turnOrder[_currentTurnIndex];
            _currentEntity.OnTurnStart();

            Debug.Log($"[TurnManager] Turn {_currentTurnIndex + 1}/{_turnOrder.Count}: {_currentEntity.DisplayName}");

            return _currentEntity;
        }

        /// <summary>
        /// 현재 턴 종료
        /// </summary>
        public void EndCurrentTurn()
        {
            if (_currentEntity != null)
            {
                _currentEntity.OnTurnEnd();
                Debug.Log($"[TurnManager] {_currentEntity.DisplayName}'s turn ended");
            }

            _currentTurnIndex++;
            _currentEntity = null;
        }

        /// <summary>
        /// 현재 턴의 엔티티 가져오기
        /// </summary>
        public ICombatEntity GetCurrentEntity()
        {
            return _currentEntity;
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 특정 팀의 생존 엔티티 수
        /// </summary>
        public int GetAliveCount(bool isPlayerTeam)
        {
            return _turnOrder.Count(e => e.IsAlive && e.IsPlayerCharacter == isPlayerTeam);
        }

        /// <summary>
        /// 플레이어 팀 전멸 여부
        /// </summary>
        public bool IsPlayerTeamDefeated()
        {
            return GetAliveCount(true) == 0;
        }

        /// <summary>
        /// 적 팀 전멸 여부
        /// </summary>
        public bool IsEnemyTeamDefeated()
        {
            return GetAliveCount(false) == 0;
        }

        /// <summary>
        /// 특정 엔티티의 턴 순서 인덱스
        /// </summary>
        public int GetTurnIndex(ICombatEntity entity)
        {
            return _turnOrder.IndexOf(entity);
        }

        #endregion

        #region Debug

        private void LogTurnOrder()
        {
            var order = string.Join(" -> ",
                _turnOrder.Select(e => $"{e.DisplayName}({e.Speed})" + (e.IsPlayerCharacter ? "[P]" : "[E]")));
            Debug.Log($"[TurnManager] Order: {order}");
        }

        #endregion
    }
}
