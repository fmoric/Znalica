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
    /// The parent area's list of a profile's school-like checks, the newest first, each with its date and its score, or
    /// marked as not finished (an unfinished check has no score). It also holds the button that starts a new check. Worked
    /// out from the check log each time the list is drawn; nothing is stored.
    /// </summary>
    public sealed class CheckResultsView
    {
        /// <summary>How many checks the list shows: the screen has no scrolling, so older ones stay in the log but are not listed.</summary>
        public const int MaxRows = 7;

        private const float TitleTop = 40f;
        private const float TitleHeight = 100f;
        private const float TitleFontSize = 64f;
        private const float ListTop = 170f;
        private const float ListSide = 120f;
        private const float RowHeight = 80f;
        private const float RowFontSize = 44f;
        private const float RowSpacing = 8f;
        private const float WideButtonWidth = 480f;
        private const float WideButtonHeight = 110f;
        private const float WideButtonFontSize = 48f;
        private const float ButtonMargin = 50f;
        private const string DateFormat = "dd.MM.yyyy. HH:mm";

        private readonly PhraseCatalog _catalog;
        private readonly Action<Profile> _startCheck;
        private readonly Action _back;

        private Profile _profile;
        private ICheckLog _log;
        private TextMeshProUGUI _title;
        private RectTransform _list;

        /// <param name="startCheck">Called with the profile when Nova provjera is pressed; the app opens the worksheet.</param>
        /// <param name="back">Called by the Natrag button.</param>
        public CheckResultsView(PhraseCatalog catalog, Action<Profile> startCheck, Action back)
        {
            _catalog = catalog;
            _startCheck = startCheck;
            _back = back;
        }

        /// <summary>The screen's panel, for the navigator.</summary>
        public RectTransform Build(RectTransform parent)
        {
            RectTransform panel = UiBuilder.NewChild("CheckResults", parent);
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            _title = UiBuilder.NewLabel("Title", UiBuilder.TopRow("TitleRow", panel, TitleTop, TitleHeight), string.Empty, TitleFontSize);
            UiBuilder.Stretch(_title.rectTransform);

            _list = UiBuilder.NewChild("List", panel);
            _list.anchorMin = new Vector2(0f, 0f);
            _list.anchorMax = new Vector2(1f, 1f);
            _list.offsetMin = new Vector2(ListSide, WideButtonHeight + 2f * ButtonMargin);
            _list.offsetMax = new Vector2(-ListSide, -ListTop);
            VerticalLayoutGroup layout = _list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = RowSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RectTransform back = UiBuilder.NewButton("Back", panel, _catalog.Text(PhraseKey.ButtonBack), WideButtonFontSize, () => _back());
            UiBuilder.PlaceBottom(back, 0f, ButtonMargin, ButtonMargin, WideButtonWidth, WideButtonHeight);
            RectTransform start = UiBuilder.NewButton(
                "NewCheck", panel, _catalog.Text(PhraseKey.ButtonNewCheck), WideButtonFontSize, () => _startCheck(_profile));
            UiBuilder.PlaceBottom(start, 1f, -ButtonMargin, ButtonMargin, WideButtonWidth, WideButtonHeight);
            return panel;
        }

        /// <summary>Reads the profile's check log and draws its list. Call before the screen is shown.</summary>
        public void Begin(Profile profile, ICheckLog log)
        {
            _profile = profile;
            _log = log;
            Reload();
        }

        /// <summary>Draws the list again from the same log, for a check that has just ended.</summary>
        public void Reload()
        {
            _title.text = _catalog.Text(PhraseKey.CheckTitle, new Dictionary<string, string> { { "Name", _profile.DisplayName } });
            UiBuilder.Clear(_list);
            IReadOnlyList<CheckResult> results = CheckHistory.From(_log.ReadAll());
            if (results.Count == 0)
            {
                AddRow(_catalog.Text(PhraseKey.CheckNoResults));
                return;
            }

            for (int i = results.Count - 1; i >= 0 && results.Count - i <= MaxRows; i--)
            {
                AddRow(RowText(results[i]));
            }
        }

        /// <summary>The check as one line: its date and its score out of the number of items, or its date and that it was not finished.</summary>
        private string RowText(CheckResult result)
        {
            var args = new Dictionary<string, string> { { "Date", result.Date.ToString(DateFormat, CultureInfo.InvariantCulture) } };
            if (!result.IsFinished)
            {
                return _catalog.Text(PhraseKey.CheckResultUnfinished, args);
            }

            args["Score"] = result.Score.Value.ToString(CultureInfo.InvariantCulture);
            args["Total"] = result.ItemCount.Value.ToString(CultureInfo.InvariantCulture);
            return _catalog.Text(PhraseKey.CheckResultFinished, args);
        }

        private void AddRow(string text)
        {
            TextMeshProUGUI row = UiBuilder.NewLabel("Row", _list, text, RowFontSize);
            row.alignment = TextAlignmentOptions.MidlineLeft;
            row.rectTransform.sizeDelta = new Vector2(0f, RowHeight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = RowHeight;
        }
    }
}
