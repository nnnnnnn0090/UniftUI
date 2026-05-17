using System;
using TMPro;

namespace UniftUI
{
    /// <summary>Multiline text editor bound to <see cref="State{T}"/>, matching SwiftUI's TextEditor role.</summary>
    public class TextEditorElement : TextFieldElement
    {
        protected override string ElementName => "TextEditor";
        protected override float DefaultPreferredHeight => 96f;

        public TextEditorElement(State<string> text, Action<string> onTextChanged = null)
            : base(string.Empty, text, null, onTextChanged)
        {
            SetLineLimit(null);
            SetTextWrappingMode(TextWrappingModes.Normal, TextOverflowModes.Overflow);
            SetTextAlignment(TextAlignmentOptions.TopLeft);
            SetTextFieldPadding(10f, 10f, 8f, 8f);
        }
    }
}
