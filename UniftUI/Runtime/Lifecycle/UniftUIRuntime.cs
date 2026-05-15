using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// Application-wide singleton for UniftUI lifecycle (shutdown hooks, texture cleanup).
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class UniftUIRuntime : MonoBehaviour
    {
        private static UniftUIRuntime instance;
        private static bool isQuitting;

        private readonly List<Action> shutdownCallbacks = new List<Action>();

        public static UniftUIRuntime Instance => GetOrCreate();

        private static UniftUIRuntime GetOrCreate()
        {
            if (instance != null) return instance;
            var go = new GameObject("[UniftUIRuntime]");
            instance = go.AddComponent<UniftUIRuntime>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        /// <summary>Registers a callback invoked on scene unload or application quit.</summary>
        public static void RegisterShutdownCallback(Action action)
        {
            if (action != null) Instance.shutdownCallbacks.Add(action);
        }

        /// <summary>Returns whether the application is shutting down.</summary>
        public static bool IsApplicationQuitting() => isQuitting;

        /// <summary>Runs shutdown callbacks and releases tracked resources.</summary>
        public static void CleanupResources() => Instance.RunShutdown();

        private void OnSceneUnloaded(Scene _) => RunShutdown();

        private void OnApplicationQuit()
        {
            isQuitting = true;
            RunShutdown();
        }

        private void RunShutdown()
        {
            foreach (var cb in shutdownCallbacks)
            {
                try { cb(); }
                catch (Exception e) { Debug.LogError($"[UniftUI] UniftUIRuntime shutdown error: {e.Message}"); }
            }
            shutdownCallbacks.Clear();
            TextureTracker.CleanupAllTextures();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneUnloaded -= OnSceneUnloaded;
                instance = null;
            }
        }
    }
}
