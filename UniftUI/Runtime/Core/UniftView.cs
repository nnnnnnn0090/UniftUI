using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UniftUI
{
    /// <summary>Base MonoBehaviour for declarative UniftUI views.</summary>
    public abstract class UniftView : MonoBehaviour
    {
        protected VStackElement VStack(Action content)
            => VStack(content, null, 8f, VStackAlignment.Center);

        protected VStackElement VStack(Action content, State[] states = null)
            => VStack(content, states, 8f, VStackAlignment.Center);

        protected VStackElement VStack(Action content, State[] states = null, float spacing = 8f,
            VStackAlignment alignment = VStackAlignment.Center)
        {
            var stack = new VStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        protected HStackElement HStack(Action content)
            => HStack(content, null, 8f, HStackAlignment.Center);

        protected HStackElement HStack(Action content, float spacing = 8f)
            => HStack(content, null, spacing, HStackAlignment.Center);

        protected HStackElement HStack(Action content, State[] states = null)
            => HStack(content, states, 8f, HStackAlignment.Center);

        protected HStackElement HStack(Action content, State[] states = null, float spacing = 8f,
            HStackAlignment alignment = HStackAlignment.Center)
        {
            var stack = new HStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        protected LazyVStackElement LazyVStack(Action content, State[] states = null, float spacing = 8f,
            VStackAlignment alignment = VStackAlignment.Center)
        {
            var stack = new LazyVStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        protected LazyHStackElement LazyHStack(Action content, State[] states = null, float spacing = 8f,
            HStackAlignment alignment = HStackAlignment.Center)
        {
            var stack = new LazyHStackElement(content, states, spacing, alignment);
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

        protected GeometryReaderElement GeometryReader(Func<GeometryProxy, UIElement> content)
        {
            var reader = new GeometryReaderElement(content);
            UIContext.Current?.AddChild(reader);
            return reader;
        }

        protected TextElement Text(string text) => new TextElement(text);

        protected TextElement Text(string text, State[] dependencyStates)
            => new TextElement(text, dependencyStates);

        protected TextElement Text(Func<string> content, State[] dependencyStates = null)
            => new TextElement(content, dependencyStates);

        protected TextElement Text(State<string> text, State[] additionalStates = null)
            => new TextElement(text, additionalStates);

        protected ButtonElement Button(string label, Action onClick) => new ButtonElement(label, onClick);

        protected ButtonElement Button(Action action, string label) => new ButtonElement(label, action);

        protected ButtonElement Button(UIElement content, Action onClick)
        {
            UIContext.Current?.RemoveChild(content);
            return new ButtonElement(content, onClick);
        }

        protected ButtonElement Button(Action action, UIElement label)
        {
            UIContext.Current?.RemoveChild(label);
            return new ButtonElement(label, action);
        }

        protected ImageElement Image(Sprite sprite) => new ImageElement(sprite);

        protected RectangleElement Rectangle() => new RectangleElement(Color.white);

        protected RectangleElement Rectangle(Color color) => new RectangleElement(color);

        protected CircleElement Circle() => new CircleElement(Color.white);

        protected CircleElement Circle(Color color) => new CircleElement(color);

        protected CapsuleElement Capsule() => new CapsuleElement(Color.white);

        protected CapsuleElement Capsule(Color color) => new CapsuleElement(color);

        protected RoundedRectangleElement RoundedRectangle(float cornerRadius)
            => new RoundedRectangleElement(cornerRadius, Color.white);

        protected RoundedRectangleElement RoundedRectangle(float cornerRadius, Color color)
            => new RoundedRectangleElement(cornerRadius, color);

        protected DividerElement Divider() => new DividerElement();

        protected DividerElement Divider(Color color, float thickness = 1f) => new DividerElement(color, thickness);

        protected ProgressViewElement ProgressView(State<float> value, float total = 1f)
            => new ProgressViewElement(value, total);

        protected StepperElement Stepper(State<int> value, int minValue, int maxValue, int step = 1)
            => new StepperElement(value, minValue, maxValue, step);

        protected StepperElement Stepper(string label, State<int> value, int minValue, int maxValue, int step = 1)
            => new StepperElement(value, minValue, maxValue, step, label);

        protected PickerElement Picker(State<int> selection, params string[] options)
            => new PickerElement(selection, options);

        protected LabelElement Label(string title, Sprite icon)
            => new LabelElement(title, icon);

        protected LabelElement Label(string title, UIElement icon)
        {
            UIContext.Current?.RemoveChild(icon);
            return new LabelElement(title, icon);
        }

        /// <summary>Flexible space along the stack main axis.</summary>
        protected SpacerElement Spacer(float minLength = 0) => new SpacerElement(minLength);

        protected UniftUI.TabView TabView(Action content, State<int> selectedIndex = null)
        {
            var tabView = new TabView(content, selectedIndex);
            UIContext.Current?.AddChild(tabView);
            return tabView;
        }

        protected TabItem Tab(Action titleContent, Action content) => new TabItem(titleContent, content);

        protected TabItem Tab(string title, Action content) => new TabItem(title, content);

        protected SliderElement Slider(State<int> value, float minValue, float maxValue)
            => new SliderElement(value, minValue, maxValue);

        protected SliderElement Slider(State<float> value, float minValue, float maxValue)
            => new SliderElement(value, minValue, maxValue);

        protected ScrollViewElement ScrollView(Action content, State[] states = null, bool horizontal = false, bool vertical = true)
        {
            var scrollView = new ScrollViewElement(content, states, horizontal, vertical);
            UIContext.Current?.AddChild(scrollView);
            return scrollView;
        }

        protected ToggleElement Toggle(State<bool> isOn, string label, Action<bool> onValueChanged = null)
            => new ToggleElement(isOn, label, onValueChanged);

        protected ToggleElement Toggle(string title, State<bool> isOn, Action<bool> onValueChanged = null)
            => new ToggleElement(isOn, title, onValueChanged);

        protected ToggleElement Toggle(State<bool> isOn, TextElement label, Action<bool> onValueChanged = null)
        {
            UIContext.Current?.RemoveChild(label);
            return new ToggleElement(isOn, label != null ? label.PlainText : string.Empty, onValueChanged);
        }

        protected ZStackElement ZStack(Action content) => ZStack(content, null, ZStackAlignment.Center);

        protected ZStackElement ZStack(Action content, State[] states = null)
            => ZStack(content, states, ZStackAlignment.Center);

        protected ZStackElement ZStack(Action content, State[] states = null, ZStackAlignment alignment = ZStackAlignment.Center)
        {
            var stack = new ZStackElement(content, states, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        protected TextFieldElement TextField(string title, State<string> text, TextElement prompt = null, Action<string> onTextChanged = null)
        {
            UIContext.Current?.RemoveChild(prompt);
            return new TextFieldElement(title, text, prompt, onTextChanged);
        }

        protected TextFieldElement TextField(string title, State<string> text, Axis axis, TextElement prompt = null,
            Action<string> onTextChanged = null)
        {
            UIContext.Current?.RemoveChild(prompt);
            return new TextFieldElement(title, text, prompt, onTextChanged).SetAxis(axis);
        }

        protected TextEditorElement TextEditor(State<string> text, Action<string> onTextChanged = null)
            => new TextEditorElement(text, onTextChanged);

        protected TextFieldElement SecureField(string title, State<string> text, TextElement prompt = null, Action<string> onTextChanged = null)
        {
            UIContext.Current?.RemoveChild(prompt);
            var field = new TextFieldElement(title, text, prompt, onTextChanged);
            field.SetContentType(TMPro.TMP_InputField.ContentType.Password);
            return field;
        }

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

        /// <summary>Creates a linear animation.</summary>
        protected Animation linear(float duration = 0.35f) => Animation.linear(duration);

        /// <summary>Creates an ease-in animation.</summary>
        protected Animation easeIn(float duration = 0.35f) => Animation.easeIn(duration);

        /// <summary>Creates an ease-out animation.</summary>
        protected Animation easeOut(float duration = 0.35f) => Animation.easeOut(duration);

        /// <summary>Creates an ease-in-out animation.</summary>
        protected Animation easeInOut(float duration = 0.35f) => Animation.easeInOut(duration);

        /// <summary>Creates a spring animation.</summary>
        protected Animation spring(float response = 0.5f, float dampingFraction = 0.825f)
            => Animation.spring(response, dampingFraction);

        /// <summary>Creates an interactive spring animation.</summary>
        protected Animation interactiveSpring(float response = 0.15f, float dampingFraction = 0.86f)
            => Animation.interactiveSpring(response, dampingFraction);

        /// <summary>Creates a bouncy animation.</summary>
        protected Animation bouncy(float duration = 0.5f) => Animation.bouncy(duration);
    }
}
