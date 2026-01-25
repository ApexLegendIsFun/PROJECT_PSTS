using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 플레이어 전투 엔티티
    /// Player combat entity
    /// </summary>
    public class PlayerCombat : CombatEntity
    {
        public static PlayerCombat Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
            entityId = "player";
            entityName = "Player";
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public override void OnTurnStart()
        {
            base.OnTurnStart();
            // 플레이어 턴 시작 시 블록 초기화
            // Clear block at player turn start
            ClearBlock();
        }

        /// <summary>
        /// 런 상태에서 플레이어 초기화
        /// Initialize player from run state
        /// </summary>
        public void InitializeFromRunState(int hp, int maxHp)
        {
            maxHealth = maxHp;
            currentHealth = hp;
        }
    }
}
