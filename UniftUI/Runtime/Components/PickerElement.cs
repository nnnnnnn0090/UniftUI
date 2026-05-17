using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>A segmented picker bound to an integer selected index.</summary>
    public class PickerElement : UIElement
    {
        private readonly State<int> selection;
        private readonly List<string> options = new List<string>();
        private readonly List<Image> segmentImages = new List<Image>();
        private readonly List<TextMeshProUGUI> segmentTexts = new List<TextMeshProUGUI>();

        private Color tintColor = new Color(0.2f, 0.55f, 1f, 1f);
        private Color trackColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        private Color textColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        private TMP_FontAsset fontAsset;
        private float fontSize = 13f;
        private PickerStyle style = PickerStyle.Segmented;

        public PickerElement(State<int> selection, IEnumerable<string> options)
        {
            this.selection = selection;
            if (options != null)
                this.options.AddRange(options);
            if (this.options.Count == 0)
                this.options.Add("Option");

            if (selection != null)
            {
                selection.Value = Mathf.Clamp(selection.Value, 0, this.options.Count - 1);
                AddPropertyBinding(selection, ApplySelection, "pickerSelection", BindingKind.Visual);
            }

            WithCornerRadius(7f);
            UIContext.Add(this);
        }

        public PickerElement SetTintColor(Color color)
        {
            tintColor = color;
            ApplySelection();
            return this;
        }

        public PickerElement SetTrackColor(Color color)
        {
            trackColor = color;
            foreach (Image image in segmentImages)
            {
                if (image != null)
                    image.color = trackColor;
            }
            ApplySelection();
            return this;
        }

        public PickerElement SetTextColor(Color color)
        {
            textColor = color;
            ApplySelection();
            return this;
        }

        public PickerElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
            if (effectiveFont != null)
            {
                foreach (TextMeshProUGUI text in segmentTexts)
                    if (text != null)
                        text.font = effectiveFont;
            }
            return this;
        }

        public PickerElement SetFontSize(float size)
        {
            fontSize = Mathf.Max(1f, size);
            foreach (TextMeshProUGUI text in segmentTexts)
                ConfigureText(text);
            return this;
        }

        public PickerElement SetPickerStyle(PickerStyle pickerStyle)
        {
            style = pickerStyle == PickerStyle.Automatic ? PickerStyle.Segmented : pickerStyle;
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            segmentImages.Clear();
            segmentTexts.Clear();

            GameObject root = new GameObject(style == PickerStyle.Segmented ? "PickerSegmented" : "Picker");
            root.transform.SetParent(parent, false);

            Image backgroundImage = root.AddComponent<Image>();
            backgroundImage.color = trackColor;

            var layout = root.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(2, 2, 2, 2);
            layout.Configure(UniftUIStackAxis.Horizontal, 2f, VStackAlignment.Center, HStackAlignment.Center);

            for (int i = 0; i < options.Count; i++)
            {
                int capturedIndex = i;
                GameObject segment = new GameObject("Segment_" + options[i]);
                segment.transform.SetParent(root.transform, false);

                Image image = segment.AddComponent<Image>();
                image.color = trackColor;
                segmentImages.Add(image);

                Button button = segment.AddComponent<Button>();
                button.targetGraphic = image;
                button.onClick.AddListener(() =>
                {
                    if (selection != null)
                        selection.Value = capturedIndex;
                    ApplySelection();
                });

                var single = segment.AddComponent<UniftUISingleChildLayoutGroup>();
                single.Configure(new RectOffset(10, 10, 4, 4), TextAnchor.MiddleCenter);

                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(segment.transform, false);
                TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
                text.text = options[i];
                ConfigureText(text);
                segmentTexts.Add(text);

                LayoutElementUtility.Configure(segment, -1f, 28f, true, false, 54f, 28f);
            }

            LayoutElementUtility.Configure(root, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight, 160f, 32f);

            ApplyAllEffects(root, backgroundImage);
            ApplySelection();

            return root;
        }

        private void ConfigureText(TextMeshProUGUI text)
        {
            if (text == null)
                return;

            text.alignment = TextAlignmentOptions.Center;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.fontSize = fontSize;
            text.fontSizeMin = Mathf.Min(8f, fontSize);
            text.fontSizeMax = fontSize;
            text.enableAutoSizing = true;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.color = textColor;
            text.raycastTarget = false;

            TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
            if (effectiveFont != null)
                text.font = effectiveFont;
        }

        private void ApplySelection()
        {
            int selectedIndex = selection != null ? Mathf.Clamp(selection.Value, 0, options.Count - 1) : 0;
            for (int i = 0; i < segmentImages.Count; i++)
            {
                bool selected = i == selectedIndex;
                if (segmentImages[i] != null)
                    segmentImages[i].color = selected ? tintColor : trackColor;
                if (segmentTexts[i] != null)
                    segmentTexts[i].color = selected ? Color.white : textColor;
            }
        }
    }
}
