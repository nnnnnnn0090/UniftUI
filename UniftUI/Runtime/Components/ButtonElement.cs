using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Button with a text label or custom child content.</summary>
    public class ButtonElement : UIElement, ILayoutContainer
    {
        private string label;
        private UIElement customContent;
        private Action onClick;
        private Color textColor = Color.black;
        private TMP_FontAsset fontAsset = null;
        private float fontSize = 14f;
        private bool isBold;
        private bool isItalic;
        private bool isUnderlined;
        private bool isStrikethrough;
        private bool hasCustomContent = false;
        private List<UIElement> children = new List<UIElement>();
        private TextMeshProUGUI builtTextComponent;
        private Image builtBackgroundImage;

        /// <summary>Creates a button that displays the given text label.</summary>
        public ButtonElement(string label, Action onClick)
        {
            this.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            this.label = label;
            this.onClick = onClick;
            this.hasCustomContent = false;
            UIContext.Add(this);
        }

        /// <summary>Creates a button whose appearance is defined by the given child element.</summary>
        public ButtonElement(UIElement content, Action onClick)
        {
            this.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            this.customContent = content;
            this.onClick = onClick;
            this.hasCustomContent = true;

            if (content != null)
            {
                AddChild(content);
            }

            UIContext.Add(this);
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
            {
                children.Add(child);
            }
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
            {
                children.Remove(child);
            }
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;
            int index = children.IndexOf(oldChild);
            if (index != -1)
            {
                children[index] = newChild;
            }
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public ButtonElement SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            if (builtBackgroundImage != null)
                builtBackgroundImage.color = color;
            return this;
        }

        public ButtonElement SetTextColor(Color color)
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
                if (hasCustomContent)
                {
                    foreach (var child in children)
                        child.ForegroundColor(color);
                }
            }

            return this;
        }

        public ButtonElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            if (builtTextComponent != null)
            {
                TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
                if (effectiveFont != null)
                    builtTextComponent.font = effectiveFont;
            }
            if (hasCustomContent)
            {
                foreach (var child in children)
                    child.Font(font);
            }

            return this;
        }

        public ButtonElement SetFontSize(float size)
        {
            fontSize = Mathf.Max(1f, size);
            if (builtTextComponent != null)
                ConfigureLabelText(builtTextComponent);
            if (hasCustomContent)
            {
                foreach (var child in children)
                    child.FontSize(fontSize);
            }

            return this;
        }

        public ButtonElement SetBold(bool bold)
        {
            isBold = bold;
            ApplyFontStyle(builtTextComponent);
            if (hasCustomContent)
                foreach (var child in children)
                    child.Bold();
            return this;
        }

        public ButtonElement SetItalic(bool italic)
        {
            isItalic = italic;
            ApplyFontStyle(builtTextComponent);
            if (hasCustomContent)
                foreach (var child in children)
                    child.Italic();
            return this;
        }

        public ButtonElement SetUnderline(bool underline)
        {
            isUnderlined = underline;
            ApplyFontStyle(builtTextComponent);
            if (hasCustomContent)
                foreach (var child in children)
                    child.Underline();
            return this;
        }

        public ButtonElement SetStrikethrough(bool strikethrough)
        {
            isStrikethrough = strikethrough;
            ApplyFontStyle(builtTextComponent);
            if (hasCustomContent)
                foreach (var child in children)
                    child.Strikethrough();
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            builtTextComponent = null;

            GameObject buttonObj = new GameObject("Button");
            buttonObj.transform.SetParent(parent, false);

            Image image = buttonObj.AddComponent<Image>();
            image.color = backgroundColor;
            builtBackgroundImage = image;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            button.colors = colors;

            var buttonLayout = buttonObj.AddComponent<UniftUISingleChildLayoutGroup>();
            buttonLayout.Configure(new RectOffset(10, 10, 5, 5), TextAnchor.MiddleCenter);

            if (hasCustomContent)
            {
                foreach (var child in children)
                {
                    ApplyInheritedFont(child);
                    child.Build(buttonObj.transform);
                }
            }
            else
            {
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);

                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.offsetMin = new Vector2(10, 5);
                textRect.offsetMax = new Vector2(-10, -5);

                TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
                textComponent.text = label;
                ConfigureLabelText(textComponent);
                builtTextComponent = textComponent;

                TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
                if (effectiveFont != null)
                {
                    textComponent.font = effectiveFont;
                }
            }

            LayoutElementUtility.Configure(buttonObj, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            var capturedClick = onClick;
            button.onClick.AddListener(() =>
            {
                try
                {
                    capturedClick?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniftUI] Button onClick error: {e.Message}");
                }
            });

            ApplyAllEffects(buttonObj, image);

            return buttonObj;
        }

        private void ConfigureLabelText(TextMeshProUGUI textComponent)
        {
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontSize = fontSize;
            textComponent.fontSizeMax = fontSize;
            textComponent.fontSizeMin = Mathf.Min(8f, fontSize);
            textComponent.enableAutoSizing = true;
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
            textComponent.overflowMode = TextOverflowModes.Ellipsis;
            textComponent.color = textColor;
            textComponent.raycastTarget = false;
            ApplyFontStyle(textComponent);
        }

        private void ApplyFontStyle(TextMeshProUGUI textComponent)
        {
            if (textComponent == null)
                return;

            FontStyles fontStyle = FontStyles.Normal;
            if (isBold) fontStyle |= FontStyles.Bold;
            if (isItalic) fontStyle |= FontStyles.Italic;
            if (isUnderlined) fontStyle |= FontStyles.Underline;
            if (isStrikethrough) fontStyle |= FontStyles.Strikethrough;
            textComponent.fontStyle = fontStyle;
        }

    }
}
