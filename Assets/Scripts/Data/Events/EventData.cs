using System.Collections.Generic;
using UnityEngine;

namespace ProjectSS.Data
{
    /// <summary>
    /// 이벤트 데이터 ScriptableObject
    /// Event data ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewEvent", menuName = "Game/Events/Event Data")]
    public class EventData : ScriptableObject
    {
        [Header("기본 정보 (Basic Info)")]
        [Tooltip("고유 ID / Unique ID")]
        public string eventId;

        [Tooltip("이벤트 제목 / Event title")]
        public string eventTitle;

        [Tooltip("이벤트 설명 / Event description")]
        [TextArea(4, 8)]
        public string eventDescription;

        [Tooltip("이벤트 이미지 / Event image")]
        public Sprite eventImage;

        [Header("선택지 (Choices)")]
        [Tooltip("선택지 목록 / List of choices")]
        public List<EventChoice> choices = new List<EventChoice>();

        private void OnValidate()
        {
            // 자동으로 ID 생성 (비어있을 경우)
            // Auto-generate ID if empty
            if (string.IsNullOrEmpty(eventId))
            {
                eventId = name.Replace(" ", "_").ToLower();
            }
        }
    }

    /// <summary>
    /// 이벤트 선택지 정의
    /// Event choice definition
    /// </summary>
    [System.Serializable]
    public class EventChoice
    {
        [Tooltip("선택지 텍스트 / Choice text")]
        public string choiceText;

        [Tooltip("선택지 결과 목록 / List of outcomes")]
        public List<EventOutcome> outcomes = new List<EventOutcome>();

        [Tooltip("선택 조건 (ex: 골드 50 이상) / Condition for availability")]
        public string condition;

        [Tooltip("비활성화 시 표시 여부 / Show when disabled")]
        public bool showWhenDisabled = true;
    }

    /// <summary>
    /// 이벤트 결과 정의
    /// Event outcome definition
    /// </summary>
    [System.Serializable]
    public class EventOutcome
    {
        [Tooltip("결과 타입 / Outcome type")]
        public EventOutcomeType outcomeType;

        [Tooltip("수치 (양수: 획득, 음수: 손실) / Value (positive: gain, negative: lose)")]
        public int value;

        [Tooltip("얻는 카드 / Card to gain")]
        public CardData card;

        [Tooltip("얻는 유물 / Relic to gain")]
        public RelicData relic;

        [Tooltip("결과 설명 / Outcome description")]
        public string description;
    }

    /// <summary>
    /// 이벤트 결과 타입
    /// Event outcome type
    /// </summary>
    public enum EventOutcomeType
    {
        GainGold,           // 골드 획득
        LoseGold,           // 골드 손실
        GainHP,             // 체력 회복
        LoseHP,             // 체력 손실
        GainMaxHP,          // 최대 체력 증가
        LoseMaxHP,          // 최대 체력 감소
        GainCard,           // 카드 획득
        RemoveCard,         // 카드 제거
        UpgradeCard,        // 카드 업그레이드
        GainRelic,          // 유물 획득
        StartCombat,        // 전투 시작
        Nothing             // 아무 일도 없음
    }
}
