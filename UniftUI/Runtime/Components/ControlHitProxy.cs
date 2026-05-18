using UnityEngine;
using UnityEngine.EventSystems;

namespace UniftUI
{
    internal abstract class ControlPointerResponder : MonoBehaviour, IPointerDownHandler,
        IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        protected ControlHitTarget target;

        public void Initialize(ControlHitTarget target)
        {
            this.target = target;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!IsPrimaryPointer(eventData) || !target.IsEnabled)
                return;

            target.SetHovered?.Invoke(true);
            target.SetPressed?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!IsPrimaryPointer(eventData))
                return;

            target.SetPressed?.Invoke(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (target.IsEnabled)
                target.SetHovered?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            target.SetHovered?.Invoke(false);
            target.SetPressed?.Invoke(false);
        }

        protected static bool IsPrimaryPointer(PointerEventData eventData)
        {
            return eventData == null || eventData.button == PointerEventData.InputButton.Left;
        }
    }

    internal sealed class ControlHitProxy : ControlPointerResponder, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsPrimaryPointer(eventData) || !target.IsEnabled)
                return;

            target.Click?.Invoke();
        }
    }

    internal sealed class ControlInteractionTracker : ControlPointerResponder
    {
    }
}
