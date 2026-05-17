using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>
    /// A tabbed container with animated content transitions.
    /// </summary>
    public class TabView : UIElement, ILayoutContainer
    {
        private List<TabItem> tabs = new List<TabItem>();
        private State<int> selectedIndex;
        private List<UIElement> children = new List<UIElement>();
        private Action content;
        private Color tabBarColor = new Color(0.95f, 0.95f, 0.95f);
        private Color activeTabColor = new Color(0.2f, 0.6f, 1.0f);
        private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);

        private TMPro.TMP_FontAsset fontAsset = null;

        private float transitionDuration = 0.3f;
        private GameObject currentContentObj;
        private GameObject pendingContentToDestroy;
        private DelayedCallback currentAnimation;
        private Transform builtTabBarParent;
        private Transform builtContentParent;
        private int renderedIndex = -1;

        /// <summary>Sets the font used for tab labels and tab content.</summary>
        public TabView SetFont(TMPro.TMP_FontAsset font)
        {
            this.fontAsset = font;
            return this;
        }

        /// <summary>
        /// Creates a tab view. Uses <paramref name="externalSelectedIndex"/> when provided;
        /// otherwise creates an internal <see cref="State{T}"/> starting at 0.
        /// </summary>
        public TabView(Action content, State<int> externalSelectedIndex = null)
        {
            this.content = content;
            this.selectedIndex = externalSelectedIndex ?? new State<int>(0);

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

        /// <summary>Sets the cross-fade duration when switching tabs (seconds).</summary>
        public TabView WithTransitionDuration(float duration)
        {
            this.transitionDuration = Mathf.Max(0, duration);
            return this;
        }

        /// <inheritdoc />
        public void AddChild(UIElement child)
        {
            if (child is TabItem tabItem)
            {
                tabs.Add(tabItem);
            }
            else
            {
                children.Add(child);
            }
        }

        /// <inheritdoc />
        public void RemoveChild(UIElement child)
        {
            if (child is TabItem tabItem)
            {
                tabs.Remove(tabItem);
            }
            else
            {
                children.Remove(child);
            }
        }

        /// <inheritdoc />
        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;

            if (oldChild is TabItem oldTabItem && newChild is TabItem newTabItem)
            {
                int index = tabs.IndexOf(oldTabItem);
                if (index != -1)
                {
                    tabs[index] = newTabItem;
                }
                else
                {
                    Debug.LogWarning($"[UniftUI] ReplaceChild: oldTabItem not found in TabView. Old: {oldChild}, New: {newChild}");
                }
            }
            else
            {
                int index = children.IndexOf(oldChild);
                if (index != -1)
                {
                    children[index] = newChild;
                }
                else
                {
                    Debug.LogWarning($"[UniftUI] ReplaceChild: oldChild not found in TabView children. Old: {oldChild}, New: {newChild}");
                }
            }
        }

        /// <inheritdoc />
        public System.Collections.Generic.IEnumerable<UIElement> GetChildren()
        {
            var allChildren = new List<UIElement>();
            allChildren.AddRange(tabs);
            allChildren.AddRange(children);
            return allChildren;
        }

        /// <inheritdoc />
        public override GameObject Build(Transform parent)
        {
            GameObject container = new GameObject("TabView");
            container.transform.SetParent(parent, false);

            Image background = null;
            if (backgroundColor != Color.clear)
            {
                background = container.AddComponent<Image>();
                background.color = backgroundColor;
            }

            UniftUIStackLayoutGroup mainLayout = container.AddComponent<UniftUIStackLayoutGroup>();
            mainLayout.padding = new RectOffset(0, 0, 0, 0);
            mainLayout.Configure(UniftUIStackAxis.Vertical, 0f, VStackAlignment.Center, HStackAlignment.Center);

            GameObject tabBar = CreateTabBar(container.transform);
            GameObject contentArea = CreateContentArea(container.transform);

            ApplyAllEffects(container, background);

            ConfigureLayout(container);

            return container;
        }

        private GameObject CreateContentArea(Transform parent)
        {
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(parent, false);

            LayoutElement contentLayout = contentArea.AddComponent<LayoutElement>();
            contentLayout.minHeight = 0;
            contentLayout.preferredHeight = -1;
            contentLayout.flexibleHeight = 1;
            contentLayout.flexibleWidth = 1;

            ClampSelectedIndex();

            if (tabs.Count > 0 && selectedIndex.Value >= 0 && selectedIndex.Value < tabs.Count)
            {
                currentContentObj = CreateTabContent(contentArea.transform, tabs[selectedIndex.Value]);
                renderedIndex = selectedIndex.Value;
            }

            StateObserver observer = contentArea.AddComponent<StateObserver>();
            observer.Initialize(new State[] { selectedIndex }, () => {
                ApplySelectedTab();
            });

            builtContentParent = contentArea.transform;
            return contentArea;
        }

        private void ApplySelectedTab()
        {
            ClampSelectedIndex();
            UpdateTabButtons(builtTabBarParent);

            if (builtContentParent == null || tabs.Count == 0 || selectedIndex.Value < 0 || selectedIndex.Value >= tabs.Count)
                return;

            if (renderedIndex == selectedIndex.Value && currentContentObj != null)
                return;

            renderedIndex = selectedIndex.Value;
            SwitchTabWithAnimation(builtContentParent, tabs[renderedIndex]);
        }

        private void SwitchTabWithAnimation(Transform contentParent, TabItem newTab)
        {
            CancelCurrentAnimation();

            if (currentContentObj != null)
            {
                pendingContentToDestroy = currentContentObj;

                if (transitionDuration > 0f)
                    UIAnimator.Fade(pendingContentToDestroy, 1.0f, 0.0f, transitionDuration * 0.5f, null);
            }

            GameObject newContent = CreateTabContent(contentParent, newTab);

            CanvasGroup canvasGroup = newContent.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = newContent.AddComponent<CanvasGroup>();
            }

            currentContentObj = newContent;

            if (transitionDuration <= 0f)
            {
                if (pendingContentToDestroy != null)
                {
                    DestroyGameObject(pendingContentToDestroy);
                    pendingContentToDestroy = null;
                }

                canvasGroup.alpha = 1f;
                Canvas.ForceUpdateCanvases();
                return;
            }

            canvasGroup.alpha = 0;

            currentAnimation = contentParent.gameObject.AddComponent<DelayedCallback>();
            currentAnimation.Initialize(transitionDuration * 0.2f, () => {
                if (pendingContentToDestroy != null)
                {
                    DestroyGameObject(pendingContentToDestroy);
                    pendingContentToDestroy = null;
                }

                UIAnimator.Fade(newContent, 0.0f, 1.0f, transitionDuration * 0.5f, () => {
                    currentAnimation = null;
                });
            });

            Canvas.ForceUpdateCanvases();
        }

        private void CancelCurrentAnimation()
        {
            if (currentAnimation != null)
            {
                currentAnimation.Cancel();
                DestroyUnityObject(currentAnimation);
                currentAnimation = null;
            }

            if (pendingContentToDestroy != null)
            {
                DestroyGameObject(pendingContentToDestroy);
                pendingContentToDestroy = null;
            }
        }

        private GameObject CreateTabContent(Transform parent, TabItem tab)
        {
            GameObject tabRootContentObj = new GameObject($"Content_Tab_{parent.childCount}");
            tabRootContentObj.transform.SetParent(parent, false);

            RectTransform rect = tabRootContentObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layoutGroup = tabRootContentObj.AddComponent<UniftUIStackLayoutGroup>();
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.Configure(UniftUIStackAxis.Vertical, 0f, VStackAlignment.Center, HStackAlignment.Center);

            var tabContentBuilder = new TabContentContainer(tabRootContentObj.transform);

            TMPro.TMP_FontAsset resolvedFont = ResolveFont(fontAsset);
            if (resolvedFont != null)
            {
                tabContentBuilder.SetFont(resolvedFont);
            }

            var parentContext = UIContext.Current;
            try
            {
                UIContext.Current = tabContentBuilder;
                tab.Content?.Invoke();
            }
            finally
            {
                UIContext.Current = parentContext;
            }
            tabContentBuilder.BuildChildren();

            return tabRootContentObj;
        }

        private GameObject CreateTabBar(Transform parent)
        {
            GameObject tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(parent, false);

            Image tabBarBg = tabBar.AddComponent<Image>();
            tabBarBg.color = tabBarColor;

            LayoutElement tabBarLayout = tabBar.AddComponent<LayoutElement>();
            tabBarLayout.minHeight = 60;
            tabBarLayout.preferredHeight = 60;
            tabBarLayout.flexibleHeight = 0;
            tabBarLayout.flexibleWidth = 1;

            UniftUIStackLayoutGroup tabLayout = tabBar.AddComponent<UniftUIStackLayoutGroup>();
            tabLayout.padding = new RectOffset(0, 0, 0, 0);
            tabLayout.Configure(UniftUIStackAxis.Horizontal, 0f, VStackAlignment.Center, HStackAlignment.Center);

            for (int i = 0; i < tabs.Count; i++)
            {
                CreateTabButton(tabBar.transform, tabs[i], i);
            }

            StateObserver observer = tabBar.AddComponent<StateObserver>();
            observer.Initialize(new State[] { selectedIndex }, () => {
                UpdateTabButtons(tabBar.transform);
            });

            builtTabBarParent = tabBar.transform;
            return tabBar;
        }

        private void CreateTabButton(Transform parent, TabItem tab, int index)
        {
            GameObject tabButton = new GameObject($"Tab_{index}");
            tabButton.transform.SetParent(parent, false);

            Image buttonBg = tabButton.AddComponent<Image>();
            buttonBg.color = index == selectedIndex.Value ? activeTabColor : inactiveTabColor;

            Button button = tabButton.AddComponent<Button>();
            button.targetGraphic = buttonBg;
            button.onClick.AddListener(() => {
                if (selectedIndex.Value == index)
                    return;
                selectedIndex.Value = index;
                ApplySelectedTab();
                UpdateTabButtons(parent);
            });

            GameObject contentContainer = new GameObject("TabHeaderContent");
            contentContainer.transform.SetParent(tabButton.transform, false);
            var headerLayout = tabButton.AddComponent<UniftUISingleChildLayoutGroup>();
            headerLayout.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            if (tab.TitleContent != null)
            {
                var contentBuilder = new TabContentContainer(contentContainer.transform);
                TMPro.TMP_FontAsset resolvedFont = ResolveFont(fontAsset);
                if (resolvedFont != null)
                {
                    contentBuilder.SetFont(resolvedFont);
                }

                var parentContext = UIContext.Current;
                try
                {
                    UIContext.Current = contentBuilder;
                    tab.TitleContent.Invoke();
                }
                finally
                {
                    UIContext.Current = parentContext;
                }
                contentBuilder.BuildChildren();
            }
            else if (!string.IsNullOrEmpty(tab.Title))
            {
                TMPro.TextMeshProUGUI text = contentContainer.AddComponent<TMPro.TextMeshProUGUI>();
                text.text = tab.Title;
                text.alignment = TMPro.TextAlignmentOptions.Center;
                text.fontSize = 16;
                text.color = Color.white;

                TMPro.TMP_FontAsset resolvedFont = ResolveFont(fontAsset);
                if (resolvedFont != null)
                {
                    text.font = resolvedFont;
                }
            }

            LayoutElement buttonLayout = tabButton.AddComponent<LayoutElement>();
            buttonLayout.minHeight = 60;
            buttonLayout.preferredHeight = 60;
            buttonLayout.flexibleWidth = 1;
            buttonLayout.flexibleHeight = 1;
        }

        private void UpdateTabButtons(Transform tabBarParent)
        {
            if (tabBarParent == null)
                return;

            for (int i = 0; i < tabBarParent.childCount; i++)
            {
                Transform tabChild = tabBarParent.GetChild(i);
                Image buttonBg = tabChild.GetComponent<Image>();
                if (buttonBg != null)
                {
                    buttonBg.color = i == selectedIndex.Value ? activeTabColor : inactiveTabColor;
                }
            }
        }

        private void ConfigureLayout(GameObject container)
        {
            bool isDirectChildOfCanvas = container.transform.parent.GetComponent<Canvas>() != null;

            if (isDirectChildOfCanvas)
            {
                RectTransform rectTransform = container.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            else
            {
                LayoutElement layoutElement = LayoutElementUtility.Configure(
                    container,
                    preferredWidth,
                    preferredHeight,
                    infiniteWidth,
                    infiniteHeight,
                    300f,
                    240f);

                if (preferredWidth < 0 && !infiniteWidth)
                    layoutElement.flexibleWidth = 1;
                if (preferredHeight < 0 && !infiniteHeight)
                    layoutElement.flexibleHeight = 1;
            }
        }

        private void ClampSelectedIndex()
        {
            if (tabs.Count == 0)
                return;

            int clamped = Mathf.Clamp(selectedIndex.Value, 0, tabs.Count - 1);
            if (clamped != selectedIndex.Value)
                selectedIndex.Value = clamped;
        }
    }

    /// <summary>Invokes a callback after a delay on the Unity player loop.</summary>
    public class DelayedCallback : MonoBehaviour
    {
        private float delay;
        private Action callback;
        private float elapsed = 0f;

        /// <summary>Schedules <paramref name="callback"/> to run after <paramref name="delay"/> seconds.</summary>
        public void Initialize(float delay, Action callback)
        {
            this.delay = delay;
            this.callback = callback;
        }

        /// <summary>Cancels the pending callback so it does not fire on destroy.</summary>
        public void Cancel()
        {
            callback = null;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= delay)
            {
                if (callback != null)
                {
                    var pendingCallback = callback;
                    callback = null;
                    pendingCallback.Invoke();
                }
                Destroy(this);
            }
        }
    }
}
