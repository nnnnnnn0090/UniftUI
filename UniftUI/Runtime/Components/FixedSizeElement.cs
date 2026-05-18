using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>
    /// Sizes to content instead of expanding to fill the parent (<c>fixedSize</c>).
    /// On a fixed axis, prefers the child's intrinsic size over extra space from the parent.
    /// </summary>
    public class FixedSizeElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private readonly bool fixedHorizontal;
        private readonly bool fixedVertical;

        public FixedSizeElement(UIElement content, bool fixedHorizontal = true, bool fixedVertical = true)
        {
            this.content = content;
            this.fixedHorizontal = fixedHorizontal;
            this.fixedVertical = fixedVertical;

            if (content != null)
                CopyFrameFrom(content);
        }

        public void AddChild(UIElement child)
        {
            SingleChildContainerUtility.Add(ref content, child, nameof(FixedSizeElement));
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
            GameObject outer = CreateElementRoot("FixedSize", parent);
            Image bg = AddBackgroundImageIfNeeded(outer);

            outer.AddComponent<UniftUISingleChildLayoutGroup>()
                .Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            LayoutElement outerLe = LayoutElementUtility.Configure(
                outer,
                preferredWidth,
                preferredHeight,
                !fixedHorizontal && infiniteWidth,
                !fixedVertical && infiniteHeight);

            if (fixedHorizontal)
                outerLe.flexibleWidth = 0f;
            if (fixedVertical)
                outerLe.flexibleHeight = 0f;

            builtGameObject = outer;

            ApplyInheritedFont(content);
            content?.Build(outer.transform);

            Canvas.ForceUpdateCanvases();

            ApplyAllEffects(outer, bg);
            return outer;
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
            if (!fixedHorizontal && ChildMayFillWidth(content))
                content.WithInfiniteWidth();
        }

        protected override void PropagateInfiniteHeightToContent()
        {
            if (!fixedVertical && ChildMayFillHeight(content))
                content.WithInfiniteHeight();
        }
    }
}
