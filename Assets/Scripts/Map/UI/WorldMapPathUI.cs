// Map/UI/WorldMapPathUI.cs
// 월드맵 경로 UI 컴포넌트

using UnityEngine;
using UnityEngine.UI;

namespace ProjectSS.Map.UI
{
    /// <summary>
    /// 노드 연결 경로 UI
    /// </summary>
    public class WorldMapPathUI : MonoBehaviour
    {
        [Header("Line Settings")]
        [SerializeField] private Image _lineImage;
        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private float _lineWidth = 4f;

        [Header("State")]
        [SerializeField] private bool _isTraversed;

        // 연결 정보
        private string _fromNodeId;
        private string _toNodeId;
        private WorldMapVisualConfig _config;

        #region Properties

        public string FromNodeId => _fromNodeId;
        public string ToNodeId => _toNodeId;
        public bool IsTraversed => _isTraversed;

        #endregion

        #region Initialization

        /// <summary>
        /// 경로 초기화
        /// </summary>
        public void Initialize(string fromNodeId, string toNodeId, Vector2 fromPos, Vector2 toPos, WorldMapVisualConfig config)
        {
            _fromNodeId = fromNodeId;
            _toNodeId = toNodeId;
            _config = config;
            _isTraversed = false;

            SetupLine(fromPos, toPos);
            UpdateVisuals();
        }

        /// <summary>
        /// 라인 설정
        /// </summary>
        private void SetupLine(Vector2 fromPos, Vector2 toPos)
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            // 두 점 사이의 거리와 각도 계산
            Vector2 direction = toPos - fromPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 중간 지점에 위치
            Vector2 midPoint = (fromPos + toPos) / 2f;
            _rectTransform.anchoredPosition = midPoint;

            // 크기 설정 (길이 x 두께)
            _rectTransform.sizeDelta = new Vector2(distance, _lineWidth);

            // 회전
            _rectTransform.rotation = Quaternion.Euler(0, 0, angle);

            // 피벗 중앙
            _rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        #endregion

        #region Visual Updates

        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        public void UpdateVisuals()
        {
            if (_lineImage == null) return;

            if (_config != null)
            {
                _lineImage.color = _isTraversed ? _config.PathTraversedColor : _config.PathDefaultColor;
            }
        }

        /// <summary>
        /// 이동 완료 상태 설정
        /// </summary>
        public void SetTraversed(bool traversed)
        {
            _isTraversed = traversed;
            UpdateVisuals();
        }

        #endregion
    }
}
