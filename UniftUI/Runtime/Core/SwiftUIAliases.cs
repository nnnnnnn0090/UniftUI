using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniftUI
{
    /// <summary>
    /// SwiftUI-style lower-camel aliases for the main UniftUI modifiers.
    /// The public examples use these methods so chains can stay close to SwiftUI,
    /// for example <c>Text("Name").italic().foregroundColor(...)</c>.
    /// </summary>
    public static class SwiftUIAliases
    {
        public static T frame<T>(this T element, float? width = null, float? height = null,
            bool? infiniteWidth = null, bool? infiniteHeight = null) where T : UIElement
            => element.Frame(width, height, infiniteWidth, infiniteHeight);

        public static T frame<T>(this T element, State<float> width = null, State<float> height = null) where T : UIElement
            => element.Frame(width, height);

        public static T frame<T>(this T element, float? minWidth = null, float? maxWidth = null,
            float? minHeight = null, float? maxHeight = null) where T : UIElement
            => (T)element.WithFrameConstraints(minWidth, maxWidth, minHeight, maxHeight);

        public static BackgroundElement background<T>(this T element, Color color) where T : UIElement
            => element.Background(color);

        public static BackgroundElement background<T>(this T element, State<Color> color) where T : UIElement
            => element.Background(color);

        public static BackgroundElement background(this ButtonElement element, Color color)
            => element.Background(color);

        public static BackgroundElement background(this ButtonElement element, State<Color> color)
            => element.Background(color);

        public static BackgroundElement background(this TextFieldElement element, Color color)
            => element.Background(color);

        public static BackgroundElement background(this TextFieldElement element, State<Color> color)
            => element.Background(color);

        public static BackgroundElement background(this TextElement element, Color color)
            => element.Background(color);

        public static BackgroundElement background(this TextElement element, State<Color> color)
            => element.Background(color);

        public static BackgroundElement background(this ToggleElement element, Color color)
            => element.Background(color);

        public static BackgroundContentElement background<T>(this T element, UIElement background,
            ZStackAlignment alignment = ZStackAlignment.Center) where T : UIElement
            => element.Background(background, alignment);

        public static BackgroundContentElement background<T>(this T element, Action background,
            ZStackAlignment alignment = ZStackAlignment.Center) where T : UIElement
            => element.Background(background, alignment);

        public static T foregroundColor<T>(this T element, Color color) where T : UIElement
            => element.ForegroundColor(color);

        public static T foregroundColor<T>(this T element, State<Color> color) where T : UIElement
            => element.ForegroundColor(color);

        public static T tint<T>(this T element, Color color) where T : UIElement
            => element.Tint(color);

        public static T tint<T>(this T element, State<Color> color) where T : UIElement
            => element.Tint(color);

        public static PaddingElement padding<T>(this T element, int padding) where T : UIElement
            => element.Padding(padding);

        public static PaddingElement padding<T>(this T element, State<int> padding) where T : UIElement
            => element.Padding(padding);

        public static PaddingElement padding<T>(this T element, RectOffset padding) where T : UIElement
            => element.Padding(padding);

        public static PaddingElement padding<T>(this T element, Edge edges, int length) where T : UIElement
            => element.Padding(EdgePadding(edges, length));

        public static PaddingElement padding<T>(this T element, float? top = null, float? bottom = null,
            float? left = null, float? right = null) where T : UIElement
            => element.Padding(top, bottom, left, right);

        public static FixedSizeElement fixedSize<T>(this T element, bool horizontal = true, bool vertical = true) where T : UIElement
            => element.FixedSize(horizontal, vertical);

        public static T bold<T>(this T element) where T : UIElement => element.Bold();

        public static T italic<T>(this T element) where T : UIElement => element.Italic();

        public static T underline<T>(this T element) where T : UIElement => element.Underline();

        public static T strikethrough<T>(this T element) where T : UIElement => element.Strikethrough();

        public static TextElement lineLimit(this TextElement element, int? limit = null)
            => element.LineLimit(limit);

        public static TextFieldElement lineLimit(this TextFieldElement element, int? limit = null)
            => element.SetLineLimit(limit);

        public static TextElement multilineTextAlignment(this TextElement element, TextAlignmentOptions alignment)
            => element.MultilineTextAlignment(alignment);

        public static TextFieldElement multilineTextAlignment(this TextFieldElement element, TextAlignmentOptions alignment)
            => element.SetTextAlignment(alignment);

        public static OpacityElement opacity<T>(this T element, float opacity) where T : UIElement
            => element.Opacity(opacity);

        public static OpacityElement opacity<T>(this T element, State<float> opacity) where T : UIElement
            => element.Opacity(opacity);

        public static ImageElement opacity(this ImageElement element, float opacity)
            => element.Opacity(opacity);

        public static ImageElement opacity(this ImageElement element, State<float> opacity)
            => element.Opacity(opacity);

        public static T fontSize<T>(this T element, float size) where T : UIElement
            => element.FontSize(size);

        public static T font<T>(this T element, TMP_FontAsset font) where T : UIElement
            => element.Font(font);

        public static T cornerRadius<T>(this T element, float radius) where T : UIElement
            => element.CornerRadius(radius);

        public static T cornerRadius<T>(this T element, State<float> radius) where T : UIElement
            => element.CornerRadius(radius);

        public static T cornerRadius<T>(this T element, float topLeft, float topRight, float bottomRight, float bottomLeft)
            where T : UIElement
            => element.CornerRadius(topLeft, topRight, bottomRight, bottomLeft);

        public static T cornerRadius<T>(this T element, RectCorner corners, float radius) where T : UIElement
            => element.CornerRadius(corners, radius);

        public static UIElement shadow<T>(this T element, Color? color = null, float radius = 3f, float x = 0f, float y = 0f)
            where T : UIElement
            => element.Shadow(color, radius, x, y);

        public static BorderElement border<T>(this T element, Color color, float width = 1f) where T : UIElement
            => element.Border(color, width);

        public static BorderElement border<T>(this T element, State<Color> color, float width = 1f) where T : UIElement
            => element.Border(color, width);

        public static OverlayElement overlay<T>(this T element, UIElement overlay, ZStackAlignment alignment = ZStackAlignment.Center)
            where T : UIElement
            => element.Overlay(overlay, alignment);

        public static OverlayElement overlay<T>(this T element, Action overlay, ZStackAlignment alignment = ZStackAlignment.Center)
            where T : UIElement
            => element.Overlay(overlay, alignment);

        public static OffsetElement offset<T>(this T element, float x, float y) where T : UIElement
            => element.Offset(x, y);

        public static OffsetElement offset<T>(this T element, Vector2 offset) where T : UIElement
            => element.Offset(offset);

        public static OffsetElement offset<T>(this T element, State<Vector2> offset) where T : UIElement
            => element.Offset(offset);

        public static OffsetElement offset<T>(this T element, State<float> x, float y) where T : UIElement
            => element.Offset(x, y);

        public static AspectRatioElement aspectRatio<T>(this T element, float ratio,
            AspectRatioContentMode contentMode = AspectRatioContentMode.Fit) where T : UIElement
            => element.AspectRatio(ratio, contentMode);

        public static AspectRatioElement aspectRatio<T>(this T element,
            AspectRatioContentMode contentMode = AspectRatioContentMode.Fit) where T : UIElement
            => element.AspectRatio(1f, contentMode);

        public static ClippedElement clipped<T>(this T element) where T : UIElement
            => element.Clipped();

        public static ClippedElement clipShape<T>(this T element, UniftUIClipShape shape, float cornerRadius = 12f)
            where T : UIElement
            => element.ClipShape(shape, cornerRadius);

        public static T disabled<T>(this T element, bool disabled = true) where T : UIElement
            => element.Disabled(disabled);

        public static T disabled<T>(this T element, State<bool> disabled) where T : UIElement
            => element.Disabled(disabled);

        public static T hidden<T>(this T element, bool hidden = true) where T : UIElement
            => (T)element.WithHidden(hidden);

        public static T layoutPriority<T>(this T element, float priority) where T : UIElement
            => (T)element.WithLayoutPriority(priority);

        public static T allowsHitTesting<T>(this T element, bool enabled) where T : UIElement
            => element.AllowsHitTesting(enabled);

        public static T allowsHitTesting<T>(this T element, State<bool> enabled) where T : UIElement
            => element.AllowsHitTesting(enabled);

        public static T onChange<T>(this T element, State state, Action action) where T : UIElement
            => element.OnChange(state, action);

        public static T onChange<T, TValue>(this T element, State<TValue> state, Action<TValue> action) where T : UIElement
            => element.OnChange(state, action);

        public static T onAppear<T>(this T element, Action action) where T : UIElement
            => element.OnAppear(action);

        public static T onAppear<T>(this T element, Func<Task> action) where T : UIElement
            => element.OnAppear(action);

        public static T update<T>(this T element, Action action) where T : UIElement
            => element.Update(action);

        public static T animation<T>(this T element, float duration) where T : UIElement
            => element.Animation(duration);

        public static T animation<T>(this T element, AnimationEasing easing, float duration) where T : UIElement
            => element.Animation(easing, duration);

        public static T animation<T>(this T element, Animation animation, State value) where T : UIElement
            => UIElementExtensions.Animation(element, animation, value);

        public static T animation<T>(this T element, Animation animation, State value0, State value1) where T : UIElement
            => UIElementExtensions.Animation(element, animation, value0, value1);

        public static T animation<T>(this T element, State value) where T : UIElement
            => UIElementExtensions.Animation(element, value);

        public static T animation<T, TValue>(this T element, State<TValue> value) where T : UIElement
            => UIElementExtensions.Animation(element, value);

        public static T rotationEffect<T>(this T element, float degrees) where T : UIElement
            => element.RotationEffect(degrees);

        public static T rotationEffect<T>(this T element, float x, float y, float z) where T : UIElement
            => element.RotationEffect(x, y, z);

        public static T rotationEffect<T>(this T element, State<float> degrees) where T : UIElement
            => element.RotationEffect(degrees);

        public static T rotationEffect<T>(this T element, State<float> x, float y, float z) where T : UIElement
            => element.RotationEffect(x, y, z);

        public static T rotationEffect<T>(this T element, float x, State<float> y, float z) where T : UIElement
            => element.RotationEffect(x, y, z);

        public static T rotationEffect<T>(this T element, float x, float y, State<float> z) where T : UIElement
            => element.RotationEffect(x, y, z);

        public static T rotationEffect<T>(this T element, State<Vector3> euler) where T : UIElement
            => element.RotationEffect(euler);

        public static T scaleEffect<T>(this T element, float scale) where T : UIElement
            => element.ScaleEffect(scale);

        public static T scaleEffect<T>(this T element, float x, float y) where T : UIElement
            => element.ScaleEffect(x, y);

        public static T scaleEffect<T>(this T element, float x, float y, float z) where T : UIElement
            => element.ScaleEffect(x, y, z);

        public static T scaleEffect<T>(this T element, Vector3 scale) where T : UIElement
            => element.ScaleEffect(scale);

        public static T scaleEffect<T>(this T element, State<float> scale) where T : UIElement
            => element.ScaleEffect(scale);

        public static T scaleEffect<T>(this T element, State<float> x, float y) where T : UIElement
            => element.ScaleEffect(x, y);

        public static T scaleEffect<T>(this T element, float x, State<float> y) where T : UIElement
            => element.ScaleEffect(x, y);

        public static T scaleEffect<T>(this T element, State<Vector3> scale) where T : UIElement
            => element.ScaleEffect(scale);

        public static T position<T>(this T element, float x, float y) where T : UIElement
            => element.Position(x, y);

        public static T position<T>(this T element, State<Vector2> position) where T : UIElement
            => element.Position(position);

        public static T position<T>(this T element, State<float> x, float y) where T : UIElement
            => element.Position(x, y);

        public static T position<T>(this T element, float x, State<float> y) where T : UIElement
            => element.Position(x, y);

        public static ButtonElement buttonStyle(this ButtonElement element, IButtonStyle style)
            => element.ButtonStyle(style);

        public static TextFieldElement textFieldStyle(this TextFieldElement element, ITextFieldStyle style)
            => element.TextFieldStyle(style);

        public static TextFieldElement focused(this TextFieldElement element, State<bool> isFocused)
            => element.SetFocused(isFocused);

        public static TextFieldElement onEditingChanged(this TextFieldElement element, Action<bool> action)
            => element.SetOnEditingChanged(action);

        public static TextFieldElement onSubmit(this TextFieldElement element, Action<string> action)
            => element.SetOnSubmit(action);

        public static TextFieldElement selectAllOnFocus(this TextFieldElement element, bool enabled = true)
            => element.SetSelectAllOnFocus(enabled);

        public static TextFieldElement textSelectionColor(this TextFieldElement element, Color color)
            => element.SetSelectionColor(color);

        public static TextFieldElement textSelectionColor(this TextFieldElement element, State<Color> color)
        {
            if (color == null)
                return element;

            element.SetSelectionColor(color.Value);
            element.AddPropertyBinding(color, () =>
            {
                element.SetSelectionColor(color.Value);
            }, "textFieldTextSelectionColor", BindingKind.Visual);
            return element;
        }

        public static TextFieldElement contentMargins(this TextFieldElement element, float horizontal, float vertical)
            => element.SetTextFieldPadding(horizontal, vertical);

        public static TextFieldElement contentMargins(this TextFieldElement element, float left, float right, float top, float bottom)
            => element.SetTextFieldPadding(left, right, top, bottom);

        public static TextFieldElement textContentType(this TextFieldElement element, TMP_InputField.ContentType type)
            => element.SetContentType(type);

        public static TextFieldElement textInputLimit(this TextFieldElement element, int limit)
            => element.SetCharacterLimit(limit);

        public static TextFieldElement caretColor(this TextFieldElement element, Color color)
            => element.SetCaretColor(color);

        public static TextFieldElement caretColor(this TextFieldElement element, State<Color> color)
        {
            if (color == null)
                return element;

            element.SetCaretColor(color.Value);
            element.AddPropertyBinding(color, () =>
            {
                element.SetCaretColor(color.Value);
            }, "textFieldCaretColor", BindingKind.Visual);
            return element;
        }

        public static TextFieldElement caretWidth(this TextFieldElement element, int width)
            => element.SetCaretWidth(width);

        public static TextFieldElement caretBlinkRate(this TextFieldElement element, float rate)
            => element.SetCaretBlinkRate(rate);

        public static TextFieldElement keyboardType(this TextFieldElement element, TouchScreenKeyboardType type)
            => element.SetKeyboardType(type);

        public static ImageElement resizable(this ImageElement element, ImageResizingMode resizingMode = ImageResizingMode.Stretch)
            => element.Resizable(resizingMode);

        public static ImageElement scaledToFit(this ImageElement element)
            => element.WithScaleMode(ImageScaleMode.AspectFit);

        public static ImageElement scaledToFill(this ImageElement element)
            => element.WithScaleMode(ImageScaleMode.AspectFill);

        public static ImageElement renderingMode(this ImageElement element, ImageRenderingMode mode)
            => element.WithRenderingMode(mode);

        public static PickerElement pickerStyle(this PickerElement element, PickerStyle style)
            => element.SetPickerStyle(style);

        public static ScrollViewElement scrollBounce(this ScrollViewElement element, bool elastic)
            => element.ScrollBounce(elastic);

        public static ScrollViewElement scrollSensitivity(this ScrollViewElement element, float sensitivity)
            => element.ScrollSensitivity(sensitivity);

        public static ScrollViewElement scrollMovementType(this ScrollViewElement element, ScrollRect.MovementType type)
            => element.ScrollMovementType(type);

        public static ScrollViewElement scrollPositionY(this ScrollViewElement element, State<float> normalized, bool twoWay = false)
            => element.ScrollPositionY(normalized, twoWay);

        public static ScrollViewElement scrollPositionX(this ScrollViewElement element, State<float> normalized, bool twoWay = false)
            => element.ScrollPositionX(normalized, twoWay);

        public static ScrollViewElement scrollIndicators(this ScrollViewElement element, ScrollIndicatorVisibility visibility)
            => element.ScrollIndicators(visibility);

        public static ScrollViewElement scrollIndicators(this ScrollViewElement element, ScrollIndicatorVisibility visibility, UniftUIScrollAxis axes)
            => element.ScrollIndicators(visibility, axes);

        private static RectOffset EdgePadding(Edge edges, int length)
        {
            int top = 0;
            int bottom = 0;
            int left = 0;
            int right = 0;

            switch (edges)
            {
                case Edge.Top:
                    top = length;
                    break;
                case Edge.Bottom:
                    bottom = length;
                    break;
                case Edge.Leading:
                    left = length;
                    break;
                case Edge.Trailing:
                    right = length;
                    break;
                case Edge.Horizontal:
                    left = length;
                    right = length;
                    break;
                case Edge.Vertical:
                    top = length;
                    bottom = length;
                    break;
                case Edge.All:
                    top = bottom = left = right = length;
                    break;
            }

            return new RectOffset(left, right, top, bottom);
        }
    }
}
