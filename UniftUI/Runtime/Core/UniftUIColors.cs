using UnityEngine;

namespace UniftUI
{
    internal static class UniftUIColors
    {
        public const float SelectionAlpha = 0.35f;

        public static Color SelectionTint(Color color)
        {
            return new Color(color.r, color.g, color.b, Mathf.Min(color.a, SelectionAlpha));
        }

        public static Color ScaleRgb(Color color, float multiplier)
        {
            return new Color(
                Mathf.Clamp01(color.r * multiplier),
                Mathf.Clamp01(color.g * multiplier),
                Mathf.Clamp01(color.b * multiplier),
                color.a);
        }

        public static Color MultiplyAlpha(Color color, float multiplier)
        {
            return new Color(color.r, color.g, color.b, Mathf.Clamp01(color.a * multiplier));
        }
    }
}
