using UnityEngine;
using UnityEngine.UI;

namespace UniftUI.Internal
{
    internal sealed class UniftUIZStackLayoutGroup : LayoutGroup
    {
        [SerializeField] private ZStackAlignment alignment = ZStackAlignment.Center;

        public void Configure(ZStackAlignment stackAlignment, RectOffset insets)
        {
            alignment = stackAlignment;
            padding = insets ?? new RectOffset();
            SetDirty();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateInput(0);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateInput(1);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildren(0);
        }

        public override void SetLayoutVertical()
        {
            SetChildren(1);
        }

        private void CalculateInput(int axis)
        {
            float min = GetPadding(axis);
            float preferred = GetPadding(axis);
            float flexible = 0f;

            foreach (RectTransform child in rectChildren)
            {
                min = Mathf.Max(min, LayoutUtility.GetMinSize(child, axis) + GetPadding(axis));
                preferred = Mathf.Max(preferred, LayoutUtility.GetPreferredSize(child, axis) + GetPadding(axis));
                flexible = Mathf.Max(flexible, LayoutUtility.GetFlexibleSize(child, axis));
            }

            SetLayoutInputForAxis(min, Mathf.Max(min, preferred), flexible, axis);
        }

        private void SetChildren(int axis)
        {
            float available = Mathf.Max(0f, rectTransform.rect.size[axis] - GetPadding(axis));
            float start = axis == 0 ? padding.left : padding.top;

            foreach (RectTransform child in rectChildren)
            {
                float preferred = Mathf.Max(LayoutUtility.GetMinSize(child, axis), LayoutUtility.GetPreferredSize(child, axis));
                bool fill = LayoutUtility.GetFlexibleSize(child, axis) > 0f;
                float resolvedAvailable = available > 0f ? available : preferred;
                float childSize = fill ? resolvedAvailable : Mathf.Min(preferred, resolvedAvailable);
                float offset = start + AlignmentOffset(axis, resolvedAvailable, childSize);
                SetChildAlongAxis(child, axis, offset, childSize);
            }
        }

        private float AlignmentOffset(int axis, float available, float childSize)
        {
            float slack = Mathf.Max(0f, available - childSize);
            if (axis == 0)
            {
                switch (alignment)
                {
                    case ZStackAlignment.TopTrailing:
                    case ZStackAlignment.Trailing:
                    case ZStackAlignment.BottomTrailing:
                        return slack;
                    case ZStackAlignment.Top:
                    case ZStackAlignment.Center:
                    case ZStackAlignment.Bottom:
                        return slack * 0.5f;
                    default:
                        return 0f;
                }
            }

            switch (alignment)
            {
                case ZStackAlignment.BottomLeading:
                case ZStackAlignment.Bottom:
                case ZStackAlignment.BottomTrailing:
                    return slack;
                case ZStackAlignment.Leading:
                case ZStackAlignment.Center:
                case ZStackAlignment.Trailing:
                    return slack * 0.5f;
                default:
                    return 0f;
            }
        }

        private int GetPadding(int axis)
        {
            return axis == 0 ? padding.horizontal : padding.vertical;
        }
    }
}
