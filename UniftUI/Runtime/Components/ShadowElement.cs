using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    public class ShadowElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private readonly Color shadowColor;
        private readonly Vector2 offset;
        private readonly float blurRadius;

        public ShadowElement(UIElement content, Color shadowColor, Vector2 offset, float blurRadius)
        {
            this.content = content;
            this.shadowColor = shadowColor;
            this.offset = offset;
            this.blurRadius = blurRadius;
        }

        public void AddChild(UIElement child)
        {
            if (content == null)
                content = child;
            else
                Debug.LogWarning("[UniftUI] ShadowElement can only contain one child.");
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
            GameObject shadowContainer = CreateElementRoot("ShadowContainer", parent);

            var layoutGroup = shadowContainer.AddComponent<UniftUISingleChildLayoutGroup>();
            layoutGroup.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            LayoutElementUtility.Configure(shadowContainer, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            if (infiniteWidth)
                PropagateInfiniteWidthToContent();
            if (infiniteHeight)
                PropagateInfiniteHeightToContent();

            ApplyAllEffects(shadowContainer);

            if (content != null)
            {
                if (preferredWidth >= 0 && !infiniteWidth && ChildMayFillWidth(content))
                    content.WithInfiniteWidth();
                if (preferredHeight >= 0 && !infiniteHeight && ChildMayFillHeight(content))
                    content.WithInfiniteHeight();

                ApplyInheritedFont(content);
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
