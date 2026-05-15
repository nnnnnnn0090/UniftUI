using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

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

            VerticalLayoutGroup mainLayout = container.AddComponent<VerticalLayoutGroup>();
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;
            mainLayout.spacing = 0;
            mainLayout.padding = new RectOffset(0, 0, 0, 0);

            GameObject contentArea = CreateContentArea(container.transform);

            GameObject tabBar = CreateTabBar(container.transform);

            ApplyAllEffects(container, background);

            ConfigureLayout(container);

            return container;
        }

        private GameObject CreateContentArea(Transform parent)
        {
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(parent, false);

            LayoutElement contentLayout = contentArea.AddComponent<LayoutElement>();
            contentLayout.flexibleHeight = 1;
            contentLayout.flexibleWidth = 1;

            if (tabs.Count > 0 && selectedIndex.Value < tabs.Count)
            {
                currentContentObj = CreateTabContent(contentArea.transform, tabs[selectedIndex.Value]);
            }

            StateObserver observer = contentArea.AddComponent<StateObserver>();
            observer.Initialize(new State[] { selectedIndex }, () => {
                if (tabs.Count > 0 && selectedIndex.Value < tabs.Count)
                {
                    SwitchTabWithAnimation(contentArea.transform, tabs[selectedIndex.Value]);
                }
            });

            return contentArea;
        }

        private void SwitchTabWithAnimation(Transform contentParent, TabItem newTab)
        {
            CancelCurrentAnimation();

            if (currentContentObj != null)
            {
                pendingContentToDestroy = currentContentObj;

                UIAnimator.Fade(pendingContentToDestroy, 1.0f, 0.0f, transitionDuration * 0.5f, null);
            }

            GameObject newContent = CreateTabContent(contentParent, newTab);

            CanvasGroup canvasGroup = newContent.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = newContent.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0;

            currentContentObj = newContent;

            currentAnimation = contentParent.gameObject.AddComponent<DelayedCallback>();
            currentAnimation.Initialize(transitionDuration * 0.2f, () => {
                if (pendingContentToDestroy != null)
                {
                    GameObject.Destroy(pendingContentToDestroy);
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
                GameObject.Destroy(currentAnimation);
                currentAnimation = null;
            }

            if (pendingContentToDestroy != null)
            {
                GameObject.Destroy(pendingContentToDestroy);
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

            var layoutGroup = tabRootContentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.padding = new RectOffset(0,0,0,0);

            var tabContentBuilder = new TabContentContainer(tabRootContentObj.transform);

            TMPro.TMP_FontAsset resolvedFont = fontAsset ?? UIContext.DefaultFont;
            if (resolvedFont != null)
            {
                tabContentBuilder.SetFont(resolvedFont);
                UIContext.SetDefaultFont(resolvedFont);
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
            tabBarLayout.preferredHeight = 60;
            tabBarLayout.flexibleHeight = 0;

            HorizontalLayoutGroup tabLayout = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;
            tabLayout.spacing = 0;
            tabLayout.padding = new RectOffset(0, 0, 0, 0);

            for (int i = 0; i < tabs.Count; i++)
            {
                CreateTabButton(tabBar.transform, tabs[i], i);
            }

            StateObserver observer = tabBar.AddComponent<StateObserver>();
            observer.Initialize(new State[] { selectedIndex }, () => {
                UpdateTabButtons(tabBar.transform);
            });

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
                selectedIndex.Value = index;
                UpdateTabButtons(parent);
            });

            GameObject contentContainer = new GameObject("TabHeaderContent");
            contentContainer.transform.SetParent(tabButton.transform, false);

            RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            if (tab.TitleContent != null)
            {
                var contentBuilder = new TabContentContainer(contentContainer.transform);
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

                if (fontAsset != null)
                {
                    text.font = fontAsset;
                }
            }

            LayoutElement buttonLayout = tabButton.AddComponent<LayoutElement>();
            buttonLayout.flexibleWidth = 1;
        }

        private void UpdateTabButtons(Transform tabBarParent)
        {
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
                LayoutElement layoutElement = container.AddComponent<LayoutElement>();
                layoutElement.flexibleWidth = 1;
                layoutElement.flexibleHeight = 1;
            }
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

        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= delay)
            {
                if (callback != null)
                {
                    var tempCallback = callback;
                    callback = null;
                    tempCallback.Invoke();
                }
                Destroy(this);
            }
        }

        private void OnDestroy()
        {
            if (callback != null)
            {
                try
                {
                    var tempCallback = callback;
                    callback = null;
                    tempCallback.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniftUI] DelayedCallback: error during callback execution on destroy: {e.Message}");
                }
            }
        }
    }
}
