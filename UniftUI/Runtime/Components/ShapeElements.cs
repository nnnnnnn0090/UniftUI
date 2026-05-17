using UnityEngine;

namespace UniftUI
{
    /// <summary>A circular filled shape. Use <c>Frame(width: height:)</c> to control its size.</summary>
    public class CircleElement : RectangleElement
    {
        public CircleElement(Color color) : base(color)
        {
            WithWidth(40f);
            WithHeight(40f);
            WithCornerRadius(50f);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject gameObject = base.Build(parent);
            gameObject.name = "Circle";
            return gameObject;
        }
    }

    /// <summary>A capsule-shaped filled shape. Use <c>Frame(width: height:)</c> to control its size.</summary>
    public class CapsuleElement : RectangleElement
    {
        public CapsuleElement(Color color) : base(color)
        {
            WithWidth(80f);
            WithHeight(32f);
            WithCornerRadius(50f);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject gameObject = base.Build(parent);
            gameObject.name = "Capsule";
            return gameObject;
        }
    }

    /// <summary>A rounded rectangular filled shape.</summary>
    public class RoundedRectangleElement : RectangleElement
    {
        public RoundedRectangleElement(float cornerRadius, Color color) : base(color)
        {
            WithCornerRadius(cornerRadius);
        }

        public override GameObject Build(Transform parent)
        {
            GameObject gameObject = base.Build(parent);
            gameObject.name = "RoundedRectangle";
            return gameObject;
        }
    }
}
