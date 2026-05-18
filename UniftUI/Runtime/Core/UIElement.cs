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
    public abstract class UIElement
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

        protected void MaterializeContent(Action content, IList<UIElement> children)
        {
            if (children == null)
                return;

            children.Clear();
            if (content == null)
                return;

            ILayoutContainer parentContext = UIContext.Current;
            try
            {
                UIContext.Current = this as ILayoutContainer;
                content.Invoke();
            }
            finally
            {
                UIContext.Current = parentContext;
            }
        }

        protected void BuildContentChildren(IList<UIElement> children, Transform parent,
            Action<UIElement> configureChild = null)
        {
            if (children == null || parent == null)
                return;

            foreach (UIElement child in children)
            {
                if (child == null)
                    continue;

                ApplyInheritedFont(child);
                configureChild?.Invoke(child);
                child.Build(parent);
            }
        }

        protected void SetupContentRebuildObserver(
            State[] states,
            GameObject observerObject,
            Transform contentParent,
            IList<UIElement> children,
            Action content,
            string ownerName,
            Action<UIElement> configureChild = null,
            Action afterRebuild = null)
        {
            if (states == null || states.Length == 0 || observerObject == null || contentParent == null)
                return;

            ContentRebuildObserver observer = observerObject.AddComponent<ContentRebuildObserver>();
            observer.Initialize(states, () => RebuildContent(
                observerObject,
                contentParent,
                children,
                content,
                ownerName,
                configureChild,
                afterRebuild));
        }

        protected void RebuildContent(
            GameObject observerObject,
            Transform contentParent,
            IList<UIElement> children,
            Action content,
            string ownerName,
            Action<UIElement> configureChild = null,
            Action afterRebuild = null)
        {
            if (observerObject == null || !observerObject || contentParent == null || !contentParent)
            {
                Debug.LogWarning($"[UniftUI] {ownerName} rebuild skipped: container was destroyed.");
                return;
            }

            try
            {
                ClearBuiltChildren(contentParent);
                MaterializeContent(content, children);
                BuildContentChildren(children, contentParent, configureChild);

                LayoutCore.ForceRebuildLayout(contentParent.gameObject);
                if (observerObject != contentParent.gameObject)
                    LayoutCore.ForceRebuildLayout(observerObject);

                Canvas.ForceUpdateCanvases();
                afterRebuild?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] {ownerName} rebuild error: {e.Message}\n{e.StackTrace}");
            }
        }

        protected void ClearBuiltChildren(Transform parent)
        {
            if (parent == null)
                return;

            List<GameObject> oldChildren = new List<GameObject>();
            foreach (Transform child in parent)
                if (child != null && child.gameObject != null)
                    oldChildren.Add(child.gameObject);

            foreach (GameObject child in oldChildren)
                DestroyGameObject(child);
        }

        protected void ObserveState(State state)
        {
            if (state != null)
                bindingRegistry.Register("__observe__" + state.GetHashCode(), state, () => { }, BindingKind.ObserveOnly);
        }

        internal void AddPropertyBinding(State state, Action updateAction, string propertyName, BindingKind kind)
        {
            if (state == null || updateAction == null) return;

            bindingRegistry.Register(propertyName, state, updateAction, kind);
        }

        protected void SetupDynamicEffects(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            builtGameObject = gameObject;

            var lifecycle = gameObject.GetComponent<ElementLifecycleHost>();
            if (lifecycle == null)
                lifecycle = gameObject.AddComponent<ElementLifecycleHost>();
            lifecycle.Attach(this, bindingRegistry);

            var animationHost = gameObject.GetComponent<ElementAnimationHost>();
            if (animationHost == null)
                animationHost = gameObject.AddComponent<ElementAnimationHost>();
            animationHost.Attach(this);
        }

        internal IEnumerable<State> GetWatchedStates()
        {
            foreach (State state in bindingRegistry.ObservedStates)
                yield return state;

            foreach (State state in animatedStates)
                yield return state;
        }

        /// <summary>Applies registered property bindings, optionally animating with <paramref name="explicitAnimation"/>.</summary>
        public virtual void ApplyDynamicEffects(State changedState = null, Animation? explicitAnimation = null)
        {
            if (builtGameObject == null)
                return;

            Animation? anim = explicitAnimation;
            if (!anim.HasValue && changedState != null)
                anim = ResolveAnimationForState(changedState);

            using (AnimationScope.TryCreate(this, anim))
                ApplyBindings(changedState);
        }

        internal virtual void HandleStateChange(State changedState)
        {
            if (builtGameObject == null)
                return;

            ApplyDynamicEffects(changedState);
        }

        internal bool IsCurrentBuiltGameObject(GameObject gameObject)
        {
            return builtGameObject == gameObject;
        }

        private void ApplyBindings(State changedState)
        {
            if (changedState != null)
                bindingRegistry.ApplyForState(changedState);
            else
                bindingRegistry.ApplyAll();
        }

        internal Animation? ResolveAnimationForState(State changedState)
        {
            if (changedState != null)
            {
                if (stateAnimationMap != null &&
                    stateAnimationMap.TryGetValue(changedState, out var stateAnim))
                    return stateAnim;

                Animation? registryAnimation = bindingRegistry.AnimationFor(changedState);
                if (registryAnimation.HasValue)
                    return registryAnimation;
            }

            return AnimationContext.Current;
        }

        /// <summary>Animates this element when <paramref name="value"/> changes.</summary>
        public UIElement Animation(Animation anim, State value)
        {
            RegisterAnimationForBoundStateInSubtree(anim, value);
            return this;
        }

        internal bool RegisterAnimationForBoundState(Animation anim, State value)
        {
            if (value == null)
                return false;

            animatedStates.Add(value);
            if (stateAnimationMap == null) stateAnimationMap = new Dictionary<State, Animation>();
            stateAnimationMap[value] = anim;
            bindingRegistry.SetStateAnimation(value, anim);
            return true;
        }

        internal bool RegisterAnimationForBoundStateInSubtree(Animation anim, State value)
        {
            bool applied = RegisterAnimationForBoundState(anim, value);

            if (this is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    if (child == null) continue;
                    applied |= child.RegisterAnimationForBoundStateInSubtree(anim, value);
                }
            }

            return applied;
        }

        protected void ApplyVisualBinding(State state, Action apply)
        {
            if (apply == null)
                return;

            using (AnimationScope.TryCreate(this, ResolveAnimationForState(state)))
                apply.Invoke();
        }

        /// <summary>Animates this element when <paramref name="value"/> changes using <see cref="global::UniftUI.Animation.Default"/>.</summary>
        public UIElement Animation(State value) => Animation(global::UniftUI.Animation.Default, value);

        /// <summary>Registers a synchronous callback invoked when the view appears.</summary>
        public UIElement WithOnAppear(Action action)
        {
            onAppearAction = action;
            onAppearAsyncAction = null;
            return this;
        }

        /// <summary>Registers an async callback invoked when the view appears.</summary>
        public UIElement WithOnAppearAsync(Func<Task> asyncAction)
        {
            onAppearAsyncAction = asyncAction;
            onAppearAction = null;
            return this;
        }

        /// <summary>Registers a per-frame update callback.</summary>
        public UIElement WithUpdate(Action action)
        {
            updateAction = action;
            return this;
        }

        /// <summary>Registers a callback for pointer enter and exit.</summary>
        public UIElement WithOnHover(Action<bool> action)
        {
            onHoverAction = action;
            return this;
        }

        protected void ApplyOnAppearCallback(GameObject gameObject)
        {
            if (gameObject == null) return;
            if (onAppearAction != null || onAppearAsyncAction != null)
            {
                var callback = gameObject.AddComponent<OnAppearCallback>();
                if (onAppearAsyncAction != null)
                    callback.Initialize(onAppearAsyncAction);
                else
                    callback.Initialize(onAppearAction);
            }
        }

        protected void ApplyUpdateCallback(GameObject gameObject)
        {
            if (gameObject == null || updateAction == null) return;
            var callback = gameObject.GetComponent<UpdateCallback>();
            if (callback == null)
                callback = gameObject.AddComponent<UpdateCallback>();
            callback.Initialize(updateAction);
        }

        protected void ApplyHoverCallback(GameObject gameObject)
        {
            if (gameObject == null || onHoverAction == null) return;
            var callback = gameObject.GetComponent<HoverCallback>();
            if (callback == null)
                callback = gameObject.AddComponent<HoverCallback>();
            callback.Initialize(onHoverAction);
        }

        protected void CleanupAllResources()
        {
            bindingRegistry.Dispose();
            onAppearAction = null;
            onAppearAsyncAction = null;
            updateAction = null;
            onHoverAction = null;
        }

        internal void SetInheritedFont(TMP_FontAsset font)
        {
            inheritedFontAsset = font;
        }

        protected TMP_FontAsset ResolveFont(TMP_FontAsset explicitFont)
        {
            return explicitFont ?? inheritedFontAsset ?? UIContext.DefaultFont ?? ResolveDefaultTMPFont();
        }

        private static TMP_FontAsset ResolveDefaultTMPFont()
        {
            try
            {
                return TMP_Settings.defaultFontAsset;
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        protected void ApplyInheritedFont(UIElement child)
        {
            TMP_FontAsset font = inheritedFontAsset ?? UIContext.DefaultFont;
            if (font != null)
                child?.Font(font);
        }

        /// <summary>Enables implicit linear animation for subsequent property changes.</summary>
        public UIElement WithAnimation(float duration)
        {
            useAnimation = duration > 0;
            animationDuration = duration > 0 ? duration : 0;
            animationEasing = AnimationEasing.Linear;
            return this;
        }

        /// <summary>Enables implicit animation with the given easing for subsequent property changes.</summary>
        public UIElement WithAnimation(AnimationEasing easing, float duration)
        {
            useAnimation = duration > 0;
            animationDuration = duration > 0 ? duration : 0;
            animationEasing = easing;
            return this;
        }

        /// <summary>Builds this element under <paramref name="parent"/> and returns the root <see cref="GameObject"/>.</summary>
        public virtual GameObject Build(Transform parent)
        {
            builtGameObject = null;
            return null;
        }

        /// <summary>Enables or disables interaction for this element subtree.</summary>
        public virtual UIElement WithDisabled(bool isDisabled)
        {
            disabled = isDisabled;
            if (builtGameObject != null)
                ApplyInteraction(builtGameObject);
            return this;
        }

        /// <summary>Reactively enables or disables interaction for this element subtree.</summary>
        public virtual UIElement WithDisabled(State<bool> isDisabled)
        {
            if (isDisabled == null)
                return this;

            disabled = isDisabled.Value;
            AddPropertyBinding(isDisabled, () =>
            {
                disabled = isDisabled.Value;
                if (builtGameObject != null)
                    ApplyInteraction(builtGameObject);
            }, "disabled", BindingKind.Visual);
            return this;
        }

        /// <summary>Controls whether this element subtree receives pointer events.</summary>
        public virtual UIElement WithAllowsHitTesting(bool allowsHitTesting)
        {
            this.allowsHitTesting = allowsHitTesting;
            if (builtGameObject != null)
                ApplyInteraction(builtGameObject);
            return this;
        }

        /// <summary>Reactively controls whether this element subtree receives pointer events.</summary>
        public virtual UIElement WithAllowsHitTesting(State<bool> allowsHitTesting)
        {
            if (allowsHitTesting == null)
                return this;

            this.allowsHitTesting = allowsHitTesting.Value;
            AddPropertyBinding(allowsHitTesting, () =>
            {
                this.allowsHitTesting = allowsHitTesting.Value;
                if (builtGameObject != null)
                    ApplyInteraction(builtGameObject);
            }, "allowsHitTesting", BindingKind.Visual);
            return this;
        }

        protected bool IsInputAllowed()
        {
            return !disabled && !hidden && allowsHitTesting;
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

        /// <summary>Allows the element to expand to the maximum proposed width.</summary>
        public virtual UIElement WithInfiniteWidth()
        {
            frameWidthState = null;
            infiniteWidth = true;
            PropagateInfiniteWidthToContent();
            if (builtGameObject != null) ApplySize(builtGameObject);
            return this;
        }

        /// <summary>Allows the element to expand to the maximum proposed height.</summary>
        public virtual UIElement WithInfiniteHeight()
        {
            frameHeightState = null;
            infiniteHeight = true;
            PropagateInfiniteHeightToContent();
            if (builtGameObject != null) ApplySize(builtGameObject);
            return this;
        }

        /// <summary>Sets a fixed preferred width.</summary>
        public virtual UIElement WithWidth(float width)
        {
            frameWidthState = null;
            if (useAnimation && builtGameObject != null && animationDuration > 0)
            {
                AnimateWidth(builtGameObject, width);
                return this;
            }

            preferredWidth = width;
            infiniteWidth = false;
            if (builtGameObject != null) ApplySize(builtGameObject);
            return this;
        }

        /// <summary>Sets a reactive preferred width.</summary>
        public virtual UIElement WithWidth(State<float> width)
        {
            if (width == null)
                return this;

            frameWidthState = width;
            preferredWidth = width.Value;
            infiniteWidth = false;
            AddPropertyBinding(width, () => {
                if (!ReferenceEquals(frameWidthState, width))
                    return;

                float target = width.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                {
                    AnimateWidth(builtGameObject, target);
                }
                else
                {
                    preferredWidth = target;
                    infiniteWidth = false;
                    if (builtGameObject != null) ApplySize(builtGameObject);
                }
            }, "width", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets a fixed preferred height.</summary>
        public virtual UIElement WithHeight(float height)
        {
            frameHeightState = null;
            if (useAnimation && builtGameObject != null && animationDuration > 0)
            {
                AnimateHeight(builtGameObject, height);
                return this;
            }

            preferredHeight = height;
            infiniteHeight = false;
            if (builtGameObject != null) ApplySize(builtGameObject);
            return this;
        }

        /// <summary>Sets a reactive preferred height.</summary>
        public virtual UIElement WithHeight(State<float> height)
        {
            if (height == null)
                return this;

            frameHeightState = height;
            preferredHeight = height.Value;
            infiniteHeight = false;
            AddPropertyBinding(height, () => {
                if (!ReferenceEquals(frameHeightState, height))
                    return;

                float target = height.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                {
                    AnimateHeight(builtGameObject, target);
                }
                else
                {
                    preferredHeight = target;
                    infiniteHeight = false;
                    if (builtGameObject != null) ApplySize(builtGameObject);
                }
            }, "height", BindingKind.Layout);
            return this;
        }

        internal virtual UIElement WithFrameConstraints(float? minWidth = null, float? maxWidth = null,
            float? minHeight = null, float? maxHeight = null)
        {
            if (minWidth.HasValue)
                minimumWidth = Mathf.Max(0f, minWidth.Value);
            if (minHeight.HasValue)
                minimumHeight = Mathf.Max(0f, minHeight.Value);

            if (maxWidth.HasValue)
            {
                if (float.IsPositiveInfinity(maxWidth.Value))
                    WithInfiniteWidth();
                else if (preferredWidth < 0f)
                    WithWidth(Mathf.Max(0f, maxWidth.Value));
            }

            if (maxHeight.HasValue)
            {
                if (float.IsPositiveInfinity(maxHeight.Value))
                    WithInfiniteHeight();
                else if (preferredHeight < 0f)
                    WithHeight(Mathf.Max(0f, maxHeight.Value));
            }

            if (builtGameObject != null)
                ApplyFrameConstraints(builtGameObject);

            return this;
        }

        internal virtual UIElement WithLayoutPriority(float priority)
        {
            layoutPriority = priority;
            if (builtGameObject != null)
                ApplyFrameConstraints(builtGameObject);
            return this;
        }

        internal virtual UIElement WithHidden(bool isHidden = true)
        {
            hidden = isHidden;
            if (builtGameObject != null)
            {
                ApplyOpacity(builtGameObject);
                ApplyInteraction(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets uniform padding on all edges.</summary>
        public virtual UIElement WithPadding(int pad)
        {
            padding = new RectOffset(pad, pad, pad, pad);
            if (builtGameObject != null) ApplyPadding(builtGameObject);
            return this;
        }

        /// <summary>Sets reactive uniform padding.</summary>
        public virtual UIElement WithPadding(State<int> pad)
        {
            if (pad == null)
                return this;

            padding = new RectOffset(pad.Value, pad.Value, pad.Value, pad.Value);
            AddPropertyBinding(pad, () => {
                padding = new RectOffset(pad.Value, pad.Value, pad.Value, pad.Value);
                if (builtGameObject != null) ApplyPadding(builtGameObject);
            }, "padding", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets explicit per-edge padding.</summary>
        public virtual UIElement WithPadding(RectOffset pad)
        {
            padding = pad ?? new RectOffset(0, 0, 0, 0);
            if (builtGameObject != null) ApplyPadding(builtGameObject);
            return this;
        }

        /// <summary>Sets absolute position (top-left origin, Y increasing downward).</summary>
        public virtual UIElement WithPosition(float x, float y)
        {
            var newPos = new Vector2(x, y);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimatePosition(builtGameObject, newPos);
            else
            {
                useCustomPosition = true;
                customPosition = newPos;
                if (builtGameObject != null) ApplyCustomPosition(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets reactive absolute position.</summary>
        public virtual UIElement WithPosition(State<Vector2> position)
        {
            if (position == null)
                return this;

            useCustomPosition = true;
            customPosition = position.Value;
            AddPropertyBinding(position, () => {
                var newPos = position.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimatePosition(builtGameObject, newPos);
                else
                {
                    customPosition = newPos;
                    if (builtGameObject != null) ApplyCustomPosition(builtGameObject);
                }
            }, "position", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets reactive X with fixed Y.</summary>
        public virtual UIElement WithPosition(State<float> x, float y)
        {
            if (x == null)
                return this;

            useCustomPosition = true;
            customPosition = new Vector2(x.Value, y);
            AddPropertyBinding(x, () => {
                var newPos = new Vector2(x.Value, customPosition.y);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimatePosition(builtGameObject, newPos);
                else
                {
                    customPosition = newPos;
                    if (builtGameObject != null) ApplyCustomPosition(builtGameObject);
                }
            }, "positionX", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets fixed X with reactive Y.</summary>
        public virtual UIElement WithPosition(float x, State<float> y)
        {
            if (y == null)
                return this;

            useCustomPosition = true;
            customPosition = new Vector2(x, y.Value);
            AddPropertyBinding(y, () => {
                var newPos = new Vector2(customPosition.x, y.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimatePosition(builtGameObject, newPos);
                else
                {
                    customPosition = newPos;
                    if (builtGameObject != null) ApplyCustomPosition(builtGameObject);
                }
            }, "positionY", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets background color.</summary>
        public virtual UIElement WithBackgroundColor(Color color)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateBackgroundColor(builtGameObject, color);
            else
            {
                backgroundColor = color;
                if (builtGameObject != null)
                    ApplyBackgroundColor(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets reactive background color.</summary>
        public virtual UIElement WithBackgroundColor(State<Color> color)
        {
            if (color == null)
                return this;

            backgroundColor = color.Value;
            AddPropertyBinding(color, () => {
                var c = color.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateBackgroundColor(builtGameObject, c);
                else
                {
                    backgroundColor = c;
                    if (builtGameObject != null) ApplyBackgroundColor(builtGameObject);
                }
            }, "backgroundColor", BindingKind.Visual);
            return this;
        }

        /// <summary>Sets opacity (0–1).</summary>
        public virtual UIElement WithOpacity(float value)
        {
            value = Mathf.Clamp01(value);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
            {
                AnimateOpacity(builtGameObject, value);
            }
            else
            {
                opacity = value;
                if (builtGameObject != null) ApplyOpacity(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets reactive opacity (0–1).</summary>
        public virtual UIElement WithOpacity(State<float> value)
        {
            if (value == null)
                return this;

            opacity = Mathf.Clamp01(value.Value);
            AddPropertyBinding(value, () => {
                if (builtGameObject == null)
                    return;

                float v = Mathf.Clamp01(value.Value);
                opacity = v;
                if (useAnimation && animationDuration > 0f)
                    AnimateOpacity(builtGameObject, v);
                else
                    ApplyOpacity(builtGameObject, cancelAnimator: false);
            }, "opacity", BindingKind.Visual);
            return this;
        }

        /// <summary>Sets uniform corner radius.</summary>
        public virtual UIElement WithCornerRadius(float radius)
        {
            radius = Mathf.Clamp(radius, 0f, 50f);
            var newR = new Vector4(radius, radius, radius, radius);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateCornerRadius(builtGameObject, newR);
            else
            {
                cornerRadius = newR;
                if (builtGameObject != null)
                    ApplyRoundedCorners(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets reactive uniform corner radius.</summary>
        public virtual UIElement WithCornerRadius(State<float> radius)
        {
            if (radius == null)
                return this;

            float initialRadius = Mathf.Clamp(radius.Value, 0f, 50f);
            cornerRadius = new Vector4(initialRadius, initialRadius, initialRadius, initialRadius);
            AddPropertyBinding(radius, () => {
                float nextRadius = Mathf.Clamp(radius.Value, 0f, 50f);
                var newR = new Vector4(nextRadius, nextRadius, nextRadius, nextRadius);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateCornerRadius(builtGameObject, newR);
                else
                {
                    cornerRadius = newR;
                    if (builtGameObject != null) ApplyRoundedCorners(builtGameObject);
                }
            }, "cornerRadius", BindingKind.Visual);
            return this;
        }

        /// <summary>Sets per-corner radius values.</summary>
        public virtual UIElement WithCornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            var newR = new Vector4(
                Mathf.Clamp(topLeft, 0f, 50f),
                Mathf.Clamp(topRight, 0f, 50f),
                Mathf.Clamp(bottomRight, 0f, 50f),
                Mathf.Clamp(bottomLeft, 0f, 50f));
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateCornerRadius(builtGameObject, newR);
            else
            {
                cornerRadius = newR;
                if (builtGameObject != null)
                    ApplyRoundedCorners(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets corner radius on selected corners only.</summary>
        public virtual UIElement WithCornerRadius(float radius, RectCorner corners)
        {
            radius = Mathf.Clamp(radius, 0f, 50f);
            var r = cornerRadius;
            if ((corners & RectCorner.TopLeft) != 0)    r.x = radius;
            if ((corners & RectCorner.TopRight) != 0)   r.y = radius;
            if ((corners & RectCorner.BottomRight) != 0) r.z = radius;
            if ((corners & RectCorner.BottomLeft) != 0)  r.w = radius;
            cornerRadius = r;
            if (builtGameObject != null)
                ApplyRoundedCorners(builtGameObject);
            return this;
        }

        /// <summary>Applies Z-axis rotation in degrees.</summary>
        public virtual UIElement WithRotationEffect(float degrees)
        {
            var newRot = new Vector3(0, 0, degrees);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, newRot);
            else
            {
                rotationEffectEuler = newRot;
                if (builtGameObject != null)
                    ApplyRotation(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies rotation with Euler angles.</summary>
        public virtual UIElement WithRotationEffect(float x, float y, float z)
        {
            var newRot = new Vector3(x, y, z);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, newRot);
            else
            {
                rotationEffectEuler = newRot;
                if (builtGameObject != null)
                    ApplyRotation(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies rotation from a <see cref="Vector3"/> Euler angles.</summary>
        public virtual UIElement WithRotationEffect(Vector3 euler)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, euler);
            else
            {
                rotationEffectEuler = euler;
                if (builtGameObject != null)
                    ApplyRotation(builtGameObject);
            }
            return this;
        }

        /// <summary>Reactive Z-axis rotation in degrees.</summary>
        public virtual UIElement WithRotationEffect(State<float> degrees)
        {
            if (degrees == null)
                return this;

            rotationEffectEuler = new Vector3(0, 0, degrees.Value);
            AddPropertyBinding(degrees, () => {
                var newRot = new Vector3(0, 0, degrees.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler = newRot;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotation", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive rotation with bound X component.</summary>
        public virtual UIElement WithRotationEffect(State<float> x, float y, float z)
        {
            if (x == null)
                return this;

            rotationEffectEuler = new Vector3(x.Value, y, z);
            AddPropertyBinding(x, () => {
                var newRot = new Vector3(x.Value, rotationEffectEuler.y, rotationEffectEuler.z);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler.x = x.Value;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotationX", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive rotation with bound Y component.</summary>
        public virtual UIElement WithRotationEffect(float x, State<float> y, float z)
        {
            if (y == null)
                return this;

            rotationEffectEuler = new Vector3(x, y.Value, z);
            AddPropertyBinding(y, () => {
                var newRot = new Vector3(rotationEffectEuler.x, y.Value, rotationEffectEuler.z);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler.y = y.Value;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotationY", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive rotation with bound Z component.</summary>
        public virtual UIElement WithRotationEffect(float x, float y, State<float> z)
        {
            if (z == null)
                return this;

            rotationEffectEuler = new Vector3(x, y, z.Value);
            AddPropertyBinding(z, () => {
                var newRot = new Vector3(rotationEffectEuler.x, rotationEffectEuler.y, z.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler.z = z.Value;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotationZ", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive rotation with bound <see cref="Vector3"/> Euler angles.</summary>
        public virtual UIElement WithRotationEffect(State<Vector3> rotation)
        {
            if (rotation == null)
                return this;

            rotationEffectEuler = rotation.Value;
            AddPropertyBinding(rotation, () => {
                var newRot = rotation.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler = newRot;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotation3d", BindingKind.Visual);
            return this;
        }

        /// <summary>Applies uniform scale.</summary>
        public virtual UIElement WithScaleEffect(float scale)
        {
            var newScale = new Vector3(scale, scale, scale);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
            {
                scaleEffect = newScale;
                if (builtGameObject != null)
                    ApplyScale(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies scale on X and Y (Z = 1).</summary>
        public virtual UIElement WithScaleEffect(float x, float y)
        {
            var newScale = new Vector3(x, y, 1f);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
            {
                scaleEffect = newScale;
                if (builtGameObject != null)
                    ApplyScale(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies scale on X, Y, and Z.</summary>
        public virtual UIElement WithScaleEffect(float x, float y, float z)
        {
            var newScale = new Vector3(x, y, z);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
            {
                scaleEffect = newScale;
                if (builtGameObject != null)
                    ApplyScale(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies scale from a <see cref="Vector3"/>.</summary>
        public virtual UIElement WithScaleEffect(Vector3 scale)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, scale);
            else
            {
                scaleEffect = scale;
                if (builtGameObject != null)
                    ApplyScale(builtGameObject);
            }
            return this;
        }

        /// <summary>Reactive uniform scale.</summary>
        public virtual UIElement WithScaleEffect(State<float> scale)
        {
            if (scale == null)
                return this;

            scaleEffect = new Vector3(scale.Value, scale.Value, scale.Value);
            AddPropertyBinding(scale, () => {
                var newScale = new Vector3(scale.Value, scale.Value, scale.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateScale(builtGameObject, newScale);
                else
                {
                    scaleEffect = newScale;
                    if (builtGameObject != null) ApplyScale(builtGameObject);
                }
            }, "scale", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive scale with bound X component.</summary>
        public virtual UIElement WithScaleEffect(State<float> x, float y)
        {
            if (x == null)
                return this;

            scaleEffect = new Vector3(x.Value, y, 1f);
            AddPropertyBinding(x, () => {
                var newScale = new Vector3(x.Value, y, 1f);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateScale(builtGameObject, newScale);
                else
                {
                    scaleEffect = newScale;
                    if (builtGameObject != null) ApplyScale(builtGameObject);
                }
            }, "scaleX", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive scale with bound Y component.</summary>
        public virtual UIElement WithScaleEffect(float x, State<float> y)
        {
            if (y == null)
                return this;

            scaleEffect = new Vector3(x, y.Value, 1f);
            AddPropertyBinding(y, () => {
                var newScale = new Vector3(x, y.Value, 1f);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateScale(builtGameObject, newScale);
                else
                {
                    scaleEffect = newScale;
                    if (builtGameObject != null) ApplyScale(builtGameObject);
                }
            }, "scaleY", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive scale with bound <see cref="Vector3"/>.</summary>
        public virtual UIElement WithScaleEffect(State<Vector3> scale)
        {
            if (scale == null)
                return this;

            scaleEffect = scale.Value;
            AddPropertyBinding(scale, () => {
                var newScale = scale.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateScale(builtGameObject, newScale);
                else
                {
                    scaleEffect = newScale;
                    if (builtGameObject != null) ApplyScale(builtGameObject);
                }
            }, "scale3d", BindingKind.Visual);
            return this;
        }

        /// <summary>Applies opacity, corners, position, rotation, scale, callbacks, and dynamic effects.</summary>
        protected void ApplyAllEffects(GameObject gameObject, Image backgroundImage = null)
        {
            ApplyFrameConstraints(gameObject);
            ApplyOpacity(gameObject);
            ApplyInteraction(gameObject);
            if (backgroundImage != null)
                ApplyRoundedCorners(gameObject, backgroundImage);
            ApplyCustomPosition(gameObject);
            ApplyRotation(gameObject);
            ApplyScale(gameObject);
            ApplyOnAppearCallback(gameObject);
            ApplyUpdateCallback(gameObject);
            ApplyHoverCallback(gameObject);
            SetupDynamicEffects(gameObject);
        }

        protected CanvasGroup ApplyOpacity(GameObject gameObject, bool cancelAnimator = true)
        {
            if (gameObject == null) return null;

            if (cancelAnimator)
            {
                var animator = gameObject.GetComponent<OpacityAnimator>();
                if (animator != null)
                    animator.CancelAnimation();
            }

            float targetAlpha = hidden ? 0f : Mathf.Clamp01(opacity);
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (targetAlpha >= 1f)
            {
                if (cg != null) cg.alpha = 1f;
                return cg;
            }

            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = targetAlpha;
            return cg;
        }

        protected CanvasGroup ApplyInteraction(GameObject gameObject)
        {
            if (gameObject == null) return null;

            bool interactable = !disabled && !hidden;
            bool blocksRaycasts = IsInputAllowed();
            CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();

            if (!interactable || !blocksRaycasts || cg != null)
            {
                if (cg == null)
                    cg = gameObject.AddComponent<CanvasGroup>();
                cg.interactable = interactable;
                cg.blocksRaycasts = blocksRaycasts;
            }

            foreach (Selectable selectable in gameObject.GetComponentsInChildren<Selectable>(true))
                selectable.interactable = interactable;

            return cg;
        }

        protected void ApplyRoundedCorners(GameObject gameObject, Image imageComponent)
        {
            if (imageComponent == null) return;
            GameObject target = imageComponent.gameObject != null ? imageComponent.gameObject : gameObject;
            var rc = target.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (cornerRadius == Vector4.zero && rc == null)
                return;

            if (rc == null)
                rc = target.AddComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            rc.r = cornerRadius;
            rc.Validate();
            rc.Refresh();
        }

        protected void ApplyRoundedCorners(GameObject gameObject)
        {
            if (gameObject == null) return;
            var rc = gameObject.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (cornerRadius == Vector4.zero && rc == null)
                return;

            if (rc == null)
                rc = gameObject.AddComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            rc.r = cornerRadius;
            rc.Validate();
            rc.Refresh();
        }

        protected void ApplyBackgroundColor(GameObject gameObject)
        {
            if (gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img == null && backgroundColor != Color.clear)
                img = AddImage(gameObject, backgroundColor);
            if (img != null)
            {
                img.color = backgroundColor;
                ApplyRoundedCorners(gameObject, img);
            }
        }

        protected void ApplyCustomPosition(GameObject gameObject)
        {
            if (!useCustomPosition || gameObject == null) return;
            var le = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(customPosition.x, -customPosition.y);
            }
        }

        protected void ApplySize(GameObject gameObject)
        {
            if (gameObject == null) return;
            var le = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            bool changed = false;
            if (infiniteWidth)
            {
                if (!Mathf.Approximately(le.minWidth, 0f) ||
                    !Mathf.Approximately(le.preferredWidth, -1f) ||
                    !Mathf.Approximately(le.flexibleWidth, 1f))
                {
                    le.minWidth = 0f;
                    le.preferredWidth = -1f;
                    le.flexibleWidth = 1f;
                    changed = true;
                }
            }
            else if (preferredWidth >= 0)
            {
                if (!Mathf.Approximately(le.preferredWidth, preferredWidth) ||
                    !Mathf.Approximately(le.minWidth, preferredWidth) ||
                    !Mathf.Approximately(le.flexibleWidth, 0f))
                {
                    le.preferredWidth = preferredWidth;
                    le.minWidth = preferredWidth;
                    le.flexibleWidth = 0f;
                    changed = true;
                }
            }

            if (infiniteHeight)
            {
                if (!Mathf.Approximately(le.minHeight, 0f) ||
                    !Mathf.Approximately(le.preferredHeight, -1f) ||
                    !Mathf.Approximately(le.flexibleHeight, 1f))
                {
                    le.minHeight = 0f;
                    le.preferredHeight = -1f;
                    le.flexibleHeight = 1f;
                    changed = true;
                }
            }
            else if (preferredHeight >= 0)
            {
                if (!Mathf.Approximately(le.preferredHeight, preferredHeight) ||
                    !Mathf.Approximately(le.minHeight, preferredHeight) ||
                    !Mathf.Approximately(le.flexibleHeight, 0f))
                {
                    le.preferredHeight = preferredHeight;
                    le.minHeight = preferredHeight;
                    le.flexibleHeight = 0f;
                    changed = true;
                }
            }

            if (changed)
                LayoutCore.ForceRebuildLayout(gameObject);
        }

        protected static void ConfigureSelectableColors(Selectable selectable, Color normalColor,
            Color? highlightedColor = null, Color? pressedColor = null, Color? selectedColor = null,
            Color? disabledColor = null)
        {
            if (selectable == null)
                return;

            ColorBlock colors = selectable.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightedColor ?? AdjustSelectableColor(normalColor, 1.08f);
            colors.pressedColor = pressedColor ?? AdjustSelectableColor(normalColor, 0.88f);
            colors.selectedColor = selectedColor ?? colors.highlightedColor;
            colors.disabledColor = disabledColor ?? new Color(normalColor.r, normalColor.g, normalColor.b, normalColor.a * 0.5f);
            colors.colorMultiplier = 1f;
            selectable.colors = colors;
        }

        protected static Color AdjustSelectableColor(Color color, float multiplier)
        {
            return UniftUIColors.ScaleRgb(color, multiplier);
        }

        internal static GameObject EnsureControlHitArea(GameObject parent, ref GameObject hitAreaObject,
            string name, ControlHitTarget target)
        {
            if (parent == null)
                return hitAreaObject;

            if (hitAreaObject == null)
            {
                hitAreaObject = CreateFullStretchChild(
                    string.IsNullOrEmpty(name) ? "ControlHitArea" : name,
                    parent.transform);

                LayoutElement layoutElement = hitAreaObject.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                AddImage(hitAreaObject, new Color(1f, 1f, 1f, 0f));
            }

            ControlHitProxy proxy = hitAreaObject.GetComponent<ControlHitProxy>();
            if (proxy == null)
                proxy = hitAreaObject.AddComponent<ControlHitProxy>();
            proxy.Initialize(target);

            hitAreaObject.transform.SetAsLastSibling();
            return hitAreaObject;
        }

        protected static Image EnsureControlHitProxy(GameObject gameObject, Image hitImage, UIElement content)
        {
            if (gameObject == null || !TryGetSingleControlHitTarget(content, out ControlHitTarget target))
                return hitImage;

            if (hitImage == null)
            {
                hitImage = gameObject.GetComponent<Image>();
                if (hitImage == null)
                    hitImage = AddImage(gameObject, new Color(1f, 1f, 1f, 0f));
                else
                    hitImage.color = new Color(1f, 1f, 1f, 0f);
            }

            hitImage.raycastTarget = true;

            ControlHitProxy proxy = gameObject.GetComponent<ControlHitProxy>();
            if (proxy == null)
                proxy = gameObject.AddComponent<ControlHitProxy>();
            proxy.Initialize(target);

            return hitImage;
        }

        private static bool TryGetSingleControlHitTarget(UIElement element, out ControlHitTarget target)
        {
            target = default(ControlHitTarget);
            if (element == null)
                return false;

            if (element is IControlHitTargetSource source)
                return source.TryGetControlHitTarget(out target);

            if (!(element is ILayoutContainer container))
                return false;

            UIElement onlyChild = null;
            foreach (UIElement child in container.GetChildren())
            {
                if (child == null)
                    continue;
                if (onlyChild != null)
                    return false;
                onlyChild = child;
            }

            return onlyChild != null && TryGetSingleControlHitTarget(onlyChild, out target);
        }

        protected void ApplyFrameConstraints(GameObject gameObject)
        {
            if (gameObject == null) return;
            var le = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();

            if (minimumWidth >= 0f && le.minWidth < minimumWidth)
                le.minWidth = minimumWidth;
            if (minimumHeight >= 0f && le.minHeight < minimumHeight)
                le.minHeight = minimumHeight;
            if (layoutPriority.HasValue)
                le.layoutPriority = Mathf.RoundToInt(layoutPriority.Value);
        }

        protected void ApplyPadding(GameObject gameObject)
        {
            if (padding == null || gameObject == null) return;

            var vlg = gameObject.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.padding = padding;
                return;
            }

            var hlg = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.padding = padding;
                return;
            }

            var glg = gameObject.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                glg.padding = padding;
                return;
            }

            var stack = gameObject.GetComponent<UniftUIStackLayoutGroup>();
            if (stack != null)
            {
                stack.padding = padding;
                LayoutCore.MarkLayoutDirty(stack.gameObject);
                return;
            }

            var zstack = gameObject.GetComponent<UniftUIZStackLayoutGroup>();
            if (zstack != null)
            {
                zstack.padding = padding;
                LayoutCore.MarkLayoutDirty(zstack.gameObject);
                return;
            }

            var single = gameObject.GetComponent<UniftUISingleChildLayoutGroup>();
            if (single != null)
            {
                single.padding = padding;
                LayoutCore.MarkLayoutDirty(single.gameObject);
            }
        }

        protected void ApplyRotation(GameObject gameObject)
        {
            if (gameObject == null) return;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                var q = Quaternion.Euler(rotationEffectEuler);
                if (rect.localRotation != q)
                    rect.localRotation = q;
            }
        }

        protected void ApplyScale(GameObject gameObject)
        {
            if (gameObject == null) return;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (!Mathf.Approximately(rect.localScale.x, scaleEffect.x) ||
                    !Mathf.Approximately(rect.localScale.y, scaleEffect.y) ||
                    !Mathf.Approximately(rect.localScale.z, scaleEffect.z))
                    rect.localScale = scaleEffect;
            }
        }

        protected void AnimatePosition(GameObject gameObject, Vector2 target)
        {
            if (gameObject == null) return;
            var le = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect == null) return;
            Vector2 from = rect.anchoredPosition;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            var animator = BaseAnimator<Vector2>.GetOrReplace<PositionAnimator>(gameObject);
            animator.AnimateTo(from, new Vector2(target.x, -target.y), animationDuration, animationEasing);
            useCustomPosition = true;
            customPosition = target;
        }

        protected void AnimateWidth(GameObject gameObject, float target)
        {
            if (gameObject == null) return;
            float from = ReadCurrentLayoutSize(gameObject, 0, preferredWidth);
            var animator = BaseAnimator<float>.GetOrReplace<LayoutWidthAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            preferredWidth = target;
            infiniteWidth = false;
        }

        protected void AnimateHeight(GameObject gameObject, float target)
        {
            if (gameObject == null) return;
            float from = ReadCurrentLayoutSize(gameObject, 1, preferredHeight);
            var animator = BaseAnimator<float>.GetOrReplace<LayoutHeightAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            preferredHeight = target;
            infiniteHeight = false;
        }

        protected void AnimateBackgroundColor(GameObject gameObject, Color target)
        {
            if (gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img == null) return;
            var animator = BaseAnimator<Color>.GetOrReplace<BackgroundColorAnimator>(gameObject);
            animator.AnimateTo(img.color, target, animationDuration, animationEasing);
            backgroundColor = target;
        }

        protected void AnimateOpacity(GameObject gameObject, float target)
        {
            if (gameObject == null) return;
            float from = opacity;
            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                from = canvasGroup.alpha;
            var animator = BaseAnimator<float>.GetOrReplace<OpacityAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            opacity = target;
        }

        protected void AnimateCornerRadius(GameObject gameObject, Vector4 target)
        {
            if (gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img == null) return;
            Vector4 from = cornerRadius;
            var roundedCorners = gameObject.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (roundedCorners != null)
                from = roundedCorners.r;
            var animator = BaseAnimator<Vector4>.GetOrReplace<CornerRadiusAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            cornerRadius = target;
        }

        protected void AnimateRotation(GameObject gameObject, Vector3 target)
        {
            if (gameObject == null) return;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect == null) return;
            Vector3 from = ReadCurrentEulerNearTarget(rect, target);
            var animator = BaseAnimator<Vector3>.GetOrReplace<RotationAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            rotationEffectEuler = target;
        }

        protected void AnimateScale(GameObject gameObject, Vector3 target)
        {
            if (gameObject == null) return;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect == null) return;
            var animator = BaseAnimator<Vector3>.GetOrReplace<ScaleAnimator>(gameObject);
            animator.AnimateTo(rect.localScale, target, animationDuration, animationEasing);
            scaleEffect = target;
        }

        private static Vector3 ReadCurrentEulerNearTarget(RectTransform rect, Vector3 target)
        {
            Vector3 current = rect.localEulerAngles;
            return new Vector3(
                target.x + Mathf.DeltaAngle(target.x, current.x),
                target.y + Mathf.DeltaAngle(target.y, current.y),
                target.z + Mathf.DeltaAngle(target.z, current.z));
        }

        private static float ReadCurrentLayoutSize(GameObject gameObject, int axis, float fallback)
        {
            var layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                float preferred = axis == 0 ? layoutElement.preferredWidth : layoutElement.preferredHeight;
                if (preferred >= 0f)
                    return preferred;
            }

            var rect = gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                float size = rect.rect.size[axis];
                if (size > 0f)
                    return size;
            }

            return Mathf.Max(0f, fallback);
        }

        internal void CopyFrameFrom(UIElement source)
        {
            if (source == null) return;

            preferredWidth = source.preferredWidth;
            preferredHeight = source.preferredHeight;
            minimumWidth = source.minimumWidth;
            minimumHeight = source.minimumHeight;
            layoutPriority = source.layoutPriority;
            infiniteWidth = source.infiniteWidth;
            infiniteHeight = source.infiniteHeight;

            if (source.frameWidthState != null)
                WithWidth(source.frameWidthState);
            else
                frameWidthState = null;

            if (source.frameHeightState != null)
                WithHeight(source.frameHeightState);
            else
                frameHeightState = null;
        }

        /// <summary>Propagates infinite width to child content (override in layout containers).</summary>
        protected virtual void PropagateInfiniteWidthToContent() { }

        /// <summary>Propagates infinite height to child content (override in layout containers).</summary>
        protected virtual void PropagateInfiniteHeightToContent() { }

        protected static bool ChildMayFillWidth(UIElement child)
        {
            return child != null && child.preferredWidth < 0f;
        }

        protected static bool ChildMayFillHeight(UIElement child)
        {
            return child != null && child.preferredHeight < 0f;
        }

        protected static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }

        protected static void DestroyGameObject(GameObject target)
        {
            DestroyUnityObject(target);
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
