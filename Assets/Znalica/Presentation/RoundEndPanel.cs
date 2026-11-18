using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The round-end panel every activity screen shows when its round is over: the score and the two buttons, Igraj opet
    /// and Završi. Built in code over the whole screen; the screen shows and hides it.
    /// </summary>
    internal sealed class RoundEndPanel
    {
        private const float WideButtonFontSize = 56f;
        private const float ScoreFontSize = 200f;
        private const float ScoreY = 160f;
        private const float EndButtonWidth = 480f;
        private const float EndButtonHeight = 170f;
        private const float EndButtonSpacing = 60f;
        private const float EndButtonsY = -170f;

        private readonly RectTransform _root;
        private readonly TextMeshProUGUI _score;

        public RoundEndPanel(RectTransform parent, PhraseCatalog catalog, Action playAgain, Action stop)
        {
            _root = UiBuilder.NewChild("RoundEnd", parent);
            UiBuilder.Stretch(_root);
            _root.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            _score = UiBuilder.NewLabel("Score", _root, string.Empty, ScoreFontSize);
            _score.rectTransform.anchorMin = _score.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _score.rectTransform.sizeDelta = new Vector2(1400f, 300f);
            _score.rectTransform.anchoredPosition = new Vector2(0f, ScoreY);

            RectTransform again = UiBuilder.NewButton(
                "PlayAgain", _root, catalog.Text(PhraseKey.ButtonPlayAgain), WideButtonFontSize, () => playAgain());
            EndButton(again, -(EndButtonWidth + EndButtonSpacing) / 2f);
            RectTransform end = UiBuilder.NewButton("Stop", _root, catalog.Text(PhraseKey.ButtonStop), WideButtonFontSize, () => stop());
            EndButton(end, (EndButtonWidth + EndButtonSpacing) / 2f);
        }

        public void SetActive(bool active)
        {
            _root.gameObject.SetActive(active);
        }

        public void SetScore(string text)
        {
            _score.text = text;
        }

        private static void EndButton(RectTransform button, float x)
        {
            button.anchorMin = button.anchorMax = new Vector2(0.5f, 0.5f);
            button.sizeDelta = new Vector2(EndButtonWidth, EndButtonHeight);
            button.anchoredPosition = new Vector2(x, EndButtonsY);
        }
    }
}
