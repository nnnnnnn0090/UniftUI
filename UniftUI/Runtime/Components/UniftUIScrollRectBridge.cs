using System;
using UnityEngine;
using UnityEngine.UI;

namespace UniftUI
{
    /// <summary>
    /// <see cref="ScrollRect"/> と <see cref="State{T}"/> を同期します。
    /// 垂直: <see cref="ScrollRect.verticalNormalizedPosition"/> に合わせ <b>1 = 先頭、0 = 末尾</b>。
    /// 水平: <see cref="ScrollRect.horizontalNormalizedPosition"/> は <b>0 = 左、1 = 右</b>。
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class UniftUIScrollRectBridge : MonoBehaviour
    {
        private ScrollRect scrollRect;
        private State<float> verticalState;
        private State<float> horizontalState;
        private bool twoWayVertical;
        private bool twoWayHorizontal;

        private Action verticalObserver;
        private Action horizontalObserver;

        public ScrollRect ScrollRect => scrollRect;

        /// <param name="twoWayVertical">縦をドラッグで <see cref="State{T}"/> に書き戻す。</param>
        /// <param name="twoWayHorizontal">横をドラッグで State に書き戻す。</param>
        public void Initialize(ScrollRect sr, State<float> vertical, State<float> horizontal, bool twoWayVertical, bool twoWayHorizontal)
        {
            scrollRect = sr != null ? sr : GetComponent<ScrollRect>();
            verticalState = vertical;
            horizontalState = horizontal;
            this.twoWayVertical = twoWayVertical;
            this.twoWayHorizontal = twoWayHorizontal;

            if (scrollRect != null && (twoWayVertical || twoWayHorizontal))
                scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

            if (verticalState != null)
            {
                verticalObserver = ApplyVerticalFromState;
                verticalState.AddObserver(verticalObserver);
                ApplyVerticalFromState();
            }

            if (horizontalState != null)
            {
                horizontalObserver = ApplyHorizontalFromState;
                horizontalState.AddObserver(horizontalObserver);
                ApplyHorizontalFromState();
            }
        }

        private void OnDestroy()
        {
            if (scrollRect != null && (twoWayVertical || twoWayHorizontal))
                scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);

            if (verticalState != null && verticalObserver != null)
                verticalState.RemoveObserver(verticalObserver);
            if (horizontalState != null && horizontalObserver != null)
                horizontalState.RemoveObserver(horizontalObserver);
        }

        private void ApplyVerticalFromState()
        {
            if (scrollRect == null || verticalState == null || !scrollRect.vertical) return;
            SetScrollSuppressed(() =>
            {
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(verticalState.Value);
            });
        }

        private void ApplyHorizontalFromState()
        {
            if (scrollRect == null || horizontalState == null || !scrollRect.horizontal) return;
            SetScrollSuppressed(() =>
            {
                scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(horizontalState.Value);
            });
        }

        private void SetScrollSuppressed(Action apply)
        {
            if (twoWayVertical || twoWayHorizontal)
                scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            try
            {
                apply?.Invoke();
            }
            finally
            {
                if (twoWayVertical || twoWayHorizontal)
                    scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            }
        }

        private void OnScrollValueChanged(Vector2 normalized)
        {
            if (twoWayVertical && verticalState != null && scrollRect.vertical)
            {
                float v = Mathf.Clamp01(normalized.y);
                if (!Mathf.Approximately(verticalState.Value, v))
                    verticalState.Value = v;
            }
            if (twoWayHorizontal && horizontalState != null && scrollRect.horizontal)
            {
                float h = Mathf.Clamp01(normalized.x);
                if (!Mathf.Approximately(horizontalState.Value, h))
                    horizontalState.Value = h;
            }
        }

        /// <summary>垂直時: 先頭へ（normalized 1）。</summary>
        public void ScrollToTop()
        {
            if (scrollRect == null || !scrollRect.vertical) return;
            SetScrollSuppressed(() => scrollRect.verticalNormalizedPosition = 1f);
            if (twoWayVertical && verticalState != null)
                verticalState.Value = 1f;
        }

        /// <summary>垂直時: 末尾へ（normalized 0）。</summary>
        public void ScrollToBottom()
        {
            if (scrollRect == null || !scrollRect.vertical) return;
            SetScrollSuppressed(() => scrollRect.verticalNormalizedPosition = 0f);
            if (twoWayVertical && verticalState != null)
                verticalState.Value = 0f;
        }
    }
}
