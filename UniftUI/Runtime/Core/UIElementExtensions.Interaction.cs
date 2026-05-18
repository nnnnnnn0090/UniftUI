using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

namespace UniftUI
{
    internal static partial class UIElementExtensions
    {
        /// <summary>Disables interaction for this element subtree.</summary>
        public static T Disabled<T>(this T element, bool disabled = true) where T : UIElement
        {
            element.WithDisabled(disabled);
            return element;
        }

        /// <summary>Reactively disables interaction for this element subtree.</summary>
        public static T Disabled<T>(this T element, State<bool> disabled) where T : UIElement
        {
            element.WithDisabled(disabled);
            return element;
        }

        /// <summary>Controls whether this element subtree receives pointer events.</summary>
        public static T AllowsHitTesting<T>(this T element, bool enabled) where T : UIElement
        {
            element.WithAllowsHitTesting(enabled);
            return element;
        }

        /// <summary>Reactively controls whether this element subtree receives pointer events.</summary>
        public static T AllowsHitTesting<T>(this T element, State<bool> enabled) where T : UIElement
        {
            element.WithAllowsHitTesting(enabled);
            return element;
        }

        /// <summary>Runs an action after a state changes. The initial binding pass does not invoke it.</summary>
        public static T OnChange<T>(this T element, State state, Action action) where T : UIElement
        {
            if (state == null || action == null)
                return element;

            bool initialized = false;
            element.AddPropertyBinding(state, () =>
            {
                if (!initialized)
                {
                    initialized = true;
                    return;
                }

                action.Invoke();
            }, "onChange_" + Guid.NewGuid().ToString("N"), BindingKind.ObserveOnly);
            return element;
        }

        /// <summary>Runs an action with the new value after a state changes. The initial binding pass does not invoke it.</summary>
        public static T OnChange<T, TValue>(this T element, State<TValue> state, Action<TValue> action) where T : UIElement
        {
            if (state == null || action == null)
                return element;

            return element.OnChange(state, () => action.Invoke(state.Value));
        }

        /// <summary>Registers a synchronous callback when the view appears.</summary>
        public static T OnAppear<T>(this T element, Action action) where T : UIElement
        {
            return (T)element.WithOnAppear(action);
        }

        /// <summary>Registers an async callback when the view appears.</summary>
        public static T OnAppear<T>(this T element, Func<Task> asyncAction) where T : UIElement
        {
            return (T)element.WithOnAppearAsync(asyncAction);
        }

        /// <summary>Registers a per-frame update callback.</summary>
        public static T Update<T>(this T element, Action action) where T : UIElement
        {
            return (T)element.WithUpdate(action);
        }

        /// <summary>Registers a callback when the pointer enters or exits the view.</summary>
        public static T OnHover<T>(this T element, Action<bool> action) where T : UIElement
        {
            return (T)element.WithOnHover(action);
        }

    }
}
