using System.Collections.Generic;
using ProjectSS.Data;
using ProjectSS.Core;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 상태이상 관리자
    /// Status effect manager
    /// </summary>
    public class StatusEffectManager
    {
        private readonly ICombatEntity owner;
        private Dictionary<StatusEffectType, StatusEffectInstance> activeEffects = new();

        public StatusEffectManager(ICombatEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// 상태이상 적용
        /// Apply status effect
        /// </summary>
        public void ApplyStatus(StatusEffectData data, int stacks)
        {
            if (data == null || stacks <= 0) return;

            if (activeEffects.TryGetValue(data.effectType, out var existing))
            {
                // 기존 효과에 스택 추가
                existing.AddStacks(stacks);
            }
            else
            {
                // 새 효과 추가
                activeEffects[data.effectType] = new StatusEffectInstance(data, stacks);
            }

            EventBus.Publish(new StatusEffectAppliedEvent(owner.EntityId, data.statusId, stacks));
        }

        /// <summary>
        /// 상태이상 보유 여부
        /// Check if has status
        /// </summary>
        public bool HasStatus(StatusEffectType type)
        {
            return activeEffects.ContainsKey(type) && activeEffects[type].Stacks > 0;
        }

        /// <summary>
        /// 스택 수 반환
        /// Get stack count
        /// </summary>
        public int GetStacks(StatusEffectType type)
        {
            return activeEffects.TryGetValue(type, out var effect) ? effect.Stacks : 0;
        }

        /// <summary>
        /// 상태이상 제거
        /// Remove status effect
        /// </summary>
        public void RemoveStatus(StatusEffectType type)
        {
            activeEffects.Remove(type);
        }

        /// <summary>
        /// 모든 상태이상 제거
        /// Clear all status effects
        /// </summary>
        public void ClearAll()
        {
            activeEffects.Clear();
        }

        /// <summary>
        /// 모든 디버프 제거
        /// Clear all debuffs
        /// </summary>
        public void ClearDebuffs()
        {
            var toRemove = new List<StatusEffectType>();
            foreach (var kvp in activeEffects)
            {
                if (kvp.Value.Data.isDebuff)
                    toRemove.Add(kvp.Key);
            }
            foreach (var type in toRemove)
            {
                activeEffects.Remove(type);
            }
        }

        /// <summary>
        /// 특정 시점 효과 발동
        /// Trigger effects at specific timing
        /// </summary>
        public void TriggerEffects(StatusTrigger trigger)
        {
            foreach (var effect in activeEffects.Values)
            {
                if (effect.Data.triggerTime == trigger)
                {
                    ApplyEffectTrigger(effect);
                }
            }
        }

        /// <summary>
        /// 지속시간 감소 (턴 종료 시)
        /// Tick durations (at turn end)
        /// </summary>
        public void TickDurations()
        {
            var toRemove = new List<StatusEffectType>();

            foreach (var kvp in activeEffects)
            {
                var effect = kvp.Value;

                // Duration 기반 효과는 스택 감소
                if (effect.Data.stackBehavior == StackBehavior.Duration)
                {
                    effect.RemoveStacks(1);
                    if (effect.Stacks <= 0)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }
            }

            foreach (var type in toRemove)
            {
                activeEffects.Remove(type);
            }
        }

        private void ApplyEffectTrigger(StatusEffectInstance effect)
        {
            switch (effect.Data.effectType)
            {
                case StatusEffectType.Poison:
                    // 독: 턴 종료 시 스택만큼 데미지, 스택 1 감소
                    owner.TakeDamage(effect.Stacks);
                    effect.RemoveStacks(1);
                    break;

                case StatusEffectType.Regeneration:
                    // 재생: 턴 종료 시 스택만큼 회복, 스택 1 감소
                    owner.Heal(effect.Stacks);
                    effect.RemoveStacks(1);
                    break;

                // Strength, Dexterity, Weak, Vulnerable, Frail은 DamageCalculator에서 처리
            }
        }

        /// <summary>
        /// 활성화된 모든 효과 반환
        /// Get all active effects
        /// </summary>
        public IEnumerable<StatusEffectInstance> GetAllEffects()
        {
            return activeEffects.Values;
        }
    }

    /// <summary>
    /// 상태이상 인스턴스
    /// Status effect instance
    /// </summary>
    public class StatusEffectInstance
    {
        public StatusEffectData Data { get; private set; }
        public int Stacks { get; private set; }

        public StatusEffectInstance(StatusEffectData data, int stacks)
        {
            Data = data;
            Stacks = stacks;
        }

        public void AddStacks(int amount)
        {
            Stacks += amount;
        }

        public void RemoveStacks(int amount)
        {
            Stacks = System.Math.Max(0, Stacks - amount);
        }

        public void SetStacks(int amount)
        {
            Stacks = System.Math.Max(0, amount);
        }
    }
}
