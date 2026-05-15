using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// Describes how property changes are animated.
    /// Not related to <see cref="UnityEngine.Animation"/>.
    /// </summary>
    public struct Animation
    {
        public float duration;
        public AnimationEasing easing;
        public float animDelay;
        public float animSpeed;
        public int animRepeatCount;
        public bool animAutoreverses;

        /// <summary>Default animation: <see cref="easeInOut"/> with a 0.35s duration.</summary>
        public static readonly Animation Default = easeInOut();

        public static Animation linear(float duration = 0.35f)
            => new Animation { easing = AnimationEasing.Linear, duration = duration, animSpeed = 1f };

        public static Animation easeIn(float duration = 0.35f)
            => new Animation { easing = AnimationEasing.EaseIn, duration = duration, animSpeed = 1f };

        public static Animation easeOut(float duration = 0.35f)
            => new Animation { easing = AnimationEasing.EaseOut, duration = duration, animSpeed = 1f };

        public static Animation easeInOut(float duration = 0.35f)
            => new Animation { easing = AnimationEasing.EaseInOut, duration = duration, animSpeed = 1f };

        public static Animation spring(float response = 0.5f, float dampingFraction = 0.825f)
            => new Animation { easing = AnimationEasing.Spring, duration = response, animSpeed = 1f };

        public static Animation interactiveSpring(float response = 0.15f, float dampingFraction = 0.86f)
            => new Animation { easing = AnimationEasing.Spring, duration = response, animSpeed = 1f };

        public static Animation bouncy(float duration = 0.5f)
            => new Animation { easing = AnimationEasing.EaseOutBounce, duration = duration, animSpeed = 1f };

        public Animation delay(float seconds) { var a = this; a.animDelay = seconds; return a; }
        public Animation speed(float multiplier) { var a = this; a.animSpeed = multiplier; return a; }
        public Animation repeatCount(int count, bool autoreverses = true) { var a = this; a.animRepeatCount = count; a.animAutoreverses = autoreverses; return a; }
        public Animation repeatForever(bool autoreverses = true) { var a = this; a.animRepeatCount = -1; a.animAutoreverses = autoreverses; return a; }

        internal float effectiveDuration => animSpeed > 0 ? duration / animSpeed : duration;
    }

    /// <summary>
    /// Holds the implicit animation used by <see cref="WithAnimation"/> during a scoped update.
    /// </summary>
    public static class AnimationContext
    {
        private static readonly Stack<Animation> stack = new Stack<Animation>();

        /// <summary>Animation from the innermost active <see cref="WithAnimation"/> scope, if any.</summary>
        public static Animation? Current => stack.Count > 0 ? stack.Peek() : (Animation?)null;

        /// <summary>Runs <paramref name="changes"/> while applying <paramref name="animation"/> to state-driven updates.</summary>
        public static void WithAnimation(Animation animation, Action changes)
        {
            Push(animation);
            try { changes(); }
            finally { Pop(); }
        }

        /// <summary>Runs <paramref name="changes"/> with <see cref="Animation.Default"/>.</summary>
        public static void WithAnimation(Action changes) => WithAnimation(Animation.Default, changes);

        internal static void Push(Animation animation) => stack.Push(animation);

        internal static void Pop()
        {
            if (stack.Count > 0) stack.Pop();
        }
    }
}
