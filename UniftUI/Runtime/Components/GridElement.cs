using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// SwiftUI の <c>Grid</c> に相当。子は <see cref="GridRowElement"/> のみを想定（<c>GridRow { }</c>）。
    /// 行間は <see cref="VerticalLayoutGroup.spacing"/>（<paramref name="verticalSpacing"/>）、セル間は各行の水平 <paramref name="horizontalSpacing"/>。
    /// </summary>
    /// <remarks>
    /// 構築後に <see cref="GridColumnSynchronizer"/> が各行同一列のセルに、列ごとの最大希望幅を適用し SwiftUI の Grid に近い列揃えを行います。
    /// </remarks>
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
                Debug.LogWarning($"ReplaceChild: Grid に oldChild が見つかりません。Children: {children.Count}");
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

            VerticalLayoutGroup layout = container.AddComponent<VerticalLayoutGroup>();
            // 親（ルート Canvas など）よりコンテンツが低いとき、行全体を縦方向に中央へ（SwiftUI で可視領域いっぱいの Grid が中央に寄る挙動に近づける）
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            // 各行（GridRow）をグリッドの利用可能幅いっぱいに伸ばす（セル側の Frame(infiniteWidth: true) が効く前提）
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = VerticalSpacing;
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);

            LayoutElement layoutElement = container.AddComponent<LayoutElement>();
            ContentSizeFitter fitter = container.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredWidth >= 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
            }

            if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
            }

            foreach (var child in children)
                child.Build(container.transform);

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
                    Debug.LogWarning("Grid: コンテナが既に破棄されています");
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
                    localContent?.Invoke();
                    UIContext.Current = parentContext;

                    foreach (var child in localChildren)
                    {
                        if (child == null || container.transform == null) continue;
                        if (UIContext.DefaultFont != null)
                            child.Font(UIContext.DefaultFont);
                        child.Build(container.transform);
                    }

                    RectTransform syncRt = container.GetComponent<RectTransform>();
                    if (syncRt != null)
                        GridColumnSynchronizer.Apply(syncRt);

                    Canvas.ForceUpdateCanvases();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Grid: UI の再構築中にエラー: {e.Message}\n{e.StackTrace}");
                }
            });
        }
    }
}
