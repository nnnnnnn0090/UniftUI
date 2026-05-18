using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>
    /// Overlays child views on top of each other.
    /// </summary>
    public class ZStackElement : UIElement, ILayoutContainer
    {
        private Action content;
        private State[] states;
        private List<UIElement> children = new List<UIElement>();
        private ZStackAlignment alignment;

        /// <summary>Creates a <see cref="ZStackElement"/> with the given content builder and optional state dependencies.</summary>
        public ZStackElement(Action content, State[] states = null, ZStackAlignment alignment = ZStackAlignment.Center)
        {
            this.content = content;
            this.states = states;
            this.alignment = alignment;
            MaterializeContent(content, children);
        }

        /// <inheritdoc />
        public void AddChild(UIElement child)
        {
            if (child != null) children.Add(child);
        }

        /// <inheritdoc />
        public void RemoveChild(UIElement child)
        {
            if (child != null) children.Remove(child);
        }

        /// <inheritdoc />
        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;
            int index = children.IndexOf(oldChild);
            if (index != -1)
                children[index] = newChild;
            else
                Debug.LogWarning($"[UniftUI] ReplaceChild: oldChild not found in ZStack. Old: {oldChild}, New: {newChild}. Children count: {children.Count}");
        }

        /// <inheritdoc />
        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        /// <inheritdoc />
        public override GameObject Build(Transform parent)
        {
            GameObject container = CreateElementRoot("ZStack", parent);
            Image backgroundImage = AddBackgroundImageIfNeeded(container);

            UniftUIZStackLayoutGroup layout = container.AddComponent<UniftUIZStackLayoutGroup>();
            layout.Configure(alignment, padding ?? new RectOffset(0, 0, 0, 0));

            LayoutElementUtility.Configure(container, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            BuildContentChildren(children, container.transform);

            SetupContentRebuildObserver(states, container, container.transform, children, content, "ZStack");

            ApplyAllEffects(container, backgroundImage);

            return container;
        }
        protected override void PropagateInfiniteWidthToContent()
        {
        }

        protected override void PropagateInfiniteHeightToContent()
        {
        }

    }

    /// <summary>Alignment options for <see cref="ZStackElement"/> children.</summary>
    public enum ZStackAlignment
    {
        TopLeading,
        Top,
        TopTrailing,
        Leading,
        Center,
        Trailing,
        BottomLeading,
        Bottom,
        BottomTrailing
    }
}
