using System;
using System.Collections.Generic;

namespace UniftUI
{
    internal sealed class StateSubscriptionGroup : IDisposable
    {
        private readonly HashSet<State> observedStates = new HashSet<State>();
        private readonly List<IDisposable> subscriptions = new List<IDisposable>();

        public int Count => subscriptions.Count;

        public bool Subscribe(State state, Action onChanged)
        {
            if (state == null || onChanged == null || !observedStates.Add(state))
                return false;

            subscriptions.Add(state.AddObserver(onChanged));
            return true;
        }

        public void Clear()
        {
            foreach (IDisposable subscription in subscriptions)
                subscription?.Dispose();

            subscriptions.Clear();
            observedStates.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
