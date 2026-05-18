using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

namespace UniftUI
{
    internal static partial class UIElementExtensions
    {
        /// <summary>Enables implicit animation for property changes over <paramref name="duration"/> seconds.</summary>
        public static T Animation<T>(this T element, float duration) where T : UIElement
        {
            ApplyAnimationRecursively(element, AnimationEasing.Linear, duration);
            return element;
        }

        /// <summary>Enables implicit animation with easing over <paramref name="duration"/> seconds.</summary>
        public static T Animation<T>(this T element, AnimationEasing easing, float duration) where T : UIElement
        {
            ApplyAnimationRecursively(element, easing, duration);
            return element;
        }

        /// <summary>
        /// Binds an animation to a state so changes to that state are animated:
        /// animates this view when <paramref name="value"/> changes.
        /// </summary>
        public static T Animation<T>(this T element, Animation anim, State value) where T : UIElement
        {
            ApplyStateAnimationRecursively(element, anim, value);
            return element;
        }

        /// <summary>
        /// Binds the same animation to multiple states (e.g. opacity and frame width on one card).
        /// </summary>
        public static T Animation<T>(this T element, Animation anim, State value0, State value1) where T : UIElement
        {
            ApplyStateAnimationRecursively(element, anim, value0);
            ApplyStateAnimationRecursively(element, anim, value1);
            return element;
        }

        /// <summary>Animates property changes when <paramref name="value"/> changes using the default animation.</summary>
        public static T Animation<T>(this T element, State value) where T : UIElement
        {
            ApplyStateAnimationRecursively(element, global::UniftUI.Animation.Default, value);
            return element;
        }

        private static void ApplyAnimationRecursively(UIElement element, AnimationEasing easing, float duration)
        {
            if (element == null) return;
            element.WithAnimation(easing, duration);
            if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                    ApplyAnimationRecursively(child, easing, duration);
            }
        }

        private static void ApplyStateAnimationRecursively(UIElement element, Animation anim, State value)
        {
            if (element == null) return;
            element.RegisterAnimationForBoundStateInSubtree(anim, value);
        }

        /// <summary>Sets elastic scroll bounce (<see cref="ScrollRect.MovementType"/>).</summary>
        public static ScrollViewElement ScrollBounce(this ScrollViewElement e, bool elastic) =>
            e.WithScrollBounce(elastic);

        /// <summary>Sets scroll wheel and trackpad sensitivity.</summary>
        public static ScrollViewElement ScrollSensitivity(this ScrollViewElement e, float sensitivity) =>
            e.WithScrollSensitivity(sensitivity);

        /// <summary>Sets <see cref="ScrollRect.movementType"/> directly.</summary>
        public static ScrollViewElement ScrollMovementType(this ScrollViewElement e, ScrollRect.MovementType type) =>
            e.WithMovementType(type);

        /// <summary>Binds vertical normalized scroll position (1 = top, 0 = bottom) to a <see cref="State{T}"/>.</summary>
        public static ScrollViewElement ScrollPositionY(this ScrollViewElement e, State<float> normalized, bool twoWay = false) =>
            e.BindScrollPositionY(normalized, twoWay);

        /// <summary>Sets scroll indicator visibility for all enabled axes.</summary>
        public static ScrollViewElement ScrollIndicators(this ScrollViewElement e, ScrollIndicatorVisibility visibility) =>
            e.WithScrollIndicators(visibility);

        /// <summary>Sets scroll indicator visibility for selected axes.</summary>
        public static ScrollViewElement ScrollIndicators(this ScrollViewElement e, ScrollIndicatorVisibility visibility, UniftUIScrollAxis axes) =>
            e.WithScrollIndicators(visibility, axes);

        /// <summary>Binds horizontal normalized scroll position (0 = left, 1 = right) to a <see cref="State{T}"/>.</summary>
        public static ScrollViewElement ScrollPositionX(this ScrollViewElement e, State<float> normalized, bool twoWay = false) =>
            e.BindScrollPositionX(normalized, twoWay);

    }
}
