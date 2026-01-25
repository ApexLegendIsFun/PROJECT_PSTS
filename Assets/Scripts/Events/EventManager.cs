using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectSS.Data;
using ProjectSS.Run;

namespace ProjectSS.Events
{
    /// <summary>
    /// 이벤트 관리자
    /// Event manager
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [Header("Event Pools")]
        [SerializeField] private EventData[] allEvents;
        [SerializeField] private EventData[] act1Events;
        [SerializeField] private EventData[] act2Events;
        [SerializeField] private EventData[] act3Events;

        // 이번 런에서 본 이벤트
        private HashSet<string> seenEvents = new HashSet<string>();

        // 현재 이벤트
        private EventData currentEvent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadEventDatabase();
        }

        /// <summary>
        /// 이벤트 데이터베이스 로드
        /// Load event database
        /// </summary>
        private void LoadEventDatabase()
        {
            if (allEvents == null || allEvents.Length == 0)
            {
                allEvents = Resources.LoadAll<EventData>("Data/Events");
            }

            Debug.Log($"[EventManager] Loaded {allEvents?.Length ?? 0} events");
        }

        /// <summary>
        /// 런 시작 시 초기화
        /// Initialize on run start
        /// </summary>
        public void ResetForNewRun()
        {
            seenEvents.Clear();
        }

        /// <summary>
        /// 랜덤 이벤트 가져오기
        /// Get random event
        /// </summary>
        public EventData GetRandomEvent()
        {
            EventData[] pool = GetEventPool();

            if (pool == null || pool.Length == 0)
            {
                Debug.LogWarning("[EventManager] No events available");
                return null;
            }

            // 본 적 없는 이벤트 우선
            var unseenEvents = pool.Where(e => !seenEvents.Contains(e.eventId)).ToArray();

            EventData selected;
            if (unseenEvents.Length > 0)
            {
                selected = unseenEvents[Random.Range(0, unseenEvents.Length)];
            }
            else
            {
                // 모든 이벤트를 봤으면 다시 리셋
                seenEvents.Clear();
                selected = pool[Random.Range(0, pool.Length)];
            }

            seenEvents.Add(selected.eventId);
            currentEvent = selected;

            Debug.Log($"[EventManager] Selected event: {selected.eventTitle}");
            return selected;
        }

        /// <summary>
        /// 현재 액트에 맞는 이벤트 풀 가져오기
        /// Get event pool for current act
        /// </summary>
        private EventData[] GetEventPool()
        {
            // 층에 따라 액트 결정
            int floor = RunManager.Instance?.CurrentRun?.currentFloor ?? 1;

            if (floor <= 5 && act1Events != null && act1Events.Length > 0)
                return act1Events;
            else if (floor <= 10 && act2Events != null && act2Events.Length > 0)
                return act2Events;
            else if (act3Events != null && act3Events.Length > 0)
                return act3Events;

            return allEvents;
        }

        /// <summary>
        /// ID로 이벤트 가져오기
        /// Get event by ID
        /// </summary>
        public EventData GetEventById(string eventId)
        {
            if (allEvents == null) return null;

            return allEvents.FirstOrDefault(e => e.eventId == eventId);
        }

        /// <summary>
        /// 현재 이벤트
        /// Current event
        /// </summary>
        public EventData CurrentEvent => currentEvent;

        /// <summary>
        /// 선택지 조건 확인
        /// Check choice condition
        /// </summary>
        public bool CheckCondition(string condition)
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            var run = RunManager.Instance?.CurrentRun;
            if (run == null) return true;

            // 골드 조건
            if (condition.StartsWith("gold>="))
            {
                if (int.TryParse(condition.Substring(6), out int required))
                    return run.gold >= required;
            }
            else if (condition.StartsWith("gold<"))
            {
                if (int.TryParse(condition.Substring(5), out int required))
                    return run.gold < required;
            }

            // HP 조건 (파티 전체 HP 비율)
            if (condition.StartsWith("hp>="))
            {
                if (int.TryParse(condition.Substring(4), out int percent))
                {
                    if (RunManager.Instance.IsPartyMode)
                    {
                        var party = RunManager.Instance.PartyState;
                        int totalCurrent = party.warrior.currentHP + party.mage.currentHP + party.rogue.currentHP;
                        int totalMax = party.warrior.maxHP + party.mage.maxHP + party.rogue.maxHP;
                        int currentPercent = (int)((float)totalCurrent / totalMax * 100);
                        return currentPercent >= percent;
                    }
                }
            }
            else if (condition.StartsWith("hp<"))
            {
                if (int.TryParse(condition.Substring(3), out int percent))
                {
                    if (RunManager.Instance.IsPartyMode)
                    {
                        var party = RunManager.Instance.PartyState;
                        int totalCurrent = party.warrior.currentHP + party.mage.currentHP + party.rogue.currentHP;
                        int totalMax = party.warrior.maxHP + party.mage.maxHP + party.rogue.maxHP;
                        int currentPercent = (int)((float)totalCurrent / totalMax * 100);
                        return currentPercent < percent;
                    }
                }
            }

            // 유물 조건
            if (condition.StartsWith("hasRelic:"))
            {
                string relicId = condition.Substring(9);
                return run.relicIds.Contains(relicId);
            }
            else if (condition.StartsWith("!hasRelic:"))
            {
                string relicId = condition.Substring(10);
                return !run.relicIds.Contains(relicId);
            }

            // 층 조건
            if (condition.StartsWith("floor>="))
            {
                if (int.TryParse(condition.Substring(7), out int required))
                    return run.currentFloor >= required;
            }
            else if (condition.StartsWith("floor<"))
            {
                if (int.TryParse(condition.Substring(6), out int required))
                    return run.currentFloor < required;
            }

            return true;
        }

        /// <summary>
        /// 결과 적용
        /// Apply outcome
        /// </summary>
        public string ApplyOutcome(EventOutcome outcome)
        {
            var run = RunManager.Instance?.CurrentRun;
            if (run == null) return "Error: No run active";

            string resultMessage = "";

            switch (outcome.outcomeType)
            {
                case EventOutcomeType.GainGold:
                    run.gold += outcome.value;
                    resultMessage = $"골드 +{outcome.value}";
                    break;

                case EventOutcomeType.LoseGold:
                    int goldLoss = Mathf.Min(run.gold, Mathf.Abs(outcome.value));
                    run.gold -= goldLoss;
                    resultMessage = $"골드 -{goldLoss}";
                    break;

                case EventOutcomeType.GainHP:
                    resultMessage = ApplyPartyHeal(outcome.value);
                    break;

                case EventOutcomeType.LoseHP:
                    resultMessage = ApplyPartyDamage(Mathf.Abs(outcome.value));
                    break;

                case EventOutcomeType.GainMaxHP:
                    resultMessage = ApplyPartyMaxHPGain(outcome.value);
                    break;

                case EventOutcomeType.LoseMaxHP:
                    resultMessage = ApplyPartyMaxHPLoss(Mathf.Abs(outcome.value));
                    break;

                case EventOutcomeType.GainCard:
                    if (outcome.card != null && RunManager.Instance.IsPartyMode)
                    {
                        RunManager.Instance.PartyState.AddCard(outcome.card, false);
                        resultMessage = $"카드 획득: {outcome.card.cardName}";
                    }
                    break;

                case EventOutcomeType.RemoveCard:
                    resultMessage = "카드 제거 기회 획득";
                    // TODO: 카드 제거 UI 연동
                    break;

                case EventOutcomeType.UpgradeCard:
                    resultMessage = "카드 업그레이드 기회 획득";
                    // TODO: 카드 업그레이드 UI 연동
                    break;

                case EventOutcomeType.GainRelic:
                    if (outcome.relic != null)
                    {
                        run.relicIds.Add(outcome.relic.relicId);
                        resultMessage = $"유물 획득: {outcome.relic.relicName}";
                    }
                    break;

                case EventOutcomeType.StartCombat:
                    resultMessage = "전투 시작!";
                    // TODO: 전투 씬으로 전환
                    break;

                case EventOutcomeType.Nothing:
                    resultMessage = outcome.description ?? "아무 일도 없었다.";
                    break;
            }

            return resultMessage;
        }

        /// <summary>
        /// 파티 힐 적용
        /// Apply party heal
        /// </summary>
        private string ApplyPartyHeal(int amount)
        {
            if (!RunManager.Instance.IsPartyMode)
                return "";

            var party = RunManager.Instance.PartyState;

            // 각 캐릭터에게 분배
            int perCharacter = amount / 3;
            party.warrior.Heal(perCharacter);
            party.mage.Heal(perCharacter);
            party.rogue.Heal(perCharacter);

            return $"전체 HP +{amount}";
        }

        /// <summary>
        /// 파티 데미지 적용
        /// Apply party damage
        /// </summary>
        private string ApplyPartyDamage(int amount)
        {
            if (!RunManager.Instance.IsPartyMode)
                return "";

            var party = RunManager.Instance.PartyState;

            // Active 캐릭터에게만 데미지
            var active = party.GetActive();
            active.TakeDamage(amount);

            return $"{active.characterClass} HP -{amount}";
        }

        /// <summary>
        /// 파티 최대 HP 증가
        /// Apply party max HP gain
        /// </summary>
        private string ApplyPartyMaxHPGain(int amount)
        {
            if (!RunManager.Instance.IsPartyMode)
                return "";

            var party = RunManager.Instance.PartyState;

            int perCharacter = amount / 3;
            party.warrior.maxHP += perCharacter;
            party.warrior.currentHP += perCharacter;
            party.mage.maxHP += perCharacter;
            party.mage.currentHP += perCharacter;
            party.rogue.maxHP += perCharacter;
            party.rogue.currentHP += perCharacter;

            return $"전체 최대HP +{amount}";
        }

        /// <summary>
        /// 파티 최대 HP 감소
        /// Apply party max HP loss
        /// </summary>
        private string ApplyPartyMaxHPLoss(int amount)
        {
            if (!RunManager.Instance.IsPartyMode)
                return "";

            var party = RunManager.Instance.PartyState;

            int perCharacter = amount / 3;

            party.warrior.maxHP = Mathf.Max(1, party.warrior.maxHP - perCharacter);
            party.warrior.currentHP = Mathf.Min(party.warrior.currentHP, party.warrior.maxHP);

            party.mage.maxHP = Mathf.Max(1, party.mage.maxHP - perCharacter);
            party.mage.currentHP = Mathf.Min(party.mage.currentHP, party.mage.maxHP);

            party.rogue.maxHP = Mathf.Max(1, party.rogue.maxHP - perCharacter);
            party.rogue.currentHP = Mathf.Min(party.rogue.currentHP, party.rogue.maxHP);

            return $"전체 최대HP -{amount}";
        }
    }
}
