using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Places overlay content above a base element without changing the base layout size.</summary>
    public class OverlayElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private readonly List<UIElement> overlayChildren = new List<UIElement>();
        private readonly ZStackAlignment alignment;

        public OverlayElement(UIElement content, UIElement overlay, ZStackAlignment alignment = ZStackAlignment.Center)
        {
            this.content = content;
            this.alignment = alignment;
            if (overlay != null)
                overlayChildren.Add(overlay);
            CopyFrameFrom(content);
        }

        public OverlayElement(UIElement content, Action overlay, ZStackAlignment alignment = ZStackAlignment.Center)
        {
            this.content = content;
            this.alignment = alignment;
            CopyFrameFrom(content);

            if (overlay != null)
            {
                var parentContext = UIContext.Current;
                try
                {
                    UIContext.Current = this;
                    overlay.Invoke();
                }
                finally
                {
                    UIContext.Current = parentContext;
                }
            }
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
                overlayChildren.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (content == child)
                content = null;
            else if (child != null)
                overlayChildren.Remove(child);
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null)
                return;

            if (content == oldChild)
            {
                content = newChild;
                return;
            }

            int index = overlayChildren.IndexOf(oldChild);
            if (index >= 0)
                overlayChildren[index] = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            if (content != null)
                yield return content;
            foreach (var child in overlayChildren)
            {
                if (child != null)
                    yield return child;
            }
        }

        public override GameObject Build(Transform parent)
        {
            GameObject container = new GameObject("OverlayContainer");
            container.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = container.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
                backgroundImage.raycastTarget = false;
            }

            var layoutGroup = container.AddComponent<UniftUISingleChildLayoutGroup>();
            layoutGroup.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            LayoutElementUtility.Configure(container, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            if (preferredWidth >= 0 && !infiniteWidth && ChildMayFillWidth(content))
                content?.WithInfiniteWidth();
            if (preferredHeight >= 0 && !infiniteHeight && ChildMayFillHeight(content))
                content?.WithInfiniteHeight();
            if (infiniteWidth)
                PropagateInfiniteWidthToContent();
            if (infiniteHeight)
                PropagateInfiniteHeightToContent();

            ApplyInheritedFont(content);
            content?.Build(container.transform);

            if (overlayChildren.Count > 0)
                BuildOverlayLayer(container.transform);

            ApplyAllEffects(container, backgroundImage);
            return container;
        }

        public override UIElement WithCornerRadius(float radius)
        {
            base.WithCornerRadius(radius);
            content?.WithCornerRadius(radius);
            return this;
        }

        public override UIElement WithCornerRadius(State<float> radius)
        {
            base.WithCornerRadius(radius);
            content?.WithCornerRadius(radius);
            return this;
        }

        public override UIElement WithCornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            base.WithCornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            content?.WithCornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            return this;
        }

        public override UIElement WithCornerRadius(float radius, RectCorner corners)
        {
            base.WithCornerRadius(radius, corners);
            content?.WithCornerRadius(radius, corners);
            return this;
        }

        private void BuildOverlayLayer(Transform parent)
        {
            GameObject layer = new GameObject("OverlayLayer");
            layer.transform.SetParent(parent, false);

            RectTransform rect = layer.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layoutElement = layer.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            var layoutGroup = layer.AddComponent<UniftUIZStackLayoutGroup>();
            layoutGroup.Configure(alignment, new RectOffset(0, 0, 0, 0));

            foreach (var child in overlayChildren)
            {
                if (child == null)
                    continue;
                ApplyInheritedFont(child);
                child.Build(layer.transform);
            }
        }

        protected override void PropagateInfiniteWidthToContent()
        {
            if (ChildMayFillWidth(content))
                content?.WithInfiniteWidth();
        }

        protected override void PropagateInfiniteHeightToContent()
        {
            if (ChildMayFillHeight(content))
                content?.WithInfiniteHeight();
        }
    }
}
