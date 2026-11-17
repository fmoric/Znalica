using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>One picture of the activity menu: the phrase that names it, spoken and shown, and its picture.</summary>
    [Serializable]
    public struct ActivityPicture
    {
        public PhraseKey NameKey;
        public Sprite Picture;
    }

    /// <summary>
    /// The activity menu: three large pictures, each with its name shown under it; a click speaks the name. A speaker
    /// button repeats the last thing spoken. The widgets are built in code under the given parent.
    /// </summary>
    public sealed class ActivityMenuView
    {
        private const float PictureSize = 440f;
        private const float CardSpacing = 80f;
        private const float LabelHeight = 110f;
        private const float LabelFontSize = 56f;
        private const float ReplayWidth = 360f;
        private const float ReplayHeight = 120f;
        private const float ReplayBottomMargin = 80f;
        private const float ReplayFontSize = 48f;

        private readonly PhraseCatalog _catalog;
        private readonly VoicePlayer _voice;
        private readonly Action<PhraseKey> _picked;

        /// <param name="picked">Called with a picture's name key after its name has been spoken; the app opens that activity.</param>
        public ActivityMenuView(PhraseCatalog catalog, VoicePlayer voice, Action<PhraseKey> picked)
        {
            _catalog = catalog;
            _voice = voice;
            _picked = picked;
        }

        /// <returns>The menu's panel, for the navigator.</returns>
        public RectTransform Build(RectTransform parent, ActivityPicture[] pictures)
        {
            RectTransform panel = UiBuilder.NewChild("ActivityMenu", parent);
            UiBuilder.Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = UiBuilder.BackgroundColor;

            RectTransform row = UiBuilder.NewChild("Pictures", panel);
            UiBuilder.Stretch(row);
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = CardSpacing;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = false;
            rowLayout.childControlHeight = false;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            foreach (ActivityPicture picture in pictures)
            {
                BuildCard(row, picture);
            }

            BuildReplayButton(panel);
            return panel;
        }

        private void BuildCard(RectTransform row, ActivityPicture picture)
        {
            RectTransform card = UiBuilder.NewChild(picture.NameKey.ToString(), row);
            card.sizeDelta = new Vector2(PictureSize, PictureSize + LabelHeight);

            RectTransform image = UiBuilder.NewChild("Picture", card);
            image.anchorMin = image.anchorMax = new Vector2(0.5f, 1f);
            image.pivot = new Vector2(0.5f, 1f);
            image.sizeDelta = new Vector2(PictureSize, PictureSize);
            image.anchoredPosition = Vector2.zero;
            Image graphic = image.gameObject.AddComponent<Image>();
            graphic.sprite = picture.Picture;
            graphic.preserveAspect = true;
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            button.onClick.AddListener(Speak(picture.NameKey));

            TextMeshProUGUI label = UiBuilder.NewLabel("Name", card, _catalog.Text(picture.NameKey), LabelFontSize);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 0f);
            label.rectTransform.pivot = new Vector2(0.5f, 0f);
            label.rectTransform.sizeDelta = new Vector2(0f, LabelHeight);
            label.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void BuildReplayButton(RectTransform panel)
        {
            RectTransform replay = UiBuilder.NewButton("Replay", panel, _catalog.Text(PhraseKey.ButtonReplay), ReplayFontSize, _voice.ReplayLast);
            replay.anchorMin = replay.anchorMax = new Vector2(0.5f, 0f);
            replay.pivot = new Vector2(0.5f, 0f);
            replay.sizeDelta = new Vector2(ReplayWidth, ReplayHeight);
            replay.anchoredPosition = new Vector2(0f, ReplayBottomMargin);
        }

        private UnityAction Speak(PhraseKey key)
        {
            return () =>
            {
                _voice.Play(_catalog.VoiceClips(key));
                _picked?.Invoke(key);
            };
        }
    }
}
