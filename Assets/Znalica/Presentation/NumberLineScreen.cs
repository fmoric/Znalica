using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The Brojevna crta screen: the task and what to do with it, the feedback in place with its picture and, at the end of a
    /// round, the round-end panel. A number to place is shown with the 0-20 line, which the child clicks; the number before or
    /// after one, and where a jump lands, are typed on the number pad as in arithmetic. It draws a <see cref="NumberLinePlay"/>
    /// and passes clicks, pad buttons and keys to it; every rule is in the play. The widgets are built in code under the given
    /// parent, and the task is drawn again from the play's state after every answer.
    /// </summary>
    public sealed class NumberLineScreen : MonoBehaviour
    {
        private const float TaskAreaRightWithPad = 0.62f;
        private const float InstructionTop = 10f;
        private const float InstructionHeight = 70f;
        private const float InstructionFontSize = 44f;
        private const float EquationTop = 90f;
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

        // The line a number is placed on has labels at these numbers only; the child counts the ticks to the rest.
        private const int LabelEvery = 10;

        private PhraseCatalog _catalog;
        private VoicePlayer _voice;
        private IItemSet _items;
        private IClock _clock;
        private SessionClock _session;
        private IProblemLog _problems;
        private IAnswerLog _log;
        private ItemSchedule _schedule;
        private int _sessionNumber;
        private Action _onStop;
        private NumberLinePlay _play;

        private RectTransform _taskArea;
        private TextMeshProUGUI _instruction;
        private RectTransform _equation;
        private TextMeshProUGUI _feedback;
        private RectTransform _line;
        private RectTransform _next;
        private RectTransform _pad;
        private CanvasGroup _padInput;
        private RectTransform _taskGroup;
        private RoundEndPanel _roundEnd;

        public RectTransform Panel => (RectTransform)transform;

        /// <param name="onStop">Called when the child leaves the round-end screen with Stop; the app goes back to the menu.</param>
        public static NumberLineScreen Create(
            RectTransform parent,
            PhraseCatalog catalog,
            VoicePlayer voice,
            IItemSet items,
            IClock clock,
            SessionClock session,
            IProblemLog problems,
            Action onStop)
        {
            RectTransform panel = UiBuilder.NewChild("NumberLineScreen", parent);
            NumberLineScreen screen = panel.gameObject.AddComponent<NumberLineScreen>();
            screen._catalog = catalog;
            screen._voice = voice;
            screen._items = items;
            screen._clock = clock;
            screen._session = session;
            screen._problems = problems;
            screen._onStop = onStop;
            screen.Build();
            return screen;
        }

        /// <summary>
        /// Starts the picked profile's session on this screen: its answers go to <paramref name="log"/> under
        /// <paramref name="sessionNumber"/>, and what the log says of earlier sessions decides what is asked. Plans the
        /// first round and shows its first item; call before the screen is shown.
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
            _play = new NumberLinePlay(round, _clock);
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

            Give(() => _play.Press(key));
        }

        private void PlaceAtClick(float xFromCentre)
        {
            Give(() => _play.PlaceAt(NumberLineDrawing.NumberAt(xFromCentre)));
        }

        private void Give(Func<Feedback> give)
        {
            if (!AnswerGiving.TryGive(give, _problems, _catalog, _voice))
            {
                // The answer was not kept and nothing moved: the child can give it again.
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

            _taskArea = UiBuilder.NewChild("TaskArea", _taskGroup);
            _taskArea.anchorMin = Vector2.zero;
            _taskArea.offsetMin = _taskArea.offsetMax = Vector2.zero;

            RectTransform instructionRow = UiBuilder.TopRow("InstructionRow", _taskArea, InstructionTop, InstructionHeight);
            _instruction = UiBuilder.NewLabel("Instruction", instructionRow, string.Empty, InstructionFontSize);
            UiBuilder.Stretch(_instruction.rectTransform);

            _equation = EquationDrawing.NewRow(_taskArea, EquationTop);

            RectTransform feedbackRow = UiBuilder.TopRow("Feedback", _taskArea, FeedbackTop, FeedbackHeight);
            _feedback = UiBuilder.NewLabel("Text", feedbackRow, string.Empty, FeedbackFontSize);
            UiBuilder.Stretch(_feedback.rectTransform);

            // The line is where a number is placed: a click anywhere on it, on a tick or between, is read back to a number.
            _line = UiBuilder.TopRow("NumberLine", _taskArea, LineTop, LineHeight);
            _line.gameObject.AddComponent<Image>().color = Color.clear;
            _line.gameObject.AddComponent<LineClickArea>().Clicked = PlaceAtClick;

            _next = UiBuilder.NewButton(
                "Next", _taskArea, _catalog.Text(PhraseKey.ButtonNext), WideButtonFontSize, () => Press(AnswerKey.Continue));
            UiBuilder.CentreTop(_next, NextTop, WideButtonWidth, WideButtonHeight);

            RectTransform replay = UiBuilder.NewButton(
                "Replay", _taskArea, _catalog.Text(PhraseKey.ButtonReplay), WideButtonFontSize, _voice.ReplayLast);
            replay.anchorMin = replay.anchorMax = new Vector2(0.5f, 0f);
            replay.pivot = new Vector2(0.5f, 0f);
            replay.sizeDelta = new Vector2(WideButtonWidth, WideButtonHeight);
            replay.anchoredPosition = new Vector2(0f, ReplayBottomMargin);

            _pad = NumberPad.Build(_taskGroup, _catalog, Press, out _padInput);
            _pad.anchorMin = _pad.anchorMax = new Vector2((1f + TaskAreaRightWithPad) / 2f, 0.5f);
            _pad.pivot = new Vector2(0.5f, 0.5f);
            _pad.anchoredPosition = Vector2.zero;
            _roundEnd = new RoundEndPanel(panel, _catalog, _voice, BeginRound, () => _onStop());
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
                _roundEnd.SuggestBreak(_session.BreakDue);
                return;
            }

            NumberLineItem item = _play.CurrentItem;
            bool placing = item.Task == NumberLineTask.Place;

            // Placing is clicked on the line, which then has the whole width; the other tasks are typed, with the pad beside them.
            _taskArea.anchorMax = new Vector2(placing ? 1f : TaskAreaRightWithPad, 1f);
            _pad.gameObject.SetActive(!placing);
            _instruction.text = _catalog.Text(InstructionFor(item.Task));
            EquationDrawing.Draw(_equation, ItemFormatter.Tokens(item), _play.Entered);

            Feedback feedback = _play.Feedback;
            _feedback.text = feedback == null ? string.Empty : _catalog.Text(feedback.Phrase, feedback.Args);
            UiBuilder.Clear(_line);
            if (feedback != null && feedback.Explanation != null)
            {
                DrawExplanation(feedback.Explanation, item);
            }
            else if (placing)
            {
                NumberLineDrawing.Draw(_line, number => null, number => number % LabelEvery == 0);
            }

            _next.gameObject.SetActive(_play.AwaitsContinue);
            _padInput.interactable = !_play.AwaitsContinue;
        }

        /// <summary>
        /// A jump: the jump on the line. Placing, before and after: the right place marked, with the number the task named,
        /// and for placing the child's own click, so the two can be compared. Drawn at once, with no animation.
        /// </summary>
        private void DrawExplanation(Explanation explanation, NumberLineItem item)
        {
            if (explanation.Kind == ExplanationKind.NumberLineJump)
            {
                NumberLineDrawing.DrawJump(_line, explanation);
            }
            else if (explanation.Kind == ExplanationKind.PositionOnLine)
            {
                NumberLineDrawing.DrawPosition(_line, explanation, item.Task == NumberLineTask.Place ? _play.LastPlaced : null);
            }
        }

        private static PhraseKey InstructionFor(NumberLineTask task)
        {
            switch (task)
            {
                case NumberLineTask.Before: return PhraseKey.InstructionBefore;
                case NumberLineTask.After: return PhraseKey.InstructionAfter;
                case NumberLineTask.Jump: return PhraseKey.InstructionJump;
                default: return PhraseKey.InstructionPlace;
            }
        }
    }
}
