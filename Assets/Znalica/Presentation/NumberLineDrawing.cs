using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The 0-20 line every explanation that shows numbers on a line draws: each whole number labelled with a tick, and the
    /// numbers the caller marks coloured. Drawn at once, with no animation. The jump of the arithmetic screen adds its bar
    /// on top, using <see cref="X"/>.
    /// </summary>
    internal static class NumberLineDrawing
    {
        public const float CellHeight = 60f;
        public static readonly Color StartColor = new Color(0.62f, 0.78f, 0.95f);
        public static readonly Color EndColor = new Color(0.62f, 0.88f, 0.62f);

        private const float CellWidth = 52f;
        private const float FontSize = 34f;
        private static readonly Color TickColor = new Color(0.4f, 0.4f, 0.4f);

        /// <summary>
        /// Draws the line's numbers under <paramref name="line"/>. <paramref name="markFor"/> gives the colour to mark a
        /// number with, or null to leave it unmarked.
        /// </summary>
        public static void Draw(RectTransform line, Func<int, Color?> markFor)
        {
            for (int number = NumberRange.Min; number <= NumberRange.Max; number++)
            {
                float x = X(number);
                RectTransform cell = UiBuilder.NewChild("Number" + number, line);
                cell.anchorMin = cell.anchorMax = new Vector2(0.5f, 0f);
                cell.pivot = new Vector2(0.5f, 0f);
                cell.sizeDelta = new Vector2(CellWidth - 4f, CellHeight);
                cell.anchoredPosition = new Vector2(x, 0f);
                Color? mark = markFor(number);
                if (mark.HasValue)
                {
                    cell.gameObject.AddComponent<Image>().color = mark.Value;
                }

                TextMeshProUGUI label = UiBuilder.NewLabel("Text", cell, number.ToString(), FontSize);
                UiBuilder.Stretch(label.rectTransform);
                label.alignment = TextAlignmentOptions.Center;
                RectTransform tick = UiBuilder.NewChild("Tick", line);
                tick.anchorMin = tick.anchorMax = new Vector2(0.5f, 0f);
                tick.pivot = new Vector2(0.5f, 0f);
                tick.sizeDelta = new Vector2(3f, 18f);
                tick.anchoredPosition = new Vector2(x, CellHeight + 4f);
                tick.gameObject.AddComponent<Image>().color = TickColor;
            }
        }

        /// <summary>The horizontal position of a number on the line, from the line's centre, which is the middle number.</summary>
        public static float X(int number)
        {
            return CellWidth * (number - (NumberRange.Min + NumberRange.Max) / 2f);
        }
    }
}
