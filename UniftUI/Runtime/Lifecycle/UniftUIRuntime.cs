using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// UniftUI のライフサイクルを管理するシングルトン。
    /// UniftUICleanup の後継。ActionManager への依存なし。
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class UniftUIRuntime : MonoBehaviour
    {
        private static UniftUIRuntime instance;
        private static bool isQuitting;

        private readonly List<Action> shutdownCallbacks = new List<Action>();

        // ── シングルトン ──────────────────────────────────────────────────────

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

        // ── 公開 API ──────────────────────────────────────────────────────────

        /// <summary>シーンアンロード・アプリ終了時に実行するコールバックを登録する。</summary>
        public static void RegisterShutdownCallback(Action action)
        {
            if (action != null) Instance.shutdownCallbacks.Add(action);
        }

        /// <summary>アプリケーションが終了中かどうか。</summary>
        public static bool IsApplicationQuitting() => isQuitting;

        // ── UniftUICleanup 後方互換 ──────────────────────────────────────────────

        /// <summary>後方互換用エイリアス。</summary>
        public static void CleanupResources() => Instance.RunShutdown();

        // ── 内部 ─────────────────────────────────────────────────────────────

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
