using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ThreeDee.UI
{
    public class GameOverUI : MonoBehaviour
    {
        private GameObject _panel;

        public static GameOverUI Create()
        {
            var go = new GameObject("GameOverUI");
            return go.AddComponent<GameOverUI>();
        }

        private void Awake()
        {
            BuildUI();
            _panel.SetActive(false);
        }

        private void BuildUI()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();

            // Dark overlay panel
            _panel = new GameObject("Panel");
            _panel.transform.SetParent(transform, false);
            var panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);

            // "GAME OVER" text
            CreateText(_panel.transform, "GameOverText",
                text: "GAME OVER",
                fontSize: 58,
                bold: true,
                color: new Color(0.9f, 0.15f, 0.1f),
                anchorY: 0.62f,
                height: 80f);

            // Retry button
            CreateButton(_panel.transform, "RetryButton",
                label: "RETRY",
                anchorY: 0.42f,
                onClick: OnRetry);
        }

        private static void CreateText(Transform parent, string name, string text, int fontSize,
            bool bold, Color color, float anchorY, float height)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, anchorY);
            rect.anchorMax = new Vector2(0.5f, anchorY);
            rect.sizeDelta = new Vector2(500f, height);
            rect.anchoredPosition = Vector2.zero;
            var t = go.AddComponent<Text>();
            t.text = text;
            t.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
            t.fontSize = fontSize;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color;
        }

        private static void CreateButton(Transform parent, string name, string label,
            float anchorY, UnityEngine.Events.UnityAction onClick)
        {
            var btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, anchorY);
            rect.anchorMax = new Vector2(0.5f, anchorY);
            rect.sizeDelta = new Vector2(220f, 60f);
            rect.anchoredPosition = Vector2.zero;
            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.15f, 0.55f, 0.15f);
            var btn = btnGo.AddComponent<Button>();
            btn.targetGraphic = img;
            var cs = btn.colors;
            cs.highlightedColor = new Color(0.2f, 0.75f, 0.2f);
            cs.pressedColor = new Color(0.1f, 0.4f, 0.1f);
            btn.colors = cs;
            btn.onClick.AddListener(onClick);

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var t = textGo.AddComponent<Text>();
            t.text = label;
            t.font = Font.CreateDynamicFontFromOSFont("Arial", 28);
            t.fontSize = 28;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
        }

        public void Show()
        {
            _panel.SetActive(true);
        }

        private void OnRetry()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
