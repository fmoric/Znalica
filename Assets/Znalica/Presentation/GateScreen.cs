using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The parent gate screen: the question in words, an answer box and the number pad. It draws a <see cref="ParentGate"/>
    /// and passes pad buttons and keys to it; the rules, the three tries and the 30-second wait are the gate's. Nothing
    /// on this screen is spoken.
    /// </summary>
    public sealed class GateScreen : MonoBehaviour
    {
        private const float TaskAreaRight = 0.62f;
        private const float QuestionTop = 140f;
        private const float QuestionHeight = 320f;
        private const float QuestionFontSize = 80f;
        private const float BoxTop = 500f;
        private const float BoxWidth = 360f;
        private const float BoxHeight = 200f;
        private const float BoxFontSize = 150f;
        private const float StatusTop = 740f;
        private const float StatusHeight = 130f;
        private const float StatusFontSize = 56f;
        private const float BackWidth = 380f;
        private const float BackHeight = 130f;
        private const float BackBottomMargin = 60f;
        private const float BackFontSize = 56f;

        private PhraseCatalog _catalog;
        private ParentGate _gate;
        private Action _onPassed;
        private GateResult _last;

        private TextMeshProUGUI _question;
        private TextMeshProUGUI _box;
        private TextMeshProUGUI _status;
        private CanvasGroup _pad;
        private bool _wasWaiting;

        public RectTransform Panel => (RectTransform)transform;

        /// <param name="gate">The app's one gate, so a wait is not escaped by leaving the screen and coming back.</param>
        /// <param name="onPassed">Called once the right answer has been given.</param>
        /// <param name="onBack">Called by the Natrag button.</param>
        public static GateScreen Create(RectTransform parent, PhraseCatalog catalog, ParentGate gate, Action onPassed, Action onBack)
        {
            RectTransform panel = UiBuilder.NewChild("GateScreen", parent);
            GateScreen screen = panel.gameObject.AddComponent<GateScreen>();
            screen._catalog = catalog;
            screen._gate = gate;
            screen._onPassed = onPassed;
            screen.Build(onBack);
            return screen;
        }

        /// <summary>Empties the answer box and redraws the gate as it stands. Call before the screen is shown.</summary>
        public void Begin()
        {
            _gate.ClearEntry();
            _last = GateResult.Typing;
            Render();
        }

        private void Update()
        {
            // The wait ends by itself: redraw while it runs and once when it is over.
            if (_gate.IsWaiting || _wasWaiting)
            {
                Render();
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            foreach (AnswerKey key in NumberPad.KeysPressed(keyboard))
            {
                Press(key);
            }
        }

        private void Press(AnswerKey key)
        {
            _last = _gate.Press(key);
            Render();
            if (_last == GateResult.Passed)
            {
                _onPassed();
            }
        }

        private void Build(Action onBack)
        {
            RectTransform panel = Panel;
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            RectTransform left = UiBuilder.NewChild("TaskArea", panel);
            left.anchorMin = Vector2.zero;
            left.anchorMax = new Vector2(TaskAreaRight, 1f);
            left.offsetMin = left.offsetMax = Vector2.zero;

            _question = UiBuilder.NewLabel("Question", left, string.Empty, QuestionFontSize);
            TopRow(_question.rectTransform, QuestionTop, QuestionHeight);

            RectTransform box = UiBuilder.NewChild("AnswerBox", left);
            box.anchorMin = box.anchorMax = new Vector2(0.5f, 1f);
            box.pivot = new Vector2(0.5f, 1f);
            box.sizeDelta = new Vector2(BoxWidth, BoxHeight);
            box.anchoredPosition = new Vector2(0f, -BoxTop);
            box.gameObject.AddComponent<Image>().color = Color.white;
            _box = UiBuilder.NewLabel("Text", box, string.Empty, BoxFontSize);
            UiBuilder.Stretch(_box.rectTransform);

            _status = UiBuilder.NewLabel("Status", left, string.Empty, StatusFontSize);
            TopRow(_status.rectTransform, StatusTop, StatusHeight);

            RectTransform back = UiBuilder.NewButton("Back", left, _catalog.Text(PhraseKey.ButtonBack), BackFontSize, () => onBack());
            back.anchorMin = back.anchorMax = new Vector2(0.5f, 0f);
            back.pivot = new Vector2(0.5f, 0f);
            back.sizeDelta = new Vector2(BackWidth, BackHeight);
            back.anchoredPosition = new Vector2(0f, BackBottomMargin);

            RectTransform pad = NumberPad.Build(panel, _catalog, Press, out _pad);
            pad.anchorMin = pad.anchorMax = new Vector2((1f + TaskAreaRight) / 2f, 0.5f);
            pad.pivot = new Vector2(0.5f, 0.5f);
            pad.anchoredPosition = Vector2.zero;
        }

        private static void TopRow(RectTransform row, float top, float height)
        {
            row.anchorMin = new Vector2(0f, 1f);
            row.anchorMax = new Vector2(1f, 1f);
            row.pivot = new Vector2(0.5f, 1f);
            row.sizeDelta = new Vector2(0f, height);
            row.anchoredPosition = new Vector2(0f, -top);
        }

        private void Render()
        {
            bool waiting = _gate.IsWaiting;
            _wasWaiting = waiting;
            _pad.interactable = !waiting;
            // During the wait the question is hidden: the next one is asked when the wait is over.
            _question.text = waiting ? string.Empty : _gate.Challenge.Text(_catalog);
            _box.text = _gate.Entered.HasValue ? _gate.Entered.Value.ToString() : string.Empty;
            if (waiting)
            {
                _status.text = _catalog.Text(
                    PhraseKey.GateWait, new System.Collections.Generic.Dictionary<string, string> { { "Seconds", _gate.SecondsLeft.ToString() } });
            }
            else
            {
                _status.text = _last == GateResult.Wrong || _last == GateResult.Locked ? _catalog.Text(PhraseKey.GateWrong) : string.Empty;
            }
        }
    }
}
