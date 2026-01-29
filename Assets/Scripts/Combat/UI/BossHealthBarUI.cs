using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 보스 체력바 UI (보스전 전용)
    /// Boss health bar UI for boss fights only
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text _bossNameText;
        [SerializeField] private Image _healthBarBG;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private Text _healthText;
        [SerializeField] private Image _heartIcon;

        [Header("Color Settings")]
        [SerializeField] private Color _healthBarColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _healthBarBGColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        private string _bossEntityId;
        private int _currentHP;
        private int _maxHP;

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 보스 정보로 초기화
        /// </summary>
        public void Initialize(ICombatEntity bossEntity)
        {
            if (bossEntity == null) return;

            _bossEntityId = bossEntity.EntityId;
            _currentHP = bossEntity.CurrentHP;
            _maxHP = bossEntity.MaxHP;

            if (_bossNameText != null)
            {
                _bossNameText.text = bossEntity.DisplayName;
            }

            UpdateHealthDisplay();
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 보스 체력바 숨기기
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            _bossEntityId = null;
        }

        private void UpdateHealthDisplay()
        {
            if (_healthBarFill != null && _maxHP > 0)
            {
                _healthBarFill.fillAmount = (float)_currentHP / _maxHP;
            }

            if (_healthText != null)
            {
                _healthText.text = $"{_currentHP}/{_maxHP}";
            }
        }

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<HealEvent>(OnHeal);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<HealEvent>(OnHeal);
        }

        private void OnDamageDealt(DamageDealtEvent e)
        {
            if (e.TargetId != _bossEntityId) return;

            _currentHP = Mathf.Max(0, _currentHP - e.Damage);
            UpdateHealthDisplay();
        }

        private void OnHeal(HealEvent e)
        {
            if (e.EntityId != _bossEntityId) return;

            _currentHP = Mathf.Min(_maxHP, _currentHP + e.Amount);
            UpdateHealthDisplay();
        }

        /// <summary>
        /// UI가 없을 때 동적으로 생성
        /// </summary>
        public void CreateSimpleUI()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }

            // Background - check if Image already exists (from scene builder)
            var bgImage = GetComponent<Image>();
            if (bgImage == null)
            {
                bgImage = gameObject.AddComponent<Image>();
            }
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            // Boss Name Text
            var nameObj = new GameObject("BossNameText");
            nameObj.transform.SetParent(transform, false);
            _bossNameText = nameObj.AddComponent<Text>();
            _bossNameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _bossNameText.fontSize = 18;
            _bossNameText.fontStyle = FontStyle.Bold;
            _bossNameText.color = Color.white;
            _bossNameText.alignment = TextAnchor.MiddleCenter;
            _bossNameText.text = "보스";

            var nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.6f);
            nameRect.anchorMax = new Vector2(1, 0.9f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;

            // Health Bar Background
            var barBGObj = new GameObject("HealthBarBG");
            barBGObj.transform.SetParent(transform, false);
            _healthBarBG = barBGObj.AddComponent<Image>();
            _healthBarBG.color = _healthBarBGColor;

            var barBGRect = barBGObj.GetComponent<RectTransform>();
            barBGRect.anchorMin = new Vector2(0.05f, 0.15f);
            barBGRect.anchorMax = new Vector2(0.95f, 0.55f);
            barBGRect.offsetMin = Vector2.zero;
            barBGRect.offsetMax = Vector2.zero;

            // Health Bar Fill
            var barFillObj = new GameObject("HealthBarFill");
            barFillObj.transform.SetParent(barBGObj.transform, false);
            _healthBarFill = barFillObj.AddComponent<Image>();
            _healthBarFill.color = _healthBarColor;
            _healthBarFill.type = Image.Type.Filled;
            _healthBarFill.fillMethod = Image.FillMethod.Horizontal;
            _healthBarFill.fillOrigin = 0;
            _healthBarFill.fillAmount = 1f;

            var barFillRect = barFillObj.GetComponent<RectTransform>();
            barFillRect.anchorMin = Vector2.zero;
            barFillRect.anchorMax = Vector2.one;
            barFillRect.offsetMin = new Vector2(2, 2);
            barFillRect.offsetMax = new Vector2(-2, -2);

            // Heart Icon
            var heartObj = new GameObject("HeartIcon");
            heartObj.transform.SetParent(transform, false);
            _heartIcon = heartObj.AddComponent<Image>();
            _heartIcon.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark heart

            var heartRect = heartObj.GetComponent<RectTransform>();
            heartRect.anchorMin = new Vector2(0, 0.15f);
            heartRect.anchorMax = new Vector2(0.05f, 0.55f);
            heartRect.offsetMin = Vector2.zero;
            heartRect.offsetMax = Vector2.zero;

            // Health Text
            var hpTextObj = new GameObject("HealthText");
            hpTextObj.transform.SetParent(barBGObj.transform, false);
            _healthText = hpTextObj.AddComponent<Text>();
            _healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _healthText.fontSize = 14;
            _healthText.color = Color.white;
            _healthText.alignment = TextAnchor.MiddleCenter;
            _healthText.text = "100/100";

            var hpTextRect = hpTextObj.GetComponent<RectTransform>();
            hpTextRect.anchorMin = Vector2.zero;
            hpTextRect.anchorMax = Vector2.one;
            hpTextRect.offsetMin = Vector2.zero;
            hpTextRect.offsetMax = Vector2.zero;

            gameObject.SetActive(false); // Hidden by default
        }

        public bool IsActive => gameObject.activeSelf;
        public string BossEntityId => _bossEntityId;
    }
}
