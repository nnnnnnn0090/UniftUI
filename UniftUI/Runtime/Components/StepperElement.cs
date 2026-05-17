using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>An integer stepper with minus/plus controls.</summary>
    public class StepperElement : UIElement
    {
        private readonly State<int> value;
        private readonly int minValue;
        private readonly int maxValue;
        private readonly int step;
        private readonly string label;

        private Color tintColor = new Color(0.2f, 0.55f, 1f, 1f);
        private Color textColor = Color.black;
        private TMP_FontAsset fontAsset;
        private float fontSize = 14f;

        private Button minusButton;
        private Button plusButton;
        private Image minusImage;
        private Image plusImage;
        private TextMeshProUGUI valueText;
        private TextMeshProUGUI labelText;

        public StepperElement(State<int> value, int minValue, int maxValue, int step = 1, string label = null)
        {
            this.value = value;
            this.minValue = Mathf.Min(minValue, maxValue);
            this.maxValue = Mathf.Max(minValue, maxValue);
            this.step = Mathf.Max(1, step);
            this.label = label;

            if (value != null)
            {
                value.Value = Mathf.Clamp(value.Value, this.minValue, this.maxValue);
                AddPropertyBinding(value, ApplyValue, "stepperValue", BindingKind.Visual);
            }

            UIContext.Add(this);
        }

        public StepperElement SetTintColor(Color color)
        {
            tintColor = color;
            ApplyValue();
            return this;
        }

        public StepperElement SetTextColor(Color color)
        {
            textColor = color;
            if (valueText != null)
                valueText.color = color;
            if (labelText != null)
                labelText.color = color;
            return this;
        }

        public StepperElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
            if (effectiveFont != null)
            {
                if (valueText != null)
                    valueText.font = effectiveFont;
                if (labelText != null)
                    labelText.font = effectiveFont;
            }
            return this;
        }

        public StepperElement SetFontSize(float size)
        {
            fontSize = Mathf.Max(1f, size);
            ConfigureText(valueText);
            ConfigureText(labelText);
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject root = new GameObject("Stepper");
            root.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = root.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            var layout = root.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Horizontal, 8f, VStackAlignment.Center, HStackAlignment.Center);

            if (!string.IsNullOrEmpty(label))
            {
                labelText = CreateText(root.transform, "Label", label);
                LayoutElement labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
                labelLayout.flexibleWidth = 1f;
                labelLayout.minHeight = 32f;
            }

            minusButton = CreateButton(root.transform, "Minus", "-", () => StepBy(-step), out minusImage);

            valueText = CreateText(root.transform, "Value", CurrentText());
            LayoutElement valueLayout = valueText.gameObject.AddComponent<LayoutElement>();
            valueLayout.preferredWidth = 44f;
            valueLayout.minHeight = 32f;

            plusButton = CreateButton(root.transform, "Plus", "+", () => StepBy(step), out plusImage);

            LayoutElementUtility.Configure(root, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight, string.IsNullOrEmpty(label) ? 128f : 220f, 32f);

            ApplyAllEffects(root, backgroundImage);
            ApplyValue();

            return root;
        }

        private Button CreateButton(Transform parent, string name, string text, UnityEngine.Events.UnityAction action, out Image image)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            image = buttonObj.AddComponent<Image>();
            image.color = tintColor;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(action);

            var layout = buttonObj.AddComponent<UniftUISingleChildLayoutGroup>();
            layout.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            TextMeshProUGUI textComponent = CreateText(buttonObj.transform, "Text", text);
            textComponent.color = Color.white;
            textComponent.raycastTarget = false;

            LayoutElementUtility.Configure(buttonObj, 32f, 32f, false, false, 32f, 32f);
            return button;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            ConfigureText(tmp);
            return tmp;
        }

        private void ConfigureText(TextMeshProUGUI tmp)
        {
            if (tmp == null)
                return;

            tmp.fontSize = fontSize;
            tmp.fontSizeMin = Mathf.Min(8f, fontSize);
            tmp.fontSizeMax = fontSize;
            tmp.enableAutoSizing = true;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.color = textColor;
            tmp.raycastTarget = false;

            TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
            if (effectiveFont != null)
                tmp.font = effectiveFont;
        }

        private void StepBy(int delta)
        {
            if (value == null)
                return;

            value.Value = Mathf.Clamp(value.Value + delta, minValue, maxValue);
            ApplyValue();
        }

        private void ApplyValue()
        {
            int current = value != null ? Mathf.Clamp(value.Value, minValue, maxValue) : minValue;
            if (valueText != null)
                valueText.text = current.ToString();

            bool canDecrease = value == null || current > minValue;
            bool canIncrease = value == null || current < maxValue;
            ApplyButtonState(minusButton, minusImage, canDecrease);
            ApplyButtonState(plusButton, plusImage, canIncrease);
        }

        private void ApplyButtonState(Button button, Image image, bool enabled)
        {
            if (button != null)
                button.interactable = enabled;
            if (image != null)
                image.color = enabled ? tintColor : new Color(tintColor.r, tintColor.g, tintColor.b, tintColor.a * 0.35f);
        }

        private string CurrentText()
        {
            return value != null ? Mathf.Clamp(value.Value, minValue, maxValue).ToString() : minValue.ToString();
        }
    }
}
