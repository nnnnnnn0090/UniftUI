using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// Overlays child views on top of each other.
    /// </summary>
    public class ZStackElement : UIElement, ILayoutContainer
    {
        private Action content;
        private State[] states;
        private List<UIElement> children = new List<UIElement>();
        private ZStackAlignment alignment;

        /// <summary>Creates a <see cref="ZStackElement"/> with the given content builder and optional state dependencies.</summary>
        public ZStackElement(Action content, State[] states = null, ZStackAlignment alignment = ZStackAlignment.Center)
        {
            this.content = content;
            this.states = states;
            this.alignment = alignment;

            if (content != null)
            {
                var parentContext = UIContext.Current;
                try
                {
                    UIContext.Current = this;
                    content.Invoke();
                }
                finally
                {
                    UIContext.Current = parentContext;
                }
            }
        }

        /// <inheritdoc />
        public void AddChild(UIElement child)
        {
            if (child != null) children.Add(child);
        }

        /// <inheritdoc />
        public void RemoveChild(UIElement child)
        {
            if (child != null) children.Remove(child);
        }

        /// <inheritdoc />
        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;
            int index = children.IndexOf(oldChild);
            if (index != -1)
                children[index] = newChild;
            else
                Debug.LogWarning($"[UniftUI] ReplaceChild: oldChild not found in ZStack. Old: {oldChild}, New: {newChild}. Children count: {children.Count}");
        }

        /// <inheritdoc />
        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        /// <inheritdoc />
        public override GameObject Build(Transform parent)
        {
            GameObject container = new GameObject("ZStack");
            container.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = container.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            LayoutElement layoutElement = container.AddComponent<LayoutElement>();
            ContentSizeFitter buttonFitter = container.AddComponent<ContentSizeFitter>();

            buttonFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
                buttonFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredWidth >= 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
            }

            if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
                buttonFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
            }

            TextAnchor childAlignment = GetTextAnchorFromAlignment(alignment);
            RectOffset actualPadding = padding ?? new RectOffset(0, 0, 0, 0);

            float maxWidth = 0;
            float maxHeight = 0;

            foreach (var child in children)
            {
                if (child == null) continue;

                GameObject childObj = child.Build(container.transform);

                RectTransform childRect = childObj.GetComponent<RectTransform>();
                if (childRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(childRect);
                    maxWidth = Mathf.Max(maxWidth, childRect.rect.width);
                    maxHeight = Mathf.Max(maxHeight, childRect.rect.height);

                    ApplyZStackChildRect(child, childObj, childRect, actualPadding, childAlignment);
                }
            }

            if (!infiniteWidth && preferredWidth < 0 && maxWidth > 0)
            {
                layoutElement.preferredWidth = maxWidth + actualPadding.left + actualPadding.right;
                layoutElement.minWidth = maxWidth + actualPadding.left + actualPadding.right;
            }

            if (!infiniteHeight && preferredHeight < 0 && maxHeight > 0)
            {
                layoutElement.preferredHeight = maxHeight + actualPadding.top + actualPadding.bottom;
                layoutElement.minHeight = maxHeight + actualPadding.top + actualPadding.bottom;
            }

            if (states != null && states.Length > 0)
            {
                SetupStateObserver(container);
            }

            ApplyAllEffects(container, backgroundImage);

            return container;
        }

        private void SetupStateObserver(GameObject container)
        {
            if (container == null) return;

            StateObserver observer = container.AddComponent<StateObserver>();

            observer.Initialize(states, () => {
                if (container == null || !container)
                {
                    Debug.LogWarning("[UniftUI] ZStack: container was already destroyed.");
                    return;
                }

                try
                {
                    foreach (Transform child in container.transform)
                    {
                        if (child != null && child.gameObject != null)
                            GameObject.Destroy(child.gameObject);
                    }

                    children.Clear();

                    var parentContext = UIContext.Current;
                    UIContext.Current = this;

                    if (content != null)
                        content.Invoke();

                    UIContext.Current = parentContext;

                    TextAnchor childAlignment = GetTextAnchorFromAlignment(alignment);
                    RectOffset actualPadding = padding ?? new RectOffset(0, 0, 0, 0);

                    foreach (var child in children)
                    {
                        if (child == null) continue;

                        if (UIContext.DefaultFont != null)
                            child.Font(UIContext.DefaultFont);

                        GameObject childObj = child.Build(container.transform);

                        RectTransform childRect = childObj.GetComponent<RectTransform>();
                        if (childRect != null)
                        {
                            ApplyZStackChildRect(child, childObj, childRect, actualPadding, childAlignment);
                        }
                    }

                    Canvas.ForceUpdateCanvases();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniftUI] ZStack: error while rebuilding UI: {e.Message}\n{e.StackTrace}");
                }
            });
        }

        /// <summary>
        /// Lays out a child in the stack. <see cref="UIElement.WithPosition"/> uses a top-left origin with Y increasing downward.
        /// </summary>
        private void ApplyZStackChildRect(UIElement child, GameObject childObj, RectTransform childRect,
            RectOffset actualPadding, TextAnchor childAlignment)
        {
            ContentSizeFitter childFitter = childObj.GetComponent<ContentSizeFitter>();
            if (childFitter != null)
            {
                if (!child.infiniteWidth)
                    childFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                if (!child.infiniteHeight)
                    childFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (child.infiniteWidth || child.infiniteHeight)
            {
                childRect.anchorMin = Vector2.zero;
                childRect.anchorMax = Vector2.one;
                childRect.pivot = GetPivotFromAlignment(alignment);
                childRect.offsetMin = new Vector2(actualPadding.left, actualPadding.bottom);
                childRect.offsetMax = new Vector2(-actualPadding.right, -actualPadding.top);
            }
            else if (child.useCustomPosition)
            {
                childRect.anchorMin = new Vector2(0, 1);
                childRect.anchorMax = new Vector2(0, 1);
                childRect.pivot = new Vector2(0, 1);
                Vector2 p = child.customPosition;
                childRect.anchoredPosition = new Vector2(
                    actualPadding.left + p.x,
                    -actualPadding.top - p.y);
            }
            else
            {
                Vector2 alignmentAnchor = GetAnchorFromAlignment(alignment);
                childRect.anchorMin = alignmentAnchor;
                childRect.anchorMax = alignmentAnchor;
                childRect.pivot = GetPivotFromAlignment(alignment);

                Vector2 positionOffset = Vector2.zero;
                switch (alignment)
                {
                    case ZStackAlignment.TopLeading:
                        positionOffset = new Vector2(actualPadding.left, -actualPadding.top);
                        break;
                    case ZStackAlignment.Top:
                        positionOffset = new Vector2(0, -actualPadding.top);
                        break;
                    case ZStackAlignment.TopTrailing:
                        positionOffset = new Vector2(-actualPadding.right, -actualPadding.top);
                        break;
                    case ZStackAlignment.Leading:
                        positionOffset = new Vector2(actualPadding.left, 0);
                        break;
                    case ZStackAlignment.Center:
                        positionOffset = Vector2.zero;
                        break;
                    case ZStackAlignment.Trailing:
                        positionOffset = new Vector2(-actualPadding.right, 0);
                        break;
                    case ZStackAlignment.BottomLeading:
                        positionOffset = new Vector2(actualPadding.left, actualPadding.bottom);
                        break;
                    case ZStackAlignment.Bottom:
                        positionOffset = new Vector2(0, actualPadding.bottom);
                        break;
                    case ZStackAlignment.BottomTrailing:
                        positionOffset = new Vector2(-actualPadding.right, actualPadding.bottom);
                        break;
                }

                childRect.anchoredPosition = positionOffset;
            }

            SetChildAlignment(childObj, childAlignment);
        }

        private TextAnchor GetTextAnchorFromAlignment(ZStackAlignment alignment)
        {
            switch (alignment)
            {
                case ZStackAlignment.TopLeading: return TextAnchor.UpperLeft;
                case ZStackAlignment.Top: return TextAnchor.UpperCenter;
                case ZStackAlignment.TopTrailing: return TextAnchor.UpperRight;
                case ZStackAlignment.Leading: return TextAnchor.MiddleLeft;
                case ZStackAlignment.Center: return TextAnchor.MiddleCenter;
                case ZStackAlignment.Trailing: return TextAnchor.MiddleRight;
                case ZStackAlignment.BottomLeading: return TextAnchor.LowerLeft;
                case ZStackAlignment.Bottom: return TextAnchor.LowerCenter;
                case ZStackAlignment.BottomTrailing: return TextAnchor.LowerRight;
                default: return TextAnchor.MiddleCenter;
            }
        }

        private Vector2 GetPivotFromAlignment(ZStackAlignment alignment)
        {
            switch (alignment)
            {
                case ZStackAlignment.TopLeading: return new Vector2(0, 1);
                case ZStackAlignment.Top: return new Vector2(0.5f, 1);
                case ZStackAlignment.TopTrailing: return new Vector2(1, 1);
                case ZStackAlignment.Leading: return new Vector2(0, 0.5f);
                case ZStackAlignment.Center: return new Vector2(0.5f, 0.5f);
                case ZStackAlignment.Trailing: return new Vector2(1, 0.5f);
                case ZStackAlignment.BottomLeading: return new Vector2(0, 0);
                case ZStackAlignment.Bottom: return new Vector2(0.5f, 0);
                case ZStackAlignment.BottomTrailing: return new Vector2(1, 0);
                default: return new Vector2(0.5f, 0.5f);
            }
        }

        private void SetChildAlignment(GameObject childObj, TextAnchor alignment)
        {
            VerticalLayoutGroup vlg = childObj.GetComponent<VerticalLayoutGroup>();
            if (vlg != null) { vlg.childAlignment = alignment; return; }

            HorizontalLayoutGroup hlg = childObj.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) { hlg.childAlignment = alignment; return; }

            GridLayoutGroup glg = childObj.GetComponent<GridLayoutGroup>();
            if (glg != null) { glg.childAlignment = alignment; return; }

            TMPro.TextMeshProUGUI tmpText = childObj.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                switch (alignment)
                {
                    case TextAnchor.UpperLeft: tmpText.alignment = TMPro.TextAlignmentOptions.TopLeft; break;
                    case TextAnchor.UpperCenter: tmpText.alignment = TMPro.TextAlignmentOptions.Top; break;
                    case TextAnchor.UpperRight: tmpText.alignment = TMPro.TextAlignmentOptions.TopRight; break;
                    case TextAnchor.MiddleLeft: tmpText.alignment = TMPro.TextAlignmentOptions.Left; break;
                    case TextAnchor.MiddleCenter: tmpText.alignment = TMPro.TextAlignmentOptions.Center; break;
                    case TextAnchor.MiddleRight: tmpText.alignment = TMPro.TextAlignmentOptions.Right; break;
                    case TextAnchor.LowerLeft: tmpText.alignment = TMPro.TextAlignmentOptions.BottomLeft; break;
                    case TextAnchor.LowerCenter: tmpText.alignment = TMPro.TextAlignmentOptions.Bottom; break;
                    case TextAnchor.LowerRight: tmpText.alignment = TMPro.TextAlignmentOptions.BottomRight; break;
                }
            }
        }

        protected override void PropagateInfiniteWidthToContent()
        {
        }

        protected override void PropagateInfiniteHeightToContent()
        {
        }

        private Vector2 GetAnchorFromAlignment(ZStackAlignment alignment)
        {
            switch (alignment)
            {
                case ZStackAlignment.TopLeading: return new Vector2(0, 1);
                case ZStackAlignment.Top: return new Vector2(0.5f, 1);
                case ZStackAlignment.TopTrailing: return new Vector2(1, 1);
                case ZStackAlignment.Leading: return new Vector2(0, 0.5f);
                case ZStackAlignment.Center: return new Vector2(0.5f, 0.5f);
                case ZStackAlignment.Trailing: return new Vector2(1, 0.5f);
                case ZStackAlignment.BottomLeading: return new Vector2(0, 0);
                case ZStackAlignment.Bottom: return new Vector2(0.5f, 0);
                case ZStackAlignment.BottomTrailing: return new Vector2(1, 0);
                default: return new Vector2(0.5f, 0.5f);
            }
        }
    }

    /// <summary>Alignment options for <see cref="ZStackElement"/> children.</summary>
    public enum ZStackAlignment
    {
        TopLeading,
        Top,
        TopTrailing,
        Leading,
        Center,
        Trailing,
        BottomLeading,
        Bottom,
        BottomTrailing
    }
}
