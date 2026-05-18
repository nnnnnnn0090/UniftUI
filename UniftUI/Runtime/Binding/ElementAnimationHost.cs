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
        private readonly StateSubscriptionGroup subscriptions = new StateSubscriptionGroup();

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
            subscriptions.Clear();
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
            State captured = state;
            subscriptions.Subscribe(state, () => OnStateChanged(captured));
        }

        private void OnStateChanged(State changedState)
        {
            if (!this)
            {
                Detach();
                return;
            }

            if (element == null || !isActiveAndEnabled)
                return;

            element.HandleStateChange(changedState);
        }

        private void OnEnable()
        {
            if (element == null)
                return;

            if (subscriptions.Count == 0)
                SubscribeAll();

            element.ApplyDynamicEffects();
        }

        private void OnDisable()
        {
            subscriptions.Clear();
        }

        private void OnDestroy() => Detach();
    }
}
