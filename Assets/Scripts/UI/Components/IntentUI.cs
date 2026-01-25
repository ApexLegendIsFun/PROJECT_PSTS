using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Data;
using ProjectSS.Combat;
using ProjectSS.Core;

namespace ProjectSS.UI
{
    /// <summary>
    /// 적 인텐트 UI
    /// Enemy intent UI
    ///
    /// TRIAD: Break 게이지 및 Groggy 상태 표시 통합
    /// </summary>
    public class IntentUI : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private Image intentIcon;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("TRIAD - Break Gauge")]
        [Tooltip("Break 게이지 UI / Break gauge UI")]
        [SerializeField] private BreakGaugeUI breakGaugeUI;

        [Header("TRIAD - Groggy Indicator")]
        [Tooltip("Groggy 상태 표시 오브젝트 / Groggy state indicator")]
        [SerializeField] private GameObject groggyOverlay;
        [SerializeField] private TextMeshProUGUI groggyText;

        [Header("Intent Icons (Placeholder Colors)")]
        [SerializeField] private Color attackColor = new Color(0.9f, 0.3f, 0.3f);
        [SerializeField] private Color defendColor = new Color(0.3f, 0.5f, 0.9f);
        [SerializeField] private Color buffColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color debuffColor = new Color(0.7f, 0.3f, 0.8f);
        [SerializeField] private Color unknownColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color groggyColor = new Color(1f, 0.5f, 0.1f);

        private EnemyCombat enemy;

        private void OnEnable()
        {
            EventBus.Subscribe<EnemyBrokenEvent>(OnEnemyBroken);
            EventBus.Subscribe<EnemyGroggyEndedEvent>(OnEnemyGroggyEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemyBrokenEvent>(OnEnemyBroken);
            EventBus.Unsubscribe<EnemyGroggyEndedEvent>(OnEnemyGroggyEnded);
        }

        /// <summary>
        /// 적과 연결
        /// Link to enemy
        /// </summary>
        public void SetEnemy(EnemyCombat enemyCombat)
        {
            enemy = enemyCombat;

            // TRIAD: Break 게이지 연결
            if (breakGaugeUI != null)
            {
                breakGaugeUI.SetEnemy(enemy);
            }

            UpdateIntent();
        }

        /// <summary>
        /// 인텐트 업데이트
        /// Update intent display
        /// </summary>
        public void UpdateIntent()
        {
            if (enemy == null || enemy.CurrentIntent == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            var intent = enemy.CurrentIntent;

            // TRIAD: Groggy 상태 표시
            bool isGroggy = enemy.IsGroggy;
            UpdateGroggyDisplay(isGroggy);

            // 아이콘 색상 설정
            if (intentIcon != null)
            {
                // TRIAD: Groggy 상태면 색상 변경
                intentIcon.color = isGroggy ? groggyColor : GetIntentColor(intent.intentType);
            }

            // 수치 표시
            if (valueText != null)
            {
                string value = GetIntentValue(intent);
                valueText.text = value;
                valueText.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }

            // 설명 표시
            if (descriptionText != null)
            {
                string description = intent.GetIntentDescription();
                // TRIAD: Groggy 상태 표시
                if (isGroggy)
                {
                    description = "[GROGGY] " + description;
                }
                descriptionText.text = description;
            }

            // TRIAD: Break 게이지 업데이트
            if (breakGaugeUI != null)
            {
                breakGaugeUI.RefreshDisplay();
            }
        }

        /// <summary>
        /// TRIAD: Groggy 표시 업데이트
        /// TRIAD: Update groggy display
        /// </summary>
        private void UpdateGroggyDisplay(bool isGroggy)
        {
            if (groggyOverlay != null)
            {
                groggyOverlay.SetActive(isGroggy);
            }

            if (groggyText != null && isGroggy && enemy.BreakGauge != null)
            {
                groggyText.text = $"GROGGY ({enemy.BreakGauge.GroggyTurnsLeft}T)";
            }
        }

        private Color GetIntentColor(EnemyIntentType intentType)
        {
            return intentType switch
            {
                EnemyIntentType.Attack => attackColor,
                EnemyIntentType.Defend => defendColor,
                EnemyIntentType.AttackBuff => Color.Lerp(attackColor, buffColor, 0.5f),
                EnemyIntentType.AttackDebuff => Color.Lerp(attackColor, debuffColor, 0.5f),
                EnemyIntentType.Buff => buffColor,
                EnemyIntentType.Debuff => debuffColor,
                EnemyIntentType.Unknown => unknownColor,
                _ => unknownColor
            };
        }

        private string GetIntentValue(EnemyAction intent)
        {
            switch (intent.intentType)
            {
                case EnemyIntentType.Attack:
                case EnemyIntentType.AttackBuff:
                case EnemyIntentType.AttackDebuff:
                    // TRIAD: PartyManager의 Active 캐릭터 타겟으로 계산
                    // Calculate damage against Active party member
                    ICombatEntity target = PartyManager.Instance?.Active;
                    if (target == null)
                    {
                        // 폴백: Legacy PlayerCombat
                        target = PlayerCombat.Instance;
                    }

                    if (target == null)
                    {
                        return intent.damage.ToString();
                    }

                    int damage = DamageCalculator.CalculateDamage(
                        intent.damage,
                        enemy,
                        target
                    );
                    if (intent.hitCount > 1)
                        return $"{damage}x{intent.hitCount}";
                    return damage.ToString();

                case EnemyIntentType.Defend:
                    return intent.block.ToString();

                default:
                    return "";
            }
        }

        /// <summary>
        /// 인텐트 심볼 반환
        /// Get intent symbol
        /// </summary>
        public static string GetIntentSymbol(EnemyIntentType intentType)
        {
            return intentType switch
            {
                EnemyIntentType.Attack => "⚔",
                EnemyIntentType.Defend => "🛡",
                EnemyIntentType.AttackBuff => "⚔↑",
                EnemyIntentType.AttackDebuff => "⚔↓",
                EnemyIntentType.Buff => "↑",
                EnemyIntentType.Debuff => "↓",
                EnemyIntentType.Unknown => "?",
                _ => "?"
            };
        }

        #region TRIAD Event Handlers

        private void OnEnemyBroken(EnemyBrokenEvent evt)
        {
            if (enemy != null && evt.Enemy == enemy)
            {
                UpdateIntent();

                // Break 효과 재생
                if (breakGaugeUI != null)
                {
                    breakGaugeUI.PlayBreakEffect();
                }
            }
        }

        private void OnEnemyGroggyEnded(EnemyGroggyEndedEvent evt)
        {
            if (enemy != null && evt.Enemy == enemy)
            {
                UpdateIntent();
            }
        }

        #endregion
    }
}
