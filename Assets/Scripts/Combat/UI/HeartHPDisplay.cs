using UnityEngine;
using UnityEngine.UI;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 하트 기반 HP 표시 컴포넌트
    /// Heart-based HP display component with color changes based on HP percentage
    /// </summary>
    public class HeartHPDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _heartIcon;
        [SerializeField] private Text _hpText;

        [Header("Color Settings")]
        [SerializeField] private Color _healthyColor = new Color(1f, 0.27f, 0.27f, 1f);    // #FF4444 - 건강
        [SerializeField] private Color _cautionColor = new Color(1f, 0.53f, 0.27f, 1f);    // #FF8844 - 주의
        [SerializeField] private Color _dangerColor = new Color(1f, 0.8f, 0f, 1f);         // #FFCC00 - 위험
        [SerializeField] private Color _criticalColor = new Color(0.53f, 0f, 0f, 1f);      // #880000 - 위독
        [SerializeField] private Color _deadColor = new Color(0.27f, 0.27f, 0.27f, 1f);    // #444444 - 사망

        [Header("Display Settings")]
        [SerializeField] private bool _showNumericHP = true;

        private int _currentHP;
        private int _maxHP;

        /// <summary>
        /// HP 표시 초기화
        /// </summary>
        public void Initialize(int currentHP, int maxHP)
        {
            _currentHP = currentHP;
            _maxHP = maxHP;
            UpdateDisplay();
        }

        /// <summary>
        /// HP 업데이트
        /// </summary>
        public void UpdateHP(int currentHP, int maxHP)
        {
            _currentHP = currentHP;
            _maxHP = maxHP;
            UpdateDisplay();
        }

        /// <summary>
        /// HP만 업데이트 (최대 HP 유지)
        /// </summary>
        public void UpdateHP(int currentHP)
        {
            _currentHP = currentHP;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_heartIcon != null)
            {
                _heartIcon.color = GetColorByHPPercent();
            }

            if (_hpText != null && _showNumericHP)
            {
                _hpText.text = $"{_currentHP}/{_maxHP}";
            }
        }

        private Color GetColorByHPPercent()
        {
            if (_maxHP <= 0) return _deadColor;

            float percent = (float)_currentHP / _maxHP * 100f;

            if (_currentHP <= 0)
                return _deadColor;           // 0% - 사망
            else if (percent <= 19f)
                return _criticalColor;       // 1% ~ 19% - 위독
            else if (percent <= 39f)
                return _dangerColor;         // 20% ~ 39% - 위험
            else if (percent <= 69f)
                return _cautionColor;        // 40% ~ 69% - 주의
            else
                return _healthyColor;        // 70% ~ 100% - 건강
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

            // Heart Icon
            var heartObj = new GameObject("HeartIcon");
            heartObj.transform.SetParent(transform, false);
            _heartIcon = heartObj.AddComponent<Image>();

            var heartRect = heartObj.GetComponent<RectTransform>();
            heartRect.anchorMin = new Vector2(0, 0.3f);
            heartRect.anchorMax = new Vector2(0.35f, 1f);
            heartRect.offsetMin = Vector2.zero;
            heartRect.offsetMax = Vector2.zero;

            // Use a simple colored square as placeholder (should be replaced with heart sprite)
            _heartIcon.color = _healthyColor;

            // HP Text
            var textObj = new GameObject("HPText");
            textObj.transform.SetParent(transform, false);
            _hpText = textObj.AddComponent<Text>();
            _hpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _hpText.fontSize = 14;
            _hpText.color = Color.white;
            _hpText.alignment = TextAnchor.MiddleLeft;

            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.4f, 0);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            UpdateDisplay();
        }

        /// <summary>
        /// 숫자 HP 표시 여부 설정
        /// </summary>
        public void SetShowNumericHP(bool show)
        {
            _showNumericHP = show;
            if (_hpText != null)
            {
                _hpText.gameObject.SetActive(show);
            }
        }

        /// <summary>
        /// 현재 HP 퍼센트 반환
        /// </summary>
        public float GetHPPercent()
        {
            if (_maxHP <= 0) return 0f;
            return (float)_currentHP / _maxHP;
        }

        public int CurrentHP => _currentHP;
        public int MaxHP => _maxHP;
    }
}
