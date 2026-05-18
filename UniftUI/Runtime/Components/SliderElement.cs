using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Slider bound to either an integer or float state.</summary>
    public class SliderElement : UIElement
    {
        private const float DefaultSliderHeight = 40f;
        private const float DefaultSliderMinWidth = 100f;
        private const int TrackCornerRadius = 10;
        private const int CircleSpriteSize = 64;

        private static readonly Color TrackColor = new Color(0.9f, 0.9f, 0.9f, 0.5f);
        private static Sprite trackSprite;
        private static Sprite handleSprite;

        private State<int> intValue;
        private State<float> floatValue;
        private float minValue;
        private float maxValue;
        private bool showValue;
        private bool wholeNumbers;
        private Color accentColor = new Color(0.2f, 0.6f, 1.0f);
        private Color handleColor = new Color(0.1f, 0.4f, 0.8f);
        private Slider builtSlider;
        private Image builtFillImage;
        private Image builtHandleImage;
        private TextMeshProUGUI builtValueText;

        private float handleWidth = -1;
        private float handleHeight = -1;

        public SliderElement(State<int> value, float minValue, float maxValue, bool showValue = false)
        {
            this.intValue = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.showValue = showValue;
            this.wholeNumbers = true;
            AddPropertyBinding(value, SyncSliderFromState, "sliderValue", BindingKind.Content);
            UIContext.Add(this);
        }

        public SliderElement(State<float> value, float minValue, float maxValue, bool showValue = false)
        {
            this.floatValue = value;
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.showValue = showValue;
            this.wholeNumbers = false;
            AddPropertyBinding(value, SyncSliderFromState, "sliderValue", BindingKind.Content);
            UIContext.Add(this);
        }

        public SliderElement WithColors(Color accentColor, Color? handleColor = null)
        {
            this.accentColor = accentColor;
            this.handleColor = handleColor ?? accentColor.MultiplyAlpha(1.2f);
            if (builtFillImage != null)
                builtFillImage.color = this.accentColor;
            if (builtHandleImage != null)
            {
                builtHandleImage.color = this.handleColor;
                ConfigureSelectableColors(builtSlider, this.handleColor);
            }
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
            GameObject sliderObj = CreateElementRoot("SliderElement", parent);
            Image sliderObjBackgroundImage = AddBackgroundImageIfNeeded(sliderObj);
            builtValueText = null;

            LayoutElement layoutElement = LayoutElementUtility.Configure(
                sliderObj,
                preferredWidth,
                preferredHeight,
                infiniteWidth,
                infiniteHeight,
                DefaultSliderMinWidth,
                DefaultSliderHeight);
            if (preferredWidth < 0f && !infiniteWidth)
            {
                layoutElement.minWidth = DefaultSliderMinWidth;
                layoutElement.preferredWidth = -1f;
                layoutElement.flexibleWidth = 1f;
            }

            if (showValue)
            {
                UniftUIStackLayoutGroup vertLayout = sliderObj.AddComponent<UniftUIStackLayoutGroup>();
                vertLayout.padding = new RectOffset(0, 0, 0, 0);
                vertLayout.Configure(UniftUIStackAxis.Vertical, 5f, VStackAlignment.Center, HStackAlignment.Center);
            }

            GameObject sliderContainer = showValue ? CreateChildObject("SliderContainer", sliderObj.transform) : sliderObj;
            if (showValue)
            {
                LayoutElement containerLayout = sliderContainer.AddComponent<LayoutElement>();
                containerLayout.preferredHeight = DefaultSliderHeight * 0.7f;
                containerLayout.flexibleWidth = 1;
            }

            Slider slider = sliderContainer.AddComponent<Slider>();
            builtSlider = slider;
            slider.minValue = this.minValue;
            slider.maxValue = this.maxValue;
            slider.wholeNumbers = wholeNumbers;
            slider.value = CurrentValue();
            slider.transition = Selectable.Transition.ColorTint;

            GameObject background = CreateChildObject("Background", sliderContainer.transform);
            Image bgImage = AddImage(background, TrackColor);
            bgImage.sprite = TrackSprite;
            bgImage.type = Image.Type.Sliced;

            RectTransform bgRect = EnsureRectTransform(background);
            bgRect.anchorMin = new Vector2(0, 0.35f);
            bgRect.anchorMax = new Vector2(1, 0.65f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            GameObject fillArea = CreateChildObject("Fill Area", sliderContainer.transform);
            RectTransform fillAreaRect = EnsureRectTransform(fillArea);
            fillAreaRect.anchorMin = new Vector2(0, 0.35f);
            fillAreaRect.anchorMax = new Vector2(1, 0.65f);
            fillAreaRect.offsetMin = new Vector2(5, 0);
            fillAreaRect.offsetMax = new Vector2(-5, 0);

            GameObject fill = CreateChildObject("Fill", fillArea.transform);
            Image fillImage = AddImage(fill, accentColor);
            fillImage.sprite = TrackSprite;
            fillImage.type = Image.Type.Sliced;
            builtFillImage = fillImage;

            RectTransform fillRect = EnsureRectTransform(fill);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            slider.fillRect = fillRect;

            GameObject handleSlideArea = CreateChildObject("Handle Slide Area", sliderContainer.transform);
            RectTransform handleSlideAreaRect = EnsureRectTransform(handleSlideArea);
            handleSlideAreaRect.anchorMin = new Vector2(0, 0.35f);
            handleSlideAreaRect.anchorMax = new Vector2(1, 0.65f);
            handleSlideAreaRect.offsetMin = new Vector2(5, 0);
            handleSlideAreaRect.offsetMax = new Vector2(-5, 0);

            GameObject handle = CreateChildObject("Handle", handleSlideArea.transform);
            Image handleImage = AddImage(handle, handleColor);
            handleImage.sprite = HandleSprite;
            handleImage.preserveAspect = true;
            builtHandleImage = handleImage;

            Shadow handleShadow = handle.AddComponent<Shadow>();
            handleShadow.effectColor = new Color(0, 0, 0, 0.3f);
            handleShadow.effectDistance = new Vector2(1, -1);

            RectTransform handleRect = EnsureRectTransform(handle);
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);

            float referenceHeight = (preferredHeight >= 0 && !infiniteHeight)
                ? preferredHeight
                : DefaultSliderHeight;

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
            ConfigureSelectableColors(slider, handleColor);

            if (showValue)
            {
                GameObject valueText = CreateChildObject("ValueText", sliderObj.transform);
                TextMeshProUGUI tmpText = valueText.AddComponent<TextMeshProUGUI>();
                tmpText.fontSize = 14;
                tmpText.alignment = TextAlignmentOptions.MidlineRight;
                tmpText.text = CurrentValueText();
                tmpText.color = new Color(0.2f, 0.2f, 0.2f);
                builtValueText = tmpText;
                TMP_FontAsset effectiveFont = ResolveFont(null);
                if (effectiveFont != null)
                    tmpText.font = effectiveFont;

                LayoutElement textLayout = valueText.AddComponent<LayoutElement>();
                textLayout.preferredHeight = 20;
                textLayout.flexibleWidth = 1;
            }

            slider.onValueChanged.AddListener((float newValue) => {
                if (ApplySliderValue(newValue))
                    UpdateValueText();
            });

            ApplyAllEffects(sliderObj, sliderObjBackgroundImage);

            slider.value = CurrentValue();
            return sliderObj;
        }

        private void SyncSliderFromState()
        {
            float current = CurrentValue();
            if (builtSlider != null && !Mathf.Approximately(builtSlider.value, current))
                builtSlider.value = current;
            UpdateValueText();
        }

        private void UpdateValueText()
        {
            if (builtValueText != null)
                builtValueText.text = CurrentValueText();
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

        private static Sprite TrackSprite
        {
            get
            {
                if (trackSprite == null)
                    trackSprite = CreateRoundedRectSprite(TrackCornerRadius);
                return trackSprite;
            }
        }

        private static Sprite HandleSprite
        {
            get
            {
                if (handleSprite == null)
                    handleSprite = CreateCircleSprite();
                return handleSprite;
            }
        }

        private static Sprite CreateRoundedRectSprite(int cornerRadius)
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

        private static Sprite CreateCircleSprite()
        {
            int size = CircleSpriteSize;
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

}
