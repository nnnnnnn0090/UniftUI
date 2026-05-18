using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Draws a non-layout-affecting outline around a single child.</summary>
    public class BorderElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private Color borderColor;
        private readonly float borderWidth;
        private Outline builtOutline;
        private Image builtImage;

        public BorderElement(UIElement content, Color color, float width)
        {
            this.content = content;
            borderColor = color;
            borderWidth = Mathf.Max(0f, width);
            CopyFrameFrom(content);
        }

        public BorderElement(UIElement content, State<Color> color, float width)
            : this(content, color != null ? color.Value : Color.clear, width)
        {
            if (color != null)
            {
                AddPropertyBinding(color, () =>
                {
                    SetBorderColor(color.Value);
                }, "borderColor", BindingKind.Visual);
            }
        }

        public BorderElement SetBorderColor(Color color)
        {
            borderColor = color;
            if (builtOutline != null)
                builtOutline.effectColor = color;
            return this;
        }

        public void AddChild(UIElement child)
        {
            SingleChildContainerUtility.Add(ref content, child, nameof(BorderElement));
        }

        public void RemoveChild(UIElement child)
        {
            SingleChildContainerUtility.Remove(ref content, child);
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            SingleChildContainerUtility.Replace(ref content, oldChild, newChild);
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return SingleChildContainerUtility.Children(content);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject container = CreateElementRoot("BorderContainer", parent);
            builtImage = AddImage(
                container,
                backgroundColor != Color.clear ? backgroundColor : new Color(1f, 1f, 1f, 0f),
                false);

            builtOutline = container.AddComponent<Outline>();
            builtOutline.effectColor = borderColor;
            builtOutline.effectDistance = new Vector2(borderWidth, -borderWidth);
            builtOutline.useGraphicAlpha = false;

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

            ApplyAllEffects(container, builtImage);
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
