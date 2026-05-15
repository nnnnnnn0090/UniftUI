using System;
using UnityEngine;

namespace UniftUI
{
    /// <summary>
    /// Binds a <see cref="State"/> value change to a UI property update action.
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

        public void UpdateProperty()
        {
            if (!isEnabled || updateAction == null) return;

            try
            {
                updateAction.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UniftUI] PropertyBinding '{propertyName}': {e.Message}");
            }
        }

        public PropertyBinding SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            return this;
        }
    }
}
