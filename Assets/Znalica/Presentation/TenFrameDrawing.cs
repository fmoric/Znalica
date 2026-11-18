using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// A group of objects drawn in ten-frames: boxes of two rows of five, the first frame filled before the second. Up to
    /// ten objects take one frame, 11 to 20 two, and none an empty one. Placeholder art (a square for each object) until
    /// the art style is chosen (functional spec open question 6).
    /// </summary>
    internal static class TenFrameDrawing
    {
        private const int FrameSize = 10;
        private const int Columns = 5;
        private static readonly Color FrameColor = new Color(0.25f, 0.25f, 0.25f);
        private static readonly Color EmptySlotColor = new Color(0.96f, 0.96f, 0.96f);

        /// <summary>The group under <paramref name="parent"/>, sized but not placed: the caller lays it out.</summary>
        public static RectTransform Draw(RectTransform parent, int count, float cell, Color objectColor)
        {
            int frames = Mathf.Max(1, (count + FrameSize - 1) / FrameSize);
            float frameHeight = 2f * cell;
            float gap = cell * 0.25f;
            RectTransform group = UiBuilder.NewChild("Group", parent);
            group.sizeDelta = new Vector2(Columns * cell, frames * frameHeight + (frames - 1) * gap);

            for (int frameIndex = 0; frameIndex < frames; frameIndex++)
            {
                RectTransform frame = TopLeft(UiBuilder.NewChild("Frame", group), Columns * cell, frameHeight, 0f, -frameIndex * (frameHeight + gap));
                frame.gameObject.AddComponent<Image>().color = FrameColor;
                for (int slotIndex = 0; slotIndex < FrameSize; slotIndex++)
                {
                    float x = (slotIndex % Columns) * cell;
                    float y = -(slotIndex / Columns) * cell;
                    RectTransform slot = TopLeft(UiBuilder.NewChild("Slot", frame), cell - 4f, cell - 4f, x + 2f, y - 2f);
                    slot.gameObject.AddComponent<Image>().color = EmptySlotColor;
                    if (frameIndex * FrameSize + slotIndex < count)
                    {
                        RectTransform dot = TopLeft(UiBuilder.NewChild("Object", slot), cell * 0.6f, cell * 0.6f, cell * 0.18f, -cell * 0.18f);
                        dot.gameObject.AddComponent<Image>().color = objectColor;
                    }
                }
            }

            return group;
        }

        private static RectTransform TopLeft(RectTransform rect, float width, float height, float x, float y)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(x, y);
            return rect;
        }
    }
}
