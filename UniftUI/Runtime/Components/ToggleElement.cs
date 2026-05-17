using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UniftUI.Internal;

namespace UniftUI
{
    public class ToggleElement : UIElement
    {
        private State<bool> isOn;
        private string label;
        private Action<bool> onValueChanged;
        private TMP_FontAsset fontAsset = null;
        private float fontSize = 14f;
        private Color textColor = Color.black;
        private TextMeshProUGUI builtLabelText;
        private Image builtTrackImage;

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
            UIContext.Add(this);
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
            if (builtTrackImage != null && isOn != null && isOn.Value)
                builtTrackImage.color = activeTrackColor;
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject toggleContainer = new GameObject(string.IsNullOrEmpty(label) ? "ToggleSwitch" : "ToggleElement_" + label);
            toggleContainer.transform.SetParent(parent, false);

            Image toggleContainerBackgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                toggleContainerBackgroundImage = toggleContainer.AddComponent<Image>();
                toggleContainerBackgroundImage.color = backgroundColor;
            }

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
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(toggleContainer.transform, false);
                
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

            GameObject switchControl = new GameObject("SwitchControl");
            switchControl.transform.SetParent(toggleContainer.transform, false);
            
            LayoutElement switchLayout = switchControl.AddComponent<LayoutElement>();
            switchLayout.preferredWidth = SwitchWidth;
            switchLayout.minWidth = SwitchWidth;
            switchLayout.preferredHeight = SwitchHeight;
            switchLayout.minHeight = SwitchHeight;
            switchLayout.flexibleWidth = 0;

            GameObject trackObj = new GameObject("Track");
            trackObj.transform.SetParent(switchControl.transform, false);
            Image trackImage = trackObj.AddComponent<Image>();
            trackImage.type = Image.Type.Sliced;
            builtTrackImage = trackImage;

            RectTransform trackRect = trackObj.GetComponent<RectTransform>();
            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.sizeDelta = Vector2.zero;

            GameObject knobObj = new GameObject("Knob");
            knobObj.transform.SetParent(switchControl.transform, false);
            Image knobImage = knobObj.AddComponent<Image>();
            knobImage.color = KnobColor;
            knobImage.type = Image.Type.Simple;

            RectTransform knobRect = knobObj.GetComponent<RectTransform>();
            knobRect.sizeDelta = new Vector2(KnobDiameter, KnobDiameter);
            knobRect.pivot = new Vector2(0.5f, 0.5f);
            knobRect.anchorMin = new Vector2(0.5f, 0.5f);
            knobRect.anchorMax = new Vector2(0.5f, 0.5f);

            Button switchButton = switchControl.AddComponent<Button>();
            switchButton.targetGraphic = trackImage;
            var colors = switchButton.colors;
            colors.highlightedColor = colors.normalColor; 
            colors.pressedColor = colors.normalColor;     
            colors.selectedColor = colors.normalColor;
            switchButton.colors = colors;

            switchButton.onClick.AddListener(() => {
                if (isOn == null)
                    return;

                isOn.Value = !isOn.Value;
                onValueChanged?.Invoke(isOn.Value);
            });
            
            Action updateVisuals = () => {
                bool currentValue = isOn != null && isOn.Value;

                if (trackImage != null)
                {
                    trackImage.color = currentValue ? activeTrackColor : inactiveTrackColor;
                }
                if (knobRect != null && knobRect.gameObject.activeInHierarchy)
                {
                    float movableRange = SwitchWidth - KnobDiameter - (KnobPaddingFromEdge * 2);
                    float targetKnobXPosition = currentValue ? 
                        (movableRange / 2) : 
                        -(movableRange / 2);
                    
                    UIAnimator.SlideHorizontal(knobRect, targetKnobXPosition, AnimationDuration);
                }
                else if (knobRect != null)
                {
                    float movableRange = SwitchWidth - KnobDiameter - (KnobPaddingFromEdge * 2);
                    float targetKnobXPosition = currentValue ? 
                        (movableRange / 2) : 
                        -(movableRange / 2);
                    
                    knobRect.anchoredPosition = new Vector2(targetKnobXPosition, 0);
                }
            };

            updateVisuals();

            StateObserver observer = toggleContainer.AddComponent<StateObserver>();
            if (isOn != null)
            {
                observer.Initialize(new State[] { isOn }, () => {
                    if (toggleContainer != null && toggleContainer.activeInHierarchy)
                    {
                        updateVisuals();
                    }
                });
            }

            ApplyAllEffects(toggleContainer, toggleContainerBackgroundImage);
            
            return toggleContainer;
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
        }
    }
}
