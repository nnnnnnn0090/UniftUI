using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniftUI
{
    /// <summary>
    /// UIElement ごとのプロパティバインディングを管理する。
    /// リビルド時は Dispose() して新しいインスタンスに差し替えることで古いバインディングを完全に消去する。
    /// </summary>
    internal sealed class BindingRegistry
    {
        private struct LiveBinding
        {
            public State State;
            public Action UpdateAction;
        }

        private readonly Dictionary<string, LiveBinding> bindings = new Dictionary<string, LiveBinding>();
        private readonly Dictionary<State, List<string>> stateToProps = new Dictionary<State, List<string>>();
        private Dictionary<State, Animation> stateAnimations;

        public ICollection<State> ObservedStates => stateToProps.Keys;

        /// <summary>
        /// プロパティバインディングを登録する。同じ propertyName は上書き。
        /// </summary>
        public void Register(string propertyName, State state, Action updateAction)
        {
            if (state == null || updateAction == null || string.IsNullOrEmpty(propertyName)) return;

            bindings[propertyName] = new LiveBinding { State = state, UpdateAction = updateAction };

            if (!stateToProps.TryGetValue(state, out var list))
            {
                list = new List<string>();
                stateToProps[state] = list;
            }
            if (!list.Contains(propertyName))
                list.Add(propertyName);
        }

        /// <summary>
        /// .animation(_:value:) で登録された State → Animation マッピング
        /// </summary>
        public void SetStateAnimation(State state, Animation anim)
        {
            if (state == null) return;
            if (stateAnimations == null) stateAnimations = new Dictionary<State, Animation>();
            stateAnimations[state] = anim;
        }

        public Animation? AnimationFor(State state)
        {
            if (stateAnimations != null && state != null &&
                stateAnimations.TryGetValue(state, out var anim))
                return anim;
            return null;
        }

        /// <summary>
        /// 全バインディングの更新アクションを呼び出す。
        /// </summary>
        public void ApplyAll()
        {
            foreach (var kvp in bindings)
            {
                try
                {
                    kvp.Value.UpdateAction?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UniftUI] BindingRegistry: error updating '{kvp.Key}': {e.Message}");
                }
            }
        }

        public void Dispose()
        {
            bindings.Clear();
            stateToProps.Clear();
            stateAnimations?.Clear();
        }
    }
}
