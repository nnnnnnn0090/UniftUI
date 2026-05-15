using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

namespace UniftUI
{
    public static class UIElementExtensions
    {
        public static T Frame<T>(this T element, float? width = null, float? height = null, 
                                bool? infiniteWidth = null, bool? infiniteHeight = null) where T : UIElement
        {
            if (width.HasValue)
            {
                element.WithWidth(width.Value);
            }
            
            if (height.HasValue)
            {
                element.WithHeight(height.Value);
            }
            
            if (infiniteWidth.HasValue && infiniteWidth.Value)
            {
                element.WithInfiniteWidth();
            }
            
            if (infiniteHeight.HasValue && infiniteHeight.Value)
            {
                element.WithInfiniteHeight();
            }
            
            return element;
        }
        
        // State対応フレーム設定 - 修正版
        public static T Frame<T>(this T element, State<float> width = null, State<float> height = null) where T : UIElement
        {
            if (width != null)
            {
                // Stateオブジェクト自体をWithWidthに直接渡す
                element.WithWidth(width);
            }
            
            if (height != null)
            {
                // Stateオブジェクト自体をWithHeightに直接渡す
                element.WithHeight(height);
            }
            
            return element;
        }
        
        public static BackgroundElement Background<T>(this T element, Color color) where T : UIElement
        {
            var backgroundElement = new BackgroundElement(element, color);
            backgroundElement.preferredWidth = element.preferredWidth;
            backgroundElement.preferredHeight = element.preferredHeight;
            backgroundElement.infiniteWidth = element.infiniteWidth;
            backgroundElement.infiniteHeight = element.infiniteHeight;
            
            UIContext.Current?.ReplaceChild(element, backgroundElement);
            return backgroundElement;
        }
        
        // State対応背景設定
        public static BackgroundElement Background<T>(this T element, State<Color> color) where T : UIElement
        {
            var backgroundElement = new BackgroundElement(element, color.Value);
            backgroundElement.preferredWidth = element.preferredWidth;
            backgroundElement.preferredHeight = element.preferredHeight;
            backgroundElement.infiniteWidth = element.infiniteWidth;
            backgroundElement.infiniteHeight = element.infiniteHeight;
            
            // Stateの更新をバインド
            backgroundElement.WithBackgroundColor(color);
            
            UIContext.Current?.ReplaceChild(element, backgroundElement);
            return backgroundElement;
        }

        /// <summary>SwiftUI の <c>.offset(x:y:)</c> に相当します。</summary>
        public static OffsetElement Offset<T>(this T element, float x, float y) where T : UIElement
        {
            var offsetElement = new OffsetElement(element, new Vector2(x, y));
            UIContext.Current?.ReplaceChild(element, offsetElement);
            return offsetElement;
        }

        /// <summary>SwiftUI の <c>.offset(_:)</c>（CGSize / Vector2）に相当します。</summary>
        public static OffsetElement Offset<T>(this T element, Vector2 offset) where T : UIElement
        {
            var offsetElement = new OffsetElement(element, offset);
            UIContext.Current?.ReplaceChild(element, offsetElement);
            return offsetElement;
        }

        public static OffsetElement Offset<T>(this T element, State<Vector2> offset) where T : UIElement
        {
            var offsetElement = new OffsetElement(element, offset);
            UIContext.Current?.ReplaceChild(element, offsetElement);
            return offsetElement;
        }

        public static OffsetElement Offset<T>(this T element, State<float> x, float y) where T : UIElement
        {
            var offsetElement = new OffsetElement(element, x, y);
            UIContext.Current?.ReplaceChild(element, offsetElement);
            return offsetElement;
        }
        
        public static T ForegroundColor<T>(this T element, Color color) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetTextColor(color);
            }
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetTextColor(color);
            }
            else if (element is TextFieldElement textFieldElement)
            {
                textFieldElement.SetTextColor(color);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.ForegroundColor(color);
                }
            }
            return element;
        }
        
        // State対応前景色設定
        public static T ForegroundColor<T>(this T element, State<Color> color) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                if (color != null) {
                    textElement.SetTextColor(color.Value);
                    
                    textElement.AddPropertyBinding(color, () => {
                        textElement.SetTextColor(color.Value);
                    }, "textColor");
                }
            }
            else if (element is ButtonElement buttonElement)
            {
                if (color != null) {
                    buttonElement.SetTextColor(color.Value);
                    
                    buttonElement.AddPropertyBinding(color, () => {
                        buttonElement.SetTextColor(color.Value);
                    }, "buttonTextColor");
                }
            }
            else if (element is TextFieldElement textFieldElement)
            {
                if (color != null) {
                    textFieldElement.SetTextColor(color.Value);
                    
                    textFieldElement.AddPropertyBinding(color, () => {
                        textFieldElement.SetTextColor(color.Value);
                    }, "textFieldTextColor");
                }
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.ForegroundColor(color);
                }
            }
            return element;
        }
        
        public static PaddingElement Padding<T>(this T element, int padding) where T : UIElement
        {
            var paddingValue = new RectOffset(padding, padding, padding, padding);
            var paddingElement = new PaddingElement(element, paddingValue);
            paddingElement.preferredWidth = element.preferredWidth;
            paddingElement.preferredHeight = element.preferredHeight;
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }
        
        // State対応パディング設定
        public static PaddingElement Padding<T>(this T element, State<int> padding) where T : UIElement
        {
            var paddingValue = new RectOffset(padding.Value, padding.Value, padding.Value, padding.Value);
            var paddingElement = new PaddingElement(element, paddingValue);
            paddingElement.preferredWidth = element.preferredWidth;
            paddingElement.preferredHeight = element.preferredHeight;
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;
            
            paddingElement.AddPropertyBinding(padding, () => {
                // PaddingElementのUpdatePaddingメソッドを呼び出す
                paddingElement.UpdatePadding(padding.Value);
            }, "padding");

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }

        /// <summary>SwiftUI の <c>.fixedSize()</c> / <c>fixedSize(horizontal:vertical:)</c> に相当します。</summary>
        public static FixedSizeElement FixedSize<T>(this T element, bool horizontal = true, bool vertical = true) where T : UIElement
        {
            var fs = new FixedSizeElement(element, horizontal, vertical);
            UIContext.Current?.ReplaceChild(element, fs);
            return fs;
        }

        public static PaddingElement Padding<T>(this T element, RectOffset padding) where T : UIElement
        {
            var paddingElement = new PaddingElement(element, padding);
            paddingElement.preferredWidth = element.preferredWidth;
            paddingElement.preferredHeight = element.preferredHeight;
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;
            
            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }

        public static PaddingElement Padding<T>(this T element, float? top = null, float? bottom = null, float? left = null, float? right = null) where T : UIElement
        {
            var paddingValue = new RectOffset(
                left.HasValue ? (int)left.Value : 0,
                right.HasValue ? (int)right.Value : 0,
                top.HasValue ? (int)top.Value : 0,
                bottom.HasValue ? (int)bottom.Value : 0
            );

            var paddingElement = new PaddingElement(element, paddingValue);
            paddingElement.preferredWidth = element.preferredWidth;
            paddingElement.preferredHeight = element.preferredHeight;
            paddingElement.infiniteWidth = element.infiniteWidth;
            paddingElement.infiniteHeight = element.infiniteHeight;

            UIContext.Current?.ReplaceChild(element, paddingElement);
            return paddingElement;
        }
        
        public static T Bold<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetBold(true);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Bold();
                }
            }
            return element;
        }
        
        public static T Italic<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetItalic(true);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Italic();
                }
            }
            return element;
        }
        
        public static T Underline<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetUnderline(true);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Underline();
                }
            }
            return element;
        }
        
        public static T Strikethrough<T>(this T element) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetStrikethrough(true);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Strikethrough();
                }
            }
            return element;
        }
        
        public static ImageElement TintColor(this ImageElement element, Color color)
        {
            return element.WithTintColor(color);
        }
        
        public static ImageElement ScaleMode(this ImageElement element, ImageScaleMode mode)
        {
            return element.WithScaleMode(mode);
        }
        
        public static ImageElement Opacity(this ImageElement element, float opacity)
        {
            return element.WithOpacity(opacity);
        }
        
        public static T Opacity<T>(this T element, float opacity) where T : UIElement
        {
            if (element is ImageElement)
            {
                return element;
            }
            return (T)element.WithOpacity(opacity);
        }
        
        // State対応不透明度設定
        public static T Opacity<T>(this T element, State<float> opacity) where T : UIElement
        {
            if (element is ImageElement)
            {
                return element;
            }
            return (T)element.WithOpacity(opacity);
        }
        
        public static T FontSize<T>(this T element, float size) where T : UIElement
        {
            if (element is TextElement textElement)
            {
                textElement.SetFontSize(size);
            }
            else if (element is TextFieldElement textFieldElement)
            {
                textFieldElement.SetFontSize(size);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.FontSize(size);
                }
            }
            return element;
        }
        
        public static T Font<T>(this T element, TMP_FontAsset font) where T : UIElement
        {
            // グローバルの DefaultFont は更新しない（親スタックの兄弟へ波及させない）。SwiftUI の .font スコープに近い。
            // アプリ全体の既定フォントは TabView 等が UIContext.SetDefaultFont を明示するか、ルートで必要なら直接呼ぶ。

            if (element is TextElement textElement)
            {
                textElement.SetFont(font);
            }
            else if (element is ButtonElement buttonElement)
            {
                buttonElement.SetFont(font);
            }
            else if (element is ToggleElement toggleElement)
            {
                toggleElement.SetFont(font);
            }
            else if (element is TextFieldElement textFieldElement)
            {
                textFieldElement.SetFont(font);
            }
            else if (element is TabView tabView)
            {
                tabView.SetFont(font);
            }
            else if (element is ILayoutContainer container)
            {
                foreach (var child in container.GetChildren())
                {
                    child.Font(font);
                }
            }
            
            return element;
        }

        public static T CornerRadius<T>(this T element, float radius) where T : UIElement
        {
            element.WithCornerRadius(radius);
            return element;
        }
        
        // State対応角丸設定
        public static T CornerRadius<T>(this T element, State<float> radius) where T : UIElement
        {
            element.WithCornerRadius(radius);
            return element;
        }
        
        public static T CornerRadius<T>(this T element, float topLeft, float topRight, float bottomRight, float bottomLeft) where T : UIElement
        {
            element.WithCornerRadius(topLeft, topRight, bottomRight, bottomLeft);
            return element;
        }
        
        public static T CornerRadius<T>(this T element, RectCorner corners, float radius) where T : UIElement
        {
            element.WithCornerRadius(radius, corners);
            return element;
        }
        
        public static UIElement Shadow<T>(this T element, Color? color = null, float radius = 3f, float x = 0, float y = 0) where T : UIElement
        {
            Color shadowColor = color ?? new Color(0,0,0);
            Vector2 offset = new Vector2(x, y);
            
            if (element is TextElement textElement)
            {
                return textElement.SetTMProShadow(shadowColor, offset, radius);
            }
            
            var shadowElement = new ShadowElement(element, shadowColor, offset, radius);
            
            shadowElement.preferredWidth = element.preferredWidth;
            shadowElement.preferredHeight = element.preferredHeight;
            shadowElement.infiniteWidth = element.infiniteWidth;
            shadowElement.infiniteHeight = element.infiniteHeight;
            
            UIContext.Current?.ReplaceChild(element, shadowElement);
            return shadowElement;
        }
        
        public static TextFieldElement LineLimit(this TextFieldElement element, int? limit = null)
        {
            return element.SetLineLimit(limit);
        }
        
        public static T RotationEffect<T>(this T element, float degrees) where T : UIElement
        {
            return (T)element.WithRotationEffect(degrees);
        }
        
        public static T RotationEffect<T>(this T element, float x, float y, float z) where T : UIElement
        {
            return (T)element.WithRotationEffect(x, y, z);
        }
        
        public static T RotationEffect<T>(this T element, State<float> degrees) where T : UIElement
        {
            return (T)element.WithRotationEffect(degrees);
        }
        
        public static T RotationEffect<T>(this T element, State<float> x, float y, float z) where T : UIElement
        {
            return (T)element.WithRotationEffect(x, y, z);
        }
        
        public static T RotationEffect<T>(this T element, float x, State<float> y, float z) where T : UIElement
        {
            return (T)element.WithRotationEffect(x, y, z);
        }
        
        public static T RotationEffect<T>(this T element, float x, float y, State<float> z) where T : UIElement
        {
            return (T)element.WithRotationEffect(x, y, z);
        }
        
        public static T RotationEffect<T>(this T element, State<Vector3> euler) where T : UIElement
        {
            return (T)element.WithRotationEffect(euler);
        }
        
        public static T ScaleEffect<T>(this T element, float scale) where T : UIElement
        {
            return (T)element.WithScaleEffect(scale);
        }
        
        public static T ScaleEffect<T>(this T element, float x, float y) where T : UIElement
        {
            return (T)element.WithScaleEffect(x, y);
        }
        
        public static T ScaleEffect<T>(this T element, float x, float y, float z) where T : UIElement
        {
            return (T)element.WithScaleEffect(x, y, z);
        }
        
        public static T ScaleEffect<T>(this T element, Vector3 scale) where T : UIElement
        {
            return (T)element.WithScaleEffect(scale);
        }
        
        public static T ScaleEffect<T>(this T element, State<float> scale) where T : UIElement
        {
            return (T)element.WithScaleEffect(scale);
        }
        
        public static T ScaleEffect<T>(this T element, State<float> x, float y) where T : UIElement
        {
            return (T)element.WithScaleEffect(x, y);
        }
        
        public static T ScaleEffect<T>(this T element, float x, State<float> y) where T : UIElement
        {
            return (T)element.WithScaleEffect(x, y);
        }
        
        public static T ScaleEffect<T>(this T element, State<Vector3> scale) where T : UIElement
        {
            return (T)element.WithScaleEffect(scale);
        }

        public static T Position<T>(this T element, float x, float y) where T : UIElement
        {
            return (T)element.WithPosition(x, y);
        }
        
        // State対応位置設定
        public static T Position<T>(this T element, State<Vector2> position) where T : UIElement
        {
            return (T)element.WithPosition(position);
        }
        
        public static T Position<T>(this T element, State<float> x, float y) where T : UIElement
        {
            return (T)element.WithPosition(x, y);
        }
        
        public static T Position<T>(this T element, float x, State<float> y) where T : UIElement
        {
            return (T)element.WithPosition(x, y);
        }
        
        public static T OnAppear<T>(this T element, Action action) where T : UIElement
        {
            return (T)element.WithOnAppear(action);
        }
        
        public static T OnAppear<T>(this T element, Func<Task> asyncAction) where T : UIElement
        {
            return (T)element.WithOnAppearAsync(asyncAction);
        }
        
        public static T Update<T>(this T element, Action action) where T : UIElement
        {
            return (T)element.WithUpdate(action);
        }
        
        public static T Animation<T>(this T element, float duration) where T : UIElement
        {
            return (T)element.WithAnimation(duration);
        }
        
        // イージング指定可能なアニメーションメソッドを追加
        public static T Animation<T>(this T element, AnimationEasing easing, float duration) where T : UIElement
        {
            return (T)element.WithAnimation(easing, duration);
        }

        // ── SwiftUI スタイル .animation(_:value:) ───────────────────────────

        /// <summary>
        /// SwiftUI の <c>.animation(.spring(), value: myState)</c> に相当。
        /// 指定した State が変化したとき、このビューをアニメーションで補間する。
        /// </summary>
        public static T animation<T>(this T element, Animation anim, State value) where T : UIElement
        {
            ((UIElement)element).animation(anim, value);
            return element;
        }

        /// <summary>デフォルトアニメーションで補間する。</summary>
        public static T animation<T>(this T element, State value) where T : UIElement
        {
            ((UIElement)element).animation(UniftUI.Animation.Default, value);
            return element;
        }

        // ── ScrollView（SwiftUI 風チェーン）──────────────────────────────────

        /// <summary>弾性スクロールの有無（<see cref="ScrollRect.MovementType"/>）。</summary>
        public static ScrollViewElement ScrollBounce(this ScrollViewElement e, bool elastic) =>
            e.WithScrollBounce(elastic);

        public static ScrollViewElement ScrollSensitivity(this ScrollViewElement e, float sensitivity) =>
            e.WithScrollSensitivity(sensitivity);

        public static ScrollViewElement ScrollMovementType(this ScrollViewElement e, ScrollRect.MovementType type) =>
            e.WithMovementType(type);

        /// <summary>縦正規化位置（1=先頭、0=末尾）を <see cref="State{T}"/> にバインド。</summary>
        public static ScrollViewElement ScrollPositionY(this ScrollViewElement e, State<float> normalized, bool twoWay = false) =>
            e.BindScrollPositionY(normalized, twoWay);

        /// <summary>SwiftUI の <c>.scrollIndicators(_:axes:)</c> に相当。有効な軸すべてに適用。</summary>
        public static ScrollViewElement ScrollIndicators(this ScrollViewElement e, ScrollIndicatorVisibility visibility) =>
            e.WithScrollIndicators(visibility);

        /// <summary>SwiftUI の <c>.scrollIndicators(_:axes:)</c> に相当。指定軸のみ更新。</summary>
        public static ScrollViewElement ScrollIndicators(this ScrollViewElement e, ScrollIndicatorVisibility visibility, UniftUIScrollAxis axes) =>
            e.WithScrollIndicators(visibility, axes);

        /// <summary>水平正規化位置（0=左、1=右）をバインド。</summary>
        public static ScrollViewElement ScrollPositionX(this ScrollViewElement e, State<float> normalized, bool twoWay = false) =>
            e.BindScrollPositionX(normalized, twoWay);
    }
}
