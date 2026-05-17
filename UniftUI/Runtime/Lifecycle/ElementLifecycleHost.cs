using UnityEngine;
using System;
using System.Collections.Generic;

namespace UniftUI
{
    /// <summary>
    /// Attached to built <see cref="UIElement"/> GameObjects to detach observers and dispose bindings on destroy.
    /// </summary>
    internal sealed class ElementLifecycleHost : MonoBehaviour
    {
        internal BindingRegistry Registry;
        private readonly List<IDisposable> disposables = new List<IDisposable>();

        internal void AddDisposable(IDisposable disposable)
        {
            if (disposable != null && !disposables.Contains(disposable))
                disposables.Add(disposable);
        }

        private void OnDestroy()
        {
            foreach (var disposable in disposables)
                disposable?.Dispose();
            disposables.Clear();
            Registry?.Dispose();
        }
    }
}
