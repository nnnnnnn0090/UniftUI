using System.Collections.Generic;
using UnityEngine;

namespace UniftUI
{
    internal static class SingleChildContainerUtility
    {
        public static void Add(ref UIElement current, UIElement child, string ownerName)
        {
            if (child == null)
                return;

            if (current == null)
            {
                current = child;
                return;
            }

            Debug.LogWarning($"[UniftUI] {ownerName} can only contain one child.");
        }

        public static void Remove(ref UIElement current, UIElement child)
        {
            if (ReferenceEquals(current, child))
                current = null;
        }

        public static void Replace(ref UIElement current, UIElement oldChild, UIElement newChild)
        {
            if (ReferenceEquals(current, oldChild))
                current = newChild;
        }

        public static IEnumerable<UIElement> Children(UIElement child)
        {
            if (child != null)
                yield return child;
        }

        public static void LogReplaceChildNotFound(string ownerName, UIElement oldChild, UIElement newChild, int childCount)
        {
            Debug.LogWarning(
                $"[UniftUI] {ownerName} ReplaceChild: oldChild not found. Old: {oldChild}, New: {newChild}. Children count: {childCount}");
        }
    }
}
