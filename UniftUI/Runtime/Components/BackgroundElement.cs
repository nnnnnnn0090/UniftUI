using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Wraps content in a colored background layer.</summary>
    public class BackgroundElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private readonly Color elementBackgroundColor;

        public BackgroundElement(UIElement content, Color bgColor)
        {
            this.content = content;
            this.elementBackgroundColor = bgColor;
            CopyFrameFrom(content);
        }

        public void AddChild(UIElement child)
        {
            SingleChildContainerUtility.Add(ref content, child, nameof(BackgroundElement));
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
            GameObject backgroundContainer = CreateElementRoot("BackgroundContainer", parent);

            var layoutGroup = backgroundContainer.AddComponent<UniftUISingleChildLayoutGroup>();
            layoutGroup.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            LayoutElementUtility.Configure(backgroundContainer, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            if (infiniteWidth)
                PropagateInfiniteWidthToContent();
            if (infiniteHeight)
                PropagateInfiniteHeightToContent();

            Image bgImage = AddImage(backgroundContainer, elementBackgroundColor);
            bgImage = EnsureControlHitProxy(backgroundContainer, bgImage, content);

            if (base.backgroundColor != Color.clear && base.backgroundColor != elementBackgroundColor)
            {
                GameObject additionalBg = CreateFullStretchChild("AdditionalBackground", backgroundContainer.transform);
                additionalBg.transform.SetAsFirstSibling();

                Image additionalImage = AddImage(additionalBg, base.backgroundColor);

                ApplyRoundedCorners(additionalBg, additionalImage);
            }

            ApplyInheritedFont(content);
            content?.Build(backgroundContainer.transform);

            ApplyAllEffects(backgroundContainer, bgImage);

            return backgroundContainer;
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
