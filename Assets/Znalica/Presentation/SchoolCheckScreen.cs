using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Presentation
{
    /// <summary>
    /// The school-like check's worksheet: two pages of ten items, each with one answer box (a number) or one sign box, in
    /// sections that each have a written instruction. Plain on purpose: no picture, no sound, no hint, no feedback and no
    /// explanation. A number box is filled with the number pad or the keys, the box last clicked being the one typed into;
    /// a sign box is filled with its three buttons. Every rule is in <see cref="SchoolCheckSession"/>; this only draws it.
    /// </summary>
    public sealed class SchoolCheckScreen : MonoBehaviour
    {
        private const float TitleTop = 24f;
        private const float TitleHeight = 80f;
        private const float TitleFontSize = 52f;
        private const float PageTop = 100f;
        private const float PageHeight = 56f;
        private const float PageFontSize = 36f;
        private const float ColumnsTop = 170f;
        private const float ColumnLeft = 40f;
        private const float ColumnWidth = 580f;
        private const float ColumnSpacing = 40f;
        private const float InstructionHeight = 80f;
        private const float InstructionFontSize = 38f;
        private const float RowHeight = 96f;
        private const float RowSpacing = 6f;
        private const float RowFontSize = 56f;
        private const float TokenBase = 30f;
        private const float TokenPerCharacter = 32f;
        private const float OperatorWidth = 56f;
        private const float BoxWidth = 110f;
        private const float BoxHeight = 84f;
        private const float SignButtonWidth = 75f;
        private const float RowItemSpacing = 10f;
        private const float PadRightMargin = 20f;
        private const float PadTop = 170f;
        private const float WideButtonWidth = 380f;
        private const float WideButtonHeight = 110f;
        private const float WideButtonFontSize = 48f;
        private const float ButtonMargin = 40f;
        private const float MessageFontSize = 36f;
        private const float MessageWidth = 700f;
        private const float MessageHeight = 60f;

        private static readonly Color ActiveBoxColor = new Color(0.75f, 0.88f, 1f);
        private static readonly Sign[] Signs = { Sign.Less, Sign.Greater, Sign.Equal };

        private readonly Dictionary<int, TextMeshProUGUI> _boxTexts = new Dictionary<int, TextMeshProUGUI>();
        private readonly Dictionary<int, Image> _boxBackgrounds = new Dictionary<int, Image>();

        private PhraseCatalog _catalog;
        private IProblemLog _problems;
        private IClock _clock;
        private Action _onDone;
        private SchoolCheckSession _session;
        private int _active;

        private TextMeshProUGUI _title;
        private TextMeshProUGUI _page;
        private RectTransform _columns;
        private TextMeshProUGUI _message;
        private TextMeshProUGUI _nextLabel;

        public RectTransform Panel => (RectTransform)transform;

        /// <param name="onDone">Called when the check is finished and when Natrag leaves it; the app shows the results.</param>
        public static SchoolCheckScreen Create(RectTransform parent, PhraseCatalog catalog, IClock clock, IProblemLog problems, Action onDone)
        {
            RectTransform panel = UiBuilder.NewChild("SchoolCheckScreen", parent);
            SchoolCheckScreen screen = panel.gameObject.AddComponent<SchoolCheckScreen>();
            screen._catalog = catalog;
            screen._clock = clock;
            screen._problems = problems;
            screen._onDone = onDone;
            screen.Build();
            return screen;
        }

        /// <summary>Plans a new check for the profile and shows its first page; its lines go to <paramref name="log"/>. Call before the screen is shown.</summary>
        public void Begin(Profile profile, ICheckLog log)
        {
            _session = new SchoolCheckSession(
                SchoolCheckPlanner.Plan(new SeededRandom(Environment.TickCount)), log, _clock, Guid.NewGuid().ToString());
            _title.text = _catalog.Text(PhraseKey.CheckTitle, new Dictionary<string, string> { { "Name", profile.DisplayName } });
            ShowPage();
        }

        private void Update()
        {
            if (_session != null)
            {
                NumberPad.HandleKeys(() => false, Press);
            }
        }

        private void Build()
        {
            RectTransform panel = Panel;
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            _title = UiBuilder.NewLabel("Title", UiBuilder.TopRow("TitleRow", panel, TitleTop, TitleHeight), string.Empty, TitleFontSize);
            UiBuilder.Stretch(_title.rectTransform);
            _page = UiBuilder.NewLabel("Page", UiBuilder.TopRow("PageRow", panel, PageTop, PageHeight), string.Empty, PageFontSize);
            UiBuilder.Stretch(_page.rectTransform);

            _columns = UiBuilder.NewChild("Columns", panel);
            UiBuilder.PlaceTopLeft(_columns, ColumnLeft, ColumnsTop, 2f * ColumnWidth + ColumnSpacing, 800f);

            RectTransform pad = NumberPad.Build(panel, _catalog, Press, out _);
            pad.anchorMin = pad.anchorMax = new Vector2(1f, 1f);
            pad.pivot = new Vector2(1f, 1f);
            pad.anchoredPosition = new Vector2(-PadRightMargin, -PadTop);

            RectTransform back = UiBuilder.NewButton("Back", panel, _catalog.Text(PhraseKey.ButtonBack), WideButtonFontSize, () => _onDone());
            UiBuilder.PlaceBottom(back, 0f, ButtonMargin, ButtonMargin, WideButtonWidth, WideButtonHeight);

            RectTransform next = UiBuilder.NewButton("Next", panel, string.Empty, WideButtonFontSize, FinishPage);
            _nextLabel = next.GetComponentInChildren<TextMeshProUGUI>();
            // Under the pad, centred on it.
            UiBuilder.PlaceBottom(next, 1f, -(PadRightMargin + (pad.sizeDelta.x - WideButtonWidth) / 2f), ButtonMargin, WideButtonWidth, WideButtonHeight);

            _message = UiBuilder.NewLabel("Message", panel, string.Empty, MessageFontSize);
            _message.rectTransform.anchorMin = _message.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            _message.rectTransform.pivot = new Vector2(0.5f, 0f);
            _message.rectTransform.sizeDelta = new Vector2(MessageWidth, MessageHeight);
            _message.rectTransform.anchoredPosition = new Vector2(-100f, ButtonMargin + 25f);
        }

        /// <summary>Draws the session's current page: its sections side by side, each with its instruction and its items.</summary>
        private void ShowPage()
        {
            _message.text = string.Empty;
            _page.text = _catalog.Text(
                PhraseKey.CheckPage,
                new Dictionary<string, string>
                {
                    { "Page", (_session.Page + 1).ToString() },
                    { "Pages", _session.PageCount.ToString() },
                });
            _nextLabel.text = _catalog.Text(_session.Page < _session.PageCount - 1 ? PhraseKey.ButtonNext : PhraseKey.ButtonStop);

            UiBuilder.Clear(_columns);
            _boxTexts.Clear();
            _boxBackgrounds.Clear();
            int column = -1;
            int rowInColumn = 0;
            RectTransform current = null;
            CheckSection? section = null;
            _active = -1;
            for (int index = _session.PageStart; index < _session.PageEnd; index++)
            {
                CheckSection itemSection = SchoolCheckPlanner.SectionOf(_session.Items[index]);
                if (section != itemSection)
                {
                    section = itemSection;
                    column++;
                    rowInColumn = 0;
                    current = UiBuilder.NewChild("Section" + itemSection, _columns);
                    UiBuilder.PlaceTopLeft(current, column * (ColumnWidth + ColumnSpacing), 0f, ColumnWidth, 780f);
                    TextMeshProUGUI instruction = UiBuilder.NewLabel("Instruction", current, _catalog.Text(InstructionOf(itemSection)), InstructionFontSize);
                    instruction.alignment = TextAlignmentOptions.MidlineLeft;
                    UiBuilder.PlaceTopLeft(instruction.rectTransform, 0f, 0f, ColumnWidth, InstructionHeight);
                }

                RectTransform row = UiBuilder.NewChild("Item" + index, current);
                UiBuilder.PlaceTopLeft(row, 0f, InstructionHeight + rowInColumn * (RowHeight + RowSpacing), ColumnWidth, RowHeight);
                BuildRow(row, index);
                rowInColumn++;
                if (_active < 0 && !_session.IsSignItem(index))
                {
                    _active = index;
                }
            }

            Refresh();
        }

        private void BuildRow(RectTransform row, int index)
        {
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = RowItemSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            Item item = _session.Items[index];
            if (item is ComparisonItem comparison)
            {
                Token(row, comparison.Numbers[0].ToString());
                AddBox(row, index, null);
                Token(row, comparison.Numbers[1].ToString());
                foreach (Sign sign in Signs)
                {
                    Sign chosen = sign;
                    RectTransform button = UiBuilder.NewButton(
                        sign.ToString(), row, ItemFormatter.SignText(sign), RowFontSize, () => Choose(index, chosen));
                    button.sizeDelta = new Vector2(SignButtonWidth, BoxHeight);
                }

                return;
            }

            foreach (ItemToken token in ItemFormatter.Tokens(item))
            {
                if (token.IsUnknown)
                {
                    AddBox(row, index, () => Select(index));
                }
                else
                {
                    Token(row, token.Text);
                }
            }
        }

        private static void Token(RectTransform row, string text)
        {
            RectTransform cell = UiBuilder.NewChild("Token", row);
            bool isNumber = char.IsDigit(text[0]);
            cell.sizeDelta = new Vector2(isNumber ? TokenBase + TokenPerCharacter * text.Length : OperatorWidth, BoxHeight);
            UiBuilder.Stretch(UiBuilder.NewLabel("Text", cell, text, RowFontSize).rectTransform);
        }

        /// <summary>The white box an answer is written in; a number box can be clicked to be the one typed into, a sign box only shows its sign.</summary>
        private void AddBox(RectTransform row, int index, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform box = UiBuilder.NewButton("Box", row, string.Empty, RowFontSize, onClick ?? (() => { }));
            box.sizeDelta = new Vector2(BoxWidth, BoxHeight);
            _boxTexts[index] = box.GetComponentInChildren<TextMeshProUGUI>();
            _boxBackgrounds[index] = box.GetComponent<Image>();
        }

        private void Select(int index)
        {
            _active = index;
            Refresh();
        }

        private void Choose(int index, Sign sign)
        {
            _session.Choose(index, sign);
            Refresh();
        }

        private void Press(AnswerKey key)
        {
            if (key == AnswerKey.Confirm)
            {
                SelectNextNumberBox();
            }
            else if (_active >= 0)
            {
                _session.Press(_active, key);
            }

            Refresh();
        }

        private void SelectNextNumberBox()
        {
            for (int index = _active + 1; index < _session.PageEnd; index++)
            {
                if (!_session.IsSignItem(index))
                {
                    _active = index;
                    return;
                }
            }
        }

        private void Refresh()
        {
            foreach (KeyValuePair<int, TextMeshProUGUI> box in _boxTexts)
            {
                int index = box.Key;
                if (_session.IsSignItem(index))
                {
                    box.Value.text = _session.Chosen(index).HasValue ? ItemFormatter.SignText(_session.Chosen(index).Value) : string.Empty;
                }
                else
                {
                    box.Value.text = _session.Entered(index).HasValue ? _session.Entered(index).Value.ToString() : string.Empty;
                }

                _boxBackgrounds[index].color = index == _active ? ActiveBoxColor : Color.white;
            }
        }

        private void FinishPage()
        {
            if (!DiskWrite.TryRun(_session.FinishPage, _problems, "save the check"))
            {
                // What was written stays; pressing the button again writes only what is missing.
                _message.text = _catalog.Text(PhraseKey.SaveFailed);
                return;
            }

            if (_session.IsFinished)
            {
                _onDone();
                return;
            }

            ShowPage();
        }

        private static PhraseKey InstructionOf(CheckSection section)
        {
            switch (section)
            {
                case CheckSection.Result: return PhraseKey.CheckInstructionResult;
                case CheckSection.MissingNumber: return PhraseKey.CheckInstructionMissing;
                case CheckSection.Sign: return PhraseKey.CheckInstructionSign;
                default: return PhraseKey.CheckInstructionNeighbour;
            }
        }
    }
}
