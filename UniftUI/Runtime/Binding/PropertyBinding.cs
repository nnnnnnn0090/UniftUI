using System;
using UnityEngine;

namespace UniftUI
{
    /// <summary>
    /// State<T>オブジェクトの値変更をUIElement内の特定プロパティの更新にバインドするクラス
    /// </summary>
    public class PropertyBinding
    {
        private State state;
        private Action updateAction;
        private string propertyName;
        private bool isEnabled = true;

        public PropertyBinding(State state, Action updateAction, string propertyName)
        {
            this.state = state;
            this.updateAction = updateAction;
            this.propertyName = propertyName;
        }

        public State State => state;
        public string PropertyName => propertyName;
        public bool IsEnabled { get => isEnabled; set => isEnabled = value; }
        
        public Action UpdateAction
        {
            get => updateAction;
            set => updateAction = value;
        }

        /// <summary>
        /// Stateの現在値を使ってUIプロパティを更新
        /// </summary>
        public void UpdateProperty()
        {
            if (!isEnabled || updateAction == null) return;
            
            try
            {
                updateAction.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"PropertyBinding: '{propertyName}'更新中にエラーが発生: {e.Message}");
            }
        }
        
        /// <summary>
        /// バインディングを有効/無効にする
        /// </summary>
        public PropertyBinding SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            return this;
        }
    }
}
