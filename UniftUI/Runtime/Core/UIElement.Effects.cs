using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UniftUI.Internal;

namespace UniftUI
{
    public abstract partial class UIElement
    {
        /// <summary>Applies opacity, corners, position, rotation, scale, callbacks, and dynamic effects.</summary>
        protected void ApplyAllEffects(GameObject gameObject, Image backgroundImage = null)
        {
            ApplyFrameConstraints(gameObject);
            ApplyOpacity(gameObject);
            ApplyInteraction(gameObject);
            if (backgroundImage != null)
                ApplyRoundedCorners(gameObject, backgroundImage);
            ApplyCustomPosition(gameObject);
            ApplyRotation(gameObject);
            ApplyScale(gameObject);
            ApplyOnAppearCallback(gameObject);
            ApplyUpdateCallback(gameObject);
            ApplyHoverCallback(gameObject);
            SetupDynamicEffects(gameObject);
        }

        protected CanvasGroup ApplyOpacity(GameObject gameObject, bool cancelAnimator = true)
        {
            if (gameObject == null) return null;

            if (cancelAnimator)
            {
                var animator = gameObject.GetComponent<OpacityAnimator>();
                if (animator != null)
                    animator.CancelAnimation();
            }

            float targetAlpha = hidden ? 0f : Mathf.Clamp01(opacity);
            var cg = gameObject.GetComponent<CanvasGroup>();
            if (targetAlpha >= 1f)
            {
                if (cg != null) cg.alpha = 1f;
                return cg;
            }

            if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
            cg.alpha = targetAlpha;
            return cg;
        }

        protected CanvasGroup ApplyInteraction(GameObject gameObject)
        {
            if (gameObject == null) return null;

            bool interactable = !disabled && !hidden;
            bool blocksRaycasts = IsInputAllowed();
            CanvasGroup cg = gameObject.GetComponent<CanvasGroup>();

            if (!interactable || !blocksRaycasts || cg != null)
            {
                if (cg == null)
                    cg = gameObject.AddComponent<CanvasGroup>();
                cg.interactable = interactable;
                cg.blocksRaycasts = blocksRaycasts;
            }

            foreach (Selectable selectable in gameObject.GetComponentsInChildren<Selectable>(true))
                selectable.interactable = interactable;

            return cg;
        }

        protected void ApplyRoundedCorners(GameObject gameObject, Image imageComponent)
        {
            if (imageComponent == null) return;
            GameObject target = imageComponent.gameObject != null ? imageComponent.gameObject : gameObject;
            var rc = target.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (cornerRadius == Vector4.zero && rc == null)
                return;

            if (rc == null)
                rc = target.AddComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            rc.r = cornerRadius;
            rc.Validate();
            rc.Refresh();
        }

        protected void ApplyRoundedCorners(GameObject gameObject)
        {
            if (gameObject == null) return;
            var rc = gameObject.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (cornerRadius == Vector4.zero && rc == null)
                return;

            if (rc == null)
                rc = gameObject.AddComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            rc.r = cornerRadius;
            rc.Validate();
            rc.Refresh();
        }

        protected void ApplyBackgroundColor(GameObject gameObject)
        {
            if (gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img == null && backgroundColor != Color.clear)
                img = AddImage(gameObject, backgroundColor);
            if (img != null)
            {
                img.color = backgroundColor;
                ApplyRoundedCorners(gameObject, img);
            }
        }

        protected void ApplyCustomPosition(GameObject gameObject)
        {
            if (!useCustomPosition || gameObject == null) return;
            var le = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(customPosition.x, -customPosition.y);
            }
        }

        protected void ApplySize(GameObject gameObject)
        {
            if (gameObject == null) return;
            var le = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            bool changed = false;
            if (infiniteWidth)
            {
                if (!Mathf.Approximately(le.minWidth, 0f) ||
                    !Mathf.Approximately(le.preferredWidth, -1f) ||
                    !Mathf.Approximately(le.flexibleWidth, 1f))
                {
                    le.minWidth = 0f;
                    le.preferredWidth = -1f;
                    le.flexibleWidth = 1f;
                    changed = true;
                }
            }
            else if (preferredWidth >= 0)
            {
                if (!Mathf.Approximately(le.preferredWidth, preferredWidth) ||
                    !Mathf.Approximately(le.minWidth, preferredWidth) ||
                    !Mathf.Approximately(le.flexibleWidth, 0f))
                {
                    le.preferredWidth = preferredWidth;
                    le.minWidth = preferredWidth;
                    le.flexibleWidth = 0f;
                    changed = true;
                }
            }

            if (infiniteHeight)
            {
                if (!Mathf.Approximately(le.minHeight, 0f) ||
                    !Mathf.Approximately(le.preferredHeight, -1f) ||
                    !Mathf.Approximately(le.flexibleHeight, 1f))
                {
                    le.minHeight = 0f;
                    le.preferredHeight = -1f;
                    le.flexibleHeight = 1f;
                    changed = true;
                }
            }
            else if (preferredHeight >= 0)
            {
                if (!Mathf.Approximately(le.preferredHeight, preferredHeight) ||
                    !Mathf.Approximately(le.minHeight, preferredHeight) ||
                    !Mathf.Approximately(le.flexibleHeight, 0f))
                {
                    le.preferredHeight = preferredHeight;
                    le.minHeight = preferredHeight;
                    le.flexibleHeight = 0f;
                    changed = true;
                }
            }

            if (changed)
                LayoutCore.ForceRebuildLayout(gameObject);
        }

        protected static void ConfigureSelectableColors(Selectable selectable, Color normalColor,
            Color? highlightedColor = null, Color? pressedColor = null, Color? selectedColor = null,
            Color? disabledColor = null)
        {
            if (selectable == null)
                return;

            ColorBlock colors = selectable.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightedColor ?? AdjustSelectableColor(normalColor, 1.08f);
            colors.pressedColor = pressedColor ?? AdjustSelectableColor(normalColor, 0.88f);
            colors.selectedColor = selectedColor ?? colors.highlightedColor;
            colors.disabledColor = disabledColor ?? new Color(normalColor.r, normalColor.g, normalColor.b, normalColor.a * 0.5f);
            colors.colorMultiplier = 1f;
            selectable.colors = colors;
        }

        protected static Color AdjustSelectableColor(Color color, float multiplier)
        {
            return UniftUIColors.ScaleRgb(color, multiplier);
        }

        internal static GameObject EnsureControlHitArea(GameObject parent, ref GameObject hitAreaObject,
            string name, ControlHitTarget target)
        {
            if (parent == null)
                return hitAreaObject;

            if (hitAreaObject == null)
            {
                hitAreaObject = CreateFullStretchChild(
                    string.IsNullOrEmpty(name) ? "ControlHitArea" : name,
                    parent.transform);

                LayoutElement layoutElement = hitAreaObject.AddComponent<LayoutElement>();
                layoutElement.ignoreLayout = true;

                AddImage(hitAreaObject, new Color(1f, 1f, 1f, 0f));
            }

            ControlHitProxy proxy = hitAreaObject.GetComponent<ControlHitProxy>();
            if (proxy == null)
                proxy = hitAreaObject.AddComponent<ControlHitProxy>();
            proxy.Initialize(target);

            hitAreaObject.transform.SetAsLastSibling();
            return hitAreaObject;
        }

        protected static Image EnsureControlHitProxy(GameObject gameObject, Image hitImage, UIElement content)
        {
            if (gameObject == null || !TryGetSingleControlHitTarget(content, out ControlHitTarget target))
                return hitImage;

            if (hitImage == null)
            {
                hitImage = gameObject.GetComponent<Image>();
                if (hitImage == null)
                    hitImage = AddImage(gameObject, new Color(1f, 1f, 1f, 0f));
                else
                    hitImage.color = new Color(1f, 1f, 1f, 0f);
            }

            hitImage.raycastTarget = true;

            ControlHitProxy proxy = gameObject.GetComponent<ControlHitProxy>();
            if (proxy == null)
                proxy = gameObject.AddComponent<ControlHitProxy>();
            proxy.Initialize(target);

            return hitImage;
        }

        private static bool TryGetSingleControlHitTarget(UIElement element, out ControlHitTarget target)
        {
            target = default(ControlHitTarget);
            if (element == null)
                return false;

            if (element is IControlHitTargetSource source)
                return source.TryGetControlHitTarget(out target);

            if (!(element is ILayoutContainer container))
                return false;

            UIElement onlyChild = null;
            foreach (UIElement child in container.GetChildren())
            {
                if (child == null)
                    continue;
                if (onlyChild != null)
                    return false;
                onlyChild = child;
            }

            return onlyChild != null && TryGetSingleControlHitTarget(onlyChild, out target);
        }

        protected void ApplyFrameConstraints(GameObject gameObject)
        {
            if (gameObject == null) return;
            var le = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();

            if (minimumWidth >= 0f && le.minWidth < minimumWidth)
                le.minWidth = minimumWidth;
            if (minimumHeight >= 0f && le.minHeight < minimumHeight)
                le.minHeight = minimumHeight;
            if (layoutPriority.HasValue)
                le.layoutPriority = Mathf.RoundToInt(layoutPriority.Value);
        }

        protected void ApplyPadding(GameObject gameObject)
        {
            if (padding == null || gameObject == null) return;

            var vlg = gameObject.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.padding = padding;
                return;
            }

            var hlg = gameObject.GetComponent<HorizontalLayoutGroup>();
            if (hlg != null)
            {
                hlg.padding = padding;
                return;
            }

            var glg = gameObject.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                glg.padding = padding;
                return;
            }

            var stack = gameObject.GetComponent<UniftUIStackLayoutGroup>();
            if (stack != null)
            {
                stack.padding = padding;
                LayoutCore.MarkLayoutDirty(stack.gameObject);
                return;
            }

            var zstack = gameObject.GetComponent<UniftUIZStackLayoutGroup>();
            if (zstack != null)
            {
                zstack.padding = padding;
                LayoutCore.MarkLayoutDirty(zstack.gameObject);
                return;
            }

            var single = gameObject.GetComponent<UniftUISingleChildLayoutGroup>();
            if (single != null)
            {
                single.padding = padding;
                LayoutCore.MarkLayoutDirty(single.gameObject);
            }
        }

        protected void ApplyRotation(GameObject gameObject)
        {
            if (gameObject == null) return;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                var q = Quaternion.Euler(rotationEffectEuler);
                if (rect.localRotation != q)
                    rect.localRotation = q;
            }
        }

        protected void ApplyScale(GameObject gameObject)
        {
            if (gameObject == null) return;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (!Mathf.Approximately(rect.localScale.x, scaleEffect.x) ||
                    !Mathf.Approximately(rect.localScale.y, scaleEffect.y) ||
                    !Mathf.Approximately(rect.localScale.z, scaleEffect.z))
                    rect.localScale = scaleEffect;
            }
        }

        protected void AnimatePosition(GameObject gameObject, Vector2 target)
        {
            if (gameObject == null) return;
            var le = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect == null) return;
            Vector2 from = rect.anchoredPosition;
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            var animator = BaseAnimator<Vector2>.GetOrReplace<PositionAnimator>(gameObject);
            animator.AnimateTo(from, new Vector2(target.x, -target.y), animationDuration, animationEasing);
            useCustomPosition = true;
            customPosition = target;
        }

        protected void AnimateWidth(GameObject gameObject, float target)
        {
            if (gameObject == null) return;
            float from = ReadCurrentLayoutSize(gameObject, 0, preferredWidth);
            var animator = BaseAnimator<float>.GetOrReplace<LayoutWidthAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            preferredWidth = target;
            infiniteWidth = false;
        }

        protected void AnimateHeight(GameObject gameObject, float target)
        {
            if (gameObject == null) return;
            float from = ReadCurrentLayoutSize(gameObject, 1, preferredHeight);
            var animator = BaseAnimator<float>.GetOrReplace<LayoutHeightAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            preferredHeight = target;
            infiniteHeight = false;
        }

        protected void AnimateBackgroundColor(GameObject gameObject, Color target)
        {
            if (gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img == null) return;
            var animator = BaseAnimator<Color>.GetOrReplace<BackgroundColorAnimator>(gameObject);
            animator.AnimateTo(img.color, target, animationDuration, animationEasing);
            backgroundColor = target;
        }

        protected void AnimateOpacity(GameObject gameObject, float target)
        {
            if (gameObject == null) return;
            float from = opacity;
            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                from = canvasGroup.alpha;
            var animator = BaseAnimator<float>.GetOrReplace<OpacityAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            opacity = target;
        }

        protected void AnimateCornerRadius(GameObject gameObject, Vector4 target)
        {
            if (gameObject == null) return;
            var img = gameObject.GetComponent<Image>();
            if (img == null) return;
            Vector4 from = cornerRadius;
            var roundedCorners = gameObject.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            if (roundedCorners != null)
                from = roundedCorners.r;
            var animator = BaseAnimator<Vector4>.GetOrReplace<CornerRadiusAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            cornerRadius = target;
        }

        protected void AnimateRotation(GameObject gameObject, Vector3 target)
        {
            if (gameObject == null) return;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect == null) return;
            Vector3 from = ReadCurrentEulerNearTarget(rect, target);
            var animator = BaseAnimator<Vector3>.GetOrReplace<RotationAnimator>(gameObject);
            animator.AnimateTo(from, target, animationDuration, animationEasing);
            rotationEffectEuler = target;
        }

        protected void AnimateScale(GameObject gameObject, Vector3 target)
        {
            if (gameObject == null) return;
            var rect = gameObject.GetComponent<RectTransform>();
            if (rect == null) return;
            var animator = BaseAnimator<Vector3>.GetOrReplace<ScaleAnimator>(gameObject);
            animator.AnimateTo(rect.localScale, target, animationDuration, animationEasing);
            scaleEffect = target;
        }

        private static Vector3 ReadCurrentEulerNearTarget(RectTransform rect, Vector3 target)
        {
            Vector3 current = rect.localEulerAngles;
            return new Vector3(
                target.x + Mathf.DeltaAngle(target.x, current.x),
                target.y + Mathf.DeltaAngle(target.y, current.y),
                target.z + Mathf.DeltaAngle(target.z, current.z));
        }

        private static float ReadCurrentLayoutSize(GameObject gameObject, int axis, float fallback)
        {
            var layoutElement = gameObject.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                float preferred = axis == 0 ? layoutElement.preferredWidth : layoutElement.preferredHeight;
                if (preferred >= 0f)
                    return preferred;
            }

            var rect = gameObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                float size = rect.rect.size[axis];
                if (size > 0f)
                    return size;
            }

            return Mathf.Max(0f, fallback);
        }

        internal void CopyFrameFrom(UIElement source)
        {
            if (source == null) return;

            preferredWidth = source.preferredWidth;
            preferredHeight = source.preferredHeight;
            minimumWidth = source.minimumWidth;
            minimumHeight = source.minimumHeight;
            layoutPriority = source.layoutPriority;
            infiniteWidth = source.infiniteWidth;
            infiniteHeight = source.infiniteHeight;

            if (source.frameWidthState != null)
                WithWidth(source.frameWidthState);
            else
                frameWidthState = null;

            if (source.frameHeightState != null)
                WithHeight(source.frameHeightState);
            else
                frameHeightState = null;
        }

        /// <summary>Propagates infinite width to child content (override in layout containers).</summary>
        protected virtual void PropagateInfiniteWidthToContent() { }

        /// <summary>Propagates infinite height to child content (override in layout containers).</summary>
        protected virtual void PropagateInfiniteHeightToContent() { }

        protected static bool ChildMayFillWidth(UIElement child)
        {
            return child != null && child.preferredWidth < 0f;
        }

        protected static bool ChildMayFillHeight(UIElement child)
        {
            return child != null && child.preferredHeight < 0f;
        }

        protected static void DestroyUnityObject(UnityEngine.Object target)
        {
            if (target == null)
                return;

            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }

        protected static void DestroyGameObject(GameObject target)
        {
            DestroyUnityObject(target);
        }

    }
}
