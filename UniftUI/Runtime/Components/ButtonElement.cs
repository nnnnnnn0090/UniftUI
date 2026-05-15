using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Collections.Generic;

namespace UniftUI
{
    public class ButtonElement : UIElement, ILayoutContainer
    {
        private string label;
        private UIElement customContent; // カスタムUIコンテンツ用
        private Action onClick;
        private Color textColor = Color.black;
        private TMP_FontAsset fontAsset = null;
        private bool hasCustomContent = false; // カスタムコンテンツフラグ
        private List<UIElement> children = new List<UIElement>(); // ILayoutContainer実装用

        // 既存のコンストラクタ (文字列ラベル用)
        public ButtonElement(string label, Action onClick)
        {
            this.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            this.label = label;
            this.onClick = onClick;
            this.hasCustomContent = false;
            UIContext.Add(this);
        }

        // 新しいコンストラクタ (カスタムコンテンツ用)
        public ButtonElement(UIElement content, Action onClick)
        {
            this.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            this.customContent = content;
            this.onClick = onClick;
            this.hasCustomContent = true;
            
            // コンテンツを子要素として追加
            if (content != null)
            {
                AddChild(content);
            }
            
            UIContext.Add(this);
        }
        
        // ILayoutContainer実装
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
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }
        
        public ButtonElement SetBackgroundColor(Color color)
        {
            backgroundColor = color;
            return this;
        }
        
        public ButtonElement SetTextColor(Color color)
        {
            textColor = color;
            return this;
        }

        public ButtonElement SetFont(TMP_FontAsset font)
        {
            fontAsset = font;
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject buttonObj = new GameObject("Button");
            buttonObj.transform.SetParent(parent, false);

            Image image = buttonObj.AddComponent<Image>();
            image.color = backgroundColor;
            
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            
            ColorBlock colors = button.colors;
            colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
            button.colors = colors;

            LayoutElement layoutElement = buttonObj.AddComponent<LayoutElement>();
            ContentSizeFitter buttonFitter = buttonObj.AddComponent<ContentSizeFitter>();
            // カスタム時も単一子を配置するため VLGroup を付与（ルート CSF とは別コンポーネント）。
            buttonObj.AddComponent<VerticalLayoutGroup>();
            
            if (hasCustomContent) 
            {
                GameObject contentContainer = new GameObject("Content");
                contentContainer.transform.SetParent(buttonObj.transform, false);
                
                ContentSizeFitter contentFitter = contentContainer.AddComponent<ContentSizeFitter>();
                contentFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                
                VerticalLayoutGroup layoutGroup = contentContainer.AddComponent<VerticalLayoutGroup>();
                layoutGroup.childAlignment = TextAnchor.MiddleCenter;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = true;
                layoutGroup.padding = new RectOffset(5, 5, 5, 5);
                
                RectTransform contentRect = contentContainer.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.offsetMin = Vector2.zero;
                contentRect.offsetMax = Vector2.zero;
                
                // カスタムコンテンツをビルド
                foreach (var child in children)
                {
                    child.Build(contentContainer.transform);
                }
            }
            else
            {
                // 通常のテキストラベルの作成
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);

                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0, 0);
                textRect.anchorMax = new Vector2(1, 1);
                textRect.offsetMin = new Vector2(10, 5);
                textRect.offsetMax = new Vector2(-10, -5);

                // TMPテキストコンポーネントの設定
                TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
                textComponent.text = label;
                textComponent.alignment = TextAlignmentOptions.Center;
                textComponent.fontSize = 18;
                textComponent.color = textColor;
                textComponent.margin = new Vector4(5, 5, 5, 5);
                
                if (fontAsset != null)
                {
                    textComponent.font = fontAsset;
                }
            }
            
            if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
                layoutElement.preferredWidth = -1;
                layoutElement.minWidth = 0;
                buttonFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredWidth >= 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
                buttonFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize; 
            }
            else
            {
                layoutElement.preferredWidth = -1; 
                layoutElement.flexibleWidth = 0;   
                buttonFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            
            if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
                layoutElement.preferredHeight = -1;
                layoutElement.minHeight = 0;
                buttonFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
                layoutElement.flexibleHeight = 0;
                buttonFitter.verticalFit = ContentSizeFitter.FitMode.MinSize;
            }
            else
            {
                layoutElement.preferredHeight = -1; 
                layoutElement.flexibleHeight = 0;   
                buttonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            
            // onClick を直接保持。ButtonActionManager / ActionManager 不要。
            var capturedClick = onClick;
            button.onClick.AddListener(() => {
                try { capturedClick?.Invoke(); }
                catch (Exception e) { Debug.LogError($"[UniftUI] Button onClick error: {e.Message}"); }
            });
            
            ApplyAllEffects(buttonObj, image);
            
            return buttonObj;
        }
        
    }
}
