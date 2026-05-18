using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniftUI;

public sealed class HamburgerMenuExample : UniftView
{
    private static readonly string[] MenuTitles =
    {
        "Dashboard",
        "Messages",
        "Projects",
        "Settings"
    };

    private readonly State<bool> menuOpen = new State<bool>(false);
    private readonly State<int> selectedMenu = new State<int>(0);
    private readonly State<float> drawerX = new State<float>(-330f);
    private readonly State<float> dimOpacity = new State<float>(0f);
    private readonly State<float> cardScale = new State<float>(1f);

    private static readonly Color PageBackground = new Color(245 / 255f, 247 / 255f, 251 / 255f);
    private static readonly Color Ink = new Color(17 / 255f, 24 / 255f, 39 / 255f);
    private static readonly Color Muted = new Color(100 / 255f, 116 / 255f, 139 / 255f);
    private static readonly Color Accent = new Color(37 / 255f, 99 / 255f, 235 / 255f);
    private static readonly Color AccentSoft = new Color(219 / 255f, 234 / 255f, 254 / 255f);
    private static readonly Color Border = new Color(226 / 255f, 232 / 255f, 240 / 255f);

    private void Start()
    {
        PrepareHost();
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");
        UIContext.SetDefaultFont(defaultFont);
        Draw();
    }

    private void Draw()
    {
        ZStack(() =>
        {
            MainContent();

            Button(Rectangle(Color.black).opacity(dimOpacity).frame(infiniteWidth: true, infiniteHeight: true), CloseMenu)
                .buttonStyle(ButtonStyles.Plain(Color.clear))
                .allowsHitTesting(menuOpen)
                .frame(infiniteWidth: true, infiniteHeight: true);

            Drawer()
                .offset(drawerX, 0f)
                .animation(easeInOut(0.22f), drawerX)
                .shadow(new Color(15 / 255f, 23 / 255f, 42 / 255f, 0.18f), 18f, 8f, 0f);
        }, null, ZStackAlignment.TopLeading)
        .frame(infiniteWidth: true, infiniteHeight: true)
        .background(PageBackground)
        .Build(GetComponent<Canvas>());
    }

    private void MainContent()
    {
        VStack(() =>
        {
            Header();

            ScrollView(() =>
            {
                VStack(() =>
                {
                    HeroCard();
                    StatusGrid();
                    ActivityList();
                }, null, 14f, VStackAlignment.Leading)
                .padding(18)
                .frame(infiniteWidth: true);
            }, null, false, true)
            .scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical)
            .frame(infiniteWidth: true, infiniteHeight: true);
        }, null, 0f, VStackAlignment.Leading)
        .frame(infiniteWidth: true, infiniteHeight: true);
    }

    private void Header()
    {
        HStack(() =>
        {
            Button(MenuIcon(), ToggleMenu)
                .buttonStyle(ButtonStyles.Plain(Ink))
                .frame(width: 44f, height: 44f)
                .background(Color.white)
                .cornerRadius(10f)
                .shadow(new Color(15 / 255f, 23 / 255f, 42 / 255f, 0.08f), 8f, 0f, 2f);

            VStack(() =>
            {
                Text("Hamburger Menu").bold().fontSize(17f).foregroundColor(Ink);
                Text(() => MenuTitles[selectedMenu.Value], new State[] { selectedMenu })
                    .fontSize(12f)
                    .foregroundColor(Muted);
            }, null, 2f, VStackAlignment.Leading);

            Spacer();

            Text("UniftUI")
                .bold()
                .fontSize(13f)
                .foregroundColor(Accent)
                .padding(top: 8f, bottom: 8f, left: 12f, right: 12f)
                .background(AccentSoft)
                .cornerRadius(999f);
        }, null, 12f, HStackAlignment.Center)
        .padding(top: 12f, bottom: 12f, left: 16f, right: 16f)
        .background(Color.white)
        .frame(infiniteWidth: true, height: 68f);
    }

    private UIElement Drawer()
    {
        return VStack(() =>
        {
            HStack(() =>
            {
                VStack(() =>
                {
                    Text("Menu").bold().fontSize(23f).foregroundColor(Ink);
                    Text("State-driven drawer").fontSize(12f).foregroundColor(Muted);
                }, null, 2f, VStackAlignment.Leading);

                Spacer();

                Button(CloseIcon(), CloseMenu)
                    .buttonStyle(ButtonStyles.Plain(Ink))
                    .frame(width: 38f, height: 38f)
                    .background(new Color(241 / 255f, 245 / 255f, 249 / 255f))
                    .cornerRadius(9f);
            }, null, 10f, HStackAlignment.Center)
            .padding(top: 18f, bottom: 16f, left: 18f, right: 18f)
            .frame(infiniteWidth: true);

            Divider(Border, 1f);

            VStack(() =>
            {
                for (int i = 0; i < MenuTitles.Length; i++)
                    DrawerRow(i);
            }, null, 8f, VStackAlignment.Leading)
            .padding(14)
            .frame(infiniteWidth: true);

            Spacer();

            VStack(() =>
            {
                Text("Example states").bold().fontSize(12f).foregroundColor(Muted);
                Text(() => "menuOpen: " + menuOpen.Value, new State[] { menuOpen }).fontSize(12f).foregroundColor(Muted);
                Text(() => "selected: " + MenuTitles[selectedMenu.Value], new State[] { selectedMenu }).fontSize(12f).foregroundColor(Muted);
            }, null, 5f, VStackAlignment.Leading)
            .padding(14)
            .background(new Color(248 / 255f, 250 / 255f, 252 / 255f))
            .cornerRadius(10f)
            .padding(left: 14f, right: 14f, bottom: 16f);
        }, new State[] { selectedMenu }, 0f, VStackAlignment.Leading)
        .frame(width: 310f, infiniteHeight: true)
        .background(Color.white);
    }

    private void DrawerRow(int index)
    {
        bool selected = selectedMenu.Value == index;
        Button(HStack(() =>
        {
            RoundedRectangle(6f, selected ? Accent : new Color(203 / 255f, 213 / 255f, 225 / 255f))
                .frame(width: 8f, height: 28f);

            Text(MenuTitles[index])
                .bold()
                .fontSize(14f)
                .foregroundColor(selected ? Accent : Ink);

            Spacer();

            Text(index == 0 ? "24" : index == 1 ? "8" : index == 2 ? "12" : "")
                .fontSize(12f)
                .foregroundColor(selected ? Accent : Muted);
        }, null, 10f, HStackAlignment.Center), () => SelectMenu(index))
        .buttonStyle(ButtonStyles.Plain(selected ? Accent : Ink))
        .padding(top: 10f, bottom: 10f, left: 10f, right: 10f)
        .frame(infiniteWidth: true, height: 50f)
        .background(selected ? AccentSoft : Color.white)
        .cornerRadius(10f);
    }

    private void HeroCard()
    {
        VStack(() =>
        {
            Text(() => MenuTitles[selectedMenu.Value], new State[] { selectedMenu })
                .bold()
                .fontSize(26f)
                .foregroundColor(Color.white);

            Text("This page demonstrates a slide-out hamburger menu built from regular UniftUI components and modifiers.")
                .fontSize(14f)
                .foregroundColor(new Color(1f, 1f, 1f, 0.82f))
                .lineLimit(3);

            HStack(() =>
            {
                Button("Open Menu", OpenMenu)
                    .buttonStyle(ButtonStyles.Filled(Color.white, Accent, 10f))
                    .frame(width: 130f, height: 40f);

                Button("Next Section", NextMenu)
                    .buttonStyle(ButtonStyles.Plain(Color.white))
                    .foregroundColor(Color.black)
                    .frame(width: 120f, height: 40f)
                    .border(new Color(1f, 1f, 1f, 0.38f), 1f)
                    .cornerRadius(10f);
            }, null, 10f, HStackAlignment.Center);
        }, null, 12f, VStackAlignment.Leading)
        .padding(20)
        .frame(infiniteWidth: true, height: 210f)
        .background(Accent)
        .cornerRadius(14f)
        .scaleEffect(cardScale)
        .animation(spring(0.45f, 0.78f), cardScale);
    }

    private void StatusGrid()
    {
        HStack(() =>
        {
            StatCard("Open", "12", new Color(220 / 255f, 252 / 255f, 231 / 255f), new Color(22 / 255f, 101 / 255f, 52 / 255f));
            StatCard("Review", "7", new Color(254 / 255f, 249 / 255f, 195 / 255f), new Color(133 / 255f, 77 / 255f, 14 / 255f));
            StatCard("Done", "31", AccentSoft, Accent);
        }, null, 12f, HStackAlignment.Center)
        .frame(infiniteWidth: true);
    }

    private void StatCard(string title, string value, Color background, Color color)
    {
        VStack(() =>
        {
            Text(title).fontSize(12f).foregroundColor(Muted);
            Text(value).bold().fontSize(24f).foregroundColor(color);
        }, null, 3f, VStackAlignment.Leading)
        .padding(14)
        .frame(infiniteWidth: true, height: 92f)
        .background(background)
        .cornerRadius(12f);
    }

    private void ActivityList()
    {
        VStack(() =>
        {
            Text("Recent activity").bold().fontSize(18f).foregroundColor(Ink);
            ActivityRow("Design tokens updated", "2 min ago");
            ActivityRow("Navigation sample added", "18 min ago");
            ActivityRow("Docs reviewed", "Today");
        }, null, 8f, VStackAlignment.Leading)
        .padding(16)
        .frame(infiniteWidth: true)
        .background(Color.white)
        .cornerRadius(12f);
    }

    private void ActivityRow(string title, string time)
    {
        HStack(() =>
        {
            Circle(AccentSoft).frame(width: 12f, height: 12f);
            Text(title).fontSize(14f).foregroundColor(Ink);
            Spacer();
            Text(time).fontSize(12f).foregroundColor(Muted);
        }, null, 10f, HStackAlignment.Center)
        .padding(top: 7f, bottom: 7f)
        .frame(infiniteWidth: true);
    }

    private UIElement MenuIcon()
    {
        return VStack(() =>
        {
            IconLine(22f);
            IconLine(16f);
            IconLine(22f);
        }, null, 4f, VStackAlignment.Center);
    }

    private UIElement CloseIcon()
    {
        return ZStack(() =>
        {
            IconLine(20f).rotationEffect(45f);
            IconLine(20f).rotationEffect(-45f);
        });
    }

    private UIElement IconLine(float width)
    {
        return RoundedRectangle(2f, Ink).frame(width: width, height: 2f);
    }

    private void ToggleMenu()
    {
        if (menuOpen.Value)
            CloseMenu();
        else
            OpenMenu();
    }

    private void OpenMenu()
    {
        WithAnimation(easeInOut(0.22f), () =>
        {
            menuOpen.Value = true;
            drawerX.Value = 0f;
            dimOpacity.Value = 0.38f;
            cardScale.Value = 0.985f;
        });
    }

    private void CloseMenu()
    {
        WithAnimation(easeInOut(0.22f), () =>
        {
            menuOpen.Value = false;
            drawerX.Value = -330f;
            dimOpacity.Value = 0f;
            cardScale.Value = 1f;
        });
    }

    private void SelectMenu(int index)
    {
        WithAnimation(easeInOut(0.22f), () =>
        {
            selectedMenu.Value = index;
            menuOpen.Value = false;
            drawerX.Value = -330f;
            dimOpacity.Value = 0f;
            cardScale.Value = 1f;
        });
    }

    private void NextMenu()
    {
        WithAnimation(spring(0.42f, 0.82f), () =>
        {
            selectedMenu.Value = (selectedMenu.Value + 1) % MenuTitles.Length;
            cardScale.Value = cardScale.Value < 1f ? 1f : 0.975f;
        });
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
}
