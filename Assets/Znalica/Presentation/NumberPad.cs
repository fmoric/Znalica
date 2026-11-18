using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Znalica.Domain;

namespace Znalica.Presentation
{
    /// <summary>
    /// The on-screen number pad (digits, Obriši, U redu) and the keys that stand for its buttons. The arithmetic screen
    /// and the parent gate both enter numbers this way; what a button does is the caller's, through one callback.
    /// </summary>
    internal static class NumberPad
    {
        private const float PadWidth = 640f;
        private const float PadHeight = 730f;
        private const float CellWidth = 190f;
        private const float CellHeight = 160f;
        private const float Spacing = 25f;
        private const float DigitFontSize = 90f;
        private const float WordFontSize = 44f;

        // Written out, not computed: the Input System's digit keys are not in numeric order (Digit0 follows Digit9).
        private static readonly KeyValuePair<Key, AnswerKey>[] KeyMap =
        {
            new KeyValuePair<Key, AnswerKey>(Key.Digit0, AnswerKey.Digit0),
            new KeyValuePair<Key, AnswerKey>(Key.Digit1, AnswerKey.Digit1),
            new KeyValuePair<Key, AnswerKey>(Key.Digit2, AnswerKey.Digit2),
            new KeyValuePair<Key, AnswerKey>(Key.Digit3, AnswerKey.Digit3),
            new KeyValuePair<Key, AnswerKey>(Key.Digit4, AnswerKey.Digit4),
            new KeyValuePair<Key, AnswerKey>(Key.Digit5, AnswerKey.Digit5),
            new KeyValuePair<Key, AnswerKey>(Key.Digit6, AnswerKey.Digit6),
            new KeyValuePair<Key, AnswerKey>(Key.Digit7, AnswerKey.Digit7),
            new KeyValuePair<Key, AnswerKey>(Key.Digit8, AnswerKey.Digit8),
            new KeyValuePair<Key, AnswerKey>(Key.Digit9, AnswerKey.Digit9),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad0, AnswerKey.Digit0),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad1, AnswerKey.Digit1),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad2, AnswerKey.Digit2),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad3, AnswerKey.Digit3),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad4, AnswerKey.Digit4),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad5, AnswerKey.Digit5),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad6, AnswerKey.Digit6),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad7, AnswerKey.Digit7),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad8, AnswerKey.Digit8),
            new KeyValuePair<Key, AnswerKey>(Key.Numpad9, AnswerKey.Digit9),
            new KeyValuePair<Key, AnswerKey>(Key.Backspace, AnswerKey.Erase),
            new KeyValuePair<Key, AnswerKey>(Key.Enter, AnswerKey.Confirm),
            new KeyValuePair<Key, AnswerKey>(Key.NumpadEnter, AnswerKey.Confirm),
        };

        /// <summary>
        /// The pad under the parent, sized but not placed: the caller anchors it. The returned group makes the whole pad
        /// uninteractable when the caller wants it frozen.
        /// </summary>
        public static RectTransform Build(RectTransform parent, PhraseCatalog catalog, Action<AnswerKey> press, out CanvasGroup group)
        {
            RectTransform pad = UiBuilder.NewChild("Pad", parent);
            pad.sizeDelta = new Vector2(PadWidth, PadHeight);
            group = pad.gameObject.AddComponent<CanvasGroup>();
            GridLayoutGroup grid = pad.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(CellWidth, CellHeight);
            grid.spacing = new Vector2(Spacing, Spacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.MiddleCenter;

            for (int digit = 1; digit <= 9; digit++)
            {
                Digit(pad, digit, press);
            }

            UiBuilder.NewButton("Erase", pad, catalog.Text(PhraseKey.ButtonErase), WordFontSize, () => press(AnswerKey.Erase));
            Digit(pad, 0, press);
            UiBuilder.NewButton("Ok", pad, catalog.Text(PhraseKey.ButtonOk), WordFontSize, () => press(AnswerKey.Confirm));
            return pad;
        }

        /// <summary>
        /// The keys pressed this frame. At most one Confirm (Enter and numpad Enter together are one press), so a caller
        /// never handles a double Confirm in one frame.
        /// </summary>
        public static IEnumerable<AnswerKey> KeysPressed(Keyboard keyboard)
        {
            bool confirmed = false;
            foreach (KeyValuePair<Key, AnswerKey> mapping in KeyMap)
            {
                if (!keyboard[mapping.Key].wasPressedThisFrame)
                {
                    continue;
                }

                if (mapping.Value == AnswerKey.Confirm)
                {
                    if (confirmed)
                    {
                        continue;
                    }

                    confirmed = true;
                }

                yield return mapping.Value;
            }
        }

        private static void Digit(RectTransform pad, int digit, Action<AnswerKey> press)
        {
            AnswerKey key = AnswerKey.Digit0 + digit;
            UiBuilder.NewButton("Digit" + digit, pad, digit.ToString(), DigitFontSize, () => press(key));
        }
    }
}
