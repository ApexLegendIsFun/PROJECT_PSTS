using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Combat;
using ProjectSS.Data;
using ProjectSS.Core;

namespace ProjectSS.UI
{
    /// <summary>
    /// 적 Break 게이지 UI
    /// Enemy Break gauge UI
    ///
    /// TRIAD: 패턴 캔슬 시스템을 위한 Break 진행도 표시
    /// Displays Break progress for Pattern Cancel system
    /// </summary>
    public class BreakGaugeUI : MonoBehaviour
    {
        [Header("Gauge Display")]
        [SerializeField] private Slider gaugeSlider;
        [SerializeField] private Image gaugeFill;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Groggy Indicator")]
        [SerializeField] private GameObject groggyIndicator;
        [SerializeField] private TextMeshProUGUI groggyTurnsText;

        [Header("Condition Display")]
        [SerializeField] private TextMeshProUGUI conditionText;
        [SerializeField] private Image conditionIcon;

        [Header("Colors")]
        [SerializeField] private Color lowProgressColor = new Color(0.3f, 0.5f, 0.7f);
        [SerializeField] private Color mediumProgressColor = new Color(0.7f, 0.7f, 0.3f);
        [SerializeField] private Color highProgressColor = new Color(0.9f, 0.4f, 0.2f);
        [SerializeField] private Color groggyColor = new Color(1f, 0.5f, 0.1f);
        [SerializeField] private Color notBreakableColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        [Header("Animation")]
        [SerializeField] private bool animateOnBreak = true;
        [SerializeField] private float breakFlashDuration = 0.5f;

        private EnemyCombat linkedEnemy;
        private bool isGroggy;

        /// <summary>
        /// 연결된 적
        /// Linked enemy
        /// </summary>
        public EnemyCombat LinkedEnemy => linkedEnemy;

        private void OnEnable()
        {
            SubscribeEvents();
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }

        /// <summary>
        /// 적과 연결
        /// Link to enemy
        /// </summary>
        public void SetEnemy(EnemyCombat enemy)
        {
            linkedEnemy = enemy;

            if (enemy == null)
            {
                gameObject.SetActive(false);
                return;
            }

            // Break 가능한 적인지 확인
            // Check if enemy is breakable
            if (!enemy.IsBreakable)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            SetupConditionDisplay();
            RefreshDisplay();
        }

        /// <summary>
        /// 조건 표시 설정
        /// Setup condition display
        /// </summary>
        private void SetupConditionDisplay()
        {
            if (linkedEnemy == null) return;

            var breakGauge = linkedEnemy.BreakGauge;
            var conditionData = breakGauge?.GetConditionData();

            if (conditionData == null) return;

            if (conditionText != null)
            {
                conditionText.text = GetConditionShortText(conditionData);
            }
        }

        /// <summary>
        /// 조건 약어 텍스트
        /// Get condition short text
        /// </summary>
        private string GetConditionShortText(BreakConditionData condition)
        {
            return condition.conditionType switch
            {
                BreakConditionType.DamageThreshold => $"DMG:{condition.damageThreshold}",
                BreakConditionType.HitCount => $"HIT:{condition.hitCountThreshold}",
                BreakConditionType.Both => $"DMG:{condition.damageThreshold} | HIT:{condition.hitCountThreshold}",
                _ => ""
            };
        }

        /// <summary>
        /// 디스플레이 새로고침
        /// Refresh display
        /// </summary>
        public void RefreshDisplay()
        {
            if (linkedEnemy == null || !linkedEnemy.IsBreakable)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            isGroggy = linkedEnemy.IsGroggy;

            if (isGroggy)
            {
                ShowGroggyState();
            }
            else
            {
                ShowProgressState();
            }
        }

        /// <summary>
        /// 진행 상태 표시
        /// Show progress state
        /// </summary>
        private void ShowProgressState()
        {
            if (groggyIndicator != null)
            {
                groggyIndicator.SetActive(false);
            }

            float progress = linkedEnemy.GetBreakProgress();

            // 슬라이더 업데이트
            // Update slider
            if (gaugeSlider != null)
            {
                gaugeSlider.gameObject.SetActive(true);
                gaugeSlider.value = progress;
            }

            // 진행률 텍스트
            // Progress text
            if (progressText != null)
            {
                int percentage = Mathf.RoundToInt(progress * 100);
                progressText.text = $"{percentage}%";
            }

            // 색상 업데이트
            // Update color
            if (gaugeFill != null)
            {
                if (progress < 0.5f)
                {
                    gaugeFill.color = Color.Lerp(lowProgressColor, mediumProgressColor, progress * 2f);
                }
                else
                {
                    gaugeFill.color = Color.Lerp(mediumProgressColor, highProgressColor, (progress - 0.5f) * 2f);
                }
            }
        }

        /// <summary>
        /// Groggy 상태 표시
        /// Show groggy state
        /// </summary>
        private void ShowGroggyState()
        {
            // 게이지 숨기기
            // Hide gauge
            if (gaugeSlider != null)
            {
                gaugeSlider.gameObject.SetActive(false);
            }

            // Groggy 인디케이터 표시
            // Show groggy indicator
            if (groggyIndicator != null)
            {
                groggyIndicator.SetActive(true);
            }

            // 남은 턴 표시
            // Show remaining turns
            if (groggyTurnsText != null && linkedEnemy.BreakGauge != null)
            {
                int turnsLeft = linkedEnemy.BreakGauge.GroggyTurnsLeft;
                groggyTurnsText.text = $"GROGGY\n{turnsLeft}T";
            }

            // Groggy 색상
            // Groggy color
            if (gaugeFill != null)
            {
                gaugeFill.color = groggyColor;
            }
        }

        /// <summary>
        /// Break 발생 효과
        /// Break triggered effect
        /// </summary>
        public void PlayBreakEffect()
        {
            if (animateOnBreak)
            {
                StartCoroutine(BreakEffectCoroutine());
            }
        }

        private System.Collections.IEnumerator BreakEffectCoroutine()
        {
            // 플래시 효과
            // Flash effect
            if (gaugeFill != null)
            {
                Color originalColor = gaugeFill.color;
                gaugeFill.color = Color.white;

                float elapsed = 0f;
                while (elapsed < breakFlashDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / breakFlashDuration;
                    gaugeFill.color = Color.Lerp(Color.white, groggyColor, t);
                    yield return null;
                }

                gaugeFill.color = groggyColor;
            }
        }

        /// <summary>
        /// 툴팁 텍스트 가져오기
        /// Get tooltip text
        /// </summary>
        public string GetTooltipText()
        {
            if (linkedEnemy == null || !linkedEnemy.IsBreakable)
            {
                return "Break 불가 / Cannot be broken";
            }

            var breakGauge = linkedEnemy.BreakGauge;
            var conditionData = breakGauge?.GetConditionData();

            if (conditionData == null)
            {
                return "";
            }

            if (isGroggy)
            {
                return $"GROGGY!\n약화된 행동 패턴 사용 중\nUsing weakened action pattern\n남은 턴: {breakGauge.GroggyTurnsLeft}";
            }

            return conditionData.GetDescription() +
                   $"\n\n현재 진행: {Mathf.RoundToInt(linkedEnemy.GetBreakProgress() * 100)}%" +
                   $"\nCurrent Progress: {Mathf.RoundToInt(linkedEnemy.GetBreakProgress() * 100)}%";
        }

        #region Event Handling

        private void SubscribeEvents()
        {
            EventBus.Subscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Subscribe<EnemyBrokenEvent>(OnEnemyBroken);
            EventBus.Subscribe<EnemyGroggyEndedEvent>(OnEnemyGroggyEnded);
            EventBus.Subscribe<TurnStartedEvent>(OnTurnStarted);
        }

        private void UnsubscribeEvents()
        {
            EventBus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            EventBus.Unsubscribe<EnemyBrokenEvent>(OnEnemyBroken);
            EventBus.Unsubscribe<EnemyGroggyEndedEvent>(OnEnemyGroggyEnded);
            EventBus.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        }

        private void OnDamageTaken(DamageTakenEvent evt)
        {
            if (linkedEnemy != null && evt.EntityId == linkedEnemy.EntityId)
            {
                RefreshDisplay();
            }
        }

        private void OnEnemyBroken(EnemyBrokenEvent evt)
        {
            if (linkedEnemy != null && evt.Enemy == linkedEnemy)
            {
                PlayBreakEffect();
                RefreshDisplay();
            }
        }

        private void OnEnemyGroggyEnded(EnemyGroggyEndedEvent evt)
        {
            if (linkedEnemy != null && evt.Enemy == linkedEnemy)
            {
                RefreshDisplay();
            }
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            RefreshDisplay();
        }

        #endregion
    }
}
