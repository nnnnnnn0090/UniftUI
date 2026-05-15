using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    public class ScrollViewElement : UIElement, ILayoutContainer
    {
        private Action content;
        private State[] states;
        private List<UIElement> children = new List<UIElement>();
        private bool horizontal;
        private bool vertical;
        private ScrollIndicatorVisibility verticalIndicatorVisibility = ScrollIndicatorVisibility.Automatic;
        private ScrollIndicatorVisibility horizontalIndicatorVisibility = ScrollIndicatorVisibility.Automatic;

        private ScrollRect.MovementType movementType = ScrollRect.MovementType.Elastic;
        private float scrollSensitivity = 10f;
        private State<float> bindVerticalNormalized;
        private State<float> bindHorizontalNormalized;
        private bool twoWayVertical;
        private bool twoWayHorizontal;
        
        // スクロールビューの設定
        public ScrollViewElement(Action content, State[] states = null, bool horizontal = false, bool vertical = true)
        {
            this.content = content;
            this.states = states;
            this.horizontal = horizontal;
            this.vertical = vertical;
            
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
        
        /// <summary>
        /// 従来 API。各軸で <c>true</c> は <see cref="ScrollIndicatorVisibility.Automatic"/>、
        /// <c>false</c> は <see cref="ScrollIndicatorVisibility.Hidden"/> に相当します（常時表示は <see cref="WithScrollIndicators"/> を使用）。
        /// </summary>
        public ScrollViewElement ShowScrollbars(bool horizontal = true, bool vertical = true)
        {
            if (this.vertical)
                verticalIndicatorVisibility = vertical ? ScrollIndicatorVisibility.Automatic : ScrollIndicatorVisibility.Hidden;
            if (this.horizontal)
                horizontalIndicatorVisibility = horizontal ? ScrollIndicatorVisibility.Automatic : ScrollIndicatorVisibility.Hidden;
            return this;
        }

        /// <summary>
        /// SwiftUI の <c>.scrollIndicators(_:axes:)</c> に相当。指定軸のみ更新します（他軸はそのまま）。
        /// </summary>
        /// <param name="axes"><see cref="UniftUIScrollAxis.Vertical"/> / <see cref="UniftUIScrollAxis.Horizontal"/> のビット和。</param>
        public ScrollViewElement WithScrollIndicators(ScrollIndicatorVisibility visibility, UniftUIScrollAxis axes)
        {
            if (vertical && (axes & UniftUIScrollAxis.Vertical) != 0)
                verticalIndicatorVisibility = visibility;
            if (horizontal && (axes & UniftUIScrollAxis.Horizontal) != 0)
                horizontalIndicatorVisibility = visibility;
            return this;
        }

        /// <summary>
        /// 有効なスクロール軸すべてに <paramref name="visibility"/> を適用します（縦のみなら縦だけ、両方なら両方）。
        /// </summary>
        public ScrollViewElement WithScrollIndicators(ScrollIndicatorVisibility visibility)
        {
            if (vertical)
                verticalIndicatorVisibility = visibility;
            if (horizontal)
                horizontalIndicatorVisibility = visibility;
            return this;
        }

        /// <summary>SwiftUI の <c>.scrollBounceBehavior</c> に近い弾性（<see cref="ScrollRect.movementType"/>）。</summary>
        public ScrollViewElement WithScrollBounce(bool elastic)
        {
            movementType = elastic ? ScrollRect.MovementType.Elastic : ScrollRect.MovementType.Clamped;
            return this;
        }

        /// <summary>Unity <see cref="ScrollRect.movementType"/> を直接指定。</summary>
        public ScrollViewElement WithMovementType(ScrollRect.MovementType type)
        {
            movementType = type;
            return this;
        }

        /// <summary>ホイール／トラックパッドの感度（<see cref="ScrollRect.scrollSensitivity"/>）。</summary>
        public ScrollViewElement WithScrollSensitivity(float sensitivity)
        {
            scrollSensitivity = Mathf.Max(0.01f, sensitivity);
            return this;
        }

        /// <summary>
        /// SwiftUI の <c>.scrollPosition($y)</c> に相当する縦バインド。
        /// 値は <see cref="ScrollRect.verticalNormalizedPosition"/>（<b>1=先頭、0=末尾</b>）。
        /// </summary>
        public ScrollViewElement BindScrollPositionY(State<float> normalized, bool twoWay = false)
        {
            bindVerticalNormalized = normalized;
            twoWayVertical |= twoWay;
            return this;
        }

        /// <summary>水平バインド（<b>0=左、1=右</b>）。</summary>
        public ScrollViewElement BindScrollPositionX(State<float> normalized, bool twoWay = false)
        {
            bindHorizontalNormalized = normalized;
            twoWayHorizontal |= twoWay;
            return this;
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
                Debug.LogWarning($"ReplaceChild: oldChild not found in ScrollView. Old: {oldChild}, New: {newChild}. Children count: {children.Count}");
            }
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public override GameObject Build(Transform parent)
        {
            // メインのコンテナを作成
            GameObject container = new GameObject("ScrollView");
            container.transform.SetParent(parent, false);
            
            // 背景色の設定
            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = container.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }
            
            // ScrollRectコンポーネントを追加
            ScrollRect scrollRect = container.AddComponent<ScrollRect>();
            scrollRect.horizontal = horizontal;
            scrollRect.vertical = vertical;
            scrollRect.movementType = movementType;
            scrollRect.scrollSensitivity = scrollSensitivity;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.135f; // Unityのデフォルト値
            
            // コンテンツ用のコンテナを作成
            GameObject contentContainer = new GameObject("Content");
            contentContainer.transform.SetParent(container.transform, false);
            
            RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1); // 左上を基準点に
            contentRect.anchorMax = new Vector2(1, 1); // 右上を基準点に
            contentRect.pivot = new Vector2(0.5f, 1); // コンテンツの上部中央を原点に
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero; // ★ この行を追加: sizeDeltaを初期化
            
            scrollRect.content = contentRect;
            
            // レイアウトグループの設定（垂直または水平）
            if (vertical)
            {
                VerticalLayoutGroup layoutGroup = contentContainer.AddComponent<VerticalLayoutGroup>();
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.spacing = 8f;
                layoutGroup.padding = this.padding ?? new RectOffset(10, 10, 10, 10);
            }
            else if (horizontal)
            {
                HorizontalLayoutGroup layoutGroup = contentContainer.AddComponent<HorizontalLayoutGroup>();
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
                layoutGroup.childForceExpandWidth = false;
                layoutGroup.childForceExpandHeight = true;
                layoutGroup.spacing = 8f;
                layoutGroup.padding = this.padding ?? new RectOffset(10, 10, 10, 10);
            }
            
            // コンテンツサイズフィッター（コンテンツサイズを適切に計算するため）
            ContentSizeFitter contentFitter = contentContainer.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = horizontal ? 
                ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = vertical ? 
                ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
            
            // RectMask2D を使用（Maskはステンシルマテリアルキャッシュにより動的マテリアル更新が反映されないため）
            container.AddComponent<RectMask2D>();
            
            // 垂直スクロールインジケータ（Hidden 以外で Scrollbar を生成）
            if (vertical && verticalIndicatorVisibility != ScrollIndicatorVisibility.Hidden)
            {
                GameObject verticalScrollbar = CreateVerticalScrollbar(container);
                scrollRect.verticalScrollbar = verticalScrollbar.GetComponent<Scrollbar>();
                scrollRect.verticalScrollbarVisibility = verticalIndicatorVisibility == ScrollIndicatorVisibility.Visible
                    ? ScrollRect.ScrollbarVisibility.Permanent
                    : ScrollRect.ScrollbarVisibility.AutoHide;
            }

            // 水平スクロールインジケータ
            if (horizontal && horizontalIndicatorVisibility != ScrollIndicatorVisibility.Hidden)
            {
                GameObject horizontalScrollbar = CreateHorizontalScrollbar(container);
                scrollRect.horizontalScrollbar = horizontalScrollbar.GetComponent<Scrollbar>();
                scrollRect.horizontalScrollbarVisibility = horizontalIndicatorVisibility == ScrollIndicatorVisibility.Visible
                    ? ScrollRect.ScrollbarVisibility.Permanent
                    : ScrollRect.ScrollbarVisibility.AutoHide;
            }
            
            // ScrollViewのサイズ設定
            LayoutElement layoutElement = container.AddComponent<LayoutElement>();
            if (preferredWidth > 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
            }
            else if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
            }
            else
            {
                layoutElement.preferredWidth = 300; // デフォルトの幅
                layoutElement.minWidth = 100;
                layoutElement.flexibleWidth = 1;
            }
            
            if (preferredHeight > 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
                layoutElement.flexibleHeight = 0;
            }
            else if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
            }
            else
            {
                layoutElement.preferredHeight = 200; // デフォルトの高さ
                layoutElement.minHeight = 100;
                layoutElement.flexibleHeight = 1;
            }
            
            // 子要素の構築
            foreach (var child in children)
            {
                child.Build(contentContainer.transform);
            }
            
            ApplyAllEffects(container, backgroundImage);

            if (bindVerticalNormalized != null || bindHorizontalNormalized != null)
            {
                var bridge = container.AddComponent<UniftUIScrollRectBridge>();
                bridge.Initialize(scrollRect, bindVerticalNormalized, bindHorizontalNormalized, twoWayVertical, twoWayHorizontal);
            }
            
            // 状態の変更を監視
            if (states != null && states.Length > 0)
            {
                SetupStateObserver(container, contentContainer);
            }
            
            return container;
        }
        
        // 垂直スクロールバーの作成
        private GameObject CreateVerticalScrollbar(GameObject parent)
        {
            GameObject scrollbar = new GameObject("VerticalScrollbar");
            scrollbar.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = scrollbar.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(1, 0);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 0.5f);
            rectTransform.sizeDelta = new Vector2(10, 0); // 幅10px
            
            Image scrollbarImage = scrollbar.AddComponent<Image>();
            scrollbarImage.color = new Color(0.7f, 0.7f, 0.7f, 0.7f); // グレーの背景
            
            Scrollbar scrollbarComp = scrollbar.AddComponent<Scrollbar>();
            scrollbarComp.direction = Scrollbar.Direction.BottomToTop;
            
            // スライディングエリア
            GameObject slidingArea = new GameObject("SlidingArea");
            slidingArea.transform.SetParent(scrollbar.transform, false);
            
            RectTransform slidingRect = slidingArea.AddComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.sizeDelta = Vector2.zero;
            
            // ハンドル
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(slidingArea.transform, false);
            
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.sizeDelta = Vector2.zero;
            
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.4f, 0.4f, 0.4f, 0.7f); // ダークグレーのハンドル
            
            scrollbarComp.handleRect = handleRect;
            scrollbarComp.targetGraphic = handleImage;
            
            return scrollbar;
        }
        
        // 水平スクロールバーの作成
        private GameObject CreateHorizontalScrollbar(GameObject parent)
        {
            GameObject scrollbar = new GameObject("HorizontalScrollbar");
            scrollbar.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = scrollbar.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(1, 0);
            rectTransform.pivot = new Vector2(0.5f, 0);
            rectTransform.sizeDelta = new Vector2(0, 10); // 高さ10px
            
            Image scrollbarImage = scrollbar.AddComponent<Image>();
            scrollbarImage.color = new Color(0.7f, 0.7f, 0.7f, 0.7f); // グレーの背景
            
            Scrollbar scrollbarComp = scrollbar.AddComponent<Scrollbar>();
            scrollbarComp.direction = Scrollbar.Direction.LeftToRight;
            
            // スライディングエリア
            GameObject slidingArea = new GameObject("SlidingArea");
            slidingArea.transform.SetParent(scrollbar.transform, false);
            
            RectTransform slidingRect = slidingArea.AddComponent<RectTransform>();
            slidingRect.anchorMin = Vector2.zero;
            slidingRect.anchorMax = Vector2.one;
            slidingRect.sizeDelta = Vector2.zero;
            
            // ハンドル
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(slidingArea.transform, false);
            
            RectTransform handleRect = handle.AddComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.sizeDelta = Vector2.zero;
            
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.4f, 0.4f, 0.4f, 0.7f); // ダークグレーのハンドル
            
            scrollbarComp.handleRect = handleRect;
            scrollbarComp.targetGraphic = handleImage;
            
            return scrollbar;
        }
        
        // 状態監視の設定
        private void SetupStateObserver(GameObject container, GameObject contentContainer)
        {
            StateObserver observer = container.AddComponent<StateObserver>();
            observer.Initialize(states, () => {
                // 既存の子要素をクリア
                foreach (Transform child in contentContainer.transform)
                {
                    if (child.gameObject != null)
                        GameObject.Destroy(child.gameObject);
                }
                
                children.Clear();
                
                // 親コンテキストを一時保存
                var parentContext = UIContext.Current;
                UIContext.Current = this;
                
                // コンテンツを再構築
                if (content != null)
                    content.Invoke();
                
                // 親コンテキストを復元
                UIContext.Current = parentContext;
                
                // 新しい子要素を構築（VStack/HStack と同様、State 再生成後もルートフォントを伝播）
                foreach (var child in children)
                {
                    if (child == null || contentContainer == null || contentContainer.transform == null)
                        continue;
                    if (UIContext.DefaultFont != null)
                        child.Font(UIContext.DefaultFont);
                    child.Build(contentContainer.transform);
                }
                
                // レイアウトを更新
                Canvas.ForceUpdateCanvases();
            });
        }
    }
}
