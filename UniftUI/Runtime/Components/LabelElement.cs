using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>A compact icon-and-text label.</summary>
    public class LabelElement : UIElement, ILayoutContainer
    {
        private readonly string title;
        private readonly Sprite sprite;
        private readonly UIElement iconElement;

        private Color textColor = Color.black;
        private Color tintColor = new Color(0.2f, 0.55f, 1f, 1f);
        private TMP_FontAsset fontAsset;
        private float fontSize = 14f;
        private TextMeshProUGUI builtText;
        private Image builtIconImage;

        public LabelElement(string title, Sprite sprite)
        {
            this.title = title;
            this.sprite = sprite;
            UIContext.Add(this);
        }

        public LabelElement(string title, UIElement iconElement)
        {
            this.title = title;
            this.iconElement = iconElement;
            UIContext.Add(this);
        }

        public void AddChild(UIElement child) { }

        public void RemoveChild(UIElement child) { }

        public void ReplaceChild(UIElement oldChild, UIElement newChild) { }

        public System.Collections.Generic.IEnumerable<UIElement> GetChildren()
        {
            if (iconElement != null)
                yield return iconElement;
        }

        public LabelElement SetTextColor(Color color)
        {
            textColor = color;
            if (builtText != null)
                builtText.color = textColor;
            return this;
        }

        public LabelElement SetTintColor(Color color)
        {
            tintColor = color;
            if (builtIconImage != null)
                builtIconImage.color = tintColor;
            if (iconElement != null)
                iconElement.Tint(color);
            return this;
        }

        public LabelElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
            if (effectiveFont != null && builtText != null)
                builtText.font = effectiveFont;
            return this;
        }

        public LabelElement SetFontSize(float size)
        {
            fontSize = Mathf.Max(1f, size);
            ConfigureText(builtText);
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject root = CreateElementRoot("Label", parent);
            Image backgroundImage = AddBackgroundImageIfNeeded(root);

            var layout = root.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Horizontal, 6f, VStackAlignment.Center, HStackAlignment.Center);

            if (iconElement != null)
            {
                ApplyInheritedFont(iconElement);
                iconElement.Build(root.transform);
            }
            else if (sprite != null)
            {
                GameObject iconObj = CreateChildObject("Icon", root.transform);
                Image iconImage = AddImage(iconObj, tintColor, false);
                iconImage.sprite = sprite;
                iconImage.preserveAspect = true;
                builtIconImage = iconImage;
                LayoutElementUtility.Configure(iconObj, 16f, 16f, false, false, 16f, 16f);
            }

            GameObject textObj = CreateChildObject("Title", root.transform);
            builtText = textObj.AddComponent<TextMeshProUGUI>();
            builtText.text = title ?? string.Empty;
            ConfigureText(builtText);
            LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
            textLayout.minHeight = 18f;
            textLayout.flexibleWidth = 1f;

            LayoutElementUtility.Configure(root, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight, 60f, 24f);
            ApplyAllEffects(root, backgroundImage);

            return root;
        }

        private void ConfigureText(TextMeshProUGUI text)
        {
            if (text == null)
                return;

            text.fontSize = fontSize;
            text.fontSizeMin = Mathf.Min(8f, fontSize);
            text.fontSizeMax = fontSize;
            text.enableAutoSizing = true;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.alignment = TextAlignmentOptions.Left;
            text.verticalAlignment = VerticalAlignmentOptions.Middle;
            text.color = textColor;
            text.raycastTarget = false;

            TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
            if (effectiveFont != null)
                text.font = effectiveFont;
        }
    }
}
