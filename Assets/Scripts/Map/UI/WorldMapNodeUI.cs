// Map/UI/WorldMapNodeUI.cs
// 월드맵 노드 UI 컴포넌트

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using ProjectSS.Core;
using ProjectSS.Core.Events;
using ProjectSS.Map.Data;

namespace ProjectSS.Map.UI
{
    /// <summary>
    /// 개별 노드 UI 컴포넌트
    /// </summary>
    public class WorldMapNodeUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private Image _playerMarker;
        [SerializeField] private Image _clearedOverlay;
        [SerializeField] private Image _accessibleGlow;
        [SerializeField] private Button _button;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation")]
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseMinScale = 0.95f;
        [SerializeField] private float _pulseMaxScale = 1.05f;

        // 노드 데이터
        private WorldMapNode _nodeData;
        private WorldMapVisualConfig _config;
        private string _nodeId;

        // 상태
        private bool _isAccessible;
        private bool _isCleared;
        private bool _isPlayerHere;
        private bool _isHovered;

        #region Properties

        public string NodeId => _nodeId;
        public WorldMapNode NodeData => _nodeData;
        public bool IsAccessible => _isAccessible;
        public bool IsCleared => _isCleared;
        public bool IsPlayerHere => _isPlayerHere;

        #endregion

        #region Initialization

        /// <summary>
        /// 노드 초기화
        /// </summary>
        public void Initialize(WorldMapNode node, WorldMapVisualConfig config)
        {
            _nodeData = node;
            _config = config;
            _nodeId = node.NodeId;

            _isAccessible = node.IsAccessible;
            _isCleared = node.IsCleared;
            _isPlayerHere = false;

            UpdateVisuals();
        }

        #endregion

        #region Unity Lifecycle

        private void Update()
        {
            // 접근 가능 노드 펄스 애니메이션
            if (_isAccessible && !_isCleared && !_isPlayerHere)
            {
                float scale = Mathf.Lerp(_pulseMinScale, _pulseMaxScale,
                    (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f);
                transform.localScale = Vector3.one * scale;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }

        #endregion

        #region Visual Updates

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        public void UpdateVisuals()
        {
            if (_nodeData == null) return;

            // 배경 색상
            if (_backgroundImage != null && _config != null)
            {
                _backgroundImage.color = _config.GetBackgroundColor(_nodeData.RegionType);
            }

            // 아이콘
            if (_iconImage != null && _config != null)
            {
                var icon = _config.GetIcon(_nodeData.RegionType);
                _iconImage.sprite = icon;
                _iconImage.gameObject.SetActive(icon != null);
            }

            // 이름 텍스트
            if (_nameText != null)
            {
                _nameText.text = _nodeData.DisplayName;
            }

            // 플레이어 마커
            if (_playerMarker != null)
            {
                _playerMarker.gameObject.SetActive(_isPlayerHere);
            }

            // 클리어 오버레이
            if (_clearedOverlay != null)
            {
                _clearedOverlay.gameObject.SetActive(_isCleared);
                if (_isCleared && _config != null)
                {
                    _clearedOverlay.color = _config.ClearedOverlayColor;
                }
            }

            // 접근 가능 글로우
            if (_accessibleGlow != null)
            {
                _accessibleGlow.gameObject.SetActive(_isAccessible && !_isCleared && !_isPlayerHere);
                if (_isAccessible && _config != null)
                {
                    _accessibleGlow.color = _config.AccessibleGlowColor;
                }
            }

            // 캔버스 그룹 알파
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = (_isAccessible || _isCleared || _isPlayerHere) ? 1f : 0.5f;
            }

            // 버튼 상호작용
            if (_button != null)
            {
                _button.interactable = _isAccessible && !_isCleared;
            }
        }

        /// <summary>
        /// 플레이어 위치 설정
        /// </summary>
        public void SetPlayerHere(bool isHere)
        {
            _isPlayerHere = isHere;
            UpdateVisuals();
        }

        /// <summary>
        /// 클리어 상태 설정
        /// </summary>
        public void SetCleared(bool cleared)
        {
            _isCleared = cleared;
            UpdateVisuals();
        }

        /// <summary>
        /// 접근 가능 상태 설정
        /// </summary>
        public void SetAccessible(bool accessible)
        {
            _isAccessible = accessible;
            UpdateVisuals();
        }

        #endregion

        #region Pointer Events

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_isAccessible || _isCleared) return;

            Debug.Log($"[WorldMapNodeUI] Node clicked: {_nodeId}");

            EventBus.Publish(new MapNodeSelectedEvent
            {
                NodeId = _nodeId,
                RegionType = _nodeData.RegionType
            });
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;

            EventBus.Publish(new MapNodeHoverEvent
            {
                NodeId = _nodeId,
                RegionType = _nodeData.RegionType,
                DisplayName = _nodeData.DisplayName,
                IsHovering = true
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;

            EventBus.Publish(new MapNodeHoverEvent
            {
                NodeId = _nodeId,
                RegionType = _nodeData.RegionType,
                DisplayName = _nodeData.DisplayName,
                IsHovering = false
            });
        }

        #endregion
    }
}
