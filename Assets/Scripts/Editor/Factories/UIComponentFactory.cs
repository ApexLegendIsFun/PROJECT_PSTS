// Editor/Factories/UIComponentFactory.cs
// UI 컴포넌트 생성 팩토리 (UI Component Factory)
// SceneSetupAutomation에서 추출된 공용 UI 생성 메서드

using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

namespace ProjectSS.Editor
{
    /// <summary>
    /// UI 컴포넌트 생성을 위한 팩토리 클래스
    /// 씬 빌더들이 공용으로 사용하는 UI 생성 메서드 제공
    /// </summary>
    public static class UIComponentFactory
    {
        #region Color Constants

        public static readonly Color PrimaryColor = new Color(0.2f, 0.2f, 0.3f, 1f);
        public static readonly Color SecondaryColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        public static readonly Color AccentColor = new Color(0.3f, 0.6f, 0.9f, 1f);
        public static readonly Color TextColor = Color.white;

        #endregion

        #region Core UI Components

        /// <summary>
        /// EventSystem이 없으면 생성
        /// </summary>
        public static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                var eventSystemGo = new GameObject("EventSystem");
                eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
            }
        }

        /// <summary>
        /// Canvas 생성 (ScreenSpaceOverlay, 1920x1080 기준)
        /// </summary>
        public static GameObject CreateCanvas(string name)
        {
            var canvasGo = new GameObject(name);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        /// <summary>
        /// 기본 패널 생성 (투명 배경)
        /// </summary>
        public static GameObject CreatePanel(Transform parent, string name)
        {
            var panelGo = new GameObject(name);
            panelGo.transform.SetParent(parent, false);
            panelGo.AddComponent<RectTransform>();
            var image = panelGo.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0);
            return panelGo;
        }

        /// <summary>
        /// 이미지 오브젝트 생성
        /// </summary>
        public static GameObject CreateImage(Transform parent, string name, Vector2 size)
        {
            var imageGo = new GameObject(name);
            imageGo.transform.SetParent(parent, false);
            var rect = imageGo.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            var image = imageGo.AddComponent<Image>();
            image.color = Color.white;
            return imageGo;
        }

        /// <summary>
        /// 텍스트 오브젝트 생성
        /// </summary>
        public static GameObject CreateText(Transform parent, string name, string text, int fontSize)
        {
            var textGo = new GameObject(name);
            textGo.transform.SetParent(parent, false);
            textGo.AddComponent<RectTransform>();
            var textComp = textGo.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.color = TextColor;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return textGo;
        }

        #endregion

        #region Interactive Components

        /// <summary>
        /// 버튼 생성 (텍스트 포함)
        /// </summary>
        public static GameObject CreateButton(Transform parent, string name, string buttonText, Vector2 size)
        {
            var buttonGo = new GameObject(name);
            buttonGo.transform.SetParent(parent, false);

            var rect = buttonGo.AddComponent<RectTransform>();
            rect.sizeDelta = size;

            var image = buttonGo.AddComponent<Image>();
            image.color = AccentColor;

            var button = buttonGo.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = AccentColor;
            colors.highlightedColor = new Color(AccentColor.r + 0.1f, AccentColor.g + 0.1f, AccentColor.b + 0.1f, 1f);
            colors.pressedColor = new Color(AccentColor.r - 0.1f, AccentColor.g - 0.1f, AccentColor.b - 0.1f, 1f);
            button.colors = colors;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(buttonGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            SetRectTransformFill(textRect);
            var textComp = textGo.AddComponent<Text>();
            textComp.text = buttonText;
            textComp.fontSize = 20;
            textComp.color = TextColor;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return buttonGo;
        }

        /// <summary>
        /// 슬라이더 생성 (로딩 바 등에 사용)
        /// </summary>
        public static GameObject CreateSlider(Transform parent, string name)
        {
            var sliderGo = new GameObject(name);
            sliderGo.transform.SetParent(parent, false);
            sliderGo.AddComponent<RectTransform>();

            // Background
            var background = CreateImage(sliderGo.transform, "Background", Vector2.zero);
            SetRectTransformFill(background.GetComponent<RectTransform>());
            background.GetComponent<Image>().color = SecondaryColor;

            // Fill Area
            var fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.offsetMin = new Vector2(5, 0);
            fillAreaRect.offsetMax = new Vector2(-5, 0);

            var fill = CreateImage(fillArea.transform, "Fill", Vector2.zero);
            SetRectTransformFill(fill.GetComponent<RectTransform>());
            fill.GetComponent<Image>().color = AccentColor;

            var slider = sliderGo.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.value = 0.5f;
            slider.interactable = false;

            return sliderGo;
        }

        #endregion

        #region Specialized Components

        /// <summary>
        /// 캐릭터 선택 버튼 생성 (초상화 + 이름)
        /// </summary>
        public static GameObject CreateCharacterButton(Transform parent, string id, string characterName, Color themeColor)
        {
            var buttonGo = new GameObject($"CharacterButton_{id}");
            buttonGo.transform.SetParent(parent, false);

            var rect = buttonGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 200);

            var image = buttonGo.AddComponent<Image>();
            image.color = themeColor;

            var button = buttonGo.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = themeColor;
            colors.highlightedColor = new Color(themeColor.r + 0.15f, themeColor.g + 0.15f, themeColor.b + 0.15f, 1f);
            colors.pressedColor = new Color(themeColor.r - 0.1f, themeColor.g - 0.1f, themeColor.b - 0.1f, 1f);
            colors.selectedColor = new Color(1f, 0.9f, 0.3f, 1f);
            button.colors = colors;

            // Portrait placeholder
            var portrait = CreateImage(buttonGo.transform, "Portrait", new Vector2(100, 100));
            var portraitRect = portrait.GetComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0.5f, 0.6f);
            portraitRect.anchorMax = new Vector2(0.5f, 0.6f);
            portraitRect.anchoredPosition = Vector2.zero;
            portrait.GetComponent<Image>().color = new Color(themeColor.r * 0.7f, themeColor.g * 0.7f, themeColor.b * 0.7f, 1f);

            // Name text
            var nameText = CreateText(buttonGo.transform, "NameText", characterName, 18);
            var nameRect = nameText.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 0.15f);
            nameRect.anchorMax = new Vector2(0.5f, 0.15f);
            nameRect.sizeDelta = new Vector2(140, 30);

            return buttonGo;
        }

        /// <summary>
        /// 슬롯 패널 생성 (캐릭터 선택 슬롯)
        /// </summary>
        public static GameObject CreateSlotPanel(Transform parent, string name)
        {
            var slotGo = new GameObject(name);
            slotGo.transform.SetParent(parent, false);

            var rect = slotGo.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 80);

            var image = slotGo.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.35f, 0.8f);

            // Empty slot text
            var emptyText = CreateText(slotGo.transform, "EmptyText", "?", 32);
            var emptyRect = emptyText.GetComponent<RectTransform>();
            SetRectTransformFill(emptyRect);
            emptyText.GetComponent<Text>().color = new Color(0.5f, 0.5f, 0.5f, 1f);

            return slotGo;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// RectTransform을 부모에 꽉 차게 설정
        /// </summary>
        public static void SetRectTransformFill(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// 버튼 위치 설정 (앵커 기반)
        /// </summary>
        public static void PositionButton(GameObject button, float anchorX, float anchorY)
        {
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorX, anchorY);
            rect.anchorMax = new Vector2(anchorX, anchorY);
            rect.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 리플렉션을 통한 private 필드 설정
        /// SerializedField 연결에 사용
        /// </summary>
        public static void SetPrivateField(object target, string fieldName, object value)
        {
            var type = target.GetType();
            var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
                EditorUtility.SetDirty(target as Object);
            }
            else
            {
                Debug.LogWarning($"[UIComponentFactory] Field not found: {fieldName} in {type.Name}");
            }
        }

        #endregion
    }
}
