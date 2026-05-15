using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UniftUI
{
    /// <summary>Base MonoBehaviour for declarative UniftUI views.</summary>
    public abstract class UniftView : MonoBehaviour
    {
        protected UIElement VStack(Action content)
            => VStack(content, null, 8f, VStackAlignment.Center);

        protected UIElement VStack(Action content, State[] states = null)
            => VStack(content, states, 8f, VStackAlignment.Center);

        protected UIElement VStack(Action content, State[] states = null, float spacing = 8f,
            VStackAlignment alignment = VStackAlignment.Center)
        {
            var stack = new VStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        protected UIElement HStack(Action content)
            => HStack(content, null, 8f, HStackAlignment.Center);

        protected UIElement HStack(Action content, float spacing = 8f)
            => HStack(content, null, spacing, HStackAlignment.Center);

        protected UIElement HStack(Action content, State[] states = null)
            => HStack(content, states, 8f, HStackAlignment.Center);

        protected UIElement HStack(Action content, State[] states = null, float spacing = 8f,
            HStackAlignment alignment = HStackAlignment.Center)
        {
            var stack = new HStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        protected GridElement Grid(Action content, State[] states = null, float horizontalSpacing = 8f,
            float verticalSpacing = 8f, HStackAlignment rowAlignment = HStackAlignment.Center)
        {
            var grid = new GridElement(content, states, horizontalSpacing, verticalSpacing, rowAlignment);
            UIContext.Current?.AddChild(grid);
            return grid;
        }

        protected GridRowElement GridRow(Action content)
        {
            if (UIContext.Current is GridElement grid)
                return new GridRowElement(grid, content);
            Debug.LogWarning("[UniftUI] GridRow must be used inside Grid content.");
            return new GridRowElement(null, content);
        }

        protected UIElement Text(string text) => new TextElement(text);

        protected UIElement Text(string text, State[] dependencyStates)
            => new TextElement(text, dependencyStates);

        protected UIElement Text(Func<string> content, State[] dependencyStates = null)
            => new TextElement(content, dependencyStates);

        protected UIElement Text(State<string> text, State[] additionalStates = null)
            => new TextElement(text, additionalStates);

        protected UIElement Button(string label, Action onClick) => new ButtonElement(label, onClick);

        protected UIElement Button(string label, Action onClick, Color backgroundColor, Color textColor)
        {
            var button = new ButtonElement(label, onClick);
            button.SetBackgroundColor(backgroundColor);
            button.SetTextColor(textColor);
            return button;
        }

        protected UIElement Button(UIElement content, Action onClick)
        {
            UIContext.Current?.RemoveChild(content);
            return new ButtonElement(content, onClick);
        }

        protected ImageElement Image(Sprite sprite) => new ImageElement(sprite);

        /// <summary>Flexible space along the stack main axis.</summary>
        protected UIElement Spacer(float minLength = 0) => new SpacerElement(minLength);

        protected UIElement TabView(Action content, State<int> selectedIndex = null)
        {
            var tabView = new TabView(content, selectedIndex);
            UIContext.Current?.AddChild(tabView);
            return tabView;
        }

        protected UIElement Tab(Action titleContent, Action content) => new TabItem(titleContent, content);

        protected UIElement Tab(string title, Action content) => new TabItem(title, content);

        protected UIElement Slider(State<int> value, float minValue, float maxValue)
            => new SliderElement(value, minValue, maxValue);

        protected ScrollViewElement ScrollView(Action content, State[] states = null, bool horizontal = false, bool vertical = true)
        {
            var scrollView = new ScrollViewElement(content, states, horizontal, vertical);
            UIContext.Current?.AddChild(scrollView);
            return scrollView;
        }

        protected UIElement Toggle(State<bool> isOn, string label, Action<bool> onValueChanged = null)
            => new ToggleElement(isOn, label, onValueChanged);

        protected UIElement ZStack(Action content) => ZStack(content, null, ZStackAlignment.Center);

        protected UIElement ZStack(Action content, State[] states = null)
            => ZStack(content, states, ZStackAlignment.Center);

        protected UIElement ZStack(Action content, State[] states = null, ZStackAlignment alignment = ZStackAlignment.Center)
        {
            var stack = new ZStackElement(content, states, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        protected TextFieldElement TextField(State<string> text, string placeholder, Action<string> onTextChanged = null)
            => new TextFieldElement(text, placeholder, onTextChanged);

        /// <summary>Inclusive integer range loop (<c>ForEach(0, 9, …)</c>).</summary>
        protected void ForEach(int fromInclusive, int toInclusive, Action<int> content)
        {
            if (toInclusive < fromInclusive) return;
            foreach (int id in Enumerable.Range(fromInclusive, toInclusive - fromInclusive + 1))
                content(id);
        }

        /// <summary>Half-open range loop (C# <c>0..10</c>).</summary>
        protected void ForEach(Range range, Action<int> content)
        {
            if (content == null) return;
            if (range.Start.IsFromEnd || range.End.IsFromEnd)
            {
                Debug.LogWarning("[UniftUI] ForEach(Range): from-end ranges are not supported.");
                return;
            }

            int start = range.Start.Value;
            int end = range.End.Value;
            for (int i = start; i < end; i++)
                content(i);
        }

        /// <summary>Iterates a collection.</summary>
        protected void ForEach<T>(IEnumerable<T> data, Action<T> content)
        {
            foreach (T item in data)
                content(item);
        }

        /// <summary>Animates state changes inside <paramref name="changes"/>.</summary>
        protected void WithAnimation(Animation animation, Action changes)
            => AnimationContext.WithAnimation(animation, changes);

        /// <summary>Animates state changes with <see cref="Animation.Default"/>.</summary>
        protected void WithAnimation(Action changes)
            => AnimationContext.WithAnimation(changes);
    }
}
