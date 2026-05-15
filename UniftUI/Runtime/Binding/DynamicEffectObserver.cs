using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// Subscribes to <see cref="State"/> changes and applies <see cref="UIElement"/> bindings in <c>LateUpdate</c>.
    /// Pauses subscriptions while disabled to avoid orphaned observers.
    /// </summary>
    public sealed class DynamicEffectObserver : MonoBehaviour
    {
        private UIElement target;
        private BindingRegistry registry;
        private readonly Dictionary<State, Action> subscriptions = new Dictionary<State, Action>();

        private bool isDirty;
        private Animation? capturedAnimation;
        private bool attached;

        internal void Attach(UIElement element, BindingRegistry reg)
        {
            Detach();
            target = element;
            registry = reg;
            attached = true;
            Subscribe();
        }

        public void Detach()
        {
            Unsubscribe();
            target = null;
            registry = null;
            attached = false;
        }

        /// <summary>Legacy entry point used by some container elements.</summary>
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

        private void OnStateChanged(State changed)
        {
            isDirty = true;

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

        private void LateUpdate()
        {
            if (!isDirty) return;
            isDirty = false;

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
