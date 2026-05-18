using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

namespace UniftUI
{
    internal static partial class UIElementExtensions
    {
        /// <summary>Wraps the element with uniform padding.</summary>
        public static PaddingElement Padding<T>(this T element, int padding) where T : UIElement
        {
            var paddingValue = new RectOffset(padding, padding, padding, padding);
            var paddingElement = new PaddingElement(element, paddingValue);
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }

        /// <summary>Wraps the element with reactive uniform padding.</summary>
        public static PaddingElement Padding<T>(this T element, State<int> padding) where T : UIElement
        {
            if (padding == null)
                return Padding(element, 0);

            var paddingValue = new RectOffset(padding.Value, padding.Value, padding.Value, padding.Value);
            var paddingElement = new PaddingElement(element, paddingValue);
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            paddingElement.AddPropertyBinding(padding, () =>
            {
                paddingElement.UpdatePadding(padding.Value);
            }, "padding", BindingKind.Layout);

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }

        /// <summary>Sizes to content instead of expanding to fill the parent.</summary>
        public static FixedSizeElement FixedSize<T>(this T element, bool horizontal = true, bool vertical = true) where T : UIElement
        {
            var fs = new FixedSizeElement(element, horizontal, vertical);
            UIContext.Current?.ReplaceChild(element, fs);
            return fs;
        }

        /// <summary>Proposes a width-to-height aspect ratio around the element.</summary>
        public static AspectRatioElement AspectRatio<T>(this T element, float ratio,
            AspectRatioContentMode contentMode = AspectRatioContentMode.Fit) where T : UIElement
        {
            var aspect = new AspectRatioElement(element, ratio, contentMode);
            UIContext.Current?.ReplaceChild(element, aspect);
            return aspect;
        }

        /// <summary>Clips the element to its layout bounds.</summary>
        public static ClippedElement Clipped<T>(this T element) where T : UIElement
        {
            var clipped = new ClippedElement(element);
            UIContext.Current?.ReplaceChild(element, clipped);
            return clipped;
        }

        /// <summary>Clips the element to a shape mask.</summary>
        public static ClippedElement ClipShape<T>(this T element, UniftUIClipShape shape, float cornerRadius = 12f)
            where T : UIElement
        {
            var clipped = new ClippedElement(element, shape, cornerRadius);
            UIContext.Current?.ReplaceChild(element, clipped);
            return clipped;
        }

        /// <summary>Applies a reusable button style.</summary>
        public static ButtonElement ButtonStyle(this ButtonElement element, IButtonStyle style)
        {
            element.SetButtonStyle(style);
            return element;
        }

        /// <summary>Applies a reusable text field style.</summary>
        public static TextFieldElement TextFieldStyle(this TextFieldElement element, ITextFieldStyle style)
        {
            style?.Apply(element);
            return element;
        }

        /// <summary>Wraps the element with explicit <see cref="RectOffset"/> padding.</summary>
        public static PaddingElement Padding<T>(this T element, RectOffset padding) where T : UIElement
        {
            var paddingElement = new PaddingElement(element, padding);
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }

        /// <summary>Wraps the element with per-edge padding.</summary>
        public static PaddingElement Padding<T>(this T element, float? top = null, float? bottom = null, float? left = null, float? right = null) where T : UIElement
        {
            var paddingValue = new RectOffset(
                left.HasValue ? (int)left.Value : 0,
                right.HasValue ? (int)right.Value : 0,
                top.HasValue ? (int)top.Value : 0,
                bottom.HasValue ? (int)bottom.Value : 0
            );

            var paddingElement = new PaddingElement(element, paddingValue);
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }

    }
}
