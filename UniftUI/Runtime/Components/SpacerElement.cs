using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>
    /// Flexible space along the parent stack main axis (<c>minLength</c> is a minimum, not a fixed size);
    /// use <see cref="UIElementExtensions.Frame{T}(T, float?, float?, bool, bool)"/> for a fixed-size gap.
    /// </summary>
    public class SpacerElement : UIElement
    {
        private readonly float minAlongAxis;

        /// <param name="minAlongAxis">Minimum size along the stack main axis (height in VStack, width in HStack).</param>
        public SpacerElement(float minAlongAxis = 0)
        {
            this.minAlongAxis = minAlongAxis;
            UIContext.Add(this);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject spacerObj = CreateElementRoot("Spacer", parent);
            Image background = AddBackgroundImageIfNeeded(spacerObj);

            LayoutElement layoutElement = spacerObj.AddComponent<LayoutElement>();

            bool verticalMain = false;
            bool horizontalMain = false;
            Transform t = parent;
            while (t != null)
            {
                var uniftStack = t.GetComponent<UniftUIStackLayoutGroup>();
                if (uniftStack != null)
                {
                    horizontalMain = uniftStack.Axis == UniftUIStackAxis.Horizontal;
                    verticalMain = uniftStack.Axis == UniftUIStackAxis.Vertical;
                    break;
                }

                bool hasH = t.GetComponent<HorizontalLayoutGroup>() != null;
                bool hasV = t.GetComponent<VerticalLayoutGroup>() != null;
                if (hasH)
                {
                    horizontalMain = true;
                    break;
                }

                if (hasV)
                {
                    verticalMain = true;
                    break;
                }

                t = t.parent;
            }

            if (horizontalMain && !verticalMain)
            {
                infiniteWidth = true;
                infiniteHeight = false;
                layoutElement.minWidth = minAlongAxis;
                layoutElement.flexibleWidth = 10000f;
                layoutElement.minHeight = 0f;
                layoutElement.flexibleHeight = 0f;
            }
            else if (verticalMain && !horizontalMain)
            {
                infiniteWidth = false;
                infiniteHeight = true;
                layoutElement.minWidth = 0f;
                layoutElement.flexibleWidth = 0f;
                layoutElement.minHeight = minAlongAxis;
                layoutElement.flexibleHeight = 10000f;
            }
            else
            {
                infiniteWidth = true;
                infiniteHeight = true;
                layoutElement.minWidth = minAlongAxis;
                layoutElement.minHeight = minAlongAxis;
                layoutElement.flexibleWidth = 10000f;
                layoutElement.flexibleHeight = 10000f;
            }

            if (preferredWidth > 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
            }

            if (preferredHeight > 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
                layoutElement.flexibleHeight = 0;
            }

            ApplyAllEffects(spacerObj, background);

            return spacerObj;
        }
    }
}
