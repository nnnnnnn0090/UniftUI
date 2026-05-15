using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Legacy API: <c>true</c> maps to <see cref="ScrollIndicatorVisibility.Automatic"/>,
        /// <c>false</c> to <see cref="ScrollIndicatorVisibility.Hidden"/>.
        /// Use <see cref="WithScrollIndicators"/> for always-visible indicators.
        /// </summary>
        public ScrollViewElement ShowScrollbars(bool horizontal = true, bool vertical = true)
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
            GameObject container = new GameObject("ScrollView");
            container.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = container.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            ScrollRect scrollRect = container.AddComponent<ScrollRect>();
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.movementType = movementType;
            scrollRect.scrollSensitivity = scrollSensitivity;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f;

            GameObject contentContainer = new GameObject("Content");
            contentContainer.transform.SetParent(container.transform, false);

            RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;

            scrollRect.content = contentRect;

            if (vertical)
            {
                VerticalLayoutGroup layoutGroup = contentContainer.AddComponent<VerticalLayoutGroup>();
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.spacing = 8f;
                layoutGroup.padding = this.padding ?? new RectOffset(10, 10, 10, 10);
            }
            else if (horizontal)
            {
                HorizontalLayoutGroup layoutGroup = contentContainer.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = true;
                layoutGroup.spacing = 8f;
                layoutGroup.padding = this.padding ?? new RectOffset(10, 10, 10, 10);
            }

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

            LayoutElement layoutElement = container.AddComponent<LayoutElement>();
            if (preferredWidth > 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
            }
            else if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
            }
            else
            {
                layoutElement.preferredWidth = 300;
                layoutElement.minWidth = 100;
                layoutElement.flexibleWidth = 1;
            }

            if (preferredHeight > 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
                layoutElement.flexibleHeight = 0;
            }
            else if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
            }
            else
            {
                layoutElement.preferredHeight = 200;
                layoutElement.minHeight = 100;
                layoutElement.flexibleHeight = 1;
            }

            foreach (var child in children)
            {
                child.Build(contentContainer.transform);
            }

            ApplyAllEffects(container, backgroundImage);

            if (bindVerticalNormalized != null || bindHorizontalNormalized != null)
            {
                var bridge = container.AddComponent<UniftUIScrollRectBridge>();
                bridge.Initialize(scrollRect, bindVerticalNormalized, bindHorizontalNormalized, twoWayVertical, twoWayHorizontal);
            }

            if (states != null && states.Length > 0)
            {
                SetupStateObserver(container, contentContainer);
            }

            return container;
        }

        private GameObject CreateVerticalScrollbar(GameObject parent)
        {
            GameObject scrollbar = new GameObject("VerticalScrollbar");
            scrollbar.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = scrollbar.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.sizeDelta = new Vector2(10, 0);

            Image scrollbarImage = scrollbar.AddComponent<Image>();
            scrollbarImage.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);

            Scrollbar scrollbarComp = scrollbar.AddComponent<Scrollbar>();
            scrollbarComp.direction = Scrollbar.Direction.BottomToTop;

            GameObject slidingArea = new GameObject("SlidingArea");
            slidingArea.transform.SetParent(scrollbar.transform, false);

            RectTransform slidingRect = slidingArea.AddComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(slidingArea.transform, false);

            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.sizeDelta = Vector2.zero;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.4f, 0.4f, 0.4f, 0.7f);

            scrollbarComp.handleRect = handleRect;
            scrollbarComp.targetGraphic = handleImage;

            return scrollbar;
        }

        private GameObject CreateHorizontalScrollbar(GameObject parent)
        {
            GameObject scrollbar = new GameObject("HorizontalScrollbar");
            scrollbar.transform.SetParent(parent.transform, false);

            RectTransform rectTransform = scrollbar.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.sizeDelta = new Vector2(0, 10);

            Image scrollbarImage = scrollbar.AddComponent<Image>();
            scrollbarImage.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);

            Scrollbar scrollbarComp = scrollbar.AddComponent<Scrollbar>();
            scrollbarComp.direction = Scrollbar.Direction.LeftToRight;

            GameObject slidingArea = new GameObject("SlidingArea");
            slidingArea.transform.SetParent(scrollbar.transform, false);

            RectTransform slidingRect = slidingArea.AddComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.sizeDelta = Vector2.zero;

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(slidingArea.transform, false);

            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.sizeDelta = Vector2.zero;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.4f, 0.4f, 0.4f, 0.7f);

            scrollbarComp.handleRect = handleRect;
            scrollbarComp.targetGraphic = handleImage;

            return scrollbar;
        }

        private void SetupStateObserver(GameObject container, GameObject contentContainer)
        {
            StateObserver observer = container.AddComponent<StateObserver>();
            observer.Initialize(states, () => {
                foreach (Transform child in contentContainer.transform)
                {
                    if (child.gameObject != null)
                        GameObject.Destroy(child.gameObject);
                }

                children.Clear();

                var parentContext = UIContext.Current;
                UIContext.Current = this;

                if (content != null)
                    content.Invoke();

                UIContext.Current = parentContext;

                foreach (var child in children)
                {
                    if (child == null || contentContainer == null || contentContainer.transform == null)
                        continue;
                    if (UIContext.DefaultFont != null)
                        child.Font(UIContext.DefaultFont);
                    child.Build(contentContainer.transform);
                }

                Canvas.ForceUpdateCanvases();
            });
        }
    }
}
