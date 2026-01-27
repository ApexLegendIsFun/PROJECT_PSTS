// Combat/UI/TargetingSystem.cs
// 타겟팅 시스템 - 카드 드래그 시 유효 타겟 선택

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectSS.Core;

namespace ProjectSS.Combat.UI
{
    /// <summary>
    /// 타겟팅 시스템
    /// 카드 드래그 시 유효한 타겟을 하이라이트하고 선택 처리
    /// </summary>
    public class TargetingSystem : MonoBehaviour
    {
        public static TargetingSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Color _validTargetColor = new Color(0.3f, 0.8f, 0.3f, 0.5f);
        [SerializeField] private Color _invalidTargetColor = new Color(0.8f, 0.3f, 0.3f, 0.3f);
        [SerializeField] private LayerMask _targetLayerMask;

        [Header("Targeting Line (Optional)")]
        [SerializeField] private LineRenderer _targetingLine;
        [SerializeField] private Color _lineColor = Color.yellow;

        // 현재 타겟팅 상태
        private bool _isTargeting;
        private CardInstance _currentCard;
        private List<EntityStatusUI> _validTargets = new();
        private Dictionary<EntityStatusUI, Color> _originalColors = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        #region Public Methods

        /// <summary>
        /// 타겟팅 시작
        /// </summary>
        public void StartTargeting(CardInstance card)
        {
            if (card == null) return;

            _isTargeting = true;
            _currentCard = card;

            // 유효 타겟 찾기 및 하이라이트
            FindValidTargets(card.TargetType);
            HighlightValidTargets();

            Debug.Log($"[TargetingSystem] Started targeting for: {card.CardName} (Type: {card.TargetType})");
        }

        /// <summary>
        /// 타겟팅 종료
        /// </summary>
        public void EndTargeting()
        {
            if (!_isTargeting) return;

            _isTargeting = false;
            _currentCard = null;

            // 하이라이트 제거
            ClearHighlights();
            _validTargets.Clear();

            // 타겟팅 라인 숨기기
            if (_targetingLine != null)
            {
                _targetingLine.enabled = false;
            }
        }

        /// <summary>
        /// 포인터 위치의 타겟 가져오기
        /// </summary>
        public ICombatEntity GetTargetUnderPointer(PointerEventData eventData)
        {
            if (!_isTargeting) return null;

            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (var result in results)
            {
                var statusUI = result.gameObject.GetComponentInParent<EntityStatusUI>();
                if (statusUI != null && _validTargets.Contains(statusUI))
                {
                    return GetEntityFromStatusUI(statusUI);
                }
            }

            // Self 타겟일 경우 자동 타겟
            if (_currentCard != null && _currentCard.TargetType == TargetType.Self)
            {
                return GetCurrentTurnEntity();
            }

            return null;
        }

        /// <summary>
        /// 타겟팅 중인지 확인
        /// </summary>
        public bool IsTargeting => _isTargeting;

        #endregion

        #region Target Finding

        private void FindValidTargets(TargetType targetType)
        {
            _validTargets.Clear();

            if (CombatManager.Instance == null) return;

            switch (targetType)
            {
                case TargetType.Self:
                    // Self는 자동 타겟이므로 하이라이트 없음
                    break;

                case TargetType.SingleEnemy:
                    AddEnemyTargets();
                    break;

                case TargetType.AllEnemies:
                    AddEnemyTargets();
                    break;

                case TargetType.SingleAlly:
                    AddAllyTargets();
                    break;

                case TargetType.AllAllies:
                    AddAllyTargets();
                    break;

                case TargetType.Random:
                    // 랜덤은 자동이지만 적 영역 하이라이트
                    AddEnemyTargets();
                    break;
            }
        }

        private void AddEnemyTargets()
        {
            var enemyContainer = CombatUIController.Instance?.GetEnemyContainer();
            if (enemyContainer == null) return;

            foreach (Transform child in enemyContainer)
            {
                var statusUI = child.GetComponent<EntityStatusUI>();
                if (statusUI != null)
                {
                    _validTargets.Add(statusUI);
                }
            }
        }

        private void AddAllyTargets()
        {
            var partyContainer = CombatUIController.Instance?.GetPartyContainer();
            if (partyContainer == null) return;

            foreach (Transform child in partyContainer)
            {
                var statusUI = child.GetComponent<EntityStatusUI>();
                if (statusUI != null)
                {
                    _validTargets.Add(statusUI);
                }
            }
        }

        #endregion

        #region Highlighting

        private void HighlightValidTargets()
        {
            foreach (var statusUI in _validTargets)
            {
                var image = statusUI.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    // 원래 색상 저장
                    if (!_originalColors.ContainsKey(statusUI))
                    {
                        _originalColors[statusUI] = image.color;
                    }

                    // 하이라이트 색상 적용
                    image.color = Color.Lerp(image.color, _validTargetColor, 0.5f);
                }
            }
        }

        private void ClearHighlights()
        {
            foreach (var kvp in _originalColors)
            {
                if (kvp.Key != null)
                {
                    var image = kvp.Key.GetComponent<UnityEngine.UI.Image>();
                    if (image != null)
                    {
                        image.color = kvp.Value;
                    }
                }
            }
            _originalColors.Clear();
        }

        #endregion

        #region Utility

        private ICombatEntity GetEntityFromStatusUI(EntityStatusUI statusUI)
        {
            if (CombatManager.Instance == null) return null;

            // EntityStatusUI의 이름에서 엔티티 찾기 (간단한 구현)
            string name = statusUI.name.Replace("_Status", "");

            foreach (var enemy in CombatManager.Instance.Enemies)
            {
                if (statusUI.name.Contains(enemy.DisplayName))
                {
                    return enemy;
                }
            }

            foreach (var member in CombatManager.Instance.PlayerParty)
            {
                if (statusUI.name.Contains(member.DisplayName))
                {
                    return member;
                }
            }

            return null;
        }

        private ICombatEntity GetCurrentTurnEntity()
        {
            return CombatManager.Instance?.GetCurrentTurnEntity();
        }

        #endregion

        private void Update()
        {
            // 타겟팅 라인 업데이트 (선택적)
            if (_isTargeting && _targetingLine != null && _targetingLine.enabled)
            {
                // 드래그 시작 위치에서 현재 마우스 위치까지 라인
                // 구현 필요시 추가
            }
        }
    }
}
