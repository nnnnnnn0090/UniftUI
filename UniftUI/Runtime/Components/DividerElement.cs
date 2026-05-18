using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>A thin horizontal separator.</summary>
    public class DividerElement : UIElement
    {
        private readonly Color color;
        private readonly float thickness;

        public DividerElement()
            : this(new Color(0f, 0f, 0f, 0.16f), 1f)
        {
        }

        public DividerElement(Color color, float thickness = 1f)
        {
            this.color = color;
            this.thickness = Mathf.Max(0.5f, thickness);
            infiniteWidth = true;
            preferredHeight = this.thickness;
            UIContext.Add(this);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject divider = CreateElementRoot("Divider", parent);
            Image image = AddImage(divider, color, false);

            LayoutElementUtility.Configure(divider, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight, 0f, thickness);
            ApplyAllEffects(divider, image);
            return divider;
        }
    }
}
