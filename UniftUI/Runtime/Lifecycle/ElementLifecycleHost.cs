using UnityEngine;

namespace UniftUI
{
    /// <summary>
    /// UIElement がビルドした GameObject に追加されるオーナーコンポーネント。
    /// GameObject が破棄されたとき DynamicEffectObserver を確実にデタッチし、
    /// BindingRegistry を Dispose する。
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
