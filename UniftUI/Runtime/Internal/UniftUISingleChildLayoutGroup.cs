using UnityEngine;
using UnityEngine.UI;

namespace UniftUI.Internal
{
    internal sealed class UniftUISingleChildLayoutGroup : LayoutGroup
    {
        [SerializeField] private TextAnchor alignment = TextAnchor.MiddleCenter;
        [SerializeField] private Vector2 visualOffset;

        internal Vector2 VisualOffset => visualOffset;

        public void Configure(RectOffset insets, TextAnchor childAlignment = TextAnchor.MiddleCenter)
        {
            padding = insets ?? new RectOffset();
            alignment = childAlignment;
            SetDirty();
        }

        public void SetVisualOffset(Vector2 swiftOffset)
        {
            visualOffset = swiftOffset;
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
            SetChild(0);
        }

        public override void SetLayoutVertical()
        {
            SetChild(1);
        }

        private void CalculateInput(int axis)
        {
            RectTransform child = FirstChild();
            float min = GetPadding(axis);
            float preferred = GetPadding(axis);
            float flexible = 0f;

            if (child != null)
            {
                min += LayoutUtility.GetMinSize(child, axis);
                preferred += LayoutUtility.GetPreferredSize(child, axis);
                flexible = LayoutUtility.GetFlexibleSize(child, axis);
            }

            SetLayoutInputForAxis(min, Mathf.Max(min, preferred), flexible, axis);
        }

        private void SetChild(int axis)
        {
            RectTransform child = FirstChild();
            if (child == null)
                return;

            float available = Mathf.Max(0f, rectTransform.rect.size[axis] - GetPadding(axis));
            float preferred = Mathf.Max(LayoutUtility.GetMinSize(child, axis), LayoutUtility.GetPreferredSize(child, axis));
            bool fill = LayoutUtility.GetFlexibleSize(child, axis) > 0f;
            float resolvedAvailable = available > 0f ? available : preferred;
            float childSize = fill ? resolvedAvailable : Mathf.Min(preferred, resolvedAvailable);
            float start = axis == 0 ? padding.left : padding.top;
            float offset = start + AlignmentOffset(axis, resolvedAvailable, childSize);
            offset += axis == 0 ? visualOffset.x : -visualOffset.y;
            SetChildAlongAxis(child, axis, offset, childSize);
        }

        private RectTransform FirstChild()
        {
            return rectChildren.Count > 0 ? rectChildren[0] : null;
        }

        private float AlignmentOffset(int axis, float available, float childSize)
        {
            float slack = Mathf.Max(0f, available - childSize);
            if (axis == 0)
            {
                if (alignment == TextAnchor.UpperRight || alignment == TextAnchor.MiddleRight || alignment == TextAnchor.LowerRight)
                    return slack;
                if (alignment == TextAnchor.UpperCenter || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.LowerCenter)
                    return slack * 0.5f;
                return 0f;
            }

            if (alignment == TextAnchor.LowerLeft || alignment == TextAnchor.LowerCenter || alignment == TextAnchor.LowerRight)
                return slack;
            if (alignment == TextAnchor.MiddleLeft || alignment == TextAnchor.MiddleCenter || alignment == TextAnchor.MiddleRight)
                return slack * 0.5f;
            return 0f;
        }

        private int GetPadding(int axis)
        {
            return axis == 0 ? padding.horizontal : padding.vertical;
        }
    }
}
