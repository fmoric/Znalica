using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The Računanje do 20 screen: the task with its answer box, the number pad, the feedback in place and, at the end
    /// of a round, the round-end panel. It draws an <see cref="ArithmeticPlay"/> and passes pad buttons and keys to it
    /// as <see cref="AnswerKey"/>; every rule is in the play. The widgets are built in code under the given parent.
    /// </summary>
    public sealed class ArithmeticScreen : MonoBehaviour
    {
        private const float TaskAreaRight = 0.62f;
        private const float EquationTop = 70f;
        private const float EquationHeight = 200f;
        private const float EquationFontSize = 150f;
        private const float TokenWidthBase = 60f;
        private const float TokenWidthPerCharacter = 75f;
        private const float TokenSpacing = 20f;
        private const float FeedbackTop = 320f;
        private const float FeedbackHeight = 140f;
        private const float FeedbackFontSize = 60f;
        private const float LineTop = 480f;
        private const float LineHeight = 260f;
        private const float LineBarY = 110f;
        private const float LineBarHeight = 14f;
        private const float LineChangeY = 140f;
        private const float LineChangeFontSize = 56f;
        private const float NextTop = 750f;
        private const float WideButtonWidth = 380f;
        private const float WideButtonHeight = 130f;
        private const float WideButtonFontSize = 56f;
        private const float ReplayBottomMargin = 60f;

        private static readonly Color BoxColor = Color.white;
        private static readonly Color JumpColor = new Color(0.95f, 0.62f, 0.2f);

        private PhraseCatalog _catalog;
        private VoicePlayer _voice;
        private IItemSet _items;
        private IClock _clock;
        private IProblemLog _problems;
        private IAnswerLog _log;
        private ItemSchedule _schedule;
        private int _sessionNumber;
        private Action _onStop;
        private ArithmeticPlay _play;

        private RectTransform _equation;
        private TextMeshProUGUI _feedback;
        private RectTransform _line;
        private RectTransform _next;
        private CanvasGroup _pad;
        private RectTransform _taskGroup;
        private RoundEndPanel _roundEnd;

        public RectTransform Panel => (RectTransform)transform;

        /// <param name="onStop">Called when the child leaves the round-end screen with Stop; the app goes back to the menu.</param>
        public static ArithmeticScreen Create(
            RectTransform parent,
            PhraseCatalog catalog,
            VoicePlayer voice,
            IItemSet items,
            IClock clock,
            IProblemLog problems,
            Action onStop)
        {
            RectTransform panel = UiBuilder.NewChild("ArithmeticScreen", parent);
            ArithmeticScreen screen = panel.gameObject.AddComponent<ArithmeticScreen>();
            screen._catalog = catalog;
            screen._voice = voice;
            screen._items = items;
            screen._clock = clock;
            screen._problems = problems;
            screen._onStop = onStop;
            screen.Build();
            return screen;
        }

        /// <summary>
        /// Starts the picked profile's session on this screen: its answers go to <paramref name="log"/> under
        /// <paramref name="sessionNumber"/>, and what the log says of earlier sessions decides what is asked. Plans the first round and shows its first item; call before the screen is shown.
        /// </summary>
        public void Begin(IAnswerLog log, int sessionNumber)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _sessionNumber = sessionNumber;
            _schedule = ItemSchedule.Build(log.ReadAll());
            BeginRound();
        }

        /// <summary>Plans a new round of ten items and shows its first.</summary>
        private void BeginRound()
        {
            var random = new SeededRandom(Environment.TickCount);
            var round = new RoundSession(
                RoundPlanner.Plan(_items, _schedule, _sessionNumber, random), _log, _schedule, _clock, _sessionNumber, Guid.NewGuid().ToString());
            _play = new ArithmeticPlay(round, _clock);
            Render();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || _play == null)
            {
                return;
            }

            // Enter goes on after a second wrong answer, as the Dalje button does.
            foreach (AnswerKey pressed in NumberPad.KeysPressed(keyboard))
            {
                Press(pressed == AnswerKey.Confirm && _play.AwaitsContinue ? AnswerKey.Continue : pressed);
            }
        }

        private void Press(AnswerKey key)
        {
            if (key == AnswerKey.Continue)
            {
                // The explanation of the item just left must not go on speaking over the next one.
                _voice.Stop();
            }

            Feedback feedback = null;
            if (!DiskWrite.TryRun(() => feedback = _play.Press(key), _problems, "save the answer"))
            {
                // The answer was not kept and nothing moved: the child can press U redu again.
                Render();
                _feedback.text = _catalog.Text(PhraseKey.SaveFailed);
                return;
            }

            if (feedback != null)
            {
                _voice.Play(_catalog.VoiceClips(feedback.Phrase, feedback.Args));
            }

            Render();
        }

        private void Build()
        {
            RectTransform panel = Panel;
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            _taskGroup = UiBuilder.NewChild("Task", panel);
            UiBuilder.Stretch(_taskGroup);

            RectTransform left = UiBuilder.NewChild("TaskArea", _taskGroup);
            left.anchorMin = Vector2.zero;
            left.anchorMax = new Vector2(TaskAreaRight, 1f);
            left.offsetMin = left.offsetMax = Vector2.zero;

            _equation = UiBuilder.TopRow("Equation", left, EquationTop, EquationHeight);
            UiBuilder.CentreRow(_equation, TokenSpacing);

            RectTransform feedbackRow = UiBuilder.TopRow("Feedback", left, FeedbackTop, FeedbackHeight);
            _feedback = UiBuilder.NewLabel("Text", feedbackRow, string.Empty, FeedbackFontSize);
            UiBuilder.Stretch(_feedback.rectTransform);

            _line = UiBuilder.TopRow("NumberLine", left, LineTop, LineHeight);

            _next = UiBuilder.NewButton(
                "Next", left, _catalog.Text(PhraseKey.ButtonNext), WideButtonFontSize, () => Press(AnswerKey.Continue));
            UiBuilder.CentreTop(_next, NextTop, WideButtonWidth, WideButtonHeight);

            RectTransform replay = UiBuilder.NewButton(
                "Replay", left, _catalog.Text(PhraseKey.ButtonReplay), WideButtonFontSize, _voice.ReplayLast);
            replay.anchorMin = replay.anchorMax = new Vector2(0.5f, 0f);
            replay.pivot = new Vector2(0.5f, 0f);
            replay.sizeDelta = new Vector2(WideButtonWidth, WideButtonHeight);
            replay.anchoredPosition = new Vector2(0f, ReplayBottomMargin);

            RectTransform pad = NumberPad.Build(_taskGroup, _catalog, Press, out _pad);
            pad.anchorMin = pad.anchorMax = new Vector2((1f + TaskAreaRight) / 2f, 0.5f);
            pad.pivot = new Vector2(0.5f, 0.5f);
            pad.anchoredPosition = Vector2.zero;
            _roundEnd = new RoundEndPanel(panel, _catalog, BeginRound, () => _onStop());
        }

        /// <summary>Draws everything from the play's current state.</summary>
        private void Render()
        {
            bool ended = _play.IsEnded;
            _taskGroup.gameObject.SetActive(!ended);
            _roundEnd.SetActive(ended);
            if (ended)
            {
                _roundEnd.SetScore( _catalog.Text(PhraseKey.RoundEndScore, _play.SummaryArgs));
                return;
            }

            RenderEquation();
            Feedback feedback = _play.Feedback;
            _feedback.text = feedback == null ? string.Empty : _catalog.Text(feedback.Phrase, feedback.Args);
            UiBuilder.Clear(_line);
            if (feedback != null && feedback.Explanation != null)
            {
                DrawJump(feedback.Explanation);
            }

            _next.gameObject.SetActive(_play.AwaitsContinue);
            _pad.interactable = !_play.AwaitsContinue;
        }

        private void RenderEquation()
        {
            UiBuilder.Clear(_equation);
            foreach (ItemToken token in ItemFormatter.Tokens(_play.CurrentItem))
            {
                string text = token.IsUnknown ? (_play.Entered.HasValue ? _play.Entered.Value.ToString() : string.Empty) : token.Text;
                RectTransform cell = UiBuilder.NewChild("Token", _equation);
                // The answer box keeps the width of two digits, so it does not change size while the child types.
                int width = token.IsUnknown ? 2 : text.Length;
                cell.sizeDelta = new Vector2(TokenWidthBase + TokenWidthPerCharacter * width, EquationHeight);
                if (token.IsUnknown)
                {
                    cell.gameObject.AddComponent<Image>().color = BoxColor;
                }

                TextMeshProUGUI label = UiBuilder.NewLabel("Text", cell, text, EquationFontSize);
                UiBuilder.Stretch(label.rectTransform);
            }
        }

        /// <summary>
        /// The jump on the 0-20 line: every whole number labelled, the start and the landing marked, and a bar from one
        /// to the other with the size of the jump above it. Drawn at once, with no animation.
        /// </summary>
        private void DrawJump(Explanation explanation)
        {
            NumberLineDrawing.Draw(_line, number =>
                number == explanation.End ? NumberLineDrawing.EndColor
                : number == explanation.Start ? NumberLineDrawing.StartColor
                : (Color?)null);

            float startX = NumberLineDrawing.X(explanation.Start);
            float endX = NumberLineDrawing.X(explanation.End);
            RectTransform bar = UiBuilder.NewChild("Jump", _line);
            bar.anchorMin = bar.anchorMax = new Vector2(0.5f, 0f);
            bar.pivot = new Vector2(0.5f, 0f);
            bar.sizeDelta = new Vector2(Mathf.Max(Mathf.Abs(endX - startX), LineBarHeight), LineBarHeight);
            bar.anchoredPosition = new Vector2((startX + endX) / 2f, LineBarY);
            bar.gameObject.AddComponent<Image>().color = JumpColor;

            TextMeshProUGUI change = UiBuilder.NewLabel("Change", _line, ItemFormatter.ChangeText(explanation), LineChangeFontSize);
            change.rectTransform.anchorMin = change.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            change.rectTransform.pivot = new Vector2(0.5f, 0f);
            change.rectTransform.sizeDelta = new Vector2(240f, 80f);
            change.rectTransform.anchoredPosition = new Vector2((startX + endX) / 2f, LineChangeY);
        }
    }
}
