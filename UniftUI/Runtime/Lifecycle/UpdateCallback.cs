using UnityEngine;
using System;

namespace UniftUI
{
    /// <summary>Invokes a callback every frame until disabled or too many errors occur.</summary>
    public class UpdateCallback : MonoBehaviour
    {
        private Action callback;
        private bool isInitialized;
        private bool hasError;
        private const int MaxErrorCount = 3;
        private int errorCount;
        private bool updatesEnabled = true;

        public void Initialize(Action callback)
        {
            if (callback == null) return;
            this.callback = callback;
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized || hasError || callback == null || !updatesEnabled)
                return;

            try
            {
                callback.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] UpdateCallback: {e.Message}");
                errorCount++;
                if (errorCount >= MaxErrorCount)
                {
                    hasError = true;
                    Debug.LogWarning($"[UniftUI] UpdateCallback disabled after {errorCount} errors.");
                    callback = null;
                }
            }
        }

        private void OnDestroy() => callback = null;

        public void ResetErrorState()
        {
            errorCount = 0;
            hasError = false;
        }

        public void SetEnabled(bool enabledState) => updatesEnabled = enabledState;
    }
}
