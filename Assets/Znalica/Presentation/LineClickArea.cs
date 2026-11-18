using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Znalica.Presentation
{
    /// <summary>
    /// Reports where on a rect a mouse click fell, as the horizontal distance from the rect's centre in canvas units. The rect
    /// needs a raycast-target graphic of its own, so a click between its children reaches it; a click on a child reaches it too,
    /// since the event goes up to the nearest parent with a handler.
    /// </summary>
    internal sealed class LineClickArea : MonoBehaviour, IPointerClickHandler
    {
        public Action<float> Clicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            var rect = (RectTransform)transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out Vector2 local))
            {
                Clicked?.Invoke(local.x - rect.rect.center.x);
            }
        }
    }
}
