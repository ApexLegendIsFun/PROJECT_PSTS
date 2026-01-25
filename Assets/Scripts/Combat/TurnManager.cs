using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Data;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 턴 진행 관리자
    /// Turn progression manager
    ///
    /// TRIAD: PartyManager 통합으로 3인 파티 지원
    /// </summary>
    public class TurnManager
    {
        public enum TurnPhase
        {
            Idle,
            PlayerTurnStart,
            PlayerAction,
            PlayerTurnEnd,
            EnemyTurn,
            CombatEnd
        }

        public TurnPhase CurrentPhase { get; private set; } = TurnPhase.Idle;
        public int TurnNumber { get; private set; }
        public bool IsPlayerTurn => CurrentPhase == TurnPhase.PlayerAction;

        private PartyManager partyManager;
        private List<EnemyCombat> enemies;
        private EnergySystem energy;
        private DeckManager deck;

        /// <summary>
        /// TRIAD: PartyManager 기반 생성자
        /// TRIAD: PartyManager-based constructor
        /// </summary>
        public TurnManager(PartyManager partyManager, List<EnemyCombat> enemies, EnergySystem energy, DeckManager deck)
        {
            this.partyManager = partyManager;
            this.enemies = enemies;
            this.energy = energy;
            this.deck = deck;
        }

        /// <summary>
        /// 전투 시작
        /// Start combat
        /// </summary>
        public void StartCombat()
        {
            TurnNumber = 0;
            CurrentPhase = TurnPhase.Idle;
            StartPlayerTurn();
        }

        /// <summary>
        /// 플레이어 턴 시작
        /// Start player turn
        ///
        /// TRIAD: 모든 파티원 턴 시작 처리
        /// </summary>
        public void StartPlayerTurn()
        {
            TurnNumber++;
            CurrentPhase = TurnPhase.PlayerTurnStart;

            // TRIAD: 모든 파티원 턴 시작 처리
            // All party members turn start processing
            partyManager.OnPartyTurnStart();

            energy.Refill();
            deck.DrawCards(DeckManager.DEFAULT_DRAW_COUNT);

            // 액션 페이즈로 전환
            CurrentPhase = TurnPhase.PlayerAction;
            EventBus.Publish(new TurnStartedEvent(true, TurnNumber));
        }

        /// <summary>
        /// 플레이어 턴 종료 (버튼 클릭 시)
        /// End player turn (on button click)
        ///
        /// TRIAD: Standby Focus 증가 후 모든 파티원 턴 종료
        /// </summary>
        public void EndPlayerTurn()
        {
            if (CurrentPhase != TurnPhase.PlayerAction) return;

            CurrentPhase = TurnPhase.PlayerTurnEnd;

            // 핸드 버리기
            deck.DiscardHand();

            // TRIAD: Standby 캐릭터 Focus +1
            // Standby characters gain +1 Focus
            partyManager.ProcessTurnEndFocus();

            // TRIAD: 모든 파티원 턴 종료 처리
            // All party members turn end processing
            partyManager.OnPartyTurnEnd();

            EventBus.Publish(new TurnEndedEvent(true));

            // 적 턴으로 전환
            StartEnemyTurn();
        }

        /// <summary>
        /// 적 턴 시작
        /// Start enemy turn
        /// </summary>
        private void StartEnemyTurn()
        {
            CurrentPhase = TurnPhase.EnemyTurn;
            EventBus.Publish(new TurnStartedEvent(false, TurnNumber));
        }

        /// <summary>
        /// 적 행동 실행 (코루틴으로 순차 실행)
        /// Execute enemy actions (should be called from coroutine)
        ///
        /// TRIAD: 적 타겟팅 전략에 따라 파티원 선택
        /// </summary>
        public IEnumerator ExecuteEnemyActions()
        {
            foreach (var enemy in enemies)
            {
                if (!enemy.IsAlive) continue;

                enemy.OnTurnStart();
                yield return new WaitForSeconds(0.3f);

                // TRIAD: 적의 타겟팅 전략에 따라 파티원 선택
                // Select party member based on enemy targeting strategy
                var target = partyManager.GetTarget(enemy.TargetingStrategy);
                if (target != null)
                {
                    enemy.ExecuteIntent(target);
                }
                yield return new WaitForSeconds(0.5f);

                enemy.OnTurnEnd();
            }

            EventBus.Publish(new TurnEndedEvent(false));

            // 플레이어 턴으로 돌아감
            StartPlayerTurn();
        }

        /// <summary>
        /// 전투 종료
        /// End combat
        /// </summary>
        public void EndCombat()
        {
            CurrentPhase = TurnPhase.CombatEnd;
        }
    }
}
