using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Core;
using ProjectSS.Data;
using ProjectSS.Events;
using ProjectSS.Run;

namespace ProjectSS.UI
{
    /// <summary>
    /// 이벤트 씬 UI 관리자
    /// Event scene UI manager
    /// </summary>
    public class EventUI : MonoBehaviour
    {
        [Header("Event Display")]
        [SerializeField] private Image eventImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Choices")]
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Outcome Panel")]
        [SerializeField] private GameObject outcomePanel;
        [SerializeField] private TextMeshProUGUI outcomeText;
        [SerializeField] private Button continueButton;

        [Header("Player Status")]
        [SerializeField] private PartyStatusDisplay partyStatusDisplay;
        [SerializeField] private TextMeshProUGUI goldText;

        // 상태
        private EventData currentEvent;
        private bool choiceSelected = false;
        private List<GameObject> choiceButtons = new List<GameObject>();

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 초기화
        /// Initialize
        /// </summary>
        private void Initialize()
        {
            choiceSelected = false;

            // 버튼 이벤트 연결
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(OnContinue);
            }

            // 결과 패널 숨기기
            if (outcomePanel != null)
                outcomePanel.SetActive(false);

            // 이벤트 로드
            LoadEvent();

            // 플레이어 상태 표시
            UpdatePlayerStatus();
        }

        /// <summary>
        /// 이벤트 로드
        /// Load event
        /// </summary>
        private void LoadEvent()
        {
            if (EventManager.Instance == null)
            {
                Debug.LogWarning("[EventUI] EventManager not found, using test event");
                SetupTestEvent();
                return;
            }

            currentEvent = EventManager.Instance.GetRandomEvent();

            if (currentEvent == null)
            {
                Debug.LogWarning("[EventUI] No event available");
                SetupTestEvent();
                return;
            }

            DisplayEvent(currentEvent);

            // 이벤트 발행
            EventBus.Publish(new EventSceneStartedEvent(currentEvent.eventId));
        }

        /// <summary>
        /// 테스트 이벤트 설정 (에디터 테스트용)
        /// Setup test event for editor testing
        /// </summary>
        private void SetupTestEvent()
        {
#if UNITY_EDITOR
            if (titleText != null)
                titleText.text = "신비로운 상자 (Test Event)";

            if (descriptionText != null)
                descriptionText.text = "길가에서 신비로운 상자를 발견했다.\n열어볼 것인가?";

            // 테스트 선택지
            ClearChoices();

            CreateTestChoiceButton("상자를 연다 [50G 획득]", 0);
            CreateTestChoiceButton("무시하고 지나간다", 1);
#endif
        }

#if UNITY_EDITOR
        private void CreateTestChoiceButton(string text, int index)
        {
            if (choiceButtonPrefab == null || choicesContainer == null) return;

            var buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);

            var textComp = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComp != null)
                textComp.text = text;

            var button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                int choiceIndex = index;
                button.onClick.AddListener(() => OnTestChoiceSelected(choiceIndex));
            }

            choiceButtons.Add(buttonObj);
        }

        private void OnTestChoiceSelected(int index)
        {
            if (choiceSelected) return;
            choiceSelected = true;

            DisableChoices();

            string result = index == 0 ? "상자에서 50G를 발견했다!" : "상자를 무시하고 지나갔다.";
            ShowOutcome(result);
        }
#endif

        /// <summary>
        /// 이벤트 표시
        /// Display event
        /// </summary>
        private void DisplayEvent(EventData eventData)
        {
            if (eventImage != null && eventData.eventImage != null)
            {
                eventImage.sprite = eventData.eventImage;
                eventImage.gameObject.SetActive(true);
            }
            else if (eventImage != null)
            {
                eventImage.gameObject.SetActive(false);
            }

            if (titleText != null)
                titleText.text = eventData.eventTitle;

            if (descriptionText != null)
                descriptionText.text = eventData.eventDescription;

            // 선택지 생성
            DisplayChoices(eventData);
        }

        /// <summary>
        /// 선택지 표시
        /// Display choices
        /// </summary>
        private void DisplayChoices(EventData eventData)
        {
            ClearChoices();

            if (choiceButtonPrefab == null || choicesContainer == null) return;

            for (int i = 0; i < eventData.choices.Count; i++)
            {
                var choice = eventData.choices[i];
                bool conditionMet = EventManager.Instance?.CheckCondition(choice.condition) ?? true;

                // 조건 미충족 시 표시 여부 확인
                if (!conditionMet && !choice.showWhenDisabled)
                    continue;

                var buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);

                // 텍스트 설정
                var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = choice.choiceText;

                    // 조건 미충족 시 회색 표시
                    if (!conditionMet)
                    {
                        text.color = Color.gray;
                        text.text += $"\n<size=80%><color=red>[조건 미충족: {choice.condition}]</color></size>";
                    }
                }

                // 버튼 설정
                var button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = conditionMet;

                    if (conditionMet)
                    {
                        int choiceIndex = i;
                        button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
                    }
                }

                choiceButtons.Add(buttonObj);
            }
        }

        /// <summary>
        /// 선택지 버튼 정리
        /// Clear choice buttons
        /// </summary>
        private void ClearChoices()
        {
            foreach (var btn in choiceButtons)
            {
                if (btn != null) Destroy(btn);
            }
            choiceButtons.Clear();
        }

        /// <summary>
        /// 선택지 비활성화
        /// Disable choices
        /// </summary>
        private void DisableChoices()
        {
            foreach (var btn in choiceButtons)
            {
                if (btn != null)
                {
                    var button = btn.GetComponent<Button>();
                    if (button != null)
                        button.interactable = false;
                }
            }
        }

        /// <summary>
        /// 선택지 선택
        /// Choice selected
        /// </summary>
        private void OnChoiceSelected(int choiceIndex)
        {
            if (choiceSelected || currentEvent == null) return;
            if (choiceIndex < 0 || choiceIndex >= currentEvent.choices.Count) return;

            choiceSelected = true;

            // 선택지 버튼 비활성화
            DisableChoices();

            // 결과 적용
            var choice = currentEvent.choices[choiceIndex];
            var results = new List<string>();

            foreach (var outcome in choice.outcomes)
            {
                string result = EventManager.Instance?.ApplyOutcome(outcome) ?? "";
                if (!string.IsNullOrEmpty(result))
                    results.Add(result);
            }

            // 이벤트 발행
            EventBus.Publish(new EventChoiceSelectedEvent(currentEvent.eventId, choiceIndex));

            // 결과 표시
            string resultMessage = results.Count > 0
                ? string.Join("\n", results)
                : "아무 일도 없었다.";

            ShowOutcome(resultMessage);

            // 플레이어 상태 갱신
            UpdatePlayerStatus();

            Debug.Log($"[EventUI] Choice {choiceIndex} selected: {choice.choiceText}");
        }

        /// <summary>
        /// 결과 표시
        /// Show outcome
        /// </summary>
        private void ShowOutcome(string message)
        {
            if (outcomePanel != null)
            {
                outcomePanel.SetActive(true);

                if (outcomeText != null)
                    outcomeText.text = message;
            }
        }

        /// <summary>
        /// 플레이어 상태 업데이트
        /// Update player status
        /// </summary>
        private void UpdatePlayerStatus()
        {
            if (partyStatusDisplay != null)
                partyStatusDisplay.Refresh();

            if (goldText != null && RunManager.Instance != null)
            {
                int gold = RunManager.Instance.CurrentRun?.gold ?? 0;
                goldText.text = $"{gold}G";
            }
        }

        /// <summary>
        /// 계속하기 (맵으로 복귀)
        /// Continue (return to map)
        /// </summary>
        private void OnContinue()
        {
            // 이벤트 완료 발행
            if (currentEvent != null)
            {
                EventBus.Publish(new EventCompletedEvent(currentEvent.eventId));
            }

            // 맵으로 복귀
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadMap();
            }

            Debug.Log("[EventUI] Returning to map");
        }

        private void OnDisable()
        {
            ClearChoices();
        }
    }
}
