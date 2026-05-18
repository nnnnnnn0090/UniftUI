using UnityEngine;
using UnityEngine.UI;

namespace UniftUI.Internal
{
    internal readonly struct ProposedSize
    {
        public readonly float? Width;
        public readonly float? Height;

        public ProposedSize(float? width, float? height)
        {
            Width = width;
            Height = height;
        }

        public static ProposedSize Unspecified => new ProposedSize(null, null);

        public static ProposedSize FromRect(Rect rect)
        {
            return new ProposedSize(rect.width, rect.height);
        }
    }

    internal readonly struct ViewSize
    {
        public readonly float Width;
        public readonly float Height;

        public ViewSize(float width, float height)
        {
            Width = Mathf.Max(0f, width);
            Height = Mathf.Max(0f, height);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(Width, Height);
        }
    }

    internal sealed class LayoutContext
    {
        public ProposedSize Proposal { get; }

        public LayoutContext(ProposedSize proposal)
        {
            Proposal = proposal;
        }
    }

    internal interface LayoutNode
    {
        ViewSize Measure(LayoutContext context);
        void Place(Rect bounds);
    }

    internal readonly struct RenderObject
    {
        public readonly GameObject GameObject;
        public readonly RectTransform RectTransform;

        public RenderObject(GameObject gameObject)
        {
            GameObject = gameObject;
            RectTransform = gameObject != null ? gameObject.GetComponent<RectTransform>() : null;
        }
    }

    internal static class LayoutCore
    {
        public static RectTransform EnsureRectTransform(GameObject gameObject)
        {
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            if (rect == null)
                rect = gameObject.AddComponent<RectTransform>();
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void MarkLayoutDirty(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            MarkLayoutDirty(rect);
        }

        public static void MarkLayoutDirty(RectTransform rect)
        {
            if (rect == null)
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rect);
        }

        public static void ForceRebuildLayout(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            RectTransform rect = gameObject.GetComponent<RectTransform>();
            ForceRebuildLayout(rect);
        }

        public static void ForceRebuildLayout(RectTransform rect)
        {
            if (rect == null)
                return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }

    internal static class ElementHost
    {
        public static GameObject BuildRoot(UIElement element, Canvas canvas)
        {
            if (element == null || canvas == null)
                return null;

            bool fillWidth = element.infiniteWidth || element.preferredWidth < 0f;
            bool fillHeight = element.infiniteHeight || element.preferredHeight < 0f;

            if (fillWidth)
                element.WithInfiniteWidth();
            if (fillHeight)
                element.WithInfiniteHeight();

            GameObject root = element.Build(canvas.transform);
            RectTransform rect = root != null ? root.GetComponent<RectTransform>() : null;
            if (rect != null)
            {
                ConfigureRootRect(rect, fillWidth, fillHeight, element.preferredWidth, element.preferredHeight);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                Canvas.ForceUpdateCanvases();
                SyncTextPreferredLayouts(root);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                Canvas.ForceUpdateCanvases();
            }

            return root;
        }

        private static void SyncTextPreferredLayouts(GameObject root)
        {
            if (root == null)
                return;

            foreach (var sync in root.GetComponentsInChildren<UniftUI.TextPreferredLayoutSync>(true))
                sync.SyncNow();
        }

        private static void ConfigureRootRect(RectTransform rect, bool fillWidth, bool fillHeight, float preferredWidth, float preferredHeight)
        {
            if (rect == null)
                return;

            if (fillWidth && fillHeight)
            {
                LayoutCore.Stretch(rect);
                return;
            }

            rect.anchorMin = new Vector2(fillWidth ? 0f : 0.5f, fillHeight ? 0f : 0.5f);
            rect.anchorMax = new Vector2(fillWidth ? 1f : 0.5f, fillHeight ? 1f : 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(
                fillWidth ? 0f : Mathf.Max(0f, preferredWidth),
                fillHeight ? 0f : Mathf.Max(0f, preferredHeight));

            Vector2 offsetMin = rect.offsetMin;
            Vector2 offsetMax = rect.offsetMax;
            if (fillWidth)
            {
                offsetMin.x = 0f;
                offsetMax.x = 0f;
            }
            if (fillHeight)
            {
                offsetMin.y = 0f;
                offsetMax.y = 0f;
            }
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }

    internal static class LayoutElementUtility
    {
        public static LayoutElement Configure(
            GameObject gameObject,
            float preferredWidth,
            float preferredHeight,
            bool infiniteWidth,
            bool infiniteHeight,
            float defaultPreferredWidth = -1f,
            float defaultPreferredHeight = -1f)
        {
            LayoutElement layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = gameObject.AddComponent<LayoutElement>();

            ConfigureAxis(layoutElement, 0, preferredWidth, infiniteWidth, defaultPreferredWidth);
            ConfigureAxis(layoutElement, 1, preferredHeight, infiniteHeight, defaultPreferredHeight);
            return layoutElement;
        }

        private static void ConfigureAxis(LayoutElement layoutElement, int axis, float preferred, bool infinite, float fallbackPreferred)
        {
            if (infinite)
            {
                Set(layoutElement, axis, 0f, -1f, 1f);
                return;
            }

            if (preferred >= 0f)
            {
                Set(layoutElement, axis, preferred, preferred, 0f);
                return;
            }

            if (fallbackPreferred >= 0f)
            {
                Set(layoutElement, axis, 0f, fallbackPreferred, 0f);
                return;
            }

            Set(layoutElement, axis, -1f, -1f, -1f);
        }

        private static void Set(LayoutElement layoutElement, int axis, float min, float preferred, float flexible)
        {
            if (axis == 0)
            {
                layoutElement.minWidth = min;
                layoutElement.preferredWidth = preferred;
                layoutElement.flexibleWidth = flexible;
            }
            else
            {
                layoutElement.minHeight = min;
                layoutElement.preferredHeight = preferred;
                layoutElement.flexibleHeight = flexible;
            }
        }
    }
}
