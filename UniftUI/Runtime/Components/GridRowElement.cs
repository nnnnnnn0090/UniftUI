using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>SwiftUI の <c>GridRow</c> に相当。親の <see cref="GridElement"/> のコンテンツ内でのみ使用してください。</summary>
    public class GridRowElement : UIElement, ILayoutContainer
    {
        private readonly List<UIElement> children = new List<UIElement>();
        private readonly float horizontalSpacing;
        private readonly HStackAlignment rowAlignment;

        public GridRowElement(GridElement parentGrid, Action content)
        {
            if (parentGrid == null)
            {
                Debug.LogWarning("GridRow は Grid のコンテンツ内（GridRow(() => ...)）でのみ使用してください。");
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
                Debug.LogWarning($"ReplaceChild: GridRow に oldChild が見つかりません。Children: {children.Count}");
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

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            switch (rowAlignment)
            {
                case HStackAlignment.Top:
                    layout.childAlignment = TextAnchor.UpperCenter;
                    break;
                case HStackAlignment.Center:
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    break;
                case HStackAlignment.Bottom:
                    layout.childAlignment = TextAnchor.LowerCenter;
                    break;
                case HStackAlignment.FirstTextBaseline:
                case HStackAlignment.LastTextBaseline:
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    break;
            }

            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.spacing = horizontalSpacing;
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);

            LayoutElement layoutElement = container.AddComponent<LayoutElement>();
            if (preferredWidth >= 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                layoutElement.flexibleWidth = 1;
                layoutElement.minWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }

            if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
            }

            foreach (var child in children)
                child.Build(container.transform);

            ApplyAllEffects(container, backgroundImage);

            BaselineRowAligner.AlignIfNeeded(container, rowAlignment);

            return container;
        }
    }
}
