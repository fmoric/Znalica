using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Znalica.Presentation
{
    /// <summary>The widgets every screen builds in code the same way: panels, labels and buttons, and the colours they share.</summary>
    internal static class UiBuilder
    {
        public static readonly Color BackgroundColor = new Color(0.96f, 0.95f, 0.88f);
        public static readonly Color TextColor = new Color(0.12f, 0.12f, 0.12f);

        public static RectTransform NewChild(string name, RectTransform parent)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return (RectTransform)child.transform;
        }

        public static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static TextMeshProUGUI NewLabel(string name, RectTransform parent, string text, float fontSize)
        {
            RectTransform rect = NewChild(name, parent);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = TextColor;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>
        /// A white button with a centred caption. It takes no keyboard navigation and gives up the selection after a
        /// click, so a later Enter or Space key press is the screen's own and does not also "click" the last button.
        /// </summary>
        public static RectTransform NewButton(string name, RectTransform parent, string text, float fontSize, UnityAction onClick)
        {
            RectTransform rect = NewChild(name, parent);
            Image background = rect.gameObject.AddComponent<Image>();
            background.color = Color.white;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() =>
            {
                onClick();
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            });

            TextMeshProUGUI label = NewLabel("Label", rect, text, fontSize);
            Stretch(label.rectTransform);
            return rect;
        }
    }
}
