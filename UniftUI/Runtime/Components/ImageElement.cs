using UnityEngine;
using UnityEngine.UI;
using UniftUI.Internal;

namespace UniftUI
{
    /// <summary>Resizing mode for <see cref="ImageElement.Resizable"/>.</summary>
    public enum ImageResizingMode
    {
        /// <summary>Stretch to fill the frame.</summary>
        Stretch,
        /// <summary>Tile the image within the frame.</summary>
        Tile,
    }

    internal enum ImageScaleMode
    {
        Original,
        Fill,
        AspectFit,
        AspectFill
    }

    /// <summary>Displays a sprite with optional tint, scale mode, and resizing behavior.</summary>
    public class ImageElement : UIElement
    {
        private Sprite sprite;
        private Color tintColor = Color.white;
        private ImageScaleMode scaleMode = ImageScaleMode.Original;
        private ImageRenderingMode renderingMode = ImageRenderingMode.Original;
        private ImageResizingMode resizingMode = ImageResizingMode.Stretch;
        private bool isResizable;
        private Image.Type unityImageType = Image.Type.Simple;
        private GameObject builtRoot;
        private Image builtImage;
        private RectTransform builtAspectFillContent;

        public ImageElement(Sprite sprite)
        {
            this.sprite = sprite;
            UIContext.Add(this);
        }

        internal ImageElement WithTintColor(Color color)
        {
            this.tintColor = color;
            if (builtImage != null)
                builtImage.color = EffectiveImageColor;
            return this;
        }

        internal ImageElement WithScaleMode(ImageScaleMode mode)
        {
            this.scaleMode = mode;
            RefreshBuiltSizing();
            return this;
        }

        internal ImageElement WithRenderingMode(ImageRenderingMode mode)
        {
            renderingMode = mode;
            if (builtImage != null)
                builtImage.color = EffectiveImageColor;
            return this;
        }

        /// <summary>Makes the image resizable with the given mode. Nine-slice cap insets are not supported.</summary>
        internal ImageElement Resizable(ImageResizingMode resizingMode = ImageResizingMode.Stretch)
        {
            isResizable = true;
            this.resizingMode = resizingMode;
            unityImageType = resizingMode == ImageResizingMode.Tile
                ? Image.Type.Tiled
                : Image.Type.Simple;
            RefreshBuiltSizing();
            return this;
        }

        internal ImageElement WithImageType(Image.Type type)
        {
            this.unityImageType = type;
            if (builtImage != null)
                builtImage.type = unityImageType;
            return this;
        }

        public new ImageElement WithOpacity(float value)
        {
            base.WithOpacity(value);
            return this;
        }

        public new ImageElement WithOpacity(State<float> value)
        {
            base.WithOpacity(value);
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject imageObj = new GameObject("Image");
            imageObj.transform.SetParent(parent, false);
            builtRoot = imageObj;

            float spriteMinWidth = -1, spriteMinHeight = -1;
            if (sprite != null)
            {
                if (preferredWidth < 0)
                    spriteMinWidth = sprite.rect.width;
                if (preferredHeight < 0)
                    spriteMinHeight = sprite.rect.height;
            }

            bool fixedFrame = preferredWidth >= 0f && preferredHeight >= 0f && !infiniteWidth && !infiniteHeight;
            bool useAspectFill = scaleMode == ImageScaleMode.AspectFill && sprite != null && fixedFrame;

            LayoutElement layoutElement = LayoutElementUtility.Configure(
                imageObj,
                preferredWidth,
                preferredHeight,
                infiniteWidth,
                infiniteHeight,
                spriteMinWidth,
                spriteMinHeight);

            Image mainGraphic;

            if (useAspectFill)
            {
                Vector2 spriteSize = new Vector2(
                    sprite.rect.width,
                    sprite.rect.height);
                float frameW = preferredWidth;
                float frameH = preferredHeight;
                float coverScale = Mathf.Max(
                    frameW / Mathf.Max(0.0001f, spriteSize.x),
                    frameH / Mathf.Max(0.0001f, spriteSize.y));
                Vector2 coverSize = spriteSize * coverScale;

                GameObject contentObj = new GameObject("AspectFillContent");
                contentObj.transform.SetParent(imageObj.transform, false);
                RectTransform contentRt = contentObj.AddComponent<RectTransform>();
                contentRt.anchorMin = contentRt.anchorMax = new Vector2(0.5f, 0.5f);
                contentRt.pivot = new Vector2(0.5f, 0.5f);
                contentRt.sizeDelta = coverSize;
                contentRt.anchoredPosition = Vector2.zero;
                builtAspectFillContent = contentRt;

                mainGraphic = contentObj.AddComponent<Image>();
                mainGraphic.sprite = sprite;
                mainGraphic.preserveAspect = false;
                mainGraphic.type = Image.Type.Simple;
                mainGraphic.color = EffectiveImageColor;
            }
            else
            {
                mainGraphic = imageObj.AddComponent<Image>();
                mainGraphic.sprite = sprite;
                mainGraphic.color = EffectiveImageColor;
                mainGraphic.type = unityImageType;
                ConfigureImageSizing(mainGraphic);
            }

            builtImage = mainGraphic;

            if (backgroundColor != Color.clear)
            {
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(imageObj.transform, false);
                bgObj.transform.SetSiblingIndex(0);

                RectTransform bgRect = bgObj.AddComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;

                Image bgImage = bgObj.AddComponent<Image>();
                bgImage.color = backgroundColor;
            }

            ApplyAllEffects(imageObj, mainGraphic);

            return imageObj;
        }

        private Color EffectiveImageColor
            => renderingMode == ImageRenderingMode.Template ? tintColor : Color.white;

        public override UIElement WithWidth(float width)
        {
            base.WithWidth(width);
            RefreshBuiltSizing();
            return this;
        }

        public override UIElement WithWidth(State<float> width)
        {
            base.WithWidth(width);
            if (width != null)
                AddPropertyBinding(width, RefreshBuiltSizing, "imageFrameWidthSizing", BindingKind.Layout);
            RefreshBuiltSizing();
            return this;
        }

        public override UIElement WithHeight(float height)
        {
            base.WithHeight(height);
            RefreshBuiltSizing();
            return this;
        }

        public override UIElement WithHeight(State<float> height)
        {
            base.WithHeight(height);
            if (height != null)
                AddPropertyBinding(height, RefreshBuiltSizing, "imageFrameHeightSizing", BindingKind.Layout);
            RefreshBuiltSizing();
            return this;
        }

        private void ConfigureImageSizing(Image image)
        {
            if (image == null)
                return;

            image.type = unityImageType;
            image.preserveAspect = ShouldPreserveAspect();
            if (unityImageType == Image.Type.Tiled)
                image.preserveAspect = false;
        }

        private bool ShouldPreserveAspect()
        {
            switch (scaleMode)
            {
                case ImageScaleMode.AspectFit:
                    return true;
                case ImageScaleMode.AspectFill:
                case ImageScaleMode.Fill:
                    return false;
                default:
                    return !isResizable || resizingMode != ImageResizingMode.Stretch;
            }
        }

        private void RefreshBuiltSizing()
        {
            if (builtImage != null)
                ConfigureImageSizing(builtImage);

            if (builtRoot == null || builtAspectFillContent == null || sprite == null)
                return;

            if (preferredWidth < 0f || preferredHeight < 0f || infiniteWidth || infiniteHeight)
                return;

            Vector2 spriteSize = new Vector2(sprite.rect.width, sprite.rect.height);
            float coverScale = Mathf.Max(
                preferredWidth / Mathf.Max(0.0001f, spriteSize.x),
                preferredHeight / Mathf.Max(0.0001f, spriteSize.y));
            builtAspectFillContent.sizeDelta = spriteSize * coverScale;
        }
    }
}
