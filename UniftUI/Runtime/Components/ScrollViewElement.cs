using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>
    /// A scrollable container with optional axis configuration.
    /// </summary>
    public class ScrollViewElement : UIElement, ILayoutContainer
    {
        private Action content;
        private State[] states;
        private List<UIElement> children = new List<UIElement>();
        private bool horizontal;
        private bool vertical;
        private ScrollIndicatorVisibility verticalIndicatorVisibility = ScrollIndicatorVisibility.Automatic;
        private ScrollIndicatorVisibility horizontalIndicatorVisibility = ScrollIndicatorVisibility.Automatic;

        private ScrollRect.MovementType movementType = ScrollRect.MovementType.Elastic;
        private float scrollSensitivity = 10f;
        private State<float> bindVerticalNormalized;
        private State<float> bindHorizontalNormalized;
        private bool twoWayVertical;
        private bool twoWayHorizontal;

        /// <summary>Creates a scroll view with the given scroll axes and optional state dependencies.</summary>
        public ScrollViewElement(Action content, State[] states = null, bool horizontal = false, bool vertical = true)
        {
            this.content = content;
            this.states = states;
            this.horizontal = horizontal;
            this.vertical = vertical;
            MaterializeContent(content, children);
        }

        internal ScrollViewElement ShowScrollbars(bool horizontal = true, bool vertical = true)
        {
            if (this.vertical)
                verticalIndicatorVisibility = vertical ? ScrollIndicatorVisibility.Automatic : ScrollIndicatorVisibility.Hidden;
            if (this.horizontal)
                horizontalIndicatorVisibility = horizontal ? ScrollIndicatorVisibility.Automatic : ScrollIndicatorVisibility.Hidden;
            return this;
        }

        /// <summary>
        /// Sets scroll indicator visibility for the specified axes only.
        /// </summary>
        /// <param name="axes">Bitwise combination of <see cref="UniftUIScrollAxis.Vertical"/> and <see cref="UniftUIScrollAxis.Horizontal"/>.</param>
        public ScrollViewElement WithScrollIndicators(ScrollIndicatorVisibility visibility, UniftUIScrollAxis axes)
        {
            if (vertical && (axes & UniftUIScrollAxis.Vertical) != 0)
                verticalIndicatorVisibility = visibility;
            if (horizontal && (axes & UniftUIScrollAxis.Horizontal) != 0)
                horizontalIndicatorVisibility = visibility;
            return this;
        }

        /// <summary>
        /// Applies <paramref name="visibility"/> to every enabled scroll axis.
        /// </summary>
        public ScrollViewElement WithScrollIndicators(ScrollIndicatorVisibility visibility)
        {
            if (vertical)
                verticalIndicatorVisibility = visibility;
            if (horizontal)
                horizontalIndicatorVisibility = visibility;
            return this;
        }

        /// <summary>Enables or disables elastic bounce (<see cref="ScrollRect.movementType"/>).</summary>
        public ScrollViewElement WithScrollBounce(bool elastic)
        {
            movementType = elastic ? ScrollRect.MovementType.Elastic : ScrollRect.MovementType.Clamped;
            return this;
        }

        /// <summary>Sets <see cref="ScrollRect.movementType"/> directly.</summary>
        public ScrollViewElement WithMovementType(ScrollRect.MovementType type)
        {
            movementType = type;
            return this;
        }

        /// <summary>Sets wheel and trackpad sensitivity (<see cref="ScrollRect.scrollSensitivity"/>).</summary>
        public ScrollViewElement WithScrollSensitivity(float sensitivity)
        {
            scrollSensitivity = Mathf.Max(0.01f, sensitivity);
            return this;
        }

        /// <summary>
        /// Binds vertical scroll position to <paramref name="normalized"/>
        /// (<see cref="ScrollRect.verticalNormalizedPosition"/>: 1 = top, 0 = bottom).
        /// </summary>
        public ScrollViewElement BindScrollPositionY(State<float> normalized, bool twoWay = false)
        {
            bindVerticalNormalized = normalized;
            twoWayVertical |= twoWay;
            return this;
        }

        /// <summary>
        /// Binds horizontal scroll position (<see cref="ScrollRect.horizontalNormalizedPosition"/>: 0 = left, 1 = right).
        /// </summary>
        public ScrollViewElement BindScrollPositionX(State<float> normalized, bool twoWay = false)
        {
            bindHorizontalNormalized = normalized;
            twoWayHorizontal |= twoWay;
            return this;
        }

        /// <inheritdoc />
        public void AddChild(UIElement child)
        {
            if (child != null)
            {
                children.Add(child);
            }
        }

        /// <inheritdoc />
        public void RemoveChild(UIElement child)
        {
            if (child != null)
            {
                children.Remove(child);
            }
        }

        /// <inheritdoc />
        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;
            int index = children.IndexOf(oldChild);
            if (index != -1)
            {
                children[index] = newChild;
            }
            else
            {
                Debug.LogWarning($"[UniftUI] ReplaceChild: oldChild not found in ScrollView. Old: {oldChild}, New: {newChild}. Children count: {children.Count}");
            }
        }

        /// <inheritdoc />
        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        /// <inheritdoc />
        public override GameObject Build(Transform parent)
        {
            GameObject container = CreateElementRoot("ScrollView", parent);
            Image backgroundImage = AddBackgroundImageIfNeeded(container);

            ScrollRect scrollRect = container.AddComponent<ScrollRect>();
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.movementType = movementType;
            scrollRect.scrollSensitivity = scrollSensitivity;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;

            GameObject contentContainer = CreateChildObject("Content", container.transform);
            RectTransform contentRect = EnsureRectTransform(contentContainer);
            ConfigureContentRect(contentRect);

            scrollRect.content = contentRect;

            UniftUIStackLayoutGroup layoutGroup = contentContainer.AddComponent<UniftUIStackLayoutGroup>();
            layoutGroup.padding = this.padding ?? new RectOffset(10, 10, 10, 10);
            layoutGroup.Configure(
                horizontal && !vertical ? UniftUIStackAxis.Horizontal : UniftUIStackAxis.Vertical,
                8f,
                VStackAlignment.Center,
                HStackAlignment.Center);

            ContentSizeFitter contentFitter = contentContainer.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = horizontal ?
                ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = vertical ?
                ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;

            container.AddComponent<RectMask2D>();

            if (vertical && verticalIndicatorVisibility != ScrollIndicatorVisibility.Hidden)
            {
                GameObject verticalScrollbar = CreateVerticalScrollbar(container);
                scrollRect.verticalScrollbar = verticalScrollbar.GetComponent<Scrollbar>();
                scrollRect.verticalScrollbarVisibility = verticalIndicatorVisibility == ScrollIndicatorVisibility.Visible
                    ? ScrollRect.ScrollbarVisibility.Permanent
                    : ScrollRect.ScrollbarVisibility.AutoHide;
            }

            if (horizontal && horizontalIndicatorVisibility != ScrollIndicatorVisibility.Hidden)
            {
                GameObject horizontalScrollbar = CreateHorizontalScrollbar(container);
                scrollRect.horizontalScrollbar = horizontalScrollbar.GetComponent<Scrollbar>();
                scrollRect.horizontalScrollbarVisibility = horizontalIndicatorVisibility == ScrollIndicatorVisibility.Visible
                    ? ScrollRect.ScrollbarVisibility.Permanent
                    : ScrollRect.ScrollbarVisibility.AutoHide;
            }

            LayoutElement layoutElement = LayoutElementUtility.Configure(
                container,
                preferredWidth,
                preferredHeight,
                infiniteWidth,
                infiniteHeight,
                300f,
                200f);
            if (preferredWidth < 0 && !infiniteWidth)
            {
                layoutElement.minWidth = 100f;
                layoutElement.flexibleWidth = 1f;
            }
            if (preferredHeight < 0 && !infiniteHeight)
            {
                layoutElement.minHeight = 100f;
                layoutElement.flexibleHeight = 1f;
            }

            BuildContentChildren(children, contentContainer.transform, ConfigureScrollChild);

            ApplyAllEffects(container, backgroundImage);

            if (bindVerticalNormalized != null || bindHorizontalNormalized != null)
            {
                var bridge = container.AddComponent<UniftUIScrollRectBridge>();
                bridge.Initialize(scrollRect, bindVerticalNormalized, bindHorizontalNormalized, twoWayVertical, twoWayHorizontal);
            }

            SetupContentRebuildObserver(
                states,
                container,
                contentContainer.transform,
                children,
                content,
                "ScrollView",
                ConfigureScrollChild);

            return container;
        }

        private GameObject CreateVerticalScrollbar(GameObject parent)
        {
            GameObject scrollbar = CreateChildObject("VerticalScrollbar", parent.transform);

            RectTransform rectTransform = EnsureRectTransform(scrollbar);
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.sizeDelta = new Vector2(10, 0);

            AddImage(scrollbar, new Color(0.7f, 0.7f, 0.7f, 0.7f));

            Scrollbar scrollbarComp = scrollbar.AddComponent<Scrollbar>();
            scrollbarComp.direction = Scrollbar.Direction.BottomToTop;

            GameObject slidingArea = CreateFullStretchChild("SlidingArea", scrollbar.transform);

            GameObject handle = CreateFullStretchChild("Handle", slidingArea.transform);
            RectTransform handleRect = EnsureRectTransform(handle);

            Color handleColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
            Image handleImage = AddImage(handle, handleColor);

            scrollbarComp.handleRect = handleRect;
            scrollbarComp.targetGraphic = handleImage;
            ConfigureSelectableColors(scrollbarComp, handleColor);

            return scrollbar;
        }

        private GameObject CreateHorizontalScrollbar(GameObject parent)
        {
            GameObject scrollbar = CreateChildObject("HorizontalScrollbar", parent.transform);

            RectTransform rectTransform = EnsureRectTransform(scrollbar);
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.sizeDelta = new Vector2(0, 10);

            AddImage(scrollbar, new Color(0.7f, 0.7f, 0.7f, 0.7f));

            Scrollbar scrollbarComp = scrollbar.AddComponent<Scrollbar>();
            scrollbarComp.direction = Scrollbar.Direction.LeftToRight;

            GameObject slidingArea = CreateFullStretchChild("SlidingArea", scrollbar.transform);

            GameObject handle = CreateFullStretchChild("Handle", slidingArea.transform);
            RectTransform handleRect = EnsureRectTransform(handle);

            Color handleColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
            Image handleImage = AddImage(handle, handleColor);

            scrollbarComp.handleRect = handleRect;
            scrollbarComp.targetGraphic = handleImage;
            ConfigureSelectableColors(scrollbarComp, handleColor);

            return scrollbar;
        }

        private void ConfigureScrollChild(UIElement child)
        {
            if (child == null)
                return;

            if (vertical && !horizontal && ChildMayFillWidth(child))
                child.WithInfiniteWidth();
            if (horizontal && !vertical && ChildMayFillHeight(child))
                child.WithInfiniteHeight();
        }

        private void ConfigureContentRect(RectTransform contentRect)
        {
            if (contentRect == null) return;

            if (vertical && !horizontal)
            {
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(1f, 1f);
                contentRect.pivot = new Vector2(0.5f, 1f);
            }
            else if (horizontal && !vertical)
            {
                contentRect.anchorMin = new Vector2(0f, 0f);
                contentRect.anchorMax = new Vector2(0f, 1f);
                contentRect.pivot = new Vector2(0f, 0.5f);
            }
            else
            {
                contentRect.anchorMin = new Vector2(0f, 1f);
                contentRect.anchorMax = new Vector2(0f, 1f);
                contentRect.pivot = new Vector2(0f, 1f);
            }

            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
        }
    }
}
