using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectSS.UI
{
    /// <summary>
    /// 체력바 UI
    /// Health bar UI
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image healthFill;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Block Display")]
        [SerializeField] private GameObject blockContainer;
        [SerializeField] private TextMeshProUGUI blockText;
        [SerializeField] private Image blockIcon;

        [Header("Colors")]
        [SerializeField] private Color healthyColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color damagedColor = new Color(0.9f, 0.6f, 0.1f);
        [SerializeField] private Color criticalColor = new Color(0.9f, 0.2f, 0.2f);
        [SerializeField] private Color blockColor = new Color(0.4f, 0.6f, 0.9f);

        private int currentHealth;
        private int maxHealth;
        private int currentBlock;

        /// <summary>
        /// 체력 업데이트
        /// Update health display
        /// </summary>
        public void UpdateHealth(int current, int max, int block = 0)
        {
            currentHealth = current;
            maxHealth = max;
            currentBlock = block;

            UpdateHealthBar();
            UpdateBlockDisplay();
        }

        private void UpdateHealthBar()
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentHealth}/{maxHealth}";
            }

            if (healthFill != null)
            {
                float ratio = (float)currentHealth / maxHealth;
                if (ratio > 0.5f)
                    healthFill.color = healthyColor;
                else if (ratio > 0.25f)
                    healthFill.color = damagedColor;
                else
                    healthFill.color = criticalColor;
            }
        }

        private void UpdateBlockDisplay()
        {
            if (blockContainer != null)
            {
                blockContainer.SetActive(currentBlock > 0);
            }

            if (blockText != null)
            {
                blockText.text = currentBlock.ToString();
            }
        }

        /// <summary>
        /// 데미지 플래시 효과
        /// Flash effect on damage
        /// </summary>
        public void FlashDamage()
        {
            // TODO: DOTween 또는 코루틴으로 플래시 효과 구현
            if (healthFill != null)
            {
                StartCoroutine(DamageFlashCoroutine());
            }
        }

        private System.Collections.IEnumerator DamageFlashCoroutine()
        {
            var originalColor = healthFill.color;
            healthFill.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            healthFill.color = originalColor;
        }
    }
}
