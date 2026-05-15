using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UniftUI
{
    /// <summary>
    /// A declarative text view backed by TextMeshPro.
    /// </summary>
    public class TextElement : UIElement
    {
        private string text;
        private Func<string> textProvider;
        private Color textColor = Color.black;
        private bool isBold = false;
        private bool isItalic = false;
        private bool isUnderlined = false;
        private bool isStrikethrough = false;
        private float fontSize = 24;
        private TMP_FontAsset fontAsset = null;

        private bool hasShadow = false;
        private Color shadowColor = Color.black;
        private Vector2 shadowOffset = new Vector2(1, -1);
        private float shadowSoftness = 0;

        private GameObject builtTextObject;
        private TextMeshProUGUI builtTextComponent;

        /// <summary>Creates a static text element.</summary>
        public TextElement(string text)
        {
            this.text = text;
            UIContext.Add(this);
        }

        /// <summary>
        /// Creates a static text element and re-evaluates the string when any of
        /// <paramref name="dependencyStates"/> changes (useful for monitoring even when the text is constant).
        /// </summary>
        public TextElement(string text, State[] dependencyStates)
            : this((Func<string>)(() => text), dependencyStates)
        {
        }

        /// <summary>
        /// Re-runs <paramref name="textProvider"/> whenever a dependency <see cref="State"/> changes
        /// (intended for dynamic string interpolation).
        /// </summary>
        public TextElement(Func<string> textProvider, State[] dependencyStates = null)
        {
            this.textProvider = textProvider;
            this.text = textProvider != null ? (textProvider() ?? string.Empty) : string.Empty;

            if (dependencyStates != null)
            {
                for (int i = 0; i < dependencyStates.Length; i++)
                {
                    State s = dependencyStates[i];
                    if (s == null) continue;
                    AddPropertyBinding(s, RefreshFromProvider, $"textProvider_dep_{i}");
                }
            }

            UIContext.Add(this);
        }

        /// <summary>
        /// Binds the displayed string to <paramref name="textState"/> and updates without a full rebuild.
        /// This constructor is preferred over implicit conversion to <see cref="TextElement(string)"/>.
        /// </summary>
        public TextElement(State<string> textState)
            : this(textState, null)
        {
        }

        /// <summary>
        /// Binds to <paramref name="textState"/> and also re-syncs when any additional <see cref="State"/> changes.
        /// </summary>
        public TextElement(State<string> textState, State[] additionalStates)
        {
            if (textState != null)
            {
                ApplyTextString(textState.Value ?? string.Empty);
                AddPropertyBinding(textState, () => SyncFromTextState(textState), "textContent");

                if (additionalStates != null)
                {
                    for (int i = 0; i < additionalStates.Length; i++)
                    {
                        State s = additionalStates[i];
                        if (s == null || ReferenceEquals(s, textState)) continue;
                        AddPropertyBinding(s, () => SyncFromTextState(textState), $"textContent_dep_{i}");
                    }
                }
            }
            else
            {
                this.text = string.Empty;
            }

            UIContext.Add(this);
        }

        private void ApplyTextString(string s)
        {
            s ??= string.Empty;
            this.text = s;
            if (builtTextComponent != null)
                builtTextComponent.text = s;
        }

        private void SyncFromTextState(State<string> textState)
        {
            if (textState == null) return;
            ApplyTextString(textState.Value ?? string.Empty);
        }

        private void RefreshFromProvider()
        {
            if (textProvider == null) return;
            ApplyTextString(textProvider());
        }

        /// <summary>Sets the background color behind the text.</summary>
        public TextElement SetBackgroundColor(Color color)
        {
            this.backgroundColor = color;

            if (builtTextObject != null)
            {
                Image background = builtTextObject.GetComponent<Image>();
                if (background != null)
                {
                    background.color = color;
                }
            }

            return this;
        }

        /// <summary>Sets the foreground text color.</summary>
        public TextElement SetTextColor(Color color)
        {
            if (useAnimation && builtTextComponent != null && animationDuration > 0)
            {
                AnimateTextColor(builtTextComponent.gameObject, color);
            }
            else
            {
                this.textColor = color;

                if (builtTextComponent != null)
                {
                    builtTextComponent.color = color;
                }
            }

            return this;
        }

        private void AnimateTextColor(GameObject textObj, Color targetColor)
        {
            if (textObj == null || builtTextComponent == null) return;

            TextColorAnimator animator = textObj.GetComponent<TextColorAnimator>();
            if (animator == null)
            {
                animator = textObj.AddComponent<TextColorAnimator>();
            }

            animator.AnimateTo(textColor, targetColor, animationDuration, animationEasing);
            textColor = targetColor;
        }

        /// <summary>Applies or removes bold style.</summary>
        public TextElement SetBold(bool bold)
        {
            this.isBold = bold;

            if (builtTextComponent != null)
            {
                FontStyles fontStyle = builtTextComponent.fontStyle;
                if (bold)
                    fontStyle |= FontStyles.Bold;
                else
                    fontStyle &= ~FontStyles.Bold;
                builtTextComponent.fontStyle = fontStyle;
            }

            return this;
        }

        /// <summary>Applies or removes italic style.</summary>
        public TextElement SetItalic(bool italic)
        {
            this.isItalic = italic;

            if (builtTextComponent != null)
            {
                FontStyles fontStyle = builtTextComponent.fontStyle;
                if (italic)
                    fontStyle |= FontStyles.Italic;
                else
                    fontStyle &= ~FontStyles.Italic;
                builtTextComponent.fontStyle = fontStyle;
            }

            return this;
        }

        /// <summary>Applies or removes underline style.</summary>
        public TextElement SetUnderline(bool underline)
        {
            this.isUnderlined = underline;

            if (builtTextComponent != null)
            {
                FontStyles fontStyle = builtTextComponent.fontStyle;
                if (underline)
                    fontStyle |= FontStyles.Underline;
                else
                    fontStyle &= ~FontStyles.Underline;
                builtTextComponent.fontStyle = fontStyle;
            }

            return this;
        }

        /// <summary>Applies or removes strikethrough style.</summary>
        public TextElement SetStrikethrough(bool strikethrough)
        {
            this.isStrikethrough = strikethrough;

            if (builtTextComponent != null)
            {
                FontStyles fontStyle = builtTextComponent.fontStyle;
                if (strikethrough)
                    fontStyle |= FontStyles.Strikethrough;
                else
                    fontStyle &= ~FontStyles.Strikethrough;
                builtTextComponent.fontStyle = fontStyle;
            }

            return this;
        }

        /// <summary>Sets the font size in points.</summary>
        public TextElement SetFontSize(float size)
        {
            this.fontSize = size;

            if (builtTextComponent != null)
            {
                builtTextComponent.fontSize = size;
            }

            return this;
        }

        /// <summary>Sets the TextMesh Pro font asset.</summary>
        public TextElement SetFont(TMP_FontAsset font)
        {
            this.fontAsset = font;

            if (builtTextComponent != null && font != null)
            {
                builtTextComponent.font = font;
            }

            return this;
        }

        /// <summary>Applies a TextMesh Pro underlay shadow.</summary>
        public TextElement SetTMProShadow(Color color, Vector2 offset, float softness = 0)
        {
            this.hasShadow = true;

            float minAlpha = 0.5f;
            if (color.a < minAlpha) {
                color = new Color(color.r, color.g, color.b, minAlpha);
            }
            this.shadowColor = color;

            float offsetMultiplier = 1.5f;
            this.shadowOffset = offset * offsetMultiplier;

            this.shadowSoftness = softness * 0.7f;

            if (builtTextComponent != null)
            {
                ApplyShadowToTextComponent(builtTextComponent);
            }

            return this;
        }

        private void ApplyShadowToTextComponent(TextMeshProUGUI textComponent)
        {
            if (!hasShadow || textComponent == null) return;

            textComponent.enableVertexGradient = false;

            Material mat = null;
            if (textComponent.fontSharedMaterial != null)
            {
                mat = new Material(textComponent.fontSharedMaterial);
            }
            else if (textComponent.font != null && textComponent.font.material != null)
            {
                mat = new Material(textComponent.font.material);
            }

            if (mat != null)
            {
                mat.EnableKeyword("UNDERLAY_ON");

                mat.SetColor("_UnderlayColor", shadowColor);
                mat.SetFloat("_UnderlayOffsetX", shadowOffset.x);
                mat.SetFloat("_UnderlayOffsetY", shadowOffset.y);
                mat.SetFloat("_UnderlayDilate", 1);
                mat.SetFloat("_UnderlaySoftness", shadowSoftness);

                textComponent.fontMaterial = mat;
            }
        }

        /// <inheritdoc />
        public override GameObject Build(Transform parent)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);

            Image background = null;
            if (backgroundColor != Color.clear)
            {
                background = textObj.AddComponent<Image>();
                background.color = backgroundColor;
            }

            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = textColor;
            textComponent.alignment = TextAlignmentOptions.Center;

            TMP_FontAsset effectiveFont = fontAsset ?? UIContext.DefaultFont;
            if (effectiveFont != null)
            {
                textComponent.font = effectiveFont;
            }

            FontStyles fontStyle = FontStyles.Normal;
            if (isBold) fontStyle |= FontStyles.Bold;
            if (isItalic) fontStyle |= FontStyles.Italic;
            if (isUnderlined) fontStyle |= FontStyles.Underline;
            if (isStrikethrough) fontStyle |= FontStyles.Strikethrough;

            textComponent.fontStyle = fontStyle;

            if (hasShadow)
            {
                ApplyShadowToTextComponent(textComponent);
            }

            LayoutElement layoutElement = textObj.AddComponent<LayoutElement>();
            ContentSizeFitter sizeFitter = textObj.AddComponent<ContentSizeFitter>();

            if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
                layoutElement.preferredWidth = -1;
                layoutElement.flexibleHeight = 0;

                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else if (preferredWidth > 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
                layoutElement.flexibleHeight = 0;
                layoutElement.layoutPriority = 100;

                RectTransform rectTransform = textObj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(preferredWidth, rectTransform.sizeDelta.y);

                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                layoutElement.flexibleHeight = 0;

                sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            builtTextObject = textObj;
            builtTextComponent = textComponent;

            ApplyAllEffects(textObj, background);

            return textObj;
        }
    }
}
