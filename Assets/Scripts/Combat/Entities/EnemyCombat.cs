// Combat/Entities/EnemyCombat.cs
// 적 전투 엔티티

using UnityEngine;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 적 의도 타입
    /// </summary>
    public enum EnemyIntentType
    {
        Attack,         // 공격
        Defend,         // 방어
        Buff,           // 버프
        Debuff,         // 디버프
        Unknown         // 알 수 없음
    }

    /// <summary>
    /// 적 의도 데이터
    /// </summary>
    [System.Serializable]
    public class EnemyIntent
    {
        public EnemyIntentType Type;
        public int Value;           // 피해량 또는 효과 수치
        public string TargetId;     // 타겟 ID (null이면 무작위)

        public EnemyIntent(EnemyIntentType type, int value = 0, string targetId = null)
        {
            Type = type;
            Value = value;
            TargetId = targetId;
        }
    }

    /// <summary>
    /// 적 전투 엔티티
    /// AI 의도 시스템 포함
    /// </summary>
    public class EnemyCombat : CombatEntity
    {
        [Header("Enemy Info")]
        [SerializeField] private string _enemyType = "Normal";

        [Header("Intent")]
        [SerializeField] private EnemyIntent _currentIntent;

        [Header("Attack Patterns")]
        [SerializeField] private int _baseAttackDamage = 10;
        [SerializeField] private int _baseBlockAmount = 5;

        // 프로퍼티
        public override bool IsPlayerCharacter => false;
        public string EnemyType => _enemyType;
        public EnemyIntent CurrentIntent => _currentIntent;

        #region Intent System

        /// <summary>
        /// 다음 의도 결정
        /// </summary>
        public void DecideNextIntent()
        {
            // 간단한 AI: 랜덤하게 공격 또는 방어
            float roll = Random.value;

            if (roll < 0.7f)
            {
                // 70% 확률로 공격
                _currentIntent = new EnemyIntent(EnemyIntentType.Attack, _baseAttackDamage);
            }
            else
            {
                // 30% 확률로 방어
                _currentIntent = new EnemyIntent(EnemyIntentType.Defend, _baseBlockAmount);
            }

            Debug.Log($"[{DisplayName}] Intent: {_currentIntent.Type} ({_currentIntent.Value})");
        }

        /// <summary>
        /// 현재 의도 실행
        /// </summary>
        public void ExecuteIntent(ICombatEntity[] playerParty)
        {
            if (_currentIntent == null || !IsAlive)
            {
                return;
            }

            switch (_currentIntent.Type)
            {
                case EnemyIntentType.Attack:
                    ExecuteAttack(playerParty);
                    break;

                case EnemyIntentType.Defend:
                    ExecuteDefend();
                    break;

                case EnemyIntentType.Buff:
                    ExecuteBuff();
                    break;

                case EnemyIntentType.Debuff:
                    ExecuteDebuff(playerParty);
                    break;
            }
        }

        private void ExecuteAttack(ICombatEntity[] playerParty)
        {
            // 타겟 선택 (지정된 타겟 또는 랜덤)
            ICombatEntity target = null;

            if (!string.IsNullOrEmpty(_currentIntent.TargetId))
            {
                foreach (var entity in playerParty)
                {
                    if (entity.EntityId == _currentIntent.TargetId && entity.IsAlive)
                    {
                        target = entity;
                        break;
                    }
                }
            }

            // 타겟이 없으면 랜덤 선택
            if (target == null)
            {
                var aliveTargets = System.Array.FindAll(playerParty, e => e.IsAlive);
                if (aliveTargets.Length > 0)
                {
                    target = aliveTargets[Random.Range(0, aliveTargets.Length)];
                }
            }

            if (target != null)
            {
                Debug.Log($"[{DisplayName}] Attacks {target.DisplayName} for {_currentIntent.Value} damage");
                target.TakeDamage(_currentIntent.Value, this);
            }
        }

        private void ExecuteDefend()
        {
            Debug.Log($"[{DisplayName}] Defends for {_currentIntent.Value} block");
            GainBlock(_currentIntent.Value);
        }

        private void ExecuteBuff()
        {
            Debug.Log($"[{DisplayName}] Uses buff");
            // TODO: 버프 효과 구현
        }

        private void ExecuteDebuff(ICombatEntity[] playerParty)
        {
            Debug.Log($"[{DisplayName}] Uses debuff");
            // TODO: 디버프 효과 구현
        }

        #endregion

        #region Turn Events

        public override void OnTurnStart()
        {
            base.OnTurnStart();

            EventBus.Publish(new TurnStartedEvent
            {
                EntityId = EntityId,
                IsPlayerCharacter = false
            });
        }

        public override void OnTurnEnd()
        {
            base.OnTurnEnd();

            // 다음 의도 결정
            DecideNextIntent();

            EventBus.Publish(new TurnEndedEvent
            {
                EntityId = EntityId,
                IsPlayerCharacter = false
            });
        }

        public override void OnRoundStart()
        {
            base.OnRoundStart();

            // 첫 라운드에 의도 결정
            if (_currentIntent == null)
            {
                DecideNextIntent();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 적 데이터로 초기화
        /// </summary>
        public void Initialize(string enemyId, string name, string enemyType, int maxHP, int speed, int baseDamage)
        {
            _entityId = enemyId;
            _displayName = name;
            _enemyType = enemyType;
            _maxHP = maxHP;
            _currentHP = maxHP;
            _speed = speed;
            _baseAttackDamage = baseDamage;

            // 초기 의도 결정
            DecideNextIntent();
        }

        #endregion
    }
}
