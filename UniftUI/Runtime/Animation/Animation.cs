using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// SwiftUI の Animation 型と同じ API を提供する構造体。
    /// </summary>
    public struct Animation
    {
        public float duration;
        public AnimationEasing easing;
        public float animDelay;
        public float animSpeed;
        public int animRepeatCount;
        public bool animAutoreverses;

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
    /// withAnimation { } のグローバルアニメーションコンテキスト。
    /// Stack を使って再入可能に実装。メインスレッド専用。
    /// </summary>
    public static class AnimationContext
    {
        private static readonly Stack<Animation> stack = new Stack<Animation>();

        public static Animation? Current => stack.Count > 0 ? stack.Peek() : (Animation?)null;

        internal static void Push(Animation animation) => stack.Push(animation);

        internal static void Pop()
        {
            if (stack.Count > 0) stack.Pop();
        }
    }
}
