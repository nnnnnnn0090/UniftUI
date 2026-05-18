using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniftUI;

public sealed class GameUIExample : UniftView
{
    private readonly State<float> health = new State<float>(0.82f);
    private readonly State<float> mana = new State<float>(0.56f);
    private readonly State<float> stamina = new State<float>(0.68f);
    private readonly State<int> gold = new State<int>(1280);
    private readonly State<int> potionCount = new State<int>(3);
    private readonly State<int> selectedSlot = new State<int>(0);
    private readonly State<int> wave = new State<int>(4);
    private readonly State<float> questProgress = new State<float>(4f / 6f);
    private readonly State<string> combatLog = new State<string>("Entered the ruins.");
    private readonly State<float> pulseScale = new State<float>(1f);
    private readonly State<float> idleScale = new State<float>(1f);

    private static readonly Color ScreenDark = new Color(7 / 255f, 10 / 255f, 18 / 255f);
    private static readonly Color Panel = new Color(14 / 255f, 20 / 255f, 34 / 255f, 0.92f);
    private static readonly Color PanelSoft = new Color(22 / 255f, 31 / 255f, 50 / 255f, 0.92f);
    private static readonly Color Stroke = new Color(77 / 255f, 91 / 255f, 123 / 255f, 0.55f);
    private static readonly Color TextMain = new Color(238 / 255f, 242 / 255f, 255 / 255f);
    private static readonly Color TextMuted = new Color(148 / 255f, 163 / 255f, 184 / 255f);
    private static readonly Color HealthColor = new Color(239 / 255f, 68 / 255f, 68 / 255f);
    private static readonly Color ManaColor = new Color(59 / 255f, 130 / 255f, 246 / 255f);
    private static readonly Color StaminaColor = new Color(34 / 255f, 197 / 255f, 94 / 255f);
    private static readonly Color GoldColor = new Color(250 / 255f, 204 / 255f, 21 / 255f);
    private static readonly Color SkillColor = new Color(168 / 255f, 85 / 255f, 247 / 255f);

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
            BackgroundScene();

            VStack(() =>
            {
                TopHud();
                Spacer();
                BottomHud();
            }, null, 0f, VStackAlignment.Leading)
            .padding(18)
            .frame(infiniteWidth: true, infiniteHeight: true);
        }, null, ZStackAlignment.Center)
        .frame(infiniteWidth: true, infiniteHeight: true)
        .background(ScreenDark)
        .Build(GetComponent<Canvas>());
    }

    private void BackgroundScene()
    {
        ZStack(() =>
        {
            Rectangle(new Color(10 / 255f, 14 / 255f, 26 / 255f))
                .frame(infiniteWidth: true, infiniteHeight: true);

            Circle(new Color(37 / 255f, 99 / 255f, 235 / 255f, 0.22f))
                .frame(width: 420f, height: 420f)
                .offset(-280f, -120f);

            Circle(new Color(168 / 255f, 85 / 255f, 247 / 255f, 0.16f))
                .frame(width: 360f, height: 360f)
                .offset(330f, 160f);
        }, null, ZStackAlignment.Center)
        .frame(infiniteWidth: true, infiniteHeight: true);
    }

    private void TopHud()
    {
        HStack(() =>
        {
            PlayerPanel();
            Spacer();
            QuestPanel();
            MiniMapPanel();
        }, null, 14f, HStackAlignment.Top)
        .frame(infiniteWidth: true);
    }

    private void PlayerPanel()
    {
        HStack(() =>
        {
            ZStack(() =>
            {
                Circle(new Color(30 / 255f, 41 / 255f, 59 / 255f)).frame(width: 68f, height: 68f);
                Text("Lv\n18")
                    .bold()
                    .fontSize(17f)
                    .foregroundColor(TextMain)
                    .multilineTextAlignment(TextAlignmentOptions.Center);
            });

            VStack(() =>
            {
                HStack(() =>
                {
                    Text("Astra").bold().fontSize(19f).foregroundColor(TextMain);
                    Text("Ranger").fontSize(12f).foregroundColor(TextMuted);
                }, null, 8f, HStackAlignment.Center);

                ResourceBar("HP", health, HealthColor);
                ResourceBar("MP", mana, ManaColor);
                ResourceBar("ST", stamina, StaminaColor);
            }, null, 7f, VStackAlignment.Leading);
        }, null, 12f, HStackAlignment.Center)
        .padding(14)
        .frame(width: 360f)
        .background(Panel)
        .border(Stroke, 1f)
        .cornerRadius(14f)
        .shadow(new Color(0f, 0f, 0f, 0.35f), 14f, 0f, 4f);
    }

    private void ResourceBar(string label, State<float> value, Color fill)
    {
        HStack(() =>
        {
            Text(label).bold().fontSize(11f).foregroundColor(TextMuted).frame(width: 24f);

            ProgressView(value, 1f)
                .tint(fill)
                .background(new Color(15 / 255f, 23 / 255f, 42 / 255f))
                .cornerRadius(999f)
                .frame(width: 230f, height: 10f);

            Text(() => Mathf.RoundToInt(value.Value * 100f).ToString(), new State[] { value })
                .fontSize(11f)
                .foregroundColor(TextMuted)
                .frame(width: 28f);
        }, null, 7f, HStackAlignment.Center);
    }

    private void QuestPanel()
    {
        VStack(() =>
        {
            Text("Objective").bold().fontSize(14f).foregroundColor(TextMain);
            Text(() => "Survive wave " + wave.Value + " / 6", new State[] { wave })
                .fontSize(13f)
                .foregroundColor(TextMuted);
            ProgressView(questProgress, 1f)
                .tint(GoldColor)
                .cornerRadius(999f)
                .frame(infiniteWidth: true, height: 8f);
        }, new State[] { wave, questProgress }, 8f, VStackAlignment.Leading)
        .padding(14)
        .frame(width: 230f)
        .background(Panel)
        .border(Stroke, 1f)
        .cornerRadius(14f);
    }

    private void MiniMapPanel()
    {
        ZStack(() =>
        {
            RoundedRectangle(12f, new Color(15 / 255f, 23 / 255f, 42 / 255f))
                .frame(width: 142f, height: 142f);

            Circle(new Color(59 / 255f, 130 / 255f, 246 / 255f, 0.3f))
                .frame(width: 96f, height: 96f);

            Circle(GoldColor).frame(width: 10f, height: 10f).offset(28f, -18f);
            Circle(HealthColor).frame(width: 9f, height: 9f).offset(-30f, 24f);
            TriangleMarker().offset(3f, 2f).rotationEffect(35f);

            Text("N")
                .bold()
                .fontSize(12f)
                .foregroundColor(TextMuted)
                .offset(0f, -54f);
        }, null, ZStackAlignment.Center)
        .padding(8)
        .background(Panel)
        .border(Stroke, 1f)
        .cornerRadius(16f);
    }

    private void BottomHud()
    {
        HStack(() =>
        {
            CombatLogPanel();
            Spacer();
            VStack(() =>
            {
                CurrencyPanel();
                ActionBar();
            }, null, 10f, VStackAlignment.Trailing);
        }, null, 14f, HStackAlignment.Bottom)
        .frame(infiniteWidth: true);
    }

    private void CombatLogPanel()
    {
        VStack(() =>
        {
            Text("Combat Log").bold().fontSize(13f).foregroundColor(TextMuted);
            Text(combatLog)
                .fontSize(14f)
                .foregroundColor(TextMain)
                .lineLimit(2);
        }, null, 6f, VStackAlignment.Leading)
        .padding(14)
        .frame(width: 330f, height: 92f)
        .background(Panel)
        .border(Stroke, 1f)
        .cornerRadius(14f);
    }

    private void CurrencyPanel()
    {
        HStack(() =>
        {
            Text("Gold").fontSize(12f).foregroundColor(TextMuted);
            Text(() => gold.Value.ToString(), new State[] { gold })
                .bold()
                .fontSize(16f)
                .foregroundColor(GoldColor);
        }, null, 8f, HStackAlignment.Center)
        .padding(top: 8f, bottom: 8f, left: 12f, right: 12f)
        .background(Panel)
        .border(Stroke, 1f)
        .cornerRadius(999f);
    }

    private void ActionBar()
    {
        HStack(() =>
        {
            ActionSlot(0, "Atk", "1", HealthColor, Attack);
            ActionSlot(1, "Heal", potionCount.Value.ToString(), StaminaColor, Heal);
            ActionSlot(2, "Blink", "3", ManaColor, Cast);
            ActionSlot(3, "Ult", "R", SkillColor, Ultimate);
        }, new State[] { selectedSlot, potionCount }, 10f, HStackAlignment.Center)
        .padding(12)
        .background(PanelSoft)
        .border(Stroke, 1f)
        .cornerRadius(18f);
    }

    private void ActionSlot(int index, string title, string key, Color color, System.Action action)
    {
        bool selected = selectedSlot.Value == index;
        Button(VStack(() =>
        {
            ZStack(() =>
            {
                RoundedRectangle(12f, selected ? color : new Color(30 / 255f, 41 / 255f, 59 / 255f))
                    .frame(width: 64f, height: 54f);
                Text(key).bold().fontSize(20f).foregroundColor(TextMain);
            })
            .scaleEffect(selected ? pulseScale : idleScale)
            .animation(spring(0.34f, 0.7f), pulseScale);

            Text(title)
                .fontSize(11f)
                .foregroundColor(selected ? TextMain : TextMuted);
        }, null, 5f, VStackAlignment.Center), action)
        .buttonStyle(ButtonStyles.Plain(TextMain))
        .frame(width: 78f, height: 88f)
        .background(selected ? new Color(color.r, color.g, color.b, 0.16f) : Color.clear)
        .cornerRadius(14f);
    }

    private UIElement TriangleMarker()
    {
        return VStack(() =>
        {
            RoundedRectangle(2f, TextMain).frame(width: 10f, height: 22f);
            RoundedRectangle(2f, TextMain).frame(width: 22f, height: 6f);
        }, null, 0f, VStackAlignment.Center)
        .scaleEffect(0.72f);
    }

    private void Attack()
    {
        WithAnimation(easeOut(0.18f), () =>
        {
            selectedSlot.Value = 0;
            stamina.Value = Mathf.Clamp01(stamina.Value - 0.12f);
            gold.Value += 12;
            combatLog.Value = "Quick shot landed. +12 gold.";
            pulseScale.Value = pulseScale.Value > 1f ? 1f : 1.08f;
        });
    }

    private void Heal()
    {
        if (potionCount.Value <= 0)
        {
            combatLog.Value = "No potions left.";
            selectedSlot.Value = 1;
            return;
        }

        WithAnimation(easeOut(0.18f), () =>
        {
            selectedSlot.Value = 1;
            potionCount.Value--;
            health.Value = Mathf.Clamp01(health.Value + 0.22f);
            combatLog.Value = "Potion used. HP restored.";
            pulseScale.Value = pulseScale.Value > 1f ? 1f : 1.08f;
        });
    }

    private void Cast()
    {
        WithAnimation(easeOut(0.18f), () =>
        {
            selectedSlot.Value = 2;
            mana.Value = Mathf.Clamp01(mana.Value - 0.18f);
            combatLog.Value = "Blink ready. Mana consumed.";
            pulseScale.Value = pulseScale.Value > 1f ? 1f : 1.08f;
        });
    }

    private void Ultimate()
    {
        WithAnimation(spring(0.42f, 0.78f), () =>
        {
            selectedSlot.Value = 3;
            mana.Value = Mathf.Clamp01(mana.Value - 0.3f);
            stamina.Value = Mathf.Clamp01(stamina.Value - 0.24f);
            wave.Value = Mathf.Min(6, wave.Value + 1);
            questProgress.Value = wave.Value / 6f;
            combatLog.Value = "Ultimate fired. Wave advanced.";
            pulseScale.Value = pulseScale.Value > 1f ? 1f : 1.12f;
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
        scaler.referenceResolution = new Vector2(1280f, 720f);

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
