using UnityEngine;

namespace UniftUI
{
    /// <summary>
    /// Attached to built <see cref="UIElement"/> GameObjects to detach observers and dispose bindings on destroy.
    /// </summary>
    internal sealed class ElementLifecycleHost : MonoBehaviour
    {
        internal DynamicEffectObserver Observer;
        internal BindingRegistry Registry;

        private void OnDestroy()
        {
            Observer?.Detach();
            Registry?.Dispose();
        }
    }
}
