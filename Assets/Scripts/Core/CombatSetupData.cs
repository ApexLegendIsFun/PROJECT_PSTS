// Core/CombatSetupData.cs
// 전투 설정 데이터 - 씬 전환 시 전투 정보 전달용

using System.Collections.Generic;

namespace ProjectSS.Core
{
    /// <summary>
    /// 전투 설정 데이터
    /// Map -> Combat 씬 전환 시 전투 정보를 전달하는 데이터 컨테이너
    /// </summary>
    [System.Serializable]
    public class CombatSetupData
    {
        /// <summary>
        /// 전투 타입 (Enemy, Elite, Boss 등)
        /// </summary>
        public TileType EncounterType;

        /// <summary>
        /// 파티원 데이터 목록
        /// </summary>
        public List<PartyMemberSetup> PartyMembers = new();

        /// <summary>
        /// 적 생성 설정 (null이면 EncounterType 기반 자동 생성)
        /// </summary>
        public List<EnemySetup> Enemies = null;

        /// <summary>
        /// 맵 노드 ID (전투 후 복귀용)
        /// </summary>
        public string SourceNodeId;

        /// <summary>
        /// 유효성 검사
        /// </summary>
        public bool IsValid()
        {
            return PartyMembers != null && PartyMembers.Count > 0;
        }
    }

    /// <summary>
    /// 파티원 설정 데이터
    /// </summary>
    [System.Serializable]
    public class PartyMemberSetup
    {
        public string CharacterId;
        public string DisplayName;
        public CharacterClass CharacterClass;
        public int CurrentHP;
        public int MaxHP;
        public int MaxEnergy;
        public int Speed;
    }

    /// <summary>
    /// 적 설정 데이터 (수동 지정 시)
    /// </summary>
    [System.Serializable]
    public class EnemySetup
    {
        public string EnemyId;
        public string DisplayName;
        public string EnemyType;
        public int MaxHP;
        public int Speed;
        public int BaseDamage;
    }
}
