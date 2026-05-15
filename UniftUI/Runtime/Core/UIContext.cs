using UnityEngine;
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
            if (Current != null)
            {
                Current.AddChild(element);
            }
            else
            {
                Debug.LogWarning("UIContext.Current is null. Element cannot be added to the hierarchy.");
            }
        }
    }
}
