using System;

namespace UniftUI
{
    /// <summary>Temporarily enables implicit animation flags on a <see cref="UIElement"/>.</summary>
    internal readonly struct AnimationScope : IDisposable
    {
        private readonly UIElement element;
        private readonly bool previousUseAnimation;
        private readonly float previousDuration;
        private readonly AnimationEasing previousEasing;

        private AnimationScope(UIElement element, Animation animation)
        {
            this.element = element;
            previousUseAnimation = element.useAnimation;
            previousDuration = element.animationDuration;
            previousEasing = element.animationEasing;

            element.useAnimation = animation.effectiveDuration > 0f;
            element.animationDuration = animation.effectiveDuration;
            element.animationEasing = animation.easing;
        }

        public static AnimationScope? TryCreate(UIElement element, Animation? animation)
        {
            if (element == null || !animation.HasValue || animation.Value.effectiveDuration <= 0f)
                return null;

            return new AnimationScope(element, animation.Value);
        }

        public void Dispose()
        {
            if (element == null)
                return;

            element.useAnimation = previousUseAnimation;
            element.animationDuration = previousDuration;
            element.animationEasing = previousEasing;
        }
    }
}
