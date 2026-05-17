using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniftUI
{
    /// <summary>
    /// Single runtime bridge from <see cref="State"/> changes to <see cref="UIElement"/> binding updates.
    /// Attached to built GameObjects by <see cref="UIElement.SetupDynamicEffects"/>.
    /// </summary>
    internal sealed class ElementAnimationHost : MonoBehaviour
    {
        private UIElement element;
        private readonly Dictionary<State, Action> observers = new Dictionary<State, Action>();
        private readonly List<IDisposable> subscriptions = new List<IDisposable>();

        internal void Attach(UIElement target)
        {
            Detach();
            element = target;
            if (element == null)
                return;

            SubscribeAll();
            element.ApplyDynamicEffects();
        }

        internal void Detach()
        {
            foreach (var subscription in subscriptions)
                subscription?.Dispose();
            subscriptions.Clear();
            observers.Clear();
            element = null;
        }

        private void SubscribeAll()
        {
            if (element == null)
                return;

            var seen = new HashSet<State>();
            foreach (State state in element.GetWatchedStates())
            {
                if (state == null || !seen.Add(state))
                    continue;

                Subscribe(state);
            }
        }

        private void Subscribe(State state)
        {
            if (state == null || observers.ContainsKey(state))
                return;

            State captured = state;
            Action observer = () => OnStateChanged(captured);
            observers[state] = observer;
            subscriptions.Add(state.AddObserver(observer));
        }

        private void OnStateChanged(State changedState)
        {
            if (element == null)
                return;

            element.HandleStateChange(changedState);
        }

        private void OnEnable()
        {
            if (element != null && subscriptions.Count == 0)
                SubscribeAll();
        }

        private void OnDestroy() => Detach();
    }
}
