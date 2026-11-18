using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The Veće, manje, jednako screen: two quantities (numbers, or groups in ten-frames) with the three sign buttons, or
    /// three to five numbers to click in order; the feedback in place with its picture; and the round-end panel. It
    /// draws a <see cref="ComparisonPlay"/> and passes clicks to it; every rule is in the play. The widgets are built in
    /// code under the given parent, and the task and the controls are drawn again from the play's state after every click.
    /// </summary>
    public sealed class ComparisonScreen : MonoBehaviour
    {
        private const float TaskTop = 30f;
        private const float TaskHeight = 340f;
        private const float ControlsTop = 380f;
        private const float ControlsHeight = 140f;
        private const float FeedbackTop = 530f;
        private const float FeedbackHeight = 110f;
        private const float FeedbackFontSize = 60f;
        private const float PictureTop = 650f;
        private const float PictureHeight = 230f;
        private const float WideButtonWidth = 380f;
        private const float WideButtonHeight = 130f;
        private const float WideButtonFontSize = 56f;
        private const float BottomMargin = 60f;
        private const float BottomButtonOffset = 250f;

        private const float QuantityWidth = 360f;
        private const float QuantityHeight = 320f;
        private const float QuantityNumberFontSize = 200f;
        private const float TaskCell = 56f;
        private const float SignBoxSize = 200f;
        private const float SignBoxFontSize = 130f;
        private const float RowSpacing = 60f;

        private const float SignButtonWidth = 280f;
        private const float SignButtonHeight = 140f;
        private const float SignButtonFontSize = 110f;
        private const float SignButtonSpacing = 40f;

        private const float InstructionHeight = 70f;
        private const float InstructionFontSize = 48f;
        private const float TilesTop = 90f;
        private const float TilesHeight = 230f;
        private const float TileSize = 200f;
        private const float TileFontSize = 120f;
        private const float SlotSize = 120f;
        private const float SlotFontSize = 70f;
        private const float UndoWidth = 280f;
        private const float UndoHeight = 120f;
        private const float UndoFontSize = 44f;

        private const float PictureCell = 38f;
        private const float PictureNumberFontSize = 130f;
        private const float PictureSignWidth = 160f;
        private const float PictureCountHeight = 56f;
        private const float PictureCountFontSize = 48f;

        private static readonly Color BoxColor = Color.white;
        private static readonly Color EmptySlotColor = new Color(0.9f, 0.9f, 0.84f);
        private static readonly Color LeftObjectColor = new Color(0.35f, 0.55f, 0.9f);
        private static readonly Color RightObjectColor = new Color(0.95f, 0.55f, 0.2f);
        private static readonly Sign[] Signs = { Sign.Less, Sign.Greater, Sign.Equal };

        private PhraseCatalog _catalog;
        private VoicePlayer _voice;
        private ComparisonItemSet _items;
        private IClock _clock;
        private IProblemLog _problems;
        private IAnswerLog _log;
        private ItemSchedule _schedule;
        private int _sessionNumber;
        private Action _onStop;
        private ComparisonPlay _play;

        private RectTransform _taskGroup;
        private RectTransform _task;
        private RectTransform _controls;
        private TextMeshProUGUI _feedback;
        private RectTransform _picture;
        private RectTransform _next;
        private CanvasGroup _taskInput;
        private CanvasGroup _controlsInput;
        private RoundEndPanel _roundEnd;

        public RectTransform Panel => (RectTransform)transform;

        /// <param name="onStop">Called when the child leaves the round-end screen with Stop; the app goes back to the menu.</param>
        public static ComparisonScreen Create(
            RectTransform parent,
            PhraseCatalog catalog,
            VoicePlayer voice,
            ComparisonItemSet items,
            IClock clock,
            IProblemLog problems,
            Action onStop)
        {
            RectTransform panel = UiBuilder.NewChild("ComparisonScreen", parent);
            ComparisonScreen screen = panel.gameObject.AddComponent<ComparisonScreen>();
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
                ComparisonRoundPlanner.Plan(_items, _schedule, _sessionNumber, random),
                _log,
                _schedule,
                _clock,
                _sessionNumber,
                Guid.NewGuid().ToString());
            _play = new ComparisonPlay(round, _clock);
            Render();
        }

        private void GiveAnswer(Func<Feedback> click)
        {
            Feedback feedback = null;
            if (!DiskWrite.TryRun(() => feedback = click(), _problems, "save the answer"))
            {
                // The answer was not kept and nothing moved: the child can click again.
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

        private void Continue()
        {
            // The explanation of the item just left must not go on speaking over the next one.
            _voice.Stop();
            _play.Continue();
            Render();
        }

        private void Undo()
        {
            _play.UndoPick();
            Render();
        }

        private void Build()
        {
            RectTransform panel = Panel;
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            _taskGroup = UiBuilder.NewChild("Task", panel);
            UiBuilder.Stretch(_taskGroup);
            _task = UiBuilder.TopRow("Item", _taskGroup, TaskTop, TaskHeight);
            _taskInput = _task.gameObject.AddComponent<CanvasGroup>();
            _controls = UiBuilder.TopRow("Controls", _taskGroup, ControlsTop, ControlsHeight);
            _controlsInput = _controls.gameObject.AddComponent<CanvasGroup>();

            RectTransform feedbackRow = UiBuilder.TopRow("Feedback", _taskGroup, FeedbackTop, FeedbackHeight);
            _feedback = UiBuilder.NewLabel("Text", feedbackRow, string.Empty, FeedbackFontSize);
            UiBuilder.Stretch(_feedback.rectTransform);
            _picture = UiBuilder.TopRow("Picture", _taskGroup, PictureTop, PictureHeight);

            _next = BottomButton("Next", _catalog.Text(PhraseKey.ButtonNext), BottomButtonOffset, Continue);
            BottomButton("Replay", _catalog.Text(PhraseKey.ButtonReplay), -BottomButtonOffset, _voice.ReplayLast);
            _roundEnd = new RoundEndPanel(panel, _catalog, BeginRound, () => _onStop());
        }

        private RectTransform BottomButton(string name, string text, float x, Action onClick)
        {
            RectTransform button = UiBuilder.NewButton(name, _taskGroup, text, WideButtonFontSize, () => onClick());
            button.anchorMin = button.anchorMax = new Vector2(0.5f, 0f);
            button.pivot = new Vector2(0.5f, 0f);
            button.sizeDelta = new Vector2(WideButtonWidth, WideButtonHeight);
            button.anchoredPosition = new Vector2(x, BottomMargin);
            return button;
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

            UiBuilder.Clear(_task);
            UiBuilder.Clear(_controls);
            UiBuilder.Clear(_picture);
            ComparisonItem item = _play.CurrentItem;
            if (item.Task == ComparisonTask.ChooseSign)
            {
                DrawSignTask(item);
                DrawSignButtons();
            }
            else
            {
                DrawOrderTask(item);
                DrawOrderSlots(item);
            }

            Feedback feedback = _play.Feedback;
            _feedback.text = feedback == null ? string.Empty : _catalog.Text(feedback.Phrase, feedback.Args);
            if (feedback != null && feedback.Explanation != null)
            {
                DrawExplanation(feedback.Explanation);
            }

            _next.gameObject.SetActive(_play.AwaitsContinue);
            _taskInput.interactable = _controlsInput.interactable = !_play.AwaitsContinue;
        }

        private void DrawSignTask(ComparisonItem item)
        {
            RectTransform row = SideRow(_task);
            Quantity(row, item.Numbers[0], item.Representation, LeftObjectColor, TaskCell, QuantityNumberFontSize, QuantityWidth, QuantityHeight, false);
            RectTransform box = UiBuilder.NewChild("Sign", row);
            box.sizeDelta = new Vector2(SignBoxSize, SignBoxSize);
            box.gameObject.AddComponent<Image>().color = BoxColor;
            UiBuilder.Stretch(UiBuilder.NewLabel("Mark", box, ArithmeticItem.UnknownMark, SignBoxFontSize).rectTransform);
            Quantity(row, item.Numbers[1], item.Representation, RightObjectColor, TaskCell, QuantityNumberFontSize, QuantityWidth, QuantityHeight, false);
        }

        private void DrawSignButtons()
        {
            RectTransform row = SideRow(_controls, SignButtonSpacing);
            foreach (Sign sign in Signs)
            {
                Sign chosen = sign;
                RectTransform button = UiBuilder.NewButton(
                    sign.ToString(), row, ItemFormatter.SignText(sign), SignButtonFontSize, () => GiveAnswer(() => _play.ChooseSign(chosen)));
                button.sizeDelta = new Vector2(SignButtonWidth, SignButtonHeight);
            }
        }

        private void DrawOrderTask(ComparisonItem item)
        {
            RectTransform instruction = UiBuilder.NewLabel(
                "Instruction", UiBuilder.TopRow("InstructionRow", _task, 0f, InstructionHeight), _catalog.Text(PhraseKey.InstructionOrder), InstructionFontSize).rectTransform;
            UiBuilder.Stretch(instruction);

            RectTransform tiles = SideRow(UiBuilder.TopRow("Tiles", _task, TilesTop, TilesHeight));
            foreach (int number in item.ShownNumbers)
            {
                int chosen = number;
                RectTransform tile = UiBuilder.NewButton(
                    "Number" + number, tiles, number.ToString(), TileFontSize, () => GiveAnswer(() => _play.PickNumber(chosen)));
                tile.sizeDelta = new Vector2(TileSize, TileSize);
                tile.GetComponent<Button>().interactable = !_play.Picked.Contains(number);
            }
        }

        /// <summary>Empty places for the numbers still to click, the ones clicked so far in them, and Obriši.</summary>
        private void DrawOrderSlots(ComparisonItem item)
        {
            RectTransform row = SideRow(_controls, SignButtonSpacing);
            for (int index = 0; index < item.Numbers.Count; index++)
            {
                bool filled = index < _play.Picked.Count;
                RectTransform slot = UiBuilder.NewChild("Slot", row);
                slot.sizeDelta = new Vector2(SlotSize, SlotSize);
                slot.gameObject.AddComponent<Image>().color = filled ? BoxColor : EmptySlotColor;
                if (filled)
                {
                    UiBuilder.Stretch(UiBuilder.NewLabel("Number", slot, _play.Picked[index].ToString(), SlotFontSize).rectTransform);
                }
            }

            RectTransform undo = UiBuilder.NewButton("Undo", row, _catalog.Text(PhraseKey.ButtonErase), UndoFontSize, Undo);
            undo.sizeDelta = new Vector2(UndoWidth, UndoHeight);
        }

        /// <summary>
        /// Side by side: both quantities with the right sign between them, groups in ten-frames with their counts under
        /// them; or the numbers of an ordering item marked on the 0-20 line. Drawn at once, with no animation.
        /// </summary>
        private void DrawExplanation(Explanation explanation)
        {
            if (explanation.Kind == ExplanationKind.SideBySide)
            {
                RectTransform row = SideRow(_picture);
                float width = Mathf.Max(PictureCell * 5f, PictureNumberFontSize * 2f);
                Quantity(row, explanation.Left, explanation.Representation, LeftObjectColor, PictureCell, PictureNumberFontSize, width, PictureHeight, true);
                RectTransform sign = UiBuilder.NewChild("Sign", row);
                sign.sizeDelta = new Vector2(PictureSignWidth, PictureHeight);
                string text = ItemFormatter.SignText(ComparisonItem.SignOf(explanation.Left, explanation.Right));
                UiBuilder.Stretch(UiBuilder.NewLabel("Mark", sign, text, PictureNumberFontSize).rectTransform);
                Quantity(row, explanation.Right, explanation.Representation, RightObjectColor, PictureCell, PictureNumberFontSize, width, PictureHeight, true);
                return;
            }

            if (explanation.Kind == ExplanationKind.OrderedSequence)
            {
                NumberLineDrawing.Draw(_picture, number => explanation.Numbers.Contains(number) ? NumberLineDrawing.EndColor : (Color?)null);
            }
        }

        private static RectTransform SideRow(RectTransform parent, float spacing = RowSpacing)
        {
            RectTransform row = UiBuilder.NewChild("Row", parent);
            UiBuilder.Stretch(row);
            UiBuilder.CentreRow(row, spacing);
            return row;
        }

        /// <summary>
        /// One quantity: its number, or its objects in ten-frames (with the count under them when
        /// <paramref name="countUnderGroup"/>; the child counts them in the task, so there it is left out).
        /// </summary>
        private static void Quantity(
            RectTransform row,
            int value,
            Representation representation,
            Color objectColor,
            float cell,
            float numberFontSize,
            float width,
            float height,
            bool countUnderGroup)
        {
            RectTransform side = UiBuilder.NewChild("Quantity", row);
            side.sizeDelta = new Vector2(width, height);
            if (representation == Representation.Numbers)
            {
                UiBuilder.Stretch(UiBuilder.NewLabel("Number", side, value.ToString(), numberFontSize).rectTransform);
                return;
            }

            RectTransform group = TenFrameDrawing.Draw(side, value, cell, objectColor);
            group.anchorMin = group.anchorMax = new Vector2(0.5f, 1f);
            group.pivot = new Vector2(0.5f, 1f);
            group.anchoredPosition = Vector2.zero;
            if (countUnderGroup)
            {
                TextMeshProUGUI count = UiBuilder.NewLabel("Count", side, value.ToString(), PictureCountFontSize);
                count.rectTransform.anchorMin = new Vector2(0f, 0f);
                count.rectTransform.anchorMax = new Vector2(1f, 0f);
                count.rectTransform.pivot = new Vector2(0.5f, 0f);
                count.rectTransform.sizeDelta = new Vector2(0f, PictureCountHeight);
                count.rectTransform.anchoredPosition = Vector2.zero;
            }
        }
    }
}
