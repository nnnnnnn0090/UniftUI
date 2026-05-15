using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// SwiftUI の <c>.offset(x:y:)</c> に相当。親から割り当てられたレイアウト枠はそのままに、中身だけ視覚的にずらします。
    /// UniftUI の座標系は <see cref="UIElement.WithPosition"/> と同じく X 右・Y 下向きが正です。
    /// </summary>
    public class OffsetElement : UIElement, ILayoutContainer
    {
        private UIElement content;
        private Vector2 offset;
        private State<Vector2> offsetState;
        private State<float> offsetXState;
        private float offsetYConst;
        private bool useXyStates;
        private bool useVectorState;
        private RectTransform _contentRect;

        public OffsetElement(UIElement content, Vector2 offset)
        {
            this.content = content;
            this.offset = offset;
            CopyStyleFromContent(content);
        }

        public OffsetElement(UIElement content, State<Vector2> offset)
        {
            this.content = content;
            this.offsetState = offset;
            this.useVectorState = true;
            if (offset != null) this.offset = offset.Value;
            CopyStyleFromContent(content);
        }

        public OffsetElement(UIElement content, State<float> offsetX, float y)
        {
            this.content = content;
            this.offsetXState = offsetX;
            this.offsetYConst = y;
            this.useXyStates = true;
            this.offset = new Vector2(offsetX != null ? offsetX.Value : 0f, y);
            CopyStyleFromContent(content);
        }

        private void CopyStyleFromContent(UIElement src)
        {
            if (src == null) return;
            this.useCustomPosition = src.useCustomPosition;
            this.customPosition = src.customPosition;
            this.rotationEffectEuler = src.rotationEffectEuler;
            this.scaleEffect = src.scaleEffect;
            this.preferredWidth = src.preferredWidth;
            this.preferredHeight = src.preferredHeight;
            this.infiniteWidth = src.infiniteWidth;
            this.infiniteHeight = src.infiniteHeight;
        }

        public void AddChild(UIElement child)
        {
            if (this.content == null) this.content = child;
            else Debug.LogWarning("OffsetElement can only contain one child.");
        }

        public void RemoveChild(UIElement child)
        {
            if (this.content == child) this.content = null;
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (this.content == oldChild) this.content = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            if (content != null) return new List<UIElement> { content };
            return new List<UIElement>();
        }

        /// <summary>SwiftUI offset と同じ向き → Unity の anchoredPosition 加算分。</summary>
        public static Vector2 SwiftOffsetToAnchoredDelta(Vector2 swift)
        {
            return new Vector2(swift.x, -swift.y);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject outer = new GameObject("Offset");
            outer.transform.SetParent(parent, false);

            Image bg = null;
            if (backgroundColor != Color.clear)
            {
                bg = outer.AddComponent<Image>();
                bg.color = backgroundColor;
            }

            ContentSizeFitter fitter = outer.AddComponent<ContentSizeFitter>();
            LayoutElement outerLe = outer.AddComponent<LayoutElement>();

            if (infiniteWidth)
            {
                outerLe.flexibleWidth = 1;
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredWidth >= 0)
            {
                outerLe.preferredWidth = preferredWidth;
                outerLe.minWidth = preferredWidth;
                outerLe.flexibleWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                outerLe.flexibleWidth = 0;
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (infiniteHeight)
            {
                outerLe.flexibleHeight = 1;
                fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                outerLe.preferredHeight = preferredHeight;
                outerLe.minHeight = preferredHeight;
                outerLe.flexibleHeight = 0;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                outerLe.flexibleHeight = 0;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (preferredWidth >= 0 && !infiniteWidth)
                content?.WithInfiniteWidth();
            if (preferredHeight >= 0 && !infiniteHeight)
                content?.WithInfiniteHeight();
            if (infiniteWidth)
                PropagateInfiniteWidthToContent();
            if (infiniteHeight)
                PropagateInfiniteHeightToContent();

            GameObject contentObj = content != null ? content.Build(outer.transform) : null;
            RectTransform outerRt = outer.GetComponent<RectTransform>();

            if (contentObj != null)
            {
                _contentRect = contentObj.GetComponent<RectTransform>();

                LayoutElement childLe = contentObj.GetComponent<LayoutElement>();
                if (childLe == null) childLe = contentObj.AddComponent<LayoutElement>();

                float w = ResolveOuterSpan(preferredWidth, infiniteWidth, _contentRect, outerRt, true);
                float h = ResolveOuterSpan(preferredHeight, infiniteHeight, _contentRect, outerRt, false);

                if (!infiniteWidth && preferredWidth >= 0)
                {
                    outerLe.preferredWidth = w;
                    outerLe.minWidth = w;
                }
                if (!infiniteHeight && preferredHeight >= 0)
                {
                    outerLe.preferredHeight = h;
                    outerLe.minHeight = h;
                }

                ConfigureChildRectForContentSizing(childLe, w, h);
                // 初回 Build 時は即時反映（この時点では withAnimation コンテキストはない）
                SetVisualOffset(_contentRect, GetCurrentOffset());

                if (useVectorState && offsetState != null)
                {
                    AddPropertyBinding(offsetState, () =>
                    {
                        ApplyVisualOffsetFromBinding(offsetState.Value);
                    }, "offset");
                }

                if (useXyStates && offsetXState != null)
                {
                    AddPropertyBinding(offsetXState, () =>
                    {
                        ApplyVisualOffsetFromBinding(new Vector2(offsetXState.Value, offsetYConst));
                    }, "offsetX");
                }
            }

            builtGameObject = outer;
            ApplyAllEffects(outer, bg);
            return outer;
        }

        private static float ResolveOuterSpan(float preferred, bool infinite, RectTransform contentRt,
            RectTransform outerRt, bool horizontal)
        {
            if (preferred >= 0)
                return preferred;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRt);
            float r = horizontal ? contentRt.rect.width : contentRt.rect.height;
            if (infinite && r < 2f)
                return horizontal ? Screen.width : Screen.height;
            return Mathf.Max(1f, r);
        }

        private Vector2 GetCurrentOffset()
        {
            if (useVectorState && offsetState != null) return offsetState.Value;
            if (useXyStates && offsetXState != null) return new Vector2(offsetXState.Value, offsetYConst);
            return offset;
        }

        /// <summary>
        /// 子を親（Offset 外周）いっぱいにストレッチさせるか、intrinsic サイズで左上固定にするか。
        /// Build 後にここで上書きしていたため、ZStack の infiniteHeight などが効かなくなっていた。
        /// </summary>
        private void ConfigureChildRectForContentSizing(LayoutElement childLe, float resolvedW, float resolvedH)
        {
            if (content == null || _contentRect == null) return;

            bool iw = content.infiniteWidth;
            bool ih = content.infiniteHeight;

            // intrinsic（両軸とも無限でない）: レイアウトから外し、測った矩形で左上固定
            if (!iw && !ih)
            {
                childLe.ignoreLayout = true;
                _contentRect.anchorMin = _contentRect.anchorMax = new Vector2(0f, 1f);
                _contentRect.pivot = new Vector2(0f, 1f);
                _contentRect.sizeDelta = new Vector2(resolvedW, resolvedH);
                _contentRect.offsetMin = _contentRect.offsetMax = Vector2.zero;
                return;
            }

            childLe.ignoreLayout = false;

            // 両方ストレッチ: 親矩形いっぱい（ZStack の全画面オーバーレイ等）
            if (iw && ih)
            {
                _contentRect.anchorMin = Vector2.zero;
                _contentRect.anchorMax = Vector2.one;
                _contentRect.pivot = new Vector2(0.5f, 0.5f);
                _contentRect.offsetMin = Vector2.zero;
                _contentRect.offsetMax = Vector2.zero;
                return;
            }

            // 幅固定・縦ストレッチ（ドロワー列など）
            if (!iw && ih)
            {
                float colW = preferredWidth >= 0 ? preferredWidth : resolvedW;
                _contentRect.anchorMin = new Vector2(0f, 0f);
                _contentRect.anchorMax = new Vector2(0f, 1f);
                _contentRect.pivot = new Vector2(0f, 1f);
                _contentRect.sizeDelta = new Vector2(colW, 0f);
                _contentRect.anchoredPosition = Vector2.zero;
                return;
            }

            // 横ストレッチ・高さ固定（ナビバー等：上辺に張り付く）
            if (iw && !ih)
            {
                float rowH = preferredHeight >= 0 ? preferredHeight : resolvedH;
                _contentRect.anchorMin = new Vector2(0f, 1f);
                _contentRect.anchorMax = new Vector2(1f, 1f);
                _contentRect.pivot = new Vector2(0.5f, 1f);
                _contentRect.sizeDelta = new Vector2(0f, -rowH);
                _contentRect.anchoredPosition = Vector2.zero;
                return;
            }
        }

        /// <summary>アンカーは触らず、SwiftUI 方向のオフセットだけを anchoredPosition に反映する。</summary>
        private static void SetVisualOffset(RectTransform rt, Vector2 swiftOffset)
        {
            if (rt == null) return;
            rt.anchoredPosition = SwiftOffsetToAnchoredDelta(swiftOffset);
        }

        /// <summary>
        /// State 更新時: <see cref="UIElement.WithPosition(State{float}, float)"/> と同様、
        /// <c>withAnimation</c> 中は <see cref="PositionAnimator"/> でスライドする。
        /// </summary>
        private void ApplyVisualOffsetFromBinding(Vector2 swiftOffset)
        {
            if (_contentRect == null || builtGameObject == null) return;

            Vector2 targetUnity = SwiftOffsetToAnchoredDelta(swiftOffset);

            if (useAnimation && animationDuration > 0)
            {
                PositionAnimator animator = _contentRect.gameObject.GetComponent<PositionAnimator>();
                if (animator == null)
                    animator = _contentRect.gameObject.AddComponent<PositionAnimator>();

                Vector2 fromUnity = _contentRect.anchoredPosition;
                animator.AnimateTo(fromUnity, targetUnity, animationDuration, animationEasing);
            }
            else
            {
                SetVisualOffset(_contentRect, swiftOffset);
            }
        }

        protected override void PropagateInfiniteWidthToContent()
        {
            content?.WithInfiniteWidth();
        }

        protected override void PropagateInfiniteHeightToContent()
        {
            content?.WithInfiniteHeight();
        }
    }
}
