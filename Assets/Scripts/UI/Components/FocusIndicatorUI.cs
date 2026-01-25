using UnityEngine;
using UnityEngine.UI;

namespace ProjectSS.UI
{
    /// <summary>
    /// Focus 스택 시각화 UI
    /// Focus stack visualization UI
    ///
    /// TRIAD: Focus 시스템을 위한 3개 오브 형태의 인디케이터
    /// 3-orb indicator for Focus system
    /// </summary>
    public class FocusIndicatorUI : MonoBehaviour
    {
        [Header("Focus Orbs")]
        [Tooltip("Focus 오브 이미지들 (최대 3개) / Focus orb images (max 3)")]
        [SerializeField] private Image[] focusOrbs = new Image[3];

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color filledColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color maxColor = new Color(1f, 0.5f, 0.1f, 1f);

        [Header("Animation")]
        [SerializeField] private bool useGlow = true;
        [SerializeField] private float glowIntensity = 1.2f;
        [SerializeField] private float glowSpeed = 2f;

        private int currentFocus;
        private int maxFocus = 3;

        /// <summary>
        /// 현재 Focus 스택
        /// Current Focus stacks
        /// </summary>
        public int CurrentFocus => currentFocus;

        private void Update()
        {
            if (useGlow && currentFocus >= maxFocus)
            {
                AnimateMaxFocus();
            }
        }

        /// <summary>
        /// Focus 업데이트
        /// Update Focus display
        /// </summary>
        /// <param name="focusStacks">Focus 스택 수 / Focus stack count</param>
        public void UpdateFocus(int focusStacks)
        {
            currentFocus = Mathf.Clamp(focusStacks, 0, maxFocus);
            RefreshOrbs();
        }

        /// <summary>
        /// 최대 Focus 설정
        /// Set maximum Focus
        /// </summary>
        /// <param name="max">최대값 / Maximum value</param>
        public void SetMaxFocus(int max)
        {
            maxFocus = Mathf.Max(1, max);

            // 오브 개수 조정
            // Adjust orb count
            for (int i = 0; i < focusOrbs.Length; i++)
            {
                if (focusOrbs[i] != null)
                {
                    focusOrbs[i].gameObject.SetActive(i < maxFocus);
                }
            }

            RefreshOrbs();
        }

        /// <summary>
        /// 오브 상태 새로고침
        /// Refresh orb states
        /// </summary>
        private void RefreshOrbs()
        {
            for (int i = 0; i < focusOrbs.Length; i++)
            {
                if (focusOrbs[i] == null) continue;

                if (i < currentFocus)
                {
                    // 채워진 오브
                    // Filled orb
                    focusOrbs[i].color = (currentFocus >= maxFocus) ? maxColor : filledColor;
                }
                else
                {
                    // 비어있는 오브
                    // Empty orb
                    focusOrbs[i].color = emptyColor;
                }
            }
        }

        /// <summary>
        /// 최대 Focus 도달 시 글로우 애니메이션
        /// Glow animation when max Focus reached
        /// </summary>
        private void AnimateMaxFocus()
        {
            float glow = 1f + Mathf.Sin(Time.time * glowSpeed) * (glowIntensity - 1f) * 0.5f;

            for (int i = 0; i < focusOrbs.Length && i < currentFocus; i++)
            {
                if (focusOrbs[i] != null)
                {
                    Color baseColor = maxColor;
                    focusOrbs[i].color = new Color(
                        Mathf.Min(baseColor.r * glow, 1f),
                        Mathf.Min(baseColor.g * glow, 1f),
                        Mathf.Min(baseColor.b * glow, 1f),
                        baseColor.a
                    );
                }
            }
        }

        /// <summary>
        /// Focus 획득 시 펄스 효과
        /// Pulse effect on Focus gain
        /// </summary>
        public void PlayGainEffect()
        {
            StartCoroutine(GainEffectCoroutine());
        }

        private System.Collections.IEnumerator GainEffectCoroutine()
        {
            // 잠깐 밝게 번쩍임
            // Brief bright flash
            for (int i = 0; i < focusOrbs.Length && i < currentFocus; i++)
            {
                if (focusOrbs[i] != null)
                {
                    focusOrbs[i].color = Color.white;
                }
            }

            yield return new WaitForSeconds(0.1f);

            RefreshOrbs();
        }

        /// <summary>
        /// Focus 소모 시 효과
        /// Effect on Focus consume
        /// </summary>
        public void PlayConsumeEffect()
        {
            StartCoroutine(ConsumeEffectCoroutine());
        }

        private System.Collections.IEnumerator ConsumeEffectCoroutine()
        {
            // 어두워지는 효과
            // Darkening effect
            for (int i = 0; i < focusOrbs.Length; i++)
            {
                if (focusOrbs[i] != null)
                {
                    focusOrbs[i].color = new Color(0.5f, 0.4f, 0.1f, 0.8f);
                }
            }

            yield return new WaitForSeconds(0.15f);

            RefreshOrbs();
        }

        /// <summary>
        /// Focus 상태 툴팁 텍스트
        /// Get tooltip text for Focus status
        /// </summary>
        public string GetTooltipText()
        {
            if (currentFocus >= maxFocus)
            {
                return $"Focus: {currentFocus}/{maxFocus} (MAX)\n무료 Tag-In 가능!\nFree Tag-In available!";
            }
            else if (currentFocus > 0)
            {
                return $"Focus: {currentFocus}/{maxFocus}\n데미지/블록 +{currentFocus * 25}%\nDamage/Block +{currentFocus * 25}%";
            }
            else
            {
                return $"Focus: 0/{maxFocus}\n후열 대기 시 턴당 +1\n+1 per turn while in Standby";
            }
        }
    }
}
