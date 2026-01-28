// Combat/UI/EntityStatusUI.cs
// 전투 엔티티 상태 UI (HP바, 블록, 상태효과)

using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core.Events;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 전투 엔티티 상태 UI
    /// HP바, 블록, 상태효과 표시
    /// </summary>
    public class EntityStatusUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text _nameText;
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Text _hpText;
        [SerializeField] private Text _blockText;
        [SerializeField] private GameObject _blockIcon;
        [SerializeField] private Image _backgroundImage;

        [Header("Intent (Enemy Only)")]
        [SerializeField] private GameObject _intentDisplay;
        [SerializeField] private Text _intentText;
        [SerializeField] private Image _intentIcon;

        [Header("Visual")]
        [SerializeField] private Image _portraitImage;

        [Header("Colors")]
        [SerializeField] private Color _playerColor = new Color(0.2f, 0.5f, 0.3f, 0.8f);
        [SerializeField] private Color _enemyColor = new Color(0.5f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color _hpBarColor = new Color(0.8f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color _blockColor = new Color(0.3f, 0.5f, 0.8f, 1f);

        // 참조
        private ICombatEntity _entity;
        private bool _isEnemy;

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #region Initialization

        /// <summary>
        /// 엔티티로 초기화
        /// </summary>
        public void Initialize(ICombatEntity entity, bool isEnemy)
        {
            _entity = entity;
            _isEnemy = isEnemy;

            UpdateAllUI();
            SubscribeToEvents();

            // 적 인텐트 표시
            if (_intentDisplay != null)
            {
                _intentDisplay.SetActive(isEnemy);
            }

            if (isEnemy && entity is EnemyCombat enemy)
            {
                UpdateIntentDisplay(enemy);
            }
        }

        /// <summary>
        /// 동적으로 간단한 UI 생성
        /// </summary>
        public void CreateSimpleUI(ICombatEntity entity, bool isEnemy)
        {
            _entity = entity;
            _isEnemy = isEnemy;

            // 배경
            _backgroundImage = gameObject.AddComponent<Image>();
            _backgroundImage.color = isEnemy ? _enemyColor : _playerColor;

            // 이름 텍스트 (상단)
            var nameGo = new GameObject("NameText");
            nameGo.transform.SetParent(transform, false);
            var nameRect = nameGo.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.88f);
            nameRect.anchorMax = new Vector2(1, 1f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            _nameText = nameGo.AddComponent<Text>();
            _nameText.text = entity.DisplayName;
            _nameText.alignment = TextAnchor.MiddleCenter;
            _nameText.fontSize = 14;
            _nameText.color = Color.white;
            _nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 초상화 이미지 (이름 아래)
            var portraitGo = new GameObject("PortraitImage");
            portraitGo.transform.SetParent(transform, false);
            var portraitRect = portraitGo.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.1f, 0.45f);
            portraitRect.anchorMax = new Vector2(0.9f, 0.88f);
            portraitRect.offsetMin = Vector2.zero;
            portraitRect.offsetMax = Vector2.zero;
            _portraitImage = portraitGo.AddComponent<Image>();
            _portraitImage.preserveAspect = true;

            // 적/파티원에 따른 초상화 설정
            if (isEnemy && entity is EnemyCombat enemy)
            {
                if (enemy.Portrait != null)
                {
                    _portraitImage.sprite = enemy.Portrait;
                    _portraitImage.color = Color.white;
                }
                else
                {
                    // 플레이스홀더 색상 (적)
                    _portraitImage.color = new Color(0.6f, 0.3f, 0.3f, 0.8f);
                }
            }
            else
            {
                // 플레이스홀더 색상 (파티원)
                _portraitImage.color = new Color(0.3f, 0.5f, 0.4f, 0.8f);
            }

            // HP 슬라이더 (초상화 아래)
            CreateHPSlider();

            // HP 텍스트
            var hpTextGo = new GameObject("HPText");
            hpTextGo.transform.SetParent(transform, false);
            var hpTextRect = hpTextGo.AddComponent<RectTransform>();
            hpTextRect.anchorMin = new Vector2(0, 0.22f);
            hpTextRect.anchorMax = new Vector2(1, 0.35f);
            hpTextRect.offsetMin = Vector2.zero;
            hpTextRect.offsetMax = Vector2.zero;
            _hpText = hpTextGo.AddComponent<Text>();
            _hpText.alignment = TextAnchor.MiddleCenter;
            _hpText.fontSize = 12;
            _hpText.color = Color.white;
            _hpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 블록 텍스트
            var blockGo = new GameObject("BlockText");
            blockGo.transform.SetParent(transform, false);
            var blockRect = blockGo.AddComponent<RectTransform>();
            blockRect.anchorMin = new Vector2(0, 0.11f);
            blockRect.anchorMax = new Vector2(1, 0.22f);
            blockRect.offsetMin = Vector2.zero;
            blockRect.offsetMax = Vector2.zero;
            _blockText = blockGo.AddComponent<Text>();
            _blockText.alignment = TextAnchor.MiddleCenter;
            _blockText.fontSize = 12;
            _blockText.color = _blockColor;
            _blockText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 적 인텐트 표시
            if (isEnemy)
            {
                CreateIntentDisplay();
            }

            UpdateAllUI();
            SubscribeToEvents();

            if (isEnemy && entity is EnemyCombat enemy2)
            {
                UpdateIntentDisplay(enemy2);
            }
        }

        private void CreateHPSlider()
        {
            var sliderGo = new GameObject("HPSlider");
            sliderGo.transform.SetParent(transform, false);
            var sliderRect = sliderGo.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.1f, 0.35f);
            sliderRect.anchorMax = new Vector2(0.9f, 0.45f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;

            // Background
            var bgGo = new GameObject("Background");
            bgGo.transform.SetParent(sliderGo.transform, false);
            var bgRect = bgGo.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Fill Area
            var fillAreaGo = new GameObject("Fill Area");
            fillAreaGo.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillAreaGo.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;

            // Fill
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(fillAreaGo.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillGo.AddComponent<Image>();
            fillImage.color = _hpBarColor;

            _hpSlider = sliderGo.AddComponent<Slider>();
            _hpSlider.fillRect = fillRect;
            _hpSlider.interactable = false;
            _hpSlider.transition = Selectable.Transition.None;
        }

        private void CreateIntentDisplay()
        {
            _intentDisplay = new GameObject("IntentDisplay");
            _intentDisplay.transform.SetParent(transform, false);
            var intentRect = _intentDisplay.AddComponent<RectTransform>();
            intentRect.anchorMin = new Vector2(0, 0);
            intentRect.anchorMax = new Vector2(1, 0.11f);
            intentRect.offsetMin = Vector2.zero;
            intentRect.offsetMax = Vector2.zero;

            _intentText = _intentDisplay.AddComponent<Text>();
            _intentText.alignment = TextAnchor.MiddleCenter;
            _intentText.fontSize = 11;
            _intentText.color = Color.yellow;
            _intentText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            EventBus.Subscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Subscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Subscribe<TurnEndedEvent>(OnTurnEnded);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Unsubscribe<DamageDealtEvent>(OnDamageDealt);
            EventBus.Unsubscribe<BlockGainedEvent>(OnBlockGained);
            EventBus.Unsubscribe<TurnEndedEvent>(OnTurnEnded);
        }

        #endregion

        #region Event Handlers

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            if (_entity == null) return;

            if (evt.TargetId == _entity.EntityId)
            {
                UpdateHPDisplay();
                // TODO: 데미지 숫자 애니메이션
            }
        }

        private void OnBlockGained(BlockGainedEvent evt)
        {
            if (_entity == null) return;

            if (evt.EntityId == _entity.EntityId)
            {
                UpdateBlockDisplay();
            }
        }

        private void OnTurnEnded(TurnEndedEvent evt)
        {
            if (_entity == null || !_isEnemy) return;

            // 적의 턴이 끝나면 인텐트 업데이트
            if (evt.EntityId == _entity.EntityId && _entity is EnemyCombat enemy)
            {
                UpdateIntentDisplay(enemy);
            }
        }

        #endregion

        #region UI Updates

        private void UpdateAllUI()
        {
            if (_entity == null) return;

            if (_nameText != null)
            {
                _nameText.text = _entity.DisplayName;
            }

            UpdateHPDisplay();
            UpdateBlockDisplay();
        }

        private void UpdateHPDisplay()
        {
            if (_entity == null) return;

            if (_hpSlider != null)
            {
                _hpSlider.maxValue = _entity.MaxHP;
                _hpSlider.value = _entity.CurrentHP;
            }

            if (_hpText != null)
            {
                _hpText.text = $"{_entity.CurrentHP}/{_entity.MaxHP}";
            }
        }

        private void UpdateBlockDisplay()
        {
            if (_entity == null) return;

            int block = _entity.CurrentBlock;

            if (_blockText != null)
            {
                _blockText.text = block > 0 ? $"방어: {block}" : "";
            }

            if (_blockIcon != null)
            {
                _blockIcon.SetActive(block > 0);
            }
        }

        private void UpdateIntentDisplay(EnemyCombat enemy)
        {
            if (_intentText == null || enemy.CurrentIntent == null) return;

            var intent = enemy.CurrentIntent;
            string intentStr = intent.Type switch
            {
                EnemyIntentType.Attack => $"공격 {intent.Value}",
                EnemyIntentType.Defend => $"방어 {intent.Value}",
                EnemyIntentType.Buff => "버프",
                EnemyIntentType.Debuff => "디버프",
                _ => "???"
            };

            _intentText.text = intentStr;

            // 색상 변경
            _intentText.color = intent.Type switch
            {
                EnemyIntentType.Attack => Color.red,
                EnemyIntentType.Defend => Color.cyan,
                _ => Color.yellow
            };
        }

        #endregion

        private void Update()
        {
            // 실시간 HP 동기화 (이벤트 누락 방지)
            if (_entity != null)
            {
                UpdateHPDisplay();
                UpdateBlockDisplay();
            }
        }
    }
}
