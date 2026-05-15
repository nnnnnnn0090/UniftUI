using UnityEngine;
using System;

namespace UniftUI
{
    public static class UIElements
    {
        public static TextElement Text(string text)
        {
            return new TextElement(text);
        }

        public static TextElement Text(string text, State[] dependencyStates)
        {
            return new TextElement(text, dependencyStates);
        }

        public static TextElement Text(Func<string> content, State[] dependencyStates = null)
        {
            return new TextElement(content, dependencyStates);
        }

        public static TextElement Text(State<string> text, State[] additionalStates = null)
        {
            return new TextElement(text, additionalStates);
        }
        
        public static ButtonElement Button(string label, Action onClick)
        {
            return new ButtonElement(label, onClick);
        }
        
        public static ButtonElement Button(UIElement content, Action onClick)
        {
            UIContext.Current?.RemoveChild(content);
            return new ButtonElement(content, onClick);
        }
        
        public static ImageElement Image(Sprite sprite)
        {
            return new ImageElement(sprite);
        }
        
        public static VStackElement VStack(Action content, State[] states = null, float spacing = 8f, 
            VStackAlignment alignment = VStackAlignment.Center)
        {
            return new VStackElement(content, states, spacing, alignment);
        }
        
        public static HStackElement HStack(Action content, State[] states = null, float spacing = 8f, 
            HStackAlignment alignment = HStackAlignment.Center)
        {
            return new HStackElement(content, states, spacing, alignment);
        }
        
        public static ZStackElement ZStack(Action content, State[] states = null, 
            ZStackAlignment alignment = ZStackAlignment.Center)
        {
            return new ZStackElement(content, states, alignment);
        }

        /// <summary>SwiftUI の <c>Grid { }</c> に相当。</summary>
        public static GridElement Grid(Action content, State[] states = null, float horizontalSpacing = 8f,
            float verticalSpacing = 8f, HStackAlignment rowAlignment = HStackAlignment.Center)
        {
            return new GridElement(content, states, horizontalSpacing, verticalSpacing, rowAlignment);
        }

        /// <summary>SwiftUI の <c>GridRow { }</c> に相当（<see cref="Grid"/> のコンテンツ内のみ）。</summary>
        public static GridRowElement GridRow(Action content)
        {
            if (UIContext.Current is GridElement grid)
                return new GridRowElement(grid, content);
            Debug.LogWarning("GridRow は Grid のコンテンツ内でのみ使用してください。");
            return new GridRowElement(null, content);
        }
        
        /// <summary>
        /// SwiftUI の <c>Spacer(minLength:)</c>。余白があると最小長より<strong>大きく伸びます</strong>。
        /// </summary>
        /// <param name="minLength">主軸の最小長。</param>
        public static SpacerElement Spacer(float minLength = 0)
        {
            return new SpacerElement(minLength);
        }

        public static SliderElement Slider(State<int> value, float minValue, float maxValue)
        {
            return new SliderElement(value, minValue, maxValue);
        }

        public static ScrollViewElement ScrollView(Action content, State[] states = null, bool horizontal = false, bool vertical = true)
        {
            return new ScrollViewElement(content, states, horizontal, vertical);
        }

        public static ToggleElement Toggle(State<bool> isOn, string label, Action<bool> onValueChanged = null)
        {
            return new ToggleElement(isOn, label, onValueChanged);
        }
        
        public static TextFieldElement TextField(State<string> text, string placeholder, Action<string> onTextChanged = null)
        {
            return new TextFieldElement(text, placeholder, onTextChanged);
        }
    }
}
