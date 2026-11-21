using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The round-end panel every activity screen shows when its round is over: the score and the two buttons, Igraj opet
    /// and Završi, and, after about 10 minutes of play, a spoken break suggestion that locks nothing. Built in code over
    /// the whole screen; the screen shows and hides it.
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
        private const float BreakFontSize = 56f;
        private const float BreakY = -400f;

        private readonly PhraseCatalog _catalog;
        private readonly VoicePlayer _voice;
        private readonly RectTransform _root;
        private readonly TextMeshProUGUI _score;
        private readonly TextMeshProUGUI _break;
        private bool _breakShown;

        public RoundEndPanel(RectTransform parent, PhraseCatalog catalog, VoicePlayer voice, Action playAgain, Action stop)
        {
            _catalog = catalog;
            _voice = voice;
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

            _break = UiBuilder.NewLabel("Break", _root, string.Empty, BreakFontSize);
            _break.rectTransform.anchorMin = _break.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _break.rectTransform.sizeDelta = new Vector2(1600f, 200f);
            _break.rectTransform.anchoredPosition = new Vector2(0f, BreakY);
        }

        public void SetActive(bool active)
        {
            _root.gameObject.SetActive(active);
            if (!active)
            {
                // The next round end decides again whether the suggestion is shown, and speaks it again if it is.
                _breakShown = false;
                _break.text = string.Empty;
            }
        }

        public void SetScore(string text)
        {
            _score.text = text;
        }

        /// <summary>
        /// Shows the break suggestion and speaks it, after the last answer's feedback has finished, once for this showing of the panel; the screen may redraw as often as it
        /// likes. Both buttons stay as they are. With <paramref name="due"/> false it does nothing.
        /// </summary>
        public void SuggestBreak(bool due)
        {
            if (!due || _breakShown)
            {
                return;
            }

            _breakShown = true;
            _break.text = _catalog.Text(PhraseKey.BreakSuggestion);
            _voice.PlayAfterCurrent(_catalog.VoiceClips(PhraseKey.BreakSuggestion));
        }

        private static void EndButton(RectTransform button, float x)
        {
            button.anchorMin = button.anchorMax = new Vector2(0.5f, 0.5f);
            button.sizeDelta = new Vector2(EndButtonWidth, EndButtonHeight);
            button.anchoredPosition = new Vector2(x, EndButtonsY);
        }
    }
}
