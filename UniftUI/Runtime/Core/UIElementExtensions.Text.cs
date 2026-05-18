using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

namespace UniftUI
{
    internal static partial class UIElementExtensions
    {
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

    }
}
