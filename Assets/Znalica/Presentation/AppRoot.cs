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

            var voice = gameObject.AddComponent<VoicePlayer>();
            voice.Initialize(_voiceLibrary, problems);

            EnsureEventSystem();
            RectTransform canvas = NewCanvas();
            new ActivityMenuView(catalog, voice).Build(canvas, _activityPictures);
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
