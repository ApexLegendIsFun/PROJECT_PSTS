// Editor/Builders/BootSceneBuilder.cs
// Boot 씬 빌더 (Boot Scene Builder)

using UnityEngine;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using ProjectSS.Core;
using ProjectSS.Services;
using ProjectSS.Run;

namespace ProjectSS.Editor
{
    /// <summary>
    /// Boot 씬 생성 빌더
    /// - EventSystem, Canvas
    /// - LogoPanel (로고 이미지)
    /// - LoadingPanel (로딩 바, 텍스트)
    /// - BootLoader 컴포넌트
    /// </summary>
    public class BootSceneBuilder : ISceneBuilder
    {
        public string SceneName => "Boot";

        public string Build(string scenesFolder)
        {
            string scenePath = $"{scenesFolder}/{SceneName}.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = SceneName;

            // EventSystem
            UIComponentFactory.CreateEventSystem();

            // Canvas
            var canvas = UIComponentFactory.CreateCanvas("BootCanvas");

            // Logo Panel
            var logoPanel = UIComponentFactory.CreatePanel(canvas.transform, "LogoPanel");
            UIComponentFactory.SetRectTransformFill(logoPanel.GetComponent<RectTransform>());

            var logoImage = UIComponentFactory.CreateImage(logoPanel.transform, "LogoImage", new Vector2(400, 200));
            var logoRect = logoImage.GetComponent<RectTransform>();
            logoRect.anchorMin = new Vector2(0.5f, 0.5f);
            logoRect.anchorMax = new Vector2(0.5f, 0.5f);
            logoRect.anchoredPosition = Vector2.zero;
            logoImage.GetComponent<Image>().color = UIComponentFactory.AccentColor;

            // Loading Panel
            var loadingPanel = UIComponentFactory.CreatePanel(canvas.transform, "LoadingPanel");
            UIComponentFactory.SetRectTransformFill(loadingPanel.GetComponent<RectTransform>());

            var loadingText = UIComponentFactory.CreateText(loadingPanel.transform, "LoadingText", "Loading...", 24);
            var loadingTextRect = loadingText.GetComponent<RectTransform>();
            loadingTextRect.anchorMin = new Vector2(0.5f, 0.3f);
            loadingTextRect.anchorMax = new Vector2(0.5f, 0.3f);
            loadingTextRect.sizeDelta = new Vector2(300, 50);

            var loadingBar = UIComponentFactory.CreateSlider(loadingPanel.transform, "LoadingBar");
            var loadingBarRect = loadingBar.GetComponent<RectTransform>();
            loadingBarRect.anchorMin = new Vector2(0.2f, 0.2f);
            loadingBarRect.anchorMax = new Vector2(0.8f, 0.2f);
            loadingBarRect.sizeDelta = new Vector2(0, 30);

            // BootLoader 컴포넌트
            var bootLoaderGo = new GameObject("BootLoader");
            var bootLoader = bootLoaderGo.AddComponent<BootLoader>();

            // RunManager 생성 (DontDestroyOnLoad 자동 적용됨)
            var runManagerGo = new GameObject("RunManager");
            runManagerGo.AddComponent<RunManager>();

            // SerializedField 연결
            UIComponentFactory.SetPrivateField(bootLoader, "_logoObject", logoPanel);
            UIComponentFactory.SetPrivateField(bootLoader, "_loadingIndicator", loadingPanel);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[BootSceneBuilder] Boot 씬 생성: {scenePath}");
            return scenePath;
        }
    }
}
