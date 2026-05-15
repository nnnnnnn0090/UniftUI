using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    public class VStackElement : UIElement, ILayoutContainer
    {
        private Action content;
        private State[] states;
        private List<UIElement> children = new List<UIElement>();
        private float spacing;
        private VStackAlignment alignment;

        public VStackElement(Action content, State[] states = null, float spacing = 8f, 
                     VStackAlignment alignment = VStackAlignment.Center)
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
                children[index] = newChild;
            else
                 Debug.LogWarning($"ReplaceChild: oldChild not found in VStack. Old: {oldChild}, New: {newChild}. Children count: {children.Count}");
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject container = new GameObject("VStack");
            container.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = container.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            
            // SwiftUI の VStack と同様、主軸は上から下。余白があるとき子ブロック全体を Middle* にすると
            // Text + Spacer でも中央に寄ってしまうため、横方向のみ Leading/Center/Trailing を反映し縦は上端を基準にする。
            switch (alignment)
            {
                case VStackAlignment.Leading:
                    layout.childAlignment = TextAnchor.UpperLeft;
                    break;
                case VStackAlignment.Center:
                    layout.childAlignment = TextAnchor.UpperCenter;
                    break;
                case VStackAlignment.Trailing:
                    layout.childAlignment = TextAnchor.UpperRight;
                    break;
            }
            
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(0,0,0,0);

            LayoutElement layoutElement = container.AddComponent<LayoutElement>();
            ContentSizeFitter buttonFitter = container.AddComponent<ContentSizeFitter>();
            buttonFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
                buttonFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredWidth >= 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
            }

            if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
                buttonFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
            }

            foreach (var child in children)
            {
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
            
            var localContent = content;
            var localChildren = children;
            
            observer.Initialize(states, () => {
                if (container == null || !container)
                {
                    Debug.LogWarning("VStack: コンテナが既に破棄されています");
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
                    
                    if (localContent != null)
                        localContent.Invoke();
                    
                    UIContext.Current = parentContext;
                    
                    foreach (var child in localChildren)
                    {
                        if (child != null && container != null && container.transform != null)
                        {
                            if (UIContext.DefaultFont != null)
                                child.Font(UIContext.DefaultFont);
                            
                            child.Build(container.transform);
                        }
                    }
                    
                    if (container != null)
                        Canvas.ForceUpdateCanvases();
                }
                catch (Exception e)
                {
                    Debug.LogError($"VStack: UIの再構築中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
                }
            });
        }
    }
}
