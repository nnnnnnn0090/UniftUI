# UniftUI — code examples

[日本語](./Example-ja.md)

A step-by-step guide for your first screen. You write UI in C#, then call `.Build(canvas)` to create Unity uGUI objects.

## Before you start (Unity Editor)

1. Create or open a scene with a **Canvas** (Screen Space is fine).
2. Add an empty GameObject (e.g. `MyUI`) and attach your script that inherits **`UniftView`**.
3. Put `using UniftUI;` at the top of the script.
4. In `Start()`, describe the UI and end with **`.Build(GetComponent<Canvas>())`** on the same GameObject as the Canvas.

If nothing appears, check that the script is on the **same GameObject as the Canvas**, and that `Start()` runs without errors in the Console.

## The basic pattern

Every screen follows this shape:

1. **Inherit** `UniftView` — gives you helpers like `VStack`, `Text`, `Button`.
2. **Describe** the tree inside a layout block (`VStack(() => { ... })`).
3. **Build** — `.Build(GetComponent<Canvas>())` turns the description into real `RectTransform` / uGUI objects.

```csharp
using UnityEngine;
using UniftUI;

public class MyView : UniftView
{
    void Start()
    {
        VStack(() =>
        {
            Text("Hello, UniftUI!");
        })
        .Build(GetComponent<Canvas>());
    }
}
```

**Tip:** Nesting works naturally — put `HStack` inside `VStack`, put `Text` inside either, and so on.

**Advanced:** Outside `UniftView`, you can use `UIElements.VStack(...)` etc., but you must respect `UIContext` (parent/child rules). Beginners should stay on `UniftView` until comfortable.

---

## Layout stacks

Stacks arrange children in a line (vertical or horizontal) or on top of each other.

### VStack — top to bottom

Use for menus, forms, lists stacked vertically.

| Parameter | Meaning |
|-----------|---------|
| `spacing` | Gap between children (pixels) |
| `alignment` | How children align on the cross axis (e.g. `Leading` = left in a vertical stack) |

```csharp
VStack(() =>
{
    Text("Title");
    Text("Subtitle");
    Button("Continue", () => Debug.Log("next"));
}, spacing: 12f, alignment: VStackAlignment.Leading);
```

### HStack — left to right

Use for toolbars, rows of buttons, label + value on one line.

```csharp
HStack(() =>
{
    Text("Left");
    Spacer(8);   // pushes "Right" toward the trailing edge when there is extra width
    Text("Right");
}, spacing: 16f, alignment: HStackAlignment.Center);
```

### ZStack — layered (back to front)

Use when something should sit on top of something else (background + label, badge on icon).

```csharp
ZStack(() =>
{
    Image(mySprite).Opacity(0.5f);   // back
    Text("On top");                  // front
}, alignment: ZStackAlignment.Center);
```

---

## Grid

A **table**: rows (`GridRow`) inside a `Grid`. Good for keypad-style layouts or aligned columns.

- `GridRow` must be used **inside** `Grid` (not alone).
- Column widths sync across rows.

```csharp
Grid(() =>
{
    GridRow(() =>
    {
        Text("A");
        Text("B");
    });
    GridRow(() =>
    {
        Text("C");
        Text("D");
    });
}, horizontalSpacing: 8f, verticalSpacing: 8f);
```

---

## Text

| Form | When to use |
|------|-------------|
| `Text("fixed")` | String never changes |
| `Text(myStringState)` | String stored in `State<string>` |
| `Text(() => "…", new State[] { x })` | Rebuild when `x` changes (computed string) |

```csharp
// Simple
Text("Hello");

// Reactive label (recreates/updates when state changes)
var score = new State<int>(0);
Text(() => $"Score: {score.Value}", new State[] { score });
```

**Look and feel** — chain modifiers after `Text(...)`:

```csharp
Text("Title")
    .FontSize(24)
    .Bold()
    .ForegroundColor(Color.white)
    .Font(myTmpFontAsset);   // TMP font asset
```

---

## Button

First argument: label or custom content. Second: code to run when clicked.

```csharp
Button("OK", () => Debug.Log("clicked"));

// Colors without extra modifiers
Button("Delete", () => { }, backgroundColor: Color.red, textColor: Color.white);

// Icon button: pass any UIElement as the label
Button(
    Image(iconSprite).Frame(width: 24, height: 24),
    () => Debug.Log("icon tap"));
```

---

## Image

Needs a **Sprite** (assign in the Inspector or load from `Resources`).

```csharp
Image(sprite)
    .TintColor(Color.white)
    .ScaleMode(ImageScaleMode.AspectFit)   // Fit inside the frame without cropping
    .Opacity(0.9f);
```

---

## Spacer

Flexible empty space in a stack (`Spacer`).

- **`Spacer(8)`** — minimum size 8 on the main axis; **grows** if the parent has extra room.
- For a **fixed** gap (always 8 px), use `.Padding()` or `.Frame()` instead.

---

## TabView

Tabs at the top; content switches when the user picks a tab.

```csharp
var selectedTab = new State<int>(0);   // 0 = first tab

TabView(() =>
{
    Tab("Home", () => { Text("Home content"); });
    Tab("Settings", () => { Text("Settings content"); });
}, selectedTab);
```

---

## Slider

Binds to **`State<int>`** (or the value you pass). User drags → state updates → bound UI refreshes.

```csharp
var volume = new State<int>(50);
Slider(volume, minValue: 0, maxValue: 100);
```

---

## Toggle

Checkbox-style on/off bound to **`State<bool>`**.

```csharp
var soundOn = new State<bool>(true);
Toggle(soundOn, "Sound", isOn => Debug.Log($"Sound: {isOn}"));
```

---

## TextField

Single-line or multi-line input. Text lives in **`State<string>`**.

```csharp
var name = new State<string>("");
TextField(name, "Enter your name…", value => Debug.Log(value))
    .FontSize(18)
    .LineLimit(3);   // optional max lines
```

---

## ScrollView

When content is taller (or wider) than the visible area.

```csharp
ScrollView(() =>
{
    for (int i = 0; i < 40; i++)
        Text($"Row {i}");
}, horizontal: false, vertical: true)
    .ScrollIndicators(ScrollIndicatorVisibility.Hidden)
    .ScrollBounce(true);
```

---

## ForEach — repeat without copy-paste

### Inclusive range (both ends included)

`ForEach(0, 9, …)` → `i` is 0, 1, …, **9** (10 items).

```csharp
ForEach(0, 9, i =>
{
    Button(i.ToString(), () => Debug.Log(i));
});
```

### Half-open range (C# `Range`)

`ForEach(0..10, …)` → `i` is 0 … **9** (10 items). Same idea as `for (int i = 0; i < 10; i++)`.

```csharp
ForEach(0..10, i =>
{
    Button(i.ToString(), () => { });
});
```

### List or array

```csharp
ForEach(items, item =>
{
    Text(item.Name);
});
```

---

## State & animation

### What is `State<T>`?

A small reactive value. When you assign **`state.Value = …`**, anything that depends on that state (via `State[]` or bindings) can update.

```csharp
var counter = new State<int>(0);

Button("+1", () => counter.Value++);

Text(() => $"Count: {counter.Value}", new State[] { counter });
```

### Animate a change

Wrap updates in **`WithAnimation`**:

```csharp
var width = new State<float>(100f);

WithAnimation(Animation.easeInOut(0.25f), () =>
{
    width.Value = 200f;
});

// Or: animate this view when `width` changes
SomeElement()
    .Animation(Animation.Default, width);
```

---

## Common modifiers (chain after any element)

Modifiers return the element (or a wrapper), so you can chain them:

```csharp
Text("Card")
    .Padding(16)                              // inner margin
    .Background(Color.blue)                   // fill behind content
    .CornerRadius(8)                          // rounded rect (UniftUI)
    .Frame(width: 200, height: 48)            // preferred size
    .Offset(0, -4);                           // shift position

Text("Soft shadow")
    .Shadow(Color.black, radius: 2f, x: 1f, y: -1f);

Text("Tilted")
    .ScaleEffect(1.1f)
    .RotationEffect(5f);

Text("Fade in")
    .Opacity(0.85f)
    .OnAppear(() => Debug.Log("now visible"));
```

**`FixedSize`** — do not stretch to fill the parent; size to content:

```csharp
Text("Hugs content").FixedSize(horizontal: true, vertical: true);
```

---

## Quick reference

| Goal | API |
|------|-----|
| Vertical list | `VStack` |
| Horizontal row | `HStack` |
| Overlay | `ZStack` |
| Table | `Grid` + `GridRow` |
| Tap action | `Button` |
| User text input | `TextField` + `State<string>` |
| Scroll long content | `ScrollView` |
| Live data | `State<T>` + `Text(() => …, new State[] { … })` |
| Style | `.Padding` `.Background` `.Frame` `.FontSize` … |
