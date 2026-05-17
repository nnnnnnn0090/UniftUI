using UnityEngine;

namespace UniftUI
{
    public interface IButtonStyle
    {
        void Apply(ButtonElement button);
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

        public void Apply(ButtonElement button)
        {
            if (button == null)
                return;

            button.SetBackgroundColor(backgroundColor);
            button.SetTextColor(foregroundColor);
            button.WithCornerRadius(cornerRadius);
        }
    }

    public sealed class PlainButtonStyle : IButtonStyle
    {
        private readonly Color foregroundColor;

        public PlainButtonStyle(Color foregroundColor)
        {
            this.foregroundColor = foregroundColor;
        }

        public void Apply(ButtonElement button)
        {
            if (button == null)
                return;

            button.SetBackgroundColor(Color.clear);
            button.SetTextColor(foregroundColor);
            button.WithCornerRadius(0f);
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
            textField.SetSelectionColor(new Color(tintColor.r, tintColor.g, tintColor.b, Mathf.Min(tintColor.a, 0.35f)));
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
            textField.SetSelectionColor(new Color(tintColor.r, tintColor.g, tintColor.b, Mathf.Min(tintColor.a, 0.35f)));
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
                textField.SetSelectionColor(new Color(tint.r, tint.g, tint.b, Mathf.Min(tint.a, 0.35f)));
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
        public static IButtonStyle Filled(Color backgroundColor, Color foregroundColor, float cornerRadius = 8f)
            => new FilledButtonStyle(backgroundColor, foregroundColor, cornerRadius);

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
