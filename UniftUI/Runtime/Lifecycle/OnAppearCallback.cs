using UnityEngine;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace UniftUI
{
    public class OnAppearCallback : MonoBehaviour
    {
        private Action syncCallback;
        private Func<Task> asyncCallback;

        public void Initialize(Action callback)
        {
            syncCallback = callback;
        }

        public void Initialize(Func<Task> callback)
        {
            asyncCallback = callback;
        }

        private void Start()
        {
            StartCoroutine(ExecuteOnNextFrame());
        }

        private IEnumerator ExecuteOnNextFrame()
        {
            yield return null;
            try
            {
                if (asyncCallback != null)
                    _ = RunAsync();
                else
                    syncCallback?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] OnAppearCallback error: {e.Message}\n{e.StackTrace}");
            }
        }

        private async Task RunAsync()
        {
            try { await asyncCallback(); }
            catch (Exception e) { Debug.LogError($"[UniftUI] OnAppearCallback async error: {e.Message}"); }
        }

        private void OnDestroy()
        {
            syncCallback = null;
            asyncCallback = null;
        }
    }
}
