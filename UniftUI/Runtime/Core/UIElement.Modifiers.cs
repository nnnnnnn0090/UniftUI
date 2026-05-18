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
        public UIElement WithAnimation(float duration)
        {
            useAnimation = duration > 0;
            animationDuration = duration > 0 ? duration : 0;
            animationEasing = AnimationEasing.Linear;
            return this;
        }

        /// <summary>Enables implicit animation with the given easing for subsequent property changes.</summary>
        public UIElement WithAnimation(AnimationEasing easing, float duration)
        {
            useAnimation = duration > 0;
            animationDuration = duration > 0 ? duration : 0;
            animationEasing = easing;
            return this;
        }

        public virtual UIElement WithDisabled(bool isDisabled)
        {
            disabled = isDisabled;
            if (builtGameObject != null)
                ApplyInteraction(builtGameObject);
            return this;
        }

        /// <summary>Reactively enables or disables interaction for this element subtree.</summary>
        public virtual UIElement WithDisabled(State<bool> isDisabled)
        {
            if (isDisabled == null)
                return this;

            disabled = isDisabled.Value;
            AddPropertyBinding(isDisabled, () =>
            {
                disabled = isDisabled.Value;
                if (builtGameObject != null)
                    ApplyInteraction(builtGameObject);
            }, "disabled", BindingKind.Visual);
            return this;
        }

        /// <summary>Controls whether this element subtree receives pointer events.</summary>
        public virtual UIElement WithAllowsHitTesting(bool allowsHitTesting)
        {
            this.allowsHitTesting = allowsHitTesting;
            if (builtGameObject != null)
                ApplyInteraction(builtGameObject);
            return this;
        }

        /// <summary>Reactively controls whether this element subtree receives pointer events.</summary>
        public virtual UIElement WithAllowsHitTesting(State<bool> allowsHitTesting)
        {
            if (allowsHitTesting == null)
                return this;

            this.allowsHitTesting = allowsHitTesting.Value;
            AddPropertyBinding(allowsHitTesting, () =>
            {
                this.allowsHitTesting = allowsHitTesting.Value;
                if (builtGameObject != null)
                    ApplyInteraction(builtGameObject);
            }, "allowsHitTesting", BindingKind.Visual);
            return this;
        }

        protected bool IsInputAllowed()
        {
            return !disabled && !hidden && allowsHitTesting;
        }

        public virtual UIElement WithInfiniteWidth()
        {
            frameWidthState = null;
            infiniteWidth = true;
            PropagateInfiniteWidthToContent();
            if (builtGameObject != null) ApplySize(builtGameObject);
            return this;
        }

        /// <summary>Allows the element to expand to the maximum proposed height.</summary>
        public virtual UIElement WithInfiniteHeight()
        {
            frameHeightState = null;
            infiniteHeight = true;
            PropagateInfiniteHeightToContent();
            if (builtGameObject != null) ApplySize(builtGameObject);
            return this;
        }

        /// <summary>Sets a fixed preferred width.</summary>
        public virtual UIElement WithWidth(float width)
        {
            frameWidthState = null;
            if (useAnimation && builtGameObject != null && animationDuration > 0)
            {
                AnimateWidth(builtGameObject, width);
                return this;
            }

            preferredWidth = width;
            infiniteWidth = false;
            if (builtGameObject != null) ApplySize(builtGameObject);
            return this;
        }

        /// <summary>Sets a reactive preferred width.</summary>
        public virtual UIElement WithWidth(State<float> width)
        {
            if (width == null)
                return this;

            frameWidthState = width;
            preferredWidth = width.Value;
            infiniteWidth = false;
            AddPropertyBinding(width, () => {
                if (!ReferenceEquals(frameWidthState, width))
                    return;

                float target = width.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                {
                    AnimateWidth(builtGameObject, target);
                }
                else
                {
                    preferredWidth = target;
                    infiniteWidth = false;
                    if (builtGameObject != null) ApplySize(builtGameObject);
                }
            }, "width", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets a fixed preferred height.</summary>
        public virtual UIElement WithHeight(float height)
        {
            frameHeightState = null;
            if (useAnimation && builtGameObject != null && animationDuration > 0)
            {
                AnimateHeight(builtGameObject, height);
                return this;
            }

            preferredHeight = height;
            infiniteHeight = false;
            if (builtGameObject != null) ApplySize(builtGameObject);
            return this;
        }

        /// <summary>Sets a reactive preferred height.</summary>
        public virtual UIElement WithHeight(State<float> height)
        {
            if (height == null)
                return this;

            frameHeightState = height;
            preferredHeight = height.Value;
            infiniteHeight = false;
            AddPropertyBinding(height, () => {
                if (!ReferenceEquals(frameHeightState, height))
                    return;

                float target = height.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                {
                    AnimateHeight(builtGameObject, target);
                }
                else
                {
                    preferredHeight = target;
                    infiniteHeight = false;
                    if (builtGameObject != null) ApplySize(builtGameObject);
                }
            }, "height", BindingKind.Layout);
            return this;
        }

        internal virtual UIElement WithFrameConstraints(float? minWidth = null, float? maxWidth = null,
            float? minHeight = null, float? maxHeight = null)
        {
            if (minWidth.HasValue)
                minimumWidth = Mathf.Max(0f, minWidth.Value);
            if (minHeight.HasValue)
                minimumHeight = Mathf.Max(0f, minHeight.Value);

            if (maxWidth.HasValue)
            {
                if (float.IsPositiveInfinity(maxWidth.Value))
                    WithInfiniteWidth();
                else if (preferredWidth < 0f)
                    WithWidth(Mathf.Max(0f, maxWidth.Value));
            }

            if (maxHeight.HasValue)
            {
                if (float.IsPositiveInfinity(maxHeight.Value))
                    WithInfiniteHeight();
                else if (preferredHeight < 0f)
                    WithHeight(Mathf.Max(0f, maxHeight.Value));
            }

            if (builtGameObject != null)
                ApplyFrameConstraints(builtGameObject);

            return this;
        }

        internal virtual UIElement WithLayoutPriority(float priority)
        {
            layoutPriority = priority;
            if (builtGameObject != null)
                ApplyFrameConstraints(builtGameObject);
            return this;
        }

        internal virtual UIElement WithHidden(bool isHidden = true)
        {
            hidden = isHidden;
            if (builtGameObject != null)
            {
                ApplyOpacity(builtGameObject);
                ApplyInteraction(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets uniform padding on all edges.</summary>
        public virtual UIElement WithPadding(int pad)
        {
            padding = new RectOffset(pad, pad, pad, pad);
            if (builtGameObject != null) ApplyPadding(builtGameObject);
            return this;
        }

        /// <summary>Sets reactive uniform padding.</summary>
        public virtual UIElement WithPadding(State<int> pad)
        {
            if (pad == null)
                return this;

            padding = new RectOffset(pad.Value, pad.Value, pad.Value, pad.Value);
            AddPropertyBinding(pad, () => {
                padding = new RectOffset(pad.Value, pad.Value, pad.Value, pad.Value);
                if (builtGameObject != null) ApplyPadding(builtGameObject);
            }, "padding", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets explicit per-edge padding.</summary>
        public virtual UIElement WithPadding(RectOffset pad)
        {
            padding = pad ?? new RectOffset(0, 0, 0, 0);
            if (builtGameObject != null) ApplyPadding(builtGameObject);
            return this;
        }

        /// <summary>Sets absolute position (top-left origin, Y increasing downward).</summary>
        public virtual UIElement WithPosition(float x, float y)
        {
            var newPos = new Vector2(x, y);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimatePosition(builtGameObject, newPos);
            else
            {
                useCustomPosition = true;
                customPosition = newPos;
                if (builtGameObject != null) ApplyCustomPosition(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets reactive absolute position.</summary>
        public virtual UIElement WithPosition(State<Vector2> position)
        {
            if (position == null)
                return this;

            useCustomPosition = true;
            customPosition = position.Value;
            AddPropertyBinding(position, () => {
                var newPos = position.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimatePosition(builtGameObject, newPos);
                else
                {
                    customPosition = newPos;
                    if (builtGameObject != null) ApplyCustomPosition(builtGameObject);
                }
            }, "position", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets reactive X with fixed Y.</summary>
        public virtual UIElement WithPosition(State<float> x, float y)
        {
            if (x == null)
                return this;

            useCustomPosition = true;
            customPosition = new Vector2(x.Value, y);
            AddPropertyBinding(x, () => {
                var newPos = new Vector2(x.Value, customPosition.y);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimatePosition(builtGameObject, newPos);
                else
                {
                    customPosition = newPos;
                    if (builtGameObject != null) ApplyCustomPosition(builtGameObject);
                }
            }, "positionX", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets fixed X with reactive Y.</summary>
        public virtual UIElement WithPosition(float x, State<float> y)
        {
            if (y == null)
                return this;

            useCustomPosition = true;
            customPosition = new Vector2(x, y.Value);
            AddPropertyBinding(y, () => {
                var newPos = new Vector2(customPosition.x, y.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimatePosition(builtGameObject, newPos);
                else
                {
                    customPosition = newPos;
                    if (builtGameObject != null) ApplyCustomPosition(builtGameObject);
                }
            }, "positionY", BindingKind.Layout);
            return this;
        }

        /// <summary>Sets background color.</summary>
        public virtual UIElement WithBackgroundColor(Color color)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateBackgroundColor(builtGameObject, color);
            else
            {
                backgroundColor = color;
                if (builtGameObject != null)
                    ApplyBackgroundColor(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets reactive background color.</summary>
        public virtual UIElement WithBackgroundColor(State<Color> color)
        {
            if (color == null)
                return this;

            backgroundColor = color.Value;
            AddPropertyBinding(color, () => {
                var c = color.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateBackgroundColor(builtGameObject, c);
                else
                {
                    backgroundColor = c;
                    if (builtGameObject != null) ApplyBackgroundColor(builtGameObject);
                }
            }, "backgroundColor", BindingKind.Visual);
            return this;
        }

        /// <summary>Sets opacity (0–1).</summary>
        public virtual UIElement WithOpacity(float value)
        {
            value = Mathf.Clamp01(value);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
            {
                AnimateOpacity(builtGameObject, value);
            }
            else
            {
                opacity = value;
                if (builtGameObject != null) ApplyOpacity(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets reactive opacity (0–1).</summary>
        public virtual UIElement WithOpacity(State<float> value)
        {
            if (value == null)
                return this;

            opacity = Mathf.Clamp01(value.Value);
            AddPropertyBinding(value, () => {
                if (builtGameObject == null)
                    return;

                float v = Mathf.Clamp01(value.Value);
                opacity = v;
                if (useAnimation && animationDuration > 0f)
                    AnimateOpacity(builtGameObject, v);
                else
                    ApplyOpacity(builtGameObject, cancelAnimator: false);
            }, "opacity", BindingKind.Visual);
            return this;
        }

        /// <summary>Sets uniform corner radius.</summary>
        public virtual UIElement WithCornerRadius(float radius)
        {
            radius = Mathf.Clamp(radius, 0f, 50f);
            var newR = new Vector4(radius, radius, radius, radius);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateCornerRadius(builtGameObject, newR);
            else
            {
                cornerRadius = newR;
                if (builtGameObject != null)
                    ApplyRoundedCorners(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets reactive uniform corner radius.</summary>
        public virtual UIElement WithCornerRadius(State<float> radius)
        {
            if (radius == null)
                return this;

            float initialRadius = Mathf.Clamp(radius.Value, 0f, 50f);
            cornerRadius = new Vector4(initialRadius, initialRadius, initialRadius, initialRadius);
            AddPropertyBinding(radius, () => {
                float nextRadius = Mathf.Clamp(radius.Value, 0f, 50f);
                var newR = new Vector4(nextRadius, nextRadius, nextRadius, nextRadius);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateCornerRadius(builtGameObject, newR);
                else
                {
                    cornerRadius = newR;
                    if (builtGameObject != null) ApplyRoundedCorners(builtGameObject);
                }
            }, "cornerRadius", BindingKind.Visual);
            return this;
        }

        /// <summary>Sets per-corner radius values.</summary>
        public virtual UIElement WithCornerRadius(float topLeft, float topRight, float bottomRight, float bottomLeft)
        {
            var newR = new Vector4(
                Mathf.Clamp(topLeft, 0f, 50f),
                Mathf.Clamp(topRight, 0f, 50f),
                Mathf.Clamp(bottomRight, 0f, 50f),
                Mathf.Clamp(bottomLeft, 0f, 50f));
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateCornerRadius(builtGameObject, newR);
            else
            {
                cornerRadius = newR;
                if (builtGameObject != null)
                    ApplyRoundedCorners(builtGameObject);
            }
            return this;
        }

        /// <summary>Sets corner radius on selected corners only.</summary>
        public virtual UIElement WithCornerRadius(float radius, RectCorner corners)
        {
            radius = Mathf.Clamp(radius, 0f, 50f);
            var r = cornerRadius;
            if ((corners & RectCorner.TopLeft) != 0)    r.x = radius;
            if ((corners & RectCorner.TopRight) != 0)   r.y = radius;
            if ((corners & RectCorner.BottomRight) != 0) r.z = radius;
            if ((corners & RectCorner.BottomLeft) != 0)  r.w = radius;
            cornerRadius = r;
            if (builtGameObject != null)
                ApplyRoundedCorners(builtGameObject);
            return this;
        }

        /// <summary>Applies Z-axis rotation in degrees.</summary>
        public virtual UIElement WithRotationEffect(float degrees)
        {
            var newRot = new Vector3(0, 0, degrees);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, newRot);
            else
            {
                rotationEffectEuler = newRot;
                if (builtGameObject != null)
                    ApplyRotation(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies rotation with Euler angles.</summary>
        public virtual UIElement WithRotationEffect(float x, float y, float z)
        {
            var newRot = new Vector3(x, y, z);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, newRot);
            else
            {
                rotationEffectEuler = newRot;
                if (builtGameObject != null)
                    ApplyRotation(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies rotation from a <see cref="Vector3"/> Euler angles.</summary>
        public virtual UIElement WithRotationEffect(Vector3 euler)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateRotation(builtGameObject, euler);
            else
            {
                rotationEffectEuler = euler;
                if (builtGameObject != null)
                    ApplyRotation(builtGameObject);
            }
            return this;
        }

        /// <summary>Reactive Z-axis rotation in degrees.</summary>
        public virtual UIElement WithRotationEffect(State<float> degrees)
        {
            if (degrees == null)
                return this;

            rotationEffectEuler = new Vector3(0, 0, degrees.Value);
            AddPropertyBinding(degrees, () => {
                var newRot = new Vector3(0, 0, degrees.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler = newRot;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotation", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive rotation with bound X component.</summary>
        public virtual UIElement WithRotationEffect(State<float> x, float y, float z)
        {
            if (x == null)
                return this;

            rotationEffectEuler = new Vector3(x.Value, y, z);
            AddPropertyBinding(x, () => {
                var newRot = new Vector3(x.Value, rotationEffectEuler.y, rotationEffectEuler.z);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler.x = x.Value;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotationX", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive rotation with bound Y component.</summary>
        public virtual UIElement WithRotationEffect(float x, State<float> y, float z)
        {
            if (y == null)
                return this;

            rotationEffectEuler = new Vector3(x, y.Value, z);
            AddPropertyBinding(y, () => {
                var newRot = new Vector3(rotationEffectEuler.x, y.Value, rotationEffectEuler.z);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler.y = y.Value;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotationY", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive rotation with bound Z component.</summary>
        public virtual UIElement WithRotationEffect(float x, float y, State<float> z)
        {
            if (z == null)
                return this;

            rotationEffectEuler = new Vector3(x, y, z.Value);
            AddPropertyBinding(z, () => {
                var newRot = new Vector3(rotationEffectEuler.x, rotationEffectEuler.y, z.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler.z = z.Value;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotationZ", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive rotation with bound <see cref="Vector3"/> Euler angles.</summary>
        public virtual UIElement WithRotationEffect(State<Vector3> rotation)
        {
            if (rotation == null)
                return this;

            rotationEffectEuler = rotation.Value;
            AddPropertyBinding(rotation, () => {
                var newRot = rotation.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateRotation(builtGameObject, newRot);
                else
                {
                    rotationEffectEuler = newRot;
                    if (builtGameObject != null) ApplyRotation(builtGameObject);
                }
            }, "rotation3d", BindingKind.Visual);
            return this;
        }

        /// <summary>Applies uniform scale.</summary>
        public virtual UIElement WithScaleEffect(float scale)
        {
            var newScale = new Vector3(scale, scale, scale);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
            {
                scaleEffect = newScale;
                if (builtGameObject != null)
                    ApplyScale(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies scale on X and Y (Z = 1).</summary>
        public virtual UIElement WithScaleEffect(float x, float y)
        {
            var newScale = new Vector3(x, y, 1f);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
            {
                scaleEffect = newScale;
                if (builtGameObject != null)
                    ApplyScale(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies scale on X, Y, and Z.</summary>
        public virtual UIElement WithScaleEffect(float x, float y, float z)
        {
            var newScale = new Vector3(x, y, z);
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, newScale);
            else
            {
                scaleEffect = newScale;
                if (builtGameObject != null)
                    ApplyScale(builtGameObject);
            }
            return this;
        }

        /// <summary>Applies scale from a <see cref="Vector3"/>.</summary>
        public virtual UIElement WithScaleEffect(Vector3 scale)
        {
            if (useAnimation && builtGameObject != null && animationDuration > 0)
                AnimateScale(builtGameObject, scale);
            else
            {
                scaleEffect = scale;
                if (builtGameObject != null)
                    ApplyScale(builtGameObject);
            }
            return this;
        }

        /// <summary>Reactive uniform scale.</summary>
        public virtual UIElement WithScaleEffect(State<float> scale)
        {
            if (scale == null)
                return this;

            scaleEffect = new Vector3(scale.Value, scale.Value, scale.Value);
            AddPropertyBinding(scale, () => {
                var newScale = new Vector3(scale.Value, scale.Value, scale.Value);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateScale(builtGameObject, newScale);
                else
                {
                    scaleEffect = newScale;
                    if (builtGameObject != null) ApplyScale(builtGameObject);
                }
            }, "scale", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive scale with bound X component.</summary>
        public virtual UIElement WithScaleEffect(State<float> x, float y)
        {
            if (x == null)
                return this;

            scaleEffect = new Vector3(x.Value, y, 1f);
            AddPropertyBinding(x, () => {
                var newScale = new Vector3(x.Value, y, 1f);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateScale(builtGameObject, newScale);
                else
                {
                    scaleEffect = newScale;
                    if (builtGameObject != null) ApplyScale(builtGameObject);
                }
            }, "scaleX", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive scale with bound Y component.</summary>
        public virtual UIElement WithScaleEffect(float x, State<float> y)
        {
            if (y == null)
                return this;

            scaleEffect = new Vector3(x, y.Value, 1f);
            AddPropertyBinding(y, () => {
                var newScale = new Vector3(x, y.Value, 1f);
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateScale(builtGameObject, newScale);
                else
                {
                    scaleEffect = newScale;
                    if (builtGameObject != null) ApplyScale(builtGameObject);
                }
            }, "scaleY", BindingKind.Visual);
            return this;
        }

        /// <summary>Reactive scale with bound <see cref="Vector3"/>.</summary>
        public virtual UIElement WithScaleEffect(State<Vector3> scale)
        {
            if (scale == null)
                return this;

            scaleEffect = scale.Value;
            AddPropertyBinding(scale, () => {
                var newScale = scale.Value;
                if (useAnimation && builtGameObject != null && animationDuration > 0)
                    AnimateScale(builtGameObject, newScale);
                else
                {
                    scaleEffect = newScale;
                    if (builtGameObject != null) ApplyScale(builtGameObject);
                }
            }, "scale3d", BindingKind.Visual);
            return this;
        }

    }
}
