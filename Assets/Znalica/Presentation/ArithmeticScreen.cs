using System;
using TMPro;
using UnityEngine;
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
        private const float FeedbackTop = 320f;
        private const float FeedbackHeight = 140f;
        private const float FeedbackFontSize = 60f;
        private const float LineTop = 480f;
        private const float LineHeight = 260f;
        private const float NextTop = 750f;
        private const float WideButtonWidth = 380f;
        private const float WideButtonHeight = 130f;
        private const float WideButtonFontSize = 56f;
        private const float ReplayBottomMargin = 60f;

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
            if (_play != null)
            {
                NumberPad.HandleKeys(() => _play.AwaitsContinue, Press);
            }
        }

        private void Press(AnswerKey key)
        {
            if (key == AnswerKey.Continue)
            {
                // The explanation of the item just left must not go on speaking over the next one.
                _voice.Stop();
            }

            if (!AnswerGiving.TryGive(() => _play.Press(key), _problems, _catalog, _voice))
            {
                // The answer was not kept and nothing moved: the child can press U redu again.
                Render();
                _feedback.text = _catalog.Text(PhraseKey.SaveFailed);
                return;
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

            _equation = EquationDrawing.NewRow(left, EquationTop);

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
                _roundEnd.SetScore(_catalog.Text(PhraseKey.RoundEndScore, _play.SummaryArgs));
                return;
            }

            RenderEquation();
            Feedback feedback = _play.Feedback;
            _feedback.text = feedback == null ? string.Empty : _catalog.Text(feedback.Phrase, feedback.Args);
            UiBuilder.Clear(_line);
            if (feedback != null && feedback.Explanation != null)
            {
                NumberLineDrawing.DrawJump(_line, feedback.Explanation);
            }

            _next.gameObject.SetActive(_play.AwaitsContinue);
            _pad.interactable = !_play.AwaitsContinue;
        }

        private void RenderEquation()
        {
            EquationDrawing.Draw(_equation, ItemFormatter.Tokens(_play.CurrentItem), _play.Entered);
        }
    }
}
