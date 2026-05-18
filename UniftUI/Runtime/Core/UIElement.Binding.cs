using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UniftUI.Internal;

namespace UniftUI
{
    public abstract partial class UIElement
    {
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

    }
}
