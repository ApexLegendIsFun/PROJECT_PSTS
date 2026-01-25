using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectSS.Data;
using ProjectSS.Run;

namespace ProjectSS.UI
{
    /// <summary>
    /// 3인 파티 HP 상태 표시 (공용 컴포넌트)
    /// Party status display for 3 characters (shared component)
    /// 사용 씬: Rest, Event, Shop
    /// </summary>
    public class PartyStatusDisplay : MonoBehaviour
    {
        [Header("Character Displays")]
        [SerializeField] private CharacterStatusSlot warriorSlot;
        [SerializeField] private CharacterStatusSlot mageSlot;
        [SerializeField] private CharacterStatusSlot rogueSlot;

        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI goldText;

        /// <summary>
        /// 파티 상태 새로고침
        /// Refresh party status
        /// </summary>
        public void Refresh()
        {
            if (RunManager.Instance == null || !RunManager.Instance.IsPartyMode)
            {
                Debug.LogWarning("[PartyStatusDisplay] RunManager not available or not in party mode");
                return;
            }

            var party = RunManager.Instance.PartyState;

            if (warriorSlot != null)
                warriorSlot.UpdateStatus(party.warrior, "전사");

            if (mageSlot != null)
                mageSlot.UpdateStatus(party.mage, "마법사");

            if (rogueSlot != null)
                rogueSlot.UpdateStatus(party.rogue, "도적");

            UpdateGold();
        }

        /// <summary>
        /// 골드 표시 업데이트
        /// Update gold display
        /// </summary>
        public void UpdateGold()
        {
            if (goldText != null && RunManager.Instance != null)
            {
                int gold = RunManager.Instance.CurrentRun?.gold ?? 0;
                goldText.text = $"{gold}G";
            }
        }

        /// <summary>
        /// 힐 프리뷰 표시 (휴식 씬용)
        /// Show heal preview (for Rest scene)
        /// </summary>
        public void ShowHealPreview(float healPercent)
        {
            if (RunManager.Instance == null || !RunManager.Instance.IsPartyMode)
                return;

            var party = RunManager.Instance.PartyState;

            if (warriorSlot != null)
                warriorSlot.ShowHealPreview(party.warrior, healPercent);

            if (mageSlot != null)
                mageSlot.ShowHealPreview(party.mage, healPercent);

            if (rogueSlot != null)
                rogueSlot.ShowHealPreview(party.rogue, healPercent);
        }

        /// <summary>
        /// 힐 프리뷰 숨기기
        /// Hide heal preview
        /// </summary>
        public void HideHealPreview()
        {
            if (warriorSlot != null)
                warriorSlot.HideHealPreview();

            if (mageSlot != null)
                mageSlot.HideHealPreview();

            if (rogueSlot != null)
                rogueSlot.HideHealPreview();
        }

        /// <summary>
        /// 총 체력 반환
        /// Get total HP
        /// </summary>
        public int GetTotalCurrentHP()
        {
            if (RunManager.Instance == null || !RunManager.Instance.IsPartyMode)
                return 0;

            var party = RunManager.Instance.PartyState;
            return party.warrior.currentHP + party.mage.currentHP + party.rogue.currentHP;
        }

        /// <summary>
        /// 총 최대 체력 반환
        /// Get total max HP
        /// </summary>
        public int GetTotalMaxHP()
        {
            if (RunManager.Instance == null || !RunManager.Instance.IsPartyMode)
                return 0;

            var party = RunManager.Instance.PartyState;
            return party.warrior.maxHP + party.mage.maxHP + party.rogue.maxHP;
        }
    }

    /// <summary>
    /// 개별 캐릭터 상태 슬롯
    /// Individual character status slot
    /// </summary>
    [System.Serializable]
    public class CharacterStatusSlot
    {
        [Header("References")]
        public GameObject container;
        public TextMeshProUGUI nameText;
        public Slider healthSlider;
        public Image healthFill;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI healPreviewText;

        [Header("Colors")]
        public Color healthyColor = new Color(0.2f, 0.8f, 0.2f);
        public Color damagedColor = new Color(0.9f, 0.6f, 0.1f);
        public Color criticalColor = new Color(0.9f, 0.2f, 0.2f);

        /// <summary>
        /// 상태 업데이트
        /// Update status
        /// </summary>
        public void UpdateStatus(CharacterState character, string displayName)
        {
            if (container != null)
                container.SetActive(true);

            if (nameText != null)
                nameText.text = displayName;

            if (healthSlider != null)
            {
                healthSlider.maxValue = character.maxHP;
                healthSlider.value = character.currentHP;
            }

            if (healthText != null)
                healthText.text = $"{character.currentHP}/{character.maxHP}";

            UpdateHealthColor(character);

            if (healPreviewText != null)
                healPreviewText.gameObject.SetActive(false);
        }

        /// <summary>
        /// 체력 색상 업데이트
        /// Update health color
        /// </summary>
        private void UpdateHealthColor(CharacterState character)
        {
            if (healthFill == null) return;

            float ratio = (float)character.currentHP / character.maxHP;

            if (ratio > 0.5f)
                healthFill.color = healthyColor;
            else if (ratio > 0.25f)
                healthFill.color = damagedColor;
            else
                healthFill.color = criticalColor;
        }

        /// <summary>
        /// 힐 프리뷰 표시
        /// Show heal preview
        /// </summary>
        public void ShowHealPreview(CharacterState character, float healPercent)
        {
            if (healPreviewText == null) return;

            int healAmount = Mathf.RoundToInt(character.maxHP * healPercent);
            int actualHeal = Mathf.Min(healAmount, character.maxHP - character.currentHP);

            if (actualHeal > 0)
            {
                healPreviewText.gameObject.SetActive(true);
                healPreviewText.text = $"+{actualHeal}";
                healPreviewText.color = healthyColor;
            }
            else
            {
                healPreviewText.gameObject.SetActive(true);
                healPreviewText.text = "FULL";
                healPreviewText.color = Color.gray;
            }
        }

        /// <summary>
        /// 힐 프리뷰 숨기기
        /// Hide heal preview
        /// </summary>
        public void HideHealPreview()
        {
            if (healPreviewText != null)
                healPreviewText.gameObject.SetActive(false);
        }
    }
}
