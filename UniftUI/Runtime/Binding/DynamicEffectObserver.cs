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
        private readonly List<State> dirtyStates = new List<State>();
        private readonly Dictionary<State, Animation> dirtyAnimations = new Dictionary<State, Animation>();

        private bool isDirty;
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
            ClearPending();
            target = null;
            registry = null;
            attached = false;
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

            if (changed != null && !dirtyStates.Contains(changed))
                dirtyStates.Add(changed);

            Animation? capturedAnimation = CaptureAnimationFor(changed);
            if (capturedAnimation.HasValue && changed != null)
                dirtyAnimations[changed] = capturedAnimation.Value;

            ApplyIfDirty();
        }

        private Animation? CaptureAnimationFor(State changed)
        {
            if (target?.stateAnimationMap != null &&
                target.stateAnimationMap.TryGetValue(changed, out var stateAnim))
            {
                return stateAnim;
            }

            if (registry != null)
            {
                var regAnim = registry.AnimationFor(changed);
                if (regAnim.HasValue)
                    return regAnim;
            }

            return AnimationContext.Current;
        }

        private void LateUpdate()
        {
            if (!isDirty) return;
            ApplyIfDirty();
        }

        private void ApplyIfDirty()
        {
            if (!isDirty) return;
            isDirty = false;

            if (target == null)
            {
                ClearPending();
                enabled = false;
                return;
            }

            State[] states = dirtyStates.ToArray();
            dirtyStates.Clear();

            try
            {
                if (states.Length == 0)
                {
                    target.ApplyDynamicEffects();
                    dirtyAnimations.Clear();
                    return;
                }

                foreach (State state in states)
                {
                    Animation? animation = null;
                    if (state != null && dirtyAnimations.TryGetValue(state, out var captured))
                        animation = captured;

                    target.ApplyDynamicEffects(state, animation);
                    if (state != null)
                        dirtyAnimations.Remove(state);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] DynamicEffectObserver: error during ApplyDynamicEffects: {e.Message}");
                dirtyAnimations.Clear();
            }
        }

        private void OnEnable()
        {
            if (attached) Subscribe();
        }

        private void OnDisable()
        {
            ClearPending();
            Unsubscribe();
        }

        private void OnDestroy()
        {
            Detach();
        }

        private void ClearPending()
        {
            isDirty = false;
            dirtyStates.Clear();
            dirtyAnimations.Clear();
        }
    }
}
