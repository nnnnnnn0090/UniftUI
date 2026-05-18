using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

namespace UniftUI
{
    internal static partial class UIElementExtensions
    {
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
                textFieldElement.SetSelectionColor(UniftUIColors.SelectionTint(color));
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

    }
}
