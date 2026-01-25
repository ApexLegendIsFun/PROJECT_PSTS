namespace ProjectSS.Combat
{
    /// <summary>
    /// 전투 참여자 인터페이스
    /// Combat participant interface
    /// </summary>
    public interface ICombatEntity
    {
        string EntityId { get; }
        string EntityName { get; }
        int CurrentHealth { get; }
        int MaxHealth { get; }
        int Block { get; }
        bool IsAlive { get; }

        void TakeDamage(int amount);
        void Heal(int amount);
        void GainBlock(int amount);
        void ClearBlock();
        void OnTurnStart();
        void OnTurnEnd();
    }
}
