using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// SwiftUI の <c>.fixedSize()</c> / <c>fixedSize(horizontal:vertical:)</c> に相当。
    /// 指定軸で親から渡される余分な提案サイズを受けず、コンテンツの理想サイズを優先します。
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
            {
                preferredWidth = content.preferredWidth;
                preferredHeight = content.preferredHeight;
                infiniteWidth = content.infiniteWidth;
                infiniteHeight = content.infiniteHeight;
                useCustomPosition = content.useCustomPosition;
                customPosition = content.customPosition;
                rotationEffectEuler = content.rotationEffectEuler;
                scaleEffect = content.scaleEffect;
            }
        }

        public void AddChild(UIElement child)
        {
            if (content == null)
                content = child;
            else
                Debug.LogWarning("FixedSizeElement は単一の子のみサポートします。");
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
                return new List<UIElement> { content };
            return new List<UIElement>();
        }

        public override GameObject Build(Transform parent)
        {
            GameObject outer = new GameObject("FixedSize");
            outer.transform.SetParent(parent, false);

            Image bg = null;
            if (backgroundColor != Color.clear)
            {
                bg = outer.AddComponent<Image>();
                bg.color = backgroundColor;
            }

            LayoutElement outerLe = outer.AddComponent<LayoutElement>();
            ContentSizeFitter fitter = outer.AddComponent<ContentSizeFitter>();

            if (fixedHorizontal)
            {
                outerLe.flexibleWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                if (preferredWidth >= 0)
                {
                    outerLe.preferredWidth = preferredWidth;
                    outerLe.minWidth = preferredWidth;
                }
            }
            else
            {
                outerLe.flexibleWidth = infiniteWidth ? 1 : 0;
                fitter.horizontalFit = infiniteWidth
                    ? ContentSizeFitter.FitMode.Unconstrained
                    : ContentSizeFitter.FitMode.PreferredSize;
                if (preferredWidth >= 0)
                {
                    outerLe.preferredWidth = preferredWidth;
                    outerLe.minWidth = preferredWidth;
                }
            }

            if (fixedVertical)
            {
                outerLe.flexibleHeight = 0;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                if (preferredHeight >= 0)
                {
                    outerLe.preferredHeight = preferredHeight;
                    outerLe.minHeight = preferredHeight;
                }
            }
            else
            {
                outerLe.flexibleHeight = infiniteHeight ? 1 : 0;
                fitter.verticalFit = infiniteHeight
                    ? ContentSizeFitter.FitMode.Unconstrained
                    : ContentSizeFitter.FitMode.PreferredSize;
                if (preferredHeight >= 0)
                {
                    outerLe.preferredHeight = preferredHeight;
                    outerLe.minHeight = preferredHeight;
                }
            }

            builtGameObject = outer;

            content?.Build(outer.transform);

            Canvas.ForceUpdateCanvases();

            ApplyAllEffects(outer, bg);
            return outer;
        }

        protected override void PropagateInfiniteWidthToContent()
        {
            if (!fixedHorizontal && content != null)
                content.WithInfiniteWidth();
        }

        protected override void PropagateInfiniteHeightToContent()
        {
            if (!fixedVertical && content != null)
                content.WithInfiniteHeight();
        }
    }
}
