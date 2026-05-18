using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Text input bound to <see cref="State{T}"/> with optional placeholder and styling.</summary>
    public class TextFieldElement : UIElement, IControlHitTargetSource
    {
        private State<string> text;
        private string placeholder;
        private TextElement promptElement;
        private Action<string> onTextChanged;
        private TMP_FontAsset fontAsset;
        private Color textColor = Color.black;
        private Color placeholderColor = new Color(0.5f, 0.5f, 0.5f);
        private bool hasPlaceholderColor;
        private float fontSize = 18f;
        private int? lineLimit;
        private bool hasLineLimit;

        private TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard;
        private int characterLimit;
        private bool placeholderItalic;
        private bool hasPlaceholderItalic;
        private Color caretColor = new Color(0.19f, 0.19f, 0.19f);
        private float caretBlinkRate = 0.85f;
        private int caretWidth = 2;
        private Color selectionColor = new Color(0.5f, 0.5f, 1f, 0.5f);
        private TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default;
        private Color focusedBackgroundColor;
        private bool hasFocusedBackgroundColor;
        private bool hasBackgroundColor;
        private bool selectAllOnFocus = true;
        private State<bool> focusedState;
        private Action<bool> onEditingChanged;
        private Action<string> onSubmit;
        private TextAlignmentOptions textAlignment = TextAlignmentOptions.MidlineLeft;
        private TextWrappingModes textWrappingMode = TextWrappingModes.NoWrap;
        private TextOverflowModes textOverflowMode = TextOverflowModes.Overflow;
        private Vector4 textInsets = new Vector4(8f, 8f, 4f, 4f); // left, right, top, bottom
        private TMP_Text builtTextComponent;
        private TMP_Text builtPlaceholderComponent;
        private TMP_InputField builtInputField;
        private Image builtBackgroundImage;
        private RectTransform builtTextViewport;
        private EventTrigger builtFocusTrigger;
        private bool syncingFocusFromInput;

        private static readonly Vector4 TextMaskPadding = new Vector4(-8f, -5f, -8f, -5f);
        protected virtual string ElementName => "TextField";
        protected virtual float DefaultPreferredHeight => 30f;

        internal TextFieldElement(State<string> text, string placeholder, Action<string> onTextChanged = null)
        {
            this.text = text;
            this.placeholder = placeholder;
            this.onTextChanged = onTextChanged;
            UIContext.Add(this);
        }

        bool IControlHitTargetSource.TryGetControlHitTarget(out ControlHitTarget target)
        {
            target = new ControlHitTarget(FocusInputField, canReceiveInput: IsInputAllowed);
            return true;
        }

        private void FocusInputField()
        {
            if (!IsInputAllowed() || builtInputField == null)
                return;

            builtInputField.Select();
            builtInputField.ActivateInputField();
        }

        public TextFieldElement(string title, State<string> text, TextElement prompt = null, Action<string> onTextChanged = null)
        {
            this.text = text;
            this.placeholder = title;
            this.promptElement = prompt;
            this.onTextChanged = onTextChanged;
            UIContext.Current?.RemoveChild(prompt);
            UIContext.Add(this);
        }

        public TextFieldElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            if (font != null)
            {
                if (builtTextComponent != null)
                    builtTextComponent.font = font;
                if (builtPlaceholderComponent != null && (promptElement == null || !promptElement.HasExplicitFont))
                    builtPlaceholderComponent.font = font;
                if (builtInputField != null)
                    builtInputField.fontAsset = font;
            }
            return this;
        }

        public TextFieldElement SetTextColor(Color color)
        {
            if (useAnimation && builtTextComponent != null && animationDuration > 0)
            {
                TextColorAnimator animator = BaseAnimator<Color>.GetOrReplace<TextColorAnimator>(builtTextComponent.gameObject);
                animator.AnimateTo(builtTextComponent.color, color, animationDuration, animationEasing);
                textColor = color;
            }
            else
            {
                textColor = color;
                if (builtTextComponent != null)
                    builtTextComponent.color = color;
            }

            return this;
        }

        public TextFieldElement SetPlaceholderColor(Color color)
        {
            hasPlaceholderColor = true;
            placeholderColor = color;
            if (builtPlaceholderComponent != null)
                builtPlaceholderComponent.color = color;
            return this;
        }

        public TextFieldElement SetPlaceholderItalic(bool enabled)
        {
            hasPlaceholderItalic = true;
            placeholderItalic = enabled;
            ApplyPlaceholderFontStyle(builtPlaceholderComponent);
            return this;
        }

        public TextFieldElement SetFontSize(float size)
        {
            fontSize = Mathf.Max(1f, size);
            if (builtTextComponent != null)
                builtTextComponent.fontSize = fontSize;
            if (builtPlaceholderComponent != null && (promptElement == null || !promptElement.HasExplicitFontSize))
                builtPlaceholderComponent.fontSize = fontSize;
            builtInputField?.ForceLabelUpdate();
            return this;
        }

        public TextFieldElement SetLineLimit(int? limit)
        {
            hasLineLimit = true;
            lineLimit = limit;
            ApplyLineLimit(builtInputField);
            return this;
        }

        public TextFieldElement SetAxis(Axis axis)
        {
            if (axis == Axis.Vertical)
            {
                SetLineLimit(null);
                SetTextWrappingMode(TextWrappingModes.Normal, TextOverflowModes.Overflow);
            }
            else
            {
                SetLineLimit(1);
                SetTextWrappingMode(TextWrappingModes.NoWrap, TextOverflowModes.Overflow);
            }
            return this;
        }

        public TextFieldElement SetTextWrappingMode(TextWrappingModes wrappingMode,
            TextOverflowModes overflowMode = TextOverflowModes.Overflow)
        {
            textWrappingMode = wrappingMode;
            textOverflowMode = overflowMode;
            if (builtTextComponent != null)
            {
                builtTextComponent.textWrappingMode = textWrappingMode;
                builtTextComponent.overflowMode = textOverflowMode;
            }
            if (builtPlaceholderComponent != null)
            {
                builtPlaceholderComponent.textWrappingMode = textWrappingMode;
                builtPlaceholderComponent.overflowMode = overflowMode == TextOverflowModes.Overflow
                    ? TextOverflowModes.Ellipsis
                    : overflowMode;
            }
            builtInputField?.ForceLabelUpdate();
            return this;
        }

        public TextFieldElement SetContentType(TMP_InputField.ContentType type)
        {
            contentType = type;
            if (builtInputField != null)
            {
                builtInputField.contentType = type;
                if (hasLineLimit)
                    ApplyLineLimit(builtInputField);
            }
            return this;
        }

        public TextFieldElement SetCharacterLimit(int limit)
        {
            characterLimit = Mathf.Max(0, limit);
            if (builtInputField != null)
                builtInputField.characterLimit = characterLimit;
            return this;
        }

        public TextFieldElement SetCaretColor(Color color)
        {
            caretColor = color;
            if (builtInputField != null)
            {
                builtInputField.customCaretColor = true;
                builtInputField.caretColor = color;
            }
            return this;
        }

        public TextFieldElement SetCaretBlinkRate(float rate)
        {
            caretBlinkRate = Mathf.Max(0f, rate);
            if (builtInputField != null)
                builtInputField.caretBlinkRate = caretBlinkRate;
            return this;
        }

        public TextFieldElement SetCaretWidth(int width)
        {
            caretWidth = Mathf.Max(1, width);
            if (builtInputField != null)
                builtInputField.caretWidth = caretWidth;
            return this;
        }

        public TextFieldElement SetSelectionColor(Color color)
        {
            selectionColor = color;
            if (builtInputField != null)
                builtInputField.selectionColor = color;
            return this;
        }

        public TextFieldElement SetKeyboardType(TouchScreenKeyboardType type)
        {
            keyboardType = type;
            if (builtInputField != null)
                builtInputField.keyboardType = type;
            return this;
        }

        public TextFieldElement SetFocusedBackgroundColor(Color color)
        {
            focusedBackgroundColor = color;
            hasFocusedBackgroundColor = true;
            ConfigureInputFieldTransition();
            ConfigureFocusHighlight(builtInputField != null ? builtInputField.gameObject : null, builtBackgroundImage);
            return this;
        }

        public TextFieldElement SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            hasBackgroundColor = true;
            if (builtBackgroundImage != null)
            {
                builtBackgroundImage.color = color;
                ConfigureInputFieldTransition();
            }
            return this;
        }

        public TextFieldElement SetTextFieldPadding(float horizontal, float vertical)
        {
            return SetTextFieldPadding(horizontal, horizontal, vertical, vertical);
        }

        public TextFieldElement SetTextFieldPadding(float left, float right, float top, float bottom)
        {
            textInsets = new Vector4(
                Mathf.Max(0f, left),
                Mathf.Max(0f, right),
                Mathf.Max(0f, top),
                Mathf.Max(0f, bottom));
            ApplyTextInsets();
            return this;
        }

        public TextFieldElement SetFocused(State<bool> state)
        {
            focusedState = state;
            if (state != null)
            {
                AddPropertyBinding(state, ApplyFocusedState, "focused", BindingKind.Visual);
                ApplyFocusedState();
            }
            return this;
        }

        public TextFieldElement SetOnEditingChanged(Action<bool> action)
        {
            onEditingChanged = action;
            return this;
        }

        public TextFieldElement SetOnSubmit(Action<string> action)
        {
            onSubmit = action;
            return this;
        }

        public TextFieldElement SetSelectAllOnFocus(bool enabled)
        {
            selectAllOnFocus = enabled;
            if (builtInputField != null)
                builtInputField.onFocusSelectAll = enabled;
            return this;
        }

        public TextFieldElement SetTextAlignment(TextAlignmentOptions alignment)
        {
            textAlignment = alignment;
            if (builtTextComponent != null)
                builtTextComponent.alignment = alignment;
            if (builtPlaceholderComponent != null)
                builtPlaceholderComponent.alignment = alignment;
            builtInputField?.ForceLabelUpdate();
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            var fieldContainer = CreateInputFieldRoot(parent, out var inputField, out var backgroundImage,
                out var textComponent, out var placeholderComponent);

            builtTextComponent = textComponent;
            builtPlaceholderComponent = placeholderComponent;
            builtInputField = inputField;
            builtBackgroundImage = backgroundImage;
            ConfigureInputField(inputField, textComponent, placeholderComponent);
            ConfigureInputFieldTransition();
            ConfigureLayout(fieldContainer);
            BindState(inputField, fieldContainer);
            ConfigureFocusHighlight(fieldContainer, backgroundImage);

            ApplyAllEffects(fieldContainer, backgroundImage);
            fieldContainer.SetActive(true);
            inputField.ForceLabelUpdate();
            ApplyFocusedState();
            return fieldContainer;
        }

        private GameObject CreateInputFieldRoot(Transform parent, out TMP_InputField inputField, out Image backgroundImage,
            out TMP_Text textComponent, out TMP_Text placeholderComponent)
        {
            var root = CreateElementRoot(ElementName, parent);
            root.SetActive(false);
            EnsureRectTransform(root);

            backgroundImage = AddImage(root, EffectiveBackgroundColor);

            var textArea = CreateChildObject("Text Area", root.transform);
            var textAreaRect = EnsureRectTransform(textArea);
            builtTextViewport = textAreaRect;
            StretchToParent(textAreaRect, textInsets);
            var textMask = textArea.AddComponent<RectMask2D>();
            textMask.padding = TextMaskPadding;

            placeholderComponent = promptElement != null
                ? BuildPromptElement(textArea.transform)
                : BuildStringPlaceholder(textArea.transform);

            var textGo = CreateChildObject("Text", textArea.transform);
            var textRect = EnsureRectTransform(textGo);
            StretchToParent(textRect);
            textComponent = textGo.AddComponent<TextMeshProUGUI>();
            textComponent.raycastTarget = false;
            textComponent.alignment = textAlignment;
            textComponent.textWrappingMode = textWrappingMode;
            textComponent.overflowMode = textOverflowMode;
            textComponent.extraPadding = true;

            inputField = root.AddComponent<TMP_InputField>();
            inputField.textViewport = textAreaRect;
            inputField.textComponent = textComponent;
            inputField.placeholder = placeholderComponent;
            inputField.targetGraphic = backgroundImage;

            return root;
        }

        private TMP_Text BuildStringPlaceholder(Transform parent)
        {
            var placeholderGo = CreateChildObject("Placeholder", parent);
            var placeholderRect = EnsureRectTransform(placeholderGo);
            StretchToParent(placeholderRect);
            var placeholderComponent = placeholderGo.AddComponent<TextMeshProUGUI>();
            ConfigurePlaceholderComponent(placeholderComponent);
            return placeholderComponent;
        }

        private TMP_Text BuildPromptElement(Transform parent)
        {
            ApplyInheritedFont(promptElement);
            GameObject promptObject = promptElement.Build(parent);
            promptObject.name = "Placeholder";

            StretchToParent(EnsureRectTransform(promptObject));

            TMP_Text promptText = promptObject.GetComponent<TMP_Text>();
            ConfigurePlaceholderComponent(promptText);
            return promptText;
        }

        private void ConfigurePlaceholderComponent(TMP_Text placeholderComponent)
        {
            if (placeholderComponent == null)
                return;

            placeholderComponent.raycastTarget = false;
            placeholderComponent.alignment = textAlignment;
            placeholderComponent.textWrappingMode = textWrappingMode;
            placeholderComponent.overflowMode = textOverflowMode == TextOverflowModes.Overflow
                ? TextOverflowModes.Ellipsis
                : textOverflowMode;
            placeholderComponent.extraPadding = true;
        }

        private void ConfigureInputField(TMP_InputField inputField, TMP_Text textComponent, TMP_Text placeholderComponent)
        {
            var font = ResolveFont(fontAsset);

            inputField.text = text != null ? (text.Value ?? string.Empty) : string.Empty;

            ConfigurePlaceholderText(placeholderComponent, font);

            textComponent.color = textColor;
            textComponent.fontSize = fontSize;
            if (font != null) textComponent.font = font;
            inputField.fontAsset = textComponent.font;

            inputField.contentType = contentType;
            inputField.characterLimit = characterLimit;
            inputField.customCaretColor = true;
            inputField.caretColor = caretColor;
            inputField.caretBlinkRate = caretBlinkRate;
            inputField.caretWidth = Mathf.Max(1, caretWidth);
            inputField.selectionColor = selectionColor;
            inputField.keyboardType = keyboardType;
            inputField.onFocusSelectAll = selectAllOnFocus;
            inputField.onSelect.AddListener(_ => HandleEditingChanged(true));
            inputField.onDeselect.AddListener(_ => HandleEditingChanged(false));
            inputField.onSubmit.AddListener(value => onSubmit?.Invoke(value));
            if (hasLineLimit)
                ApplyLineLimit(inputField);
        }

        private void ConfigurePlaceholderText(TMP_Text placeholderComponent, TMP_FontAsset font)
        {
            if (placeholderComponent == null)
                return;

            if (promptElement == null)
            {
                placeholderComponent.text = placeholder ?? string.Empty;
                placeholderComponent.color = placeholderColor;
                placeholderComponent.fontSize = fontSize;
                if (font != null) placeholderComponent.font = font;
                ApplyPlaceholderFontStyle(placeholderComponent);
                return;
            }

            if (hasPlaceholderColor || !promptElement.HasExplicitTextColor)
                placeholderComponent.color = placeholderColor;
            if (!promptElement.HasExplicitFontSize)
                placeholderComponent.fontSize = fontSize;
            if (font != null && !promptElement.HasExplicitFont)
                placeholderComponent.font = font;
            if (hasPlaceholderItalic)
                ApplyPlaceholderFontStyle(placeholderComponent);
        }

        private void ApplyPlaceholderFontStyle(TMP_Text placeholderComponent)
        {
            if (placeholderComponent == null)
                return;

            FontStyles style = placeholderComponent.fontStyle;
            if (placeholderItalic)
                style |= FontStyles.Italic;
            else
                style &= ~FontStyles.Italic;
            placeholderComponent.fontStyle = style;
        }

        private void HandleEditingChanged(bool editing)
        {
            if (focusedState != null && focusedState.Value != editing)
            {
                syncingFocusFromInput = true;
                try
                {
                    focusedState.Value = editing;
                }
                finally
                {
                    syncingFocusFromInput = false;
                }
            }

            onEditingChanged?.Invoke(editing);
        }

        private void ApplyFocusedState()
        {
            if (builtInputField == null || focusedState == null || syncingFocusFromInput)
                return;

            EventSystem eventSystem = EventSystem.current;
            if (eventSystem != null && eventSystem.alreadySelecting)
                return;

            if (focusedState.Value)
            {
                FocusInputField();
                return;
            }

            if (builtInputField.isFocused)
                builtInputField.DeactivateInputField();

            if (eventSystem != null && eventSystem.currentSelectedGameObject == builtInputField.gameObject)
                eventSystem.SetSelectedGameObject(null);
        }

        private void ApplyLineLimit(TMP_InputField inputField)
        {
            if (inputField == null)
                return;

            if (lineLimit.HasValue)
            {
                int limit = Mathf.Max(1, lineLimit.Value);
                inputField.lineType = limit > 1
                    ? TMP_InputField.LineType.MultiLineNewline
                    : TMP_InputField.LineType.SingleLine;
                inputField.lineLimit = limit;
            }
            else
            {
                inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
                inputField.lineLimit = 0;
            }

            inputField.ForceLabelUpdate();
        }

        private void BindState(TMP_InputField inputField, GameObject fieldContainer)
        {
            inputField.onValueChanged.AddListener(newValue =>
            {
                if (text != null && text.Value != newValue)
                    text.Value = newValue;
                onTextChanged?.Invoke(newValue);
            });

            if (text == null)
                return;

            AddPropertyBinding(text, () =>
            {
                if (inputField != null && inputField.text != text.Value)
                    inputField.text = text.Value ?? string.Empty;
            }, "textFieldText", BindingKind.Content);
        }

        private void ConfigureFocusHighlight(GameObject fieldContainer, Image backgroundImage)
        {
            if (!hasFocusedBackgroundColor || fieldContainer == null || backgroundImage == null)
                return;

            var trigger = fieldContainer.GetComponent<EventTrigger>() ?? fieldContainer.AddComponent<EventTrigger>();
            if (builtFocusTrigger == trigger)
            {
                trigger.triggers.RemoveAll(entry =>
                    entry.eventID == EventTriggerType.Select || entry.eventID == EventTriggerType.Deselect);
            }
            builtFocusTrigger = trigger;

            var selectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            selectEntry.callback.AddListener(_ => backgroundImage.color = focusedBackgroundColor);
            trigger.triggers.Add(selectEntry);

            var deselectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            deselectEntry.callback.AddListener(_ => backgroundImage.color = EffectiveBackgroundColor);
            trigger.triggers.Add(deselectEntry);
        }

        private void ConfigureInputFieldTransition()
        {
            if (builtInputField == null)
                return;

            Color normal = EffectiveBackgroundColor;
            Color highlighted = hasFocusedBackgroundColor ? focusedBackgroundColor : normal;
            ConfigureSelectableColors(builtInputField, normal, highlighted, highlighted, highlighted);
        }

        private Color EffectiveBackgroundColor => hasBackgroundColor ? backgroundColor : Color.clear;

        private void ConfigureLayout(GameObject fieldContainer)
        {
            LayoutElementUtility.Configure(
                fieldContainer,
                preferredWidth,
                preferredHeight,
                infiniteWidth,
                infiniteHeight,
                200f,
                DefaultPreferredHeight);
        }

        private void ApplyTextInsets()
        {
            if (builtTextViewport == null)
                return;

            StretchToParent(builtTextViewport, textInsets);
            builtInputField?.ForceLabelUpdate();
        }

        private static void StretchToParent(RectTransform rect, Vector4 insets)
        {
            StretchToParent(rect, insets.x, insets.y, insets.z, insets.w);
        }

        private static void StretchToParent(RectTransform rect)
        {
            StretchToParent(rect, 0f, 0f, 0f, 0f);
        }

        private static void StretchToParent(RectTransform rect, float left, float right, float top, float bottom)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
