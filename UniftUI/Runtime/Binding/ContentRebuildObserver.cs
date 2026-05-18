using UnityEngine;
using System;

namespace UniftUI
{
    /// <summary>Subscribes to <see cref="State"/> changes and schedules a content rebuild callback.</summary>
    internal sealed class ContentRebuildObserver : MonoBehaviour
    {
        private Action rebuildAction;
        private State[] states;
        private readonly StateSubscriptionGroup subscriptions = new StateSubscriptionGroup();
        private bool isDestroyed = false;
        private bool isObserving = false;
        private bool isDirty = false;

        /// <summary>Starts observing the given states and invokes <paramref name="rebuildAction"/> on change.</summary>
        public void Initialize(State[] states, Action rebuildAction)
        {
            RemoveObservers();

            this.states = states;
            this.rebuildAction = rebuildAction;
            isDestroyed = false;

            AddObservers();
        }

        private void AddObservers()
        {
            if (isObserving || states == null)
                return;

            foreach (var state in states)
            {
                subscriptions.Subscribe(state, OnStateChanged);
            }

            isObserving = true;
        }

        private void OnStateChanged()
        {
            if (isDestroyed || this == null || gameObject == null || !gameObject.activeInHierarchy)
                return;

            isDirty = true;
            if (!Application.isPlaying)
                RebuildIfDirty();
        }

        private void LateUpdate()
        {
            if (!isDirty || isDestroyed || this == null || gameObject == null || !gameObject.activeInHierarchy)
                return;

            RebuildIfDirty();
        }

        private void RebuildIfDirty()
        {
            if (!isDirty || isDestroyed || this == null || gameObject == null || !gameObject.activeInHierarchy)
                return;

            isDirty = false;

            try
            {
                rebuildAction?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] ContentRebuildObserver rebuild error: {e.Message}\n{e.StackTrace}");
                RemoveObservers();
            }
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            RemoveObservers();
        }

        private void OnDisable()
        {
            isDirty = false;
            RemoveObservers();
        }

        private void OnEnable()
        {
            if (!isDestroyed)
                AddObservers();
        }

        private void RemoveObservers()
        {
            if (!isObserving)
                return;

            subscriptions.Clear();
            isObserving = false;
        }
    }
}
