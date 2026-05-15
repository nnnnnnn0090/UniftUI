using UnityEngine;
using System;

namespace UniftUI
{
    /// <summary>Static factories for UI elements outside of <see cref="UniftView"/>.</summary>
    public static class UIElements
    {
        public static TextElement Text(string text) => new TextElement(text);

        public static TextElement Text(string text, State[] dependencyStates)
            => new TextElement(text, dependencyStates);

        public static TextElement Text(Func<string> content, State[] dependencyStates = null)
            => new TextElement(content, dependencyStates);

        public static TextElement Text(State<string> text, State[] additionalStates = null)
            => new TextElement(text, additionalStates);

        public static ButtonElement Button(string label, Action onClick) => new ButtonElement(label, onClick);

        public static ButtonElement Button(UIElement content, Action onClick)
        {
            UIContext.Current?.RemoveChild(content);
            return new ButtonElement(content, onClick);
        }

        public static ImageElement Image(Sprite sprite) => new ImageElement(sprite);

        public static VStackElement VStack(Action content, State[] states = null, float spacing = 8f,
            VStackAlignment alignment = VStackAlignment.Center)
            => new VStackElement(content, states, spacing, alignment);

        public static HStackElement HStack(Action content, State[] states = null, float spacing = 8f,
            HStackAlignment alignment = HStackAlignment.Center)
            => new HStackElement(content, states, spacing, alignment);

        public static ZStackElement ZStack(Action content, State[] states = null,
            ZStackAlignment alignment = ZStackAlignment.Center)
            => new ZStackElement(content, states, alignment);

        public static GridElement Grid(Action content, State[] states = null, float horizontalSpacing = 8f,
            float verticalSpacing = 8f, HStackAlignment rowAlignment = HStackAlignment.Center)
            => new GridElement(content, states, horizontalSpacing, verticalSpacing, rowAlignment);

        public static GridRowElement GridRow(Action content)
        {
            if (UIContext.Current is GridElement grid)
                return new GridRowElement(grid, content);
            Debug.LogWarning("[UniftUI] GridRow must be used inside Grid content.");
            return new GridRowElement(null, content);
        }

        public static SpacerElement Spacer(float minLength = 0) => new SpacerElement(minLength);

        public static SliderElement Slider(State<int> value, float minValue, float maxValue)
            => new SliderElement(value, minValue, maxValue);

        public static ScrollViewElement ScrollView(Action content, State[] states = null, bool horizontal = false, bool vertical = true)
            => new ScrollViewElement(content, states, horizontal, vertical);

        public static ToggleElement Toggle(State<bool> isOn, string label, Action<bool> onValueChanged = null)
            => new ToggleElement(isOn, label, onValueChanged);

        public static TextFieldElement TextField(State<string> text, string placeholder, Action<string> onTextChanged = null)
            => new TextFieldElement(text, placeholder, onTextChanged);
    }
}
