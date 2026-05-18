using UnityEngine;

namespace UniftUI
{
    public static class ColorExtensions
    {
        public static Color MultiplyAlpha(this Color color, float alpha)
        {
            return UniftUIColors.MultiplyAlpha(color, alpha);
        }
    }
}
