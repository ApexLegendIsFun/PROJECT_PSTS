// Editor/Builders/MapSceneBuilder.cs
// Map 씬 빌더 - 월드맵 (노드 그래프 기반)

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using ProjectSS.Map;
using ProjectSS.Map.UI;

namespace ProjectSS.Editor
{
    /// <summary>
    /// Map 씬 생성 빌더
    /// 노드 그래프 기반 월드맵 UI 생성
    /// - MapBackground (배경)
    /// - WorldMapContainer (ScrollRect + 노드/경로 컨테이너)
    /// - TopInfoPanel (맵 이름, 진행도)
    /// - NodeTooltipPanel (노드 호버 정보)
    /// - WorldMapManager, WorldMapSceneInitializer, WorldMapController 컴포넌트
    /// </summary>
    public class MapSceneBuilder : ISceneBuilder
    {
        public string SceneName => "Map";

        // UI 색상 상수
        private static readonly Color BackgroundColor = new Color(0.1f, 0.12f, 0.15f, 1f);
        private static readonly Color PanelColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
        private static readonly Color ContentBgColor = new Color(0.08f, 0.08f, 0.1f, 1f);

        public string Build(string scenesFolder)
        {
            string scenePath = $"{scenesFolder}/{SceneName}.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = SceneName;

            UIComponentFactory.CreateEventSystem();
            var canvas = UIComponentFactory.CreateCanvas("MapCanvas");

            // 배경
            var background = CreateMapBackground(canvas.transform);

            // 월드맵 컨테이너 (ScrollRect)
            var (scrollRect, nodesContainer, pathsContainer) = CreateWorldMapContainer(canvas.transform);

            // 상단 정보 패널
            var topInfoPanel = CreateTopInfoPanel(canvas.transform);

            // 노드 툴팁 패널
            var tooltipPanel = CreateNodeTooltipPanel(canvas.transform);

            // 매니저 및 컨트롤러
            CreateMapComponents(scrollRect, nodesContainer, pathsContainer);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[MapSceneBuilder] Map 씬 생성: {scenePath}");
            return scenePath;
        }

        /// <summary>
        /// 맵 배경 생성
        /// </summary>
        private GameObject CreateMapBackground(Transform parent)
        {
            var bgGo = UIComponentFactory.CreatePanel(parent, "MapBackground");
            var bgRect = bgGo.GetComponent<RectTransform>();
            UIComponentFactory.SetRectTransformFill(bgRect);
            bgGo.GetComponent<Image>().color = BackgroundColor;
            return bgGo;
        }

        /// <summary>
        /// 월드맵 컨테이너 생성 (ScrollRect 구조)
        /// </summary>
        private (ScrollRect scrollRect, RectTransform nodesContainer, RectTransform pathsContainer)
            CreateWorldMapContainer(Transform parent)
        {
            // 메인 컨테이너 (ScrollRect가 붙을 오브젝트)
            var containerGo = UIComponentFactory.CreatePanel(parent, "WorldMapContainer");
            var containerRect = containerGo.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.05f, 0.15f);
            containerRect.anchorMax = new Vector2(0.95f, 0.85f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            containerGo.GetComponent<Image>().color = ContentBgColor;

            // Viewport (마스킹 영역)
            var viewportGo = UIComponentFactory.CreatePanel(containerGo.transform, "Viewport");
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            UIComponentFactory.SetRectTransformFill(viewportRect);
            var mask = viewportGo.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            viewportGo.GetComponent<Image>().color = Color.white; // 마스크용

            // Content (실제 맵 콘텐츠 - 스크롤되는 영역)
            var contentGo = UIComponentFactory.CreatePanel(viewportGo.transform, "Content");
            var contentRect = contentGo.GetComponent<RectTransform>();
            // 맵 크기 설정 (노드 위치에 맞게 충분히 크게)
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.zero;
            contentRect.pivot = Vector2.zero;
            contentRect.sizeDelta = new Vector2(800, 600); // 테스트 맵에 맞는 크기
            contentRect.anchoredPosition = Vector2.zero;
            DestroyImmediate(contentGo.GetComponent<Image>()); // Content는 배경 불필요

            // PathsContainer (경로가 먼저 그려져야 노드 뒤에 표시됨)
            var pathsGo = new GameObject("PathsContainer");
            pathsGo.transform.SetParent(contentGo.transform, false);
            var pathsRect = pathsGo.AddComponent<RectTransform>();
            UIComponentFactory.SetRectTransformFill(pathsRect);

            // NodesContainer (노드가 경로 위에 표시됨)
            var nodesGo = new GameObject("NodesContainer");
            nodesGo.transform.SetParent(contentGo.transform, false);
            var nodesRect = nodesGo.AddComponent<RectTransform>();
            UIComponentFactory.SetRectTransformFill(nodesRect);

            // ScrollRect 설정
            var scrollRect = containerGo.AddComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = true;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.elasticity = 0.1f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;
            scrollRect.scrollSensitivity = 1f;

            return (scrollRect, nodesRect, pathsRect);
        }

        /// <summary>
        /// 상단 정보 패널 생성
        /// </summary>
        private GameObject CreateTopInfoPanel(Transform parent)
        {
            var panelGo = UIComponentFactory.CreatePanel(parent, "TopInfoPanel");
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.9f);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelGo.GetComponent<Image>().color = PanelColor;

            // 맵 이름
            var mapNameText = UIComponentFactory.CreateText(panelGo.transform, "MapNameText", "월드맵", 24);
            var mapNameRect = mapNameText.GetComponent<RectTransform>();
            mapNameRect.anchorMin = new Vector2(0.02f, 0);
            mapNameRect.anchorMax = new Vector2(0.5f, 1);
            mapNameRect.offsetMin = Vector2.zero;
            mapNameRect.offsetMax = Vector2.zero;
            mapNameText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            // 진행도 텍스트
            var progressText = UIComponentFactory.CreateText(panelGo.transform, "ProgressText", "진행: 0/6", 18);
            var progressRect = progressText.GetComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.7f, 0);
            progressRect.anchorMax = new Vector2(0.98f, 1);
            progressRect.offsetMin = Vector2.zero;
            progressRect.offsetMax = Vector2.zero;
            progressText.GetComponent<Text>().alignment = TextAnchor.MiddleRight;

            return panelGo;
        }

        /// <summary>
        /// 노드 툴팁 패널 생성 (기본 숨김)
        /// </summary>
        private GameObject CreateNodeTooltipPanel(Transform parent)
        {
            var panelGo = UIComponentFactory.CreatePanel(parent, "NodeTooltipPanel");
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(1, 0.12f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelGo.GetComponent<Image>().color = PanelColor;
            panelGo.SetActive(false); // 기본 숨김

            // 노드 이름
            var nodeNameText = UIComponentFactory.CreateText(panelGo.transform, "NodeNameText", "", 22);
            var nodeNameRect = nodeNameText.GetComponent<RectTransform>();
            nodeNameRect.anchorMin = new Vector2(0.02f, 0.5f);
            nodeNameRect.anchorMax = new Vector2(0.3f, 1);
            nodeNameRect.offsetMin = Vector2.zero;
            nodeNameRect.offsetMax = Vector2.zero;
            nodeNameText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            // 노드 타입
            var nodeTypeText = UIComponentFactory.CreateText(panelGo.transform, "NodeTypeText", "", 16);
            var nodeTypeRect = nodeTypeText.GetComponent<RectTransform>();
            nodeTypeRect.anchorMin = new Vector2(0.02f, 0);
            nodeTypeRect.anchorMax = new Vector2(0.3f, 0.5f);
            nodeTypeRect.offsetMin = Vector2.zero;
            nodeTypeRect.offsetMax = Vector2.zero;
            nodeTypeText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            nodeTypeText.GetComponent<Text>().color = new Color(0.7f, 0.7f, 0.7f, 1f);

            // 노드 설명
            var nodeDescText = UIComponentFactory.CreateText(panelGo.transform, "NodeDescriptionText", "", 16);
            var nodeDescRect = nodeDescText.GetComponent<RectTransform>();
            nodeDescRect.anchorMin = new Vector2(0.35f, 0);
            nodeDescRect.anchorMax = new Vector2(0.98f, 1);
            nodeDescRect.offsetMin = Vector2.zero;
            nodeDescRect.offsetMax = Vector2.zero;
            nodeDescText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            return panelGo;
        }

        /// <summary>
        /// 매니저 및 컨트롤러 생성
        /// </summary>
        private void CreateMapComponents(ScrollRect scrollRect, RectTransform nodesContainer, RectTransform pathsContainer)
        {
            // WorldMapManager (싱글톤)
            var managerGo = new GameObject("WorldMapManager");
            managerGo.AddComponent<WorldMapManager>();

            // WorldMapSceneInitializer
            var initializerGo = new GameObject("WorldMapSceneInitializer");
            var initializer = initializerGo.AddComponent<WorldMapSceneInitializer>();

            // WorldMapController
            var controllerGo = new GameObject("WorldMapController");
            var controller = controllerGo.AddComponent<WorldMapController>();

            // 에셋 연결
            WorldMapAssetGenerator.LinkToInitializer(initializer);
            WorldMapAssetGenerator.LinkToController(controller, nodesContainer, pathsContainer, scrollRect);
        }

        /// <summary>
        /// DestroyImmediate 래퍼 (에디터 전용)
        /// </summary>
        private void DestroyImmediate(Object obj)
        {
            Object.DestroyImmediate(obj);
        }
    }
}
