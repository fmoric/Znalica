using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Presentation
{
    /// <summary>
    /// The parent area's progress screen for one profile: a column for each activity with the rounds played, the share
    /// answered right at the first try, the items still missed and the practice of the last six weeks. Everything is
    /// worked out by <see cref="ProgressCalculator"/> when the screen is opened; nothing is stored. Text only, no
    /// colours that rank or praise: a parent reads numbers, not a score.
    /// </summary>
    public sealed class ProgressView
    {
        private const float TitleTop = 40f;
        private const float TitleHeight = 100f;
        private const float TitleFontSize = 64f;
        private const float ColumnsTop = 150f;
        private const float ColumnsBottom = 170f;
        private const float ColumnsSide = 60f;
        private const float ColumnSpacing = 30f;
        private const float ColumnPadding = 24f;
        private const float ColumnItemSpacing = 4f;
        private const float HeadingFontSize = 44f;
        private const float BodyFontSize = 30f;
        private const float SmallFontSize = 26f;
        private const float BackWidth = 380f;
        private const float BackHeight = 110f;
        private const float BackMargin = 50f;
        private const float BackFontSize = 48f;
        private const int PercentFull = 100;
        private const string WeekDateFormat = "dd.MM.";

        private static readonly Dictionary<Activity, PhraseKey> ActivityNames = new Dictionary<Activity, PhraseKey>
        {
            { Activity.Arithmetic, PhraseKey.ActivityNameArithmetic },
            { Activity.Comparison, PhraseKey.ActivityNameComparison },
            { Activity.NumberLine, PhraseKey.ActivityNameNumberLine },
        };

        private readonly PhraseCatalog _catalog;
        private readonly IClock _clock;
        private readonly Dictionary<Activity, IItemSet> _itemSets = new Dictionary<Activity, IItemSet>();
        private readonly Action _back;

        private TextMeshProUGUI _title;
        private RectTransform _columns;

        /// <param name="itemSets">The item sets of the three activities, to turn an item key into the text shown for it.</param>
        /// <param name="back">Called by the Natrag button.</param>
        public ProgressView(PhraseCatalog catalog, IClock clock, IEnumerable<IItemSet> itemSets, Action back)
        {
            _catalog = catalog;
            _clock = clock;
            _back = back;
            foreach (IItemSet set in itemSets)
            {
                _itemSets[set.Activity] = set;
            }
        }

        /// <summary>The screen's panel, for the navigator.</summary>
        public RectTransform Build(RectTransform parent)
        {
            RectTransform panel = UiBuilder.NewChild("Progress", parent);
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            _title = UiBuilder.NewLabel("Title", panel, string.Empty, TitleFontSize);
            _title.rectTransform.anchorMin = new Vector2(0f, 1f);
            _title.rectTransform.anchorMax = new Vector2(1f, 1f);
            _title.rectTransform.pivot = new Vector2(0.5f, 1f);
            _title.rectTransform.sizeDelta = new Vector2(0f, TitleHeight);
            _title.rectTransform.anchoredPosition = new Vector2(0f, -TitleTop);

            _columns = UiBuilder.NewChild("Columns", panel);
            _columns.anchorMin = Vector2.zero;
            _columns.anchorMax = Vector2.one;
            _columns.offsetMin = new Vector2(ColumnsSide, ColumnsBottom);
            _columns.offsetMax = new Vector2(-ColumnsSide, -ColumnsTop);
            HorizontalLayoutGroup layout = _columns.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = ColumnSpacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            RectTransform back = UiBuilder.NewButton("Back", panel, _catalog.Text(PhraseKey.ButtonBack), BackFontSize, () => _back());
            back.anchorMin = back.anchorMax = new Vector2(0f, 0f);
            back.pivot = new Vector2(0f, 0f);
            back.sizeDelta = new Vector2(BackWidth, BackHeight);
            back.anchoredPosition = new Vector2(BackMargin, BackMargin);
            return panel;
        }

        /// <summary>Works the profile's progress out from its answer log and draws it. Call before the screen is shown.</summary>
        public void Begin(Profile profile, IAnswerLog log)
        {
            _title.text = _catalog.Text(PhraseKey.ProgressTitle, new Dictionary<string, string> { { "Name", profile.DisplayName } });
            IReadOnlyList<AnswerRecord> records = log.ReadAll();
            IReadOnlyList<ProgressReport> reports = ProgressCalculator.For(records, ItemSchedule.Build(records), _clock.Now);

            UiBuilder.Clear(_columns);
            foreach (ProgressReport report in reports)
            {
                BuildColumn(report);
            }
        }

        private void BuildColumn(ProgressReport report)
        {
            RectTransform column = UiBuilder.NewChild(report.Activity.ToString(), _columns);
            column.gameObject.AddComponent<Image>().color = Color.white;
            VerticalLayoutGroup layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset((int)ColumnPadding, (int)ColumnPadding, (int)ColumnPadding, (int)ColumnPadding);
            layout.spacing = ColumnItemSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddLine(column, _catalog.Text(ActivityNames[report.Activity]), HeadingFontSize);
            if (report.RoundsPlayed == 0)
            {
                AddLine(column, _catalog.Text(PhraseKey.ProgressNoAnswers), BodyFontSize);
                return;
            }

            AddLine(column, _catalog.Text(PhraseKey.ProgressRounds, Args("Count", report.RoundsPlayed.ToString(CultureInfo.InvariantCulture))), BodyFontSize);
            AddLine(column, _catalog.Text(PhraseKey.ProgressFirstTimeRight, Args("Percent", PercentText(report.FirstTimeRightShare))), BodyFontSize);

            AddLine(column, _catalog.Text(PhraseKey.ProgressStillMissedTitle), BodyFontSize).fontStyle = FontStyles.Bold;
            if (report.StillMissed.Count == 0)
            {
                AddLine(column, _catalog.Text(PhraseKey.ProgressNoneMissed), BodyFontSize);
            }

            foreach (ItemKey key in report.StillMissed)
            {
                AddLine(column, ItemText(report.Activity, key), BodyFontSize);
            }

            AddLine(column, _catalog.Text(PhraseKey.ProgressWeeksTitle), BodyFontSize).fontStyle = FontStyles.Bold;
            foreach (WeekPractice week in report.WeeklyPractice)
            {
                AddLine(column, WeekText(week), SmallFontSize);
            }
        }

        /// <summary>The item as the child saw it; a key the activity's set cannot build (written by another version) is shown as it is.</summary>
        private string ItemText(Activity activity, ItemKey key)
        {
            return _itemSets.TryGetValue(activity, out IItemSet set) && set.TryFind(key, out Item item)
                ? ItemFormatter.Text(item)
                : key.ToString();
        }

        private string WeekText(WeekPractice week)
        {
            return _catalog.Text(
                PhraseKey.ProgressWeekRow,
                new Dictionary<string, string>
                {
                    { "Date", week.Monday.ToString(WeekDateFormat, CultureInfo.InvariantCulture) },
                    { "Rounds", week.Rounds.ToString(CultureInfo.InvariantCulture) },
                    { "Answers", week.Answers.ToString(CultureInfo.InvariantCulture) },
                });
        }

        /// <summary>The share as a whole percent; a dash when the log holds no first try, which a column that shows it does not have.</summary>
        private static string PercentText(double? share)
        {
            return share.HasValue
                ? Math.Round(share.Value * PercentFull, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture)
                : "-";
        }

        private static Dictionary<string, string> Args(string name, string value)
        {
            return new Dictionary<string, string> { { name, value } };
        }

        private static TextMeshProUGUI AddLine(RectTransform column, string text, float fontSize)
        {
            TextMeshProUGUI line = UiBuilder.NewLabel("Line", column, text, fontSize);
            line.alignment = TextAlignmentOptions.TopLeft;
            return line;
        }
    }
}
