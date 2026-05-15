using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UniftUI
{
    public class BackgroundElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private Color elementBackgroundColor;

        public BackgroundElement(UIElement content, Color bgColor)
        {
            this.content = content;
            this.elementBackgroundColor = bgColor;
            
            if (content != null)
            {
                this.useCustomPosition = content.useCustomPosition;
                this.customPosition = content.customPosition;
                this.rotationEffectEuler = content.rotationEffectEuler;
                this.scaleEffect = content.scaleEffect;
            }
        }

        public void AddChild(UIElement child)
        {
            if (this.content == null) this.content = child;
            else Debug.LogWarning("BackgroundElement can only contain one child.");
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
            GameObject backgroundContainer = new GameObject("BackgroundContainer");
            backgroundContainer.transform.SetParent(parent, false);

            GameObject contentObj = null;
            Image existingImage = null;
            
            if (content != null)
            {
                contentObj = content.Build(backgroundContainer.transform);
                existingImage = contentObj?.GetComponent<Image>();
            }

            Image bgImage = existingImage ?? backgroundContainer.AddComponent<Image>();
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

            var layoutGroup = backgroundContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;

            LayoutElement le = backgroundContainer.AddComponent<LayoutElement>();
            ContentSizeFitter fitter = backgroundContainer.AddComponent<ContentSizeFitter>();

            if (this.infiniteWidth)
            {
                le.flexibleWidth = 1;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (this.preferredWidth >= 0)
            {
                le.preferredWidth = this.preferredWidth;
                le.minWidth = this.preferredWidth;
                le.flexibleWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                le.flexibleWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (this.infiniteHeight)
            {
                le.flexibleHeight = 1;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (this.preferredHeight >= 0)
            {
                le.preferredHeight = this.preferredHeight;
                le.minHeight = this.preferredHeight;
                le.flexibleHeight = 0;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                le.flexibleHeight = 0;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            ApplyAllEffects(backgroundContainer, bgImage);

            return backgroundContainer;
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
