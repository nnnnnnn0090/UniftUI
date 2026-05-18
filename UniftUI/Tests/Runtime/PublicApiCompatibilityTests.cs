using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace UniftUI.Tests
{
    public class PublicApiCompatibilityTests
    {
        [UnityTest]
        public IEnumerator LowercaseAliasesAndStaticFactories_BuildTogether()
        {
            Canvas canvas = CreateCanvas();
            var text = new State<string>("hello");
            var tint = new State<Color>(new Color(0.1f, 0.35f, 0.95f, 1f));
            int clicks = 0;

            try
            {
                UIElements.VStack(() =>
                {
                    UIElements.Text(text)
                        .fontSize(18)
                        .bold()
                        .foregroundColor(tint)
                        .padding(6)
                        .background(Color.white)
                        .cornerRadius(6)
                        .frame(width: 160, height: 36);

                    UIElements.Button("Tap", () => clicks++)
                        .buttonStyle(ButtonStyles.Filled(tint.Value, Color.white, 8f))
                        .frame(width: 120, height: 36);

                    UIElements.HStack(() =>
                    {
                        UIElements.Circle(Color.green).frame(width: 16, height: 16);
                        UIElements.Divider().frame(width: 80, height: 1);
                    }, spacing: 4f);
                }, spacing: 8f)
                .frame(width: 260, height: 160)
                .Build(canvas);

                yield return null;

                TMP_Text builtText = canvas.GetComponentInChildren<TMP_Text>();
                Assert.That(builtText.text, Is.EqualTo("hello"));

                text.Value = "updated";
                tint.Value = Color.red;
                yield return null;

                Assert.That(builtText.text, Is.EqualTo("updated"));

                Button button = canvas.GetComponentInChildren<Button>();
                Assert.That(button, Is.Not.Null);
                button.onClick.Invoke();
                Assert.That(clicks, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(canvas.gameObject);
            }
        }

        [UnityTest]
        public IEnumerator LowercaseAliases_CoverSpecializedControlsAndConfiguration()
        {
            Canvas canvas = CreateCanvas();
            Texture2D texture = new Texture2D(2, 2);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));

            var fieldText = new State<string>("mail@example.com");
            var focused = new State<bool>(false);
            var pickerSelection = new State<int>(1);
            var progress = new State<float>(0.35f);
            var slider = new State<float>(0.5f);
            var scrollY = new State<float>(0.25f);
            var scrollX = new State<float>(0.75f);
            var foreground = new State<Color>(new Color(0.2f, 0.25f, 0.3f, 1f));
            var tint = new State<Color>(new Color(0.85f, 0.2f, 0.4f, 1f));
            var caret = new State<Color>(new Color(0.1f, 0.7f, 0.9f, 1f));
            var selection = new State<Color>(new Color(0.1f, 0.7f, 0.9f, 0.35f));
            string submitted = null;

            try
            {
                UIElements.VStack(() =>
                {
                    UIElements.HStack(() =>
                    {
                        UIElements.Rectangle(Color.gray)
                            .frame(width: 14, height: 14)
                            .foregroundColor(foreground)
                            .cornerRadius(RectCorner.TopLeft, 2f);
                        UIElements.Circle(Color.red)
                            .frame(width: 14, height: 14)
                            .opacity(0.8f);
                        UIElements.Capsule(Color.green)
                            .frame(width: 28, height: 14)
                            .border(Color.black, 1f);
                        UIElements.RoundedRectangle(4f, Color.blue)
                            .frame(width: 28, height: 14)
                            .shadow(Color.black, 1f, 1f, 1f);
                    }, spacing: 3f);

                    UIElements.Image(sprite)
                        .resizable()
                        .scaledToFill()
                        .renderingMode(ImageRenderingMode.Template)
                        .tint(tint)
                        .frame(width: 32, height: 24)
                        .clipped();

                    UIElements.Label("Badge", UIElements.Circle(Color.yellow).frame(width: 12, height: 12))
                        .fontSize(11)
                        .foregroundColor(foreground)
                        .tint(tint);

                    UIElements.ProgressView(progress)
                        .tint(tint)
                        .cornerRadius(3f)
                        .frame(width: 120, height: 8);

                    UIElements.Slider(slider, 0f, 1f)
                        .tint(tint)
                        .frame(width: 140, height: 28);

                    UIElements.Picker(pickerSelection, "One", "Two")
                        .pickerStyle(PickerStyle.Automatic)
                        .foregroundColor(foreground)
                        .tint(tint)
                        .fontSize(12)
                        .frame(width: 160, height: 34);

                    UIElements.TextField("Email", fieldText, UIElements.Text("email"))
                        .textFieldStyle(TextFieldStyles.Chrome())
                        .focused(focused)
                        .onSubmit(value => submitted = value)
                        .selectAllOnFocus(false)
                        .textSelectionColor(selection)
                        .contentMargins(4, 6, 2, 3)
                        .textContentType(TMP_InputField.ContentType.EmailAddress)
                        .textInputLimit(32)
                        .caretColor(caret)
                        .caretWidth(3)
                        .caretBlinkRate(1.1f)
                        .keyboardType(TouchScreenKeyboardType.EmailAddress)
                        .lineLimit(1)
                        .multilineTextAlignment(TextAlignmentOptions.MidlineRight)
                        .frame(width: 240, height: 40);

                    UIElements.ScrollView(() =>
                    {
                        UIElements.HStack(() =>
                        {
                            UIElements.Text("A").frame(width: 80, height: 24);
                            UIElements.Text("B").frame(width: 80, height: 24);
                            UIElements.Text("C").frame(width: 80, height: 24);
                        }, spacing: 0f);
                    }, horizontal: true, vertical: true)
                    .scrollBounce(false)
                    .scrollSensitivity(23f)
                    .scrollMovementType(ScrollRect.MovementType.Clamped)
                    .scrollPositionY(scrollY)
                    .scrollPositionX(scrollX)
                    .scrollIndicators(ScrollIndicatorVisibility.Hidden,
                        UniftUIScrollAxis.Vertical | UniftUIScrollAxis.Horizontal)
                    .frame(width: 160, height: 50);
                }, spacing: 4f)
                .frame(width: 320, height: 360)
                .Build(canvas);

                yield return null;
                ForceLayout(canvas);

                TMP_InputField input = FindRect(canvas.transform, "TextField").GetComponent<TMP_InputField>();
                Assert.That(input.contentType, Is.EqualTo(TMP_InputField.ContentType.EmailAddress));
                Assert.That(input.characterLimit, Is.EqualTo(32));
                Assert.That(input.keyboardType, Is.EqualTo(TouchScreenKeyboardType.EmailAddress));
                Assert.That(input.caretWidth, Is.EqualTo(3));
                Assert.That(input.caretBlinkRate, Is.EqualTo(1.1f).Within(0.001f));
                AssertColor(input.selectionColor, selection.Value);
                Assert.That(input.textViewport.offsetMin.x, Is.EqualTo(4f).Within(0.001f));
                Assert.That(input.textViewport.offsetMax.x, Is.EqualTo(-6f).Within(0.001f));
                Assert.That(input.textViewport.offsetMin.y, Is.EqualTo(3f).Within(0.001f));
                Assert.That(input.textViewport.offsetMax.y, Is.EqualTo(-2f).Within(0.001f));

                input.onSubmit.Invoke("accepted");
                Assert.That(submitted, Is.EqualTo("accepted"));

                ScrollRect scroll = canvas.GetComponentInChildren<ScrollRect>();
                Assert.That(scroll.horizontal, Is.True);
                Assert.That(scroll.vertical, Is.True);
                Assert.That(scroll.movementType, Is.EqualTo(ScrollRect.MovementType.Clamped));
                Assert.That(scroll.scrollSensitivity, Is.EqualTo(23f).Within(0.001f));
                Assert.That(scroll.verticalScrollbar, Is.Null);
                Assert.That(scroll.horizontalScrollbar, Is.Null);
                Assert.That(scroll.GetComponent<UniftUIScrollRectBridge>(), Is.Not.Null);

                Assert.That(FindRect(canvas.transform, "PickerSegmented"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "ProgressView"), Is.Not.Null);
                Assert.That(FindSpriteImage(canvas, sprite), Is.Not.Null);
            }
            finally
            {
                Object.DestroyImmediate(canvas.gameObject);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        [UnityTest]
        public IEnumerator StaticFactories_CommonContainersAndControlsBuildTogether()
        {
            Canvas canvas = CreateCanvas();
            Texture2D texture = new Texture2D(2, 2);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            var selection = new State<int>(0);
            var toggle = new State<bool>(false);
            var text = new State<string>("text");
            var value = new State<int>(2);

            try
            {
                UIElements.VStack(() =>
                {
                    UIElements.Grid(() =>
                    {
                        UIElements.GridRow(() =>
                        {
                            UIElements.Text("A");
                            UIElements.Text("B");
                        });
                        UIElements.GridRow(() =>
                        {
                            UIElements.Rectangle(Color.red).frame(width: 24, height: 16);
                            UIElements.Image(sprite).frame(width: 24, height: 16);
                        });
                    }, horizontalSpacing: 2f, verticalSpacing: 2f)
                    .frame(width: 120, height: 70);

                    UIElements.GeometryReader(proxy =>
                        UIElements.Text(proxy.Size.x > 0f ? "geometry" : "empty"))
                        .frame(width: 120, height: 28);

                    UIElements.HStack(() =>
                    {
                        UIElements.ProgressView(new State<float>(0.5f)).frame(width: 80, height: 8);
                        UIElements.Stepper("Count", value, 0, 5);
                        UIElements.Picker(selection, "One", "Two");
                        UIElements.Toggle(toggle, UIElements.Text("Enabled"));
                    }, spacing: 4f);

                    UIElements.TextField("Title", text, Axis.Vertical, UIElements.Text("prompt"));
                    UIElements.SecureField("Password", text, UIElements.Text("secret"));
                    UIElements.TextEditor(text);

                    UIElements.TabView(() =>
                    {
                        UIElements.Tab("First", () => UIElements.Text("first"));
                        UIElements.Tab("Second", () => UIElements.Text("second"));
                    }, selection)
                    .WithTransitionDuration(0f)
                    .frame(width: 160, height: 100);
                }, spacing: 4f)
                .frame(width: 420, height: 520)
                .Build(canvas);

                yield return null;
                ForceLayout(canvas);

                Assert.That(FindRect(canvas.transform, "Grid"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "GeometryReader"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "ProgressView"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "Stepper"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "PickerSegmented"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "ToggleElement_Enabled"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "TextField"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "TextEditor"), Is.Not.Null);
                Assert.That(FindRect(canvas.transform, "TabView"), Is.Not.Null);
                Assert.That(ContainsText(canvas.transform, "geometry"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(canvas.gameObject);
                Object.DestroyImmediate(sprite);
                Object.DestroyImmediate(texture);
            }
        }

        [UnityTest]
        public IEnumerator ModifierChains_ReplaceContextChildWithoutLeavingOrphans()
        {
            Canvas canvas = CreateCanvas();

            try
            {
                UIElements.VStack(() =>
                {
                    UIElements.Text("content")
                        .padding(4)
                        .background(Color.white)
                        .overlay(UIElements.Text("badge"), ZStackAlignment.TopTrailing)
                        .border(Color.black, 1f)
                        .opacity(0.8f)
                        .frame(width: 120, height: 40);
                }, spacing: 0f)
                .Build(canvas);

                yield return null;
                ForceLayout(canvas);

                RectTransform stack = canvas.transform.GetChild(0).GetComponent<RectTransform>();
                Assert.That(stack.childCount, Is.EqualTo(1));
                Assert.That(canvas.GetComponentsInChildren<TMP_Text>(true).Length, Is.EqualTo(2));
                Assert.That(ContainsText(canvas.transform, "content"), Is.True);
                Assert.That(ContainsText(canvas.transform, "badge"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(canvas.gameObject);
            }
        }

        private static Canvas CreateCanvas()
        {
            var canvasObject = new GameObject("TestCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
            return canvas;
        }

        private static void ForceLayout(Canvas canvas)
        {
            Canvas.ForceUpdateCanvases();
            foreach (RectTransform rect in canvas.GetComponentsInChildren<RectTransform>(true))
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            Canvas.ForceUpdateCanvases();
        }

        private static RectTransform FindRect(Transform root, string name)
        {
            foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
                if (rect.name == name)
                    return rect;

            Assert.Fail("Could not find RectTransform named " + name);
            return null;
        }

        private static Image FindSpriteImage(Canvas canvas, Sprite sprite)
        {
            foreach (Image image in canvas.GetComponentsInChildren<Image>(true))
                if (image.sprite == sprite)
                    return image;

            return null;
        }

        private static bool ContainsText(Transform root, string value)
        {
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                if (text.text == value)
                    return true;

            return false;
        }

        private static void AssertColor(Color actual, Color expected)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(0.01f));
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(0.01f));
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(0.01f));
            Assert.That(actual.a, Is.EqualTo(expected.a).Within(0.01f));
        }
    }
}
