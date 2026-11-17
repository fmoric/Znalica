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

        private static readonly Color BackgroundColor = new Color(0.96f, 0.95f, 0.88f);
        private static readonly Color TextColor = new Color(0.12f, 0.12f, 0.12f);

        private readonly PhraseCatalog _catalog;
        private readonly VoicePlayer _voice;

        public ActivityMenuView(PhraseCatalog catalog, VoicePlayer voice)
        {
            _catalog = catalog;
            _voice = voice;
        }

        public void Build(RectTransform parent, ActivityPicture[] pictures)
        {
            RectTransform panel = NewChild("ActivityMenu", parent);
            Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = BackgroundColor;

            RectTransform row = NewChild("Pictures", panel);
            Stretch(row);
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
        }

        private void BuildCard(RectTransform row, ActivityPicture picture)
        {
            RectTransform card = NewChild(picture.NameKey.ToString(), row);
            card.sizeDelta = new Vector2(PictureSize, PictureSize + LabelHeight);

            RectTransform image = NewChild("Picture", card);
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

            TextMeshProUGUI label = NewLabel("Name", card, _catalog.Text(picture.NameKey), LabelFontSize);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(1f, 0f);
            label.rectTransform.pivot = new Vector2(0.5f, 0f);
            label.rectTransform.sizeDelta = new Vector2(0f, LabelHeight);
            label.rectTransform.anchoredPosition = Vector2.zero;
        }

        private void BuildReplayButton(RectTransform panel)
        {
            RectTransform replay = NewChild("Replay", panel);
            replay.anchorMin = replay.anchorMax = new Vector2(0.5f, 0f);
            replay.pivot = new Vector2(0.5f, 0f);
            replay.sizeDelta = new Vector2(ReplayWidth, ReplayHeight);
            replay.anchoredPosition = new Vector2(0f, ReplayBottomMargin);
            Image background = replay.gameObject.AddComponent<Image>();
            background.color = Color.white;
            Button button = replay.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(_voice.ReplayLast);

            TextMeshProUGUI label = NewLabel("Label", replay, _catalog.Text(PhraseKey.ButtonReplay), ReplayFontSize);
            Stretch(label.rectTransform);
        }

        private UnityAction Speak(PhraseKey key)
        {
            return () => _voice.Play(_catalog.VoiceClips(key));
        }

        private static TextMeshProUGUI NewLabel(string name, RectTransform parent, string text, float fontSize)
        {
            RectTransform rect = NewChild(name, parent);
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.color = TextColor;
            label.alignment = TextAlignmentOptions.Center;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.raycastTarget = false;
            return label;
        }

        private static RectTransform NewChild(string name, RectTransform parent)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return (RectTransform)child.transform;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
