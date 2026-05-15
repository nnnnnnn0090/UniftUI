using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>Applies insets around a single child.</summary>
    public class PaddingElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private RectOffset paddingValue;
        private VerticalLayoutGroup layoutGroup;

        public PaddingElement(UIElement content, RectOffset padding)
        {
            this.content = content;
            this.paddingValue = padding;
            this.padding = padding;

            if (content != null)
            {
                this.useCustomPosition = content.useCustomPosition;
                this.customPosition = content.customPosition;
                this.rotationEffectEuler = content.rotationEffectEuler;
                this.scaleEffect = content.scaleEffect;
            }
        }

        /// <summary>Updates padding uniformly on all edges.</summary>
        public void UpdatePadding(int padding)
        {
            UpdatePadding(new RectOffset(padding, padding, padding, padding));
        }

        /// <summary>Updates padding and refreshes the built layout when already on screen.</summary>
        public void UpdatePadding(RectOffset newPadding)
        {
            this.paddingValue = newPadding;
            this.padding = newPadding;

            if (layoutGroup != null && layoutGroup.gameObject != null)
            {
                layoutGroup.padding = newPadding;
                LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
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
            if (this.content == null) this.content = child;
            else Debug.LogWarning("[UniftUI] PaddingElement can only contain one child.");
        }

        public void RemoveChild(UIElement child)
        {
            if (this.content == child) this.content = null;
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (this.content == oldChild) this.content = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            if (content != null)
            {
                return new List<UIElement> { content };
            }
            return new List<UIElement>();
        }

        public override GameObject Build(Transform parent)
        {
            GameObject paddingContainer = new GameObject("PaddingContainer");
            paddingContainer.transform.SetParent(parent, false);

            Image bgImage = null;
            if (base.backgroundColor != Color.clear)
            {
                bgImage = paddingContainer.AddComponent<Image>();
                bgImage.color = base.backgroundColor;
            }

            layoutGroup = paddingContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = paddingValue;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;

            LayoutElement le = paddingContainer.AddComponent<LayoutElement>();
            ContentSizeFitter fitter = paddingContainer.AddComponent<ContentSizeFitter>();

            if (infiniteWidth)
            {
                le.flexibleWidth = 1;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredWidth >= 0)
            {
                le.preferredWidth = preferredWidth;
                le.minWidth = preferredWidth;
                le.flexibleWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                le.flexibleWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (infiniteHeight)
            {
                le.flexibleHeight = 1;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                le.preferredHeight = preferredHeight;
                le.minHeight = preferredHeight;
                le.flexibleHeight = 0;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                le.flexibleHeight = 0;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (this.preferredWidth >= 0 && !this.infiniteWidth)
            {
                content?.WithInfiniteWidth();
            }
            if (this.preferredHeight >= 0 && !this.infiniteHeight)
            {
                content?.WithInfiniteHeight();
            }

            if (this.infiniteWidth)
            {
                PropagateInfiniteWidthToContent();
            }
            if (this.infiniteHeight)
            {
                PropagateInfiniteHeightToContent();
            }

            ApplyAllEffects(paddingContainer, bgImage);

            builtGameObject = paddingContainer;

            content?.Build(paddingContainer.transform);

            return paddingContainer;
        }

        protected override void PropagateInfiniteWidthToContent()
        {
            content?.WithInfiniteWidth();
        }

        protected override void PropagateInfiniteHeightToContent()
        {
            content?.WithInfiniteHeight();
        }
    }
}
