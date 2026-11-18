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
    /// The start screen: one large picture with the name under it for each profile; a click picks it. A small button leads
    /// to the parent area, where profiles are added (a child cannot add one). The widgets are built in code under the given parent.
    /// </summary>
    public sealed class ProfilePickView
    {
        private const float TitleTop = 90f;
        private const float TitleHeight = 140f;
        private const float TitleFontSize = 80f;
        private const float PictureSize = 330f;
        private const float CardSpacing = 60f;
        private const float NameHeight = 100f;
        private const float NameFontSize = 56f;
        private const float ParentWidth = 380f;
        private const float ParentHeight = 110f;
        private const float ParentMargin = 50f;
        private const float ParentFontSize = 44f;

        private readonly PhraseCatalog _catalog;
        private readonly Action<Profile> _picked;
        private readonly Action _openParentArea;
        private RectTransform _row;

        public ProfilePickView(PhraseCatalog catalog, Action<Profile> picked, Action openParentArea)
        {
            _catalog = catalog;
            _picked = picked;
            _openParentArea = openParentArea;
        }

        /// <returns>The screen's panel, for the navigator.</returns>
        public RectTransform Build(RectTransform parent)
        {
            RectTransform panel = UiBuilder.NewChild("ProfilePick", parent);
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            TextMeshProUGUI title = UiBuilder.NewLabel("Title", panel, _catalog.Text(PhraseKey.ProfilePickTitle), TitleFontSize);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.sizeDelta = new Vector2(0f, TitleHeight);
            title.rectTransform.anchoredPosition = new Vector2(0f, -TitleTop);

            _row = UiBuilder.NewChild("Profiles", panel);
            UiBuilder.Stretch(_row);
            HorizontalLayoutGroup layout = _row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = CardSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            RectTransform parentArea = UiBuilder.NewButton(
                "ParentArea", panel, _catalog.Text(PhraseKey.ButtonParentArea), ParentFontSize, () => _openParentArea());
            parentArea.anchorMin = parentArea.anchorMax = new Vector2(1f, 0f);
            parentArea.pivot = new Vector2(1f, 0f);
            parentArea.sizeDelta = new Vector2(ParentWidth, ParentHeight);
            parentArea.anchoredPosition = new Vector2(-ParentMargin, ParentMargin);
            return panel;
        }

        /// <summary>Draws the given profiles, replacing what was shown. Call before the screen is shown.</summary>
        public void Refresh(IReadOnlyList<Profile> profiles)
        {
            for (int i = _row.childCount - 1; i >= 0; i--)
            {
                GameObject child = _row.GetChild(i).gameObject;
                child.SetActive(false);
                UnityEngine.Object.Destroy(child);
            }

            foreach (Profile profile in profiles)
            {
                BuildCard(profile);
            }
        }

        private void BuildCard(Profile profile)
        {
            RectTransform card = UiBuilder.NewChild(profile.ProfileId, _row);
            card.sizeDelta = new Vector2(PictureSize, PictureSize + NameHeight);

            RectTransform picture = UiBuilder.NewPictureTile("Picture", card, profile.PictureId, PictureSize);
            picture.anchorMin = picture.anchorMax = new Vector2(0.5f, 1f);
            picture.pivot = new Vector2(0.5f, 1f);
            picture.anchoredPosition = Vector2.zero;
            var button = picture.gameObject.AddComponent<Button>();
            button.targetGraphic = picture.GetComponent<Image>();
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Profile picked = profile;
            button.onClick.AddListener(() => _picked(picked));

            TextMeshProUGUI name = UiBuilder.NewLabel("Name", card, profile.DisplayName, NameFontSize);
            name.rectTransform.anchorMin = new Vector2(0f, 0f);
            name.rectTransform.anchorMax = new Vector2(1f, 0f);
            name.rectTransform.pivot = new Vector2(0.5f, 0f);
            name.rectTransform.sizeDelta = new Vector2(0f, NameHeight);
            name.rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
