using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectSS.Core;
using ProjectSS.Map;
using ProjectSS.Run;

namespace ProjectSS.UI
{
    /// <summary>
    /// 맵 UI 관리자
    /// Map UI manager
    /// </summary>
    public class MapUI : MonoBehaviour
    {
        [Header("Node Display")]
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private GameObject nodeButtonPrefab;

        [Header("Path Display")]
        [SerializeField] private Transform pathContainer;
        [SerializeField] private GameObject pathLinePrefab;

        [Header("Camera")]
        [SerializeField] private float nodeScale = 50f;
        [SerializeField] private Vector2 offset = new Vector2(400f, 100f);

        private Dictionary<string, GameObject> nodeObjects = new Dictionary<string, GameObject>();
        private List<GameObject> pathObjects = new List<GameObject>();

        private void OnEnable()
        {
            RefreshMap();
        }

        /// <summary>
        /// 맵 새로고침
        /// Refresh map display
        /// </summary>
        public void RefreshMap()
        {
            ClearMap();

            if (MapManager.Instance?.CurrentMap == null) return;

            var mapData = MapManager.Instance.CurrentMap;

            // 노드 생성
            foreach (var node in mapData.GetAllNodes())
            {
                CreateNodeButton(node);
            }

            // 경로 생성
            CreatePaths(mapData);

            // 접근 가능한 노드 업데이트
            UpdateAccessibleNodes();
        }

        private void ClearMap()
        {
            foreach (var obj in nodeObjects.Values)
            {
                if (obj != null) Destroy(obj);
            }
            nodeObjects.Clear();

            foreach (var obj in pathObjects)
            {
                if (obj != null) Destroy(obj);
            }
            pathObjects.Clear();
        }

        private void CreateNodeButton(MapNode node)
        {
            if (nodeButtonPrefab == null || nodeContainer == null) return;

            var nodeObj = Instantiate(nodeButtonPrefab, nodeContainer);
            var rectTransform = nodeObj.GetComponent<RectTransform>();

            if (rectTransform != null)
            {
                Vector2 screenPos = node.Position * nodeScale + offset;
                rectTransform.anchoredPosition = screenPos;
            }

            // 버튼 설정
            var button = nodeObj.GetComponent<Button>();
            if (button != null)
            {
                string nodeId = node.NodeId;
                button.onClick.AddListener(() => OnNodeClicked(nodeId));
            }

            // 색상 및 텍스트 설정
            var image = nodeObj.GetComponent<Image>();
            if (image != null)
            {
                image.color = node.GetNodeColor();
            }

            var text = nodeObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = node.GetNodeSymbol();
            }

            // 방문한 노드 표시
            if (node.IsVisited)
            {
                if (image != null)
                {
                    var color = image.color;
                    color.a = 0.5f;
                    image.color = color;
                }
            }

            nodeObjects[node.NodeId] = nodeObj;
        }

        private void CreatePaths(MapData mapData)
        {
            if (pathLinePrefab == null || pathContainer == null) return;

            foreach (var node in mapData.GetAllNodes())
            {
                foreach (var connectedId in node.ConnectedNodeIds)
                {
                    var connectedNode = mapData.GetNode(connectedId);
                    if (connectedNode == null) continue;

                    var pathObj = Instantiate(pathLinePrefab, pathContainer);
                    var line = pathObj.GetComponent<LineRenderer>();

                    if (line != null)
                    {
                        Vector3 start = new Vector3(
                            node.Position.x * nodeScale + offset.x,
                            node.Position.y * nodeScale + offset.y,
                            0);
                        Vector3 end = new Vector3(
                            connectedNode.Position.x * nodeScale + offset.x,
                            connectedNode.Position.y * nodeScale + offset.y,
                            0);

                        line.SetPosition(0, start);
                        line.SetPosition(1, end);
                    }

                    // UI Image로 선 그리기 (대안)
                    var rectTransform = pathObj.GetComponent<RectTransform>();
                    if (rectTransform != null && line == null)
                    {
                        DrawLineUI(rectTransform, node.Position, connectedNode.Position);
                    }

                    pathObjects.Add(pathObj);
                }
            }
        }

        private void DrawLineUI(RectTransform rect, Vector2 from, Vector2 to)
        {
            Vector2 startPos = from * nodeScale + offset;
            Vector2 endPos = to * nodeScale + offset;

            Vector2 direction = endPos - startPos;
            float distance = direction.magnitude;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            rect.anchoredPosition = startPos;
            rect.sizeDelta = new Vector2(distance, 2f);
            rect.rotation = Quaternion.Euler(0, 0, angle);
            rect.pivot = new Vector2(0, 0.5f);
        }

        private void UpdateAccessibleNodes()
        {
            foreach (var kvp in nodeObjects)
            {
                var node = MapManager.Instance.CurrentMap.GetNode(kvp.Key);
                var button = kvp.Value.GetComponent<Button>();

                if (button != null && node != null)
                {
                    button.interactable = node.IsAccessible;
                }
            }
        }

        private void OnNodeClicked(string nodeId)
        {
            if (MapManager.Instance.SelectNode(nodeId))
            {
                var node = MapManager.Instance.CurrentNode;

                // RunManager에 알림
                if (RunManager.Instance != null)
                {
                    RunManager.Instance.OnNodeSelected(node);
                }

                // 노드 타입에 따라 씬 전환
                if (GameManager.Instance != null)
                {
                    switch (node.NodeType)
                    {
                        case Data.MapNodeType.Combat:
                        case Data.MapNodeType.Elite:
                        case Data.MapNodeType.Boss:
                            GameManager.Instance.LoadCombat();
                            break;
                        case Data.MapNodeType.Event:
                            GameManager.Instance.LoadEvent();
                            break;
                        case Data.MapNodeType.Shop:
                            GameManager.Instance.LoadShop();
                            break;
                        case Data.MapNodeType.Rest:
                            GameManager.Instance.LoadRest();
                            break;
                        case Data.MapNodeType.Treasure:
                            GameManager.Instance.LoadReward();
                            break;
                    }
                }
            }
        }
    }
}
