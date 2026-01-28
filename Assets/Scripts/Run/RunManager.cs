// Run/RunManager.cs
// 런 상태 관리 매니저

using System.Collections.Generic;
using UnityEngine;
using ProjectSS.Core;

namespace ProjectSS.Run
{
    /// <summary>
    /// 런 상태 관리 매니저 - 싱글톤
    /// 전투 간 파티 상태 유지, 진행 상황 추적
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [Header("Current Run")]
        [SerializeField] private RunData _currentRun;

        [Header("Debug")]
        [SerializeField] private bool _debugMode = false;

        #region Properties

        /// <summary>
        /// 현재 런 데이터
        /// </summary>
        public RunData CurrentRun => _currentRun;

        /// <summary>
        /// 활성화된 런이 있는지
        /// </summary>
        public bool HasActiveRun => _currentRun != null && _currentRun.IsActive;

        /// <summary>
        /// 현재 층
        /// </summary>
        public int CurrentFloor => _currentRun?.CurrentFloor ?? 0;

        /// <summary>
        /// 현재 Act
        /// </summary>
        public int CurrentAct => _currentRun?.CurrentAct ?? 1;

        /// <summary>
        /// 현재 난이도
        /// </summary>
        public int CurrentDifficulty => _currentRun?.GetCurrentDifficulty() ?? 1;

        /// <summary>
        /// 보유 골드
        /// </summary>
        public int Gold => _currentRun?.Gold ?? 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 서비스 등록
            ServiceLocator.Register(this);

            Log("RunManager initialized.");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ServiceLocator.Unregister<RunManager>();
                Instance = null;
            }
        }

        #endregion

        #region Run Lifecycle

        /// <summary>
        /// 새 런 시작
        /// </summary>
        /// <param name="partyMembers">파티 멤버 목록</param>
        public void StartNewRun(List<RunPartyMember> partyMembers)
        {
            Log("Starting new run...");

            _currentRun = new RunData();
            _currentRun.Initialize(partyMembers);

            Log($"Run started with {partyMembers.Count} party members.");
        }

        /// <summary>
        /// 기본 파티로 새 런 시작
        /// </summary>
        public void StartNewRunWithDefaultParty()
        {
            var defaultParty = CreateDefaultParty();
            StartNewRun(defaultParty);
        }

        /// <summary>
        /// 런 종료
        /// </summary>
        /// <param name="victory">승리 여부</param>
        public void EndRun(bool victory)
        {
            if (_currentRun == null) return;

            _currentRun.End();

            if (victory)
            {
                Log($"Run completed! Combats won: {_currentRun.CombatsWon}");
            }
            else
            {
                Log($"Run failed. Combats won: {_currentRun.CombatsWon}");
            }
        }

        /// <summary>
        /// 현재 런 데이터 초기화
        /// </summary>
        public void ClearRun()
        {
            _currentRun = null;
            Log("Run cleared.");
        }

        #endregion

        #region Combat Integration

        /// <summary>
        /// 전투용 파티 설정 데이터 반환
        /// </summary>
        /// <returns>PartyMemberSetup 리스트</returns>
        public List<PartyMemberSetup> GetPartySetupForCombat()
        {
            if (_currentRun == null || _currentRun.PartyMembers == null)
            {
                Log("No active run, returning default party setup.");
                return CreateDefaultPartySetup();
            }

            var setups = new List<PartyMemberSetup>();

            foreach (var member in _currentRun.PartyMembers)
            {
                if (!member.IsAlive) continue;

                setups.Add(new PartyMemberSetup
                {
                    CharacterId = member.CharacterId,
                    DisplayName = member.DisplayName,
                    CharacterClass = member.CharacterClass,
                    CurrentHP = member.CurrentHP,
                    MaxHP = member.MaxHP,
                    MaxEnergy = member.MaxEnergy,
                    Speed = member.Speed
                });
            }

            Log($"Returning {setups.Count} party members for combat.");
            return setups;
        }

        /// <summary>
        /// 전투 완료 처리
        /// </summary>
        /// <param name="victory">승리 여부</param>
        /// <param name="goldEarned">획득 골드</param>
        public void OnCombatComplete(bool victory, int goldEarned = 0)
        {
            if (_currentRun == null) return;

            if (victory)
            {
                _currentRun.CombatsWon++;
                _currentRun.Gold += goldEarned;
                Log($"Combat won! Gold earned: {goldEarned}, Total: {_currentRun.Gold}");
            }
            else
            {
                _currentRun.CombatsLost++;
                Log("Combat lost.");
            }
        }

        /// <summary>
        /// 전투 후 파티 체력 동기화
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="currentHP">현재 HP</param>
        public void UpdatePartyHealth(string characterId, int currentHP)
        {
            if (_currentRun == null) return;

            var member = _currentRun.GetPartyMember(characterId);
            if (member != null)
            {
                member.CurrentHP = Mathf.Max(0, currentHP);
                Log($"Updated {characterId} HP to {currentHP}");
            }
        }

        /// <summary>
        /// 전투 후 모든 파티 멤버 체력 동기화
        /// </summary>
        /// <param name="healthUpdates">캐릭터ID -> 현재HP 딕셔너리</param>
        public void SyncPartyHealthFromCombat(Dictionary<string, int> healthUpdates)
        {
            if (_currentRun == null) return;

            foreach (var kvp in healthUpdates)
            {
                UpdatePartyHealth(kvp.Key, kvp.Value);
            }
        }

        #endregion

        #region Progression

        /// <summary>
        /// 다음 층으로 이동
        /// </summary>
        public void AdvanceFloor()
        {
            if (_currentRun == null) return;

            _currentRun.CurrentFloor++;
            Log($"Advanced to floor {_currentRun.CurrentFloor}");
        }

        /// <summary>
        /// 다음 Act로 이동
        /// </summary>
        public void AdvanceAct()
        {
            if (_currentRun == null) return;

            _currentRun.CurrentAct++;
            _currentRun.CurrentFloor = 0;
            Log($"Advanced to Act {_currentRun.CurrentAct}");
        }

        /// <summary>
        /// 노드 클리어 처리
        /// </summary>
        /// <param name="nodeId">클리어한 노드 ID</param>
        public void MarkNodeCleared(string nodeId)
        {
            if (_currentRun == null) return;

            if (!_currentRun.ClearedNodeIds.Contains(nodeId))
            {
                _currentRun.ClearedNodeIds.Add(nodeId);
                Log($"Node {nodeId} marked as cleared.");
            }
        }

        /// <summary>
        /// 현재 노드 설정
        /// </summary>
        /// <param name="nodeId">노드 ID</param>
        public void SetCurrentNode(string nodeId)
        {
            if (_currentRun == null) return;

            _currentRun.CurrentNodeId = nodeId;
        }

        /// <summary>
        /// 노드가 클리어되었는지 확인
        /// </summary>
        public bool IsNodeCleared(string nodeId)
        {
            return _currentRun?.ClearedNodeIds.Contains(nodeId) ?? false;
        }

        #endregion

        #region Resources

        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(int amount)
        {
            if (_currentRun == null) return;

            _currentRun.Gold += amount;
            Log($"Added {amount} gold. Total: {_currentRun.Gold}");
        }

        /// <summary>
        /// 골드 사용
        /// </summary>
        /// <returns>성공 여부</returns>
        public bool SpendGold(int amount)
        {
            if (_currentRun == null || _currentRun.Gold < amount)
                return false;

            _currentRun.Gold -= amount;
            Log($"Spent {amount} gold. Remaining: {_currentRun.Gold}");
            return true;
        }

        /// <summary>
        /// 휴식 (파티 회복)
        /// </summary>
        /// <param name="healPercent">회복 비율 (0-1)</param>
        public void RestParty(float healPercent = 0.3f)
        {
            if (_currentRun == null) return;

            foreach (var member in _currentRun.PartyMembers)
            {
                if (member.IsAlive)
                {
                    int healAmount = Mathf.RoundToInt(member.MaxHP * healPercent);
                    member.Heal(healAmount);
                }
            }

            Log($"Party rested. Healed {healPercent * 100}%");
        }

        #endregion

        #region Default Data

        /// <summary>
        /// 기본 파티 생성
        /// </summary>
        private List<RunPartyMember> CreateDefaultParty()
        {
            return new List<RunPartyMember>
            {
                new RunPartyMember("warrior_01", "전사", CharacterClass.Warrior, 50, 3, 10),
                new RunPartyMember("mage_01", "마법사", CharacterClass.Mage, 40, 4, 8),
                new RunPartyMember("healer_01", "힐러", CharacterClass.Healer, 45, 3, 12)
            };
        }

        /// <summary>
        /// 기본 파티 설정 생성 (폴백용)
        /// </summary>
        private List<PartyMemberSetup> CreateDefaultPartySetup()
        {
            return new List<PartyMemberSetup>
            {
                new PartyMemberSetup
                {
                    CharacterId = "warrior_01",
                    DisplayName = "전사",
                    CharacterClass = CharacterClass.Warrior,
                    CurrentHP = 50,
                    MaxHP = 50,
                    MaxEnergy = 3,
                    Speed = 10
                },
                new PartyMemberSetup
                {
                    CharacterId = "mage_01",
                    DisplayName = "마법사",
                    CharacterClass = CharacterClass.Mage,
                    CurrentHP = 40,
                    MaxHP = 40,
                    MaxEnergy = 4,
                    Speed = 8
                },
                new PartyMemberSetup
                {
                    CharacterId = "healer_01",
                    DisplayName = "힐러",
                    CharacterClass = CharacterClass.Healer,
                    CurrentHP = 45,
                    MaxHP = 45,
                    MaxEnergy = 3,
                    Speed = 12
                }
            };
        }

        #endregion

        #region Logging

        private void Log(string message)
        {
            if (_debugMode)
            {
                Debug.Log($"[RunManager] {message}");
            }
        }

        #endregion
    }
}
