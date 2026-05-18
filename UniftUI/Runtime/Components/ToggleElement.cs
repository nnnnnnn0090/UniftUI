using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UniftUI.Internal;

namespace UniftUI
{
    public class ToggleElement : UIElement, IControlHitTargetSource
    {
        private State<bool> isOn;
        private string label;
        private Action<bool> onValueChanged;
        private TMP_FontAsset fontAsset = null;
        private float fontSize = 14f;
        private Color textColor = Color.black;
        private TextMeshProUGUI builtLabelText;
        private Image builtTrackImage;
        private Button builtSwitchButton;
        private RectTransform builtKnobRect;
        private GameObject hitAreaObject;

        private const float SwitchWidth = 51f;
        private const float SwitchHeight = 31f;
        private const float KnobDiameter = SwitchHeight - 4f;
        private Color activeTrackColor = new Color(0.2f, 0.8f, 0.35f, 1f);
        private Color inactiveTrackColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        private readonly Color KnobColor = Color.white;
        private const float KnobPaddingFromEdge = 2f;
        private const float AnimationDuration = 0.1f;

        public ToggleElement(State<bool> isOn, string label, Action<bool> onValueChanged = null)
        {
            this.isOn = isOn;
            this.label = label;
            this.onValueChanged = onValueChanged;
            AddPropertyBinding(isOn, ApplyValue, "toggleValue", BindingKind.Visual);
            UIContext.Add(this);
        }

        bool IControlHitTargetSource.TryGetControlHitTarget(out ControlHitTarget target)
        {
            if (isOn == null)
            {
                target = default(ControlHitTarget);
                return false;
            }

            target = CreateHitTarget();
            return true;
        }

        private ControlHitTarget CreateHitTarget()
        {
            return new ControlHitTarget(ToggleValue, canReceiveInput: IsInputAllowed);
        }

        private void ToggleValue()
        {
            if (!IsInputAllowed() || isOn == null)
                return;

            isOn.Value = !isOn.Value;
            onValueChanged?.Invoke(isOn.Value);
        }

        public ToggleElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            if (builtLabelText != null)
            {
                TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
                if (effectiveFont != null)
                    builtLabelText.font = effectiveFont;
            }

            return this;
        }

        public ToggleElement SetFontSize(float size)
        {
            fontSize = Mathf.Max(1f, size);
            if (builtLabelText != null)
                ConfigureLabelText(builtLabelText);

            return this;
        }

        public ToggleElement SetTextColor(Color color)
        {
            textColor = color;
            if (builtLabelText != null)
                builtLabelText.color = textColor;
            return this;
        }

        public ToggleElement SetTintColor(Color color)
        {
            activeTrackColor = color;
            ApplyValue();
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject toggleContainer = CreateElementRoot(
                string.IsNullOrEmpty(label) ? "ToggleSwitch" : "ToggleElement_" + label,
                parent);
            Image toggleContainerBackgroundImage = AddBackgroundImageIfNeeded(toggleContainer);

            LayoutElement containerLayout = LayoutElementUtility.Configure(
                toggleContainer,
                preferredWidth,
                preferredHeight,
                preferredWidth < 0 || infiniteWidth,
                infiniteHeight,
                string.IsNullOrEmpty(label) ? SwitchWidth : -1f,
                SwitchHeight);
            containerLayout.minHeight = SwitchHeight;

            UniftUIStackLayoutGroup hlg = toggleContainer.AddComponent<UniftUIStackLayoutGroup>();
            hlg.padding = new RectOffset(5, 5, 0, 0);
            hlg.Configure(UniftUIStackAxis.Horizontal, 8f, VStackAlignment.Leading, HStackAlignment.Center);

            if (!string.IsNullOrEmpty(label))
            {
                GameObject labelObj = CreateChildObject("Label", toggleContainer.transform);

                TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
                labelText.text = label;
                ConfigureLabelText(labelText);
                builtLabelText = labelText;

                TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
                if (effectiveFont != null)
                {
                    labelText.font = effectiveFont;
                }

                LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
                labelLayout.flexibleWidth = 1;
                labelLayout.minHeight = SwitchHeight;
            }

            GameObject switchControl = CreateChildObject("SwitchControl", toggleContainer.transform);

            LayoutElement switchLayout = switchControl.AddComponent<LayoutElement>();
            switchLayout.preferredWidth = SwitchWidth;
            switchLayout.minWidth = SwitchWidth;
            switchLayout.preferredHeight = SwitchHeight;
            switchLayout.minHeight = SwitchHeight;
            switchLayout.flexibleWidth = 0;

            GameObject trackObj = CreateChildObject("Track", switchControl.transform);
            Image trackImage = AddImage(trackObj, Color.white);
            trackImage.type = Image.Type.Sliced;
            builtTrackImage = trackImage;

            RectTransform trackRect = EnsureRectTransform(trackObj);
            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.sizeDelta = Vector2.zero;

            GameObject knobObj = CreateChildObject("Knob", switchControl.transform);
            Image knobImage = AddImage(knobObj, KnobColor);
            knobImage.type = Image.Type.Simple;

            RectTransform knobRect = EnsureRectTransform(knobObj);
            builtKnobRect = knobRect;
            knobRect.sizeDelta = new Vector2(KnobDiameter, KnobDiameter);
            knobRect.pivot = new Vector2(0.5f, 0.5f);
            knobRect.anchorMin = new Vector2(0.5f, 0.5f);
            knobRect.anchorMax = new Vector2(0.5f, 0.5f);

            Button switchButton = switchControl.AddComponent<Button>();
            switchButton.targetGraphic = trackImage;
            builtSwitchButton = switchButton;

            switchButton.onClick.AddListener(ToggleValue);

            ApplyValue();

            EnsureControlHitArea(toggleContainer, ref hitAreaObject, "ToggleHitArea", CreateHitTarget());
            ApplyAllEffects(toggleContainer, toggleContainerBackgroundImage);

            return toggleContainer;
        }

        private void ApplyValue()
        {
            bool currentValue = isOn != null && isOn.Value;

            if (builtTrackImage != null)
            {
                Color trackColor = currentValue ? activeTrackColor : inactiveTrackColor;
                builtTrackImage.color = trackColor;
                ConfigureSelectableColors(builtSwitchButton, trackColor, trackColor, trackColor, trackColor);
            }

            if (builtKnobRect == null)
                return;

            float movableRange = SwitchWidth - KnobDiameter - (KnobPaddingFromEdge * 2);
            float targetKnobXPosition = currentValue
                ? movableRange / 2f
                : -movableRange / 2f;

            if (builtKnobRect.gameObject.activeInHierarchy)
                UIAnimator.SlideHorizontal(builtKnobRect, targetKnobXPosition, AnimationDuration);
            else
                builtKnobRect.anchoredPosition = new Vector2(targetKnobXPosition, 0f);
        }

        private void ConfigureLabelText(TextMeshProUGUI labelText)
        {
            labelText.fontSize = fontSize;
            labelText.fontSizeMax = fontSize;
            labelText.fontSizeMin = Mathf.Min(8f, fontSize);
            labelText.enableAutoSizing = true;
            labelText.textWrappingMode = TextWrappingModes.NoWrap;
            labelText.overflowMode = TextOverflowModes.Ellipsis;
            labelText.color = textColor;
            labelText.alignment = TextAlignmentOptions.Left;
            labelText.verticalAlignment = VerticalAlignmentOptions.Middle;
            labelText.raycastTarget = false;
        }
    }
}
