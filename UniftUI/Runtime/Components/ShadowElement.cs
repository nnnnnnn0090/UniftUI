using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UniftUI
{
    public class ShadowElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private Color shadowColor;
        private Vector2 offset;
        private float blurRadius;

        public ShadowElement(UIElement content, Color shadowColor, Vector2 offset, float blurRadius)
        {
            this.content = content;
            this.shadowColor = shadowColor;
            this.offset = offset;
            this.blurRadius = blurRadius;
            
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
            else Debug.LogWarning("ShadowElement can only contain one child.");
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
            GameObject shadowContainer = new GameObject("ShadowContainer");
            shadowContainer.transform.SetParent(parent, false);

            var layoutGroup = shadowContainer.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = true;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);

            LayoutElement le = shadowContainer.AddComponent<LayoutElement>();
            ContentSizeFitter fitter = shadowContainer.AddComponent<ContentSizeFitter>();

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

            ApplyAllEffects(shadowContainer);

            if (content != null)
            {
                GameObject contentObj = content.Build(shadowContainer.transform);
                
                if (contentObj != null)
                {
                    Shadow shadow = contentObj.AddComponent<Shadow>();
                    shadow.effectColor = shadowColor;
                    shadow.effectDistance = offset;
                    
                    if (blurRadius > 0)
                    {
                        shadow.useGraphicAlpha = false;
                        
                        CanvasRenderer renderer = contentObj.GetComponent<CanvasRenderer>();
                        if (renderer != null)
                        {
                            renderer.SetAlphaTexture(null);
                        }
                        
                        if (blurRadius > 5)
                        {
                            Shadow shadow2 = contentObj.AddComponent<Shadow>();
                            shadow2.effectColor = new Color(shadowColor.r, shadowColor.g, shadowColor.b, shadowColor.a * 0.5f);
                            shadow2.effectDistance = offset * 1.5f;
                            shadow2.useGraphicAlpha = false;
                        }
                    }
                }
            }

            return shadowContainer;
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
