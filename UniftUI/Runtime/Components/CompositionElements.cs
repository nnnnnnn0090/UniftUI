using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    public enum AspectRatioContentMode
    {
        Fit,
        Fill
    }

    public enum UniftUIClipShape
    {
        Rectangle,
        RoundedRectangle,
        Circle,
        Capsule
    }

    /// <summary>Applies an aspect-ratio layout proposal around a single child.</summary>
    public class AspectRatioElement : UIElement, ILayoutContainer
    {
        private UIElement child;
        private readonly float ratio;
        private readonly AspectRatioContentMode contentMode;

        public AspectRatioElement(UIElement child, float ratio, AspectRatioContentMode contentMode)
        {
            this.child = child;
            this.ratio = Mathf.Max(0.0001f, ratio);
            this.contentMode = contentMode;
            CopyFrameFrom(child);
        }

        public void AddChild(UIElement child)
        {
            this.child = child;
        }

        public void RemoveChild(UIElement child)
        {
            if (ReferenceEquals(this.child, child))
                this.child = null;
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (ReferenceEquals(child, oldChild))
                child = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            if (child != null)
                yield return child;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject root = new GameObject("AspectRatio");
            root.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = root.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            float width = preferredWidth;
            float height = preferredHeight;
            if (width >= 0f && height < 0f)
                height = width / ratio;
            else if (height >= 0f && width < 0f)
                width = height * ratio;

            float defaultWidth = 80f;
            float defaultHeight = defaultWidth / ratio;
            LayoutElementUtility.Configure(root, width, height, infiniteWidth, infiniteHeight, defaultWidth, defaultHeight);

            var layout = root.AddComponent<UniftUISingleChildLayoutGroup>();
            layout.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            if (child != null)
            {
                ApplyInheritedFont(child);
                if (contentMode == AspectRatioContentMode.Fill)
                {
                    child.WithInfiniteWidth();
                    child.WithInfiniteHeight();
                }
                child.Build(root.transform);
            }

            ApplyAllEffects(root, backgroundImage);
            return root;
        }

        public override UIElement WithCornerRadius(float radius)
        {
            base.WithCornerRadius(radius);
            child?.WithCornerRadius(radius);
            return this;
        }

        public override UIElement WithCornerRadius(State<float> radius)
        {
            base.WithCornerRadius(radius);
            child?.WithCornerRadius(radius);
            return this;
        }

        public override UIElement WithCornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            base.WithCornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            child?.WithCornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            return this;
        }

        public override UIElement WithCornerRadius(float radius, RectCorner corners)
        {
            base.WithCornerRadius(radius, corners);
            child?.WithCornerRadius(radius, corners);
            return this;
        }
    }

    /// <summary>Clips a single child to a rectangular or rounded mask.</summary>
    public class ClippedElement : UIElement, ILayoutContainer
    {
        private UIElement child;
        private readonly UniftUIClipShape shape;
        private readonly float radius;

        public ClippedElement(UIElement child, UniftUIClipShape shape = UniftUIClipShape.Rectangle, float radius = 0f)
        {
            this.child = child;
            this.shape = shape;
            this.radius = Mathf.Clamp(radius, 0f, 50f);
            CopyFrameFrom(child);
        }

        public void AddChild(UIElement child)
        {
            this.child = child;
        }

        public void RemoveChild(UIElement child)
        {
            if (ReferenceEquals(this.child, child))
                this.child = null;
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            if (ReferenceEquals(child, oldChild))
                child = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            if (child != null)
                yield return child;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject root = new GameObject(shape == UniftUIClipShape.Rectangle ? "Clipped" : "ClipShape");
            root.transform.SetParent(parent, false);

            Image maskImage = null;
            if (shape == UniftUIClipShape.Rectangle && Mathf.Approximately(radius, 0f))
            {
                root.AddComponent<RectMask2D>();
            }
            else
            {
                maskImage = root.AddComponent<Image>();
                maskImage.color = Color.white;
                maskImage.raycastTarget = false;

                Mask mask = root.AddComponent<Mask>();
                mask.showMaskGraphic = false;

                WithCornerRadius(EffectiveRadius());
                ApplyRoundedCorners(root, maskImage);
            }

            var layout = root.AddComponent<UniftUISingleChildLayoutGroup>();
            layout.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);
            LayoutElementUtility.Configure(root, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            if (child != null)
            {
                ApplyInheritedFont(child);
                child.Build(root.transform);
            }

            ApplyAllEffects(root, maskImage);
            return root;
        }

        private float EffectiveRadius()
        {
            switch (shape)
            {
                case UniftUIClipShape.Circle:
                case UniftUIClipShape.Capsule:
                    return 50f;
                case UniftUIClipShape.RoundedRectangle:
                    return radius > 0f ? radius : 12f;
                default:
                    return radius;
            }
        }
    }

    /// <summary>A vertical stack that defers child creation until build time.</summary>
    public class LazyVStackElement : UIElement, ILayoutContainer
    {
        private readonly Action content;
        private readonly State[] states;
        private readonly float spacing;
        private readonly VStackAlignment alignment;
        private readonly List<UIElement> children = new List<UIElement>();

        public LazyVStackElement(Action content, State[] states = null, float spacing = 8f,
            VStackAlignment alignment = VStackAlignment.Center)
        {
            this.content = content;
            this.states = states;
            this.spacing = spacing;
            this.alignment = alignment;
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
                children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
                children.Remove(child);
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            int index = children.IndexOf(oldChild);
            if (index >= 0)
                children[index] = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject root = new GameObject("LazyVStack");
            root.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = root.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            var layout = root.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Vertical, spacing, alignment, HStackAlignment.Center);
            LayoutElementUtility.Configure(root, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            MaterializeChildren();
            BuildChildren(root.transform);

            if (states != null && states.Length > 0)
                SetupStateObserver(root);

            ApplyAllEffects(root, backgroundImage);
            return root;
        }

        private void SetupStateObserver(GameObject root)
        {
            StateObserver observer = root.AddComponent<StateObserver>();
            observer.Initialize(states, () =>
            {
                foreach (Transform child in root.transform)
                    if (child != null)
                        DestroyGameObject(child.gameObject);

                MaterializeChildren();
                BuildChildren(root.transform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());
            });
        }

        private void MaterializeChildren()
        {
            children.Clear();
            if (content == null)
                return;

            ILayoutContainer parentContext = UIContext.Current;
            try
            {
                UIContext.Current = this;
                content.Invoke();
            }
            finally
            {
                UIContext.Current = parentContext;
            }
        }

        private void BuildChildren(Transform parent)
        {
            foreach (UIElement child in children)
            {
                ApplyInheritedFont(child);
                child.Build(parent);
            }
        }
    }

    /// <summary>A horizontal stack that defers child creation until build time.</summary>
    public class LazyHStackElement : UIElement, ILayoutContainer
    {
        private readonly Action content;
        private readonly State[] states;
        private readonly float spacing;
        private readonly HStackAlignment alignment;
        private readonly List<UIElement> children = new List<UIElement>();

        public LazyHStackElement(Action content, State[] states = null, float spacing = 8f,
            HStackAlignment alignment = HStackAlignment.Center)
        {
            this.content = content;
            this.states = states;
            this.spacing = spacing;
            this.alignment = alignment;
        }

        public void AddChild(UIElement child)
        {
            if (child != null)
                children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
                children.Remove(child);
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            int index = children.IndexOf(oldChild);
            if (index >= 0)
                children[index] = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject root = new GameObject("LazyHStack");
            root.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = root.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            var layout = root.AddComponent<UniftUIStackLayoutGroup>();
            layout.padding = padding ?? new RectOffset(0, 0, 0, 0);
            layout.Configure(UniftUIStackAxis.Horizontal, spacing, VStackAlignment.Center, alignment);
            LayoutElementUtility.Configure(root, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight);

            MaterializeChildren();
            BuildChildren(root.transform);

            if (states != null && states.Length > 0)
                SetupStateObserver(root);

            ApplyAllEffects(root, backgroundImage);
            BaselineRowAligner.AlignIfNeeded(root, alignment);
            return root;
        }

        private void SetupStateObserver(GameObject root)
        {
            StateObserver observer = root.AddComponent<StateObserver>();
            observer.Initialize(states, () =>
            {
                foreach (Transform child in root.transform)
                    if (child != null)
                        DestroyGameObject(child.gameObject);

                MaterializeChildren();
                BuildChildren(root.transform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(root.GetComponent<RectTransform>());
                BaselineRowAligner.AlignIfNeeded(root, alignment);
            });
        }

        private void MaterializeChildren()
        {
            children.Clear();
            if (content == null)
                return;

            ILayoutContainer parentContext = UIContext.Current;
            try
            {
                UIContext.Current = this;
                content.Invoke();
            }
            finally
            {
                UIContext.Current = parentContext;
            }
        }

        private void BuildChildren(Transform parent)
        {
            foreach (UIElement child in children)
            {
                ApplyInheritedFont(child);
                child.Build(parent);
            }
        }
    }

    public readonly struct GeometryProxy
    {
        public GeometryProxy(Vector2 size)
        {
            Size = size;
        }

        public Vector2 Size { get; }

        public Rect Rect => new Rect(Vector2.zero, Size);
    }

    /// <summary>Builds content with the parent geometry available.</summary>
    public class GeometryReaderElement : UIElement, ILayoutContainer
    {
        private readonly Func<GeometryProxy, UIElement> content;
        private readonly List<UIElement> children = new List<UIElement>();

        public GeometryReaderElement(Func<GeometryProxy, UIElement> content)
        {
            this.content = content;
        }

        public void AddChild(UIElement child)
        {
            if (child != null && !children.Contains(child))
                children.Add(child);
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null)
                children.Remove(child);
        }

        public void ReplaceChild(UIElement oldChild, UIElement newChild)
        {
            int index = children.IndexOf(oldChild);
            if (index >= 0)
                children[index] = newChild;
        }

        public IEnumerable<UIElement> GetChildren()
        {
            return children;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject root = new GameObject("GeometryReader");
            root.transform.SetParent(parent, false);

            Image backgroundImage = null;
            if (backgroundColor != Color.clear)
            {
                backgroundImage = root.AddComponent<Image>();
                backgroundImage.color = backgroundColor;
            }

            Vector2 proposedSize = ResolveProposedSize(parent);
            LayoutElementUtility.Configure(
                root,
                preferredWidth,
                preferredHeight,
                infiniteWidth || preferredWidth < 0f,
                infiniteHeight || preferredHeight < 0f,
                proposedSize.x,
                proposedSize.y);

            var layout = root.AddComponent<UniftUISingleChildLayoutGroup>();
            layout.Configure(new RectOffset(0, 0, 0, 0), TextAnchor.MiddleCenter);

            BuildContent(new GeometryProxy(proposedSize));
            foreach (UIElement child in children)
            {
                ApplyInheritedFont(child);
                child.Build(root.transform);
            }

            ApplyAllEffects(root, backgroundImage);
            return root;
        }

        private void BuildContent(GeometryProxy proxy)
        {
            children.Clear();
            if (content == null)
                return;

            ILayoutContainer parentContext = UIContext.Current;
            try
            {
                UIContext.Current = this;
                UIElement returned = content.Invoke(proxy);
                if (returned != null && !children.Contains(returned))
                    children.Add(returned);
            }
            finally
            {
                UIContext.Current = parentContext;
            }
        }

        private Vector2 ResolveProposedSize(Transform parent)
        {
            float width = preferredWidth >= 0f ? preferredWidth : 100f;
            float height = preferredHeight >= 0f ? preferredHeight : 100f;

            RectTransform parentRect = parent as RectTransform;
            if (parentRect != null)
            {
                if (preferredWidth < 0f && parentRect.rect.width > 0f)
                    width = parentRect.rect.width;
                if (preferredHeight < 0f && parentRect.rect.height > 0f)
                    height = parentRect.rect.height;
            }

            return new Vector2(Mathf.Max(0f, width), Mathf.Max(0f, height));
        }
    }
}
