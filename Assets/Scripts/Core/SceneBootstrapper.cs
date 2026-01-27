// Core/SceneBootstrapper.cs
// 씬 부트스트랩 유틸리티 - Boot 씬 없이 개별 씬 실행 지원

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ProjectSS.Core
{
    /// <summary>
    /// 씬 부트스트랩 유틸리티
    /// Boot 씬을 거치지 않고 개별 씬을 직접 실행할 때 필수 매니저 초기화
    /// </summary>
    public static class SceneBootstrapper
    {
        /// <summary>
        /// 필수 시스템 초기화 여부 확인 및 초기화
        /// Boot 씬 없이 직접 실행된 씬에서 호출
        /// </summary>
        public static void EnsureInitialized()
        {
            // GameManager가 없으면 생성
            if (GameManager.Instance == null)
            {
                Debug.Log("[SceneBootstrapper] GameManager not found. Creating...");
                var go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
            }

            // ServiceLocator 초기화 완료 표시
            if (!ServiceLocator.IsInitialized)
            {
                ServiceLocator.IsInitialized = true;
                Debug.Log("[SceneBootstrapper] ServiceLocator marked as initialized.");
            }
        }

        /// <summary>
        /// Boot 씬을 거치지 않고 실행되었는지 확인
        /// </summary>
        public static bool IsDirectSceneRun()
        {
            return GameManager.Instance == null || !ServiceLocator.IsInitialized;
        }

        /// <summary>
        /// UI 인프라 확인 및 생성 (Canvas, EventSystem)
        /// Boot 씬 없이 직접 실행 시 UI가 작동하도록 보장
        /// </summary>
        public static void EnsureUIInfrastructure()
        {
            // Canvas 확인/생성
            if (Object.FindObjectOfType<Canvas>() == null)
            {
                Debug.Log("[SceneBootstrapper] Canvas not found. Creating...");
                var canvasGo = new GameObject("TestCanvas");
                var canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;

                var scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // EventSystem 확인/생성
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                Debug.Log("[SceneBootstrapper] EventSystem not found. Creating...");
                var eventGo = new GameObject("EventSystem");
                eventGo.AddComponent<EventSystem>();
                eventGo.AddComponent<StandaloneInputModule>();
            }
        }
    }
}
