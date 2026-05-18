using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>
    /// Two-dimensional grid layout. Children should be <see cref="GridRowElement"/> instances.
    /// Column widths are synchronized after build via <see cref="GridColumnSynchronizer"/>.
    /// </summary>
    public class GridElement : UIElement, ILayoutContainer
    {
        private readonly Action content;
        private readonly State[] states;
        private readonly List<UIElement> children = new List<UIElement>();

        public float RowHorizontalSpacing { get; private set; }
        public float VerticalSpacing { get; private set; }
        public HStackAlignment RowAlignment { get; private set; }

        public GridElement(Action content, State[] states, float horizontalSpacing, float verticalSpacing,
            HStackAlignment rowAlignment)
        {
            this.content = content;
            this.states = states;
            RowHorizontalSpacing = horizontalSpacing;
            VerticalSpacing = verticalSpacing;
            RowAlignment = rowAlignment;
            MaterializeContent(content, children);
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
                children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
                children.Remove(child);
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;
            int index = children.IndexOf(oldChild);
            if (index != -1)
                children[index] = newChild;
            else
                SingleChildContainerUtility.LogReplaceChildNotFound(nameof(GridElement), oldChild, newChild, children.Count);
        }

        public IEnumerable<UIElement> GetChildren() => children;

        public override GameObject Build(Transform parent)
        {
            GameObject container = CreateElementRoot("Grid", parent);
            Image backgroundImage = AddBackgroundImageIfNeeded(container);

            UniftUIStackLayoutGroup layout = container.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Vertical, VerticalSpacing, VStackAlignment.Center, HStackAlignment.Center);

            LayoutElementUtility.Configure(container, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            BuildContentChildren(children, container.transform);

            SetupContentRebuildObserver(
                states,
                container,
                container.transform,
                children,
                content,
                "Grid",
                afterRebuild: () => GridColumnSynchronizer.Apply(EnsureRectTransform(container)));

            ApplyAllEffects(container, backgroundImage);

            GridColumnSynchronizer.Apply(EnsureRectTransform(container));

            return container;
        }
    }
}
