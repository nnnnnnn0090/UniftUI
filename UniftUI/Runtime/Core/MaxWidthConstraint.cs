using UnityEngine;
using UnityEngine.UI;

namespace UniftUI
{
    public class MaxWidthConstraint : MonoBehaviour
    {
        private float maxWidth;

        public void Initialize(float maxWidth)
        {
            this.maxWidth = maxWidth;
            ApplyConstraint();
        }

        private void ApplyConstraint()
        {
            LayoutElement layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.preferredWidth = -1;
            layoutElement.flexibleWidth = 1;
            layoutElement.minWidth = -1;
            
            if (maxWidth > 0)
            {
                layoutElement.preferredWidth = maxWidth;
                layoutElement.flexibleWidth = 0;
            }
        }
    }
}
