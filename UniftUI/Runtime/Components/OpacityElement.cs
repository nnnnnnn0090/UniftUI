using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Wraps content in a CanvasGroup so opacity applies to the rendered subtree.</summary>
    public class OpacityElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private State<float> opacityState;
        private GameObject opacityContainer;

        public OpacityElement(UIElement content, float opacity)
        {
            this.content = content;
            CopyFrameFrom(content);
            this.opacity = Mathf.Clamp01(opacity);
        }

        public OpacityElement(UIElement content, State<float> opacity)
        {
            this.content = content;
            this.opacityState = opacity;
            CopyFrameFrom(content);

            if (opacity != null)
                WithOpacity(opacity);
        }

        public void AddChild(UIElement child)
        {
            SingleChildContainerUtility.Add(ref content, child, nameof(OpacityElement));
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
            opacityContainer = CreateElementRoot("OpacityContainer", parent);

            var layoutGroup = opacityContainer.AddComponent<UniftUISingleChildLayoutGroup>();
            layoutGroup.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            LayoutElementUtility.Configure(opacityContainer, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            if (preferredWidth >= 0 && !infiniteWidth && ChildMayFillWidth(content))
                content?.WithInfiniteWidth();
            if (preferredHeight >= 0 && !infiniteHeight && ChildMayFillHeight(content))
                content?.WithInfiniteHeight();
            if (infiniteWidth)
                PropagateInfiniteWidthToContent();
            if (infiniteHeight)
                PropagateInfiniteHeightToContent();

            ApplyInheritedFont(content);
            content?.Build(opacityContainer.transform);

            ApplyAllEffects(opacityContainer);

            return opacityContainer;
        }

        internal void SyncOpacityVisual(GameObject container, State<float> state)
        {
            if (container == null || state == null)
                return;

            builtGameObject = container;
            float target = Mathf.Clamp01(state.Value);
            opacity = target;

            var canvasGroup = container.GetComponent<CanvasGroup>();
            float current = canvasGroup != null ? canvasGroup.alpha : 1f;

            Animation? anim = ResolveAnimationForState(state);
            if (!anim.HasValue && useAnimation && animationDuration > 0f)
            {
                anim = new Animation
                {
                    duration = animationDuration,
                    easing = animationEasing,
                    animSpeed = 1f
                };
            }

            if (anim.HasValue && anim.Value.effectiveDuration > 0f && !Mathf.Approximately(current, target))
            {
                var animator = BaseAnimator<float>.GetOrReplace<OpacityAnimator>(container);
                animator.AnimateTo(current, target, anim.Value.effectiveDuration, anim.Value.easing);
            }
            else
            {
                if (canvasGroup == null)
                    canvasGroup = container.AddComponent<CanvasGroup>();
                canvasGroup.alpha = target;
            }

            Canvas.ForceUpdateCanvases();
        }

        internal override void HandleStateChange(State changedState)
        {
            if (opacityState != null && ReferenceEquals(changedState, opacityState) && opacityContainer != null)
            {
                SyncOpacityVisual(opacityContainer, opacityState);
                return;
            }

            base.HandleStateChange(changedState);
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
