using System;
using TMPro;

namespace UniftUI
{
    public static class UIContext
    {
        public static ILayoutContainer Current { get; set; }
        public static TMP_FontAsset DefaultFont { get; private set; }

        public static void SetDefaultFont(TMP_FontAsset font)
        {
            DefaultFont = font;
        }

        public static void Add(UIElement element)
        {
            Current?.AddChild(element);
        }

        internal static IDisposable Push(ILayoutContainer next)
        {
            ILayoutContainer previous = Current;
            Current = next;
            return new ContextScope(previous);
        }

        private sealed class ContextScope : IDisposable
        {
            private ILayoutContainer previous;
            private bool disposed;

            public ContextScope(ILayoutContainer previous)
            {
                this.previous = previous;
            }

            public void Dispose()
            {
                if (disposed)
                    return;

                disposed = true;
                Current = previous;
                previous = null;
            }
        }
    }
}
