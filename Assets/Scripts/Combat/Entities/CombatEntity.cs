// Combat/Entities/CombatEntity.cs
// 전투 엔티티 기본 클래스

using UnityEngine;
using ProjectSS.Core;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 전투 엔티티 기본 구현
    /// </summary>
    public abstract class CombatEntity : MonoBehaviour, ICombatEntity
    {
        [Header("Entity Info")]
        [SerializeField] protected string _entityId;
        [SerializeField] protected string _displayName;

        [Header("Stats")]
        [SerializeField] protected int _maxHP = 100;
        [SerializeField] protected int _currentHP = 100;
        [SerializeField] protected int _currentBlock = 0;
        [SerializeField] protected int _speed = 10;

        // ICombatEntity 구현
        public string EntityId => _entityId;
        public string DisplayName => _displayName;
        public abstract bool IsPlayerCharacter { get; }
        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
        public int CurrentBlock => _currentBlock;
        public bool IsAlive => _currentHP > 0;
        public int Speed => _speed;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(_entityId))
            {
                _entityId = System.Guid.NewGuid().ToString();
            }
        }

        #region Combat Actions

        /// <summary>
        /// 피해 받기
        /// </summary>
        public virtual void TakeDamage(int damage, ICombatEntity source)
        {
            if (!IsAlive) return;

            int blockedAmount = Mathf.Min(_currentBlock, damage);
            int actualDamage = damage - blockedAmount;

            _currentBlock -= blockedAmount;
            _currentHP = Mathf.Max(0, _currentHP - actualDamage);

            Debug.Log($"[{DisplayName}] Took {actualDamage} damage (blocked {blockedAmount}). HP: {_currentHP}/{_maxHP}");

            EventBus.Publish(new DamageDealtEvent
            {
                SourceId = source?.EntityId,
                TargetId = EntityId,
                Damage = actualDamage,
                BlockedAmount = blockedAmount,
                IsCritical = false
            });

            if (!IsAlive)
            {
                OnDeath();
            }
        }

        /// <summary>
        /// 회복
        /// </summary>
        public virtual void Heal(int amount)
        {
            if (!IsAlive) return;

            int previousHP = _currentHP;
            _currentHP = Mathf.Min(_maxHP, _currentHP + amount);
            int actualHeal = _currentHP - previousHP;

            Debug.Log($"[{DisplayName}] Healed {actualHeal}. HP: {_currentHP}/{_maxHP}");
        }

        /// <summary>
        /// 방어막 획득
        /// </summary>
        public virtual void GainBlock(int amount)
        {
            if (!IsAlive) return;

            _currentBlock += amount;

            Debug.Log($"[{DisplayName}] Gained {amount} block. Total: {_currentBlock}");

            EventBus.Publish(new BlockGainedEvent
            {
                EntityId = EntityId,
                Amount = amount,
                TotalBlock = _currentBlock
            });
        }

        /// <summary>
        /// 사망 처리
        /// </summary>
        protected virtual void OnDeath()
        {
            Debug.Log($"[{DisplayName}] Died!");

            EventBus.Publish(new EntityDiedEvent
            {
                EntityId = EntityId,
                IsPlayerCharacter = IsPlayerCharacter
            });
        }

        #endregion

        #region Turn Events

        /// <summary>
        /// 턴 시작
        /// </summary>
        public virtual void OnTurnStart()
        {
            Debug.Log($"[{DisplayName}] Turn started");
        }

        /// <summary>
        /// 턴 종료
        /// </summary>
        public virtual void OnTurnEnd()
        {
            Debug.Log($"[{DisplayName}] Turn ended");
        }

        /// <summary>
        /// 라운드 시작 - 방어막 초기화
        /// </summary>
        public virtual void OnRoundStart()
        {
            // 라운드 시작 시 방어막 초기화 (슬더스 스타일)
            _currentBlock = 0;
        }

        /// <summary>
        /// 라운드 종료
        /// </summary>
        public virtual void OnRoundEnd()
        {
            // 상태 효과 적용 등
        }

        #endregion

        #region Utility

        /// <summary>
        /// 스탯 초기화
        /// </summary>
        public virtual void InitializeStats(int maxHP, int speed)
        {
            _maxHP = maxHP;
            _currentHP = maxHP;
            _currentBlock = 0;
            _speed = speed;
        }

        #endregion
    }
}
