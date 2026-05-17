using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>
    /// Visual offset (<c>x</c>, <c>y</c>). Keeps the layout frame; shifts content only.
    /// Coordinates match <see cref="UIElement.WithPosition"/> (X right, Y down positive).
    /// </summary>
    public class OffsetElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private Vector2 offset;
        private State<Vector2> offsetState;
        private State<float> offsetXState;
        private float offsetYConst;
        private bool useXyStates;
        private bool useVectorState;
        private UniftUISingleChildLayoutGroup layoutGroup;

        public OffsetElement(UIElement content, Vector2 offset)
        {
            this.content = content;
            this.offset = offset;
            CopyStyleFromContent(content);
        }

        public OffsetElement(UIElement content, State<Vector2> offset)
        {
            this.content = content;
            this.offsetState = offset;
            this.useVectorState = true;
            if (offset != null) this.offset = offset.Value;
            CopyStyleFromContent(content);
        }

        public OffsetElement(UIElement content, State<float> offsetX, float y)
        {
            this.content = content;
            this.offsetXState = offsetX;
            this.offsetYConst = y;
            this.useXyStates = true;
            this.offset = new Vector2(offsetX != null ? offsetX.Value : 0f, y);
            CopyStyleFromContent(content);
        }

        private void CopyStyleFromContent(UIElement src)
        {
            if (src == null) return;
            CopyFrameFrom(src);
        }

        public void AddChild(UIElement child)
        {
            if (content == null)
                content = child;
            else
                Debug.LogWarning("[UniftUI] OffsetElement can only contain one child.");
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

        /// <summary>Applies offset as a Unity <c>anchoredPosition</c> delta.</summary>
        public static Vector2 SwiftOffsetToAnchoredDelta(Vector2 swift)
        {
            return new Vector2(swift.x, -swift.y);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject outer = new GameObject("Offset");
            outer.transform.SetParent(parent, false);

            Image bg = null;
            if (backgroundColor != Color.clear)
            {
                bg = outer.AddComponent<Image>();
                bg.color = backgroundColor;
            }

            layoutGroup = outer.AddComponent<UniftUISingleChildLayoutGroup>();
            layoutGroup.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);
            layoutGroup.SetVisualOffset(GetCurrentOffset());

            LayoutElementUtility.Configure(outer, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            if (preferredWidth >= 0 && !infiniteWidth && ChildMayFillWidth(content))
                content?.WithInfiniteWidth();
            if (preferredHeight >= 0 && !infiniteHeight && ChildMayFillHeight(content))
                content?.WithInfiniteHeight();
            if (infiniteWidth)
                PropagateInfiniteWidthToContent();
            if (infiniteHeight)
                PropagateInfiniteHeightToContent();

            ApplyInheritedFont(content);
            GameObject contentObj = content != null ? content.Build(outer.transform) : null;

            if (contentObj != null)
            {
                if (useVectorState && offsetState != null)
                {
                    AddPropertyBinding(offsetState, () =>
                    {
                        ApplyVisualOffsetFromBinding(offsetState.Value);
                    }, "offset", BindingKind.Visual);
                }

                if (useXyStates && offsetXState != null)
                {
                    AddPropertyBinding(offsetXState, () =>
                    {
                        ApplyVisualOffsetFromBinding(new Vector2(offsetXState.Value, offsetYConst));
                    }, "offsetX", BindingKind.Visual);
                }
            }

            builtGameObject = outer;
            ApplyAllEffects(outer, bg);
            return outer;
        }

        private Vector2 GetCurrentOffset()
        {
            if (useVectorState && offsetState != null) return offsetState.Value;
            if (useXyStates && offsetXState != null) return new Vector2(offsetXState.Value, offsetYConst);
            return offset;
        }

        private void ApplyVisualOffsetFromBinding(Vector2 swiftOffset)
        {
            if (builtGameObject == null || layoutGroup == null) return;
            if (useAnimation && animationDuration > 0f)
            {
                var animator = BaseAnimator<Vector2>.GetOrReplace<VisualOffsetAnimator>(builtGameObject);
                animator.AnimateTo(layoutGroup.VisualOffset, swiftOffset, animationDuration, animationEasing);
            }
            else
            {
                layoutGroup.SetVisualOffset(swiftOffset);
            }

            offset = swiftOffset;
            LayoutRebuilder.MarkLayoutForRebuild(builtGameObject.GetComponent<RectTransform>());
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
