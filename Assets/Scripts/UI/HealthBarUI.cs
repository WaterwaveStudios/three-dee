using UnityEngine;
using UnityEngine.UI;

namespace ThreeDee.UI
{
    public class HealthBarUI : MonoBehaviour
    {
        private Image _fill;

        public static HealthBarUI Create(Canvas canvas)
        {
            // Background bar
            var barGo = new GameObject("HealthBar");
            barGo.transform.SetParent(canvas.transform, false);
            var bgRect = barGo.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 1f);
            bgRect.anchorMax = new Vector2(0f, 1f);
            bgRect.pivot = new Vector2(0f, 1f);
            bgRect.anchoredPosition = new Vector2(20f, -20f);
            bgRect.sizeDelta = new Vector2(200f, 26f);
            var bgImg = barGo.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Fill
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(barGo.transform, false);
            var fillRect = fillGo.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.8f, 0.2f);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillAmount = 1f;

            // HP label
            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(barGo.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGo.AddComponent<Text>();
            label.text = "HP";
            label.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            label.fontSize = 14;
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;

            var hb = barGo.AddComponent<HealthBarUI>();
            hb._fill = fillImg;
            return hb;
        }

        public void SetHealth(int current, int max)
        {
            if (_fill == null) return;
            float t = max > 0 ? (float)current / max : 0f;
            _fill.fillAmount = t;
            _fill.color = Color.Lerp(new Color(0.9f, 0.15f, 0.1f), new Color(0.2f, 0.8f, 0.2f), t);
        }
    }
}
