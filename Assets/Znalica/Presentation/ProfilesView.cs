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
    /// The parent area's profiles and settings screen: the list of profiles with Rename and Delete (the delete asks once
    /// more, since it removes all of a child's progress), a form that adds a profile or renames the one being edited, and
    /// the voice volume. Everything is stored through the <see cref="ProfileStore"/> and the settings through the callback.
    /// </summary>
    public sealed class ProfilesView
    {
        private const float TitleTop = 40f;
        private const float TitleHeight = 100f;
        private const float TitleFontSize = 64f;
        private const float ListLeft = 60f;
        private const float ListTop = 170f;
        private const float ListWidth = 900f;
        private const float RowHeight = 130f;
        private const float RowSpacing = 14f;
        private const float RowPictureSize = 110f;
        private const float RowNameFontSize = 48f;
        private const float RowButtonWidth = 250f;
        private const float RowButtonFontSize = 34f;
        private const float FormLeft = 1020f;
        private const float FormWidth = 840f;
        private const float FormTop = 170f;
        private const float FieldHeight = 110f;
        private const float FieldFontSize = 52f;
        private const float PickSize = 120f;
        private const float PickSpacing = 14f;
        private const float MessageFontSize = 36f;
        private const float SaveWidth = 380f;
        private const float SaveHeight = 110f;
        private const float SaveFontSize = 52f;
        private const float VolumeTop = 760f;
        private const float VolumeStepButton = 110f;
        private const float VolumeFontSize = 52f;
        private const float BackWidth = 380f;
        private const float BackHeight = 110f;
        private const float BackMargin = 50f;
        private const float BackFontSize = 48f;
        private const float ConfirmFontSize = 56f;
        private const float ConfirmButtonWidth = 300f;
        private const float ConfirmButtonHeight = 130f;
        private const float VolumeStep = 0.1f;
        private const int PercentFull = 100;

        private static readonly Color SelectedColor = new Color(0.12f, 0.12f, 0.12f);

        private readonly PhraseCatalog _catalog;
        private readonly ProfileStore _store;
        private readonly Action<float> _volumeChanged;
        private readonly Action _back;

        private RectTransform _list;
        private TMP_InputField _name;
        private TextMeshProUGUI _message;
        private TextMeshProUGUI _volumeText;
        private RectTransform _confirm;
        private TextMeshProUGUI _confirmText;
        private readonly List<Image> _pictureFrames = new List<Image>();
        private string _selectedPicture = ProfilePictures.Ids[0];
        private string _editing;
        private string _deleting;
        private float _volume;

        /// <param name="volumeChanged">Called with the new volume (0.0 to 1.0) each time the parent changes it; the app plays and stores it.</param>
        /// <param name="back">Called by the Natrag button.</param>
        public ProfilesView(PhraseCatalog catalog, ProfileStore store, Action<float> volumeChanged, Action back)
        {
            _catalog = catalog;
            _store = store;
            _volumeChanged = volumeChanged;
            _back = back;
        }

        /// <returns>The screen's panel, for the navigator.</returns>
        public RectTransform Build(RectTransform parent)
        {
            RectTransform panel = UiBuilder.NewChild("Profiles", parent);
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            TextMeshProUGUI title = UiBuilder.NewLabel("Title", panel, _catalog.Text(PhraseKey.ProfilesTitle), TitleFontSize);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.sizeDelta = new Vector2(0f, TitleHeight);
            title.rectTransform.anchoredPosition = new Vector2(0f, -TitleTop);

            _list = UiBuilder.NewChild("List", panel);
            _list.anchorMin = new Vector2(0f, 0f);
            _list.anchorMax = new Vector2(0f, 1f);
            _list.pivot = new Vector2(0f, 1f);
            _list.offsetMax = new Vector2(ListLeft + ListWidth, -ListTop);
            _list.offsetMin = new Vector2(ListLeft, BackHeight + 2f * BackMargin);
            VerticalLayoutGroup layout = _list.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = RowSpacing;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            BuildForm(panel);
            BuildVolume(panel);

            RectTransform back = UiBuilder.NewButton("Back", panel, _catalog.Text(PhraseKey.ButtonBack), BackFontSize, () => _back());
            back.anchorMin = back.anchorMax = new Vector2(0f, 0f);
            back.pivot = new Vector2(0f, 0f);
            back.sizeDelta = new Vector2(BackWidth, BackHeight);
            back.anchoredPosition = new Vector2(BackMargin, BackMargin);

            BuildConfirm(panel);
            return panel;
        }

        /// <summary>Redraws the list and clears the form and any open question. Call before the screen is shown.</summary>
        public void Begin(float volume)
        {
            _volume = VoiceVolume.Clamp(volume);
            ShowVolume();
            ResetForm();
            _confirm.gameObject.SetActive(false);
            RefreshList();
        }

        private void BuildForm(RectTransform panel)
        {
            _name = UiBuilder.NewInputField("Name", panel, _catalog.Text(PhraseKey.ProfileNamePlaceholder), FieldFontSize, ProfileName.MaxLength);
            Place(_name.GetComponent<RectTransform>(), FormLeft, FormTop, FormWidth, FieldHeight);

            RectTransform pictures = UiBuilder.NewChild("Pictures", panel);
            Place(pictures, FormLeft, FormTop + FieldHeight + 30f, FormWidth, PickSize + 20f);
            for (int i = 0; i < ProfilePictures.Ids.Count; i++)
            {
                BuildPictureChoice(pictures, ProfilePictures.Ids[i], i);
            }

            _message = UiBuilder.NewLabel("Message", panel, string.Empty, MessageFontSize);
            _message.alignment = TextAlignmentOptions.MidlineLeft;
            Place(_message.rectTransform, FormLeft, FormTop + FieldHeight + PickSize + 70f, FormWidth, 60f);

            RectTransform save = UiBuilder.NewButton("Save", panel, _catalog.Text(PhraseKey.ButtonSave), SaveFontSize, Save);
            Place(save, FormLeft, FormTop + FieldHeight + PickSize + 150f, SaveWidth, SaveHeight);
        }

        private void BuildPictureChoice(RectTransform parent, string pictureId, int index)
        {
            RectTransform cell = UiBuilder.NewChild("Choice" + pictureId, parent);
            cell.anchorMin = cell.anchorMax = new Vector2(0f, 0.5f);
            cell.pivot = new Vector2(0f, 0.5f);
            cell.sizeDelta = new Vector2(PickSize + 12f, PickSize + 12f);
            cell.anchoredPosition = new Vector2(index * (PickSize + 12f + PickSpacing), 0f);
            Image frame = cell.gameObject.AddComponent<Image>();
            frame.color = SelectedColor;
            _pictureFrames.Add(frame);

            RectTransform tile = UiBuilder.NewPictureTile("Picture", cell, pictureId, PickSize);
            tile.anchorMin = tile.anchorMax = new Vector2(0.5f, 0.5f);
            tile.anchoredPosition = Vector2.zero;
            var button = tile.gameObject.AddComponent<Button>();
            button.targetGraphic = tile.GetComponent<Image>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() =>
            {
                // A rename changes the name only: the picture of the profile being renamed stays as it was.
                if (_editing == null)
                {
                    _selectedPicture = pictureId;
                    ShowSelectedPicture();
                }
            });
        }

        private void BuildVolume(RectTransform panel)
        {
            TextMeshProUGUI label = UiBuilder.NewLabel("VolumeLabel", panel, _catalog.Text(PhraseKey.VolumeLabel), VolumeFontSize);
            label.alignment = TextAlignmentOptions.MidlineLeft;
            Place(label.rectTransform, FormLeft, VolumeTop, 300f, VolumeStepButton);

            RectTransform down = UiBuilder.NewButton("VolumeDown", panel, "-", VolumeFontSize, () => ChangeVolume(-VolumeStep));
            Place(down, FormLeft + 320f, VolumeTop, VolumeStepButton, VolumeStepButton);
            _volumeText = UiBuilder.NewLabel("VolumeValue", panel, string.Empty, VolumeFontSize);
            Place(_volumeText.rectTransform, FormLeft + 320f + VolumeStepButton + 10f, VolumeTop, 200f, VolumeStepButton);
            RectTransform up = UiBuilder.NewButton("VolumeUp", panel, "+", VolumeFontSize, () => ChangeVolume(VolumeStep));
            Place(up, FormLeft + 320f + VolumeStepButton + 220f, VolumeTop, VolumeStepButton, VolumeStepButton);
        }

        private void BuildConfirm(RectTransform panel)
        {
            _confirm = UiBuilder.NewChild("ConfirmDelete", panel);
            UiBuilder.Stretch(_confirm);
            _confirm.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            _confirmText = UiBuilder.NewLabel("Question", _confirm, string.Empty, ConfirmFontSize);
            _confirmText.rectTransform.anchorMin = _confirmText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            _confirmText.rectTransform.sizeDelta = new Vector2(1500f, 300f);
            _confirmText.rectTransform.anchoredPosition = new Vector2(0f, 120f);

            RectTransform yes = UiBuilder.NewButton("Yes", _confirm, _catalog.Text(PhraseKey.ButtonYes), ConfirmFontSize, ConfirmDelete);
            ConfirmButton(yes, -(ConfirmButtonWidth + 60f) / 2f);
            RectTransform no = UiBuilder.NewButton("No", _confirm, _catalog.Text(PhraseKey.ButtonNo), ConfirmFontSize, () => _confirm.gameObject.SetActive(false));
            ConfirmButton(no, (ConfirmButtonWidth + 60f) / 2f);
            _confirm.gameObject.SetActive(false);
        }

        private static void ConfirmButton(RectTransform button, float x)
        {
            button.anchorMin = button.anchorMax = new Vector2(0.5f, 0.5f);
            button.sizeDelta = new Vector2(ConfirmButtonWidth, ConfirmButtonHeight);
            button.anchoredPosition = new Vector2(x, -120f);
        }

        /// <summary>Anchors the rectangle to the panel's top left, at the given offset and size.</summary>
        private static void Place(RectTransform rect, float left, float top, float width, float height)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(width, height);
            rect.anchoredPosition = new Vector2(left, -top);
        }

        private void RefreshList()
        {
            for (int i = _list.childCount - 1; i >= 0; i--)
            {
                GameObject child = _list.GetChild(i).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }

            foreach (Profile profile in _store.List())
            {
                BuildRow(profile);
            }
        }

        private void BuildRow(Profile profile)
        {
            RectTransform row = UiBuilder.NewChild(profile.ProfileId, _list);
            row.sizeDelta = new Vector2(0f, RowHeight);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = RowHeight;

            RectTransform picture = UiBuilder.NewPictureTile("Picture", row, profile.PictureId, RowPictureSize);
            picture.anchorMin = picture.anchorMax = new Vector2(0f, 0.5f);
            picture.pivot = new Vector2(0f, 0.5f);
            picture.anchoredPosition = Vector2.zero;

            TextMeshProUGUI name = UiBuilder.NewLabel("Name", row, profile.DisplayName, RowNameFontSize);
            name.alignment = TextAlignmentOptions.MidlineLeft;
            name.rectTransform.anchorMin = Vector2.zero;
            name.rectTransform.anchorMax = Vector2.one;
            name.rectTransform.offsetMin = new Vector2(RowPictureSize + 20f, 0f);
            name.rectTransform.offsetMax = new Vector2(-(2f * RowButtonWidth + 30f), 0f);

            Profile target = profile;
            RectTransform rename = UiBuilder.NewButton("Rename", row, _catalog.Text(PhraseKey.ButtonRename), RowButtonFontSize, () => StartRename(target));
            RowButton(rename, -(RowButtonWidth + 10f));
            RectTransform delete = UiBuilder.NewButton("Delete", row, _catalog.Text(PhraseKey.ButtonDelete), RowButtonFontSize, () => AskDelete(target));
            RowButton(delete, 0f);
        }

        private static void RowButton(RectTransform button, float rightOffset)
        {
            button.anchorMin = button.anchorMax = new Vector2(1f, 0.5f);
            button.pivot = new Vector2(1f, 0.5f);
            button.sizeDelta = new Vector2(RowButtonWidth, RowHeight * 0.7f);
            button.anchoredPosition = new Vector2(rightOffset, 0f);
        }

        private void StartRename(Profile profile)
        {
            _editing = profile.ProfileId;
            _name.text = profile.DisplayName;
            _selectedPicture = profile.PictureId;
            _message.text = string.Empty;
            ShowSelectedPicture();
        }

        private void AskDelete(Profile profile)
        {
            _deleting = profile.ProfileId;
            _confirmText.text = _catalog.Text(PhraseKey.ConfirmDeleteProfile, new Dictionary<string, string> { { "Name", profile.DisplayName } });
            _confirm.gameObject.SetActive(true);
        }

        private void ConfirmDelete()
        {
            if (_deleting != null)
            {
                _store.Delete(_deleting);
            }

            _deleting = null;
            _confirm.gameObject.SetActive(false);
            if (_editing != null)
            {
                ResetForm();
            }

            RefreshList();
        }

        private void Save()
        {
            if (!ProfileName.TryNormalize(_name.text, out string name))
            {
                _message.text = _catalog.Text(PhraseKey.ProfileNameInvalid);
                return;
            }

            if (_editing == null)
            {
                _store.Create(name, _selectedPicture);
            }
            else
            {
                _store.Rename(_editing, name);
            }

            ResetForm();
            RefreshList();
        }

        private void ResetForm()
        {
            _editing = null;
            _name.text = string.Empty;
            _message.text = string.Empty;
            _selectedPicture = ProfilePictures.Ids[0];
            ShowSelectedPicture();
        }

        private void ShowSelectedPicture()
        {
            for (int i = 0; i < _pictureFrames.Count; i++)
            {
                _pictureFrames[i].enabled = ProfilePictures.Ids[i] == _selectedPicture;
            }
        }

        private void ChangeVolume(float step)
        {
            _volume = VoiceVolume.Clamp(Mathf.Round((_volume + step) * PercentFull) / PercentFull);
            ShowVolume();
            _volumeChanged(_volume);
        }

        private void ShowVolume()
        {
            _volumeText.text = _catalog.Text(
                PhraseKey.VolumeValue, new Dictionary<string, string> { { "Percent", Mathf.RoundToInt(_volume * PercentFull).ToString() } });
        }
    }
}
