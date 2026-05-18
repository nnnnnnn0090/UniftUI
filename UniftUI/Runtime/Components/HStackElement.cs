using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Horizontal stack layout.</summary>
    public class HStackElement : UIElement, ILayoutContainer
    {
        private Action content;
        private State[] states;
        private List<UIElement> children = new List<UIElement>();
        private float spacing;
        private HStackAlignment alignment;

        public HStackElement(Action content, State[] states = null, float spacing = 8f, HStackAlignment alignment = HStackAlignment.Center)
        {
            this.content = content;
            this.states = states;
            this.spacing = spacing;
            this.alignment = alignment;
            MaterializeContent(content, children);
        }

        public void AddChild(UIElement child)
        {
            if (child == null)
                return;

            children.Add(child);
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
            {
                children[index] = newChild;
            }
            else
            {
                SingleChildContainerUtility.LogReplaceChildNotFound(nameof(HStackElement), oldChild, newChild, children.Count);
            }
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject container = CreateElementRoot("HStack", parent);
            Image backgroundImage = AddBackgroundImageIfNeeded(container);

            UniftUIStackLayoutGroup layout = container.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Horizontal, spacing, VStackAlignment.Center, alignment);

            LayoutElementUtility.Configure(container, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            BuildContentChildren(children, container.transform);

            SetupContentRebuildObserver(
                states,
                container,
                container.transform,
                children,
                content,
                "HStack",
                afterRebuild: () => BaselineRowAligner.AlignIfNeeded(container, alignment));

            ApplyAllEffects(container, backgroundImage);

            BaselineRowAligner.AlignIfNeeded(container, alignment);

            return container;
        }
    }
}
