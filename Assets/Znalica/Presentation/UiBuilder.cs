using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Znalica.Domain;

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

        /// <summary>Lays the row's children out side by side, centred, each at the size it was given.</summary>
        public static void CentreRow(RectTransform row, float spacing)
        {
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        /// <summary>A full-width row that hangs <paramref name="top"/> pixels below the top of the parent.</summary>
        public static RectTransform TopRow(string name, RectTransform parent, float top, float height)
        {
            RectTransform row = NewChild(name, parent);
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.sizeDelta = new Vector2(0f, height);
            row.anchoredPosition = new Vector2(0f, -top);
            return row;
        }

        /// <summary>Centres the rect horizontally, <paramref name="top"/> pixels below the top of its parent.</summary>
        public static void CentreTop(RectTransform rect, float top, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(0f, -top);
        }

        /// <summary>Anchors the rectangle to its parent's top left, at the given offset and size.</summary>
        public static void PlaceTopLeft(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(left, -top);
        }

        /// <summary>
        /// Anchors the rectangle to the bottom edge of its parent, <paramref name="y"/> pixels above it and <paramref name="x"/> in
        /// from the left edge (<paramref name="anchorX"/> 0) or from the right edge (1; a negative x then moves left).
        /// </summary>
        public static void PlaceBottom(RectTransform rect, float anchorX, float x, float y, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(anchorX, 0f);
            rect.pivot = new Vector2(anchorX, 0f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, y);
        }

        /// <summary>Removes every child, so a screen can draw its state again.</summary>
        public static void Clear(RectTransform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                Object.Destroy(child);
            }
        }

        /// <summary>
        /// A profile picture: a square in a colour of its own with the picture's name on it. Placeholder art until the
        /// art style is chosen (functional spec open question 6); the colour is what tells the pictures apart.
        /// </summary>
        public static RectTransform NewPictureTile(string name, RectTransform parent, string pictureId, float size)
        {
            RectTransform tile = NewChild(name, parent);
            tile.sizeDelta = new Vector2(size, size);
            int index = Mathf.Max(0, ProfilePictures.IndexOf(pictureId));
            tile.gameObject.AddComponent<Image>().color = Color.HSVToRGB(index / (float)ProfilePictures.Ids.Count, 0.45f, 0.95f);
            TextMeshProUGUI label = NewLabel("Name", tile, pictureId, size * 0.2f);
            Stretch(label.rectTransform);
            return tile;
        }

        /// <summary>A single-line text field with a white box and a grey hint that shows while it is empty.</summary>
        public static TMP_InputField NewInputField(string name, RectTransform parent, string hint, float fontSize, int characterLimit)
        {
            RectTransform rect = NewChild(name, parent);
            // Inactive while it is assembled: the field reads its text component when it is enabled.
            rect.gameObject.SetActive(false);
            Image background = rect.gameObject.AddComponent<Image>();
            background.color = Color.white;

            RectTransform area = NewChild("TextArea", rect);
            Stretch(area);
            area.offsetMin = new Vector2(20f, 8f);
            area.offsetMax = new Vector2(-20f, -8f);
            area.gameObject.AddComponent<RectMask2D>();

            TextMeshProUGUI text = NewLabel("Text", area, string.Empty, fontSize);
            Stretch(text.rectTransform);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            TextMeshProUGUI placeholder = NewLabel("Hint", area, hint, fontSize);
            Stretch(placeholder.rectTransform);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.color = new Color(0.5f, 0.5f, 0.5f);

            TMP_InputField input = rect.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = background;
            input.textViewport = area;
            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = characterLimit;
            rect.gameObject.SetActive(true);
            return input;
        }
    }
}
