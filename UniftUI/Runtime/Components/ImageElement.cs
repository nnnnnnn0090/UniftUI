using UnityEngine;
using UnityEngine.UI;

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

    public enum ImageScaleMode
    {
        Fill,
        AspectFit,
        AspectFill
    }

    /// <summary>Displays a sprite with optional tint, scale mode, and resizing behavior.</summary>
    public class ImageElement : UIElement
    {
        private Sprite sprite;
        private Color tintColor = Color.white;
        private ImageScaleMode scaleMode = ImageScaleMode.AspectFit;
        private Image.Type unityImageType = Image.Type.Simple;

        public ImageElement(Sprite sprite)
        {
            this.sprite = sprite;
            UIContext.Add(this);
        }

        public ImageElement WithTintColor(Color color)
        {
            this.tintColor = color;
            return this;
        }

        public ImageElement WithScaleMode(ImageScaleMode mode)
        {
            this.scaleMode = mode;
            return this;
        }

        /// <summary>Makes the image resizable with the given mode. Nine-slice cap insets are not supported.</summary>
        public ImageElement Resizable(ImageResizingMode resizingMode = ImageResizingMode.Stretch)
        {
            unityImageType = resizingMode == ImageResizingMode.Tile
                ? Image.Type.Tiled
                : Image.Type.Simple;
            return this;
        }

        /// <summary>Sets uGUI <see cref="Image.type"/> directly (prefer <see cref="Resizable"/>).</summary>
        public ImageElement WithImageType(Image.Type type)
        {
            this.unityImageType = type;
            return this;
        }

        public new ImageElement WithOpacity(float value)
        {
            base.WithOpacity(value);
            return this;
        }

        public override GameObject Build(Transform parent)
        {
            GameObject imageObj = new GameObject("Image");
            imageObj.transform.SetParent(parent, false);

            LayoutElement layoutElement = imageObj.AddComponent<LayoutElement>();

            float spriteMinWidth = -1, spriteMinHeight = -1;
            if (sprite != null)
            {
                if (preferredWidth < 0)
                    spriteMinWidth = sprite.rect.width / sprite.pixelsPerUnit;
                if (preferredHeight < 0)
                    spriteMinHeight = sprite.rect.height / sprite.pixelsPerUnit;
            }

            bool fixedFrame = preferredWidth >= 0f && preferredHeight >= 0f && !infiniteWidth && !infiniteHeight;
            bool useAspectFill = scaleMode == ImageScaleMode.AspectFill && sprite != null && fixedFrame;

            ContentSizeFitter sizeFitter = null;
            if (!useAspectFill)
                sizeFitter = imageObj.AddComponent<ContentSizeFitter>();

            if (infiniteWidth)
            {
                layoutElement.flexibleWidth = 1;
                layoutElement.preferredWidth = -1;
                layoutElement.minWidth = 0;
                if (sizeFitter != null)
                    sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredWidth >= 0)
            {
                layoutElement.preferredWidth = preferredWidth;
                layoutElement.minWidth = preferredWidth;
                layoutElement.flexibleWidth = 0;
                if (sizeFitter != null)
                    sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                layoutElement.preferredWidth = -1;
                if (spriteMinWidth >= 0) layoutElement.minWidth = spriteMinWidth;
                else layoutElement.minWidth = 0;
                layoutElement.flexibleWidth = 0;
                if (sizeFitter != null)
                    sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (infiniteHeight)
            {
                layoutElement.flexibleHeight = 1;
                layoutElement.preferredHeight = -1;
                layoutElement.minHeight = 0;
                if (sizeFitter != null)
                    sizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
            else if (preferredHeight >= 0)
            {
                layoutElement.preferredHeight = preferredHeight;
                layoutElement.minHeight = preferredHeight;
                layoutElement.flexibleHeight = 0;
                if (sizeFitter != null)
                    sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            else
            {
                layoutElement.preferredHeight = -1;
                if (spriteMinHeight >= 0) layoutElement.minHeight = spriteMinHeight;
                else layoutElement.minHeight = 0;
                layoutElement.flexibleHeight = 0;
                if (sizeFitter != null)
                    sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            Image mainGraphic;

            if (useAspectFill)
            {
                imageObj.AddComponent<RectMask2D>();

                Vector2 spriteSize = new Vector2(
                    sprite.rect.width / sprite.pixelsPerUnit,
                    sprite.rect.height / sprite.pixelsPerUnit);
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

                mainGraphic = contentObj.AddComponent<Image>();
                mainGraphic.sprite = sprite;
                mainGraphic.preserveAspect = false;
                mainGraphic.type = Image.Type.Simple;
                mainGraphic.color = tintColor;
            }
            else
            {
                mainGraphic = imageObj.AddComponent<Image>();
                mainGraphic.sprite = sprite;
                mainGraphic.color = tintColor;
                mainGraphic.type = unityImageType;

                switch (scaleMode)
                {
                    case ImageScaleMode.Fill:
                        mainGraphic.preserveAspect = false;
                        break;
                    case ImageScaleMode.AspectFit:
                        mainGraphic.preserveAspect = true;
                        break;
                    case ImageScaleMode.AspectFill:
                        mainGraphic.preserveAspect = true;
                        break;
                }

                if (unityImageType == Image.Type.Tiled)
                    mainGraphic.preserveAspect = false;
            }

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
    }
}
