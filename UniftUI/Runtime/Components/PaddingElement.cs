using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Applies insets around a single child.</summary>
    public class PaddingElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private RectOffset paddingValue;
        private UniftUISingleChildLayoutGroup layoutGroup;

        public PaddingElement(UIElement content, RectOffset padding)
        {
            this.content = content;
            this.paddingValue = padding ?? new RectOffset(0, 0, 0, 0);
            this.padding = this.paddingValue;
        }

        /// <summary>Updates padding uniformly on all edges.</summary>
        public void UpdatePadding(int padding)
        {
            UpdatePadding(new RectOffset(padding, padding, padding, padding));
        }

        /// <summary>Updates padding and refreshes the built layout when already on screen.</summary>
        public void UpdatePadding(RectOffset newPadding)
        {
            newPadding = newPadding ?? new RectOffset(0, 0, 0, 0);
            this.paddingValue = newPadding;
            this.padding = newPadding;

            if (layoutGroup != null && layoutGroup.gameObject != null)
            {
                if (useAnimation && animationDuration > 0f && builtGameObject != null)
                {
                    Vector4 from = PaddingToVector(layoutGroup.padding);
                    Vector4 to = PaddingToVector(newPadding);
                    var animator = BaseAnimator<Vector4>.GetOrReplace<PaddingAnimator>(builtGameObject);
                    animator.AnimateTo(from, to, animationDuration, animationEasing);
                }
                else
                {
                    layoutGroup.padding = newPadding;
                    LayoutCore.ForceRebuildLayout(layoutGroup.gameObject);
                }
            }
        }

        public override UIElement WithPadding(int padding)
        {
            base.WithPadding(padding);
            UpdatePadding(padding);
            return this;
        }

        public override UIElement WithPadding(RectOffset padding)
        {
            base.WithPadding(padding);
            UpdatePadding(padding);
            return this;
        }

        public void AddChild(UIElement child)
        {
            SingleChildContainerUtility.Add(ref content, child, nameof(PaddingElement));
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
            GameObject paddingContainer = CreateElementRoot("PaddingContainer", parent);

            Image bgImage = null;
            if (base.backgroundColor != Color.clear)
                bgImage = AddImage(paddingContainer, base.backgroundColor);
            bgImage = EnsureControlHitProxy(paddingContainer, bgImage, content);

            layoutGroup = paddingContainer.AddComponent<UniftUISingleChildLayoutGroup>();
            layoutGroup.Configure(paddingValue, TextAnchor.MiddleCenter);

            LayoutElementUtility.Configure(paddingContainer, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            if (preferredWidth >= 0 && !infiniteWidth && ChildMayFillWidth(content))
            {
                content?.WithInfiniteWidth();
            }
            if (preferredHeight >= 0 && !infiniteHeight && ChildMayFillHeight(content))
            {
                content?.WithInfiniteHeight();
            }

            if (infiniteWidth)
            {
                PropagateInfiniteWidthToContent();
            }
            if (infiniteHeight)
            {
                PropagateInfiniteHeightToContent();
            }

            ApplyAllEffects(paddingContainer, bgImage);

            builtGameObject = paddingContainer;

            ApplyInheritedFont(content);
            content?.Build(paddingContainer.transform);

            return paddingContainer;
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

        private static Vector4 PaddingToVector(RectOffset padding)
        {
            if (padding == null)
                return Vector4.zero;
            return new Vector4(padding.left, padding.right, padding.top, padding.bottom);
        }
    }
}
