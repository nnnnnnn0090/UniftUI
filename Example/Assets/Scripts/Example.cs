using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniftUI;

public sealed class Example : UniftView
{
    private static readonly string[] Sections =
    {
        "Components",
        "Text",
        "TextField",
        "Layout",
        "Visual Modifiers",
        "Transforms",
        "Animation",
        "ScrollView",
        "Image",
        "Composition"
    };

    private static readonly string[] SectionNotes =
    {
        "Controls and small factories: Button, Toggle, Slider, ProgressView, Stepper, Picker, Label, Divider, and shapes.",
        "Text overloads, line limits, alignment, and typography modifiers.",
        "TextField chrome, caret, selection, keyboard, content, and multiline modifiers.",
        "VStack, HStack, Lazy stacks, ZStack, Grid, GeometryReader, Spacer, alignment, and sizing.",
        "Frame, AspectRatio, Padding, Background, ForegroundColor, Tint, Border, Overlay, ClipShape, CornerRadius, Shadow, Opacity, Disabled.",
        "Offset, Position, RotationEffect, and ScaleEffect.",
        "WithAnimation and .animation(...) modifier variants.",
        "Vertical, horizontal, and two-axis scrolling modifiers.",
        "Image factory, scale modes, resizing modes, and template tint.",
        "TabView, Tab, ForEach, OnAppear, OnChange, and Update."
    };

    private TMP_FontAsset font;
    private Texture2D tileTexture;
    private Texture2D gradientTexture;
    private Sprite tileSprite;
    private Sprite gradientSprite;

    private readonly State<int> section = new State<int>(0);
    private readonly State<int> count = new State<int>(0);
    private readonly State<int> sliderValue = new State<int>(42);
    private readonly State<float> sliderFloat = new State<float>(0.35f);
    private readonly State<float> progressValue = new State<float>(0.35f);
    private readonly State<int> stepperValue = new State<int>(2);
    private readonly State<int> pickerSelection = new State<int>(0);
    private readonly State<int> tab = new State<int>(0);
    private readonly State<int> rows = new State<int>(10);
    private readonly State<int> appearCount = new State<int>(0);
    private readonly State<string> textValue = new State<string>("Bound text");
    private readonly State<string> email = new State<string>("");
    private readonly State<string> search = new State<string>("");
    private readonly State<string> notes = new State<string>("");
    private readonly State<string> secureText = new State<string>("");
    private readonly State<string> chromeText = new State<string>("Editable text");
    private readonly State<string> selectionText = new State<string>("Select this text");
    private readonly State<string> submitStatus = new State<string>("Submit has not run");
    private readonly State<bool> flagA = new State<bool>(true);
    private readonly State<bool> flagB = new State<bool>(false);
    private readonly State<bool> disabledDemo = new State<bool>(false);
    private readonly State<bool> chromeFocused = new State<bool>(false);
    private readonly State<Color> accent = new State<Color>(new Color(38 / 255f, 94 / 255f, 190 / 255f));
    private readonly State<Color> fill = new State<Color>(new Color(236 / 255f, 244 / 255f, 255 / 255f));
    private readonly State<Color> selectionFill = new State<Color>(new Color(26 / 255f, 92 / 255f, 220 / 255f, 0.28f));
    private readonly State<int> paddingAmount = new State<int>(10);
    private readonly State<float> alpha = new State<float>(1f);
    private readonly State<float> radius = new State<float>(10f);
    private readonly State<float> width = new State<float>(180f);
    private readonly State<float> height = new State<float>(38f);
    private readonly State<float> rotation = new State<float>(0f);
    private readonly State<float> rotationX = new State<float>(0f);
    private readonly State<float> rotationY = new State<float>(0f);
    private readonly State<float> scale = new State<float>(1f);
    private readonly State<float> scaleX = new State<float>(1f);
    private readonly State<float> scaleY = new State<float>(1f);
    private readonly State<Vector3> rotation3 = new State<Vector3>(Vector3.zero);
    private readonly State<Vector3> scale3 = new State<Vector3>(Vector3.one);
    private readonly State<float> offsetX = new State<float>(0f);
    private readonly State<Vector2> offset2 = new State<Vector2>(Vector2.zero);
    private readonly State<Vector2> position = new State<Vector2>(new Vector2(16f, 16f));
    private readonly State<float> positionX = new State<float>(20f);
    private readonly State<float> positionY = new State<float>(20f);
    private readonly State<float> scrollY = new State<float>(1f);
    private readonly State<float> scrollX = new State<float>(0f);
    private readonly State<string> updateText = new State<string>("Update has not ticked yet");
    private readonly State<string> changeStatus = new State<string>("No change yet");

    private int updateTicks;

    public void Start()
    {
        PrepareHost();
        font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");
        UIContext.SetDefaultFont(font);
        BuildSprites();
        Draw();
    }

    private void OnDestroy()
    {
        if (tileSprite != null) Destroy(tileSprite);
        if (gradientSprite != null) Destroy(gradientSprite);
        if (tileTexture != null) Destroy(tileTexture);
        if (gradientTexture != null) Destroy(gradientTexture);
    }

    private void Draw()
    {
        HStack(() =>
        {
            Sidebar().frame(width: 220f, infiniteHeight: true);

            VStack(() =>
            {
                TopBar();

                ScrollView(() =>
                {
                    VStack(() =>
                    {
                        PageTitle(Sections[section.Value], SectionNotes[section.Value]);
                        DrawSection();
                        Spacer().frame(height: 28f);
                    }, null, 14f, VStackAlignment.Leading)
                    .padding(18)
                    .frame(infiniteWidth: true);
                }, null, false, true)
                .scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical)
                .frame(infiniteWidth: true, infiniteHeight: true);
            }, null, 0f, VStackAlignment.Leading)
            .frame(infiniteWidth: true, infiniteHeight: true);
        }, new State[] { section }, 0f, HStackAlignment.Top)
        .frame(infiniteWidth: true, infiniteHeight: true)
        .background(new Color(244 / 255f, 247 / 255f, 251 / 255f))
        .Build(GetComponent<Canvas>());
    }

    private UIElement Sidebar()
    {
        return VStack(() =>
        {
            Text("UniftUI")
                .bold()
                .fontSize(22)
                .foregroundColor(new Color(20 / 255f, 28 / 255f, 42 / 255f))
                .padding(top: 18f, bottom: 2f, left: 14f, right: 14f);

            Text("API Test Gallery")
                .fontSize(12)
                .foregroundColor(new Color(88 / 255f, 100 / 255f, 118 / 255f))
                .padding(top: 0f, bottom: 12f, left: 14f, right: 14f);

            for (int i = 0; i < Sections.Length; i++)
                SectionButton(i);

            Spacer();

            Text("State")
                .bold()
                .fontSize(12)
                .foregroundColor(new Color(76 / 255f, 88 / 255f, 108 / 255f))
                .padding(left: 14f, right: 14f);

            StateReadout()
                .padding(top: 6f, bottom: 14f, left: 14f, right: 14f);
        }, null, 4f, VStackAlignment.Leading)
        .background(Color.white)
        .frame(infiniteWidth: true, infiniteHeight: true);
    }

    private void TopBar()
    {
        HStack(() =>
        {
            Text(() => Sections[section.Value], new State[] { section })
                .bold()
                .fontSize(15)
                .foregroundColor(new Color(24 / 255f, 32 / 255f, 46 / 255f));

            Spacer();

                Text(() => $"count={count.Value} slider={sliderValue.Value} flags={flagA.Value}/{flagB.Value}",
                    new State[] { count, sliderValue, flagA, flagB })
                .fontSize(12)
                .foregroundColor(new Color(76 / 255f, 88 / 255f, 108 / 255f));

            Button("Reset", ResetState)
                .frame(width: 82f, height: 34f)
                .background(new Color(226 / 255f, 232 / 255f, 240 / 255f))
                .cornerRadius(8f);
        }, null, 12f, HStackAlignment.Center)
        .padding(top: 10f, bottom: 10f, left: 16f, right: 16f)
        .background(Color.white)
        .frame(infiniteWidth: true, height: 56f);
    }

    private void DrawSection()
    {
        switch (section.Value)
        {
            case 0: ComponentsSection(); break;
            case 1: TextSection(); break;
            case 2: TextFieldSection(); break;
            case 3: LayoutSection(); break;
            case 4: VisualModifiersSection(); break;
            case 5: TransformSection(); break;
            case 6: AnimationSection(); break;
            case 7: ScrollViewSection(); break;
            case 8: ImageSection(); break;
            case 9: CompositionSection(); break;
        }
    }

    private void ComponentsSection()
    {
        Card("Controls, shapes, and utilities", () =>
        {
            ApiRow("Button(\"+1\", action)", () =>
            {
                Button("+1", () => count.Value++)
                    .frame(width: 82f, height: 34f)
                    .background(new Color(224 / 255f, 235 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow("Button(UIElement, action)", () =>
            {
                Button(
                    HStack(() =>
                    {
                        Text("Count").bold().fontSize(13);
                        Text(() => count.Value.ToString(), new State[] { count })
                            .fontSize(12)
                            .foregroundColor(Color.white)
                            .padding(6)
                            .background(accent)
                            .cornerRadius(12f);
                    }, 6f),
                    () => count.Value++)
                    .padding(8)
                    .background(new Color(244 / 255f, 247 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow("Toggle(State<bool>, label)", () =>
            {
                Toggle(flagA, "flagA").frame(width: 180f, height: 34f);
            });

            ApiRow("Slider(State<int>, min, max)", () =>
            {
                Slider(sliderValue, 0, 100)
                    .WithColors(new Color(50 / 255f, 114 / 255f, 220 / 255f), new Color(20 / 255f, 54 / 255f, 120 / 255f))
                    .WithHandleSize(24f, 24f)
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow("Slider(State<float>, min, max)", () =>
            {
                Slider(sliderFloat, 0f, 1f)
                    .WithColors(new Color(88 / 255f, 136 / 255f, 255 / 255f), new Color(35 / 255f, 68 / 255f, 142 / 255f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow("ProgressView(State<float>, total)", () =>
            {
                VStack(() =>
                {
                    ProgressView(progressValue, 1f)
                        .tint(accent)
                        .frame(infiniteWidth: true, height: 10f);

                    Text(() => progressValue.Value.ToString("0%"), new State[] { progressValue })
                        .fontSize(12)
                        .foregroundColor(new Color(76 / 255f, 88 / 255f, 108 / 255f));
                }, null, 6f, VStackAlignment.Leading)
                .frame(infiniteWidth: true);
            });

            ApiRow("Stepper(State<int>, min, max)", () =>
            {
                Stepper("Quantity", stepperValue, 0, 8)
                    .tint(accent)
                    .frame(infiniteWidth: true, height: 34f);
            });

            ApiRow("Picker(selection, options).pickerStyle(PickerStyle.Segmented)", () =>
            {
                Picker(pickerSelection, "One", "Two", "Three")
                    .pickerStyle(PickerStyle.Segmented)
                    .tint(accent)
                    .frame(width: 260f, height: 34f);
            });

            ApiRow("Rectangle() / Rectangle(Color)", () =>
            {
                HStack(() =>
                {
                    Rectangle()
                        .foregroundColor(new Color(224 / 255f, 235 / 255f, 255 / 255f))
                        .frame(width: 70f, height: 34f)
                        .cornerRadius(8f);
                    Rectangle(new Color(225 / 255f, 244 / 255f, 231 / 255f))
                        .frame(width: 70f, height: 34f)
                        .cornerRadius(8f);
                }, 8f);
            });

            ApiRow("Circle() / Capsule() / RoundedRectangle(radius)", () =>
            {
                HStack(() =>
                {
                    Circle(new Color(224 / 255f, 235 / 255f, 255 / 255f))
                        .frame(width: 34f, height: 34f);
                    Capsule(new Color(225 / 255f, 244 / 255f, 231 / 255f))
                        .frame(width: 92f, height: 34f);
                    RoundedRectangle(10f, new Color(255 / 255f, 236 / 255f, 210 / 255f))
                        .frame(width: 92f, height: 34f);
                }, 8f);
            });

            ApiRow("Label(title, icon)", () =>
            {
                Label("Status", Circle(accent.Value).frame(width: 12f, height: 12f))
                    .foregroundColor(new Color(24 / 255f, 32 / 255f, 44 / 255f))
                    .frame(width: 120f, height: 28f);
            });

            ApiRow("Divider()", () =>
            {
                VStack(() =>
                {
                    Text("Above").fontSize(12);
                    Divider();
                    Text("Below").fontSize(12);
                }, null, 4f, VStackAlignment.Leading)
                .frame(infiniteWidth: true);
            });

            HStack(() =>
            {
                Button("progress", () => progressValue.Value = progressValue.Value > 0.75f ? 0.2f : progressValue.Value + 0.2f);
                Button("picker", () => pickerSelection.Value = (pickerSelection.Value + 1) % 3);
            }, 8f);
        });
    }

    private void TextSection()
    {
        Card("Text overloads", () =>
        {
            ApiRow("Text(string)", () => Text("Plain string").fontSize(16));

            ApiRow("Text(Func<string>, State[])", () =>
            {
                Text(() => $"count = {count.Value}", new State[] { count })
                    .fontSize(16)
                    .foregroundColor(accent);
            });

            ApiRow("Text(State<string>)", () =>
            {
                Text(textValue).fontSize(16);
            });
        });

        Card("Text modifiers", () =>
        {
            ApiRow("Fluent aliases: .italic().foregroundColor().padding()", () =>
            {
                Text("lower camel aliases")
                    .italic()
                    .foregroundColor(accent)
                    .padding(8)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });
            ApiRow(".fontSize(22)", () => Text("FontSize").fontSize(22));
            ApiRow(".font(font)", () => Text("Font").font(font).fontSize(18));
            ApiRow(".foregroundColor(Color)", () => Text("ForegroundColor").fontSize(18).foregroundColor(new Color(30 / 255f, 92 / 255f, 180 / 255f)));
            ApiRow(".bold() .italic()", () => Text("Bold Italic").bold().italic().fontSize(18));
            ApiRow(".underline()", () => Text("Underline").underline().fontSize(18));
            ApiRow(".strikethrough()", () => Text("Strikethrough").strikethrough().fontSize(18));
            ApiRow(".lineLimit(1)", () =>
            {
                Text("This text is intentionally long and should stay on one line.")
                    .lineLimit(1)
                    .frame(width: 220f)
                    .fontSize(16);
            });
            ApiRow(".lineLimit(2) .multilineTextAlignment(...)", () =>
            {
                Text("Leading aligned text wraps into two visible lines when the width is constrained.")
                    .lineLimit(2)
                    .multilineTextAlignment(TextAlignmentOptions.TopLeft)
                    .frame(width: 230f)
                    .fontSize(16);
            });
            ApiRow(".fixedSize()", () =>
            {
                Text("FixedSize text")
                    .fixedSize()
                    .padding(8)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".fixedSize(horizontal, vertical)", () =>
            {
                Text("Fixed vertically")
                    .fixedSize(horizontal: false, vertical: true)
                    .padding(8)
                    .background(new Color(225 / 255f, 244 / 255f, 231 / 255f))
                    .cornerRadius(8f)
                    .frame(infiniteWidth: true);
            });
        });
    }

    private void TextFieldSection()
    {
        Card("TextField and TextEditor", () =>
        {
            ApiRow("TextField(\"Title\", text: state, prompt: Text(...))", () =>
            {
                TextField(
                        "Name",
                        text: textValue,
                        prompt: Text("Single line").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .contentMargins(horizontal: 12f, vertical: 8f)
                    .textFieldStyle(TextFieldStyles.Chrome(backgroundColor: Color.white, cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow("SecureField(\"Title\", text: state, prompt: Text(...))", () =>
            {
                SecureField(
                        "Password",
                        text: secureText,
                        prompt: Text("Password").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .contentMargins(horizontal: 12f, vertical: 8f)
                    .tint(accent)
                    .textFieldStyle(TextFieldStyles.Chrome(backgroundColor: Color.white, cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow("TextField(..., axis: Axis.Vertical)", () =>
            {
                TextField(
                        "Notes",
                        text: notes,
                        axis: Axis.Vertical,
                        prompt: Text("Vertical TextField").italic().foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(3)
                    .multilineTextAlignment(TextAlignmentOptions.TopLeft)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(252 / 255f, 253 / 255f, 255 / 255f),
                        contentMargins: new Vector4(12f, 12f, 10f, 10f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 82f);
            });

            ApiRow("TextEditor(text: state)", () =>
            {
                TextEditor(notes)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: Color.white,
                        focusedBackgroundColor: new Color(239 / 255f, 246 / 255f, 255 / 255f),
                        contentMargins: new Vector4(12f, 12f, 10f, 10f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 96f);
            });

            ApiRow(".textContentType(...) .textInputLimit(...) .keyboardType(...)", () =>
            {
                TextField(
                        "Email",
                        text: email,
                        prompt: Text("email@example.com").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .textContentType(TMP_InputField.ContentType.EmailAddress)
                    .textInputLimit(40)
                    .keyboardType(TouchScreenKeyboardType.EmailAddress)
                    .contentMargins(horizontal: 12f, vertical: 8f)
                    .textFieldStyle(TextFieldStyles.Chrome(backgroundColor: Color.white, cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });
        });

        Card("TextField style and chrome", () =>
        {
            ApiRow(".background(Color) wrapper", () =>
            {
                TextField(
                        "Search",
                        text: search,
                        prompt: Text("outer wrapper fill").foregroundColor(new Color(1f, 1f, 1f, 0.45f)))
                    .lineLimit(1)
                    .contentMargins(horizontal: 12f, vertical: 8f)
                    .foregroundColor(Color.white)
                    .background(new Color(18 / 255f, 24 / 255f, 38 / 255f))
                    .cornerRadius(8f)
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow(".contentMargins(horizontal, vertical)", () =>
            {
                TextField("Search", text: search, prompt: Text("uniform inset").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .contentMargins(horizontal: 20f, vertical: 6f)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(243 / 255f, 232 / 255f, 255 / 255f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow(".textFieldStyle(TextFieldStyles.Chrome(...))", () =>
            {
                TextField("Focus", text: chromeText, prompt: Text("focus me").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(255 / 255f, 237 / 255f, 213 / 255f),
                        focusedBackgroundColor: new Color(187 / 255f, 247 / 255f, 208 / 255f),
                        contentMargins: new Vector4(12f, 12f, 8f, 8f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow("prompt: Text(\"...\").italic().foregroundColor(...)", () =>
            {
                TextField(
                        "Nickname",
                        text: new State<string>(""),
                        prompt: Text("italic placeholder")
                            .italic()
                            .foregroundColor(new Color(90 / 255f, 98 / 255f, 116 / 255f)))
                    .lineLimit(1)
                    .contentMargins(horizontal: 12f, vertical: 8f)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(248 / 255f, 250 / 255f, 252 / 255f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow(".tint .caretWidth .caretBlinkRate", () =>
            {
                TextField("Caret", text: chromeText, prompt: Text("caret").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .tint(new Color(220 / 255f, 72 / 255f, 52 / 255f))
                    .caretWidth(4)
                    .caretBlinkRate(1.2f)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(255 / 255f, 241 / 255f, 242 / 255f),
                        contentMargins: new Vector4(12f, 12f, 8f, 8f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow(".textSelectionColor(Color)", () =>
            {
                TextField("Selection", text: selectionText, prompt: Text("selection").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .textSelectionColor(new Color(250 / 255f, 204 / 255f, 21 / 255f, 0.45f))
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(254 / 255f, 249 / 255f, 195 / 255f),
                        contentMargins: new Vector4(12f, 12f, 8f, 8f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow(".textSelectionColor(State<Color>)", () =>
            {
                TextField("Selection", text: selectionText, prompt: Text("state selection").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .textSelectionColor(selectionFill)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(239 / 255f, 246 / 255f, 255 / 255f),
                        contentMargins: new Vector4(12f, 12f, 8f, 8f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow(".textFieldStyle(TextFieldStyles.RoundedBorder(...))", () =>
            {
                TextField("Styled", text: search, prompt: Text("styled field").foregroundColor(new Color(112 / 255f, 124 / 255f, 145 / 255f)))
                    .lineLimit(1)
                    .textFieldStyle(TextFieldStyles.RoundedBorder(
                        Color.white,
                        new Color(239 / 255f, 246 / 255f, 255 / 255f),
                        new Color(24 / 255f, 32 / 255f, 44 / 255f),
                        accent.Value,
                        10f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow(".contentMargins(left, right, top, bottom)", () =>
            {
                TextField("Inset", text: search, prompt: Text("large left inset").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .contentMargins(left: 28f, right: 12f, top: 8f, bottom: 8f)
                    .background(new Color(241 / 255f, 245 / 255f, 249 / 255f))
                    .cornerRadius(18f)
                    .frame(infiniteWidth: true, height: 38f);
            });

            HStack(() =>
            {
                Button("Accent", CycleAccent);
                Button("Selection", CycleSelectionFill);
            }, 8f);
        });

        Card("TextField focus and submission", () =>
        {
            ApiRow(".focused(State<bool>)", () =>
            {
                TextField("Focus", text: chromeText, prompt: Text("focused binding").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .focused(chromeFocused)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(248 / 255f, 250 / 255f, 252 / 255f),
                        focusedBackgroundColor: new Color(219 / 255f, 234 / 255f, 254 / 255f),
                        contentMargins: new Vector4(12f, 12f, 8f, 8f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);

                Text(() => $"focused = {chromeFocused.Value}", new State[] { chromeFocused })
                    .fontSize(12)
                    .foregroundColor(new Color(76 / 255f, 88 / 255f, 108 / 255f));
            });

            ApiRow(".selectAllOnFocus(false)", () =>
            {
                TextField("Selection", text: selectionText, prompt: Text("no select all").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .selectAllOnFocus(false)
                    .tint(accent)
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(255 / 255f, 247 / 255f, 237 / 255f),
                        contentMargins: new Vector4(12f, 12f, 8f, 8f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);
            });

            ApiRow(".onSubmit(...) .onEditingChanged(...)", () =>
            {
                TextField("Submit", text: chromeText, prompt: Text("submit").foregroundColor(new Color(94 / 255f, 104 / 255f, 124 / 255f)))
                    .lineLimit(1)
                    .onSubmit(value => submitStatus.Value = "submitted: " + value)
                    .onEditingChanged(editing => submitStatus.Value = editing ? "editing" : "not editing")
                    .textFieldStyle(TextFieldStyles.Chrome(
                        backgroundColor: new Color(236 / 255f, 253 / 255f, 245 / 255f),
                        contentMargins: new Vector4(12f, 12f, 8f, 8f),
                        cornerRadius: 8f))
                    .frame(infiniteWidth: true, height: 38f);

                Text(submitStatus)
                    .fontSize(12)
                    .foregroundColor(new Color(76 / 255f, 88 / 255f, 108 / 255f));
            });
        });
    }

    private void LayoutSection()
    {
        Card("Stacks", () =>
        {
            ApiRow("VStack(..., spacing, VStackAlignment.Leading)", () =>
            {
                VStack(() =>
                {
                    Chip("short", new Color(224 / 255f, 235 / 255f, 255 / 255f)).frame(width: 80f);
                    Chip("long item", new Color(225 / 255f, 244 / 255f, 231 / 255f)).frame(width: 130f);
                    Chip("mid", new Color(255 / 255f, 236 / 255f, 210 / 255f)).frame(width: 100f);
                }, null, 6f, VStackAlignment.Leading)
                .padding(8)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f)
                .frame(infiniteWidth: true);
            });

            ApiRow("HStack(..., spacing, HStackAlignment.Center)", () =>
            {
                HStack(() =>
                {
                    Chip("A", new Color(224 / 255f, 235 / 255f, 255 / 255f));
                    Chip("B", new Color(225 / 255f, 244 / 255f, 231 / 255f));
                    Chip("C", new Color(255 / 255f, 236 / 255f, 210 / 255f));
                }, null, 8f, HStackAlignment.Center)
                .frame(infiniteWidth: true, height: 44f);
            });

            ApiRow("LazyVStack(...) / LazyHStack(...)", () =>
            {
                HStack(() =>
                {
                    LazyVStack(() =>
                    {
                        ForEach(0, 2, i => Text("V" + i).frame(width: 54f, height: 24f).background(new Color(224 / 255f, 235 / 255f, 255 / 255f)).cornerRadius(6f));
                    }, null, 4f, VStackAlignment.Leading);

                    LazyHStack(() =>
                    {
                        ForEach(0, 2, i => Text("H" + i).frame(width: 54f, height: 24f).background(new Color(225 / 255f, 244 / 255f, 231 / 255f)).cornerRadius(6f));
                    }, null, 4f, HStackAlignment.Center);
                }, 10f)
                .frame(infiniteWidth: true);
            });

            ApiRow("ZStack(..., ZStackAlignment.BottomTrailing)", () =>
            {
                ZStack(() =>
                {
                    Spacer().frame(width: 180f, height: 72f).background(new Color(238 / 255f, 242 / 255f, 248 / 255f)).cornerRadius(8f);
                    Text("badge").fontSize(12).foregroundColor(Color.white).padding(6).background(accent).cornerRadius(10f);
                }, null, ZStackAlignment.BottomTrailing)
                .frame(width: 180f, height: 72f);
            });
        });

        Card("Grid / Spacer / Frame", () =>
        {
            ApiRow("Grid + GridRow", () =>
            {
                Grid(() =>
                {
                    for (int row = 0; row < 2; row++)
                    {
                        int capturedRow = row;
                        GridRow(() =>
                        {
                            for (int col = 0; col < 3; col++)
                            {
                                int n = capturedRow * 3 + col + 1;
                                Text(n.ToString())
                                    .bold()
                                    .frame(infiniteWidth: true, height: 34f)
                                    .background(n % 2 == 0 ? new Color(232 / 255f, 238 / 255f, 255 / 255f) : new Color(236 / 255f, 247 / 255f, 236 / 255f))
                                    .cornerRadius(8f);
                            }
                        });
                    }
                }, null, 8f, 8f, HStackAlignment.Center)
                .frame(infiniteWidth: true);
            });

            ApiRow("GeometryReader(proxy => ...)", () =>
            {
                GeometryReader(proxy =>
                    Text($"{proxy.Size.x:0} x {proxy.Size.y:0}")
                        .frame(infiniteWidth: true, infiniteHeight: true)
                        .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                        .cornerRadius(8f))
                .frame(infiniteWidth: true, height: 48f);
            });

            ApiRow("Spacer(minLength)", () =>
            {
                HStack(() =>
                {
                    Chip("Left", new Color(224 / 255f, 235 / 255f, 255 / 255f));
                    Spacer(24f);
                    Chip("Right", new Color(225 / 255f, 244 / 255f, 231 / 255f));
                }, null, 0f, HStackAlignment.Center)
                .frame(infiniteWidth: true, height: 42f);
            });

            ApiRow(".frame(width, height)", () =>
            {
                Text("120 x 40")
                    .frame(width: 120f, height: 40f)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".frame(State<float> width, State<float> height)", () =>
            {
                Text("State size")
                    .frame(width: width, height: height)
                    .background(new Color(225 / 255f, 244 / 255f, 231 / 255f))
                    .cornerRadius(8f)
                    .animation(easeInOut(0.25f), width, height);
            });

            ApiRow(".frame(infiniteWidth: true)", () =>
            {
                Text("fills row")
                    .frame(infiniteWidth: true, height: 38f)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".frame(infiniteHeight: true)", () =>
            {
                HStack(() =>
                {
                    Text("fills height")
                        .frame(width: 130f, infiniteHeight: true)
                        .background(new Color(255 / 255f, 236 / 255f, 210 / 255f))
                        .cornerRadius(8f);
                }, null, 8f, HStackAlignment.Center)
                .frame(infiniteWidth: true, height: 68f);
            });

            HStack(() =>
            {
                Button("width", () => width.Value = width.Value > 210f ? 150f : 260f);
                Button("height", () => height.Value = height.Value > 46f ? 34f : 58f);
            }, 8f);
        });
    }

    private void VisualModifiersSection()
    {
        Card("Common modifiers", () =>
        {
            ApiRow(".padding(12)", () =>
            {
                Text("Padding")
                    .padding(12)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".padding(State<int>)", () =>
            {
                Text("State Padding")
                    .padding(paddingAmount)
                    .background(new Color(225 / 255f, 244 / 255f, 231 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".padding(RectOffset)", () =>
            {
                Text("RectOffset Padding")
                    .padding(new RectOffset(22, 6, 4, 14))
                    .background(new Color(255 / 255f, 236 / 255f, 210 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".padding(top,bottom,left,right)", () =>
            {
                Text("Per-edge Padding")
                    .padding(top: 4f, bottom: 12f, left: 24f, right: 8f)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".background(Color)", () =>
            {
                Text("Background")
                    .padding(10)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".background(State<Color>)", () =>
            {
                Text("State background")
                    .padding(10)
                    .background(fill)
                    .cornerRadius(8f);
            });

            ApiRow(".foregroundColor(State<Color>)", () =>
            {
                Text("State foreground")
                    .bold()
                    .fontSize(18)
                    .foregroundColor(accent);
            });

            ApiRow(".tint(State<Color>)", () =>
            {
                HStack(() =>
                {
                    Button("Button", () => count.Value++)
                        .tint(accent)
                        .foregroundColor(Color.white)
                        .cornerRadius(8f)
                        .frame(width: 90f, height: 34f);
                    Toggle(flagB, "Toggle").tint(accent).frame(width: 140f, height: 34f);
                }, 10f);
            });

            ApiRow(".buttonStyle(ButtonStyles.Filled(...))", () =>
            {
                Button("Styled", () => count.Value++)
                    .buttonStyle(ButtonStyles.Filled(accent.Value, Color.white, 10f))
                    .frame(width: 110f, height: 34f);
            });

            ApiRow(".aspectRatio(16f / 9f)", () =>
            {
                Image(gradientSprite)
                    .resizable()
                    .scaledToFill()
                    .frame(width: 180f)
                    .aspectRatio(16f / 9f, AspectRatioContentMode.Fill)
                    .clipped()
                    .cornerRadius(8f);
            });

            ApiRow(".opacity(float)", () =>
            {
                Text("Opacity 0.55")
                    .padding(10)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f)
                    .opacity(0.55f);
            });

            ApiRow(".opacity(State<float>)", () =>
            {
                Text("Opacity")
                    .padding(10)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f)
                    .opacity(alpha);
            });

            ApiRow(".cornerRadius(State<float>)", () =>
            {
                Text("State radius")
                    .padding(10)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(radius)
                    .animation(easeInOut(0.25f), radius);
            });

            ApiRow(".border(Color, width)", () =>
            {
                Text("Border")
                    .padding(10)
                    .background(Color.white)
                    .border(accent.Value, 2f)
                    .cornerRadius(8f);
            });

            ApiRow(".overlay(..., alignment)", () =>
            {
                Text("Overlay")
                    .padding(12)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f)
                    .overlay(
                        Text("NEW")
                            .fontSize(10)
                            .foregroundColor(Color.white)
                            .padding(top: 3f, bottom: 3f, left: 6f, right: 6f)
                            .background(accent)
                            .cornerRadius(8f),
                        ZStackAlignment.TopTrailing);
            });

            ApiRow(".clipped() / .clipShape(...)", () =>
            {
                HStack(() =>
                {
                    Image(gradientSprite)
                        .resizable()
                        .scaledToFill()
                        .frame(width: 72f, height: 42f)
                        .clipped();
                    Image(gradientSprite)
                        .resizable()
                        .scaledToFill()
                        .frame(width: 72f, height: 42f)
                        .clipShape(UniftUIClipShape.RoundedRectangle, 12f);
                }, 8f);
            });
        });

        Card("Corners, shadow, and order", () =>
        {
            ApiRow(".cornerRadius(16)", () =>
            {
                Chip("All", new Color(224 / 255f, 235 / 255f, 255 / 255f)).cornerRadius(16f);
            });

            ApiRow(".cornerRadius(RectCorner.Top, 16)", () =>
            {
                Chip("Top", new Color(225 / 255f, 244 / 255f, 231 / 255f)).cornerRadius(RectCorner.Top, 16f);
            });

            ApiRow(".cornerRadius(tl,tr,br,bl)", () =>
            {
                Chip("Mixed", new Color(255 / 255f, 236 / 255f, 210 / 255f)).cornerRadius(4f, 18f, 4f, 18f);
            });

            ApiRow(".shadow(color, radius, x, y)", () =>
            {
                Text("Shadow")
                    .padding(10)
                    .background(Color.white)
                    .cornerRadius(8f)
                    .shadow(new Color(0f, 0f, 0f, 0.22f), 8f, 0f, -4f);
            });

            ApiRow("modifier order: Frame -> Padding -> Background", () =>
            {
                Text("A")
                    .frame(width: 90f, height: 28f)
                    .padding(10)
                    .background(new Color(224 / 255f, 235 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow("modifier order: Padding -> Background -> Frame", () =>
            {
                Text("B")
                    .padding(10)
                    .background(new Color(225 / 255f, 244 / 255f, 231 / 255f))
                    .cornerRadius(8f)
                    .frame(width: 150f, height: 48f);
            });

            ApiRow(".disabled(State<bool>)", () =>
            {
                Button("Disabled target", () => count.Value++)
                    .disabled(disabledDemo)
                    .frame(width: 150f, height: 34f)
                    .background(disabledDemo.Value ? new Color(226 / 255f, 232 / 255f, 240 / 255f) : new Color(224 / 255f, 235 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".allowsHitTesting(false)", () =>
            {
                Button("No hit testing", () => count.Value++)
                    .allowsHitTesting(false)
                    .frame(width: 150f, height: 34f)
                    .background(new Color(241 / 255f, 245 / 255f, 249 / 255f))
                    .cornerRadius(8f);
            });
        });

        Card("State controls", () =>
        {
            HStack(() =>
            {
                Button("Accent", CycleAccent);
                Button("Fill", () => fill.Value = fill.Value.g > 0.94f ? new Color(235 / 255f, 248 / 255f, 238 / 255f) : new Color(236 / 255f, 244 / 255f, 255 / 255f));
                Button("Padding", () => paddingAmount.Value = paddingAmount.Value > 14 ? 6 : 18);
                Button("Alpha", () => alpha.Value = alpha.Value > 0.7f ? 0.45f : 1f);
                Button("Radius", () => radius.Value = radius.Value > 14f ? 6f : 24f);
                Button("Disabled", () => disabledDemo.Value = !disabledDemo.Value);
            }, 8f);
        });
    }

    private void TransformSection()
    {
        Card("Transform modifiers", () =>
        {
            ApiRow(".offset(x, y)", () =>
            {
                TransformStage(() =>
                {
                    Chip("Offset", new Color(224 / 255f, 235 / 255f, 255 / 255f)).offset(22f, 8f);
                });
            });

            ApiRow(".offset(Vector2)", () =>
            {
                TransformStage(() =>
                {
                    Chip("Vector2", new Color(235 / 255f, 224 / 255f, 255 / 255f)).offset(new Vector2(28f, -8f));
                });
            });

            ApiRow(".offset(State<Vector2>)", () =>
            {
                TransformStage(() =>
                {
                    Chip("Vector", new Color(225 / 255f, 244 / 255f, 231 / 255f)).offset(offset2);
                });
            });

            ApiRow(".offset(State<float>, y)", () =>
            {
                TransformStage(() =>
                {
                    Chip("Axis", new Color(255 / 255f, 236 / 255f, 210 / 255f)).offset(offsetX, 0f);
                });
            });

            ApiRow(".position(State<Vector2>)", () =>
            {
                TransformStage(() =>
                {
                    Chip("Position", new Color(224 / 255f, 235 / 255f, 255 / 255f)).position(position);
                });
            });

            ApiRow(".position(x, y)", () =>
            {
                TransformStage(() =>
                {
                    Chip("Point", new Color(225 / 255f, 244 / 255f, 231 / 255f)).position(112f, 44f);
                });
            });

            ApiRow(".position(State<float> x, y)", () =>
            {
                TransformStage(() =>
                {
                    Chip("X", new Color(255 / 255f, 236 / 255f, 210 / 255f)).position(positionX, 24f);
                });
            });

            ApiRow(".position(x, State<float> y)", () =>
            {
                TransformStage(() =>
                {
                    Chip("Y", new Color(235 / 255f, 224 / 255f, 255 / 255f)).position(24f, positionY);
                });
            });

            ApiRow(".rotationEffect(float)", () =>
            {
                Chip("Rotate", new Color(235 / 255f, 244 / 255f, 255 / 255f)).rotationEffect(12f);
            });

            ApiRow(".rotationEffect(x, y, z)", () =>
            {
                Chip("Euler", new Color(225 / 255f, 244 / 255f, 231 / 255f)).rotationEffect(0f, 0f, -12f);
            });

            ApiRow(".rotationEffect(State<float>)", () =>
            {
                Chip("Rotate", new Color(224 / 255f, 235 / 255f, 255 / 255f)).rotationEffect(rotation);
            });

            ApiRow(".rotationEffect(State<float> x, y, z)", () =>
            {
                Chip("Rot X", new Color(255 / 255f, 236 / 255f, 210 / 255f)).rotationEffect(rotationX, 0f, 0f);
            });

            ApiRow(".rotationEffect(x, State<float> y, z)", () =>
            {
                Chip("Rot Y", new Color(235 / 255f, 224 / 255f, 255 / 255f)).rotationEffect(0f, rotationY, 0f);
            });

            ApiRow(".rotationEffect(x, y, State<float> z)", () =>
            {
                Chip("Rot Z", new Color(224 / 255f, 235 / 255f, 255 / 255f)).rotationEffect(0f, 0f, rotation);
            });

            ApiRow(".rotationEffect(State<Vector3>)", () =>
            {
                Chip("Rotate3", new Color(225 / 255f, 244 / 255f, 231 / 255f)).rotationEffect(rotation3);
            });

            ApiRow(".scaleEffect(float)", () =>
            {
                Chip("Scale", new Color(235 / 255f, 244 / 255f, 255 / 255f)).scaleEffect(1.15f);
            });

            ApiRow(".scaleEffect(x, y)", () =>
            {
                Chip("ScaleXY", new Color(225 / 255f, 244 / 255f, 231 / 255f)).scaleEffect(1.2f, 0.86f);
            });

            ApiRow(".scaleEffect(x, y, z)", () =>
            {
                Chip("Scale3F", new Color(255 / 255f, 236 / 255f, 210 / 255f)).scaleEffect(1.12f, 0.9f, 1f);
            });

            ApiRow(".scaleEffect(Vector3)", () =>
            {
                Chip("Vector3", new Color(235 / 255f, 224 / 255f, 255 / 255f)).scaleEffect(new Vector3(1.12f, 0.92f, 1f));
            });

            ApiRow(".scaleEffect(State<float>)", () =>
            {
                Chip("Scale", new Color(255 / 255f, 236 / 255f, 210 / 255f)).scaleEffect(scale);
            });

            ApiRow(".scaleEffect(State<float> x, y)", () =>
            {
                Chip("Scale X", new Color(224 / 255f, 235 / 255f, 255 / 255f)).scaleEffect(scaleX, 1f);
            });

            ApiRow(".scaleEffect(x, State<float> y)", () =>
            {
                Chip("Scale Y", new Color(225 / 255f, 244 / 255f, 231 / 255f)).scaleEffect(1f, scaleY);
            });

            ApiRow(".scaleEffect(State<Vector3>)", () =>
            {
                Chip("Scale3", new Color(235 / 255f, 224 / 255f, 255 / 255f)).scaleEffect(scale3);
            });
        });

        Card("Transform controls", () =>
        {
            HStack(() =>
            {
                Button("Offset", () => offsetX.Value = offsetX.Value > 0f ? 0f : 48f);
                Button("Vector", () => offset2.Value = offset2.Value == Vector2.zero ? new Vector2(54f, 16f) : Vector2.zero);
                Button("Position", () => position.Value = position.Value.x < 80f ? new Vector2(132f, 54f) : new Vector2(16f, 16f));
                Button("Position X", () => positionX.Value = positionX.Value < 80f ? 150f : 20f);
                Button("Position Y", () => positionY.Value = positionY.Value < 50f ? 72f : 20f);
                Button("Rotate", () => rotation.Value = rotation.Value > 5f ? 0f : 18f);
                Button("Scale", () => scale.Value = scale.Value > 1.05f ? 1f : 1.18f);
            }, 8f);

            HStack(() =>
            {
                Button("Rotate X", () => rotationX.Value = rotationX.Value > 5f ? 0f : 24f);
                Button("Rotate Y", () => rotationY.Value = rotationY.Value > 5f ? 0f : 24f);
                Button("Rotate3", () => rotation3.Value = rotation3.Value == Vector3.zero ? new Vector3(0f, 0f, 18f) : Vector3.zero);
                Button("Scale X", () => scaleX.Value = scaleX.Value > 1.05f ? 1f : 1.22f);
                Button("Scale Y", () => scaleY.Value = scaleY.Value > 1.05f ? 1f : 1.22f);
                Button("Scale3", () => scale3.Value = scale3.Value.x > 1.05f ? Vector3.one : new Vector3(1.15f, 0.9f, 1f));
            }, 8f);
        });
    }

    private void AnimationSection()
    {
        Card("WithAnimation(...)", () =>
        {
            ApiRow("WithAnimation(easeInOut(...), changes)", () =>
            {
                AnimatedBoxes();
            });

            HStack(() =>
            {
                Button("easeInOut", () => WithAnimation(easeInOut(0.30f), FlipMotion));
                Button("spring", () => WithAnimation(spring(0.5f, 0.8f), FlipMotion));
                Button("bouncy", () => WithAnimation(bouncy(0.45f), FlipMotion));
                Button("default", () => WithAnimation(FlipMotion));
            }, 8f);
        });

        Card(".animation(...)", () =>
        {
            ApiRow(".animation(duration)", () =>
            {
                Chip("Linear", new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .rotationEffect(rotation)
                    .animation(0.35f);
            });

            ApiRow(".animation(AnimationEasing.EaseInOut, duration)", () =>
            {
                Chip("Rotate", new Color(224 / 255f, 235 / 255f, 255 / 255f))
                    .rotationEffect(rotation)
                    .animation(AnimationEasing.EaseInOut, 0.45f);
            });

            ApiRow(".animation(State value)", () =>
            {
                Text("Default")
                    .padding(10)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f)
                    .frame(width: width)
                    .animation(width);
            });

            ApiRow(".animation(easeOut(...), state)", () =>
            {
                Text("Width")
                    .padding(10)
                    .background(new Color(225 / 255f, 244 / 255f, 231 / 255f))
                    .cornerRadius(8f)
                    .frame(width: width)
                    .animation(easeOut(0.35f), width);
            });

            ApiRow(".animation(..., width, alpha) + .animation(..., radius)", () =>
            {
                Text("Width + alpha")
                    .padding(10)
                    .background(new Color(255 / 255f, 236 / 255f, 210 / 255f))
                    .cornerRadius(radius)
                    .frame(width: width)
                    .opacity(alpha)
                    .animation(easeInOut(0.35f), width, alpha)
                    .animation(easeInOut(0.35f), radius);
            });

            HStack(() =>
            {
                Button("rotation", () => rotation.Value = rotation.Value > 5f ? 0f : 24f);
                Button("width", () => width.Value = width.Value > 210f ? 150f : 260f);
                Button("alpha", () => alpha.Value = alpha.Value > 0.7f ? 0.45f : 1f);
                Button("radius", () => radius.Value = radius.Value > 14f ? 6f : 24f);
            }, 8f);
        });
    }

    private void ScrollViewSection()
    {
        Card("Vertical", () =>
        {
            ApiRow("ScrollView(..., vertical: true)", () =>
            {
                ScrollView(() =>
                {
                    VStack(() =>
                    {
                        for (int i = 0; i < rows.Value; i++)
                            Row("Row " + (i + 1), i);
                    }, new State[] { rows }, 6f, VStackAlignment.Leading)
                    .padding(8)
                    .frame(infiniteWidth: true);
                }, new State[] { rows }, false, true)
                .scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical)
                .scrollPositionY(scrollY, true)
                .scrollBounce(true)
                .frame(infiniteWidth: true, height: 190f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });

            ApiRow(".scrollIndicators(.hidden)", () =>
            {
                ScrollView(() =>
                {
                    VStack(() =>
                    {
                        for (int i = 0; i < 5; i++)
                            Row("Scrollbar row " + (i + 1), i);
                    }, null, 6f, VStackAlignment.Leading)
                    .padding(8)
                    .frame(infiniteWidth: true);
                }, null, false, true)
                .scrollIndicators(ScrollIndicatorVisibility.Hidden)
                .frame(infiniteWidth: true, height: 108f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });

            ApiRow(".scrollIndicators(visibility)", () =>
            {
                ScrollView(() =>
                {
                    VStack(() =>
                    {
                        for (int i = 0; i < 5; i++)
                            Row("Indicator row " + (i + 1), i);
                    }, null, 6f, VStackAlignment.Leading)
                    .padding(8)
                    .frame(infiniteWidth: true);
                }, null, false, true)
                .scrollIndicators(ScrollIndicatorVisibility.Hidden)
                .frame(infiniteWidth: true, height: 108f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });

            ApiRow(".scrollIndicators(visibility, axes)", () =>
            {
                ScrollView(() =>
                {
                    VStack(() =>
                    {
                        for (int i = 0; i < 5; i++)
                            Row("Axis indicator " + (i + 1), i);
                    }, null, 6f, VStackAlignment.Leading)
                    .padding(8)
                    .frame(infiniteWidth: true);
                }, null, false, true)
                .scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical)
                .frame(infiniteWidth: true, height: 108f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });

            ApiRow(".scrollPositionY(State<float>, twoWay)", () =>
            {
                ScrollView(() =>
                {
                    VStack(() =>
                    {
                        for (int i = 0; i < 8; i++)
                            Row("Y position " + (i + 1), i);
                    }, null, 6f, VStackAlignment.Leading)
                    .padding(8)
                    .frame(infiniteWidth: true);
                }, null, false, true)
                .scrollPositionY(scrollY, true)
                .scrollIndicators(ScrollIndicatorVisibility.Visible)
                .frame(infiniteWidth: true, height: 128f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });

            ApiRow(".scrollBounce(bool)", () =>
            {
                ScrollView(() =>
                {
                    VStack(() =>
                    {
                        for (int i = 0; i < 6; i++)
                            Row("Clamped row " + (i + 1), i);
                    }, null, 6f, VStackAlignment.Leading)
                    .padding(8)
                    .frame(infiniteWidth: true);
                }, null, false, true)
                .scrollBounce(false)
                .scrollIndicators(ScrollIndicatorVisibility.Visible)
                .frame(infiniteWidth: true, height: 120f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });

            HStack(() =>
            {
                Button("- rows", () => rows.Value = Mathf.Max(4, rows.Value - 2));
                Button("+ rows", () => rows.Value = Mathf.Min(30, rows.Value + 2));
                Button("top", () => scrollY.Value = 1f);
                Button("middle", () => scrollY.Value = 0.5f);
                Button("bottom", () => scrollY.Value = 0f);
            }, 8f);
        });

        Card("Horizontal / two-axis", () =>
        {
            ApiRow("ScrollView(..., horizontal: true, vertical: false)", () =>
            {
                ScrollView(() =>
                {
                    HStack(() =>
                    {
                        for (int i = 0; i < 12; i++)
                            Chip("Col " + (i + 1), i % 2 == 0 ? new Color(224 / 255f, 235 / 255f, 255 / 255f) : new Color(225 / 255f, 244 / 255f, 231 / 255f))
                                .frame(width: 110f, height: 58f);
                    }, null, 8f, HStackAlignment.Center)
                    .padding(8);
                }, null, true, false)
                .scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Horizontal)
                .scrollPositionX(scrollX, true)
                .frame(infiniteWidth: true, height: 96f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });

            ApiRow(".scrollPositionX(State<float>, twoWay)", () =>
            {
                ScrollView(() =>
                {
                    HStack(() =>
                    {
                        for (int i = 0; i < 10; i++)
                            Chip("X " + (i + 1), i % 2 == 0 ? new Color(224 / 255f, 235 / 255f, 255 / 255f) : new Color(225 / 255f, 244 / 255f, 231 / 255f))
                                .frame(width: 92f, height: 48f);
                    }, null, 8f, HStackAlignment.Center)
                    .padding(8);
                }, null, true, false)
                .scrollPositionX(scrollX, true)
                .scrollIndicators(ScrollIndicatorVisibility.Visible)
                .frame(infiniteWidth: true, height: 86f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });

            ApiRow("ScrollMovementType + ScrollSensitivity", () =>
            {
                ScrollView(() =>
                {
                    VStack(() =>
                    {
                        for (int i = 0; i < 8; i++)
                            Text("Wide row " + (i + 1))
                                .frame(width: 640f, height: 34f)
                                .background(i % 2 == 0 ? new Color(236 / 255f, 242 / 255f, 255 / 255f) : new Color(238 / 255f, 248 / 255f, 236 / 255f))
                                .cornerRadius(8f);
                    }, null, 6f, VStackAlignment.Leading)
                    .padding(8);
                }, null, true, true)
                .scrollIndicators(ScrollIndicatorVisibility.Automatic)
                .scrollMovementType(ScrollRect.MovementType.Elastic)
                .scrollSensitivity(35f)
                .frame(infiniteWidth: true, height: 145f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);
            });
        });
    }

    private void ImageSection()
    {
        Card("Image sizing", () =>
        {
            ApiRow("Image(Sprite)", () =>
            {
                Image(tileSprite)
                    .frame(width: 84f, height: 42f)
                    .background(new Color(238 / 255f, 242 / 255f, 248 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".resizable()", () => ImageProbe("Resizable", false));
            ApiRow(".scaledToFit()", () => ImageProbe("Fit", false));
            ApiRow(".scaledToFill()", () => ImageProbe("Fill", true));
        });

        Card("Image modifiers", () =>
        {
            ApiRow(".resizable(ImageResizingMode.Stretch)", () =>
            {
                Image(tileSprite)
                    .resizable(ImageResizingMode.Stretch)
                    .frame(width: 140f, height: 62f)
                    .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".resizable(ImageResizingMode.Tile)", () =>
            {
                Image(tileSprite)
                    .resizable(ImageResizingMode.Tile)
                    .frame(width: 140f, height: 62f)
                    .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".renderingMode(.template) + .tint(Color)", () =>
            {
                Image(tileSprite)
                    .resizable(ImageResizingMode.Stretch)
                    .renderingMode(ImageRenderingMode.Template)
                    .tint(new Color(65 / 255f, 120 / 255f, 220 / 255f))
                    .frame(width: 140f, height: 62f)
                    .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                    .cornerRadius(8f);
            });
        });
    }

    private void CompositionSection()
    {
        Card("TabView / Tab", () =>
        {
            ApiRow("TabView(() => { Tab(...); }, State<int>)", () =>
            {
                TabView(() =>
                {
                    Tab("Text", () =>
                    {
                        VStack(() =>
                        {
                            Text("Tab 1").bold().fontSize(18);
                            Text(textValue).foregroundColor(accent);
                        }, null, 8f, VStackAlignment.Leading)
                        .padding(12)
                        .frame(infiniteWidth: true, infiniteHeight: true);
                    });

                    Tab(() =>
                    {
                        HStack(() =>
                        {
                            Text("Custom").bold().fontSize(13);
                            Text("title").fontSize(12);
                        }, 4f);
                    }, () =>
                    {
                        VStack(() =>
                        {
                            Text("Tab 2").bold().fontSize(18);
                            Slider(sliderValue, 0, 100).frame(infiniteWidth: true, height: 38f);
                        }, null, 8f, VStackAlignment.Leading)
                        .padding(12)
                        .frame(infiniteWidth: true, infiniteHeight: true);
                    });

                    Tab("Layout", () =>
                    {
                        HStack(() =>
                        {
                            Chip("A", new Color(224 / 255f, 235 / 255f, 255 / 255f));
                            Spacer();
                            Chip("B", new Color(225 / 255f, 244 / 255f, 231 / 255f));
                        }, null, 8f, HStackAlignment.Center)
                        .padding(12)
                        .frame(infiniteWidth: true, infiniteHeight: true);
                    });
                }, tab)
                .frame(infiniteWidth: true, height: 220f);
            });

            ApiRow("TabView(...).WithTransitionDuration(seconds)", () =>
            {
                TabView(() =>
                {
                    Tab("Fast", () =>
                    {
                        Text("Transition 0.12s")
                            .padding(12)
                            .frame(infiniteWidth: true, infiniteHeight: true);
                    });

                    Tab("Next", () =>
                    {
                        Text("Second tab")
                            .padding(12)
                            .frame(infiniteWidth: true, infiniteHeight: true);
                    });
                }, tab)
                .WithTransitionDuration(0.12f)
                .frame(infiniteWidth: true, height: 156f);
            });

            HStack(() =>
            {
                Button("tab 0", () => tab.Value = 0);
                Button("tab 1", () => tab.Value = 1);
                Button("tab 2", () => tab.Value = 2);
            }, 8f);
        });

        Card("ForEach", () =>
        {
            ApiRow("ForEach(0, 4, i => ...)", () =>
            {
                HStack(() =>
                {
                    ForEach(0, 4, i => Chip("I" + i, new Color(224 / 255f, 235 / 255f, 255 / 255f)).frame(width: 54f, height: 34f));
                }, 6f);
            });

            ApiRow("ForEach(0..4, i => ...)", () =>
            {
                HStack(() =>
                {
                    ForEach(0..4, i => Chip("R" + i, new Color(255 / 255f, 236 / 255f, 210 / 255f)).frame(width: 54f, height: 34f));
                }, 6f);
            });

            ApiRow("ForEach(IEnumerable<T>, item => ...)", () =>
            {
                HStack(() =>
                {
                    string[] items = { "A", "B", "C", "D" };
                    ForEach(items, item => Chip(item, new Color(225 / 255f, 244 / 255f, 231 / 255f)).frame(width: 54f, height: 34f));
                }, 6f);
            });
        });

        Card("Lifecycle", () =>
        {
            ApiRow(".onAppear(action)", () =>
            {
                Text("OnAppear target")
                    .onAppear(() => appearCount.Value++)
                    .padding(10)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);

                Text(() => $"appearCount = {appearCount.Value}", new State[] { appearCount }).fontSize(13);
            });

            ApiRow(".onAppear(async action)", () =>
            {
                Text("Async OnAppear target")
                    .onAppear(async () =>
                    {
                        await Task.Yield();
                        appearCount.Value++;
                    })
                    .padding(10)
                    .background(new Color(225 / 255f, 244 / 255f, 231 / 255f))
                    .cornerRadius(8f);
            });

            ApiRow(".update(action)", () =>
            {
                Text(updateText)
                    .update(() =>
                    {
                        updateTicks++;
                        if (updateTicks % 60 == 0)
                            updateText.Value = "Update tick " + updateTicks;
                    })
                    .fontSize(13)
                    .foregroundColor(new Color(76 / 255f, 88 / 255f, 108 / 255f));
            });

            ApiRow(".onChange(State<T>, value => ...)", () =>
            {
                Text(changeStatus)
                    .onChange(count, value => changeStatus.Value = "count changed to " + value)
                    .padding(10)
                    .background(new Color(235 / 255f, 244 / 255f, 255 / 255f))
                    .cornerRadius(8f);
            });
        });
    }

    private void SectionButton(int index)
    {
        bool selected = section.Value == index;
        Button(Sections[index], () => section.Value = index)
            .frame(infiniteWidth: true, height: 36f)
            .background(selected ? new Color(224 / 255f, 235 / 255f, 255 / 255f) : Color.white)
            .foregroundColor(selected ? new Color(26 / 255f, 72 / 255f, 150 / 255f) : new Color(48 / 255f, 58 / 255f, 74 / 255f))
            .cornerRadius(8f)
            .padding(left: 10f, right: 10f);
    }

    private void PageTitle(string title, string note)
    {
        VStack(() =>
        {
            Text(title).bold().fontSize(28).foregroundColor(new Color(20 / 255f, 28 / 255f, 42 / 255f));
            Text(note).fontSize(14).foregroundColor(new Color(76 / 255f, 88 / 255f, 108 / 255f)).frame(infiniteWidth: true);
        }, null, 4f, VStackAlignment.Leading)
        .frame(infiniteWidth: true);
    }

    private void Card(string title, Action content)
    {
        VStack(() =>
        {
            Text(title).bold().fontSize(18).foregroundColor(new Color(24 / 255f, 32 / 255f, 46 / 255f));
            content();
        }, null, 10f, VStackAlignment.Leading)
        .padding(14)
        .background(Color.white)
        .cornerRadius(8f)
        .frame(infiniteWidth: true);
    }

    private void ApiRow(string api, Action preview)
    {
        HStack(() =>
        {
            Text(api)
                .font(font)
                .fontSize(12)
                .foregroundColor(new Color(44 / 255f, 54 / 255f, 72 / 255f))
                .frame(width: 290f);

            VStack(preview, null, 6f, VStackAlignment.Leading)
                .frame(infiniteWidth: true);
        }, null, 12f, HStackAlignment.Center)
        .padding(10)
        .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
        .cornerRadius(8f)
        .frame(infiniteWidth: true);
    }

    private UIElement StateReadout()
    {
        return VStack(() =>
        {
            Text(() => "count: " + count.Value, new State[] { count }).fontSize(12);
            Text(() => "slider: " + sliderValue.Value, new State[] { sliderValue }).fontSize(12);
            Text(() => "progress: " + progressValue.Value.ToString("0.00"), new State[] { progressValue }).fontSize(12);
            Text(() => "text: " + textValue.Value, new State[] { textValue }).fontSize(12);
            Text(() => "alpha: " + alpha.Value.ToString("0.00"), new State[] { alpha }).fontSize(12);
        }, null, 4f, VStackAlignment.Leading)
        .frame(infiniteWidth: true);
    }

    private UIElement Chip(string label, Color color)
    {
        return Text(label)
            .bold()
            .fontSize(13)
            .foregroundColor(new Color(24 / 255f, 32 / 255f, 44 / 255f))
            .frame(width: 86f, height: 34f)
            .background(color)
            .cornerRadius(8f);
    }

    private void Row(string label, int index)
    {
        Text(label)
            .fontSize(13)
            .frame(infiniteWidth: true, height: 30f)
            .background(index % 2 == 0 ? new Color(236 / 255f, 242 / 255f, 255 / 255f) : new Color(248 / 255f, 250 / 255f, 255 / 255f))
            .cornerRadius(6f);
    }

    private void TransformStage(Action content)
    {
        ZStack(() =>
        {
            Spacer().frame(infiniteWidth: true, height: 90f).background(new Color(242 / 255f, 245 / 255f, 250 / 255f)).cornerRadius(8f);
            content();
        }, null, ZStackAlignment.TopLeading)
        .frame(infiniteWidth: true, height: 90f);
    }

    private void AnimatedBoxes()
    {
        HStack(() =>
        {
            Chip("Opacity", new Color(224 / 255f, 235 / 255f, 255 / 255f)).opacity(alpha);
            Chip("Rotate", new Color(225 / 255f, 244 / 255f, 231 / 255f)).rotationEffect(rotation);
            Chip("Scale", new Color(255 / 255f, 236 / 255f, 210 / 255f)).scaleEffect(scale);
        }, null, 12f, HStackAlignment.Center);
    }

    private void ImageProbe(string label, bool fill)
    {
        VStack(() =>
        {
            Image(fill ? gradientSprite : tileSprite)
                .resizable()
                .scaledToFit()
                .frame(width: 140f, height: 76f)
                .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                .cornerRadius(8f);

            if (fill)
            {
                Image(gradientSprite)
                    .resizable()
                    .scaledToFill()
                    .frame(width: 140f, height: 76f)
                    .clipped()
                    .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                    .cornerRadius(8f);
            }
            else
            {
                Image(tileSprite)
                    .resizable()
                    .frame(width: 140f, height: 76f)
                    .background(new Color(248 / 255f, 250 / 255f, 253 / 255f))
                    .cornerRadius(8f);
            }

            Text(label).fontSize(12).foregroundColor(new Color(76 / 255f, 88 / 255f, 108 / 255f));
        }, null, 6f, VStackAlignment.Center);
    }

    private void CycleAccent()
    {
        Color c = accent.Value;
        if (c.r > 0.6f)
            accent.Value = new Color(38 / 255f, 94 / 255f, 190 / 255f);
        else if (c.g > 0.45f)
            accent.Value = new Color(190 / 255f, 56 / 255f, 50 / 255f);
        else
            accent.Value = new Color(24 / 255f, 132 / 255f, 92 / 255f);
    }

    private void CycleSelectionFill()
    {
        Color c = selectionFill.Value;
        if (c.r > 0.6f)
            selectionFill.Value = new Color(26 / 255f, 92 / 255f, 220 / 255f, 0.28f);
        else
            selectionFill.Value = new Color(220 / 255f, 38 / 255f, 38 / 255f, 0.32f);
    }

    private void FlipMotion()
    {
        alpha.Value = alpha.Value > 0.7f ? 0.45f : 1f;
        rotation.Value = rotation.Value > 5f ? 0f : 24f;
        scale.Value = scale.Value > 1.05f ? 0.92f : 1.15f;
        width.Value = width.Value > 210f ? 150f : 260f;
        radius.Value = radius.Value > 14f ? 6f : 24f;
    }

    private void ResetState()
    {
        section.Value = 0;
        count.Value = 0;
        sliderValue.Value = 42;
        sliderFloat.Value = 0.35f;
        progressValue.Value = 0.35f;
        stepperValue.Value = 2;
        pickerSelection.Value = 0;
        tab.Value = 0;
        rows.Value = 10;
        textValue.Value = "Bound text";
        email.Value = string.Empty;
        search.Value = string.Empty;
        notes.Value = string.Empty;
        secureText.Value = string.Empty;
        chromeText.Value = "Editable text";
        selectionText.Value = "Select this text";
        submitStatus.Value = "Submit has not run";
        flagA.Value = true;
        flagB.Value = false;
        disabledDemo.Value = false;
        chromeFocused.Value = false;
        accent.Value = new Color(38 / 255f, 94 / 255f, 190 / 255f);
        fill.Value = new Color(236 / 255f, 244 / 255f, 255 / 255f);
        selectionFill.Value = new Color(26 / 255f, 92 / 255f, 220 / 255f, 0.28f);
        paddingAmount.Value = 10;
        alpha.Value = 1f;
        radius.Value = 10f;
        width.Value = 180f;
        height.Value = 38f;
        rotation.Value = 0f;
        rotationX.Value = 0f;
        rotationY.Value = 0f;
        rotation3.Value = Vector3.zero;
        scale.Value = 1f;
        scaleX.Value = 1f;
        scaleY.Value = 1f;
        scale3.Value = Vector3.one;
        offsetX.Value = 0f;
        offset2.Value = Vector2.zero;
        position.Value = new Vector2(16f, 16f);
        positionX.Value = 20f;
        positionY.Value = 20f;
        scrollY.Value = 1f;
        scrollX.Value = 0f;
        updateText.Value = "Update has not ticked yet";
        changeStatus.Value = "No change yet";
    }

    private void PrepareHost()
    {
        transform.localScale = Vector3.one;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        RectTransform rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
        }

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1200f, 800f);

        if (GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }

    private void BuildSprites()
    {
        tileTexture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        for (int y = 0; y < tileTexture.height; y++)
        {
            for (int x = 0; x < tileTexture.width; x++)
            {
                bool even = ((x / 8) + (y / 8)) % 2 == 0;
                tileTexture.SetPixel(x, y, even ? new Color(46 / 255f, 107 / 255f, 208 / 255f) : new Color(224 / 255f, 238 / 255f, 255 / 255f));
            }
        }
        tileTexture.Apply();
        tileSprite = Sprite.Create(tileTexture, new Rect(0, 0, tileTexture.width, tileTexture.height), new Vector2(0.5f, 0.5f), 16f);

        gradientTexture = new Texture2D(48, 72, TextureFormat.RGBA32, false);
        for (int y = 0; y < gradientTexture.height; y++)
        {
            for (int x = 0; x < gradientTexture.width; x++)
            {
                float nx = x / (float)(gradientTexture.width - 1);
                float ny = y / (float)(gradientTexture.height - 1);
                gradientTexture.SetPixel(x, y, new Color(0.18f + nx * 0.38f, 0.30f + ny * 0.42f, 0.72f - ny * 0.30f));
            }
        }
        gradientTexture.Apply();
        gradientSprite = Sprite.Create(gradientTexture, new Rect(0, 0, gradientTexture.width, gradientTexture.height), new Vector2(0.5f, 0.5f), 24f);
    }
}
