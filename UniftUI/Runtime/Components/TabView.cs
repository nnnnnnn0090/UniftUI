using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    public class TabView : UIElement, ILayoutContainer
    {
        private List<TabItem> tabs = new List<TabItem>();
        private State<int> selectedIndex;
        private List<UIElement> children = new List<UIElement>();
        private Action content;
        private Color tabBarColor = new Color(0.95f, 0.95f, 0.95f);
        private Color activeTabColor = new Color(0.2f, 0.6f, 1.0f);
        private Color inactiveTabColor = new Color(0.7f, 0.7f, 0.7f);
        
        // フォント設定を保持するプロパティを追加
        private TMPro.TMP_FontAsset fontAsset = null;
        
        // アニメーション関連のプロパティ
        private float transitionDuration = 0.3f; // アニメーション時間
        private GameObject currentContentObj; // 現在表示中のコンテンツ
        private GameObject pendingContentToDestroy; // キャンセル待ちの古いコンテンツ
        private DelayedCallback currentAnimation; // 現在実行中のアニメーションタイマー

        // フォント設定メソッドを追加
        public TabView SetFont(TMPro.TMP_FontAsset font)
        {
            this.fontAsset = font;
            return this;
        }

        public TabView(Action content, State<int> externalSelectedIndex = null)
        {
            this.content = content;
            // 外部から渡された状態があればそれを使用し、なければ新しい状態を作成
            this.selectedIndex = externalSelectedIndex ?? new State<int>(0);
            
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

        // アニメーション時間を設定するメソッド
        public TabView WithTransitionDuration(float duration)
        {
            this.transitionDuration = Mathf.Max(0, duration);
            return this;
        }

        public void AddChild(UIElement child)
        {
            if (child is TabItem tabItem)
            {
                tabs.Add(tabItem);
            }
            else
            {
                children.Add(child);
            }
        }

        public void RemoveChild(UIElement child)
        {
            if (child is TabItem tabItem)
            {
                tabs.Remove(tabItem);
            }
            else
            {
                children.Remove(child);
            }
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (oldChild == null || newChild == null) return;

            if (oldChild is TabItem oldTabItem && newChild is TabItem newTabItem)
            {
                int index = tabs.IndexOf(oldTabItem);
                if (index != -1)
                {
                    tabs[index] = newTabItem;
                }
                else
                {
                    Debug.LogWarning($"ReplaceChild: oldTabItem not found in TabView. Old: {oldChild}, New: {newChild}");
                }
            }
            else
            {
                int index = children.IndexOf(oldChild);
                if (index != -1)
                {
                    children[index] = newChild;
                }
                else
                {
                    // TabItemではない要素がchildrenリストにも見つからない場合、
                    // oldChildがTabItemでnewChildがそうでない、またはその逆のケースも考慮する。
                    // ただし、通常モディファイアは同じ型のラッパーを返すため、このケースは稀。
                    Debug.LogWarning($"ReplaceChild: oldChild not found in TabView children. Old: {oldChild}, New: {newChild}");
                }
            }
        }

        public System.Collections.Generic.IEnumerable<UIElement> GetChildren()
        {
            // TabViewの論理的な子はTabItemのリストです。
            // childrenリストは通常空ですが、インターフェース準拠のために含めることも考慮できます。
            // ここでは、主に操作対象となるtabsを返します。
            var allChildren = new List<UIElement>();
            allChildren.AddRange(tabs);
            allChildren.AddRange(children); // 通常は空
            return allChildren;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject container = new GameObject("TabView");
            container.transform.SetParent(parent, false);

            // 背景色の適用
            Image background = null;
            if (backgroundColor != Color.clear)
            {
                background = container.AddComponent<Image>();
                background.color = backgroundColor;
            }

            // メインレイアウト（縦方向）
            VerticalLayoutGroup mainLayout = container.AddComponent<VerticalLayoutGroup>();
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;
            mainLayout.spacing = 0;
            mainLayout.padding = new RectOffset(0, 0, 0, 0);

            // コンテンツエリア
            GameObject contentArea = CreateContentArea(container.transform);
            
            // タブバー
            GameObject tabBar = CreateTabBar(container.transform);

            // 不透明度を適用
            ApplyAllEffects(container, background);

            // レイアウト設定
            ConfigureLayout(container);

            return container;
        }

        private GameObject CreateContentArea(Transform parent)
        {
            GameObject contentArea = new GameObject("ContentArea");
            contentArea.transform.SetParent(parent, false);

            // レイアウト設定
            LayoutElement contentLayout = contentArea.AddComponent<LayoutElement>();
            contentLayout.flexibleHeight = 1;
            contentLayout.flexibleWidth = 1;

            // 選択されたタブのコンテンツを表示（最初のタブは即時表示）
            if (tabs.Count > 0 && selectedIndex.Value < tabs.Count)
            {
                currentContentObj = CreateTabContent(contentArea.transform, tabs[selectedIndex.Value]);
            }

            // StateObserverを追加して選択変更を監視
            StateObserver observer = contentArea.AddComponent<StateObserver>();
            observer.Initialize(new State[] { selectedIndex }, () => {
                // タブ切り替え時はアニメーションを適用
                if (tabs.Count > 0 && selectedIndex.Value < tabs.Count)
                {
                    SwitchTabWithAnimation(contentArea.transform, tabs[selectedIndex.Value]);
                }
            });

            return contentArea;
        }

        // アニメーション付きでタブを切り替え
        private void SwitchTabWithAnimation(Transform contentParent, TabItem newTab)
        {
            // 既存のアニメーションがある場合はキャンセル
            CancelCurrentAnimation();
            
            // 現在のコンテンツをフェードアウト
            if (currentContentObj != null)
            {
                // 既存のゲームオブジェクトは破棄対象としてマーク
                pendingContentToDestroy = currentContentObj;
                
                // フェードアウトを開始
                UIAnimator.Fade(pendingContentToDestroy, 1.0f, 0.0f, transitionDuration * 0.5f, null);
            }
            
            // 新しいコンテンツを作成（透明状態で）
            GameObject newContent = CreateTabContent(contentParent, newTab);
            
            // 初期状態を透明に
            CanvasGroup canvasGroup = newContent.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = newContent.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0;
            
            // 新しいコンテンツを現在のコンテンツとして設定
            currentContentObj = newContent;
            
            // 短い遅延後に新しいコンテンツをフェードイン
            currentAnimation = contentParent.gameObject.AddComponent<DelayedCallback>();
            currentAnimation.Initialize(transitionDuration * 0.2f, () => {
                // 古いコンテンツを破棄
                if (pendingContentToDestroy != null)
                {
                    GameObject.Destroy(pendingContentToDestroy);
                    pendingContentToDestroy = null;
                }
                
                // 新しいコンテンツをフェードイン
                UIAnimator.Fade(newContent, 0.0f, 1.0f, transitionDuration * 0.5f, () => {
                    currentAnimation = null;
                });
            });
            
            Canvas.ForceUpdateCanvases();
        }
        
        // 現在実行中のアニメーションをキャンセル
        private void CancelCurrentAnimation()
        {
            if (currentAnimation != null)
            {
                GameObject.Destroy(currentAnimation);
                currentAnimation = null;
            }
            
            // 破棄待ちのコンテンツがあればすぐに破棄
            if (pendingContentToDestroy != null)
            {
                GameObject.Destroy(pendingContentToDestroy);
                pendingContentToDestroy = null;
            }
        }

        // タブコンテンツを作成
        private GameObject CreateTabContent(Transform parent, TabItem tab)
        {
            GameObject tabRootContentObj = new GameObject($"Content_Tab_{parent.childCount}");
            tabRootContentObj.transform.SetParent(parent, false);

            // tabRootContentObj を親いっぱいに広げる
            RectTransform rect = tabRootContentObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // tabRootContentObj に LayoutGroup を追加して、その唯一の子要素が拡張されるようにする
            var layoutGroup = tabRootContentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.padding = new RectOffset(0,0,0,0); // パディングなし

            // コンテンツを構築 (tabRootContentObj の子として)
            var tabContentBuilder = new TabContentContainer(tabRootContentObj.transform); // TabContentContainerの親を tabRootContentObj に変更
            
            // フォント設定
            // 1. TabViewに直接設定されているフォントを優先
            // 2. UIContextのグローバルフォントをチェック
            // 解決したフォントは DefaultFont にも反映（静的がドメインリロード等で欠けてもタブ本文・Scroll 再構築で伝播する）
            TMPro.TMP_FontAsset resolvedFont = fontAsset ?? UIContext.DefaultFont;
            if (resolvedFont != null)
            {
                tabContentBuilder.SetFont(resolvedFont);
                UIContext.SetDefaultFont(resolvedFont);
            }
            
            var parentContext = UIContext.Current;
            try
            {
                UIContext.Current = tabContentBuilder; // UIContext に設定
                tab.Content?.Invoke(); // 子要素を tabContentBuilder に収集
            }
            finally
            {
                UIContext.Current = parentContext;
            }
            tabContentBuilder.BuildChildren(); // 収集した子要素を実際にビルド

            return tabRootContentObj;
        }

        private GameObject CreateTabBar(Transform parent)
        {
            GameObject tabBar = new GameObject("TabBar");
            tabBar.transform.SetParent(parent, false);

            // 背景
            Image tabBarBg = tabBar.AddComponent<Image>();
            tabBarBg.color = tabBarColor;

            // 高さ固定
            LayoutElement tabBarLayout = tabBar.AddComponent<LayoutElement>();
            tabBarLayout.preferredHeight = 60;
            tabBarLayout.flexibleHeight = 0;

            // 水平レイアウト
            HorizontalLayoutGroup tabLayout = tabBar.AddComponent<HorizontalLayoutGroup>();
            tabLayout.childControlWidth = true;
            tabLayout.childControlHeight = true;
            tabLayout.childForceExpandWidth = true;
            tabLayout.childForceExpandHeight = true;
            tabLayout.spacing = 0;
            tabLayout.padding = new RectOffset(0, 0, 0, 0); // パディングを0に設定

            // タブボタンを作成
            for (int i = 0; i < tabs.Count; i++)
            {
                CreateTabButton(tabBar.transform, tabs[i], i);
            }

            // StateObserverを追加して選択タブの変更を監視
            StateObserver observer = tabBar.AddComponent<StateObserver>();
            observer.Initialize(new State[] { selectedIndex }, () => {
                // タブの選択が変更されたらボタンの表示を更新
                UpdateTabButtons(tabBar.transform);
            });

            return tabBar;
        }

        private void CreateTabButton(Transform parent, TabItem tab, int index)
        {
            GameObject tabButton = new GameObject($"Tab_{index}");
            tabButton.transform.SetParent(parent, false);

            // ボタン背景
            Image buttonBg = tabButton.AddComponent<Image>();
            buttonBg.color = index == selectedIndex.Value ? activeTabColor : inactiveTabColor;

            // ボタンコンポーネント
            Button button = tabButton.AddComponent<Button>();
            button.targetGraphic = buttonBg;
            button.onClick.AddListener(() => {
                selectedIndex.Value = index;
                UpdateTabButtons(parent);
            });

            // タブヘッダーコンテンツ用コンテナ
            GameObject contentContainer = new GameObject("TabHeaderContent");
            contentContainer.transform.SetParent(tabButton.transform, false);

            RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero; // 余白を0に設定
            contentRect.offsetMax = Vector2.zero; // 余白を0に設定

            // タイトルコンテンツの構築
            if (tab.TitleContent != null)
            {
                // アクションベースのタイトル（新機能）
                var contentBuilder = new TabContentContainer(contentContainer.transform);
                var parentContext = UIContext.Current;
                try
                {
                    UIContext.Current = contentBuilder;
                    tab.TitleContent.Invoke();
                }
                finally
                {
                    UIContext.Current = parentContext;
                }
                contentBuilder.BuildChildren();
            }
            else if (!string.IsNullOrEmpty(tab.Title))
            {
                // 文字列ベースのタイトル（従来の機能）
                TMPro.TextMeshProUGUI text = contentContainer.AddComponent<TMPro.TextMeshProUGUI>();
                text.text = tab.Title;
                text.alignment = TMPro.TextAlignmentOptions.Center;
                text.fontSize = 16;
                text.color = Color.white;
                
                // フォントアセットが設定されていれば適用
                if (fontAsset != null)
                {
                    text.font = fontAsset;
                }
            }

            // レイアウト設定
            LayoutElement buttonLayout = tabButton.AddComponent<LayoutElement>();
            buttonLayout.flexibleWidth = 1;
        }

        private void UpdateTabButtons(Transform tabBarParent)
        {
            for (int i = 0; i < tabBarParent.childCount; i++)
            {
                Transform tabChild = tabBarParent.GetChild(i);
                Image buttonBg = tabChild.GetComponent<Image>();
                if (buttonBg != null)
                {
                    buttonBg.color = i == selectedIndex.Value ? activeTabColor : inactiveTabColor;
                }
            }
        }

        private void ConfigureLayout(GameObject container)
        {
            bool isDirectChildOfCanvas = container.transform.parent.GetComponent<Canvas>() != null;

            if (isDirectChildOfCanvas)
            {
                RectTransform rectTransform = container.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }
            else
            {
                LayoutElement layoutElement = container.AddComponent<LayoutElement>();
                layoutElement.flexibleWidth = 1;
                layoutElement.flexibleHeight = 1;
            }
        }
    }

    // 遅延実行用の補助クラス
    public class DelayedCallback : MonoBehaviour
    {
        private float delay;
        private Action callback;
        private float elapsed = 0f;
        
        public void Initialize(float delay, Action callback)
        {
            this.delay = delay;
            this.callback = callback;
        }
        
        private void Update()
        {
            elapsed += Time.deltaTime;
            if (elapsed >= delay)
            {
                if (callback != null)
                {
                    var tempCallback = callback;
                    callback = null; // コールバックの重複実行を防止
                    tempCallback.Invoke();
                }
                Destroy(this);
            }
        }
        
        private void OnDestroy()
        {
            // コンポーネント破棄時にコールバックが未実行なら実行する
            if (callback != null)
            {
                try
                {
                    var tempCallback = callback;
                    callback = null; // 二重実行防止
                    tempCallback.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"DelayedCallback: Error during callback execution on destroy: {e.Message}");
                }
            }
        }
    }
}
