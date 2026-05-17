using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniftUI
{
    internal enum BindingKind
    {
        Visual,
        Layout,
        Content,
        ObserveOnly
    }

    /// <summary>
    /// Per-<see cref="UIElement"/> registry of state-driven property updates.
    /// Call <see cref="Dispose"/> on rebuild to drop stale bindings.
    /// </summary>
    internal sealed class BindingRegistry
    {
        private struct LiveBinding
        {
            public State State;
            public Action UpdateAction;
            public BindingKind Kind;
        }

        private readonly Dictionary<string, LiveBinding> bindings = new Dictionary<string, LiveBinding>();
        private readonly Dictionary<State, List<string>> stateToProps = new Dictionary<State, List<string>>();
        private Dictionary<State, Animation> stateAnimations;

        public ICollection<State> ObservedStates => stateToProps.Keys;

        public void Register(string propertyName, State state, Action updateAction,
            BindingKind kind = BindingKind.Visual)
        {
            if (state == null || updateAction == null || string.IsNullOrEmpty(propertyName)) return;

            bindings[propertyName] = new LiveBinding
            {
                State = state,
                UpdateAction = updateAction,
                Kind = kind
            };

            if (!stateToProps.TryGetValue(state, out var list))
            {
                list = new List<string>();
                stateToProps[state] = list;
            }
            if (!list.Contains(propertyName))
                list.Add(propertyName);
        }

        public void SetStateAnimation(State state, Animation anim)
        {
            if (state == null) return;
            if (stateAnimations == null) stateAnimations = new Dictionary<State, Animation>();
            stateAnimations[state] = anim;
        }

        public Animation? AnimationFor(State state)
        {
            if (stateAnimations != null && state != null &&
                stateAnimations.TryGetValue(state, out var anim))
                return anim;
            return null;
        }

        public bool HasAnimatableBindingFor(State state)
        {
            if (state == null || !stateToProps.TryGetValue(state, out var properties))
                return false;

            foreach (string propertyName in properties)
            {
                if (!bindings.TryGetValue(propertyName, out var binding))
                    continue;

                if (!ReferenceEquals(binding.State, state))
                    continue;

                if (binding.Kind == BindingKind.Visual || binding.Kind == BindingKind.Layout)
                    return true;
            }

            return false;
        }

        public void ApplyAll()
        {
            foreach (var kvp in bindings)
            {
                Apply(kvp.Key, kvp.Value);
            }
        }

        public void ApplyForState(State state)
        {
            if (state == null)
            {
                ApplyAll();
                return;
            }

            if (!stateToProps.TryGetValue(state, out var properties))
                return;

            foreach (string propertyName in properties)
            {
                if (!bindings.TryGetValue(propertyName, out var binding))
                    continue;

                if (!ReferenceEquals(binding.State, state))
                    continue;

                Apply(propertyName, binding);
            }
        }

        private static void Apply(string propertyName, LiveBinding binding)
        {
            try
            {
                binding.UpdateAction?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] BindingRegistry: error updating '{propertyName}': {e.Message}");
            }
        }

        public void Dispose()
        {
            bindings.Clear();
            stateToProps.Clear();
            stateAnimations?.Clear();
        }
    }
}
