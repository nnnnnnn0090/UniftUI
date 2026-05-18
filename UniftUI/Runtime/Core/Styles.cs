using UnityEngine;

namespace UniftUI
{
    public interface IButtonStyle
    {
        UIElement MakeBody(ButtonStyleConfiguration configuration);
    }

    public sealed class ButtonStyleConfiguration
    {
        public ButtonStyleConfiguration(UIElement label, bool isPressed, bool isHovered)
        {
            Label = label;
            IsPressed = isPressed;
            IsHovered = isHovered;
        }

        public UIElement Label { get; }
        public bool IsPressed { get; }
        public bool IsHovered { get; }
        public UIElement label => Label;
        public bool isPressed => IsPressed;
        public bool isHovered => IsHovered;
    }

    public sealed class DefaultButtonStyle : IButtonStyle
    {
        public UIElement MakeBody(ButtonStyleConfiguration configuration)
        {
            if (configuration == null)
                return null;

            UIElement label = configuration.Label;
            return configuration.IsPressed ? label.Opacity(0.45f) : label;
        }
    }

    public interface ITextFieldStyle
    {
        void Apply(TextFieldElement textField);
    }

    public sealed class FilledButtonStyle : IButtonStyle
    {
        private readonly Color backgroundColor;
        private readonly Color foregroundColor;
        private readonly float cornerRadius;

        public FilledButtonStyle(Color backgroundColor, Color foregroundColor, float cornerRadius = 8f)
        {
            this.backgroundColor = backgroundColor;
            this.foregroundColor = foregroundColor;
            this.cornerRadius = cornerRadius;
        }

        public UIElement MakeBody(ButtonStyleConfiguration configuration)
        {
            if (configuration == null)
                return null;

            Color fill = configuration.IsPressed ? UniftUIColors.ScaleRgb(backgroundColor, 0.88f) : backgroundColor;
            return configuration.Label
                .ForegroundColor(foregroundColor)
                .Padding(top: 5f, bottom: 5f, left: 10f, right: 10f)
                .Background(fill)
                .CornerRadius(cornerRadius);
        }
    }

    public sealed class PlainButtonStyle : IButtonStyle
    {
        private readonly Color? foregroundColor;

        public PlainButtonStyle()
        {
        }

        public PlainButtonStyle(Color foregroundColor)
        {
            this.foregroundColor = foregroundColor;
        }

        public UIElement MakeBody(ButtonStyleConfiguration configuration)
        {
            if (configuration == null)
                return null;

            UIElement label = configuration.Label;
            if (foregroundColor.HasValue)
                label = label.ForegroundColor(foregroundColor.Value);
            return configuration.IsPressed ? label.Opacity(0.45f) : label;
        }
    }

    public sealed class RoundedBorderTextFieldStyle : ITextFieldStyle
    {
        private readonly Color backgroundColor;
        private readonly Color focusedBackgroundColor;
        private readonly Color textColor;
        private readonly Color tintColor;
        private readonly float cornerRadius;
        private readonly Vector4 margins;

        public RoundedBorderTextFieldStyle(
            Color backgroundColor,
            Color focusedBackgroundColor,
            Color textColor,
            Color tintColor,
            float cornerRadius = 8f,
            float horizontalMargin = 12f,
            float verticalMargin = 7f)
        {
            this.backgroundColor = backgroundColor;
            this.focusedBackgroundColor = focusedBackgroundColor;
            this.textColor = textColor;
            this.tintColor = tintColor;
            this.cornerRadius = cornerRadius;
            this.margins = new Vector4(horizontalMargin, horizontalMargin, verticalMargin, verticalMargin);
        }

        public void Apply(TextFieldElement textField)
        {
            if (textField == null)
                return;

            textField.SetBackgroundColor(backgroundColor);
            textField.SetFocusedBackgroundColor(focusedBackgroundColor);
            textField.SetTextColor(textColor);
            textField.SetCaretColor(tintColor);
            textField.SetSelectionColor(UniftUIColors.SelectionTint(tintColor));
            textField.SetTextFieldPadding(margins.x, margins.y, margins.z, margins.w);
            textField.WithCornerRadius(cornerRadius);
        }
    }

    public sealed class PlainTextFieldStyle : ITextFieldStyle
    {
        private readonly Color textColor;
        private readonly Color tintColor;

        public PlainTextFieldStyle(Color textColor, Color tintColor)
        {
            this.textColor = textColor;
            this.tintColor = tintColor;
        }

        public void Apply(TextFieldElement textField)
        {
            if (textField == null)
                return;

            textField.SetBackgroundColor(Color.clear);
            textField.SetFocusedBackgroundColor(Color.clear);
            textField.SetTextColor(textColor);
            textField.SetCaretColor(tintColor);
            textField.SetSelectionColor(UniftUIColors.SelectionTint(tintColor));
            textField.SetTextFieldPadding(0f, 0f, 0f, 0f);
            textField.WithCornerRadius(0f);
        }
    }

    public sealed class ChromeTextFieldStyle : ITextFieldStyle
    {
        private readonly Color? backgroundColor;
        private readonly Color? focusedBackgroundColor;
        private readonly Color? textColor;
        private readonly Color? tintColor;
        private readonly Color? textSelectionColor;
        private readonly Vector4? contentMargins;
        private readonly float? cornerRadius;
        private readonly int? caretWidth;
        private readonly float? caretBlinkRate;

        public ChromeTextFieldStyle(
            Color? backgroundColor = null,
            Color? focusedBackgroundColor = null,
            Color? textColor = null,
            Color? tintColor = null,
            Color? textSelectionColor = null,
            Vector4? contentMargins = null,
            float? cornerRadius = null,
            int? caretWidth = null,
            float? caretBlinkRate = null)
        {
            this.backgroundColor = backgroundColor;
            this.focusedBackgroundColor = focusedBackgroundColor;
            this.textColor = textColor;
            this.tintColor = tintColor;
            this.textSelectionColor = textSelectionColor;
            this.contentMargins = contentMargins;
            this.cornerRadius = cornerRadius;
            this.caretWidth = caretWidth;
            this.caretBlinkRate = caretBlinkRate;
        }

        public void Apply(TextFieldElement textField)
        {
            if (textField == null)
                return;

            if (backgroundColor.HasValue)
                textField.SetBackgroundColor(backgroundColor.Value);
            if (focusedBackgroundColor.HasValue)
                textField.SetFocusedBackgroundColor(focusedBackgroundColor.Value);
            if (textColor.HasValue)
                textField.SetTextColor(textColor.Value);
            if (tintColor.HasValue)
            {
                Color tint = tintColor.Value;
                textField.SetCaretColor(tint);
                textField.SetSelectionColor(UniftUIColors.SelectionTint(tint));
            }
            if (textSelectionColor.HasValue)
                textField.SetSelectionColor(textSelectionColor.Value);
            if (contentMargins.HasValue)
            {
                Vector4 margins = contentMargins.Value;
                textField.SetTextFieldPadding(margins.x, margins.y, margins.z, margins.w);
            }
            if (cornerRadius.HasValue)
                textField.WithCornerRadius(cornerRadius.Value);
            if (caretWidth.HasValue)
                textField.SetCaretWidth(caretWidth.Value);
            if (caretBlinkRate.HasValue)
                textField.SetCaretBlinkRate(caretBlinkRate.Value);
        }
    }

    public static class ButtonStyles
    {
        public static IButtonStyle Automatic()
            => new DefaultButtonStyle();

        public static IButtonStyle Filled(Color backgroundColor, Color foregroundColor, float cornerRadius = 8f)
            => new FilledButtonStyle(backgroundColor, foregroundColor, cornerRadius);

        public static IButtonStyle Plain()
            => new PlainButtonStyle();

        public static IButtonStyle Plain(Color foregroundColor)
            => new PlainButtonStyle(foregroundColor);
    }

    public static class TextFieldStyles
    {
        public static ITextFieldStyle RoundedBorder(
            Color backgroundColor,
            Color focusedBackgroundColor,
            Color textColor,
            Color tintColor,
            float cornerRadius = 8f)
            => new RoundedBorderTextFieldStyle(backgroundColor, focusedBackgroundColor, textColor, tintColor, cornerRadius);

        public static ITextFieldStyle Plain(Color textColor, Color tintColor)
            => new PlainTextFieldStyle(textColor, tintColor);

        public static ITextFieldStyle Chrome(
            Color? backgroundColor = null,
            Color? focusedBackgroundColor = null,
            Color? textColor = null,
            Color? tintColor = null,
            Color? textSelectionColor = null,
            Vector4? contentMargins = null,
            float? cornerRadius = null,
            int? caretWidth = null,
            float? caretBlinkRate = null)
            => new ChromeTextFieldStyle(backgroundColor, focusedBackgroundColor, textColor, tintColor,
                textSelectionColor, contentMargins, cornerRadius, caretWidth, caretBlinkRate);
    }
}
