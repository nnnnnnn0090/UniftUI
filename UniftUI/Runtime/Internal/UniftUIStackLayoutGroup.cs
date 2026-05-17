using UnityEngine;
using UnityEngine.UI;

namespace UniftUI.Internal
{
    internal enum UniftUIStackAxis
    {
        Horizontal,
        Vertical
    }

    internal sealed class UniftUIStackLayoutGroup : LayoutGroup
    {
        [SerializeField] private UniftUIStackAxis axis = UniftUIStackAxis.Vertical;
        [SerializeField] private float spacing = 8f;
        [SerializeField] private VStackAlignment verticalStackAlignment = VStackAlignment.Center;
        [SerializeField] private HStackAlignment horizontalStackAlignment = HStackAlignment.Center;

        public UniftUIStackAxis Axis => axis;

        public void Configure(UniftUIStackAxis stackAxis, float childSpacing, VStackAlignment vAlignment, HStackAlignment hAlignment)
        {
            axis = stackAxis;
            spacing = childSpacing;
            verticalStackAlignment = vAlignment;
            horizontalStackAlignment = hAlignment;
            SetDirty();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateInputForAxis(0);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateInputForAxis(1);
        }

        public override void SetLayoutHorizontal()
        {
            if (axis == UniftUIStackAxis.Horizontal)
                SetChildrenOnMainAxis(0);
            else
                SetChildrenOnCrossAxis(0);
        }

        public override void SetLayoutVertical()
        {
            if (axis == UniftUIStackAxis.Vertical)
                SetChildrenOnMainAxis(1);
            else
                SetChildrenOnCrossAxis(1);
        }

        private void CalculateInputForAxis(int layoutAxis)
        {
            bool main = IsMainAxis(layoutAxis);
            float totalMin = 0f;
            float totalPreferred = 0f;
            float totalFlexible = 0f;

            if (main)
            {
                float space = rectChildren.Count > 1 ? spacing * (rectChildren.Count - 1) : 0f;
                totalMin = space + GetPadding(layoutAxis);
                totalPreferred = space + GetPadding(layoutAxis);

                foreach (RectTransform child in rectChildren)
                {
                    totalMin += LayoutUtility.GetMinSize(child, layoutAxis);
                    totalPreferred += Mathf.Max(LayoutUtility.GetPreferredSize(child, layoutAxis), LayoutUtility.GetMinSize(child, layoutAxis));
                    totalFlexible += LayoutUtility.GetFlexibleSize(child, layoutAxis);
                }
            }
            else
            {
                totalMin = GetPadding(layoutAxis);
                totalPreferred = GetPadding(layoutAxis);

                foreach (RectTransform child in rectChildren)
                {
                    totalMin = Mathf.Max(totalMin, LayoutUtility.GetMinSize(child, layoutAxis) + GetPadding(layoutAxis));
                    totalPreferred = Mathf.Max(totalPreferred, LayoutUtility.GetPreferredSize(child, layoutAxis) + GetPadding(layoutAxis));
                    totalFlexible = Mathf.Max(totalFlexible, LayoutUtility.GetFlexibleSize(child, layoutAxis));
                }
            }

            SetLayoutInputForAxis(totalMin, Mathf.Max(totalMin, totalPreferred), totalFlexible, layoutAxis);
        }

        private void SetChildrenOnMainAxis(int layoutAxis)
        {
            float totalPreferred = 0f;
            float totalFlexible = 0f;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                totalPreferred += Preferred(child, layoutAxis);
                totalFlexible += LayoutUtility.GetFlexibleSize(child, layoutAxis);
            }
            totalPreferred += rectChildren.Count > 1 ? spacing * (rectChildren.Count - 1) : 0f;

            float available = Mathf.Max(0f, rectTransform.rect.size[layoutAxis] - GetPadding(layoutAxis));
            if (available <= 0f)
                available = totalPreferred;

            float extra = Mathf.Max(0f, available - totalPreferred);
            float position = GetStartOffsetOnMainAxis(layoutAxis, available, totalPreferred);

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float childSize = Preferred(child, layoutAxis);
                float flexible = LayoutUtility.GetFlexibleSize(child, layoutAxis);
                if (totalFlexible > 0f && flexible > 0f)
                    childSize += extra * (flexible / totalFlexible);

                SetChildAlongAxis(child, layoutAxis, position, childSize);
                position += childSize + spacing;
            }
        }

        private void SetChildrenOnCrossAxis(int layoutAxis)
        {
            float available = Mathf.Max(0f, rectTransform.rect.size[layoutAxis] - GetPadding(layoutAxis));
            float start = layoutAxis == 0 ? padding.left : padding.top;

            foreach (RectTransform child in rectChildren)
            {
                float preferred = Preferred(child, layoutAxis);
                float flexible = LayoutUtility.GetFlexibleSize(child, layoutAxis);
                bool fill = flexible > 0f;
                float resolvedAvailable = available > 0f ? available : preferred;
                float childSize = fill ? resolvedAvailable : Mathf.Min(preferred, resolvedAvailable);
                float offset = start + CrossAxisAlignmentOffset(layoutAxis, resolvedAvailable, childSize);
                SetChildAlongAxis(child, layoutAxis, offset, childSize);
            }
        }

        private float GetStartOffsetOnMainAxis(int layoutAxis, float available, float totalPreferred)
        {
            float start = layoutAxis == 0 ? padding.left : padding.top;
            float slack = Mathf.Max(0f, available - totalPreferred);

            if (axis == UniftUIStackAxis.Horizontal)
                return start;

            return start;
        }

        private float CrossAxisAlignmentOffset(int layoutAxis, float available, float childSize)
        {
            if (axis == UniftUIStackAxis.Vertical && layoutAxis == 0)
            {
                switch (verticalStackAlignment)
                {
                    case VStackAlignment.Leading:
                        return 0f;
                    case VStackAlignment.Trailing:
                        return Mathf.Max(0f, available - childSize);
                    default:
                        return Mathf.Max(0f, (available - childSize) * 0.5f);
                }
            }

            if (axis == UniftUIStackAxis.Horizontal && layoutAxis == 1)
            {
                switch (horizontalStackAlignment)
                {
                    case HStackAlignment.Top:
                    case HStackAlignment.FirstTextBaseline:
                        return 0f;
                    case HStackAlignment.Bottom:
                    case HStackAlignment.LastTextBaseline:
                        return Mathf.Max(0f, available - childSize);
                    default:
                        return Mathf.Max(0f, (available - childSize) * 0.5f);
                }
            }

            return 0f;
        }

        private bool IsMainAxis(int layoutAxis)
        {
            return (axis == UniftUIStackAxis.Horizontal && layoutAxis == 0) ||
                   (axis == UniftUIStackAxis.Vertical && layoutAxis == 1);
        }

        private int GetPadding(int layoutAxis)
        {
            return layoutAxis == 0 ? padding.horizontal : padding.vertical;
        }

        private static float Preferred(RectTransform child, int layoutAxis)
        {
            float min = LayoutUtility.GetMinSize(child, layoutAxis);
            float preferred = LayoutUtility.GetPreferredSize(child, layoutAxis);
            float resolved = Mathf.Max(min, preferred);
            return Mathf.Max(0f, resolved);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SetDirty();
        }
    }
}
