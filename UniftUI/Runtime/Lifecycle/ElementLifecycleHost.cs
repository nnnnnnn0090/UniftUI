using UnityEngine;

namespace UniftUI
{
    /// <summary>
    /// Attached to built <see cref="UIElement"/> GameObjects to detach observers and dispose bindings on destroy.
    /// </summary>
    internal sealed class ElementLifecycleHost : MonoBehaviour
    {
        private UIElement owner;
        private BindingRegistry registry;

        internal void Attach(UIElement element, BindingRegistry bindingRegistry)
        {
            owner = element;
            registry = bindingRegistry;
        }

        private void OnDestroy()
        {
            if (owner == null || owner.IsCurrentBuiltGameObject(gameObject))
                registry?.Dispose();

            owner = null;
            registry = null;
        }
    }
}
