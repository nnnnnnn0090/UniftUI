using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Horizontal stack layout.</summary>
    public class HStackElement : UIElement, ILayoutContainer
    {
        private Action content;
        private State[] states;
        private List<UIElement> children = new List<UIElement>();
        private float spacing;
        private HStackAlignment alignment;

        public HStackElement(Action content, State[] states = null, float spacing = 8f, HStackAlignment alignment = HStackAlignment.Center)
        {
            this.content = content;
            this.states = states;
            this.spacing = spacing;
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

        public void AddChild(UIElement child)
        {
            if (child == null)
                return;

            children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
            {
                children.Remove(child);
            }
        }

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
                Debug.LogWarning($"[UniftUI] HStack ReplaceChild: oldChild not found. Children count: {children.Count}");
            }
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject container = new GameObject("HStack");
            container.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = container.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            UniftUIStackLayoutGroup layout = container.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Horizontal, spacing, VStackAlignment.Center, alignment);

            LayoutElementUtility.Configure(container, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            foreach (var child in children)
            {
                ApplyInheritedFont(child);
                child.Build(container.transform);
            }

            if (states != null && states.Length > 0)
            {
                SetupStateObserver(container);
            }

            ApplyAllEffects(container, backgroundImage);

            BaselineRowAligner.AlignIfNeeded(container, alignment);

            return container;
        }

        private void SetupStateObserver(GameObject container)
        {
            if (container == null) return;

            StateObserver observer = container.AddComponent<StateObserver>();
            observer.Initialize(states, () => {
                if (container == null || !container)
                {
                    Debug.LogWarning("[UniftUI] HStack rebuild skipped: container was destroyed.");
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

                    BaselineRowAligner.AlignIfNeeded(container, alignment);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniftUI] HStack rebuild error: {e.Message}\n{e.StackTrace}");
                }
            });
        }
    }
}
