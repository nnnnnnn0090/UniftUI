using UnityEngine;
using System;

namespace UniftUI
{
    public class StateObserver : MonoBehaviour
    {
        private Action rebuildAction;
        private State[] states;
        private bool isDestroyed = false;
        private bool isObserving = false;

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
                if (state != null)
                {
                    state.AddObserver(OnStateChanged);
                }
            }

            isObserving = true;
        }

        private void OnStateChanged()
        {
            if (isDestroyed || this == null || gameObject == null || !gameObject.activeInHierarchy)
                return;
                
            try
            {
                rebuildAction?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"StateObserver: UI再構築中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
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
            RemoveObservers();
        }
        
        private void OnEnable()
        {
            if (!isDestroyed)
                AddObservers();
        }
        
        private void RemoveObservers()
        {
            if (!isObserving || states == null)
                return;

            foreach (var state in states)
            {
                if (state != null)
                {
                    state.RemoveObserver(OnStateChanged);
                }
            }

            isObserving = false;
        }
    }
}
