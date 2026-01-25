using System.Collections.Generic;
using ProjectSS.Data;
using ProjectSS.Map;

namespace ProjectSS.Run
{
    /// <summary>
    /// 런 상태 (저장/로드용)
    /// Run state (for save/load)
    ///
    /// TRIAD: 파티 모드 지원 추가
    /// </summary>
    [System.Serializable]
    public class RunState
    {
        #region TRIAD Party Mode

        /// <summary>
        /// 파티 모드 여부 (TRIAD 시스템 사용)
        /// Is party mode enabled (using TRIAD system)
        /// </summary>
        public bool isPartyMode = true;

        /// <summary>
        /// 파티 상태 (파티 모드일 때 사용)
        /// Party state (used in party mode)
        /// </summary>
        public PartyState partyState;

        #endregion

        #region Legacy Player State (하위 호환)

        // 플레이어 상태 (레거시)
        public int currentHP;
        public int maxHP;
        public int gold;

        // 덱 (카드 ID 목록) - 레거시, 파티 모드에서는 partyState.deckCardIds 사용
        public List<string> deckCardIds = new List<string>();
        public List<bool> deckUpgraded = new List<bool>();

        #endregion

        // 유물 (유물 ID 목록)
        public List<string> relicIds = new List<string>();

        // 맵 상태
        public int mapSeed;
        public int currentFloor;
        public string currentNodeId;
        public List<string> visitedNodeIds = new List<string>();

        // 런 메타
        public int runSeed;
        public int turnCount;
        public int combatCount;
        public int eliteKills;
        public int bossKills;

        #region TRIAD Performance Stats

        /// <summary>
        /// Speed 보너스 포인트 (빠른 클리어)
        /// Speed bonus points (fast clear)
        /// </summary>
        public int speedBonus;

        /// <summary>
        /// Impact 보너스 포인트 (오버킬)
        /// Impact bonus points (overkill damage)
        /// </summary>
        public int impactBonus;

        /// <summary>
        /// Risk 보너스 포인트 (무기절 클리어)
        /// Risk bonus points (no incapacitation)
        /// </summary>
        public int riskBonus;

        /// <summary>
        /// 총 Tag-In 횟수
        /// Total Tag-In count
        /// </summary>
        public int totalTagIns;

        /// <summary>
        /// 총 Break 횟수
        /// Total Break count
        /// </summary>
        public int totalBreaks;

        #endregion

        public RunState()
        {
            // 기본값 - TRIAD 파티 모드
            isPartyMode = true;
            partyState = new PartyState();

            // 레거시 기본값
            currentHP = 80;
            maxHP = 80;
            gold = 99;
            currentFloor = -1;
            turnCount = 0;
            combatCount = 0;
            eliteKills = 0;
            bossKills = 0;

            // TRIAD 성과 보너스 초기화
            speedBonus = 0;
            impactBonus = 0;
            riskBonus = 0;
            totalTagIns = 0;
            totalBreaks = 0;
        }

        /// <summary>
        /// 초기 덱 설정
        /// Set initial deck
        /// </summary>
        public void SetInitialDeck(List<CardData> cards)
        {
            deckCardIds.Clear();
            deckUpgraded.Clear();

            foreach (var card in cards)
            {
                deckCardIds.Add(card.cardId);
                deckUpgraded.Add(false);
            }
        }

        /// <summary>
        /// 카드 추가
        /// Add card to deck
        /// </summary>
        public void AddCard(CardData card, bool upgraded = false)
        {
            deckCardIds.Add(card.cardId);
            deckUpgraded.Add(upgraded);
        }

        /// <summary>
        /// 카드 제거
        /// Remove card from deck
        /// </summary>
        public bool RemoveCard(int index)
        {
            if (index >= 0 && index < deckCardIds.Count)
            {
                deckCardIds.RemoveAt(index);
                deckUpgraded.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 유물 추가
        /// Add relic
        /// </summary>
        public void AddRelic(RelicData relic)
        {
            if (!relicIds.Contains(relic.relicId))
            {
                relicIds.Add(relic.relicId);
            }
        }

        /// <summary>
        /// 유물 보유 여부
        /// Check if has relic
        /// </summary>
        public bool HasRelic(string relicId)
        {
            return relicIds.Contains(relicId);
        }

        /// <summary>
        /// 복제본 생성
        /// Create copy
        /// </summary>
        public RunState Clone()
        {
            var copy = new RunState
            {
                // TRIAD Party Mode
                isPartyMode = isPartyMode,
                partyState = partyState?.Clone(),

                // Legacy
                currentHP = currentHP,
                maxHP = maxHP,
                gold = gold,
                mapSeed = mapSeed,
                currentFloor = currentFloor,
                currentNodeId = currentNodeId,
                runSeed = runSeed,
                turnCount = turnCount,
                combatCount = combatCount,
                eliteKills = eliteKills,
                bossKills = bossKills,

                // TRIAD Performance
                speedBonus = speedBonus,
                impactBonus = impactBonus,
                riskBonus = riskBonus,
                totalTagIns = totalTagIns,
                totalBreaks = totalBreaks
            };

            copy.deckCardIds.AddRange(deckCardIds);
            copy.deckUpgraded.AddRange(deckUpgraded);
            copy.relicIds.AddRange(relicIds);
            copy.visitedNodeIds.AddRange(visitedNodeIds);

            return copy;
        }

        #region TRIAD Party Mode Helpers

        /// <summary>
        /// 파티 모드 덱 카드 ID 가져오기
        /// Get deck card IDs (party mode aware)
        /// </summary>
        public List<string> GetDeckCardIds()
        {
            if (isPartyMode && partyState != null)
            {
                return partyState.deckCardIds;
            }
            return deckCardIds;
        }

        /// <summary>
        /// 파티 모드 덱 업그레이드 상태 가져오기
        /// Get deck upgrade status (party mode aware)
        /// </summary>
        public List<bool> GetDeckUpgraded()
        {
            if (isPartyMode && partyState != null)
            {
                return partyState.deckUpgraded;
            }
            return deckUpgraded;
        }

        /// <summary>
        /// 총 성과 보너스
        /// Total performance bonus
        /// </summary>
        public int TotalPerformanceBonus => speedBonus + impactBonus + riskBonus;

        #endregion
    }
}
