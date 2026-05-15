using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>Defines one tab's title and body content for <see cref="TabView"/>.</summary>
    public class TabItem : UIElement
    {
        public string Title { get; private set; }
        public Action TitleContent { get; private set; }
        public Action Content { get; private set; }

        public TabItem(Action titleContent, Action content)
        {
            TitleContent = titleContent;
            Content = content;
            UIContext.Add(this);
        }

        public TabItem(string title, Action content)
        {
            Title = title;
            Content = content;
            UIContext.Add(this);
        }

        /// <summary>Builds this tab's body inside the given parent transform.</summary>
        public void BuildContent(Transform parent)
        {
            if (Content != null)
            {
                GameObject contentContainer = new GameObject($"Content_{Title}");
                contentContainer.transform.SetParent(parent, false);

                RectTransform rect = contentContainer.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;

                var tabContentContainer = new TabContentContainer(contentContainer.transform);
                var parentContext = UIContext.Current;
                try
                {
                    UIContext.Current = tabContentContainer;
                    Content.Invoke();
                }
                finally
                {
                    UIContext.Current = parentContext;
                }
                tabContentContainer.BuildChildren();
            }
        }

        public override GameObject Build(Transform parent)
        {
            return null;
        }
    }

    internal class TabContentContainer : ILayoutContainer
    {
        private Transform parentTransform;
        private List<UIElement> children = new List<UIElement>();
        private TMPro.TMP_FontAsset fontAsset = null;

        public TabContentContainer(Transform parentTransform)
        {
            this.parentTransform = parentTransform;
        }

        public void SetFont(TMPro.TMP_FontAsset font)
        {
            this.fontAsset = font;
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
            {
                children.Add(child);
            }
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
                Debug.LogWarning($"[UniftUI] TabContentContainer ReplaceChild: oldChild not found. Children count: {children.Count}");
            }
        }

        public System.Collections.Generic.IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public void BuildChildren()
        {
            foreach (var child in children)
            {
                if (child != null)
                {
                    if (fontAsset != null)
                    {
                        child.Font(fontAsset);
                    }

                    child.Build(parentTransform);
                }
            }
        }
    }
}
