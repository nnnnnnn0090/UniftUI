using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniftUI
{
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
                tabContentContainer.BuildChildren(); // Build collected children
            }
        }

        public override GameObject Build(Transform parent)
        {
            // TabItemは直接UIを構築しない
            // ここでOnAppearとUpdateコールバックを処理する必要はない
            // これらは実際のコンテンツが構築されるときに処理される
            return null;
        }
    }

    // タブコンテンツ用のコンテナクラス
    internal class TabContentContainer : ILayoutContainer
    {
        private Transform parentTransform;
        private List<UIElement> children = new List<UIElement>();
        private TMPro.TMP_FontAsset fontAsset = null;

        public TabContentContainer(Transform parentTransform)
        {
            this.parentTransform = parentTransform;
        }

        // フォント設定用のメソッドを追加
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
                Debug.LogWarning($"ReplaceChild: oldChild not found in TabContentContainer. Old: {oldChild}, New: {newChild}. Children count: {children.Count}");
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
                    // 子要素をビルドする前にフォント設定を適用
                    if (fontAsset != null)
                    {
                        child.Font(fontAsset);
                    }
                    
                    // 子要素をビルドし、OnAppearとUpdateコールバックは
                    // 各子要素自身のBuildメソッドで処理される
                    child.Build(parentTransform);
                }
            }
        }
    }
}
