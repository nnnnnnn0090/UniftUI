using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace UniftUI
{
    public class ToggleElement : UIElement
    {
        private State<bool> isOn;
        private string label;
        private Action<bool> onValueChanged;
        private TMP_FontAsset fontAsset = null;

        private const float SwitchWidth = 51f;
        private const float SwitchHeight = 31f;
        private const float KnobDiameter = SwitchHeight - 4f;
        private readonly Color ActiveTrackColor = new Color(0.2f, 0.8f, 0.35f, 1f);
        private readonly Color InactiveTrackColor = new Color(0.88f, 0.88f, 0.88f, 1f);
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

            LayoutElement containerLayout = toggleContainer.AddComponent<LayoutElement>();
            containerLayout.minHeight = SwitchHeight;
            containerLayout.preferredHeight = this.preferredHeight > 0 ? this.preferredHeight : SwitchHeight;
            
            if (this.preferredWidth > 0)
            {
                containerLayout.preferredWidth = this.preferredWidth;
                containerLayout.flexibleWidth = 0;
            }
            else
            {
                containerLayout.flexibleWidth = 1;
                if (string.IsNullOrEmpty(label))
                {
                    containerLayout.preferredWidth = SwitchWidth;
                }
            }

            HorizontalLayoutGroup hlg = toggleContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(5, 5, 0, 0);
            hlg.childAlignment = TextAnchor.MiddleLeft;

            if (!string.IsNullOrEmpty(label))
            {
                GameObject labelObj = new GameObject("Label");
                labelObj.transform.SetParent(toggleContainer.transform, false);
                
                TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
                labelText.text = label;
                labelText.fontSize = 18;
                labelText.color = Color.black;
                labelText.alignment = TextAlignmentOptions.Left;
                labelText.verticalAlignment = VerticalAlignmentOptions.Middle;
                
                if (fontAsset != null)
                {
                    labelText.font = fontAsset;
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
                isOn.Value = !isOn.Value;
                onValueChanged?.Invoke(isOn.Value);
            });
            
            Action updateVisuals = () => {
                if (trackImage != null)
                {
                    trackImage.color = isOn.Value ? ActiveTrackColor : InactiveTrackColor;
                }
                if (knobRect != null && knobRect.gameObject.activeInHierarchy)
                {
                    float movableRange = SwitchWidth - KnobDiameter - (KnobPaddingFromEdge * 2);
                    float targetKnobXPosition = isOn.Value ? 
                        (movableRange / 2) : 
                        -(movableRange / 2);
                    
                    UIAnimator.SlideHorizontal(knobRect, targetKnobXPosition, AnimationDuration);
                }
                else if (knobRect != null)
                {
                    float movableRange = SwitchWidth - KnobDiameter - (KnobPaddingFromEdge * 2);
                    float targetKnobXPosition = isOn.Value ? 
                        (movableRange / 2) : 
                        -(movableRange / 2);
                    
                    knobRect.anchoredPosition = new Vector2(targetKnobXPosition, 0);
                }
            };

            updateVisuals();

            StateObserver observer = toggleContainer.AddComponent<StateObserver>();
            observer.Initialize(new State[] { isOn }, () => {
                if (toggleContainer != null && toggleContainer.activeInHierarchy)
                {
                    updateVisuals();
                }
            });

            ApplyAllEffects(toggleContainer, toggleContainerBackgroundImage);
            
            return toggleContainer;
        }
    }
}
