using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// Two-dimensional grid layout. Children should be <see cref="GridRowElement"/> instances.
    /// Column widths are synchronized after build via <see cref="GridColumnSynchronizer"/>.
    /// </summary>
    public class GridElement : UIElement, ILayoutContainer
    {
        private readonly Action content;
        private readonly State[] states;
        private readonly List<UIElement> children = new List<UIElement>();

        public float RowHorizontalSpacing { get; private set; }
        public float VerticalSpacing { get; private set; }
        public HStackAlignment RowAlignment { get; private set; }

        public GridElement(Action content, State[] states, float horizontalSpacing, float verticalSpacing,
            HStackAlignment rowAlignment)
        {
            this.content = content;
            this.states = states;
            RowHorizontalSpacing = horizontalSpacing;
            VerticalSpacing = verticalSpacing;
            RowAlignment = rowAlignment;

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

        public void AddChild(UIElement child)
        {
            if (child != null)
                children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
                children.Remove(child);
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;
            int index = children.IndexOf(oldChild);
            if (index != -1)
                children[index] = newChild;
            else
                Debug.LogWarning($"[UniftUI] Grid ReplaceChild: oldChild not found. Children count: {children.Count}");
        }

        public IEnumerable<UIElement> GetChildren() => children;

        public override GameObject Build(Transform parent)
        {
            GameObject container = new GameObject("Grid");
            container.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = container.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = VerticalSpacing;
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);

            LayoutElement layoutElement = container.AddComponent<LayoutElement>();
            ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredWidth >= 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
            }

            if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
            }

            foreach (var child in children)
                child.Build(container.transform);

            if (states != null && states.Length > 0)
                SetupStateObserver(container);

            ApplyAllEffects(container, backgroundImage);

            RectTransform gridRt = container.GetComponent<RectTransform>();
            if (gridRt != null)
                GridColumnSynchronizer.Apply(gridRt);

            return container;
        }

        private void SetupStateObserver(GameObject container)
        {
            if (container == null) return;

            var localContent = content;
            var localChildren = children;

            StateObserver observer = container.AddComponent<StateObserver>();
            observer.Initialize(states, () =>
            {
                if (container == null || !container)
                {
                    Debug.LogWarning("[UniftUI] Grid rebuild skipped: container was destroyed.");
                    return;
                }

                try
                {
                    foreach (Transform child in container.transform)
                        if (child != null && child.gameObject != null)
                            UnityEngine.Object.Destroy(child.gameObject);

                    localChildren.Clear();

                    var parentContext = UIContext.Current;
                    UIContext.Current = this;
                    localContent?.Invoke();
                    UIContext.Current = parentContext;

                    foreach (var child in localChildren)
                    {
                        if (child == null || container.transform == null) continue;
                        if (UIContext.DefaultFont != null)
                            child.Font(UIContext.DefaultFont);
                        child.Build(container.transform);
                    }

                    RectTransform syncRt = container.GetComponent<RectTransform>();
                    if (syncRt != null)
                        GridColumnSynchronizer.Apply(syncRt);

                    Canvas.ForceUpdateCanvases();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniftUI] Grid rebuild error: {e.Message}\n{e.StackTrace}");
                }
            });
        }
    }
}
