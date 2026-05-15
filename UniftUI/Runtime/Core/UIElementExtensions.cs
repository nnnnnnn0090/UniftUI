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
    public static class UIElementExtensions
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

        /// <summary>Wraps the element in a background layer.</summary>
        public static BackgroundElement Background<T>(this T element, Color color) where T : UIElement
        {
            var backgroundElement = new BackgroundElement(element, color);
            backgroundElement.preferredWidth = element.preferredWidth;
            backgroundElement.preferredHeight = element.preferredHeight;
            backgroundElement.infiniteWidth = element.infiniteWidth;
            backgroundElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, backgroundElement);
            return backgroundElement;
        }

        /// <summary>Wraps the element in a reactive background layer.</summary>
        public static BackgroundElement Background<T>(this T element, State<Color> color) where T : UIElement
        {
            var backgroundElement = new BackgroundElement(element, color.Value);
            backgroundElement.preferredWidth = element.preferredWidth;
            backgroundElement.preferredHeight = element.preferredHeight;
            backgroundElement.infiniteWidth = element.infiniteWidth;
            backgroundElement.infiniteHeight = element.infiniteHeight;

            backgroundElement.WithBackgroundColor(color);

            UIContext.Current?.ReplaceChild(element, backgroundElement);
            return backgroundElement;
        }

        /// <summary>Applies a visual offset by x and y.</summary>
        public static OffsetElement Offset<T>(this T element, float x, float y) where T : UIElement
        {
            var offsetElement = new OffsetElement(element, new Vector2(x, y));
            UIContext.Current?.ReplaceChild(element, offsetElement);
            return offsetElement;
        }

        /// <summary>Applies a visual offset from a vector.</summary>
        public static OffsetElement Offset<T>(this T element, Vector2 offset) where T : UIElement
        {
            var offsetElement = new OffsetElement(element, offset);
            UIContext.Current?.ReplaceChild(element, offsetElement);
            return offsetElement;
        }

        /// <summary>Reactive offset from <see cref="State{Vector2}"/>.</summary>
        public static OffsetElement Offset<T>(this T element, State<Vector2> offset) where T : UIElement
        {
            var offsetElement = new OffsetElement(element, offset);
            UIContext.Current?.ReplaceChild(element, offsetElement);
            return offsetElement;
        }

        /// <summary>Reactive offset with independent X state.</summary>
        public static OffsetElement Offset<T>(this T element, State<float> x, float y) where T : UIElement
        {
            var offsetElement = new OffsetElement(element, x, y);
            UIContext.Current?.ReplaceChild(element, offsetElement);
            return offsetElement;
        }

        /// <summary>Sets foreground color on text-like elements and propagates through layout containers.</summary>
        public static T ForegroundColor<T>(this T element, Color color) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetTextColor(color);
            }
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetTextColor(color);
            }
            else if (element is TextFieldElement textFieldElement)
            {
                textFieldElement.SetTextColor(color);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.ForegroundColor(color);
                }
            }
            return element;
        }

        /// <summary>Reactive foreground color.</summary>
        public static T ForegroundColor<T>(this T element, State<Color> color) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                if (color != null) {
                    textElement.SetTextColor(color.Value);

                    textElement.AddPropertyBinding(color, () => {
                        textElement.SetTextColor(color.Value);
                    }, "textColor");
                }
            }
            else if (element is ButtonElement buttonElement)
            {
                if (color != null) {
                    buttonElement.SetTextColor(color.Value);

                    buttonElement.AddPropertyBinding(color, () => {
                        buttonElement.SetTextColor(color.Value);
                    }, "buttonTextColor");
                }
            }
            else if (element is TextFieldElement textFieldElement)
            {
                if (color != null) {
                    textFieldElement.SetTextColor(color.Value);

                    textFieldElement.AddPropertyBinding(color, () => {
                        textFieldElement.SetTextColor(color.Value);
                    }, "textFieldTextColor");
                }
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.ForegroundColor(color);
                }
            }
            return element;
        }

        /// <summary>Wraps the element with uniform padding.</summary>
        public static PaddingElement Padding<T>(this T element, int padding) where T : UIElement
        {
            var paddingValue = new RectOffset(padding, padding, padding, padding);
            var paddingElement = new PaddingElement(element, paddingValue);
            paddingElement.preferredWidth = element.preferredWidth;
            paddingElement.preferredHeight = element.preferredHeight;
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }

        /// <summary>Wraps the element with reactive uniform padding.</summary>
        public static PaddingElement Padding<T>(this T element, State<int> padding) where T : UIElement
        {
            var paddingValue = new RectOffset(padding.Value, padding.Value, padding.Value, padding.Value);
            var paddingElement = new PaddingElement(element, paddingValue);
            paddingElement.preferredWidth = element.preferredWidth;
            paddingElement.preferredHeight = element.preferredHeight;
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            paddingElement.AddPropertyBinding(padding, () => {
                paddingElement.UpdatePadding(padding.Value);
            }, "padding");

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

        /// <summary>Wraps the element with explicit <see cref="RectOffset"/> padding.</summary>
        public static PaddingElement Padding<T>(this T element, RectOffset padding) where T : UIElement
        {
            var paddingElement = new PaddingElement(element, padding);
            paddingElement.preferredWidth = element.preferredWidth;
            paddingElement.preferredHeight = element.preferredHeight;
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
            paddingElement.preferredWidth = element.preferredWidth;
            paddingElement.preferredHeight = element.preferredHeight;
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }

        /// <summary>Applies bold to text elements and propagates through layout containers.</summary>
        public static T Bold<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetBold(true);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Bold();
                }
            }
            return element;
        }

        /// <summary>Applies italic to text elements and propagates through layout containers.</summary>
        public static T Italic<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetItalic(true);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Italic();
                }
            }
            return element;
        }

        /// <summary>Applies underline to text elements and propagates through layout containers.</summary>
        public static T Underline<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetUnderline(true);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Underline();
                }
            }
            return element;
        }

        /// <summary>Applies strikethrough to text elements and propagates through layout containers.</summary>
        public static T Strikethrough<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetStrikethrough(true);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Strikethrough();
                }
            }
            return element;
        }

        /// <summary>Sets image tint color.</summary>
        public static ImageElement TintColor(this ImageElement element, Color color)
        {
            return element.WithTintColor(color);
        }

        /// <summary>Sets image scale mode.</summary>
        public static ImageElement ScaleMode(this ImageElement element, ImageScaleMode mode)
        {
            return element.WithScaleMode(mode);
        }

        /// <summary>Sets image opacity.</summary>
        public static ImageElement Opacity(this ImageElement element, float opacity)
        {
            return element.WithOpacity(opacity);
        }

        /// <summary>Sets view opacity (delegates to <see cref="UIElement.WithOpacity"/>).</summary>
        public static T Opacity<T>(this T element, float opacity) where T : UIElement
        {
            if (element is ImageElement)
            {
                return element;
            }
            return (T)element.WithOpacity(opacity);
        }

        /// <summary>Reactive opacity.</summary>
        public static T Opacity<T>(this T element, State<float> opacity) where T : UIElement
        {
            if (element is ImageElement)
            {
                return element;
            }
            return (T)element.WithOpacity(opacity);
        }

        /// <summary>Sets font size on text-like elements and propagates through layout containers.</summary>
        public static T FontSize<T>(this T element, float size) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetFontSize(size);
            }
            else if (element is TextFieldElement textFieldElement)
            {
                textFieldElement.SetFontSize(size);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.FontSize(size);
                }
            }
            return element;
        }

        /// <summary>
        /// Sets font on text-like elements within this subtree. Does not change global <see cref="UIContext.DefaultFont"/>.
        /// </summary>
        public static T Font<T>(this T element, TMP_FontAsset font) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetFont(font);
            }
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetFont(font);
            }
            else if (element is ToggleElement toggleElement)
            {
                toggleElement.SetFont(font);
            }
            else if (element is TextFieldElement textFieldElement)
            {
                textFieldElement.SetFont(font);
            }
            else if (element is TabView tabView)
            {
                tabView.SetFont(font);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Font(font);
                }
            }

            return element;
        }

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

            shadowElement.preferredWidth = element.preferredWidth;
            shadowElement.preferredHeight = element.preferredHeight;
            shadowElement.infiniteWidth = element.infiniteWidth;
            shadowElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, shadowElement);
            return shadowElement;
        }

        /// <summary>Sets text field line limit.</summary>
        public static TextFieldElement LineLimit(this TextFieldElement element, int? limit = null)
        {
            return element.SetLineLimit(limit);
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

        /// <summary>Registers a synchronous callback when the view appears.</summary>
        public static T OnAppear<T>(this T element, Action action) where T : UIElement
        {
            return (T)element.WithOnAppear(action);
        }

        /// <summary>Registers an async callback when the view appears.</summary>
        public static T OnAppear<T>(this T element, Func<Task> asyncAction) where T : UIElement
        {
            return (T)element.WithOnAppearAsync(asyncAction);
        }

        /// <summary>Registers a per-frame update callback.</summary>
        public static T Update<T>(this T element, Action action) where T : UIElement
        {
            return (T)element.WithUpdate(action);
        }

        /// <summary>Enables implicit animation for property changes over <paramref name="duration"/> seconds.</summary>
        public static T Animation<T>(this T element, float duration) where T : UIElement
        {
            return (T)element.WithAnimation(duration);
        }

        /// <summary>Enables implicit animation with easing over <paramref name="duration"/> seconds.</summary>
        public static T Animation<T>(this T element, AnimationEasing easing, float duration) where T : UIElement
        {
            return (T)element.WithAnimation(easing, duration);
        }

        /// <summary>
        /// Binds an animation to a state so changes to that state are animated:
        /// animates this view when <paramref name="value"/> changes.
        /// </summary>
        public static T Animation<T>(this T element, Animation anim, State value) where T : UIElement
        {
            ((UIElement)element).Animation(anim, value);
            return element;
        }

        /// <summary>Animates property changes when <paramref name="value"/> changes using the default animation.</summary>
        public static T Animation<T>(this T element, State value) where T : UIElement
        {
            ((UIElement)element).Animation(global::UniftUI.Animation.Default, value);
            return element;
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
