using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniftUI
{
    public class State
    {
        private readonly List<Action> observers = new List<Action>();
        private static int batchDepth = 0;
        private static readonly HashSet<State> changedStates = new HashSet<State>();

        public IDisposable AddObserver(Action onChanged)
        {
            if (onChanged != null && !observers.Contains(onChanged))
                observers.Add(onChanged);
            return new StateSubscription(this, onChanged);
        }

        public void RemoveObserver(Action onChanged)
        {
            if (onChanged != null)
                observers.Remove(onChanged);
        }

        protected void NotifyObservers()
        {
            if (batchDepth > 0)
            {
                changedStates.Add(this);
                return;
            }
            PerformNotify();
        }

        internal void PerformNotify()
        {
            var copy = observers.ToArray();
            foreach (var observer in copy)
            {
                try
                {
                    observer?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniftUI] State observer error: {e.Message}\n{e.StackTrace}");
                    observers.Remove(observer);
                }
            }
        }

        public static void BeginBatchUpdate()
        {
            batchDepth++;
        }

        // Drain loop: handles re-entrant State changes triggered during notification.
        public static void EndBatchUpdate()
        {
            if (batchDepth == 0)
            {
                Debug.LogWarning("[UniftUI] EndBatchUpdate called without matching BeginBatchUpdate.");
                return;
            }
            if (--batchDepth > 0)
                return;

            while (changedStates.Count > 0)
            {
                var batch = new List<State>(changedStates);
                changedStates.Clear();
                foreach (var state in batch)
                {
                    if (state == null) continue;
                    state.PerformNotify();
                }
            }
        }

        public static IDisposable BatchUpdate()
        {
            BeginBatchUpdate();
            return new BatchUpdateScope();
        }

        private sealed class BatchUpdateScope : IDisposable
        {
            private bool disposed;

            public void Dispose()
            {
                if (disposed) return;
                disposed = true;
                EndBatchUpdate();
            }
        }

        private sealed class StateSubscription : IDisposable
        {
            private State state;
            private Action observer;

            public StateSubscription(State state, Action observer)
            {
                this.state = state;
                this.observer = observer;
            }

            public void Dispose()
            {
                state?.RemoveObserver(observer);
                state = null;
                observer = null;
            }
        }
    }

    public class State<T> : State
    {
        private T value;

        public State(T initialValue)
        {
            value = initialValue;
        }

        public T Value
        {
            get => value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    this.value = value;
                    NotifyObservers();
                }
            }
        }

        public static implicit operator T(State<T> state) => state.Value;

        public override string ToString() => value?.ToString() ?? "null";
    }
}
