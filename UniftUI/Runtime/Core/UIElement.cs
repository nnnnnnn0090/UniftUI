using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UniftUI
{
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
        internal RectOffset padding = new RectOffset(0, 0, 0, 0);
        internal bool useCustomPosition;
        internal Vector2 customPosition;

        protected Color backgroundColor = Color.clear;
        protected float opacity = 1f;
        protected Vector4 cornerRadius = Vector4.zero;

        internal Vector3 rotationEffectEuler = Vector3.zero;
        internal Vector3 scaleEffect = Vector3.one;

        internal float animationDuration;
        internal bool useAnimation;
        internal AnimationEasing animationEasing = AnimationEasing.Linear;

        internal Dictionary<State, Animation> stateAnimationMap;
        internal Animation? pendingAnimation;

        protected Action onAppearAction;
        protected Func<Task> onAppearAsyncAction;
        protected Action updateAction;

        internal readonly BindingRegistry bindingRegistry = new BindingRegistry();

        /// <summary>Backward-compatible read-only view of observed states (register via <see cref="ObserveState"/>).</summary>
        protected List<State> observedStates
        {
            get
            {
                var list = new List<State>(bindingRegistry.ObservedStates);
                return list;
            }
        }

        protected GameObject builtGameObject;

        /// <summary>Associates a <see cref="State"/> with a property name for dynamic effect updates.</summary>
        protected class StateReference
        {
            public State state;
            public string propertyType;
            public int axis;
            public bool isVector3;

            public StateReference(State state, string propertyType, int axis = -1, bool isVector3 = false)
            {
                this.state = state;
                this.propertyType = propertyType;
                this.axis = axis;
                this.isVector3 = isVector3;
            }
        }

        protected List<StateReference> stateReferences = new List<StateReference>();

        /// <summary>Backward-compatible accessor; bindings are managed by <see cref="bindingRegistry"/>.</summary>
        protected List<PropertyBinding> propertyBindings
        {
            get
            {
                var list = new List<PropertyBinding>();
                return list;
            }
        }

        protected void ObserveState(State state)
        {
            if (state != null)
                bindingRegistry.Register("__observe__" + state.GetHashCode(), state, () => { });
        }

        /// <summary>Registers a state-driven property update and returns a legacy <see cref="PropertyBinding"/> handle.</summary>
        public PropertyBinding AddPropertyBinding(State state, Action updateAction, string propertyName)
        {
            if (state == null || updateAction == null) return null;

            bindingRegistry.Register(propertyName, state, updateAction);

            return new PropertyBinding(state, updateAction, propertyName);
        }

        protected void SetupDynamicEffects(GameObject gameObject)
        {
            if (gameObject == null || bindingRegistry.ObservedStates.Count == 0) return;

            builtGameObject = gameObject;

            var observer = gameObject.GetComponent<DynamicEffectObserver>();
            if (observer == null)
                observer = gameObject.AddComponent<DynamicEffectObserver>();

            observer.Attach(this, bindingRegistry);
        }

        /// <summary>Applies all registered property bindings, honoring any pending animation.</summary>
        public virtual void ApplyDynamicEffects()
        {
            if (builtGameObject == null) return;

            var anim = pendingAnimation;
            pendingAnimation = null;

            if (anim.HasValue)
            {
                bool prevUse = useAnimation;
                float prevDur = animationDuration;
                AnimationEasing prevEase = animationEasing;

                useAnimation = true;
                animationDuration = anim.Value.effectiveDuration;
                animationEasing = anim.Value.easing;

                bindingRegistry.ApplyAll();

                useAnimation = prevUse;
                animationDuration = prevDur;
                animationEasing = prevEase;
            }
            else
            {
                bindingRegistry.ApplyAll();
            }
        }

        /// <summary>Animates this element when <paramref name="value"/> changes.</summary>
        public UIElement Animation(Animation anim, State value)
        {
            if (value == null) return this;
            if (stateAnimationMap == null) stateAnimationMap = new Dictionary<State, Animation>();
            stateAnimationMap[value] = anim;
            bindingRegistry.SetStateAnimation(value, anim);
            ObserveState(value);
            return this;
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

        protected void CleanupAllResources()
        {
            bindingRegistry.Dispose();
            onAppearAction = null;
            onAppearAsyncAction = null;
            updateAction = null;
        }

        protected void CleanupActions() { }

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

        /// <summary>
        /// Builds on a <see cref="Canvas"/> and stretches the root <see cref="RectTransform"/> to fill the canvas,
        /// similar to a root view receiving the maximum proposed size.
        /// </summary>
        public UIElement Build(Canvas canvas)
        {
            GameObject obj = Build(canvas.transform);
            if (obj != null)
            {
                var rect = obj.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = Vector2.one;
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.offsetMin = Vector2.zero;
                    rect.offsetMax = Vector2.zero;

                    var fitter = obj.GetComponent<ContentSizeFitter>();
                    if (fitter != null)
                    {
                        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                        fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                    }

                    var le = obj.GetComponent<LayoutElement>();
                    if (le == null)
                        le = obj.AddComponent<LayoutElement>();
                    le.flexibleWidth = Mathf.Max(le.flexibleWidth, 1f);
                    le.flexibleHeight = Mathf.Max(le.flexibleHeight, 1f);
                }
            }
            return this;
        }

        /// <summary>Allows the element to expand to the maximum proposed width.</summary>
        public virtual UIElement WithInfiniteWidth()
        {
            infiniteWidth = true;
            PropagateInfiniteWidthToContent();
            return this;
        }

        /// <summary>Allows the element to expand to the maximum proposed height.</summary>
        public virtual UIElement WithInfiniteHeight()
        {
            infiniteHeight = true;
            PropagateInfiniteHeightToContent();
            return this;
        }

        /// <summary>Sets a fixed preferred width.</summary>
        public virtual UIElement WithWidth(float width)
        {
            preferredWidth = width;
            return this;
        }

        /// <summary>Sets a reactive preferred width.</summary>
        public virtual UIElement WithWidth(State<float> width)
        {
            preferredWidth = width.Value;
            AddPropertyBinding(width, () => {
                preferredWidth = width.Value;
                if (builtGameObject != null) ApplySize(builtGameObject);
            }, "width");
            return this;
        }

        /// <summary>Sets a fixed preferred height.</summary>
        public virtual UIElement WithHeight(float height)
        {
            preferredHeight = height;
            return this;
        }

        /// <summary>Sets a reactive preferred height.</summary>
        public virtual UIElement WithHeight(State<float> height)
        {
            preferredHeight = height.Value;
            AddPropertyBinding(height, () => {
                preferredHeight = height.Value;
                if (builtGameObject != null) ApplySize(builtGameObject);
            }, "height");
            return this;
        }

        /// <summary>Sets uniform padding on all edges.</summary>
        public virtual UIElement WithPadding(int pad)
        {
            padding = new RectOffset(pad, pad, pad, pad);
            return this;
        }

        /// <summary>Sets reactive uniform padding.</summary>
        public virtual UIElement WithPadding(State<int> pad)
        {
            padding = new RectOffset(pad.Value, pad.Value, pad.Value, pad.Value);
            AddPropertyBinding(pad, () => {
                padding = new RectOffset(pad.Value, pad.Value, pad.Value, pad.Value);
                if (builtGameObject != null) ApplyPadding(builtGameObject);
            }, "padding");
            return this;
        }

        /// <summary>Sets explicit per-edge padding.</summary>
        public virtual UIElement WithPadding(RectOffset pad)
        {
            padding = pad;
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
            }, "position");
            return this;
        }

        /// <summary>Sets reactive X with fixed Y.</summary>
        public virtual UIElement WithPosition(State<float> x, float y)
        {
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
            }, "positionX");
            return this;
        }

        /// <summary>Sets fixed X with reactive Y.</summary>
        public virtual UIElement WithPosition(float x, State<float> y)
        {
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
            }, "positionY");
            return this;
        }

        /// <summary>Sets background color.</summary>
        public virtual UIElement WithBackgroundColor(Color color)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateBackgroundColor(builtGameObject, color);
            else
                backgroundColor = color;
            return this;
        }

        /// <summary>Sets reactive background color.</summary>
        public virtual UIElement WithBackgroundColor(State<Color> color)
        {
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
            }, "backgroundColor");
            return this;
        }

        /// <summary>Sets opacity (0–1).</summary>
        public virtual UIElement WithOpacity(float value)
        {
            value = Mathf.Clamp01(value);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateOpacity(builtGameObject, value);
            else
                opacity = value;
            return this;
        }

        /// <summary>Sets reactive opacity (0–1).</summary>
        public virtual UIElement WithOpacity(State<float> value)
        {
            opacity = Mathf.Clamp01(value.Value);
            AddPropertyBinding(value, () => {
                var v = Mathf.Clamp01(value.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateOpacity(builtGameObject, v);
                else
                {
                    opacity = v;
                    if (builtGameObject != null) ApplyOpacity(builtGameObject);
                }
            }, "opacity");
            return this;
        }

        /// <summary>Sets uniform corner radius (0–50, mapped to UiRoundedCorners scale).</summary>
        public virtual UIElement WithCornerRadius(float radius)
        {
            radius = Mathf.Clamp(radius, 0f, 50f);
            float s = radius * (40f / 50f);
            var newR = new Vector4(s, s, s, s);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateCornerRadius(builtGameObject, newR);
            else
                cornerRadius = newR;
            return this;
        }

        /// <summary>Sets reactive uniform corner radius.</summary>
        public virtual UIElement WithCornerRadius(State<float> radius)
        {
            float s = Mathf.Clamp(radius.Value, 0f, 50f) * (40f / 50f);
            cornerRadius = new Vector4(s, s, s, s);
            AddPropertyBinding(radius, () => {
                float ns = Mathf.Clamp(radius.Value, 0f, 50f) * (40f / 50f);
                var newR = new Vector4(ns, ns, ns, ns);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateCornerRadius(builtGameObject, newR);
                else
                {
                    cornerRadius = newR;
                    if (builtGameObject != null) ApplyRoundedCorners(builtGameObject);
                }
            }, "cornerRadius");
            return this;
        }

        /// <summary>Sets per-corner radius values.</summary>
        public virtual UIElement WithCornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            float f = 40f / 50f;
            var newR = new Vector4(
                Mathf.Clamp(topLeft, 0f, 50f) * f,
                Mathf.Clamp(topRight, 0f, 50f) * f,
                Mathf.Clamp(bottomRight, 0f, 50f) * f,
                Mathf.Clamp(bottomLeft, 0f, 50f) * f);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateCornerRadius(builtGameObject, newR);
            else
                cornerRadius = newR;
            return this;
        }

        /// <summary>Sets corner radius on selected corners only.</summary>
        public virtual UIElement WithCornerRadius(float radius, RectCorner corners)
        {
            radius = Mathf.Clamp(radius, 0f, 50f);
            float s = radius * (40f / 50f);
            var r = cornerRadius;
            if ((corners & RectCorner.TopLeft) != 0)    r.x = s;
            if ((corners & RectCorner.TopRight) != 0)   r.y = s;
            if ((corners & RectCorner.BottomRight) != 0) r.z = s;
            if ((corners & RectCorner.BottomLeft) != 0)  r.w = s;
            cornerRadius = r;
            return this;
        }

        /// <summary>Applies Z-axis rotation in degrees.</summary>
        public virtual UIElement WithRotationEffect(float degrees)
        {
            var newRot = new Vector3(0, 0, degrees);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, newRot);
            else
                rotationEffectEuler = newRot;
            return this;
        }

        /// <summary>Applies rotation with Euler angles.</summary>
        public virtual UIElement WithRotationEffect(float x, float y, float z)
        {
            var newRot = new Vector3(x, y, z);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, newRot);
            else
                rotationEffectEuler = newRot;
            return this;
        }

        /// <summary>Applies rotation from a <see cref="Vector3"/> Euler angles.</summary>
        public virtual UIElement WithRotationEffect(Vector3 euler)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, euler);
            else
                rotationEffectEuler = euler;
            return this;
        }

        /// <summary>Reactive Z-axis rotation in degrees.</summary>
        public virtual UIElement WithRotationEffect(State<float> degrees)
        {
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
            }, "rotation");
            return this;
        }

        /// <summary>Reactive rotation with bound X component.</summary>
        public virtual UIElement WithRotationEffect(State<float> x, float y, float z)
        {
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
            }, "rotationX");
            return this;
        }

        /// <summary>Reactive rotation with bound Y component.</summary>
        public virtual UIElement WithRotationEffect(float x, State<float> y, float z)
        {
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
            }, "rotationY");
            return this;
        }

        /// <summary>Reactive rotation with bound Z component.</summary>
        public virtual UIElement WithRotationEffect(float x, float y, State<float> z)
        {
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
            }, "rotationZ");
            return this;
        }

        /// <summary>Reactive rotation with bound <see cref="Vector3"/> Euler angles.</summary>
        public virtual UIElement WithRotationEffect(State<Vector3> rotation)
        {
            rotationEffectEuler = rotation.Value;
            AddPropertyBinding(rotation, () => {
                rotationEffectEuler = rotation.Value;
                if (builtGameObject != null) ApplyRotation(builtGameObject);
            }, "rotation3d");
            return this;
        }

        /// <summary>Applies uniform scale.</summary>
        public virtual UIElement WithScaleEffect(float scale)
        {
            var newScale = new Vector3(scale, scale, scale);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
                scaleEffect = newScale;
            return this;
        }

        /// <summary>Applies scale on X and Y (Z = 1).</summary>
        public virtual UIElement WithScaleEffect(float x, float y)
        {
            var newScale = new Vector3(x, y, 1f);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
                scaleEffect = newScale;
            return this;
        }

        /// <summary>Applies scale on X, Y, and Z.</summary>
        public virtual UIElement WithScaleEffect(float x, float y, float z)
        {
            var newScale = new Vector3(x, y, z);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
                scaleEffect = newScale;
            return this;
        }

        /// <summary>Applies scale from a <see cref="Vector3"/>.</summary>
        public virtual UIElement WithScaleEffect(Vector3 scale)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, scale);
            else
                scaleEffect = scale;
            return this;
        }

        /// <summary>Reactive uniform scale.</summary>
        public virtual UIElement WithScaleEffect(State<float> scale)
        {
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
            }, "scale");
            return this;
        }

        /// <summary>Reactive scale with bound X component.</summary>
        public virtual UIElement WithScaleEffect(State<float> x, float y)
        {
            scaleEffect = new Vector3(x.Value, y, 1f);
            AddPropertyBinding(x, () => {
                scaleEffect.x = x.Value;
                if (builtGameObject != null) ApplyScale(builtGameObject);
            }, "scaleX");
            return this;
        }

        /// <summary>Reactive scale with bound Y component.</summary>
        public virtual UIElement WithScaleEffect(float x, State<float> y)
        {
            scaleEffect = new Vector3(x, y.Value, 1f);
            AddPropertyBinding(y, () => {
                scaleEffect.y = y.Value;
                if (builtGameObject != null) ApplyScale(builtGameObject);
            }, "scaleY");
            return this;
        }

        /// <summary>Reactive scale with bound <see cref="Vector3"/>.</summary>
        public virtual UIElement WithScaleEffect(State<Vector3> scale)
        {
            scaleEffect = scale.Value;
            AddPropertyBinding(scale, () => {
                scaleEffect = scale.Value;
                if (builtGameObject != null) ApplyScale(builtGameObject);
            }, "scale3d");
            return this;
        }

        /// <summary>Applies opacity, corners, position, rotation, scale, callbacks, and dynamic effects.</summary>
        protected void ApplyAllEffects(GameObject gameObject, Image backgroundImage = null)
        {
            ApplyOpacity(gameObject);
            if (backgroundImage != null)
                ApplyRoundedCorners(gameObject, backgroundImage);
            ApplyCustomPosition(gameObject);
            ApplyRotation(gameObject);
            ApplyScale(gameObject);
            ApplyOnAppearCallback(gameObject);
            ApplyUpdateCallback(gameObject);
            SetupDynamicEffects(gameObject);
        }

        protected CanvasGroup ApplyOpacity(GameObject gameObject)
        {
            if (gameObject == null) return null;
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (opacity >= 1f)
            {
                if (cg != null) cg.alpha = 1f;
                return cg;
            }
            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            if (cg != null) cg.alpha = opacity;
            return cg;
        }

        protected void ApplyRoundedCorners(GameObject gameObject, Image imageComponent)
        {
            if (imageComponent == null) return;
            var rc = gameObject.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (rc == null)
                rc = gameObject.AddComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            rc.r = cornerRadius;
            rc.Validate();
            rc.Refresh();
        }

        protected void ApplyRoundedCorners(GameObject gameObject)
        {
            if (gameObject == null) return;
            var rc = gameObject.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (rc == null)
                rc = gameObject.AddComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            rc.r = cornerRadius;
            rc.Validate();
            rc.Refresh();
        }

        protected void ApplyBackgroundColor(GameObject gameObject)
        {
            if (backgroundColor == Color.clear || gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img != null) img.color = backgroundColor;
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
            if (preferredWidth >= 0 && le.preferredWidth != preferredWidth)
            {
                le.preferredWidth = preferredWidth;
                le.minWidth = preferredWidth;
                changed = true;
            }
            if (preferredHeight >= 0 && le.preferredHeight != preferredHeight)
            {
                le.preferredHeight = preferredHeight;
                le.minHeight = preferredHeight;
                changed = true;
            }
            if (changed)
            {
                var rect = gameObject.GetComponent<RectTransform>();
                if (rect != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        protected void ApplyPadding(GameObject gameObject)
        {
            if (padding == null || gameObject == null) return;
            var vlg = gameObject.GetComponent<VerticalLayoutGroup>();
            if (vlg != null) { vlg.padding = padding; return; }
            var hlg = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null) { hlg.padding = padding; return; }
            var glg = gameObject.GetComponent<GridLayoutGroup>();
            if (glg != null) { glg.padding = padding; return; }
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
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            var animator = BaseAnimator<Vector2>.GetOrReplace<PositionAnimator>(gameObject);
            animator.AnimateTo(new Vector2(customPosition.x, -customPosition.y),
                               new Vector2(target.x, -target.y),
                               animationDuration, animationEasing);
            useCustomPosition = true;
            customPosition = target;
        }

        protected void AnimateBackgroundColor(GameObject gameObject, Color target)
        {
            if (gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img == null) return;
            var animator = BaseAnimator<Color>.GetOrReplace<BackgroundColorAnimator>(gameObject);
            animator.AnimateTo(backgroundColor, target, animationDuration, animationEasing);
            backgroundColor = target;
        }

        protected void AnimateOpacity(GameObject gameObject, float target)
        {
            if (gameObject == null) return;
            var animator = BaseAnimator<float>.GetOrReplace<OpacityAnimator>(gameObject);
            animator.AnimateTo(opacity, target, animationDuration, animationEasing);
            opacity = target;
        }

        protected void AnimateCornerRadius(GameObject gameObject, Vector4 target)
        {
            if (gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img == null) return;
            var animator = BaseAnimator<Vector4>.GetOrReplace<CornerRadiusAnimator>(gameObject);
            animator.AnimateTo(cornerRadius, target, animationDuration, animationEasing);
            cornerRadius = target;
        }

        protected void AnimateRotation(GameObject gameObject, Vector3 target)
        {
            if (gameObject == null) return;
            if (gameObject.GetComponent<RectTransform>() == null) return;
            var animator = BaseAnimator<Vector3>.GetOrReplace<RotationAnimator>(gameObject);
            animator.AnimateTo(rotationEffectEuler, target, animationDuration, animationEasing);
            rotationEffectEuler = target;
        }

        protected void AnimateScale(GameObject gameObject, Vector3 target)
        {
            if (gameObject == null) return;
            if (gameObject.GetComponent<RectTransform>() == null) return;
            var animator = BaseAnimator<Vector3>.GetOrReplace<ScaleAnimator>(gameObject);
            animator.AnimateTo(scaleEffect, target, animationDuration, animationEasing);
            scaleEffect = target;
        }

        /// <summary>Propagates infinite width to child content (override in layout containers).</summary>
        protected virtual void PropagateInfiniteWidthToContent() { }

        /// <summary>Propagates infinite height to child content (override in layout containers).</summary>
        protected virtual void PropagateInfiniteHeightToContent() { }
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
