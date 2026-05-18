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
    internal static class UIElementExtensions
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

        /// <summary>Wraps a button in a background layer, preserving modifier ordering.</summary>
        public static BackgroundElement Background(this ButtonElement button, Color color)
        {
            button.SetBackgroundColor(Color.clear);
            return WrapBackground(button, color);
        }

        /// <summary>Wraps a button in a reactive background layer, preserving modifier ordering.</summary>
        public static BackgroundElement Background(this ButtonElement button, State<Color> color)
        {
            button.SetBackgroundColor(Color.clear);
            return WrapBackground(button, color);
        }

        /// <summary>Wraps text in a background layer, preserving modifier ordering.</summary>
        public static BackgroundElement Background(this TextElement text, Color color)
        {
            return WrapBackground(text, color);
        }

        /// <summary>Wraps a text field background while leaving the input chrome customizable by modifiers.</summary>
        public static BackgroundElement Background(this TextFieldElement textField, Color color)
        {
            textField.SetBackgroundColor(Color.clear);
            return WrapBackground(textField, color);
        }

        /// <summary>Wraps text in a reactive background layer, preserving modifier ordering.</summary>
        public static BackgroundElement Background(this TextElement text, State<Color> color)
        {
            return WrapBackground(text, color);
        }

        /// <summary>Wraps a text field in a reactive background layer.</summary>
        public static BackgroundElement Background(this TextFieldElement textField, State<Color> color)
        {
            textField.SetBackgroundColor(Color.clear);
            return WrapBackground(textField, color);
        }

        /// <summary>Wraps a toggle in a background layer, preserving modifier ordering.</summary>
        public static BackgroundElement Background(this ToggleElement toggle, Color color)
        {
            return WrapBackground(toggle, color);
        }

        /// <summary>Wraps the element in a background layer (e.g. behind <see cref="ImageElement"/> in a frame).</summary>
        public static BackgroundElement Background<T>(this T element, Color color) where T : UIElement
        {
            return WrapBackground(element, color);
        }

        /// <summary>Wraps the element in a reactive background layer.</summary>
        public static BackgroundElement Background<T>(this T element, State<Color> color) where T : UIElement
        {
            return WrapBackground(element, color);
        }

        /// <summary>Places custom background content behind this element without changing layout.</summary>
        public static BackgroundContentElement Background<T>(this T element, UIElement background,
            ZStackAlignment alignment = ZStackAlignment.Center) where T : UIElement
        {
            UIContext.Current?.RemoveChild(background);
            var backgroundElement = new BackgroundContentElement(element, background, alignment);
            UIContext.Current?.ReplaceChild(element, backgroundElement);
            return backgroundElement;
        }

        /// <summary>Builds custom background content behind this element without changing layout.</summary>
        public static BackgroundContentElement Background<T>(this T element, Action background,
            ZStackAlignment alignment = ZStackAlignment.Center) where T : UIElement
        {
            var backgroundElement = new BackgroundContentElement(element, background, alignment);
            UIContext.Current?.ReplaceChild(element, backgroundElement);
            return backgroundElement;
        }

        private static BackgroundElement WrapBackground(UIElement element, Color color)
        {
            var backgroundElement = new BackgroundElement(element, color);
            backgroundElement.CopyFrameFrom(element);

            UIContext.Current?.ReplaceChild(element, backgroundElement);
            return backgroundElement;
        }

        private static BackgroundElement WrapBackground(UIElement element, State<Color> color)
        {
            if (color == null)
                return WrapBackground(element, Color.clear);

            var backgroundElement = new BackgroundElement(element, color.Value);
            backgroundElement.CopyFrameFrom(element);

            backgroundElement.WithBackgroundColor(color);

            UIContext.Current?.ReplaceChild(element, backgroundElement);
            return backgroundElement;
        }

        /// <summary>Draws a border around the element without changing its layout.</summary>
        public static BorderElement Border<T>(this T element, Color color, float width = 1f) where T : UIElement
        {
            var borderElement = new BorderElement(element, color, width);
            UIContext.Current?.ReplaceChild(element, borderElement);
            return borderElement;
        }

        /// <summary>Draws a reactive border around the element without changing its layout.</summary>
        public static BorderElement Border<T>(this T element, State<Color> color, float width = 1f) where T : UIElement
        {
            var borderElement = new BorderElement(element, color, width);
            UIContext.Current?.ReplaceChild(element, borderElement);
            return borderElement;
        }

        /// <summary>Places another element over this element without affecting layout.</summary>
        public static OverlayElement Overlay<T>(this T element, UIElement overlay, ZStackAlignment alignment = ZStackAlignment.Center)
            where T : UIElement
        {
            UIContext.Current?.RemoveChild(overlay);
            var overlayElement = new OverlayElement(element, overlay, alignment);
            UIContext.Current?.ReplaceChild(element, overlayElement);
            return overlayElement;
        }

        /// <summary>Places overlay content over this element without affecting layout.</summary>
        public static OverlayElement Overlay<T>(this T element, Action overlay, ZStackAlignment alignment = ZStackAlignment.Center)
            where T : UIElement
        {
            var overlayElement = new OverlayElement(element, overlay, alignment);
            UIContext.Current?.ReplaceChild(element, overlayElement);
            return overlayElement;
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
            else if (element is ToggleElement toggleElement)
            {
                toggleElement.SetTextColor(color);
            }
            else if (element is RectangleElement rectangleElement)
            {
                rectangleElement.SetFillColor(color);
            }
            else if (element is LabelElement labelElement)
            {
                labelElement.SetTextColor(color);
                labelElement.SetTintColor(color);
            }
            else if (element is StepperElement stepperElement)
            {
                stepperElement.SetTextColor(color);
            }
            else if (element is PickerElement pickerElement)
            {
                pickerElement.SetTextColor(color);
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
                if (color != null)
                {
                    textElement.SetTextColor(color.Value);

                    textElement.AddPropertyBinding(color, () =>
                    {
                        textElement.SetTextColor(color.Value);
                    }, "textColor", BindingKind.Visual);
                }
            }
            else if (element is ButtonElement buttonElement)
            {
                if (color != null)
                {
                    buttonElement.SetTextColor(color.Value);

                    buttonElement.AddPropertyBinding(color, () =>
                    {
                        buttonElement.SetTextColor(color.Value);
                    }, "buttonTextColor", BindingKind.Visual);
                }
            }
            else if (element is TextFieldElement textFieldElement)
            {
                if (color != null)
                {
                    textFieldElement.SetTextColor(color.Value);

                    textFieldElement.AddPropertyBinding(color, () =>
                    {
                        textFieldElement.SetTextColor(color.Value);
                    }, "textFieldTextColor", BindingKind.Visual);
                }
            }
            else if (element is RectangleElement rectangleElement)
            {
                if (color != null)
                {
                    rectangleElement.SetFillColor(color.Value);

                    rectangleElement.AddPropertyBinding(color, () =>
                    {
                        rectangleElement.SetFillColor(color.Value);
                    }, "rectangleFillColor", BindingKind.Visual);
                }
            }
            else if (element is ToggleElement toggleElement)
            {
                if (color != null)
                {
                    toggleElement.SetTextColor(color.Value);
                    toggleElement.AddPropertyBinding(color, () =>
                    {
                        toggleElement.SetTextColor(color.Value);
                    }, "toggleForegroundColor", BindingKind.Visual);
                }
            }
            else if (element is LabelElement labelElement)
            {
                if (color != null)
                {
                    labelElement.SetTextColor(color.Value);
                    labelElement.SetTintColor(color.Value);

                    labelElement.AddPropertyBinding(color, () =>
                    {
                        labelElement.SetTextColor(color.Value);
                        labelElement.SetTintColor(color.Value);
                    }, "labelForegroundColor", BindingKind.Visual);
                }
            }
            else if (element is StepperElement stepperElement)
            {
                if (color != null)
                {
                    stepperElement.SetTextColor(color.Value);
                    stepperElement.AddPropertyBinding(color, () =>
                    {
                        stepperElement.SetTextColor(color.Value);
                    }, "stepperForegroundColor", BindingKind.Visual);
                }
            }
            else if (element is PickerElement pickerElement)
            {
                if (color != null)
                {
                    pickerElement.SetTextColor(color.Value);
                    pickerElement.AddPropertyBinding(color, () =>
                    {
                        pickerElement.SetTextColor(color.Value);
                    }, "pickerForegroundColor", BindingKind.Visual);
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

        /// <summary>Applies accent tint to controls and accentable visuals.</summary>
        public static T Tint<T>(this T element, Color color) where T : UIElement
        {
            ApplyTint(element, color);
            return element;
        }

        /// <summary>Applies reactive accent tint to controls and accentable visuals.</summary>
        public static T Tint<T>(this T element, State<Color> color) where T : UIElement
        {
            if (color == null)
                return element;

            ApplyTint(element, color.Value);
            element.AddPropertyBinding(color, () =>
            {
                ApplyTint(element, color.Value);
            }, "tint_" + Guid.NewGuid().ToString("N"), BindingKind.Visual);
            return element;
        }

        private static void ApplyTint(UIElement element, Color color)
        {
            if (element is TextFieldElement textFieldElement)
            {
                textFieldElement.SetCaretColor(color);
                textFieldElement.SetSelectionColor(SelectionTint(color));
            }
            else if (element is ImageElement imageElement)
            {
                imageElement.WithTintColor(color);
            }
            else if (element is SliderElement sliderElement)
            {
                sliderElement.WithColors(color);
            }
            else if (element is ToggleElement toggleElement)
            {
                toggleElement.SetTintColor(color);
            }
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetBackgroundColor(color);
            }
            else if (element is RectangleElement rectangleElement)
            {
                rectangleElement.SetFillColor(color);
            }
            else if (element is ProgressViewElement progressViewElement)
            {
                progressViewElement.SetTintColor(color);
            }
            else if (element is StepperElement stepperElement)
            {
                stepperElement.SetTintColor(color);
            }
            else if (element is PickerElement pickerElement)
            {
                pickerElement.SetTintColor(color);
            }
            else if (element is LabelElement labelElement)
            {
                labelElement.SetTintColor(color);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    ApplyTint(child, color);
                }
            }
        }

        /// <summary>Disables interaction for this element subtree.</summary>
        public static T Disabled<T>(this T element, bool disabled = true) where T : UIElement
        {
            element.WithDisabled(disabled);
            return element;
        }

        /// <summary>Reactively disables interaction for this element subtree.</summary>
        public static T Disabled<T>(this T element, State<bool> disabled) where T : UIElement
        {
            element.WithDisabled(disabled);
            return element;
        }

        /// <summary>Controls whether this element subtree receives pointer events.</summary>
        public static T AllowsHitTesting<T>(this T element, bool enabled) where T : UIElement
        {
            element.WithAllowsHitTesting(enabled);
            return element;
        }

        /// <summary>Reactively controls whether this element subtree receives pointer events.</summary>
        public static T AllowsHitTesting<T>(this T element, State<bool> enabled) where T : UIElement
        {
            element.WithAllowsHitTesting(enabled);
            return element;
        }

        /// <summary>Runs an action after a state changes. The initial binding pass does not invoke it.</summary>
        public static T OnChange<T>(this T element, State state, Action action) where T : UIElement
        {
            if (state == null || action == null)
                return element;

            bool initialized = false;
            element.AddPropertyBinding(state, () =>
            {
                if (!initialized)
                {
                    initialized = true;
                    return;
                }

                action.Invoke();
            }, "onChange_" + Guid.NewGuid().ToString("N"), BindingKind.ObserveOnly);
            return element;
        }

        /// <summary>Runs an action with the new value after a state changes. The initial binding pass does not invoke it.</summary>
        public static T OnChange<T, TValue>(this T element, State<TValue> state, Action<TValue> action) where T : UIElement
        {
            if (state == null || action == null)
                return element;

            return element.OnChange(state, () => action.Invoke(state.Value));
        }

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
            style?.Apply(element);
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

        /// <summary>Applies bold to text elements and propagates through layout containers.</summary>
        public static T Bold<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetBold(true);
            }
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetBold(true);
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
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetItalic(true);
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
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetUnderline(true);
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
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetStrikethrough(true);
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

        /// <summary>Sets the maximum number of visible lines for text.</summary>
        public static TextElement LineLimit(this TextElement element, int? limit = null)
        {
            return element.SetLineLimit(limit);
        }

        /// <summary>Sets text alignment for multiline text.</summary>
        public static TextElement MultilineTextAlignment(this TextElement element, TextAlignmentOptions alignment)
        {
            return element.SetTextAlignment(alignment);
        }

        /// <summary>Sets image opacity.</summary>
        public static ImageElement Opacity(this ImageElement element, float opacity)
        {
            return element.WithOpacity(opacity);
        }

        /// <summary>Reactive image opacity.</summary>
        public static ImageElement Opacity(this ImageElement element, State<float> opacity)
        {
            return (ImageElement)element.WithOpacity(opacity);
        }

        /// <summary>Wraps a background subtree so opacity applies to the whole rendered result.</summary>
        public static OpacityElement Opacity(this BackgroundElement element, float opacity)
        {
            return WrapOpacity(element, opacity);
        }

        /// <summary>Wraps a background subtree with reactive opacity.</summary>
        public static OpacityElement Opacity(this BackgroundElement element, State<float> opacity)
        {
            return WrapOpacity(element, opacity);
        }

        /// <summary>Wraps a padded subtree so opacity applies after padding.</summary>
        public static OpacityElement Opacity(this PaddingElement element, float opacity)
        {
            return WrapOpacity(element, opacity);
        }

        /// <summary>Wraps a padded subtree with reactive opacity.</summary>
        public static OpacityElement Opacity(this PaddingElement element, State<float> opacity)
        {
            return WrapOpacity(element, opacity);
        }

        /// <summary>Wraps the current subtree so opacity follows modifier order.</summary>
        public static OpacityElement Opacity<T>(this T element, float opacity) where T : UIElement
        {
            return WrapOpacity(element, opacity);
        }

        /// <summary>Wraps the current subtree with reactive opacity.</summary>
        public static OpacityElement Opacity<T>(this T element, State<float> opacity) where T : UIElement
        {
            return WrapOpacity(element, opacity);
        }

        private static OpacityElement WrapOpacity(UIElement element, float opacity)
        {
            var opacityElement = new OpacityElement(element, opacity);
            UIContext.Current?.ReplaceChild(element, opacityElement);
            return opacityElement;
        }

        private static OpacityElement WrapOpacity(UIElement element, State<float> opacity)
        {
            if (opacity == null)
                return WrapOpacity(element, 1f);

            var opacityElement = new OpacityElement(element, opacity);
            UIContext.Current?.ReplaceChild(element, opacityElement);
            return opacityElement;
        }

        /// <summary>Sets font size on text-like elements and propagates through layout containers.</summary>
        public static T FontSize<T>(this T element, float size) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetFontSize(size);
            }
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetFontSize(size);
            }
            else if (element is TextFieldElement textFieldElement)
            {
                textFieldElement.SetFontSize(size);
            }
            else if (element is ToggleElement toggleElement)
            {
                toggleElement.SetFontSize(size);
            }
            else if (element is LabelElement labelElement)
            {
                labelElement.SetFontSize(size);
            }
            else if (element is StepperElement stepperElement)
            {
                stepperElement.SetFontSize(size);
            }
            else if (element is PickerElement pickerElement)
            {
                pickerElement.SetFontSize(size);
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
            element.SetInheritedFont(font);

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
            else if (element is LabelElement labelElement)
            {
                labelElement.SetFont(font);
            }
            else if (element is StepperElement stepperElement)
            {
                stepperElement.SetFont(font);
            }
            else if (element is PickerElement pickerElement)
            {
                pickerElement.SetFont(font);
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
            shadowElement.CopyFrameFrom(element);

            UIContext.Current?.ReplaceChild(element, shadowElement);
            return shadowElement;
        }

        private static Color SelectionTint(Color color)
        {
            return new Color(color.r, color.g, color.b, Mathf.Min(color.a, 0.35f));
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
