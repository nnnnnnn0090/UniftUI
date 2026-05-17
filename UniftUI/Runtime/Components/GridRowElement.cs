using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>A row inside <see cref="GridElement"/>. Use only within a grid content closure.</summary>
    public class GridRowElement : UIElement, ILayoutContainer
    {
        private readonly List<UIElement> children = new List<UIElement>();
        private readonly float horizontalSpacing;
        private readonly HStackAlignment rowAlignment;

        public GridRowElement(GridElement parentGrid, Action content)
        {
            if (parentGrid == null)
            {
                Debug.LogWarning("[UniftUI] GridRow must be created inside a Grid content closure.");
                horizontalSpacing = 8f;
                rowAlignment = HStackAlignment.Center;
                return;
            }

            this.horizontalSpacing = parentGrid.RowHorizontalSpacing;
            this.rowAlignment = parentGrid.RowAlignment;

            var saved = UIContext.Current;
            try
            {
                UIContext.Current = this;
                content?.Invoke();
            }
            finally
            {
                UIContext.Current = saved;
            }

            parentGrid.AddChild(this);
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
                Debug.LogWarning($"[UniftUI] GridRow ReplaceChild: oldChild not found. Children count: {children.Count}");
        }

        public IEnumerable<UIElement> GetChildren() => children;

        public override GameObject Build(Transform parent)
        {
            GameObject container = new GameObject("GridRow");
            container.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = container.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            UniftUIStackLayoutGroup layout = container.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Horizontal, horizontalSpacing, VStackAlignment.Center, rowAlignment);

            LayoutElementUtility.Configure(
                container,
                preferredWidth,
                preferredHeight,
                preferredWidth < 0 || infiniteWidth,
                infiniteHeight);

            foreach (var child in children)
            {
                ApplyInheritedFont(child);
                child.Build(container.transform);
            }

            ApplyAllEffects(container, backgroundImage);

            BaselineRowAligner.AlignIfNeeded(container, rowAlignment);

            return container;
        }
    }
}
