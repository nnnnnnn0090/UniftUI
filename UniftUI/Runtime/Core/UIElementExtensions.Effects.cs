using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

namespace UniftUI
{
    internal static partial class UIElementExtensions
    {
        /// <summary>Applies uniform corner radius.</summary>
        public static T CornerRadius<T>(this T element, float radius) where T : UIElement
        {
            element.WithCornerRadius(radius);
            return element;
        }

        /// <summary>Reactive uniform corner radius.</summary>
        public static T CornerRadius<T>(this T element, State<float> radius) where T : UIElement
        {
            element.WithCornerRadius(radius);
            return element;
        }

        /// <summary>Applies per-corner corner radius.</summary>
        public static T CornerRadius<T>(this T element, float topLeft, float topRight, float bottomRight, float bottomLeft) where T : UIElement
        {
            element.WithCornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            return element;
        }

        /// <summary>Applies corner radius to selected corners.</summary>
        public static T CornerRadius<T>(this T element, RectCorner corners, float radius) where T : UIElement
        {
            element.WithCornerRadius(radius, corners);
            return element;
        }

        /// <summary>Applies a shadow to the element or TextMesh Pro underlay shadow for text.</summary>
        public static UIElement Shadow<T>(this T element, Color? color = null, float radius = 3f, float x = 0, float y = 0) where T : UIElement
        {
            Color shadowColor = color ?? new Color(0,0,0);
            Vector2 offset = new Vector2(x, y);

            if (element is TextElement textElement)
            {
                return textElement.SetTMProShadow(shadowColor, offset, radius);
            }

            var shadowElement = new ShadowElement(element, shadowColor, offset, radius);
            shadowElement.CopyFrameFrom(element);

            UIContext.Current?.ReplaceChild(element, shadowElement);
            return shadowElement;
        }

        /// <summary>Applies rotation effect in degrees (Z axis).</summary>
        public static T RotationEffect<T>(this T element, float degrees) where T : UIElement
        {
            return (T)element.WithRotationEffect(degrees);
        }

        /// <summary>Applies rotation effect with Euler angles.</summary>
        public static T RotationEffect<T>(this T element, float x, float y, float z) where T : UIElement
        {
            return (T)element.WithRotationEffect(x, y, z);
        }

        /// <summary>Reactive rotation on the Z axis.</summary>
        public static T RotationEffect<T>(this T element, State<float> degrees) where T : UIElement
        {
            return (T)element.WithRotationEffect(degrees);
        }

        /// <summary>Reactive rotation with bound X component.</summary>
        public static T RotationEffect<T>(this T element, State<float> x, float y, float z) where T : UIElement
        {
            return (T)element.WithRotationEffect(x, y, z);
        }

        /// <summary>Reactive rotation with bound Y component.</summary>
        public static T RotationEffect<T>(this T element, float x, State<float> y, float z) where T : UIElement
        {
            return (T)element.WithRotationEffect(x, y, z);
        }

        /// <summary>Reactive rotation with bound Z component.</summary>
        public static T RotationEffect<T>(this T element, float x, float y, State<float> z) where T : UIElement
        {
            return (T)element.WithRotationEffect(x, y, z);
        }

        /// <summary>Reactive rotation with bound <see cref="Vector3"/> Euler angles.</summary>
        public static T RotationEffect<T>(this T element, State<Vector3> euler) where T : UIElement
        {
            return (T)element.WithRotationEffect(euler);
        }

        /// <summary>Applies uniform scale effect.</summary>
        public static T ScaleEffect<T>(this T element, float scale) where T : UIElement
        {
            return (T)element.WithScaleEffect(scale);
        }

        /// <summary>Applies scale effect on X and Y.</summary>
        public static T ScaleEffect<T>(this T element, float x, float y) where T : UIElement
        {
            return (T)element.WithScaleEffect(x, y);
        }

        /// <summary>Applies scale effect on X, Y, and Z.</summary>
        public static T ScaleEffect<T>(this T element, float x, float y, float z) where T : UIElement
        {
            return (T)element.WithScaleEffect(x, y, z);
        }

        /// <summary>Applies scale effect from a <see cref="Vector3"/>.</summary>
        public static T ScaleEffect<T>(this T element, Vector3 scale) where T : UIElement
        {
            return (T)element.WithScaleEffect(scale);
        }

        /// <summary>Reactive uniform scale.</summary>
        public static T ScaleEffect<T>(this T element, State<float> scale) where T : UIElement
        {
            return (T)element.WithScaleEffect(scale);
        }

        /// <summary>Reactive scale with bound X component.</summary>
        public static T ScaleEffect<T>(this T element, State<float> x, float y) where T : UIElement
        {
            return (T)element.WithScaleEffect(x, y);
        }

        /// <summary>Reactive scale with bound Y component.</summary>
        public static T ScaleEffect<T>(this T element, float x, State<float> y) where T : UIElement
        {
            return (T)element.WithScaleEffect(x, y);
        }

        /// <summary>Reactive scale with bound <see cref="Vector3"/>.</summary>
        public static T ScaleEffect<T>(this T element, State<Vector3> scale) where T : UIElement
        {
            return (T)element.WithScaleEffect(scale);
        }

        /// <summary>Sets absolute position (top-left origin, Y down).</summary>
        public static T Position<T>(this T element, float x, float y) where T : UIElement
        {
            return (T)element.WithPosition(x, y);
        }

        /// <summary>Reactive position from <see cref="State{Vector2}"/>.</summary>
        public static T Position<T>(this T element, State<Vector2> position) where T : UIElement
        {
            return (T)element.WithPosition(position);
        }

        /// <summary>Reactive position with bound X.</summary>
        public static T Position<T>(this T element, State<float> x, float y) where T : UIElement
        {
            return (T)element.WithPosition(x, y);
        }

        /// <summary>Reactive position with bound Y.</summary>
        public static T Position<T>(this T element, float x, State<float> y) where T : UIElement
        {
            return (T)element.WithPosition(x, y);
        }

    }
}
