using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The large row of tokens an item is shown as (<c>8 + 5 = ?</c>), with a white box for the unknown that shows what the
    /// child has typed. The arithmetic and number-line screens draw their tasks with it.
    /// </summary>
    internal static class EquationDrawing
    {
        public const float Height = 200f;

        private const float FontSize = 150f;
        private const float WidthBase = 60f;
        private const float WidthPerCharacter = 75f;
        private const float Spacing = 20f;

        /// <summary>A centred row, <paramref name="top"/> pixels below the top of the parent, ready for <see cref="Draw"/>.</summary>
        public static RectTransform NewRow(RectTransform parent, float top)
        {
            RectTransform row = UiBuilder.TopRow("Equation", parent, top, Height);
            UiBuilder.CentreRow(row, Spacing);
            return row;
        }

        /// <param name="entered">What the child has typed so far; shown in the unknown's box, which stays empty while it is null.</param>
        public static void Draw(RectTransform row, System.Collections.Generic.IReadOnlyList<ItemToken> tokens, int? entered)
        {
            UiBuilder.Clear(row);
            foreach (ItemToken token in tokens)
            {
                string text = token.IsUnknown ? (entered.HasValue ? entered.Value.ToString() : string.Empty) : token.Text;
                RectTransform cell = UiBuilder.NewChild("Token", row);
                // The answer box keeps the width of two digits, so it does not change size while the child types.
                int width = token.IsUnknown ? 2 : text.Length;
                cell.sizeDelta = new Vector2(WidthBase + WidthPerCharacter * width, Height);
                if (token.IsUnknown)
                {
                    cell.gameObject.AddComponent<Image>().color = Color.white;
                }

                TextMeshProUGUI label = UiBuilder.NewLabel("Text", cell, text, FontSize);
                UiBuilder.Stretch(label.rectTransform);
            }
        }
    }
}
