using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Znalica.Domain;
using Znalica.Storage;

namespace Znalica.Presentation
{
    /// <summary>Composition root: the one object that wires the app together.</summary>
    public sealed class AppRoot : MonoBehaviour
    {
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private VoiceLibrary _voiceLibrary;
        [SerializeField] private ActivityPicture[] _activityPictures;

        private void Awake()
        {
            IProblemLog problems = new UnityProblemLog();
            PhraseCatalog catalog = PhraseCatalogFile.Read(PhraseCatalogFile.PathIn(Application.streamingAssetsPath), problems);
            var clock = new SystemClock();

            var voice = gameObject.AddComponent<VoicePlayer>();
            voice.Initialize(_voiceLibrary, problems);

            var dataFolder = new DataFolder(Application.persistentDataPath);
            var settingsStore = new SettingsStore(dataFolder, problems);
            var profileStore = new ProfileStore(dataFolder, clock, problems);
            voice.Volume = settingsStore.Load().VoiceVolume;
            // One gate for the app's whole run, so its 30-second wait cannot be escaped by leaving the screen and coming back.
            var gate = new ParentGate(clock, new SeededRandom(Environment.TickCount));

            EnsureEventSystem();
            RectTransform canvas = NewCanvas();
            ScreenNavigator navigator = FindFirstObjectByType<ScreenNavigator>();
            if (navigator == null)
            {
                navigator = gameObject.AddComponent<ScreenNavigator>();
            }

            // The screens lead to one another, so each is declared first and filled in below.
            ProfilePickView pick = null;
            GateScreen gateScreen = null;
            ProfilesView profiles = null;
            ArithmeticScreen arithmetic = null;
            ComparisonScreen comparison = null;
            NumberLineScreen numberLine = null;
            RectTransform pickPanel = null;
            RectTransform menuPanel = null;
            RectTransform profilesPanel = null;

            // The picked profile's answer log and session number, fixed when the profile is picked.
            IAnswerLog activeLog = null;
            int activeSession = 0;

            // The start of the app, and where every way out of a session and of the parent area leads. With no profile yet,
            // the first start asks for the parent gate and then for the first profile (the profiles screen).
            void ShowStart()
            {
                activeLog = null;
                IReadOnlyList<Profile> existing = profileStore.List();
                if (existing.Count == 0)
                {
                    gateScreen.Begin();
                    navigator.Show(gateScreen.Panel);
                    return;
                }

                pick.Refresh(existing);
                navigator.Show(pickPanel);
            }

            pick = new ProfilePickView(
                catalog,
                picked =>
                {
                    activeLog = profileStore.AnswerLog(picked.ProfileId);
                    activeSession = SessionNumber.Next(activeLog.ReadAll());
                    navigator.Show(menuPanel);
                },
                () =>
                {
                    gateScreen.Begin();
                    navigator.Show(gateScreen.Panel);
                });
            pickPanel = pick.Build(canvas);

            gateScreen = GateScreen.Create(
                canvas,
                catalog,
                gate,
                () =>
                {
                    profiles.Begin(voice.Volume);
                    navigator.Show(profilesPanel);
                },
                ShowStart);

            profiles = new ProfilesView(
                catalog,
                profileStore,
                problems,
                volume =>
                {
                    voice.Volume = volume;
                    settingsStore.Save(new Settings { VoiceVolume = volume });
                },
                ShowStart);
            profilesPanel = profiles.Build(canvas);

            var menu = new ActivityMenuView(
                catalog,
                voice,
                picked =>
                {
                    if (picked == PhraseKey.ActivityNameArithmetic)
                    {
                        arithmetic.Begin(activeLog, activeSession);
                        navigator.Show(arithmetic.Panel);
                    }
                    else if (picked == PhraseKey.ActivityNameComparison)
                    {
                        comparison.Begin(activeLog, activeSession);
                        navigator.Show(comparison.Panel);
                    }
                    else if (picked == PhraseKey.ActivityNameNumberLine)
                    {
                        numberLine.Begin(activeLog, activeSession);
                        navigator.Show(numberLine.Panel);
                    }
                },
                ShowStart);
            menuPanel = menu.Build(canvas, _activityPictures);
            arithmetic = ArithmeticScreen.Create(canvas, catalog, voice, new ArithmeticItemSet(), clock, problems, () => navigator.Show(menuPanel));

            comparison = ComparisonScreen.Create(canvas, catalog, voice, new ComparisonItemSet(), clock, problems, () => navigator.Show(menuPanel));

            numberLine = NumberLineScreen.Create(canvas, catalog, voice, new NumberLineItemSet(), clock, problems, () => navigator.Show(menuPanel));

            navigator.Add(pickPanel);
            navigator.Add(gateScreen.Panel);
            navigator.Add(profilesPanel);
            navigator.Add(menuPanel);
            navigator.Add(arithmetic.Panel);
            navigator.Add(comparison.Panel);
            navigator.Add(numberLine.Panel);
            ShowStart();
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        private static RectTransform NewCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return (RectTransform)canvasObject.transform;
        }
    }
}
