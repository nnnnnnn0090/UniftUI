using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

namespace UniftUI
{
    public class TextFieldElement : UIElement
    {
        private State<string> text;
        private string placeholder;
        private Action<string> onTextChanged;
        private TMP_FontAsset fontAsset = null;
        private Color textColor = Color.black;
        private Color placeholderColor = new Color(0.5f, 0.5f, 0.5f);
        private float fontSize = 18f;
        private int? lineLimit = null;
        
        private TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard;
        private int characterLimit = 0;
        private Color caretColor = new Color(0.19f, 0.19f, 0.19f);
        private float caretBlinkRate = 0.85f;
        private Color selectionColor = new Color(0.5f, 0.5f, 1f, 0.5f);
        private TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default;
        private Color focusedBackgroundColor;
        private bool hasFocusedBackgroundColor = false;

        private const string PREFAB_PATH = "Prefabs/InputField (TMP)";

        public TextFieldElement(State<string> text, string placeholder, Action<string> onTextChanged = null)
        {
            this.text = text;
            this.placeholder = placeholder;
            this.onTextChanged = onTextChanged;
            UIContext.Add(this);
        }

        public TextFieldElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            return this;
        }

        public TextFieldElement SetTextColor(Color color)
        {
            textColor = color;
            return this;
        }

        public TextFieldElement SetPlaceholderColor(Color color)
        {
            placeholderColor = color;
            return this;
        }

        public TextFieldElement SetFontSize(float size)
        {
            fontSize = size;
            return this;
        }

        public TextFieldElement SetLineLimit(int? limit)
        {
            lineLimit = limit;
            return this;
        }

        public TextFieldElement SetContentType(TMP_InputField.ContentType type)
        {
            contentType = type;
            return this;
        }

        public TextFieldElement SetCharacterLimit(int limit)
        {
            characterLimit = limit;
            return this;
        }

        public TextFieldElement SetCaretColor(Color color)
        {
            caretColor = color;
            return this;
        }

        public TextFieldElement SetCaretBlinkRate(float rate)
        {
            caretBlinkRate = rate;
            return this;
        }

        public TextFieldElement SetSelectionColor(Color color)
        {
            selectionColor = color;
            return this;
        }

        public TextFieldElement SetKeyboardType(TouchScreenKeyboardType type)
        {
            keyboardType = type;
            return this;
        }

        public TextFieldElement SetFocusedBackgroundColor(Color color)
        {
            focusedBackgroundColor = color;
            hasFocusedBackgroundColor = true;
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject prefab = Resources.Load<GameObject>(PREFAB_PATH);
            if (prefab == null)
            {
                Debug.LogError($"Failed to load InputField prefab from path: {PREFAB_PATH}");
                return null;
            }

            GameObject fieldContainer = GameObject.Instantiate(prefab, parent, false);
            fieldContainer.name = "TextField";

            TMP_InputField inputField = fieldContainer.GetComponent<TMP_InputField>();
            if (inputField == null)
            {
                Debug.LogError("TMP_InputField component not found on the prefab.", fieldContainer);
                return fieldContainer;
            }

            TMP_Text textComponent = inputField.textComponent;
            TMP_Text placeholderComponent = inputField.placeholder as TMP_Text;

            inputField.text = text.Value ?? string.Empty;

            if (placeholderComponent != null)
            {
                placeholderComponent.text = placeholder;
                placeholderComponent.color = placeholderColor;
                if (fontAsset != null) placeholderComponent.font = fontAsset;
                placeholderComponent.fontSize = fontSize;
            }

            if (textComponent != null)
            {
                textComponent.color = textColor;
                if (fontAsset != null) textComponent.font = fontAsset;
                textComponent.fontSize = fontSize;
            }
            
            if (lineLimit.HasValue)
            {
                inputField.lineLimit = lineLimit.Value;
                inputField.lineType = lineLimit.Value > 1 ? TMP_InputField.LineType.MultiLineNewline : TMP_InputField.LineType.SingleLine;
            }
            else
            {
                inputField.lineType = TMP_InputField.LineType.MultiLineNewline;
            }

            inputField.contentType = contentType;
            inputField.characterLimit = characterLimit;
            inputField.caretColor = caretColor;
            inputField.caretBlinkRate = caretBlinkRate;
            inputField.selectionColor = selectionColor;
            inputField.keyboardType = keyboardType;
            
            Image backgroundImage = fieldContainer.GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = fieldContainer.GetComponentInChildren<Image>();
            }
            Color originalBackgroundColor = Color.white;
            if (backgroundImage != null)
            {
                originalBackgroundColor = backgroundImage.color;
            }

            inputField.onValueChanged.AddListener((newValue) =>
            {
                if (text.Value != newValue)
                {
                    text.Value = newValue;
                }
                onTextChanged?.Invoke(newValue);
            });

            StateObserver observer = fieldContainer.AddComponent<StateObserver>();
            observer.Initialize(new State[] { text }, () =>
            {
                if (inputField != null && inputField.text != text.Value)
                {
                    inputField.text = text.Value ?? string.Empty;
                }
            });

            if (hasFocusedBackgroundColor && backgroundImage != null)
            {
                EventTrigger trigger = fieldContainer.GetComponent<EventTrigger>() ?? fieldContainer.AddComponent<EventTrigger>();

                EventTrigger.Entry selectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
                selectEntry.callback.AddListener((eventData) => { backgroundImage.color = focusedBackgroundColor; });
                trigger.triggers.Add(selectEntry);

                EventTrigger.Entry deselectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
                deselectEntry.callback.AddListener((eventData) => { backgroundImage.color = originalBackgroundColor; });
                trigger.triggers.Add(deselectEntry);
            }
            
            LayoutElement layoutElement = fieldContainer.GetComponent<LayoutElement>();
            if (layoutElement == null) layoutElement = fieldContainer.AddComponent<LayoutElement>();

            if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
            }
            else if (preferredWidth >= 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
            }
            else
            {
                layoutElement.minWidth = 200;
                layoutElement.flexibleWidth = 0;
            }

            if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
            }
            else if (preferredHeight >= 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
                layoutElement.flexibleHeight = 0;
            }
            else
            {
                layoutElement.minHeight = 30;
                layoutElement.flexibleHeight = 0;
            }
            
            ApplyAllEffects(fieldContainer, backgroundImage);

            return fieldContainer;
        }
    }
}
