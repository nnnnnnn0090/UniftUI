using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// State 変化を監視し、UIElement のプロパティバインディングを LateUpdate で適用する。
    /// OnDisable/OnEnable で購読を一時停止・再開し、オーファン化しない。
    /// </summary>
    public sealed class DynamicEffectObserver : MonoBehaviour
    {
        private UIElement target;
        private BindingRegistry registry;
        private readonly Dictionary<State, Action> subscriptions = new Dictionary<State, Action>();

        private bool isDirty;
        private Animation? capturedAnimation;
        private bool attached;

        // ── 初期化 ───────────────────────────────────────────────────────────

        /// <summary>新しい UIElement と BindingRegistry をアタッチする。</summary>
        internal void Attach(UIElement element, BindingRegistry reg)
        {
            Detach();
            target = element;
            registry = reg;
            attached = true;
            Subscribe();
        }

        /// <summary>購読を全て解除してターゲット参照を切る。</summary>
        public void Detach()
        {
            Unsubscribe();
            target = null;
            registry = null;
            attached = false;
        }

        // ── 後方互換 Initialize ──────────────────────────────────────────────
        // Components/VStack 等が呼び出す旧 API を維持する

        public void Initialize(State[] states, UIElement element)
        {
            Detach();
            target = element;
            registry = element?.bindingRegistry;
            attached = true;

            if (states != null)
            {
                foreach (var state in states)
                {
                    if (state == null || subscriptions.ContainsKey(state)) continue;
                    var captured = state;
                    Action obs = () => OnStateChanged(captured);
                    subscriptions[state] = obs;
                    state.AddObserver(obs);
                }
            }
        }

        // ── 購読管理 ─────────────────────────────────────────────────────────

        private void Subscribe()
        {
            if (registry == null) return;
            foreach (var state in registry.ObservedStates)
            {
                if (state == null || subscriptions.ContainsKey(state)) continue;
                var captured = state;
                Action obs = () => OnStateChanged(captured);
                subscriptions[state] = obs;
                state.AddObserver(obs);
            }
        }

        private void Unsubscribe()
        {
            foreach (var kvp in subscriptions)
                kvp.Key?.RemoveObserver(kvp.Value);
            subscriptions.Clear();
        }

        // ── State 変化ハンドラ ────────────────────────────────────────────────

        private void OnStateChanged(State changed)
        {
            isDirty = true;

            // アニメーション優先度: stateAnimationMap > withAnimation コンテキスト
            if (!capturedAnimation.HasValue)
            {
                if (target?.stateAnimationMap != null &&
                    target.stateAnimationMap.TryGetValue(changed, out var stateAnim))
                {
                    capturedAnimation = stateAnim;
                }
                else if (registry != null)
                {
                    var regAnim = registry.AnimationFor(changed);
                    if (regAnim.HasValue)
                        capturedAnimation = regAnim;
                    else if (AnimationContext.Current.HasValue)
                        capturedAnimation = AnimationContext.Current;
                }
                else if (AnimationContext.Current.HasValue)
                {
                    capturedAnimation = AnimationContext.Current;
                }
            }
        }

        // ── LateUpdate ───────────────────────────────────────────────────────

        private void LateUpdate()
        {
            if (!isDirty) return;
            isDirty = false;

            // オーファン防止: ターゲットが null なら自己無効化
            if (target == null)
            {
                enabled = false;
                return;
            }

            if (capturedAnimation.HasValue)
            {
                target.pendingAnimation = capturedAnimation;
                capturedAnimation = null;
            }

            try
            {
                target.ApplyDynamicEffects();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] DynamicEffectObserver: error during ApplyDynamicEffects: {e.Message}");
            }
        }

        // ── Unity ライフサイクル ─────────────────────────────────────────────

        private void OnEnable()
        {
            if (attached) Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Detach();
        }
    }
}
