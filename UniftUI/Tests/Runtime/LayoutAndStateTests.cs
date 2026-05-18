using System.Collections;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace UniftUI.Tests
{
    public class LayoutAndStateTests
    {
        private GameObject canvasObject;
        private GameObject eventSystemObject;
        private EventSystem testEventSystem;

        [TearDown]
        public void TearDown()
        {
            if (canvasObject != null)
            {
                Object.DestroyImmediate(canvasObject);
                canvasObject = null;
            }
            if (testEventSystem != null)
            {
                UnregisterTestEventSystem(testEventSystem);
                testEventSystem = null;
            }
            if (eventSystemObject != null)
            {
                Object.DestroyImmediate(eventSystemObject);
                eventSystemObject = null;
            }
        }

        [UnityTest]
        public IEnumerator HStack_DistributesSpacerExtraWidth()
        {
            Canvas canvas = CreateCanvas();

            UIElements.HStack(() =>
            {
                UIElements.Text("A").frame(width: 50, height: 20);
                UIElements.Spacer();
                UIElements.Text("B").frame(width: 50, height: 20);
            }, spacing: 0f)
            .frame(width: 300, height: 40)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform hstack = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform spacer = hstack.Find("Spacer").GetComponent<RectTransform>();

            Assert.That(hstack.rect.width, Is.EqualTo(300f).Within(0.5f));
            Assert.That(spacer.rect.width, Is.GreaterThan(190f));
        }

        [UnityTest]
        public IEnumerator VStack_FrameCentersContentOnMainAxis()
        {
            Canvas canvas = CreateCanvas();

            UIElements.VStack(() =>
            {
                UIElements.Text("Count: 0").frame(width: 100f, height: 20f);
                UIElements.Text("Increment").frame(width: 100f, height: 20f);
            }, spacing: 12f)
            .frame(width: 300f, height: 200f)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform vstack = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            Rect first = GetLocalRect(vstack, FindRect(vstack, "Text", 0));
            Rect second = GetLocalRect(vstack, FindRect(vstack, "Text", 1));
            float combinedCenterY = (Mathf.Min(first.yMin, second.yMin) + Mathf.Max(first.yMax, second.yMax)) * 0.5f;

            Assert.That(combinedCenterY, Is.EqualTo(0f).Within(1f));
        }

        [UnityTest]
        public IEnumerator HStack_FrameCentersContentOnMainAxis()
        {
            Canvas canvas = CreateCanvas();

            UIElements.HStack(() =>
            {
                UIElements.Text("A").frame(width: 50f, height: 20f);
                UIElements.Text("B").frame(width: 50f, height: 20f);
            }, spacing: 12f)
            .frame(width: 300f, height: 80f)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform hstack = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            Rect first = GetLocalRect(hstack, FindRect(hstack, "Text", 0));
            Rect second = GetLocalRect(hstack, FindRect(hstack, "Text", 1));
            float combinedCenterX = (Mathf.Min(first.xMin, second.xMin) + Mathf.Max(first.xMax, second.xMax)) * 0.5f;

            Assert.That(combinedCenterX, Is.EqualTo(0f).Within(1f));
        }

        [UnityTest]
        public IEnumerator Text_StateBindingUpdatesTextComponent()
        {
            Canvas canvas = CreateCanvas();
            var value = new State<string>("before");

            UIElements.Text(value).Build(canvas);
            yield return null;

            value.Value = "after";
            yield return null;

            TMP_Text tmp = canvas.GetComponentInChildren<TMP_Text>();
            Assert.That(tmp.text, Is.EqualTo("after"));
        }

        [UnityTest]
        public IEnumerator VStack_StateRebuildsOnlyItsSubtree()
        {
            Canvas canvas = CreateCanvas();
            var count = new State<int>(1);

            UIElements.VStack(() =>
            {
                for (int i = 0; i < count.Value; i++)
                    UIElements.Text("row " + i).frame(width: 80, height: 20);
            }, new State[] { count }, spacing: 2f)
            .Build(canvas);

            yield return null;
            count.Value = 3;
            yield return null;
            ForceLayout(canvas);

            RectTransform vstack = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            Assert.That(vstack.GetComponentsInChildren<TMP_Text>().Length, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator Controls_BindToStateWithoutExceptions()
        {
            Canvas canvas = CreateCanvas();
            var toggle = new State<bool>(false);
            var slider = new State<int>(5);
            var input = new State<string>("hello");

            UIElements.VStack(() =>
            {
                UIElements.Toggle(toggle, "toggle");
                UIElements.Slider(slider, 0, 10);
                UIElements.TextField("Placeholder", text: input, prompt: UIElements.Text("placeholder"));
            })
            .frame(width: 320, height: 160)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            Assert.That(canvas.GetComponentsInChildren<Button>().Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(canvas.GetComponentInChildren<Slider>().value, Is.EqualTo(5f));
            Assert.That(canvas.GetComponentInChildren<TMP_InputField>().text, Is.EqualTo("hello"));
        }

        [UnityTest]
        public IEnumerator Slider_FloatStateBindsTwoWay()
        {
            Canvas canvas = CreateCanvas();
            var value = new State<float>(0.25f);

            UIElements.Slider(value, 0f, 1f)
                .frame(width: 240, height: 40)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            Slider slider = canvas.GetComponentInChildren<Slider>();
            Assert.That(slider, Is.Not.Null);
            Assert.That(slider.wholeNumbers, Is.False);
            Assert.That(slider.value, Is.EqualTo(0.25f).Within(0.001f));

            slider.onValueChanged.Invoke(0.75f);
            Assert.That(value.Value, Is.EqualTo(0.75f).Within(0.001f));

            value.Value = 0.4f;
            yield return null;
            Assert.That(slider.value, Is.EqualTo(0.4f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator Text_LineLimitAndAlignmentConfigureTMP()
        {
            Canvas canvas = CreateCanvas();

            UIElements.Text("A long text value that can wrap over multiple lines.")
                .lineLimit(2)
                .multilineTextAlignment(TextAlignmentOptions.TopLeft)
                .frame(width: 120, height: 60)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            TMP_Text label = canvas.GetComponentInChildren<TMP_Text>();
            Assert.That(label, Is.Not.Null);
            Assert.That(label.maxVisibleLines, Is.EqualTo(2));
            Assert.That(label.alignment, Is.EqualTo(TextAlignmentOptions.TopLeft));
            Assert.That(label.textWrappingMode, Is.EqualTo(TextWrappingModes.Normal));
        }

        [UnityTest]
        public IEnumerator Interaction_DisabledAndHitTestingUpdateCanvasGroupAndSelectables()
        {
            Canvas canvas = CreateCanvas();
            var disabled = new State<bool>(true);
            var hitTesting = new State<bool>(false);

            UIElements.Button("tap", () => { })
                .disabled(disabled)
                .allowsHitTesting(hitTesting)
                .frame(width: 120, height: 36)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            Button button = canvas.GetComponentInChildren<Button>();
            CanvasGroup canvasGroup = canvas.GetComponentInChildren<CanvasGroup>();
            Assert.That(button, Is.Not.Null);
            Assert.That(canvasGroup, Is.Not.Null);
            Assert.That(button.interactable, Is.False);
            Assert.That(canvasGroup.interactable, Is.False);
            Assert.That(canvasGroup.blocksRaycasts, Is.False);

            disabled.Value = false;
            hitTesting.Value = true;
            yield return null;

            Assert.That(button.interactable, Is.True);
            Assert.That(canvasGroup.interactable, Is.True);
            Assert.That(canvasGroup.blocksRaycasts, Is.True);
        }

        [UnityTest]
        public IEnumerator ShapesOverlayBorderSecureFieldAndOnChangeBuild()
        {
            Canvas canvas = CreateCanvas();
            var password = new State<string>("secret");
            var count = new State<int>(0);
            int changes = 0;
            int latest = -1;

            UIElements.VStack(() =>
            {
                UIElements.Rectangle(new Color(0.2f, 0.4f, 0.8f))
                    .frame(width: 90, height: 32)
                    .border(Color.black, 2f);
                UIElements.Color(new Color(0.8f, 0.9f, 1f))
                    .frame(width: 80, height: 24);
                UIElements.Divider();
                UIElements.Text("Base")
                    .overlay(UIElements.Text("Badge").fontSize(10), ZStackAlignment.TopTrailing);
                UIElements.SecureField("Password", text: password, prompt: UIElements.Text("password"));
                UIElements.Text("watch")
                    .onChange(count, value =>
                    {
                        changes++;
                        latest = value;
                    });
            }, spacing: 4f)
            .frame(width: 260, height: 220)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            Assert.That(canvas.GetComponentInChildren<Outline>(), Is.Not.Null);
            Assert.That(FindRect(canvas.transform, "OverlayLayer"), Is.Not.Null);
            RectTransform[] rects = canvas.transform.GetComponentsInChildren<RectTransform>(true);
            Assert.That(System.Array.Exists(rects, rect => rect.name == "Divider"), Is.True);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);
            Assert.That(field.contentType, Is.EqualTo(TMP_InputField.ContentType.Password));
            Assert.That(changes, Is.EqualTo(0));

            count.Value = 3;
            yield return null;
            Assert.That(changes, Is.EqualTo(1));
            Assert.That(latest, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator PublicApi_ControlsShapesStylesAndTintBuildAndBind()
        {
            Canvas canvas = CreateCanvas();
            var progress = new State<float>(0.25f);
            var stepper = new State<int>(2);
            var picker = new State<int>(0);
            var tint = new State<Color>(new Color(0.1f, 0.35f, 0.95f, 1f));
            var input = new State<string>("value");

            UIElements.VStack(() =>
            {
                UIElements.HStack(() =>
                {
                    UIElements.Circle(new Color(0.8f, 0.9f, 1f)).frame(width: 24, height: 24);
                    UIElements.Capsule(new Color(0.8f, 1f, 0.86f)).frame(width: 68, height: 24);
                    UIElements.RoundedRectangle(8f, new Color(1f, 0.9f, 0.7f)).frame(width: 68, height: 24);
                }, spacing: 4f);

                UIElements.ProgressView(progress, 1f).tint(tint).frame(width: 180, height: 10);
                UIElements.Stepper("Qty", stepper, 0, 5).tint(tint).frame(width: 220, height: 34);
                UIElements.Picker(picker, "One", "Two", "Three").pickerStyle(PickerStyle.Segmented).tint(tint).frame(width: 240, height: 34);
                UIElements.Label("Ready", UIElements.Circle(Color.green).frame(width: 12, height: 12));
                UIElements.Button("Styled", () => { }).buttonStyle(ButtonStyles.Filled(tint.Value, Color.white, 9f));
                UIElements.TextField("Prompt", text: input, prompt: UIElements.Text("prompt"))
                    .textFieldStyle(TextFieldStyles.RoundedBorder(Color.white, new Color(0.92f, 0.96f, 1f), Color.black, tint.Value))
                    .tint(tint);
            }, spacing: 6f, alignment: VStackAlignment.Leading)
            .frame(width: 320, height: 260)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform progressView = FindRect(canvas.transform, "ProgressView");
            RectTransform fill = FindRect(progressView, "Fill");
            Assert.That(fill.anchorMax.x, Is.EqualTo(0.25f).Within(0.001f));

            progress.Value = 0.8f;
            yield return null;
            Assert.That(fill.anchorMax.x, Is.EqualTo(0.8f).Within(0.001f));

            FindRect(canvas.transform, "Plus").GetComponent<Button>().onClick.Invoke();
            Assert.That(stepper.Value, Is.EqualTo(3));

            FindRect(canvas.transform, "Segment_Two").GetComponent<Button>().onClick.Invoke();
            Assert.That(picker.Value, Is.EqualTo(1));

            Color updatedTint = new Color(0.8f, 0.2f, 0.12f, 1f);
            tint.Value = updatedTint;
            yield return null;
            AssertColor(fill.GetComponent<Image>().color, updatedTint);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);
            Assert.That(field.caretColor.r, Is.EqualTo(tint.Value.r).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator PublicApi_CompositionModifiersAndGeometryBuild()
        {
            Canvas canvas = CreateCanvas();

            UIElements.VStack(() =>
            {
                UIElements.Rectangle(new Color(0.8f, 0.9f, 1f))
                    .frame(width: 160)
                    .aspectRatio(2f);

                UIElements.Rectangle(new Color(0.8f, 1f, 0.86f))
                    .frame(width: 80, height: 38)
                    .clipped();

                UIElements.Rectangle(new Color(1f, 0.9f, 0.7f))
                    .frame(width: 80, height: 38)
                    .clipShape(UniftUIClipShape.RoundedRectangle, 12f);

                UIElements.LazyVStack(() =>
                {
                    UIElements.Text("lazy v").frame(width: 90, height: 24);
                }, spacing: 2f);

                UIElements.LazyHStack(() =>
                {
                    UIElements.Text("lazy h").frame(width: 90, height: 24);
                }, spacing: 2f);

                UIElements.GeometryReader(proxy =>
                    UIElements.Text(proxy.Size.x.ToString("0") + "x" + proxy.Size.y.ToString("0"))
                        .frame(infiniteWidth: true, infiniteHeight: true))
                    .frame(width: 140, height: 36);
            }, spacing: 6f, alignment: VStackAlignment.Leading)
            .frame(width: 260, height: 260)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform aspect = FindRect(canvas.transform, "AspectRatio");
            Assert.That(aspect.rect.width, Is.EqualTo(160f).Within(1f));
            Assert.That(aspect.rect.height, Is.EqualTo(80f).Within(1f));

            Assert.That(FindRect(canvas.transform, "Clipped").GetComponent<RectMask2D>(), Is.Not.Null);
            Assert.That(FindRect(canvas.transform, "ClipShape").GetComponent<Mask>(), Is.Not.Null);
            Assert.That(FindRect(canvas.transform, "GeometryReader"), Is.Not.Null);
            Assert.That(System.Array.Exists(canvas.GetComponentsInChildren<TMP_Text>(), text => text.text == "lazy v"), Is.True);
            Assert.That(System.Array.Exists(canvas.GetComponentsInChildren<TMP_Text>(), text => text.text == "lazy h"), Is.True);
        }

        [UnityTest]
        public IEnumerator TextField_ConfiguresVisibleCaret()
        {
            Canvas canvas = CreateCanvas();
            var input = new State<string>(string.Empty);

            UIElements.TextField("Placeholder", text: input, prompt: UIElements.Text("placeholder"))
                .frame(width: 240, height: 36)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);
            Assert.That(field.customCaretColor, Is.True);
            Assert.That(field.caretWidth, Is.GreaterThanOrEqualTo(2));
            Assert.That(field.textViewport, Is.Not.Null);
            Assert.That(field.textViewport.GetComponent<RectMask2D>(), Is.Not.Null);
            Assert.That(field.textViewport.GetComponent<RectMask2D>().padding.x, Is.LessThan(0f));
            Assert.That(field.textViewport.offsetMin.x, Is.GreaterThan(0f));
            Assert.That(field.textViewport.offsetMax.x, Is.LessThan(0f));
            Assert.That(field.textComponent.extraPadding, Is.True);
            Assert.That((((TMP_Text)field.placeholder).fontStyle & FontStyles.Italic) != 0, Is.False);
            Assert.That(field.gameObject.activeSelf, Is.True);
        }

        [UnityTest]
        public IEnumerator TextField_ModifiersCustomizeChrome()
        {
            Canvas canvas = CreateCanvas();
            var input = new State<string>("user@example.com");
            var background = new Color(0.12f, 0.16f, 0.2f, 0.85f);
            var placeholder = new Color(0.65f, 0.7f, 0.76f, 0.9f);
            var caret = new Color(0.9f, 0.7f, 0.25f, 1f);
            var selection = new Color(0.2f, 0.5f, 0.95f, 0.35f);
            var focused = new Color(0.18f, 0.22f, 0.3f, 1f);

            UIElements.TextField(
                    "Email",
                    text: input,
                    prompt: UIElements.Text("email").italic().foregroundColor(placeholder))
                .textFieldStyle(TextFieldStyles.Chrome(
                    backgroundColor: background,
                    focusedBackgroundColor: focused,
                    textSelectionColor: selection,
                    contentMargins: new Vector4(12f, 14f, 6f, 8f),
                    caretWidth: 4,
                    caretBlinkRate: 1.2f))
                .caretColor(caret)
                .multilineTextAlignment(TextAlignmentOptions.MidlineRight)
                .textContentType(TMP_InputField.ContentType.EmailAddress)
                .textInputLimit(24)
                .keyboardType(TouchScreenKeyboardType.EmailAddress)
                .frame(width: 280, height: 44)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);
            Assert.That(field.GetComponent<Image>().color.r, Is.EqualTo(background.r).Within(0.001f));
            Assert.That(((TMP_Text)field.placeholder).color.g, Is.EqualTo(placeholder.g).Within(0.001f));
            Assert.That((((TMP_Text)field.placeholder).fontStyle & FontStyles.Italic) != 0, Is.True);
            Assert.That(field.caretColor.r, Is.EqualTo(caret.r).Within(0.001f));
            Assert.That(field.caretWidth, Is.EqualTo(4));
            Assert.That(field.caretBlinkRate, Is.EqualTo(1.2f).Within(0.001f));
            Assert.That(field.selectionColor.a, Is.EqualTo(selection.a).Within(0.001f));
            Assert.That(field.textViewport.offsetMin.x, Is.EqualTo(12f).Within(0.001f));
            Assert.That(field.textViewport.offsetMax.x, Is.EqualTo(-14f).Within(0.001f));
            Assert.That(field.textViewport.offsetMin.y, Is.EqualTo(8f).Within(0.001f));
            Assert.That(field.textViewport.offsetMax.y, Is.EqualTo(-6f).Within(0.001f));
            Assert.That(field.textComponent.alignment, Is.EqualTo(TextAlignmentOptions.MidlineRight));
            Assert.That(field.contentType, Is.EqualTo(TMP_InputField.ContentType.EmailAddress));
            Assert.That(field.characterLimit, Is.EqualTo(24));
            Assert.That(field.keyboardType, Is.EqualTo(TouchScreenKeyboardType.EmailAddress));
            Assert.That(field.GetComponent<EventTrigger>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator TextField_SwiftStylePromptUsesTextStyling()
        {
            Canvas canvas = CreateCanvas();
            var input = new State<string>(string.Empty);
            var promptColor = new Color(0.72f, 0.18f, 0.18f, 1f);

            UIElements.TextField(
                    "Email",
                    text: input,
                    prompt: UIElements.Text("Required")
                        .italic()
                        .foregroundColor(promptColor)
                        .fontSize(13f))
                .frame(width: 280, height: 44)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);

            TMP_Text prompt = (TMP_Text)field.placeholder;
            Assert.That(prompt.text, Is.EqualTo("Required"));
            Assert.That((prompt.fontStyle & FontStyles.Italic) != 0, Is.True);
            Assert.That(prompt.fontSize, Is.EqualTo(13f).Within(0.001f));
            AssertColor(prompt.color, promptColor);
        }

        [UnityTest]
        public IEnumerator FluentAliases_BuildAndBind()
        {
            Canvas canvas = CreateCanvas();
            var input = new State<string>(string.Empty);
            var isOn = new State<bool>(true);
            var tint = new Color(0.1f, 0.35f, 0.95f, 1f);
            int taps = 0;

            UIElements.VStack(() =>
            {
                UIElements.Text("Alias")
                    .italic()
                    .foregroundColor(tint)
                    .padding(8)
                    .background(new Color(0.92f, 0.96f, 1f))
                    .cornerRadius(8f)
                    .frame(width: 120f, height: 36f);

                UIElements.Button(action: () => taps++, label: UIElements.Text("Tap").bold())
                    .buttonStyle(ButtonStyles.Filled(tint, Color.white, 8f));

                UIElements.Toggle("Enabled", isOn: isOn)
                    .tint(tint);

                UIElements.TextField(
                        "Name",
                        text: input,
                        prompt: UIElements.Text("Name").italic().foregroundColor(Color.gray))
                    .lineLimit(1)
                    .contentMargins(horizontal: 10f, vertical: 6f)
                    .tint(tint);
            }, spacing: 6f, alignment: VStackAlignment.Leading)
            .frame(width: 280f, height: 190f)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            Button button = FindRect(canvas.transform, "Button").GetComponent<Button>();
            button.onClick.Invoke();
            Assert.That(taps, Is.EqualTo(1));

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);
            Assert.That((((TMP_Text)field.placeholder).fontStyle & FontStyles.Italic) != 0, Is.True);
            AssertColor(field.caretColor, tint);
        }

        [UnityTest]
        public IEnumerator FluentAdditions_BuildTextEditorPickerBackgroundAndImage()
        {
            Canvas canvas = CreateCanvas();
            var text = new State<string>("line 1\nline 2");
            var selection = new State<int>(1);
            var tint = new Color(0.1f, 0.35f, 0.95f, 1f);
            Texture2D texture = new Texture2D(4, 4);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 1f);

            try
            {
                UIElements.VStack(() =>
                {
                    UIElements.Text("Background")
                        .padding(Edge.Horizontal, 10)
                        .background(() =>
                        {
                            UIElements.RoundedRectangle(8f, new Color(0.92f, 0.96f, 1f));
                        })
                        .layoutPriority(2);

                    UIElements.Picker(selection, "One", "Two", "Three")
                        .pickerStyle(PickerStyle.Segmented)
                        .tint(tint);

                    UIElements.TextEditor(text)
                        .textFieldStyle(TextFieldStyles.Chrome(
                            backgroundColor: Color.white,
                            contentMargins: new Vector4(10f, 12f, 8f, 8f),
                            cornerRadius: 8f))
                        .frame(height: 96f);

                    UIElements.Image(sprite)
                        .resizable()
                        .scaledToFit()
                        .renderingMode(ImageRenderingMode.Template)
                        .tint(tint)
                        .frame(width: 48f, height: 32f);
                }, spacing: 6f, alignment: VStackAlignment.Leading)
                .frame(width: 320f, height: 220f)
                .Build(canvas);

                yield return null;
                ForceLayout(canvas);

                Assert.That(FindRect(canvas.transform, "BackgroundContentContainer").GetComponent<LayoutElement>().layoutPriority,
                    Is.EqualTo(2));
                Assert.That(FindRect(canvas.transform, "PickerSegmented"), Is.Not.Null);

                TMP_InputField editor = FindRect(canvas.transform, "TextEditor").GetComponent<TMP_InputField>();
                Assert.That(editor, Is.Not.Null);
                Assert.That(editor.lineType, Is.EqualTo(TMP_InputField.LineType.MultiLineNewline));
                Assert.That(editor.textComponent.textWrappingMode, Is.EqualTo(TextWrappingModes.Normal));

                Image image = FindRect(canvas.transform, "Image").GetComponent<Image>();
                Assert.That(image, Is.Not.Null);
                AssertColor(image.color, tint);
            }
            finally
            {
                if (sprite != null) Object.DestroyImmediate(sprite);
                if (texture != null) Object.DestroyImmediate(texture);
            }
        }

        [UnityTest]
        public IEnumerator TextField_LineLimitGreaterThanOneKeepsMultilineNewline()
        {
            Canvas canvas = CreateCanvas();
            var input = new State<string>("line 1");

            UIElements.TextField("Notes", text: input, prompt: UIElements.Text("notes"))
                .lineLimit(3)
                .textContentType(TMP_InputField.ContentType.Standard)
                .frame(width: 280, height: 84)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);
            Assert.That(field.lineLimit, Is.EqualTo(3));
            Assert.That(field.lineType, Is.EqualTo(TMP_InputField.LineType.MultiLineNewline));
        }

        [UnityTest]
        public IEnumerator TextField_SwiftLikeModifiersConfigureFocusSubmitAndSelection()
        {
            Canvas canvas = CreateCanvas();
            var input = new State<string>("select me");
            var focused = new State<bool>(false);
            var background = new Color(0.92f, 0.96f, 1f, 1f);
            var prompt = new Color(0.55f, 0.48f, 0.7f, 1f);
            var tint = new Color(0.1f, 0.35f, 0.95f, 1f);
            var selection = new Color(1f, 0.8f, 0.1f, 0.45f);
            bool? editing = null;
            string submitted = null;

            UIElements.TextField(
                    "Email",
                    text: input,
                    prompt: UIElements.Text("prompt").foregroundColor(prompt))
                .textFieldStyle(TextFieldStyles.Chrome(
                    backgroundColor: background,
                    contentMargins: new Vector4(10f, 16f, 7f, 9f)))
                .tint(tint)
                .textSelectionColor(selection)
                .multilineTextAlignment(TextAlignmentOptions.MidlineRight)
                .textContentType(TMP_InputField.ContentType.EmailAddress)
                .textInputLimit(20)
                .keyboardType(TouchScreenKeyboardType.EmailAddress)
                .focused(focused)
                .selectAllOnFocus(false)
                .onEditingChanged(value => editing = value)
                .onSubmit(value => submitted = value)
                .frame(width: 280, height: 44)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);
            Assert.That(field.GetComponent<Image>().color.r, Is.EqualTo(background.r).Within(0.001f));
            Assert.That(((TMP_Text)field.placeholder).color.b, Is.EqualTo(prompt.b).Within(0.001f));
            Assert.That(field.caretColor.b, Is.EqualTo(tint.b).Within(0.001f));
            Assert.That(field.selectionColor.a, Is.EqualTo(selection.a).Within(0.001f));
            Assert.That(field.textViewport.offsetMin.x, Is.EqualTo(10f).Within(0.001f));
            Assert.That(field.textViewport.offsetMax.x, Is.EqualTo(-16f).Within(0.001f));
            Assert.That(field.textComponent.alignment, Is.EqualTo(TextAlignmentOptions.MidlineRight));
            Assert.That(field.contentType, Is.EqualTo(TMP_InputField.ContentType.EmailAddress));
            Assert.That(field.characterLimit, Is.EqualTo(20));
            Assert.That(field.onFocusSelectAll, Is.False);

            field.onSelect.Invoke(field.text);
            Assert.That(focused.Value, Is.True);
            Assert.That(editing, Is.True);

            field.onSubmit.Invoke(field.text);
            Assert.That(submitted, Is.EqualTo("select me"));

            field.onDeselect.Invoke(field.text);
            Assert.That(focused.Value, Is.False);
            Assert.That(editing, Is.False);
        }

        [UnityTest]
        public IEnumerator TextField_FocusedBindingDoesNotReselectDuringDeselect()
        {
            Canvas canvas = CreateCanvas();
            EventSystem eventSystem = CreateTestEventSystem();
            var input = new State<string>("focused");
            var focused = new State<bool>(false);

            UIElements.TextField("Prompt", text: input, prompt: UIElements.Text("prompt"))
                .focused(focused)
                .frame(width: 280, height: 44)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);

            eventSystem.SetSelectedGameObject(field.gameObject);
            yield return null;
            Assert.That(focused.Value, Is.True);

            var nextSelection = new GameObject("NextSelection");
            nextSelection.transform.SetParent(canvas.transform, false);
            eventSystem.SetSelectedGameObject(nextSelection);
            yield return null;

            Assert.That(focused.Value, Is.False);
            Assert.That(eventSystem.currentSelectedGameObject, Is.EqualTo(nextSelection));
        }

        [UnityTest]
        public IEnumerator TextField_BackgroundModifierShowsWrapperFill()
        {
            Canvas canvas = CreateCanvas();
            var input = new State<string>(string.Empty);
            var fill = new Color(0.08f, 0.1f, 0.14f, 1f);

            UIElements.TextField("Username", text: input, prompt: UIElements.Text("username"))
                .background(fill)
                .frame(width: 280, height: 44)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            TMP_InputField field = canvas.GetComponentInChildren<TMP_InputField>();
            Assert.That(field, Is.Not.Null);

            Image fieldImage = field.GetComponent<Image>();
            Assert.That(fieldImage.color.a, Is.EqualTo(0f).Within(0.001f));

            Image wrapperImage = field.transform.parent.GetComponent<Image>();
            Assert.That(wrapperImage, Is.Not.Null);
            Assert.That(wrapperImage.color.r, Is.EqualTo(fill.r).Within(0.001f));
            Assert.That(wrapperImage.color.g, Is.EqualTo(fill.g).Within(0.001f));
            Assert.That(wrapperImage.color.b, Is.EqualTo(fill.b).Within(0.001f));
        }

        [Test]
        public void SpringEasing_StartsAtZeroAndEndsAtOne()
        {
            GameObject probeObject = new GameObject("EasingProbe");
            try
            {
                EasingProbe probe = probeObject.AddComponent<EasingProbe>();

                Assert.That(probe.Sample(0f, AnimationEasing.Spring), Is.EqualTo(0f).Within(0.0001f));
                Assert.That(probe.Sample(1f, AnimationEasing.Spring), Is.EqualTo(1f).Within(0.0001f));
                Assert.That(probe.Sample(0.01f, AnimationEasing.Spring), Is.InRange(-0.05f, 0.5f));
            }
            finally
            {
                Object.DestroyImmediate(probeObject);
            }
        }

        [Test]
        public void AllEasings_StartAndEndAtExactEndpoints()
        {
            GameObject probeObject = new GameObject("EasingProbe");
            try
            {
                EasingProbe probe = probeObject.AddComponent<EasingProbe>();
                AnimationEasing[] easings =
                {
                    AnimationEasing.Linear,
                    AnimationEasing.EaseIn,
                    AnimationEasing.EaseOut,
                    AnimationEasing.EaseInOut,
                    AnimationEasing.EaseOutBounce,
                    AnimationEasing.EaseOutElastic,
                    AnimationEasing.EaseOutBack,
                    AnimationEasing.Spring
                };

                foreach (AnimationEasing easing in easings)
                {
                    Assert.That(probe.Sample(0f, easing), Is.EqualTo(0f).Within(0.0001f), easing + " at t=0");
                    Assert.That(probe.Sample(1f, easing), Is.EqualTo(1f).Within(0.0001f), easing + " at t=1");
                }
            }
            finally
            {
                Object.DestroyImmediate(probeObject);
            }
        }

        [Test]
        public void ZeroDurationAnimator_AppliesTargetImmediately()
        {
            GameObject target = new GameObject("OpacityTarget");
            try
            {
                var animator = target.AddComponent<OpacityAnimator>();
                animator.AnimateTo(0f, 1f, 0f, AnimationEasing.Linear);

                CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
                Assert.That(canvasGroup, Is.Not.Null);
                Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.0001f));
            }
            finally
            {
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void ConcreteAnimatorsDeclareUnityUpdateMessage()
        {
            System.Type[] animatorTypes =
            {
                typeof(RotationAnimator),
                typeof(ScaleAnimator),
                typeof(PositionAnimator),
                typeof(OpacityAnimator),
                typeof(BackgroundColorAnimator),
                typeof(CornerRadiusAnimator),
                typeof(TextColorAnimator)
            };

            foreach (System.Type type in animatorTypes)
            {
                MethodInfo update = type.GetMethod("Update",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                Assert.That(update, Is.Not.Null, type.Name + " must declare Update so Unity advances it.");
            }
        }

        [UnityTest]
        public IEnumerator FramePaddingBackground_OrderKeepsExpectedOuterSizes()
        {
            Canvas canvas = CreateCanvas();

            UIElements.VStack(() =>
            {
                UIElements.Text("A")
                    .frame(width: 100, height: 20)
                    .padding(10)
                    .background(Color.red);

                UIElements.Text("B")
                    .padding(10)
                    .background(Color.blue)
                    .frame(width: 140, height: 50);
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform firstBackground = root.GetChild(0).GetComponent<RectTransform>();
            RectTransform firstPadding = FindRect(firstBackground, "PaddingContainer");
            RectTransform firstText = FindRect(firstPadding, "Text");
            RectTransform secondBackground = root.GetChild(1).GetComponent<RectTransform>();

            Assert.That(firstBackground.rect.width, Is.EqualTo(120f).Within(1f));
            Assert.That(firstBackground.rect.height, Is.EqualTo(40f).Within(1f));
            Assert.That(firstPadding.rect.width, Is.EqualTo(120f).Within(1f));
            Assert.That(firstText.rect.width, Is.EqualTo(100f).Within(1f));
            Assert.That(firstText.rect.height, Is.EqualTo(20f).Within(1f));

            Assert.That(secondBackground.rect.width, Is.EqualTo(140f).Within(1f));
            Assert.That(secondBackground.rect.height, Is.EqualTo(50f).Within(1f));
        }

        [UnityTest]
        public IEnumerator ZStack_AllAlignmentsPlaceForegroundInFixedFrame()
        {
            Canvas canvas = CreateCanvas();
            ZStackAlignment[] alignments =
            {
                ZStackAlignment.TopLeading,
                ZStackAlignment.Top,
                ZStackAlignment.TopTrailing,
                ZStackAlignment.Leading,
                ZStackAlignment.Center,
                ZStackAlignment.Trailing,
                ZStackAlignment.BottomLeading,
                ZStackAlignment.Bottom,
                ZStackAlignment.BottomTrailing
            };

            UIElements.VStack(() =>
            {
                foreach (ZStackAlignment alignment in alignments)
                {
                    UIElements.ZStack(() =>
                    {
                        UIElements.Text("bg").frame(width: 80, height: 40);
                        UIElements.Text("fg").frame(width: 20, height: 10);
                    }, alignment: alignment)
                    .frame(width: 80, height: 40);
                }
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            for (int i = 0; i < alignments.Length; i++)
            {
                RectTransform zstack = root.GetChild(i).GetComponent<RectTransform>();
                RectTransform foreground = zstack.GetChild(1).GetComponent<RectTransform>();
                Rect localRect = GetLocalRect(zstack, foreground);

                AssertHorizontalAlignment(alignments[i], localRect, -40f, 40f);
                AssertVerticalAlignment(alignments[i], localRect, -20f, 20f);
            }
        }

        [UnityTest]
        public IEnumerator ScrollView_ContentSizingMatchesEnabledAxes()
        {
            Canvas canvas = CreateCanvas();

            UIElements.VStack(() =>
            {
                UIElements.ScrollView(() =>
                {
                    for (int i = 0; i < 6; i++)
                        UIElements.Text("row " + i).frame(height: 30, infiniteWidth: true);
                }, horizontal: false, vertical: true)
                .frame(width: 300, height: 120);

                UIElements.ScrollView(() =>
                {
                    UIElements.HStack(() =>
                    {
                        for (int i = 0; i < 5; i++)
                            UIElements.Text("col " + i).frame(width: 80, height: 30);
                    }, spacing: 0f);
                }, horizontal: true, vertical: false)
                .frame(width: 300, height: 120);

                UIElements.ScrollView(() =>
                {
                    for (int i = 0; i < 6; i++)
                        UIElements.Text("wide " + i).frame(width: 380, height: 30);
                }, horizontal: true, vertical: true)
                .frame(width: 300, height: 120);
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform verticalContent = FindRect(root.GetChild(0), "Content");
            RectTransform horizontalContent = FindRect(root.GetChild(1), "Content");
            RectTransform bothContent = FindRect(root.GetChild(2), "Content");

            Assert.That(verticalContent.rect.width, Is.EqualTo(300f).Within(1.5f));
            Assert.That(verticalContent.rect.height, Is.GreaterThan(120f));

            Assert.That(horizontalContent.rect.width, Is.GreaterThan(300f));
            Assert.That(horizontalContent.rect.height, Is.EqualTo(120f).Within(1.5f));

            Assert.That(bothContent.rect.width, Is.GreaterThan(300f));
            Assert.That(bothContent.rect.height, Is.GreaterThan(120f));
        }

        [UnityTest]
        public IEnumerator TabView_ContentAreaFillsRemainingHeightAfterSwitch()
        {
            Canvas canvas = CreateCanvas();
            var selected = new State<int>(0);

            UIElements.VStack(() =>
            {
                TabView tabView = new TabView(() =>
                {
                    new TabItem("One", () =>
                    {
                        UIElements.Text("one").frame(infiniteWidth: true, infiniteHeight: true);
                    });
                    new TabItem("Two", () =>
                    {
                        UIElements.Text("two").frame(infiniteWidth: true, infiniteHeight: true);
                    });
                }, selected)
                .WithTransitionDuration(0f);

                UIContext.Current.AddChild(tabView.frame(width: 320, height: 240));
            }, spacing: 0f)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform tabViewRect = root.GetChild(0).GetComponent<RectTransform>();
            AssertTabViewSizing(tabViewRect);

            selected.Value = 1;
            yield return null;
            yield return null;
            ForceLayout(canvas);

            AssertTabViewSizing(tabViewRect);
            RectTransform contentArea = FindRect(tabViewRect, "ContentArea");
            Assert.That(contentArea.childCount, Is.GreaterThanOrEqualTo(1));
        }

        [UnityTest]
        public IEnumerator TabView_HeaderClickSwitchesContent()
        {
            Canvas canvas = CreateCanvas();
            var selected = new State<int>(0);

            UIElements.VStack(() =>
            {
                TabView tabView = new TabView(() =>
                {
                    new TabItem("One", () =>
                    {
                        UIElements.Text("one").frame(infiniteWidth: true, infiniteHeight: true);
                    });
                    new TabItem("Two", () =>
                    {
                        UIElements.Text("two").frame(infiniteWidth: true, infiniteHeight: true);
                    });
                }, selected)
                .WithTransitionDuration(0f);

                UIContext.Current.AddChild(tabView.frame(width: 320, height: 240));
            }, spacing: 0f)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            Button[] buttons = canvas.GetComponentsInChildren<Button>();
            Assert.That(buttons.Length, Is.GreaterThanOrEqualTo(2));

            buttons[1].onClick.Invoke();
            yield return null;
            ForceLayout(canvas);

            Assert.That(selected.Value, Is.EqualTo(1));
            TMP_Text[] labels = canvas.GetComponentsInChildren<TMP_Text>();
            Assert.That(System.Array.Exists(labels, label => label.text == "two"), Is.True);
            Assert.That(System.Array.Exists(labels, label => label.text == "one"), Is.False);
        }

        [UnityTest]
        public IEnumerator RotationAfterBackground_TargetsBackgroundWrapper()
        {
            Canvas canvas = CreateCanvas();
            var rotation = new State<float>(0f);

            UIElements.VStack(() =>
            {
                UIElements.Text("Background + RotationEffect(State)")
                    .padding(12)
                    .background(new Color(0.93f, 0.95f, 0.99f))
                    .rotationEffect(rotation)
                    .animation(AnimationEasing.EaseInOut, 0.45f)
                    .frame(infiniteWidth: true);
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform background = FindRect(root, "BackgroundContainer");

            rotation.Value = 25f;
            yield return null;
            ForceLayout(canvas);

            RotationAnimator[] animators = root.GetComponentsInChildren<RotationAnimator>(true);
            Assert.That(animators.Length, Is.EqualTo(1));
            Assert.That(animators[0].gameObject, Is.EqualTo(background.gameObject));
        }

        [UnityTest]
        public IEnumerator RotationBeforeDirectTextBackground_TargetsTextOnly()
        {
            Canvas canvas = CreateCanvas();
            var rotation = new State<float>(0f);

            UIElements.VStack(() =>
            {
                UIElements.Text("Rotation + direct Text.Background")
                    .rotationEffect(rotation)
                    .animation(AnimationEasing.EaseInOut, 0.45f)
                    .background(new Color(0.93f, 0.95f, 0.99f))
                    .frame(infiniteWidth: true);
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform background = FindRect(root, "BackgroundContainer");

            rotation.Value = 25f;
            yield return null;
            ForceLayout(canvas);

            RotationAnimator[] animators = root.GetComponentsInChildren<RotationAnimator>(true);
            Assert.That(animators.Length, Is.EqualTo(1));
            Assert.That(animators[0].gameObject.name, Is.EqualTo("Text"));
            Assert.That(background.GetComponent<RotationAnimator>(), Is.Null);
            Assert.That(Quaternion.Angle(background.localRotation, Quaternion.identity), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator RotationBeforeDirectButtonBackground_TargetsButtonOnly()
        {
            Canvas canvas = CreateCanvas();
            var rotation = new State<float>(0f);

            UIElements.VStack(() =>
            {
                UIElements.Button("Tap", () => { })
                    .rotationEffect(rotation)
                    .animation(AnimationEasing.EaseInOut, 0.45f)
                    .background(new Color(0.93f, 0.95f, 0.99f));
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform background = FindRect(root, "BackgroundContainer");
            RectTransform button = FindRect(root, "Button");

            rotation.Value = 25f;
            yield return null;
            ForceLayout(canvas);

            RotationAnimator[] animators = root.GetComponentsInChildren<RotationAnimator>(true);
            Assert.That(animators.Length, Is.EqualTo(1));
            Assert.That(animators[0].gameObject, Is.EqualTo(button.gameObject));
            Assert.That(background.GetComponent<RotationAnimator>(), Is.Null);
            Assert.That(Quaternion.Angle(background.localRotation, Quaternion.identity), Is.LessThan(0.01f));
        }

        [UnityTest]
        public IEnumerator CornerRadiusOnlyAffectsTheElementThatReceivesIt()
        {
            Canvas canvas = CreateCanvas();

            UIElements.VStack(() =>
            {
                UIElements.Text("before background")
                    .cornerRadius(14f)
                    .padding(8)
                    .background(Color.red);

                UIElements.Text("after background")
                    .padding(8)
                    .background(Color.blue)
                    .cornerRadius(14f);
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform firstBackground = FindRect(root, "BackgroundContainer");
            RectTransform secondBackground = FindRect(root, "BackgroundContainer", 1);

            Assert.That(firstBackground.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>(), Is.Null);

            var rounded = secondBackground.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            Assert.That(rounded, Is.Not.Null);
            Assert.That(rounded.r.x, Is.EqualTo(14f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator BorderCornerRadiusPropagatesToContent()
        {
            Canvas canvas = CreateCanvas();

            UIElements.Text("bordered")
                .padding(8)
                .background(Color.red)
                .border(Color.black, 2f)
                .cornerRadius(12f)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform border = FindRect(canvas.transform, "BorderContainer");
            RectTransform background = FindRect(border, "BackgroundContainer");

            var borderRounded = border.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            var backgroundRounded = background.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();

            Assert.That(borderRounded, Is.Not.Null);
            Assert.That(backgroundRounded, Is.Not.Null);
            Assert.That(borderRounded.r.x, Is.EqualTo(12f).Within(0.01f));
            Assert.That(backgroundRounded.r.x, Is.EqualTo(12f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator TransparentWrappersPropagateCornerRadiusToContent()
        {
            Canvas canvas = CreateCanvas();

            UIElements.VStack(() =>
            {
                UIElements.Text("opacity")
                    .padding(4)
                    .background(Color.red)
                    .opacity(0.8f)
                    .cornerRadius(9f);

                UIElements.Text("offset")
                    .padding(4)
                    .background(Color.red)
                    .offset(2f, 2f)
                    .cornerRadius(9f);

                UIElements.Text("fixed")
                    .padding(4)
                    .background(Color.red)
                    .fixedSize()
                    .cornerRadius(9f);

                UIElements.Text("aspect")
                    .padding(4)
                    .background(Color.red)
                    .aspectRatio(2f, AspectRatioContentMode.Fit)
                    .cornerRadius(9f);

                UIElements.Text("shadow")
                    .padding(4)
                    .background(Color.red)
                    .shadow(Color.black, 2f, 1f, 1f)
                    .cornerRadius(9f);

                UIElements.Text("overlay")
                    .padding(4)
                    .background(Color.red)
                    .overlay(UIElements.Text("!"), ZStackAlignment.TopTrailing)
                    .cornerRadius(9f);

                UIElements.Text("background content")
                    .padding(4)
                    .background(Color.red)
                    .background(UIElements.Rectangle(Color.blue), ZStackAlignment.Center)
                    .cornerRadius(9f);
            }, spacing: 4f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            for (int i = 0; i < 7; i++)
            {
                RectTransform background = FindRect(canvas.transform, "BackgroundContainer", i);
                var rounded = background.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
                Assert.That(rounded, Is.Not.Null, "BackgroundContainer " + i + " should receive propagated corner radius.");
                Assert.That(rounded.r.x, Is.EqualTo(9f).Within(0.01f));
            }
        }

        [UnityTest]
        public IEnumerator ImageOpacityStateWorksWhenImageIsTypedAsUIElement()
        {
            Canvas canvas = CreateCanvas();
            var opacity = new State<float>(1f);
            Texture2D texture = new Texture2D(4, 4);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 1f);

            try
            {
                UIElement image = UIElements.Image(sprite);
                image.opacity(opacity).Build(canvas);

                yield return null;
                ForceLayout(canvas);

                opacity.Value = 0.25f;
                yield return null;
                ForceLayout(canvas);

                RectTransform opacityContainer = FindRect(canvas.transform, "OpacityContainer");
                CanvasGroup canvasGroup = opacityContainer.GetComponent<CanvasGroup>();
                Assert.That(canvasGroup, Is.Not.Null);
                Assert.That(canvasGroup.alpha, Is.EqualTo(0.25f).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        [UnityTest]
        public IEnumerator ForegroundColorStateUpdatesButtonAndTextFieldAfterBuild()
        {
            Canvas canvas = CreateCanvas();
            var color = new State<Color>(Color.black);
            var input = new State<string>("value");

            UIElements.VStack(() =>
            {
                UIElements.Button("button", () => { })
                    .foregroundColor(color);

                UIElements.TextField("Placeholder", text: input, prompt: UIElements.Text("placeholder"))
                    .foregroundColor(color);
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            Color target = new Color(0.2f, 0.45f, 0.85f, 1f);
            color.Value = target;
            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            RectTransform button = FindRect(root, "Button");
            RectTransform textField = FindRect(root, "TextField");

            AssertColor(button.GetComponentInChildren<TMP_Text>(true).color, target);
            AssertColor(FindRect(textField, "Text").GetComponent<TMP_Text>().color, target);
        }

        [UnityTest]
        public IEnumerator WithAnimation_CountChangeDoesNotAnimateUnchangedOpacityBinding()
        {
            Canvas canvas = CreateCanvas();
            var count = new State<int>(0);
            var opacity = new State<float>(1f);

            UIElements.Text(() => $"count = {count.Value}", new State[] { count })
                .opacity(opacity)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            global::UniftUI.AnimationContext.WithAnimation(global::UniftUI.Animation.spring(0.5f, 0.8f), () =>
            {
                count.Value++;
            });

            yield return null;
            ForceLayout(canvas);

            RectTransform opacityContainer = FindRect(canvas.transform, "OpacityContainer");
            Assert.That(opacityContainer.GetComponent<OpacityAnimator>(), Is.Null);
            CanvasGroup canvasGroup = opacityContainer.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator AnimationRestart_UsesCurrentRenderedOpacityAsStartValue()
        {
            Canvas canvas = CreateCanvas();
            var opacity = new State<float>(1f);

            UIElements.Text("animated opacity")
                .opacity(opacity)
                .animation(AnimationEasing.Linear, 1f)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            opacity.Value = 0.2f;
            yield return null;
            ForceLayout(canvas);

            RectTransform opacityContainer = FindRect(canvas.transform, "OpacityContainer");
            CanvasGroup canvasGroup = opacityContainer.GetComponent<CanvasGroup>();
            Assert.That(canvasGroup, Is.Not.Null);

            canvasGroup.alpha = 0.6f;
            opacity.Value = 0.9f;
            yield return null;
            ForceLayout(canvas);

            Assert.That(canvasGroup.alpha, Is.EqualTo(0.6f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator AnimationValue_OpacityWrapperAnimatesExactModifierChain()
        {
            Canvas canvas = CreateCanvas();
            var metric = new State<int>(0);
            var alpha = new State<float>(1f);
            var radius = new State<float>(10f);
            var liveWidth = new State<float>(170f);
            var swatch = new State<Color>(new Color(0.93f, 0.96f, 1f));

            UIElements.Text(() => $"metric={metric.Value}", new State[] { metric })
                .padding(12)
                .background(swatch)
                .cornerRadius(radius)
                .frame(width: liveWidth)
                .opacity(alpha)
                .animation(global::UniftUI.Animation.easeOut(0.35f), alpha)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            alpha.Value = 0.45f;
            yield return null;
            ForceLayout(canvas);

            RectTransform opacityContainer = FindRect(canvas.transform, "OpacityContainer");
            Assert.That(opacityContainer.GetComponent<OpacityAnimator>(), Is.Not.Null);
            Assert.That(FindRect(canvas.transform, "BackgroundContainer").GetComponent<OpacityAnimator>(), Is.Null);
            Assert.That(FindRect(canvas.transform, "PaddingContainer").GetComponent<OpacityAnimator>(), Is.Null);
        }

        [UnityTest]
        public IEnumerator AnimationValue_OpacityWrapperReachesTargetAlphaOnExactModifierChain()
        {
            Canvas canvas = CreateCanvas();
            var metric = new State<int>(0);
            var alpha = new State<float>(1f);
            var radius = new State<float>(10f);
            var liveWidth = new State<float>(170f);
            var swatch = new State<Color>(new Color(0.93f, 0.96f, 1f));

            UIElements.Text(() => $"metric={metric.Value}", new State[] { metric })
                .padding(12)
                .background(swatch)
                .cornerRadius(radius)
                .frame(width: liveWidth)
                .opacity(alpha)
                .animation(global::UniftUI.Animation.easeOut(0.001f), alpha)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            alpha.Value = 0.45f;
            yield return new WaitForSeconds(0.1f);
            ForceLayout(canvas);

            RectTransform opacityContainer = FindRect(canvas.transform, "OpacityContainer");
            CanvasGroup canvasGroup = opacityContainer.GetComponent<CanvasGroup>();
            Assert.That(canvasGroup, Is.Not.Null);
            Assert.That(canvasGroup.alpha, Is.EqualTo(0.45f).Within(0.05f));
        }

        [UnityTest]
        public IEnumerator AnimationValue_ContentDependencyDoesNotTriggerOpacityAnimation()
        {
            Canvas canvas = CreateCanvas();
            var metric = new State<int>(0);
            var alpha = new State<float>(1f);
            var radius = new State<float>(10f);
            var liveWidth = new State<float>(170f);
            var swatch = new State<Color>(new Color(0.93f, 0.96f, 1f));

            UIElements.Text(() => $"metric={metric.Value}", new State[] { metric })
                .padding(12)
                .background(swatch)
                .cornerRadius(radius)
                .frame(width: liveWidth)
                .opacity(alpha)
                .animation(global::UniftUI.Animation.easeOut(0.35f), alpha)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            metric.Value++;
            yield return null;
            ForceLayout(canvas);

            RectTransform opacityContainer = FindRect(canvas.transform, "OpacityContainer");
            Assert.That(opacityContainer.GetComponent<OpacityAnimator>(), Is.Null);

            CanvasGroup canvasGroup = opacityContainer.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                Assert.That(canvasGroup.alpha, Is.EqualTo(1f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator WithAnimation_OffsetStateAnimatesInsteadOfJumping()
        {
            Canvas canvas = CreateCanvas();
            var offsetX = new State<float>(-64f);

            UIElements.Text("drawer")
                .frame(width: 120f, height: 30f)
                .offset(offsetX, 0f)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform offset = FindRect(canvas.transform, "Offset");
            Assert.That(ReadVisualOffset(offset).x, Is.EqualTo(-64f).Within(0.01f));

            global::UniftUI.AnimationContext.WithAnimation(global::UniftUI.Animation.easeInOut(0.3f), () =>
            {
                offsetX.Value = 0f;
            });

            yield return null;
            ForceLayout(canvas);

            Assert.That(HasComponentNamed(offset, "VisualOffsetAnimator"), Is.True);
            Assert.That(ReadVisualOffset(offset).x, Is.EqualTo(-64f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator WithAnimation_FrameStateAnimatesLayoutSizeInsteadOfJumping()
        {
            Canvas canvas = CreateCanvas();
            var width = new State<float>(80f);
            var height = new State<float>(24f);

            UIElements.Text("frame")
                .frame(width: width, height: height)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform text = FindRect(canvas.transform, "Text");
            LayoutElement layout = text.GetComponent<LayoutElement>();
            Assert.That(layout.preferredWidth, Is.EqualTo(80f).Within(0.01f));
            Assert.That(layout.preferredHeight, Is.EqualTo(24f).Within(0.01f));

            global::UniftUI.AnimationContext.WithAnimation(global::UniftUI.Animation.easeInOut(0.3f), () =>
            {
                width.Value = 160f;
                height.Value = 48f;
            });

            yield return null;
            ForceLayout(canvas);

            Assert.That(HasComponentNamed(text, "LayoutWidthAnimator"), Is.True);
            Assert.That(HasComponentNamed(text, "LayoutHeightAnimator"), Is.True);
            Assert.That(layout.preferredWidth, Is.EqualTo(80f).Within(0.01f));
            Assert.That(layout.preferredHeight, Is.EqualTo(24f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator AnimationValue_FrameStateBeforeBackgroundAnimatesOuterFrame()
        {
            Canvas canvas = CreateCanvas();
            var width = new State<float>(90f);

            UIElements.Text("frame")
                .frame(width: width)
                .frame(height: 28f)
                .background(Color.red)
                .animation(global::UniftUI.Animation.linear(1f), width)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform background = FindRect(canvas.transform, "BackgroundContainer");
            LayoutElement layout = background.GetComponent<LayoutElement>();
            Assert.That(layout.preferredWidth, Is.EqualTo(90f).Within(0.01f));

            width.Value = 170f;

            yield return null;
            ForceLayout(canvas);

            Assert.That(HasComponentNamed(background, "LayoutWidthAnimator"), Is.True);
            Assert.That(layout.preferredWidth, Is.EqualTo(90f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator WithAnimation_PaddingStateAnimatesInsetsInsteadOfJumping()
        {
            Canvas canvas = CreateCanvas();
            var padding = new State<int>(4);

            UIElements.Text("padding")
                .padding(padding)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform paddingContainer = FindRect(canvas.transform, "PaddingContainer");
            LayoutGroup layoutGroup = paddingContainer.GetComponent<LayoutGroup>();
            Assert.That(layoutGroup.padding.left, Is.EqualTo(4));

            global::UniftUI.AnimationContext.WithAnimation(global::UniftUI.Animation.easeInOut(0.3f), () =>
            {
                padding.Value = 18;
            });

            yield return null;
            ForceLayout(canvas);

            Assert.That(HasComponentNamed(paddingContainer, "PaddingAnimator"), Is.True);
            Assert.That(layoutGroup.padding.left, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator AnimationValue_CornerRadiusStateAnimatesInsteadOfJumping()
        {
            Canvas canvas = CreateCanvas();
            var radius = new State<float>(4f);

            UIElements.Text("radius")
                .padding(8)
                .background(Color.white)
                .cornerRadius(radius)
                .animation(global::UniftUI.Animation.easeInOut(0.3f), radius)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform background = FindRect(canvas.transform, "BackgroundContainer");
            var rounded = background.GetComponent<Nobi.UiRoundedCorners.ImageWithIndependentRoundedCorners>();
            Assert.That(rounded, Is.Not.Null);
            Assert.That(rounded.r.x, Is.EqualTo(4f).Within(0.01f));

            radius.Value = 24f;

            yield return null;
            ForceLayout(canvas);

            Assert.That(HasComponentNamed(background, "CornerRadiusAnimator"), Is.True);
            Assert.That(rounded.r.x, Is.EqualTo(4f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator TextColorAnimationRestart_UsesCurrentRenderedColorAsStartValue()
        {
            Canvas canvas = CreateCanvas();
            var color = new State<Color>(Color.black);

            UIElements.Text("animated color")
                .foregroundColor(color)
                .animation(AnimationEasing.Linear, 1f)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            color.Value = Color.red;
            yield return null;
            ForceLayout(canvas);

            TMP_Text text = FindRect(canvas.transform, "Text").GetComponent<TMP_Text>();
            Color current = new Color(0.35f, 0.36f, 0.37f, 1f);
            text.color = current;

            color.Value = Color.green;
            yield return null;
            ForceLayout(canvas);

            AssertColor(text.color, current);
        }

        [UnityTest]
        public IEnumerator ReactiveVectorAndAxisTransformBindings_UseConfiguredAnimation()
        {
            Canvas canvas = CreateCanvas();
            var rotation = new State<Vector3>(Vector3.zero);
            var scaleX = new State<float>(1f);
            var scaleY = new State<float>(1f);
            var scaleVector = new State<Vector3>(Vector3.one);

            UIElements.VStack(() =>
            {
                UIElements.Text("rotation vector")
                    .rotationEffect(rotation)
                    .animation(AnimationEasing.EaseInOut, 0.3f);

                UIElements.Text("scale x")
                    .scaleEffect(scaleX, 1f)
                    .animation(AnimationEasing.EaseInOut, 0.3f);

                UIElements.Text("scale y")
                    .scaleEffect(1f, scaleY)
                    .animation(AnimationEasing.EaseInOut, 0.3f);

                UIElements.Text("scale vector")
                    .scaleEffect(scaleVector)
                    .animation(AnimationEasing.EaseInOut, 0.3f);
            }, spacing: 0f, alignment: VStackAlignment.Leading)
            .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            rotation.Value = new Vector3(5f, 0f, 20f);
            scaleX.Value = 1.2f;
            scaleY.Value = 0.8f;
            scaleVector.Value = new Vector3(1.1f, 0.9f, 1f);
            yield return null;
            ForceLayout(canvas);

            RectTransform root = canvas.transform.GetChild(0).GetComponent<RectTransform>();
            Assert.That(FindRect(root, "Text", 0).GetComponent<RotationAnimator>(), Is.Not.Null);
            Assert.That(FindRect(root, "Text", 1).GetComponent<ScaleAnimator>(), Is.Not.Null);
            Assert.That(FindRect(root, "Text", 2).GetComponent<ScaleAnimator>(), Is.Not.Null);
            Assert.That(FindRect(root, "Text", 3).GetComponent<ScaleAnimator>(), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator SpringCountDemo_DoesNotChangeCardSizeOnFirstUpdate()
        {
            Canvas canvas = CreateCanvas();
            var count = new State<int>(0);
            var opacity = new State<float>(1f);

            UIElements.Text(() => $"count = {count.Value}（spring で不透明度も連動）", new State[] { count })
                .opacity(opacity)
                .padding(12)
                .background(new Color(0.95f, 0.92f, 0.88f))
                .frame(width: 360f)
                .Build(canvas);

            yield return null;
            ForceLayout(canvas);

            RectTransform background = FindRect(canvas.transform, "BackgroundContainer");
            Vector2 before = background.rect.size;

            global::UniftUI.AnimationContext.WithAnimation(global::UniftUI.Animation.spring(0.5f, 0.8f), () =>
            {
                count.Value++;
                opacity.Value = 0.42f;
            });

            yield return null;
            ForceLayout(canvas);

            Vector2 after = background.rect.size;
            Assert.That(after.x, Is.EqualTo(before.x).Within(0.5f));
            Assert.That(after.y, Is.EqualTo(before.y).Within(0.5f));
        }

        private Canvas CreateCanvas()
        {
            canvasObject = new GameObject("TestCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            RectTransform rect = canvasObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(800, 600);
            return canvas;
        }

        private EventSystem CreateTestEventSystem()
        {
            eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            testEventSystem = eventSystemObject.GetComponent<EventSystem>();
            var systems = GetRegisteredEventSystems();
            if (!systems.Contains(testEventSystem))
                systems.Insert(0, testEventSystem);
            EventSystem.current = testEventSystem;
            return testEventSystem;
        }

        private static void UnregisterTestEventSystem(EventSystem eventSystem)
        {
            var systems = GetRegisteredEventSystems();
            while (systems.Contains(eventSystem))
                systems.Remove(eventSystem);
        }

        private static System.Collections.Generic.List<EventSystem> GetRegisteredEventSystems()
        {
            FieldInfo field = typeof(EventSystem).GetField("m_EventSystems", BindingFlags.NonPublic | BindingFlags.Static);
            return (System.Collections.Generic.List<EventSystem>)field.GetValue(null);
        }

        private sealed class EasingProbe : BaseAnimator<float>
        {
            public float Sample(float t, AnimationEasing easing)
            {
                return ApplyEasing(t, easing);
            }

            protected override void SetInitialValue(float value)
            {
            }

            protected override void UpdateValue(float t)
            {
            }
        }

        private static void ForceLayout(Canvas canvas)
        {
            Canvas.ForceUpdateCanvases();
            foreach (RectTransform rect in canvas.GetComponentsInChildren<RectTransform>(true))
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            Canvas.ForceUpdateCanvases();
        }

        private static RectTransform FindRect(Transform root, string name, int skip = 0)
        {
            foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect.name != name)
                    continue;

                if (skip <= 0)
                    return rect;

                skip--;
            }

            Assert.Fail("Could not find RectTransform named " + name);
            return null;
        }

        private static Rect GetLocalRect(RectTransform parent, RectTransform child)
        {
            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);

            Vector3 first = parent.InverseTransformPoint(corners[0]);
            float minX = first.x;
            float maxX = first.x;
            float minY = first.y;
            float maxY = first.y;

            for (int i = 1; i < corners.Length; i++)
            {
                Vector3 local = parent.InverseTransformPoint(corners[i]);
                minX = Mathf.Min(minX, local.x);
                maxX = Mathf.Max(maxX, local.x);
                minY = Mathf.Min(minY, local.y);
                maxY = Mathf.Max(maxY, local.y);
            }

            return Rect.MinMaxRect(minX, minY, maxX, maxY);
        }

        private static void AssertHorizontalAlignment(ZStackAlignment alignment, Rect rect, float parentLeft, float parentRight)
        {
            switch (alignment)
            {
                case ZStackAlignment.TopLeading:
                case ZStackAlignment.Leading:
                case ZStackAlignment.BottomLeading:
                    Assert.That(rect.xMin, Is.EqualTo(parentLeft).Within(1f));
                    break;
                case ZStackAlignment.TopTrailing:
                case ZStackAlignment.Trailing:
                case ZStackAlignment.BottomTrailing:
                    Assert.That(rect.xMax, Is.EqualTo(parentRight).Within(1f));
                    break;
                default:
                    Assert.That(rect.center.x, Is.EqualTo(0f).Within(1f));
                    break;
            }
        }

        private static void AssertVerticalAlignment(ZStackAlignment alignment, Rect rect, float parentBottom, float parentTop)
        {
            switch (alignment)
            {
                case ZStackAlignment.TopLeading:
                case ZStackAlignment.Top:
                case ZStackAlignment.TopTrailing:
                    Assert.That(rect.yMax, Is.EqualTo(parentTop).Within(1f));
                    break;
                case ZStackAlignment.BottomLeading:
                case ZStackAlignment.Bottom:
                case ZStackAlignment.BottomTrailing:
                    Assert.That(rect.yMin, Is.EqualTo(parentBottom).Within(1f));
                    break;
                default:
                    Assert.That(rect.center.y, Is.EqualTo(0f).Within(1f));
                    break;
            }
        }

        private static void AssertColor(Color actual, Color expected)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.01f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.01f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.01f));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.01f));
        }

        private static bool HasComponentNamed(RectTransform rect, string componentName)
        {
            foreach (Component component in rect.GetComponents<Component>())
            {
                if (component != null && component.GetType().Name == componentName)
                    return true;
            }

            return false;
        }

        private static Vector2 ReadVisualOffset(RectTransform rect)
        {
            foreach (Component component in rect.GetComponents<Component>())
            {
                if (component == null || component.GetType().Name != "UniftUISingleChildLayoutGroup")
                    continue;

                var field = component.GetType().GetField(
                    "visualOffset",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (field != null)
                    return (Vector2)field.GetValue(component);
            }

            Assert.Fail("Could not read UniftUISingleChildLayoutGroup.visualOffset.");
            return Vector2.zero;
        }

        private static void AssertTabViewSizing(RectTransform tabViewRect)
        {
            RectTransform tabBar = FindRect(tabViewRect, "TabBar");
            RectTransform contentArea = FindRect(tabViewRect, "ContentArea");

            Assert.That(tabViewRect.rect.width, Is.EqualTo(320f).Within(1f));
            Assert.That(tabViewRect.rect.height, Is.EqualTo(240f).Within(1f));
            Assert.That(tabBar.rect.height, Is.EqualTo(60f).Within(1f));
            Assert.That(contentArea.rect.height, Is.EqualTo(180f).Within(1f));
        }
    }
}
