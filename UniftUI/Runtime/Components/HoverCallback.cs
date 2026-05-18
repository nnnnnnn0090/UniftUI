using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UniftUI
{
    internal sealed class HoverCallback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private Action<bool> action;

        public void Initialize(Action<bool> action)
        {
            this.action = action;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            action?.Invoke(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            action?.Invoke(false);
        }
    }
}
