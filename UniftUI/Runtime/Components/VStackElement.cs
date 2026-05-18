using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Vertical stack layout.</summary>
    public class VStackElement : UIElement, ILayoutContainer
    {
        private Action content;
        private State[] states;
        private List<UIElement> children = new List<UIElement>();
        private float spacing;
        private VStackAlignment alignment;

        public VStackElement(Action content, State[] states = null, float spacing = 8f,
                     VStackAlignment alignment = VStackAlignment.Center)
        {
            this.content = content;
            this.states = states;
            this.spacing = spacing;
            this.alignment = alignment;
            MaterializeContent(content, children);
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
            {
                children.Add(child);
            }
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
            {
                children.Remove(child);
            }
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;
            int index = children.IndexOf(oldChild);
            if (index != -1)
                children[index] = newChild;
            else
                Debug.LogWarning($"[UniftUI] VStack ReplaceChild: oldChild not found. Children count: {children.Count}");
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject container = CreateElementRoot("VStack", parent);
            Image backgroundImage = AddBackgroundImageIfNeeded(container);

            UniftUIStackLayoutGroup layout = container.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Vertical, spacing, alignment, HStackAlignment.Center);

            LayoutElementUtility.Configure(container, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            BuildContentChildren(children, container.transform);

            SetupContentRebuildObserver(states, container, container.transform, children, content, "VStack");

            ApplyAllEffects(container, backgroundImage);

            return container;
        }
    }
}
