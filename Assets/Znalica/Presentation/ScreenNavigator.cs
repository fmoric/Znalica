using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Znalica.Presentation
{
    /// <summary>Shows one screen panel at a time.</summary>
    public sealed class ScreenNavigator : MonoBehaviour
    {
        private readonly List<GameObject> _screens = new List<GameObject>();

        /// <summary>Makes the panel one of the screens that <see cref="Show"/> switches between; it starts hidden.</summary>
        public void Add(RectTransform screen)
        {
            _screens.Add(screen.gameObject);
            screen.gameObject.SetActive(false);
        }

        /// <summary>Shows the panel and hides every other one. A button of the screen just left no longer holds the keyboard.</summary>
        public void Show(RectTransform screen)
        {
            foreach (GameObject other in _screens)
            {
                other.SetActive(other == screen.gameObject);
            }

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}
