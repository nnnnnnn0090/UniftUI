using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UniftUI
{
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
                 Debug.LogWarning($"ReplaceChild: oldChild not found in HStack. Old: {oldChild}, New: {newChild}. Children count: {children.Count}");
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

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            ContentSizeFitter buttonFitter = container.AddComponent<ContentSizeFitter>();
            buttonFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            buttonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            switch (alignment)
            {
                case HStackAlignment.Top:
                    layout.childAlignment = TextAnchor.UpperCenter;
                    break;
                case HStackAlignment.Center:
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    break;
                case HStackAlignment.Bottom:
                    layout.childAlignment = TextAnchor.LowerCenter;
                    break;
                case HStackAlignment.FirstTextBaseline:
                case HStackAlignment.LastTextBaseline:
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    break;
            }
            
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(0,0,0,0);

            LayoutElement layoutElement = container.AddComponent<LayoutElement>();

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
                    Debug.LogWarning("HStack: コンテナが既に破棄されています");
                    return;
                }
                
                try
                {
                    foreach (Transform child in container.transform)
                    {
                        if (child != null && child.gameObject != null)
                            UnityEngine.Object.Destroy(child.gameObject);
                    }
                    
                    children.Clear();
                    
                    var parentContext = UIContext.Current;
                    UIContext.Current = this;
                    
                    if (content != null)
                        content.Invoke();
                    
                    UIContext.Current = parentContext;
                    
                    foreach (var child in children)
                    {
                        if (child == null) continue;
                        
                        if (UIContext.DefaultFont != null)
                            child.Font(UIContext.DefaultFont);
                        
                        child.Build(container.transform);
                    }
                    
                    Canvas.ForceUpdateCanvases();

                    BaselineRowAligner.AlignIfNeeded(container, alignment);
                }
                catch (Exception e)
                {
                    Debug.LogError($"HStack: UIの再構築中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
                }
            });
        }
    }
}
