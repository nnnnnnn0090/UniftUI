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

        public static ButtonElement Button(Action action, string label) => new ButtonElement(label, action);

        public static ButtonElement Button(UIElement content, Action onClick)
        {
            UIContext.Current?.RemoveChild(content);
            return new ButtonElement(content, onClick);
        }

        public static ButtonElement Button(Action action, UIElement label)
        {
            UIContext.Current?.RemoveChild(label);
            return new ButtonElement(label, action);
        }

        public static ImageElement Image(Sprite sprite) => new ImageElement(sprite);

        public static RectangleElement Rectangle() => new RectangleElement(UnityEngine.Color.white);

        public static RectangleElement Rectangle(Color color) => new RectangleElement(color);

        public static CircleElement Circle() => new CircleElement(UnityEngine.Color.white);

        public static CircleElement Circle(Color color) => new CircleElement(color);

        public static CapsuleElement Capsule() => new CapsuleElement(UnityEngine.Color.white);

        public static CapsuleElement Capsule(Color color) => new CapsuleElement(color);

        public static RoundedRectangleElement RoundedRectangle(float cornerRadius)
            => new RoundedRectangleElement(cornerRadius, UnityEngine.Color.white);

        public static RoundedRectangleElement RoundedRectangle(float cornerRadius, Color color)
            => new RoundedRectangleElement(cornerRadius, color);

        public static RectangleElement Color(Color color) => new RectangleElement(color);

        public static DividerElement Divider() => new DividerElement();

        public static DividerElement Divider(Color color, float thickness = 1f) => new DividerElement(color, thickness);

        public static ProgressViewElement ProgressView(State<float> value, float total = 1f)
            => new ProgressViewElement(value, total);

        public static StepperElement Stepper(State<int> value, int minValue, int maxValue, int step = 1)
            => new StepperElement(value, minValue, maxValue, step);

        public static StepperElement Stepper(string label, State<int> value, int minValue, int maxValue, int step = 1)
            => new StepperElement(value, minValue, maxValue, step, label);

        public static PickerElement Picker(State<int> selection, params string[] options)
            => new PickerElement(selection, options);

        public static LabelElement Label(string title, Sprite icon)
            => new LabelElement(title, icon);

        public static LabelElement Label(string title, UIElement icon)
        {
            UIContext.Current?.RemoveChild(icon);
            return new LabelElement(title, icon);
        }

        public static VStackElement VStack(Action content, State[] states = null, float spacing = 8f,
            VStackAlignment alignment = VStackAlignment.Center)
        {
            var stack = new VStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        public static HStackElement HStack(Action content, State[] states = null, float spacing = 8f,
            HStackAlignment alignment = HStackAlignment.Center)
        {
            var stack = new HStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        public static LazyVStackElement LazyVStack(Action content, State[] states = null, float spacing = 8f,
            VStackAlignment alignment = VStackAlignment.Center)
        {
            var stack = new LazyVStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        public static LazyHStackElement LazyHStack(Action content, State[] states = null, float spacing = 8f,
            HStackAlignment alignment = HStackAlignment.Center)
        {
            var stack = new LazyHStackElement(content, states, spacing, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        public static ZStackElement ZStack(Action content, State[] states = null,
            ZStackAlignment alignment = ZStackAlignment.Center)
        {
            var stack = new ZStackElement(content, states, alignment);
            UIContext.Current?.AddChild(stack);
            return stack;
        }

        public static GridElement Grid(Action content, State[] states = null, float horizontalSpacing = 8f,
            float verticalSpacing = 8f, HStackAlignment rowAlignment = HStackAlignment.Center)
        {
            var grid = new GridElement(content, states, horizontalSpacing, verticalSpacing, rowAlignment);
            UIContext.Current?.AddChild(grid);
            return grid;
        }

        public static GridRowElement GridRow(Action content)
        {
            if (UIContext.Current is GridElement grid)
                return new GridRowElement(grid, content);
            Debug.LogWarning("[UniftUI] GridRow must be used inside Grid content.");
            return new GridRowElement(null, content);
        }

        public static GeometryReaderElement GeometryReader(Func<GeometryProxy, UIElement> content)
        {
            var reader = new GeometryReaderElement(content);
            UIContext.Current?.AddChild(reader);
            return reader;
        }

        public static SpacerElement Spacer(float minLength = 0) => new SpacerElement(minLength);

        public static SliderElement Slider(State<int> value, float minValue, float maxValue)
            => new SliderElement(value, minValue, maxValue);

        public static SliderElement Slider(State<float> value, float minValue, float maxValue)
            => new SliderElement(value, minValue, maxValue);

        public static ScrollViewElement ScrollView(Action content, State[] states = null, bool horizontal = false, bool vertical = true)
        {
            var scrollView = new ScrollViewElement(content, states, horizontal, vertical);
            UIContext.Current?.AddChild(scrollView);
            return scrollView;
        }

        public static ToggleElement Toggle(State<bool> isOn, string label, Action<bool> onValueChanged = null)
            => new ToggleElement(isOn, label, onValueChanged);

        public static ToggleElement Toggle(string title, State<bool> isOn, Action<bool> onValueChanged = null)
            => new ToggleElement(isOn, title, onValueChanged);

        public static ToggleElement Toggle(State<bool> isOn, TextElement label, Action<bool> onValueChanged = null)
        {
            UIContext.Current?.RemoveChild(label);
            return new ToggleElement(isOn, label != null ? labelText(label) : string.Empty, onValueChanged);
        }

        public static TextFieldElement TextField(string title, State<string> text, TextElement prompt = null, Action<string> onTextChanged = null)
        {
            UIContext.Current?.RemoveChild(prompt);
            return new TextFieldElement(title, text, prompt, onTextChanged);
        }

        public static TextFieldElement TextField(string title, State<string> text, Axis axis, TextElement prompt = null,
            Action<string> onTextChanged = null)
        {
            UIContext.Current?.RemoveChild(prompt);
            return new TextFieldElement(title, text, prompt, onTextChanged).SetAxis(axis);
        }

        public static TextEditorElement TextEditor(State<string> text, Action<string> onTextChanged = null)
            => new TextEditorElement(text, onTextChanged);

        public static TextFieldElement SecureField(string title, State<string> text, TextElement prompt = null, Action<string> onTextChanged = null)
        {
            UIContext.Current?.RemoveChild(prompt);
            var field = new TextFieldElement(title, text, prompt, onTextChanged);
            field.SetContentType(TMPro.TMP_InputField.ContentType.Password);
            return field;
        }

        public static TabView TabView(Action content, State<int> selectedIndex = null)
        {
            var tabView = new TabView(content, selectedIndex);
            UIContext.Current?.AddChild(tabView);
            return tabView;
        }

        public static TabItem Tab(Action titleContent, Action content) => new TabItem(titleContent, content);

        public static TabItem Tab(string title, Action content) => new TabItem(title, content);

        private static string labelText(TextElement label)
        {
            return label != null ? label.PlainText : string.Empty;
        }
    }
}
