using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The 0-20 line every explanation that shows numbers on a line draws: each whole number labelled with a tick, and the
    /// numbers the caller marks coloured. Drawn at once, with no animation. Three callers: the jump of the arithmetic and
    /// number-line screens (<see cref="DrawJump"/>), the order of the comparison screen, and the number-line screen, which
    /// also reads a click on it back to a number (<see cref="NumberAt"/>).
    /// </summary>
    internal static class NumberLineDrawing
    {
        public const float CellHeight = 60f;
        public static readonly Color StartColor = new Color(0.62f, 0.78f, 0.95f);
        public static readonly Color EndColor = new Color(0.62f, 0.88f, 0.62f);

        private const float BarY = 110f;
        private const float BarHeight = 14f;
        private const float ChangeY = 140f;
        private const float ChangeFontSize = 56f;
        private const float CellWidth = 52f;
        private const float FontSize = 34f;
        private static readonly Color JumpColor = new Color(0.95f, 0.62f, 0.2f);
        private static readonly Color TickColor = new Color(0.4f, 0.4f, 0.4f);

        /// <summary>
        /// Draws the line's numbers under <paramref name="line"/>. <paramref name="markFor"/> gives the colour to mark a
        /// number with, or null to leave it unmarked. Every number carries its label unless <paramref name="labelled"/> says
        /// otherwise; a number without one still has its tick, so a child counts to it.
        /// </summary>
        public static void Draw(RectTransform line, Func<int, Color?> markFor, Func<int, bool> labelled = null)
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

                if (labelled == null || labelled(number))
                {
                    TextMeshProUGUI label = UiBuilder.NewLabel("Text", cell, number.ToString(), FontSize);
                    UiBuilder.Stretch(label.rectTransform);
                    label.alignment = TextAlignmentOptions.Center;
                }

                RectTransform tick = UiBuilder.NewChild("Tick", line);
                tick.anchorMin = tick.anchorMax = new Vector2(0.5f, 0f);
                tick.pivot = new Vector2(0.5f, 0f);
                tick.sizeDelta = new Vector2(3f, 18f);
                tick.anchoredPosition = new Vector2(x, CellHeight + 4f);
                tick.gameObject.AddComponent<Image>().color = TickColor;
            }
        }

        /// <summary>
        /// The jump on the 0-20 line: every whole number labelled, the start and the landing marked, and a bar from one to
        /// the other with the size of the jump above it. Drawn at once, with no animation.
        /// </summary>
        public static void DrawJump(RectTransform line, Explanation explanation)
        {
            Draw(line, number =>
                number == explanation.End ? EndColor
                : number == explanation.Start ? StartColor
                : (Color?)null);

            float startX = X(explanation.Start);
            float endX = X(explanation.End);
            RectTransform bar = UiBuilder.NewChild("Jump", line);
            bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(Mathf.Max(Mathf.Abs(endX - startX), BarHeight), BarHeight);
            bar.anchoredPosition = new Vector2((startX + endX) / 2f, BarY);
            bar.gameObject.AddComponent<Image>().color = JumpColor;

            TextMeshProUGUI change = UiBuilder.NewLabel("Change", line, ItemFormatter.ChangeText(explanation), ChangeFontSize);
            change.rectTransform.anchorMin = change.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            change.rectTransform.pivot = new Vector2(0.5f, 0f);
            change.rectTransform.sizeDelta = new Vector2(240f, 80f);
            change.rectTransform.anchoredPosition = new Vector2((startX + endX) / 2f, ChangeY);
        }

        /// <summary>
        /// The right place for a number: the number the task named marked as the start, the right place as the end and, when
        /// the child clicked somewhere else, that click marked as the start too (<paramref name="clicked"/>).
        /// </summary>
        public static void DrawPosition(RectTransform line, Explanation explanation, int? clicked)
        {
            Draw(line, number =>
                number == explanation.Target ? EndColor
                : number == explanation.Given || number == clicked ? StartColor
                : (Color?)null);
        }

        /// <summary>The number at a horizontal position from the line's centre: the inverse of <see cref="X"/>, not yet rounded.</summary>
        public static double NumberAt(float x)
        {
            return x / CellWidth + (NumberRange.Min + NumberRange.Max) / 2.0;
        }

        /// <summary>The horizontal position of a number on the line, from the line's centre, which is the middle number.</summary>
        public static float X(int number)
        {
            return CellWidth * (number - (NumberRange.Min + NumberRange.Max) / 2f);
        }
    }
}
