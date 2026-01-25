using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ProjectSS.UI
{
    /// <summary>
    /// 에너지 UI
    /// Energy UI
    /// </summary>
    public class EnergyUI : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private Image energyOrb;
        [SerializeField] private Image energyFill;

        [Header("Colors")]
        [SerializeField] private Color fullColor = new Color(0.2f, 0.6f, 1f);
        [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.4f);
        [SerializeField] private Color glowColor = new Color(0.4f, 0.8f, 1f);

        [Header("Animation")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 0.1f;

        private int currentEnergy;
        private int maxEnergy;
        private Vector3 originalScale;

        private void Start()
        {
            originalScale = transform.localScale;
        }

        private void Update()
        {
            // 에너지가 있을 때 미세한 펄스 효과
            if (currentEnergy > 0 && energyOrb != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
                transform.localScale = originalScale * pulse;
            }
        }

        /// <summary>
        /// 에너지 업데이트
        /// Update energy display
        /// </summary>
        public void UpdateEnergy(int current, int max)
        {
            currentEnergy = current;
            maxEnergy = max;

            if (energyText != null)
            {
                energyText.text = $"{current}/{max}";
            }

            if (energyFill != null)
            {
                energyFill.fillAmount = (float)current / max;
            }

            UpdateColors();
        }

        private void UpdateColors()
        {
            if (energyOrb != null)
            {
                energyOrb.color = currentEnergy > 0 ? fullColor : emptyColor;
            }

            if (energyText != null)
            {
                energyText.color = currentEnergy > 0 ? Color.white : Color.gray;
            }
        }

        /// <summary>
        /// 에너지 사용 효과
        /// Energy spent effect
        /// </summary>
        public void PlaySpentEffect()
        {
            StartCoroutine(SpentEffectCoroutine());
        }

        private System.Collections.IEnumerator SpentEffectCoroutine()
        {
            var originalScale = transform.localScale;
            transform.localScale = originalScale * 0.9f;
            yield return new WaitForSeconds(0.1f);
            transform.localScale = originalScale;
        }

        /// <summary>
        /// 에너지 충전 효과
        /// Energy gain effect
        /// </summary>
        public void PlayGainEffect()
        {
            StartCoroutine(GainEffectCoroutine());
        }

        private System.Collections.IEnumerator GainEffectCoroutine()
        {
            var originalScale = transform.localScale;
            transform.localScale = originalScale * 1.2f;
            yield return new WaitForSeconds(0.15f);
            transform.localScale = originalScale;
        }
    }
}
