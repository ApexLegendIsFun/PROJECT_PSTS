using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 에너지 시스템
    /// Energy system for card playing
    /// </summary>
    public class EnergySystem
    {
        public int CurrentEnergy { get; private set; }
        public int MaxEnergy { get; private set; }

        public EnergySystem(int maxEnergy = 3)
        {
            MaxEnergy = maxEnergy;
            CurrentEnergy = 0;
        }

        /// <summary>
        /// 턴 시작 시 에너지 충전
        /// Refill energy at turn start
        /// </summary>
        public void Refill()
        {
            CurrentEnergy = MaxEnergy;
            EventBus.Publish(new EnergyChangedEvent(CurrentEnergy, MaxEnergy));
        }

        /// <summary>
        /// 에너지 사용 가능 여부
        /// Check if can spend energy
        /// </summary>
        public bool CanSpend(int amount)
        {
            return CurrentEnergy >= amount;
        }

        /// <summary>
        /// 에너지 사용
        /// Spend energy
        /// </summary>
        public bool Spend(int amount)
        {
            if (!CanSpend(amount)) return false;

            CurrentEnergy -= amount;
            EventBus.Publish(new EnergyChangedEvent(CurrentEnergy, MaxEnergy));
            return true;
        }

        /// <summary>
        /// 에너지 획득
        /// Gain energy
        /// </summary>
        public void Gain(int amount)
        {
            CurrentEnergy += amount;
            EventBus.Publish(new EnergyChangedEvent(CurrentEnergy, MaxEnergy));
        }

        /// <summary>
        /// 최대 에너지 설정
        /// Set max energy
        /// </summary>
        public void SetMaxEnergy(int amount)
        {
            MaxEnergy = amount;
            EventBus.Publish(new EnergyChangedEvent(CurrentEnergy, MaxEnergy));
        }
    }
}
