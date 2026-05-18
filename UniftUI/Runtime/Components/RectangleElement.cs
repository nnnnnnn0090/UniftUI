using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>A filled rectangular shape.</summary>
    public class RectangleElement : UIElement
    {
        private Color fillColor;
        private Image builtImage;

        public RectangleElement(Color color)
        {
            fillColor = color;
            UIContext.Add(this);
        }

        public RectangleElement SetFillColor(Color color)
        {
            fillColor = color;
            if (builtImage != null)
                builtImage.color = fillColor;
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject rectangle = CreateElementRoot("Rectangle", parent);
            builtImage = AddImage(rectangle, fillColor, false);

            LayoutElementUtility.Configure(rectangle, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight, 10f, 10f);
            ApplyAllEffects(rectangle, builtImage);
            return rectangle;
        }
    }
}
