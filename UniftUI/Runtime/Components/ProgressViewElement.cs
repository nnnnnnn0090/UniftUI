using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>A linear progress indicator bound to a float state.</summary>
    public class ProgressViewElement : UIElement
    {
        private readonly State<float> value;
        private readonly float total;
        private Color tintColor = new Color(0.2f, 0.55f, 1f, 1f);
        private Color trackColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private GameObject builtFillObject;
        private RectTransform builtFillRect;
        private Image builtFillImage;
        private Image builtTrackImage;

        public ProgressViewElement(State<float> value, float total = 1f)
        {
            this.value = value;
            this.total = Mathf.Approximately(total, 0f) ? 1f : total;
            base.WithCornerRadius(4f);

            if (value != null)
            {
                AddPropertyBinding(value, ApplyProgress, "progressValue", BindingKind.Visual);
            }

            UIContext.Add(this);
        }

        public ProgressViewElement SetTintColor(Color color)
        {
            tintColor = color;
            if (builtFillImage != null)
                builtFillImage.color = tintColor;
            return this;
        }

        public ProgressViewElement SetTrackColor(Color color)
        {
            trackColor = color;
            if (builtTrackImage != null)
                builtTrackImage.color = trackColor;
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject root = new GameObject("ProgressView");
            root.transform.SetParent(parent, false);

            Image trackImage = root.AddComponent<Image>();
            trackImage.color = trackColor;
            trackImage.raycastTarget = false;
            builtTrackImage = trackImage;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(root.transform, false);
            builtFillObject = fill;

            RectTransform fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(NormalizedValue(), 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            builtFillRect = fillRect;

            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = tintColor;
            fillImage.raycastTarget = false;
            builtFillImage = fillImage;

            LayoutElementUtility.Configure(root, preferredWidth, preferredHeight, infiniteWidth, infiniteHeight, 120f, 8f);

            ApplyAllEffects(root, trackImage);
            ApplyFillCorners(fill);
            ApplyProgress();

            return root;
        }

        public override UIElement WithCornerRadius(float radius)
        {
            base.WithCornerRadius(radius);
            ApplyFillCorners(builtFillObject);
            return this;
        }

        public override UIElement WithCornerRadius(State<float> radius)
        {
            base.WithCornerRadius(radius);
            if (radius != null)
            {
                AddPropertyBinding(radius, () =>
                {
                    ApplyFillCorners(builtFillObject);
                }, "progressFillCornerRadius", BindingKind.Visual);
            }
            return this;
        }

        public override UIElement WithCornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            base.WithCornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            ApplyFillCorners(builtFillObject);
            return this;
        }

        public override UIElement WithCornerRadius(float radius, RectCorner corners)
        {
            base.WithCornerRadius(radius, corners);
            ApplyFillCorners(builtFillObject);
            return this;
        }

        private void ApplyProgress()
        {
            if (builtFillRect == null)
                return;

            builtFillRect.anchorMax = new Vector2(NormalizedValue(), 1f);
            builtFillRect.offsetMin = Vector2.zero;
            builtFillRect.offsetMax = Vector2.zero;
        }

        private float NormalizedValue()
        {
            float current = value != null ? value.Value : 0f;
            return Mathf.Clamp01(current / total);
        }

        private void ApplyFillCorners(GameObject fill)
        {
            if (fill == null)
                return;

            var rounded = fill.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (rounded == null)
                rounded = fill.AddComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            rounded.r = cornerRadius;
            rounded.Validate();
            rounded.Refresh();
        }
    }
}
