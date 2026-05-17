using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Slider bound to either an integer or float state.</summary>
    public class SliderElement : UIElement
    {
        private State<int> intValue;
        private State<float> floatValue;
        private float minValue;
        private float maxValue;
        private bool showValue;
        private bool wholeNumbers;
        private Color accentColor = new Color(0.2f, 0.6f, 1.0f);
        private Color handleColor = new Color(0.1f, 0.4f, 0.8f);
        private Image builtFillImage;
        private Image builtHandleImage;

        private float handleWidth = -1;
        private float handleHeight = -1;

        public SliderElement(State<int> value, float minValue, float maxValue, bool showValue = false)
        {
            this.intValue = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.showValue = showValue;
            this.wholeNumbers = true;
            UIContext.Add(this);
        }

        public SliderElement(State<float> value, float minValue, float maxValue, bool showValue = false)
        {
            this.floatValue = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.showValue = showValue;
            this.wholeNumbers = false;
            UIContext.Add(this);
        }

        public SliderElement WithColors(Color accentColor, Color? handleColor = null)
        {
            this.accentColor = accentColor;
            this.handleColor = handleColor ?? accentColor.MultiplyAlpha(1.2f);
            if (builtFillImage != null)
                builtFillImage.color = this.accentColor;
            if (builtHandleImage != null)
                builtHandleImage.color = this.handleColor;
            return this;
        }

        public SliderElement WithHandleSize(float width, float height)
        {
            this.handleWidth = width;
            this.handleHeight = height;
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject sliderObj = new GameObject("SliderElement");
            sliderObj.transform.SetParent(parent, false);

            Image sliderObjBackgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                sliderObjBackgroundImage = sliderObj.AddComponent<Image>();
                sliderObjBackgroundImage.color = backgroundColor;
            }

            float defaultSliderHeight = 40f;
            float defaultSliderMinWidth = 100f;
            LayoutElement layoutElement = LayoutElementUtility.Configure(
                sliderObj,
                preferredWidth,
                preferredHeight,
                infiniteWidth,
                infiniteHeight,
                defaultSliderMinWidth,
                defaultSliderHeight);
            if (preferredWidth < 0f && !infiniteWidth)
            {
                layoutElement.minWidth = defaultSliderMinWidth;
                layoutElement.preferredWidth = -1f;
                layoutElement.flexibleWidth = 1f;
            }

            if (showValue)
            {
                UniftUIStackLayoutGroup vertLayout = sliderObj.AddComponent<UniftUIStackLayoutGroup>();
                vertLayout.padding = new RectOffset(0, 0, 0, 0);
                vertLayout.Configure(UniftUIStackAxis.Vertical, 5f, VStackAlignment.Center, HStackAlignment.Center);
            }

            GameObject sliderContainer = showValue ? new GameObject("SliderContainer") : sliderObj;
            if (showValue)
            {
                sliderContainer.transform.SetParent(sliderObj.transform, false);
                LayoutElement containerLayout = sliderContainer.AddComponent<LayoutElement>();
                containerLayout.preferredHeight = defaultSliderHeight * 0.7f;
                containerLayout.flexibleWidth = 1;
            }

            Slider slider = sliderContainer.AddComponent<Slider>();
            slider.minValue = this.minValue;
            slider.maxValue = this.maxValue;
            slider.wholeNumbers = wholeNumbers;
            slider.value = CurrentValue();
            slider.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = slider.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f);
            colors.pressedColor = new Color(0.9f, 0.9f, 0.9f);
            colors.selectedColor = Color.white;
            slider.colors = colors;

            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderContainer.transform, false);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.9f, 0.9f, 0.9f, 0.5f);
            bgImage.sprite = CreateRoundedRectSprite(10);
            bgImage.type = Image.Type.Sliced;

            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.35f);
            bgRect.anchorMax = new Vector2(1, 0.65f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderContainer.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.35f);
            fillAreaRect.anchorMax = new Vector2(1, 0.65f);
            fillAreaRect.offsetMin = new Vector2(5, 0);
            fillAreaRect.offsetMax = new Vector2(-5, 0);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = accentColor;
            fillImage.sprite = CreateRoundedRectSprite(10);
            fillImage.type = Image.Type.Sliced;
            builtFillImage = fillImage;

            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            slider.fillRect = fillRect;

            GameObject handleSlideArea = new GameObject("Handle Slide Area");
            handleSlideArea.transform.SetParent(sliderContainer.transform, false);
            RectTransform handleSlideAreaRect = handleSlideArea.AddComponent<RectTransform>();
            handleSlideAreaRect.anchorMin = new Vector2(0, 0.35f);
            handleSlideAreaRect.anchorMax = new Vector2(1, 0.65f);
            handleSlideAreaRect.offsetMin = new Vector2(5, 0);
            handleSlideAreaRect.offsetMax = new Vector2(-5, 0);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleSlideArea.transform, false);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = handleColor;
            handleImage.sprite = CreateCircleSprite();
            handleImage.preserveAspect = true;
            builtHandleImage = handleImage;

            Shadow handleShadow = handle.AddComponent<Shadow>();
            handleShadow.effectColor = new Color(0, 0, 0, 0.3f);
            handleShadow.effectDistance = new Vector2(1, -1);

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);

            float referenceHeight = (preferredHeight >= 0 && !infiniteHeight)
                ? preferredHeight
                : defaultSliderHeight;

            if (showValue) referenceHeight *= 0.7f;

            float calculatedHandleSize = Mathf.Min(24, referenceHeight * 0.6f);
            float handleW = (handleWidth >= 0) ? handleWidth : calculatedHandleSize;
            float handleH = (handleHeight >= 0) ? handleHeight : calculatedHandleSize;

            handleRect.sizeDelta = new Vector2(handleW, handleH);

            LayoutElement handleLayout = handle.AddComponent<LayoutElement>();
            handleLayout.preferredWidth = handleW;
            handleLayout.preferredHeight = handleH;
            handleLayout.minWidth = handleW;
            handleLayout.minHeight = handleH;
            handleLayout.flexibleWidth = 0;
            handleLayout.flexibleHeight = 0;

            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;

            if (showValue)
            {
                GameObject valueText = new GameObject("ValueText");
                valueText.transform.SetParent(sliderObj.transform, false);
                TextMeshProUGUI tmpText = valueText.AddComponent<TextMeshProUGUI>();
                tmpText.fontSize = 14;
                tmpText.alignment = TextAlignmentOptions.MidlineRight;
                tmpText.text = CurrentValueText();
                tmpText.color = new Color(0.2f, 0.2f, 0.2f);
                TMP_FontAsset effectiveFont = ResolveFont(null);
                if (effectiveFont != null)
                    tmpText.font = effectiveFont;

                LayoutElement textLayout = valueText.AddComponent<LayoutElement>();
                textLayout.preferredHeight = 20;
                textLayout.flexibleWidth = 1;
            }

            slider.onValueChanged.AddListener((float newValue) => {
                if (ApplySliderValue(newValue))
                {
                    if (showValue && slider.gameObject.activeInHierarchy)
                    {
                        Transform valueTextTrans = sliderObj.transform.Find("ValueText");
                        if (valueTextTrans != null)
                        {
                            TextMeshProUGUI valueTextComponent = valueTextTrans.GetComponent<TextMeshProUGUI>();
                            if (valueTextComponent != null)
                            {
                                valueTextComponent.text = CurrentValueText();
                            }
                        }
                    }
                }
            });

            StateObserver observer = sliderObj.AddComponent<StateObserver>();
            observer.Initialize(new[] { WatchedState() }, () => {
                float current = CurrentValue();
                if (slider != null && !Mathf.Approximately(slider.value, current))
                {
                    slider.value = current;
                    if (showValue)
                    {
                        Transform valueTextTrans = sliderObj.transform.Find("ValueText");
                        if (valueTextTrans != null)
                        {
                            TextMeshProUGUI valueTextComponent = valueTextTrans.GetComponent<TextMeshProUGUI>();
                            if (valueTextComponent != null)
                            {
                                valueTextComponent.text = CurrentValueText();
                            }
                        }
                    }
                }
            });

            ApplyAllEffects(sliderObj, sliderObjBackgroundImage);

            slider.value = CurrentValue();
            return sliderObj;
        }

        private State WatchedState()
        {
            return intValue != null ? (State)intValue : floatValue;
        }

        private float CurrentValue()
        {
            if (intValue != null)
                return intValue.Value;
            return floatValue != null ? floatValue.Value : minValue;
        }

        private bool ApplySliderValue(float newValue)
        {
            if (intValue != null)
            {
                int next = Mathf.RoundToInt(newValue);
                if (intValue.Value == next)
                    return false;
                intValue.Value = next;
                return true;
            }

            if (floatValue != null)
            {
                if (Mathf.Approximately(floatValue.Value, newValue))
                    return false;
                floatValue.Value = newValue;
                return true;
            }

            return false;
        }

        private string CurrentValueText()
        {
            return intValue != null
                ? intValue.Value.ToString()
                : CurrentValue().ToString("0.##");
        }

        private Sprite CreateRoundedRectSprite(int cornerRadius)
        {
            int width = 100;
            int height = 20;

            Texture2D texture = new Texture2D(width, height);
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x < cornerRadius && y < cornerRadius)
                    {
                        float distance = Mathf.Sqrt((cornerRadius - x) * (cornerRadius - x) + (cornerRadius - y) * (cornerRadius - y));
                        colors[y * width + x] = distance <= cornerRadius ? Color.white : Color.clear;
                    }
                    else if (x >= width - cornerRadius && y < cornerRadius)
                    {
                        float distance = Mathf.Sqrt((x - (width - cornerRadius - 1)) * (x - (width - cornerRadius - 1)) + (cornerRadius - y) * (cornerRadius - y));
                        colors[y * width + x] = distance <= cornerRadius ? Color.white : Color.clear;
                    }
                    else if (x < cornerRadius && y >= height - cornerRadius)
                    {
                        float distance = Mathf.Sqrt((cornerRadius - x) * (cornerRadius - x) + (y - (height - cornerRadius - 1)) * (y - (height - cornerRadius - 1)));
                        colors[y * width + x] = distance <= cornerRadius ? Color.white : Color.clear;
                    }
                    else if (x >= width - cornerRadius && y >= height - cornerRadius)
                    {
                        float distance = Mathf.Sqrt((x - (width - cornerRadius - 1)) * (x - (width - cornerRadius - 1)) + (y - (height - cornerRadius - 1)) * (y - (height - cornerRadius - 1)));
                        colors[y * width + x] = distance <= cornerRadius ? Color.white : Color.clear;
                    }
                    else
                    {
                        colors[y * width + x] = Color.white;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            TextureTracker.RegisterTexture(texture);

            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
        }

        private Sprite CreateCircleSprite()
        {
            int size = 64;
            int radius = size / 2;

            Texture2D texture = new Texture2D(size, size);
            Color[] colors = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius + 0.5f;
                    float dy = y - radius + 0.5f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);

                    if (distance <= radius)
                    {
                        colors[y * size + x] = Color.white;
                    }
                    else
                    {
                        colors[y * size + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            TextureTracker.RegisterTexture(texture);

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
        }
    }

    public static class ColorExtensions
    {
        public static Color MultiplyAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, color.a * alpha);
        }
    }
}
