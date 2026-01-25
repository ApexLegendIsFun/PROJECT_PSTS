using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 전투 엔티티 기본 클래스
    /// Base combat entity class
    /// </summary>
    public abstract class CombatEntity : MonoBehaviour, ICombatEntity
    {
        [SerializeField] protected string entityId;
        [SerializeField] protected string entityName;
        [SerializeField] protected int maxHealth = 100;
        protected int currentHealth;
        protected int block;

        public string EntityId => entityId;
        public string EntityName => entityName;
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public int Block => block;
        public bool IsAlive => currentHealth > 0;

        protected StatusEffectManager statusEffects;

        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            statusEffects = new StatusEffectManager(this);
        }

        public virtual void TakeDamage(int amount)
        {
            // 블록으로 먼저 흡수
            // Absorb with block first
            int damageToBlock = Mathf.Min(block, amount);
            block -= damageToBlock;
            int remainingDamage = amount - damageToBlock;

            if (remainingDamage > 0)
            {
                currentHealth = Mathf.Max(0, currentHealth - remainingDamage);
                EventBus.Publish(new DamageTakenEvent(entityId, remainingDamage, currentHealth));
            }

            if (!IsAlive)
            {
                OnDeath();
            }
        }

        public virtual void Heal(int amount)
        {
            int actualHeal = Mathf.Min(amount, maxHealth - currentHealth);
            currentHealth += actualHeal;
        }

        public virtual void GainBlock(int amount)
        {
            block += amount;
            EventBus.Publish(new BlockGainedEvent(entityId, amount));
        }

        public virtual void ClearBlock()
        {
            block = 0;
        }

        public virtual void OnTurnStart()
        {
            statusEffects.TriggerEffects(Data.StatusTrigger.TurnStart);
        }

        public virtual void OnTurnEnd()
        {
            statusEffects.TriggerEffects(Data.StatusTrigger.TurnEnd);
            statusEffects.TickDurations();
        }

        protected virtual void OnDeath()
        {
            EventBus.Publish(new EntityDiedEvent(entityId, this is PlayerCombat));
        }

        public StatusEffectManager GetStatusEffects() => statusEffects;

        public void SetMaxHealth(int value)
        {
            maxHealth = value;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        public void SetCurrentHealth(int value)
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
        }
    }
}
