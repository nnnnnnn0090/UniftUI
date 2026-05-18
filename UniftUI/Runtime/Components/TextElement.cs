using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniftUI.Internal;

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
        private bool hasExplicitTextColor;
        private bool isBold = false;
        private bool isItalic = false;
        private bool isUnderlined = false;
        private bool isStrikethrough = false;
        private float fontSize = 24;
        private bool hasExplicitFontSize;
        private TMP_FontAsset fontAsset = null;
        private bool hasExplicitFont;
        private int? lineLimit;
        private TextAlignmentOptions textAlignment = TextAlignmentOptions.Center;

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
                    AddPropertyBinding(s, RefreshFromProvider, $"textProvider_dep_{i}", BindingKind.Content);
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
                AddPropertyBinding(textState, () => SyncFromTextState(textState), "textContent", BindingKind.Content);

                if (additionalStates != null)
                {
                    for (int i = 0; i < additionalStates.Length; i++)
                    {
                        State s = additionalStates[i];
                        if (s == null || ReferenceEquals(s, textState)) continue;
                        AddPropertyBinding(s, () => SyncFromTextState(textState), $"textContent_dep_{i}", BindingKind.Content);
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
            {
                builtTextComponent.text = s;
                UpdatePreferredLayout();
            }
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

        public override UIElement WithInfiniteWidth()
        {
            base.WithInfiniteWidth();
            RefreshLayoutAfterFrameChange();
            return this;
        }

        public override UIElement WithInfiniteHeight()
        {
            base.WithInfiniteHeight();
            RefreshLayoutAfterFrameChange();
            return this;
        }

        public override UIElement WithWidth(float width)
        {
            base.WithWidth(width);
            RefreshLayoutAfterFrameChange();
            return this;
        }

        public override UIElement WithWidth(State<float> width)
        {
            base.WithWidth(width);
            if (width != null)
                AddPropertyBinding(width, RefreshLayoutAfterFrameChange, "textFrameWidthLayout", BindingKind.Layout);
            RefreshLayoutAfterFrameChange();
            return this;
        }

        public override UIElement WithHeight(float height)
        {
            base.WithHeight(height);
            RefreshLayoutAfterFrameChange();
            return this;
        }

        public override UIElement WithHeight(State<float> height)
        {
            base.WithHeight(height);
            if (height != null)
                AddPropertyBinding(height, RefreshLayoutAfterFrameChange, "textFrameHeightLayout", BindingKind.Layout);
            RefreshLayoutAfterFrameChange();
            return this;
        }

        private void RefreshLayoutAfterFrameChange()
        {
            if (builtTextComponent == null)
                return;

            ConfigureLineBehavior(builtTextComponent);
            UpdatePreferredLayout();
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
            hasExplicitTextColor = true;
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
                    UpdatePreferredLayout();
                }
            }

            return this;
        }

        private void AnimateTextColor(GameObject textObj, Color targetColor)
        {
            if (textObj == null || builtTextComponent == null) return;

            TextColorAnimator animator = BaseAnimator<Color>.GetOrReplace<TextColorAnimator>(textObj);
            animator.AnimateTo(builtTextComponent.color, targetColor, animationDuration, animationEasing);
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
                UpdatePreferredLayout();
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
                UpdatePreferredLayout();
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
                UpdatePreferredLayout();
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
                UpdatePreferredLayout();
            }

            return this;
        }

        /// <summary>Sets the font size in points.</summary>
        public TextElement SetFontSize(float size)
        {
            hasExplicitFontSize = true;
            this.fontSize = size;

            if (builtTextComponent != null)
            {
                builtTextComponent.fontSize = size;
                UpdatePreferredLayout();
            }

            return this;
        }

        /// <summary>Sets the TextMesh Pro font asset.</summary>
        public TextElement SetFont(TMP_FontAsset font)
        {
            hasExplicitFont = font != null;
            this.fontAsset = font;

            if (builtTextComponent != null && font != null)
            {
                builtTextComponent.font = font;
                UpdatePreferredLayout();
            }

            return this;
        }

        internal bool HasExplicitTextColor => hasExplicitTextColor;

        internal bool HasExplicitFontSize => hasExplicitFontSize;

        internal bool HasExplicitFont => hasExplicitFont;

        internal string PlainText => text ?? string.Empty;

        /// <summary>Sets the maximum number of visible lines.</summary>
        public TextElement SetLineLimit(int? limit)
        {
            lineLimit = limit.HasValue ? Mathf.Max(1, limit.Value) : null;
            if (builtTextComponent != null)
                ConfigureLineBehavior(builtTextComponent);
            return this;
        }

        /// <summary>Sets TextMesh Pro alignment.</summary>
        public TextElement SetTextAlignment(TextAlignmentOptions alignment)
        {
            textAlignment = alignment;
            if (builtTextComponent != null)
            {
                builtTextComponent.alignment = textAlignment;
                UpdatePreferredLayout();
            }
            return this;
        }

        /// <summary>Applies a TextMesh Pro underlay shadow.</summary>
        public TextElement SetTMProShadow(Color color, Vector2 offset, float softness = 0)
        {
            this.hasShadow = true;

            float minAlpha = 0.5f;
            if (color.a < minAlpha)
            {
                color = new Color(color.r, color.g, color.b, minAlpha);
            }

            this.shadowColor = color;

            float offsetMultiplier = 1.5f;
            this.shadowOffset = offset * offsetMultiplier;

            this.shadowSoftness = Mathf.Max(0f, softness) * 0.7f;

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
            GameObject textObj = CreateElementRoot("Text", parent);
            Image background = AddBackgroundImageIfNeeded(textObj);

            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = textColor;
            textComponent.alignment = textAlignment;
            ConfigureLineBehavior(textComponent);

            TMP_FontAsset effectiveFont = ResolveFont(fontAsset);
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

            builtTextObject = textObj;
            builtTextComponent = textComponent;
            UpdatePreferredLayout();

            ApplyAllEffects(textObj, background);

            var preferredLayoutSync = textObj.AddComponent<TextPreferredLayoutSync>();
            preferredLayoutSync.Initialize(this);

            return textObj;
        }

        private void ConfigureLineBehavior(TextMeshProUGUI textComponent)
        {
            textComponent.textWrappingMode = ShouldWrapText()
                ? TextWrappingModes.Normal
                : TextWrappingModes.NoWrap;
            textComponent.maxVisibleLines = lineLimit ?? 99999;
            textComponent.overflowMode = lineLimit == 1
                ? TextOverflowModes.Ellipsis
                : TextOverflowModes.Overflow;
            UpdatePreferredLayout();
        }

        private bool ShouldWrapText()
        {
            return preferredWidth > 0f || infiniteWidth || (lineLimit.HasValue && lineLimit.Value > 1);
        }

        private Vector2 CalculatePreferredTextSize()
        {
            if (builtTextComponent == null)
                return Vector2.zero;

            try
            {
                if (preferredWidth > 0f)
                    return builtTextComponent.GetPreferredValues(text, preferredWidth, Mathf.Infinity);

                if (infiniteWidth)
                {
                    RectTransform rect = builtTextObject != null
                        ? EnsureRectTransform(builtTextObject)
                        : null;
                    float width = rect != null ? rect.rect.width : 0f;
                    if (width > 0.5f)
                        return builtTextComponent.GetPreferredValues(text, width, Mathf.Infinity);
                }

                return builtTextComponent.GetPreferredValues();
            }
            catch (NullReferenceException)
            {
                return EstimatePreferredTextSize();
            }
        }

        private Vector2 EstimatePreferredTextSize()
        {
            string value = text ?? string.Empty;
            float lineHeight = Mathf.Max(1f, fontSize * 1.2f);
            int lines = Mathf.Max(1, value.Split('\n').Length);

            if (preferredWidth > 0f)
                return new Vector2(preferredWidth, lineHeight * lines);

            float width = Mathf.Max(1f, value.Length * fontSize * 0.55f);
            return new Vector2(width, lineHeight * lines);
        }

        internal void SyncPreferredLayoutAfterGeometryChange()
        {
            UpdatePreferredLayout();
        }

        private void UpdatePreferredLayout()
        {
            if (builtTextObject == null || builtTextComponent == null)
                return;

            Vector2 preferred = CalculatePreferredTextSize();
            LayoutElement layoutElement = builtTextObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = builtTextObject.AddComponent<LayoutElement>();

            bool widthAnimating = IsLayoutAxisAnimating<LayoutWidthAnimator>();
            bool heightAnimating = IsLayoutAxisAnimating<LayoutHeightAnimator>();

            if (!widthAnimating && infiniteWidth)
            {
                layoutElement.minWidth = 0f;
                layoutElement.preferredWidth = -1f;
                layoutElement.flexibleWidth = 1f;
            }
            else if (!widthAnimating && preferredWidth > 0f)
            {
                layoutElement.minWidth = preferredWidth;
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.flexibleWidth = 0f;
            }
            else if (!widthAnimating)
            {
                layoutElement.minWidth = 0f;
                layoutElement.preferredWidth = Mathf.Max(0f, preferred.x);
                layoutElement.flexibleWidth = 0f;
            }

            if (!heightAnimating && infiniteHeight)
            {
                layoutElement.minHeight = 0f;
                layoutElement.preferredHeight = -1f;
                layoutElement.flexibleHeight = 1f;
            }
            else if (!heightAnimating && preferredHeight > 0f)
            {
                layoutElement.minHeight = preferredHeight;
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.flexibleHeight = 0f;
            }
            else if (!heightAnimating)
            {
                layoutElement.minHeight = 0f;
                layoutElement.preferredHeight = Mathf.Max(0f, preferred.y);
                layoutElement.flexibleHeight = 0f;
            }

            LayoutCore.MarkLayoutDirty(builtTextObject);
        }

        private bool IsLayoutAxisAnimating<TAnimator>() where TAnimator : BaseAnimator<float>
        {
            TAnimator animator = builtTextObject != null ? builtTextObject.GetComponent<TAnimator>() : null;
            return animator != null && animator.IsAnimating;
        }
    }

    internal sealed class TextPreferredLayoutSync : MonoBehaviour
    {
        private TextElement owner;
        private RectTransform rectTransform;
        private float lastWidth = -1f;

        internal void Initialize(TextElement textElement)
        {
            owner = textElement;
            rectTransform = GetComponent<RectTransform>();
            RememberWidth();
        }

        internal void SyncNow()
        {
            if (owner == null)
                return;

            owner.SyncPreferredLayoutAfterGeometryChange();
            RememberWidth();
        }

        private void LateUpdate()
        {
            if (owner == null || rectTransform == null)
                return;

            float width = rectTransform.rect.width;
            if (width <= 0.5f || Mathf.Abs(width - lastWidth) <= 0.5f)
                return;

            SyncNow();
        }

        private void RememberWidth()
        {
            lastWidth = rectTransform != null ? rectTransform.rect.width : -1f;
        }
    }
}
