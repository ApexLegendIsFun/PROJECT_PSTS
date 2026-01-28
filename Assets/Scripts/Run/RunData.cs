// Run/RunData.cs
// 런 상태 데이터 클래스

using System;
using System.Collections.Generic;
using ProjectSS.Core;

namespace ProjectSS.Run
{
    /// <summary>
    /// 런 진행 중인 파티 멤버 상태
    /// </summary>
    [Serializable]
    public class RunPartyMember
    {
        /// <summary>
        /// 캐릭터 고유 ID
        /// </summary>
        public string CharacterId;

        /// <summary>
        /// 표시 이름
        /// </summary>
        public string DisplayName;

        /// <summary>
        /// 캐릭터 클래스
        /// </summary>
        public CharacterClass CharacterClass;

        /// <summary>
        /// 현재 HP
        /// </summary>
        public int CurrentHP;

        /// <summary>
        /// 최대 HP
        /// </summary>
        public int MaxHP;

        /// <summary>
        /// 최대 에너지
        /// </summary>
        public int MaxEnergy;

        /// <summary>
        /// 속도
        /// </summary>
        public int Speed;

        /// <summary>
        /// 덱에 있는 카드 ID 목록
        /// </summary>
        public List<string> DeckCardIds = new List<string>();

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public RunPartyMember()
        {
        }

        /// <summary>
        /// 복사 생성자
        /// </summary>
        public RunPartyMember(RunPartyMember other)
        {
            CharacterId = other.CharacterId;
            DisplayName = other.DisplayName;
            CharacterClass = other.CharacterClass;
            CurrentHP = other.CurrentHP;
            MaxHP = other.MaxHP;
            MaxEnergy = other.MaxEnergy;
            Speed = other.Speed;
            DeckCardIds = new List<string>(other.DeckCardIds);
        }

        /// <summary>
        /// 파라미터 생성자
        /// </summary>
        public RunPartyMember(string id, string name, CharacterClass characterClass,
            int maxHP, int maxEnergy, int speed)
        {
            CharacterId = id;
            DisplayName = name;
            CharacterClass = characterClass;
            MaxHP = maxHP;
            CurrentHP = maxHP;
            MaxEnergy = maxEnergy;
            Speed = speed;
        }

        /// <summary>
        /// 체력 회복
        /// </summary>
        public void Heal(int amount)
        {
            CurrentHP = Math.Min(CurrentHP + amount, MaxHP);
        }

        /// <summary>
        /// 피해 적용
        /// </summary>
        public void TakeDamage(int amount)
        {
            CurrentHP = Math.Max(CurrentHP - amount, 0);
        }

        /// <summary>
        /// 살아있는지 확인
        /// </summary>
        public bool IsAlive => CurrentHP > 0;

        /// <summary>
        /// HP 비율 (0-1)
        /// </summary>
        public float HPRatio => MaxHP > 0 ? (float)CurrentHP / MaxHP : 0f;
    }

    /// <summary>
    /// 런 전체 상태 데이터
    /// </summary>
    [Serializable]
    public class RunData
    {
        /// <summary>
        /// 런이 활성화되어 있는지
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// 현재 Act (1부터 시작)
        /// </summary>
        public int CurrentAct = 1;

        /// <summary>
        /// 현재 층 (0부터 시작)
        /// </summary>
        public int CurrentFloor = 0;

        /// <summary>
        /// 보유 골드
        /// </summary>
        public int Gold = 0;

        /// <summary>
        /// 파티 멤버 목록
        /// </summary>
        public List<RunPartyMember> PartyMembers = new List<RunPartyMember>();

        /// <summary>
        /// 수집한 유물 ID 목록
        /// </summary>
        public List<string> CollectedRelics = new List<string>();

        /// <summary>
        /// 런 시작 시간 (Unix timestamp)
        /// </summary>
        public long StartTimestamp;

        /// <summary>
        /// 승리한 전투 수
        /// </summary>
        public int CombatsWon = 0;

        /// <summary>
        /// 패배한 전투 수
        /// </summary>
        public int CombatsLost = 0;

        /// <summary>
        /// 현재 맵 노드 ID
        /// </summary>
        public string CurrentNodeId;

        /// <summary>
        /// 클리어한 노드 ID 목록
        /// </summary>
        public List<string> ClearedNodeIds = new List<string>();

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public RunData()
        {
            StartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 새 런 초기화
        /// </summary>
        public void Initialize(List<RunPartyMember> party)
        {
            IsActive = true;
            CurrentAct = 1;
            CurrentFloor = 0;
            Gold = 0;
            PartyMembers = new List<RunPartyMember>(party);
            CollectedRelics.Clear();
            ClearedNodeIds.Clear();
            CombatsWon = 0;
            CombatsLost = 0;
            StartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// 런 종료
        /// </summary>
        public void End()
        {
            IsActive = false;
        }

        /// <summary>
        /// 현재 난이도 계산 (Act와 Floor 기반)
        /// </summary>
        public int GetCurrentDifficulty()
        {
            // 기본 난이도 = Act * 3 + Floor
            return (CurrentAct - 1) * 3 + CurrentFloor + 1;
        }

        /// <summary>
        /// 파티가 전멸했는지 확인
        /// </summary>
        public bool IsPartyWiped()
        {
            foreach (var member in PartyMembers)
            {
                if (member.IsAlive) return false;
            }
            return true;
        }

        /// <summary>
        /// 생존한 파티 멤버 수
        /// </summary>
        public int GetAlivePartyCount()
        {
            int count = 0;
            foreach (var member in PartyMembers)
            {
                if (member.IsAlive) count++;
            }
            return count;
        }

        /// <summary>
        /// ID로 파티 멤버 찾기
        /// </summary>
        public RunPartyMember GetPartyMember(string characterId)
        {
            foreach (var member in PartyMembers)
            {
                if (member.CharacterId == characterId)
                    return member;
            }
            return null;
        }

        /// <summary>
        /// 런 진행 시간 (초)
        /// </summary>
        public long GetElapsedSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds() - StartTimestamp;
        }
    }
}
