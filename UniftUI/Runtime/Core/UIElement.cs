using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UniftUI.Internal;

namespace UniftUI
{
    internal interface IControlHitTargetSource
    {
        bool TryGetControlHitTarget(out ControlHitTarget target);
    }

    internal struct ControlHitTarget
    {
        public ControlHitTarget(Action click, Action<bool> setPressed = null, Action<bool> setHovered = null,
            Func<bool> canReceiveInput = null)
        {
            Click = click;
            SetPressed = setPressed;
            SetHovered = setHovered;
            CanReceiveInput = canReceiveInput;
        }

        public Action Click { get; }
        public Action<bool> SetPressed { get; }
        public Action<bool> SetHovered { get; }
        public Func<bool> CanReceiveInput { get; }

        public bool IsEnabled => CanReceiveInput == null || CanReceiveInput.Invoke();
    }

    /// <summary>
    /// Base class for all declarative UniftUI elements. Subclasses implement <see cref="Build"/>
    /// to produce Unity UI <see cref="GameObject"/> hierarchies.
    /// </summary>
    public abstract partial class UIElement
    {
        internal bool infiniteWidth;
        internal bool infiniteHeight;
        internal float preferredWidth = -1;
        internal float preferredHeight = -1;
        internal float minimumWidth = -1;
        internal float minimumHeight = -1;
        internal float? layoutPriority;
        internal State<float> frameWidthState;
        internal State<float> frameHeightState;
        internal RectOffset padding = new RectOffset(0, 0, 0, 0);
        internal bool useCustomPosition;
        internal Vector2 customPosition;

        protected Color backgroundColor = Color.clear;
        protected float opacity = 1f;
        protected Vector4 cornerRadius = Vector4.zero;

        internal Vector3 rotationEffectEuler = Vector3.zero;
        internal Vector3 scaleEffect = Vector3.one;
        internal bool disabled;
        internal bool hidden;
        internal bool allowsHitTesting = true;

        internal float animationDuration;
        internal bool useAnimation;
        internal AnimationEasing animationEasing = AnimationEasing.Linear;

        internal Dictionary<State, Animation> stateAnimationMap;
        internal readonly HashSet<State> animatedStates = new HashSet<State>();
        internal TMP_FontAsset inheritedFontAsset;

        protected Action onAppearAction;
        protected Func<Task> onAppearAsyncAction;
        protected Action updateAction;
        protected Action<bool> onHoverAction;

        internal readonly BindingRegistry bindingRegistry = new BindingRegistry();

        protected UIElement()
        {
        }

        protected GameObject builtGameObject;

        protected static GameObject CreateChildObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        protected static RectTransform EnsureRectTransform(GameObject gameObject)
        {
            return LayoutCore.EnsureRectTransform(gameObject);
        }

        protected static RectTransform AddFullStretchRect(GameObject gameObject)
        {
            if (gameObject == null)
                return null;

            RectTransform rect = EnsureRectTransform(gameObject);
            LayoutCore.Stretch(rect);
            return rect;
        }

        protected static GameObject CreateFullStretchChild(string name, Transform parent)
        {
            GameObject gameObject = CreateChildObject(name, parent);
            AddFullStretchRect(gameObject);
            return gameObject;
        }

        protected GameObject CreateElementRoot(string name, Transform parent)
        {
            GameObject gameObject = CreateChildObject(name, parent);
            EnsureRectTransform(gameObject);
            return gameObject;
        }

        protected static Image AddImage(GameObject gameObject, Color color, bool raycastTarget = true)
        {
            if (gameObject == null)
                return null;

            Image image = gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        protected Image AddBackgroundImageIfNeeded(GameObject gameObject, bool raycastTarget = true)
        {
            return backgroundColor != Color.clear
                ? AddImage(gameObject, backgroundColor, raycastTarget)
                : null;
        }

        /// <summary>Builds this element under <paramref name="parent"/> and returns the root <see cref="GameObject"/>.</summary>
        public virtual GameObject Build(Transform parent)
        {
            builtGameObject = null;
            return null;
        }

        /// <summary>
        /// Builds on a <see cref="Canvas"/> and stretches the root <see cref="RectTransform"/> to fill the canvas,
        /// similar to a root view receiving the maximum proposed size.
        /// </summary>
        public UIElement Build(Canvas canvas)
        {
            ElementHost.BuildRoot(this, canvas);
            return this;
        }
    }

    /// <summary>Horizontal alignment for vertical stacks.</summary>
    public enum VStackAlignment { Leading, Center, Trailing }

    /// <summary>Vertical alignment for horizontal stacks.</summary>
    public enum HStackAlignment
    {
        Top,
        Center,
        Bottom,
        /// <summary>Aligns children by the first text baseline.</summary>
        FirstTextBaseline,
        /// <summary>Aligns children by the last text baseline.</summary>
        LastTextBaseline
    }
}
