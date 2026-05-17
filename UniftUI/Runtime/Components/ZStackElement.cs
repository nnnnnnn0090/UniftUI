using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

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

            UniftUIZStackLayoutGroup layout = container.AddComponent<UniftUIZStackLayoutGroup>();
            layout.Configure(alignment, padding ?? new RectOffset(0, 0, 0, 0));

            LayoutElementUtility.Configure(container, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            foreach (var child in children)
            {
                if (child == null) continue;
                ApplyInheritedFont(child);
                child.Build(container.transform);
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
                            DestroyGameObject(child.gameObject);
                    }

                    children.Clear();

                    var parentContext = UIContext.Current;
                    try
                    {
                        UIContext.Current = this;
                        content?.Invoke();
                    }
                    finally
                    {
                        UIContext.Current = parentContext;
                    }

                    foreach (var child in children)
                    {
                        if (child == null) continue;

                        ApplyInheritedFont(child);
                        child.Build(container.transform);
                    }

                    LayoutRebuilder.ForceRebuildLayoutImmediate(container.GetComponent<RectTransform>());
                    Canvas.ForceUpdateCanvases();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniftUI] ZStack: error while rebuilding UI: {e.Message}\n{e.StackTrace}");
                }
            });
        }

        protected override void PropagateInfiniteWidthToContent()
        {
        }

        protected override void PropagateInfiniteHeightToContent()
        {
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
