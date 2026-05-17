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
            if (content == null)
                content = child;
            else
                Debug.LogWarning("[UniftUI] BackgroundElement can only contain one child.");
        }

        public void RemoveChild(UIElement child)
        {
            if (content == child)
                content = null;
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (content == oldChild)
                content = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            if (content != null)
                yield return content;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject backgroundContainer = new GameObject("BackgroundContainer");
            backgroundContainer.transform.SetParent(parent, false);

            var layoutGroup = backgroundContainer.AddComponent<UniftUISingleChildLayoutGroup>();
            layoutGroup.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            LayoutElementUtility.Configure(backgroundContainer, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            if (infiniteWidth)
                PropagateInfiniteWidthToContent();
            if (infiniteHeight)
                PropagateInfiniteHeightToContent();

            Image bgImage = backgroundContainer.AddComponent<Image>();
            bgImage.color = elementBackgroundColor;

            if (base.backgroundColor != Color.clear && base.backgroundColor != elementBackgroundColor)
            {
                GameObject additionalBg = new GameObject("AdditionalBackground");
                additionalBg.transform.SetParent(backgroundContainer.transform, false);
                additionalBg.transform.SetAsFirstSibling();

                RectTransform rectTransform = additionalBg.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;

                Image additionalImage = additionalBg.AddComponent<Image>();
                additionalImage.color = base.backgroundColor;

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
