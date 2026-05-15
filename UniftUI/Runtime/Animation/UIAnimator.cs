using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniftUI
{
    // ──────────────────────────────────────────────────────────────────────────
    // UIAnimator — Fade / SlideHorizontal などの高レベルアニメーションユーティリティ
    // ──────────────────────────────────────────────────────────────────────────
    public static class UIAnimator
    {
        private static readonly Dictionary<RectTransform, SlideState> activeSlides
            = new Dictionary<RectTransform, SlideState>();

        public static void Fade(GameObject target, float from, float to, float duration, Action onComplete = null)
        {
            if (target == null) return;
            var runner = GetOrAddRunner(target);
            runner.StartCoroutine(FadeRoutine(target, from, to, duration, onComplete));
        }

        public static void FadeOutAndDestroy(GameObject target, float duration)
        {
            if (target == null) return;
            Fade(target, 1f, 0f, duration, () => { if (target != null) GameObject.Destroy(target); });
        }

        public static GameObject FadeIn(GameObject target, float duration)
        {
            if (target == null) return null;
            GetOrAddCanvasGroup(target).alpha = 0f;
            target.SetActive(true);
            Fade(target, 0f, 1f, duration);
            return target;
        }

        public static void SlideHorizontal(RectTransform targetRect, float targetX, float duration, Action onComplete = null)
        {
            if (targetRect == null || targetRect.gameObject == null) return;
            var runner = GetOrAddRunner(targetRect.gameObject);

            if (activeSlides.TryGetValue(targetRect, out var existing))
            {
                if (existing.Coroutine != null) runner.StopCoroutine(existing.Coroutine);
                activeSlides.Remove(targetRect);
            }

            if (runner != null && runner.gameObject.activeInHierarchy)
            {
                var co = runner.StartCoroutine(SlideRoutine(targetRect, targetX, duration, onComplete));
                activeSlides[targetRect] = new SlideState(co);
            }
            else
            {
                var pos = targetRect.anchoredPosition;
                pos.x = targetX;
                targetRect.anchoredPosition = pos;
                onComplete?.Invoke();
            }
        }

        private static IEnumerator FadeRoutine(GameObject target, float from, float to, float duration, Action onComplete)
        {
            if (target == null) { onComplete?.Invoke(); yield break; }
            var cg = GetOrAddCanvasGroup(target);
            if (cg == null) { onComplete?.Invoke(); yield break; }
            cg.alpha = from;
            float t = 0;
            while (t < duration)
            {
                if (target == null || cg == null) { onComplete?.Invoke(); yield break; }
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            if (target != null && cg != null) cg.alpha = to;
            onComplete?.Invoke();
        }

        private static IEnumerator SlideRoutine(RectTransform targetRect, float targetX, float duration, Action onComplete)
        {
            if (targetRect == null) { onComplete?.Invoke(); yield break; }
            var startPos = targetRect.anchoredPosition;
            float startX = startPos.x;
            float t = 0;
            while (t < duration)
            {
                if (targetRect == null) { onComplete?.Invoke(); yield break; }
                t += Time.deltaTime;
                float x = Mathf.Lerp(startX, targetX, duration > 0 ? Mathf.Clamp01(t / duration) : 1f);
                targetRect.anchoredPosition = new Vector2(x, startPos.y);
                yield return null;
            }
            if (targetRect != null)
                targetRect.anchoredPosition = new Vector2(targetX, startPos.y);
            if (activeSlides.ContainsKey(targetRect))
                activeSlides.Remove(targetRect);
            onComplete?.Invoke();
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
            => go.GetComponent<CanvasGroup>() ?? go.AddComponent<CanvasGroup>();

        private static AnimationRunner GetOrAddRunner(GameObject go)
            => go.GetComponent<AnimationRunner>() ?? go.AddComponent<AnimationRunner>();

        private struct SlideState
        {
            public readonly Coroutine Coroutine;
            public SlideState(Coroutine co) { Coroutine = co; }
        }

        private class AnimationRunner : MonoBehaviour { }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // BaseAnimator<T> — 全アニメーターの汎用基底クラス
    //
    // GetOrReplace<TAnimator>(go) で重複コンポーネントを防ぐ。
    // CancelAnimation() でコールバックを発火させずに停止できる。
    // ──────────────────────────────────────────────────────────────────────────
    public abstract class BaseAnimator<T> : MonoBehaviour
    {
        protected T startValue;
        protected T targetValue;
        protected float duration = 1f;
        protected float elapsedTime;
        protected bool isAnimating;
        protected Action onCompleteCallback;
        protected AnimationEasing easing = AnimationEasing.Linear;

        private int generation;
        internal int CurrentGeneration => generation;
        internal bool IsAnimating => isAnimating;

        /// <summary>
        /// 同じ GameObject 上に既存コンポーネントがあればキャンセルして再利用する。
        /// 新規の場合は AddComponent する。
        /// </summary>
        public static TAnimator GetOrReplace<TAnimator>(GameObject go)
            where TAnimator : BaseAnimator<T>
        {
            var existing = go.GetComponent<TAnimator>();
            if (existing != null)
            {
                existing.CancelAnimation();
                return existing;
            }
            return go.AddComponent<TAnimator>();
        }

        public void AnimateTo(T from, T to, float dur, Action onComplete = null)
            => AnimateTo(from, to, dur, AnimationEasing.Linear, onComplete);

        public void AnimateTo(T from, T to, float dur, AnimationEasing ease, Action onComplete = null)
        {
            generation++;
            startValue = from;
            targetValue = to;
            duration = dur;
            easing = ease;
            elapsedTime = 0f;
            isAnimating = true;
            onCompleteCallback = onComplete;
            SetInitialValue(from);
        }

        /// <summary>アニメーションを停止する。onComplete コールバックは呼ばれない。</summary>
        public void CancelAnimation()
        {
            isAnimating = false;
            onCompleteCallback = null;
        }

        protected virtual void Update()
        {
            if (!isAnimating) return;
            elapsedTime += Time.deltaTime;
            float n = Mathf.Clamp01(elapsedTime / duration);
            UpdateValue(ApplyEasing(n, easing));
            if (n >= 1f)
            {
                isAnimating = false;
                var cb = onCompleteCallback;
                onCompleteCallback = null;
                cb?.Invoke();
            }
        }

        protected float ApplyEasing(float t, AnimationEasing easingType)
        {
            switch (easingType)
            {
                case AnimationEasing.Linear:      return t;
                case AnimationEasing.EaseIn:      return t * t * t;
                case AnimationEasing.EaseOut:     return 1f - Mathf.Pow(1f - t, 3);
                case AnimationEasing.EaseInOut:   return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3) / 2f;
                case AnimationEasing.EaseOutBounce:
                    if (t < 1f / 2.75f)       return 7.5625f * t * t;
                    if (t < 2f / 2.75f)       return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
                    if (t < 2.5f / 2.75f)     return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
                    return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
                case AnimationEasing.EaseOutElastic:
                    float p = 0.3f;
                    return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - p / 4f) * (2f * Mathf.PI) / p) + 1f;
                case AnimationEasing.EaseOutBack:
                    float c1 = 1.70158f, c3 = c1 + 1f;
                    return 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
                case AnimationEasing.Spring:
                    float s = 1f - t;
                    return 1f - s * Mathf.Sin(10f * Mathf.PI * t) * Mathf.Exp(-5f * t);
                default: return t;
            }
        }

        protected abstract void SetInitialValue(T value);
        protected abstract void UpdateValue(float t);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // 具体的アニメーターコンポーネント
    // ──────────────────────────────────────────────────────────────────────────

    public class RotationAnimator : BaseAnimator<Vector3>
    {
        protected override void SetInitialValue(Vector3 v)
        {
            var r = GetComponent<RectTransform>();
            if (r) r.localRotation = Quaternion.Euler(v);
        }
        protected override void UpdateValue(float t)
        {
            var r = GetComponent<RectTransform>();
            if (r) r.localRotation = Quaternion.Euler(Vector3.Lerp(startValue, targetValue, t));
        }
    }

    public class ScaleAnimator : BaseAnimator<Vector3>
    {
        protected override void SetInitialValue(Vector3 v)
        {
            var r = GetComponent<RectTransform>();
            if (r) r.localScale = v;
        }
        protected override void UpdateValue(float t)
        {
            var r = GetComponent<RectTransform>();
            if (r) r.localScale = Vector3.Lerp(startValue, targetValue, t);
        }
    }

    public class PositionAnimator : BaseAnimator<Vector2>
    {
        protected override void SetInitialValue(Vector2 v)
        {
            var r = GetComponent<RectTransform>();
            if (r) r.anchoredPosition = v;
        }
        protected override void UpdateValue(float t)
        {
            var r = GetComponent<RectTransform>();
            if (r) r.anchoredPosition = Vector2.Lerp(startValue, targetValue, t);
        }
    }

    public class OpacityAnimator : BaseAnimator<float>
    {
        private CanvasGroup cg;
        protected override void SetInitialValue(float v)
        {
            cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (cg != null) cg.alpha = v;
        }
        protected override void UpdateValue(float t)
        {
            if (cg != null) cg.alpha = Mathf.Lerp(startValue, targetValue, t);
        }
    }

    public class BackgroundColorAnimator : BaseAnimator<Color>
    {
        private Image img;
        protected override void SetInitialValue(Color v)
        {
            img = GetComponent<Image>();
            if (img) img.color = v;
        }
        protected override void UpdateValue(float t)
        {
            if (img) img.color = Color.Lerp(startValue, targetValue, t);
        }
    }

    public class CornerRadiusAnimator : BaseAnimator<Vector4>
    {
        private Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners rc;
        protected override void SetInitialValue(Vector4 v)
        {
            rc = GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (rc == null)
                rc = gameObject.AddComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            rc.r = v;
            rc.Validate();
            rc.Refresh();
        }
        protected override void UpdateValue(float t)
        {
            if (rc) { rc.r = Vector4.Lerp(startValue, targetValue, t); rc.Validate(); rc.Refresh(); }
        }
    }

    public class TextColorAnimator : BaseAnimator<Color>
    {
        private TMPro.TextMeshProUGUI tmp;
        protected override void SetInitialValue(Color v)
        {
            tmp = GetComponent<TMPro.TextMeshProUGUI>();
            if (tmp) tmp.color = v;
        }
        protected override void UpdateValue(float t)
        {
            if (tmp) tmp.color = Color.Lerp(startValue, targetValue, t);
        }
    }
}
