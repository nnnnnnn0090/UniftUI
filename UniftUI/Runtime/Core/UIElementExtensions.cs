using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

namespace UniftUI
{
    /// <summary>
    /// Modifier extensions for <see cref="UIElement"/> types.
    /// </summary>
    internal static partial class UIElementExtensions
    {
        /// <summary>Sets frame dimensions and optional infinite expansion.</summary>
        public static T Frame<T>(this T element, float? width = null, float? height = null,
                                bool? infiniteWidth = null, bool? infiniteHeight = null) where T : UIElement
        {
            if (width.HasValue)
            {
                element.WithWidth(width.Value);
            }

            if (height.HasValue)
            {
                element.WithHeight(height.Value);
            }

            if (infiniteWidth.HasValue && infiniteWidth.Value)
            {
                element.WithInfiniteWidth();
            }

            if (infiniteHeight.HasValue && infiniteHeight.Value)
            {
                element.WithInfiniteHeight();
            }

            return element;
        }

        /// <summary>Sets frame dimensions from reactive <see cref="State{T}"/> values.</summary>
        public static T Frame<T>(this T element, State<float> width = null, State<float> height = null) where T : UIElement
        {
            if (width != null)
            {
                element.WithWidth(width);
            }

            if (height != null)
            {
                element.WithHeight(height);
            }

            return element;
        }
    }
}
