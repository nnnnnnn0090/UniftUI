using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

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

            UniftUIStackLayoutGroup layout = container.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Vertical, VerticalSpacing, VStackAlignment.Center, HStackAlignment.Center);

            LayoutElementUtility.Configure(container, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            foreach (var child in children)
            {
                ApplyInheritedFont(child);
                child.Build(container.transform);
            }

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
                            DestroyGameObject(child.gameObject);

                    localChildren.Clear();

                    var parentContext = UIContext.Current;
                    try
                    {
                        UIContext.Current = this;
                        localContent?.Invoke();
                    }
                    finally
                    {
                        UIContext.Current = parentContext;
                    }

                    foreach (var child in localChildren)
                    {
                        if (child == null || container.transform == null) continue;
                        ApplyInheritedFont(child);
                        child.Build(container.transform);
                    }

                    LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());

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
