// Combat/Entities/ICombatEntity.cs
// 전투 엔티티 인터페이스

using System.Collections.Generic;

namespace ProjectSS.Combat
{
    /// <summary>
    /// 전투 참여 엔티티 인터페이스
    /// 플레이어 캐릭터와 적 모두 구현
    /// </summary>
    public interface ICombatEntity
    {
        /// <summary>
        /// 고유 ID
        /// </summary>
        string EntityId { get; }

        /// <summary>
        /// 표시 이름
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 플레이어 캐릭터 여부
        /// </summary>
        bool IsPlayerCharacter { get; }

        /// <summary>
        /// 현재 HP
        /// </summary>
        int CurrentHP { get; }

        /// <summary>
        /// 최대 HP
        /// </summary>
        int MaxHP { get; }

        /// <summary>
        /// 현재 방어막
        /// </summary>
        int CurrentBlock { get; }

        /// <summary>
        /// 생존 여부
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 턴 순서 결정용 속도값
        /// </summary>
        int Speed { get; }

        /// <summary>
        /// 피해 받기
        /// </summary>
        void TakeDamage(int damage, ICombatEntity source);

        /// <summary>
        /// 회복
        /// </summary>
        void Heal(int amount);

        /// <summary>
        /// 방어막 획득
        /// </summary>
        void GainBlock(int amount);

        /// <summary>
        /// 턴 시작 시 호출
        /// </summary>
        void OnTurnStart();

        /// <summary>
        /// 턴 종료 시 호출
        /// </summary>
        void OnTurnEnd();

        /// <summary>
        /// 라운드 시작 시 호출
        /// </summary>
        void OnRoundStart();

        /// <summary>
        /// 라운드 종료 시 호출
        /// </summary>
        void OnRoundEnd();
    }
}
