const languageStorageKey = "uniftui-docs-language";
const themeStorageKey = "uniftui-docs-theme";
const themeChoices = ["system", "light", "dark"];

function escapeHtml(value) {
  return String(value)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;");
}

function code(value) {
  return `<pre><code>${escapeHtml(value.trim())}</code></pre>`;
}

function api(value) {
  return `<code>${escapeHtml(value)}</code>`;
}

function table(headers, rows) {
  return `<table class="api-table">
    <thead><tr>${headers.map(header => `<th>${escapeHtml(header)}</th>`).join("")}</tr></thead>
    <tbody>${rows.map(row => `<tr>${row.map(cell => `<td>${cell}</td>`).join("")}</tr>`).join("")}</tbody>
  </table>`;
}

function cards(items) {
  return `<div class="topics">${items.map(item => `
    <div class="topic-card">
      <div><span class="signature">${escapeHtml(item.kicker)}</span>${item.href ? `<a href="${item.href}">${escapeHtml(item.title)}</a>` : `<strong>${escapeHtml(item.title)}</strong>`}</div>
      <p>${item.body}</p>
    </div>
  `).join("")}</div>`;
}

function steps(items) {
  return `<div class="steps">${items.map(item => `<div class="step"><strong>${item.title}</strong><br>${item.body}</div>`).join("")}</div>`;
}

function callout(body) {
  return `<div class="callout"><p>${body}</p></div>`;
}

function referenceTable(lang, rows) {
  return table(
    lang === "ja" ? ["名前", "用途", "よく使う場面"] : ["Name", "Purpose", "Use it for"],
    rows
  );
}

const sharedReference = {
  factories: [
    ["Text", "Display text", "Labels, headings, reactive strings"],
    ["Button", "Run an action", "Primary actions, toolbar actions"],
    ["TextField / SecureField / TextEditor", "Text input", "Forms, search, notes, passwords"],
    ["Toggle / Slider / Stepper / Picker", "State-bound controls", "Settings and simple configuration"],
    ["ProgressView", "Show progress", "Loading and completion"],
    ["Image / Label / Divider", "Media and utility views", "Icons, status rows, separators"],
    ["Rectangle / Circle / Capsule / RoundedRectangle", "Shapes", "Backgrounds, badges, masks"],
    ["VStack / HStack / ZStack / Spacer", "Basic layout", "Rows, columns, layered UI"],
    ["LazyVStack / LazyHStack / Grid / GridRow", "Larger layout", "Lists and table-like content"],
    ["ScrollView / TabView / Tab", "Containers", "Scrollable pages and tab screens"],
    ["GeometryReader", "Read parent size", "Responsive layouts"],
    ["ForEach", "Repeat content", "Rows and generated controls"]
  ],
  modifiers: [
    ["frame", "Size proposal", "Fixed size, full width, max/min constraints"],
    ["padding", "Inner spacing", "Cards, buttons, rows"],
    ["background / overlay", "Layer views", "Fills, badges, custom chrome"],
    ["foregroundColor / tint / opacity", "Color and alpha", "Text, controls, template images"],
    ["font / fontSize / bold / italic", "Text styling", "Headings, labels, prompts"],
    ["lineLimit / multilineTextAlignment", "Text wrapping", "Descriptions and multiline input"],
    ["cornerRadius / clipped / clipShape", "Rounded and masked shapes", "Cards, avatars, image crops"],
    ["border / shadow", "Decoration", "Depth and outlines"],
    ["offset / position", "Move views", "Badges, free placement, animated movement"],
    ["rotationEffect / scaleEffect", "Transforms", "Feedback and animation"],
    ["disabled / hidden / allowsHitTesting", "Interaction state", "Loading, overlays, conditional UI"],
    ["onAppear / onChange / update", "Lifecycle hooks", "Start work, observe values, per-frame updates"],
    ["animation / WithAnimation", "Animate changes", "Transitions, progress, feedback"]
  ],
  input: [
    ["focused", "Bind focus", "Validation, keyboard control"],
    ["onEditingChanged / onSubmit", "Input events", "Search, login, submit flows"],
    ["selectAllOnFocus", "Selection behavior", "Numeric fields and search boxes"],
    ["contentMargins", "TextField inner spacing", "Reusable field chrome"],
    ["caretColor / caretWidth / caretBlinkRate", "Caret styling", "Brand polish"],
    ["textSelectionColor", "Selection color", "Readable highlight"],
    ["textContentType / textInputLimit / keyboardType", "Input rules", "Email, password, mobile keyboard"]
  ],
  types: [
    ["State<T>", "Observable value", "Reactive UI"],
    ["Animation", "Animation definition", "Timing, easing, springs"],
    ["Axis / Edge / RectCorner", "Layout options", "TextField axis, padding edges, rounded corners"],
    ["VStackAlignment / HStackAlignment / ZStackAlignment", "Alignment", "Stack positioning"],
    ["PickerStyle / ImageRenderingMode / ImageResizingMode", "Control and image options", "Segmented picker, template tint, tiling"],
    ["ScrollIndicatorVisibility / UniftUIScrollAxis", "Scroll options", "Vertical/horizontal scroll chrome"],
    ["ButtonStyles / TextFieldStyles", "Built-in style factories", "Consistent controls"],
    ["GeometryProxy", "GeometryReader value", "Responsive layout decisions"]
  ]
};

const docs = {
  ja: {
    chrome: {
      search: "検索",
      toc: "このページ",
      noResults: "該当するページがありません。",
      docsTitle: "ドキュメント",
      menu: "メニュー",
      theme: "テーマ",
      themeSystem: "システム",
      themeLight: "ライト",
      themeDark: "ダーク"
    },
    groups: [
      {
        title: "はじめに",
        pages: [
          {
            id: "overview",
            title: "UniftUIの全体像",
            kind: "Guide",
            summary: "UnityのCanvas UIを、C#だけで宣言的に組み立てるためのライブラリです。まず何を覚えればよいか、どこまでできるかを掴みます。",
            keywords: "overview start beginner Unity Canvas uGUI State modifier",
            sections: [
              {
                title: "何ができるか",
                body: `
                  <p>UniftUIは、<code>Text</code>、<code>Button</code>、<code>VStack</code> などの要素をC#で組み合わせ、UnityのuGUIオブジェクトを生成します。値の変化は <code>State&lt;T&gt;</code> で扱い、見た目やレイアウトは modifier をチェーンして指定します。</p>
                  ${cards([
                    { kicker: "1", title: "Factory", body: "Text、Button、VStack のような関数でUI要素を作ります。", href: "#/api-index" },
                    { kicker: "2", title: "State", body: "変わる値を State<T> に入れると、UIが追従します。", href: "#/state" },
                    { kicker: "3", title: "Modifier", body: "padding、background、fontSize などで見た目を積み重ねます。", href: "#/modifiers" }
                  ])}
                `
              },
              {
                title: "最小の実用例",
                body: code(`
using UniftUI;
using UnityEngine;

public sealed class CounterView : UniftView
{
    private readonly State<int> count = new State<int>(0);

    private void Start()
    {
        VStack(() =>
        {
            Text(() => $"Count: {count.Value}", new State[] { count })
                .fontSize(28)
                .bold();

            Button("Increment", () => count.Value++)
                .padding(12)
                .background(new Color(0.15f, 0.38f, 0.9f))
                .foregroundColor(Color.white)
                .cornerRadius(10);
        }, spacing: 12f)
        .padding(24)
        .Build(GetComponent<Canvas>());
    }
}`)
              },
              {
                title: "読む順番",
                body: steps([
                  { title: "セットアップ", body: "Canvasに表示するところまでを確認します。" },
                  { title: "State", body: "値が変わるUIの作り方を覚えます。" },
                  { title: "Layout", body: "Stack、Spacer、frame、paddingで画面を組みます。" },
                  { title: "Controls", body: "Button、TextField、PickerなどをStateに接続します。" },
                  { title: "Reference", body: "迷ったときはAPI一覧とmodifier引数表を引きます。" }
                ])
              },
              {
                title: "向いている用途",
                body: `
                  ${cards([
                    { kicker: "UI", title: "ゲーム内メニュー", body: "設定、ステータス、インベントリ、デバッグUIのようなCanvas UI。" },
                    { kicker: "TOOLS", title: "社内/開発用ツール", body: "Unity上で動く操作パネル、パラメータ編集、プレビュー画面。" },
                    { kicker: "FAST", title: "プロトタイピング", body: "Prefabを作り込む前に、C#で画面構造を素早く試せます。" }
                  ])}
                `
              }
            ],
            related: ["setup", "state", "layout"]
          },
          {
            id: "setup",
            title: "インストールと表示",
            kind: "Guide",
            summary: "Package Managerで導入し、Canvasに最初のViewを表示します。",
            keywords: "install setup Package Manager Canvas Build TMP font",
            sections: [
              {
                title: "Package Manager",
                body: `
                  <p>UnityのPackage Managerで <strong>Add package from git URL</strong> を選び、次を入力します。</p>
                  ${code("https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI")}
                  ${callout("タグを固定したい場合は末尾に <code>#v0.1.0</code> のように付けます。")}
                `
              },
              {
                title: "Sceneの準備",
                body: steps([
                  { title: "Canvasを作る", body: "GameObjectにCanvasを追加します。" },
                  { title: "UniftViewを継承する", body: "Canvasと同じGameObjectにスクリプトを付けます。" },
                  { title: "Buildする", body: "<code>.Build(GetComponent&lt;Canvas&gt;())</code> でUIを生成します。" }
                ])
              },
              {
                title: "テンプレート",
                body: code(`
using UniftUI;
using UnityEngine;

public sealed class MainMenuView : UniftView
{
    private void Start()
    {
        VStack(() =>
        {
            Text("Main Menu").fontSize(32).bold();
            Button("Start", StartGame);
            Button("Options", OpenOptions);
        }, spacing: 12f)
        .frame(infiniteWidth: true, infiniteHeight: true)
        .Build(GetComponent<Canvas>());
    }
}`)
              },
              {
                title: "表示されないとき",
                body: `
                  <ul>
                    <li>スクリプトがCanvasと同じGameObjectに付いているか確認します。</li>
                    <li><code>Build</code> が呼ばれているか確認します。</li>
                    <li>Consoleに例外が出ていないか確認します。</li>
                    <li>文字が出ない場合はTextMesh Proのフォント設定を確認します。</li>
                  </ul>
                `
              }
            ],
            related: ["overview", "state", "text"]
          }
        ]
      },
      {
        title: "基本",
        pages: [
          {
            id: "state",
            title: "Stateと更新",
            kind: "Basics",
            summary: "変化する値をStateに置き、Text、入力、コントロール、アニメーションへ接続します。",
            declaration: "public class State<T> : State",
            keywords: "State Value reactive onChange BatchUpdate binding update",
            sections: [
              {
                title: "Stateの考え方",
                body: `
                  <p><code>State&lt;T&gt;</code> は監視できる値です。<code>Value</code> を変更すると、依存しているUIが更新されます。</p>
                  ${code(`
private readonly State<int> score = new State<int>(0);

Text(() => $"Score: {score.Value}", new State[] { score });
Button("+10", () => score.Value += 10);`)}
                `
              },
              {
                title: "TextFieldと双方向更新",
                body: code(`
private readonly State<string> playerName = new State<string>("");

TextField("Name", text: playerName, prompt: Text("Player name"));
Text(() => $"Hello, {playerName.Value}", new State[] { playerName });`)
              },
              {
                title: "変更時に処理する",
                body: code(`
Slider(volume, 0, 100)
    .onChange(volume, value =>
    {
        AudioListener.volume = value / 100f;
    });`)
              },
              {
                title: "まとめて変更する",
                body: code(`
using (State.BatchUpdate())
{
    hp.Value = maxHp.Value;
    mana.Value = maxMana.Value;
    selectedTab.Value = 0;
}`)
              }
            ],
            related: ["controls", "text-input", "animation"]
          },
          {
            id: "layout",
            title: "レイアウト",
            kind: "Basics",
            summary: "VStack、HStack、ZStack、Spacer、frame、padding、Gridで画面を組みます。",
            keywords: "layout VStack HStack ZStack Spacer frame padding fixedSize Grid LazyVStack GeometryReader",
            sections: [
              {
                title: "Stackで並べる",
                body: code(`
VStack(() =>
{
    Text("Profile").fontSize(24).bold();

    HStack(() =>
    {
        Text("Level");
        Spacer();
        Text("12").bold();
    });
}, spacing: 12f, alignment: VStackAlignment.Leading);`)
              },
              {
                title: "横いっぱいのカード",
                body: code(`
Text("Quest accepted")
    .padding(16)
    .frame(infiniteWidth: true)
    .background(new Color(0.95f, 0.97f, 1f))
    .cornerRadius(12);`)
              },
              {
                title: "リストと繰り返し",
                body: code(`
ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, quests.Count, i =>
        {
            Text(quests[i].Title)
                .padding(12)
                .frame(infiniteWidth: true);
        });
    }, spacing: 4f);
});`)
              },
              {
                title: "Grid",
                body: code(`
Grid(() =>
{
    GridRow(() => { Text("Stat").bold(); Text("Value").bold(); });
    GridRow(() => { Text("HP"); Text("120"); });
    GridRow(() => { Text("Attack"); Text("34"); });
}, horizontalSpacing: 16f, verticalSpacing: 8f);`)
              },
              {
                title: "GeometryReader",
                body: code(`
GeometryReader(proxy =>
{
    return Text($"Size: {proxy.Size.x:0} x {proxy.Size.y:0}")
        .frame(infiniteWidth: true, infiniteHeight: true);
})
.frame(infiniteWidth: true, height: 80);`)
              }
            ],
            related: ["modifiers", "scroll-tabs", "api-index"]
          },
          {
            id: "text",
            title: "Textとフォント",
            kind: "Basics",
            summary: "固定文字列、State、計算文字列を表示し、TextMesh Proのフォントや行数を指定します。",
            declaration: "Text(string text)\nText(State<string> text)\nText(Func<string> content, State[] dependencyStates)",
            keywords: "Text font fontSize TMP_FontAsset bold italic lineLimit multilineTextAlignment foregroundColor",
            sections: [
              {
                title: "3つのText",
                body: code(`
Text("Static");
Text(titleState);
Text(() => $"HP: {hp.Value}", new State[] { hp });`)
              },
              {
                title: "見た目",
                body: code(`
Text("Warning")
    .fontSize(20)
    .bold()
    .foregroundColor(new Color(0.9f, 0.18f, 0.12f));`)
              },
              {
                title: "フォント",
                body: code(`
UIContext.SetDefaultFont(defaultFont);

Text("Custom font")
    .font(titleFont)
    .fontSize(28);`)
              },
              {
                title: "折り返し",
                body: code(`
Text("Long description that can wrap onto multiple lines.")
    .lineLimit(2)
    .multilineTextAlignment(TextAlignmentOptions.TopLeft)
    .frame(width: 240);`)
              }
            ],
            related: ["text-input", "modifiers", "api-index"]
          },
          {
            id: "controls",
            title: "ButtonとControls",
            kind: "Basics",
            summary: "操作できるUIを作り、Stateに接続します。",
            keywords: "Button Toggle Slider Stepper ProgressView Picker buttonStyle pickerStyle controls",
            sections: [
              {
                title: "Button",
                body: code(`
Button("Save", Save)
    .frame(infiniteWidth: true, height: 44)
    .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));`)
              },
              {
                title: "カスタムラベル",
                body: code(`
Button(
    HStack(() =>
    {
        Image(saveIcon).frame(width: 18, height: 18);
        Text("Save").bold();
    }, spacing: 8f),
    Save);`)
              },
              {
                title: "Stateに紐づくControl",
                body: code(`
Toggle("Enabled", isEnabled);
Slider(volume, 0, 100);
Stepper("Quantity", quantity, 0, 9);
ProgressView(downloadProgress, total: 1);`)
              },
              {
                title: "Picker",
                body: code(`
Picker(difficulty, "Easy", "Normal", "Hard")
    .pickerStyle(PickerStyle.Segmented)
    .tint(accent);`)
              }
            ],
            related: ["state", "text-input", "styles"]
          },
          {
            id: "text-input",
            title: "Text入力",
            kind: "Basics",
            summary: "TextField、SecureField、TextEditor、フォーカス、キャレット、入力制限を扱います。",
            declaration: "TextField(\"Title\", text: state, prompt: Text(\"Placeholder\"))\nSecureField(\"Password\", text: state, prompt: Text(\"Password\"))\nTextEditor(state)",
            keywords: "TextField SecureField TextEditor focused prompt contentMargins caretColor onSubmit keyboardType textInputLimit",
            sections: [
              {
                title: "基本",
                body: code(`
TextField("Email", text: email, prompt: Text("email@example.com"))
    .lineLimit(1)
    .contentMargins(horizontal: 12, vertical: 8)
    .textContentType(TMP_InputField.ContentType.EmailAddress)
    .keyboardType(TouchScreenKeyboardType.EmailAddress);`)
              },
              {
                title: "promptを装飾する",
                body: code(`
TextField(
    "Nickname",
    text: nickname,
    prompt: Text("Optional")
        .italic()
        .foregroundColor(Color.gray));`)
              },
              {
                title: "パスワードと複数行",
                body: code(`
SecureField("Password", text: password, prompt: Text("Password"));

TextField("Notes", text: notes, axis: Axis.Vertical, prompt: Text("Notes"))
    .lineLimit(4)
    .frame(infiniteWidth: true, height: 100);

TextEditor(notes)
    .frame(infiniteWidth: true, height: 140);`)
              },
              {
                title: "フォーカスと送信",
                body: code(`
TextField("Search", text: query, prompt: Text("Search"))
    .focused(searchFocused)
    .selectAllOnFocus()
    .onSubmit(value => Search(value))
    .onEditingChanged(isEditing => Debug.Log(isEditing));`)
              },
              {
                title: "キャレットと選択色",
                body: code(`
TextField("Name", text: name)
    .caretColor(accent)
    .caretWidth(2)
    .caretBlinkRate(0.8f)
    .textSelectionColor(new Color(0f, 0.45f, 1f, 0.25f));`)
              }
            ],
            related: ["styles", "state", "modifier-parameters"]
          }
        ]
      },
      {
        title: "応用",
        pages: [
          {
            id: "modifiers",
            title: "Modifier設計",
            kind: "Guide",
            summary: "modifierの順番、背景、重ね合わせ、角丸、切り抜き、操作状態を理解します。",
            keywords: "modifier order background overlay border shadow cornerRadius clipped clipShape opacity tint disabled",
            sections: [
              {
                title: "順番で結果が変わる",
                body: `
                  <p>modifierは上から順に適用されます。たとえば <code>padding</code> の後に <code>background</code> を置くと、余白を含んだ背景になります。</p>
                  ${code(`
Text("Badge")
    .padding(Edge.Horizontal, 12)
    .padding(Edge.Vertical, 6)
    .background(Capsule(new Color(1f, 0.88f, 0.35f)))
    .foregroundColor(new Color(0.2f, 0.12f, 0f));`)}
                `
              },
              {
                title: "backgroundとoverlay",
                body: code(`
Text("Inbox")
    .padding(12)
    .background(RoundedRectangle(10, Color.white))
    .overlay(
        Circle(Color.red).frame(width: 8, height: 8),
        ZStackAlignment.TopTrailing);`)
              },
              {
                title: "切り抜き",
                body: code(`
Image(avatar)
    .resizable()
    .scaledToFill()
    .frame(width: 64, height: 64)
    .clipShape(UniftUIClipShape.Circle);`)
              },
              {
                title: "操作状態",
                body: code(`
Button("Submit", Submit)
    .disabled(isSubmitting)
    .allowsHitTesting(canTap)
    .opacity(isSubmitting.Value ? 0.6f : 1f);`)
              }
            ],
            related: ["modifier-parameters", "styles", "animation"]
          },
          {
            id: "styles",
            title: "Styleと再利用",
            kind: "Guide",
            summary: "ButtonStyles、TextFieldStyles、独自の組み合わせで画面全体の一貫性を作ります。",
            keywords: "ButtonStyles TextFieldStyles style reuse theme tint foregroundColor",
            sections: [
              {
                title: "ButtonStyles",
                body: code(`
Button("Continue", Continue)
    .buttonStyle(ButtonStyles.Filled(
        backgroundColor: accent,
        foregroundColor: Color.white,
        cornerRadius: 10));

Button("Cancel", Cancel)
    .buttonStyle(ButtonStyles.Plain(accent));`)
              },
              {
                title: "TextFieldStyles",
                body: code(`
var fieldStyle = TextFieldStyles.Chrome(
    backgroundColor: Color.white,
    focusedBackgroundColor: new Color(0.94f, 0.97f, 1f),
    textColor: Color.black,
    tintColor: accent,
    contentMargins: new Vector4(12, 12, 8, 8),
    cornerRadius: 8);

TextField("First name", text: firstName).textFieldStyle(fieldStyle);
TextField("Last name", text: lastName).textFieldStyle(fieldStyle);`)
              },
              {
                title: "Theme変数を持つ",
                body: code(`
private readonly Color accent = new Color(0.16f, 0.38f, 0.9f);
private readonly Color panel = new Color(0.96f, 0.97f, 0.99f);

VStack(() =>
{
    Text("Settings").bold();
    Toggle("Sound", soundEnabled).tint(accent);
})
.padding(16)
.background(panel)
.cornerRadius(12);`)
              }
            ],
            related: ["controls", "text-input", "recipes"]
          },
          {
            id: "animation",
            title: "Animation",
            kind: "Guide",
            summary: "State変化に合わせてサイズ、色、角丸、透明度、移動、回転、拡大を動かします。",
            keywords: "Animation WithAnimation animation spring easeInOut opacity offset scaleEffect rotationEffect cornerRadius",
            sections: [
              {
                title: "WithAnimation",
                body: code(`
WithAnimation(Animation.easeInOut(0.25f), () =>
{
    expanded.Value = !expanded.Value;
});`)
              },
              {
                title: "特定のStateに紐づける",
                body: code(`
Text("Tap")
    .scaleEffect(isPressed.Value ? 0.96f : 1f)
    .animation(Animation.easeOut(0.12f), isPressed);`)
              },
              {
                title: "よく使うAnimation",
                body: code(`
Animation.linear(0.2f)
Animation.easeIn(0.2f)
Animation.easeOut(0.2f)
Animation.easeInOut(0.25f)
Animation.spring()
Animation.bouncy(0.5f)
Animation.easeInOut(0.25f).delay(0.1f).repeatCount(2)`)
              },
              {
                title: "動かしやすい値",
                body: referenceTable("ja", [
                  ["opacity", "透明度", "フェードイン/アウト"],
                  ["offset / position", "移動", "開閉、通知、ドラッグ風の動き"],
                  ["frame", "サイズ", "展開/折りたたみ"],
                  ["cornerRadius", "角丸", "カードから丸いピルへの変化"],
                  ["foregroundColor / background", "色", "状態変化のフィードバック"],
                  ["rotationEffect / scaleEffect", "変形", "ボタン押下、注目演出"]
                ])
              }
            ],
            related: ["state", "modifiers", "recipes"]
          },
          {
            id: "scroll-tabs",
            title: "ScrollViewとTabView",
            kind: "Guide",
            summary: "長いコンテンツ、横スクロール、スクロール位置同期、タブ切り替えを扱います。",
            keywords: "ScrollView TabView scrollIndicators scrollPositionX scrollPositionY Tab LazyVStack",
            sections: [
              {
                title: "縦スクロール",
                body: code(`
ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, 50, i => Text($"Row {i}").padding(10));
    });
}, horizontal: false, vertical: true)
.scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical);`)
              },
              {
                title: "スクロール位置をStateへ",
                body: code(`
private readonly State<float> scrollY = new State<float>(1f);

ScrollView(() =>
{
    LazyVStack(() => { /* rows */ });
})
.scrollPositionY(scrollY, twoWay: true);`)
              },
              {
                title: "TabView",
                body: code(`
private readonly State<int> selectedTab = new State<int>(0);

TabView(() =>
{
    Tab("Home", () => HomePage());
    Tab("Settings", () => SettingsPage());
}, selectedTab)
.frame(infiniteWidth: true, infiniteHeight: true);`)
              }
            ],
            related: ["layout", "state", "api-index"]
          },
          {
            id: "performance",
            title: "実装とパフォーマンスの考え方",
            kind: "Advanced",
            summary: "大きな画面を作るときのState粒度、再構築範囲、Lazy系、Unity UIとしての注意点をまとめます。",
            keywords: "performance advanced rebuild LazyVStack State granularity Unity Canvas layout",
            sections: [
              {
                title: "Stateは小さく持つ",
                body: `
                  <p>画面全体を1つのStateで更新するより、変わる値ごとにStateを分ける方が影響範囲を読みやすくできます。</p>
                  ${code(`
private readonly State<int> hp = new State<int>(100);
private readonly State<int> gold = new State<int>(0);
private readonly State<bool> menuOpen = new State<bool>(false);`)}
                `
              },
              {
                title: "一覧はLazy系を使う",
                body: code(`
ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, items.Count, i => ItemRow(items[i]));
    });
});`)
              },
              {
                title: "modifierの責務を分ける",
                body: `
                  <p>レイアウト、見た目、操作を分けて読むと、意図が追いやすくなります。</p>
                  ${code(`
Button("Buy", Buy)
    .frame(infiniteWidth: true, height: 44)   // layout
    .buttonStyle(primaryButtonStyle)          // appearance
    .disabled(!canBuy.Value);                 // behavior`)}
                `
              },
              {
                title: "Unity UIとしての注意",
                body: `
                  <ul>
                    <li>大量のCanvas更新はUnity側のレイアウト計算にも影響します。</li>
                    <li>頻繁に変わる値は、必要な要素だけが依存するようにします。</li>
                    <li>毎フレーム処理は <code>update</code> でできますが、必要な箇所に限定します。</li>
                    <li>画像の <code>scaledToFill</code> は <code>clipped</code> や <code>clipShape</code> と組み合わせると扱いやすいです。</li>
                  </ul>
                `
              }
            ],
            related: ["state", "layout", "api-index"]
          }
        ]
      },
      {
        title: "レシピ",
        pages: [
          {
            id: "recipes",
            title: "よく使うUIパターン",
            kind: "Recipes",
            summary: "カード、フォーム、設定行、空状態、ステータスHUDなど、実装に近い形のサンプルです。",
            keywords: "recipes card form settings row empty state hud badge",
            sections: [
              {
                title: "カード",
                body: code(`
VStack(() =>
{
    Text("Storage").bold().fontSize(18);
    Text("42 GB used").fontSize(13).foregroundColor(Color.gray);

    ProgressView(storageUsed, total: 1)
        .tint(accent)
        .frame(infiniteWidth: true, height: 8);
}, spacing: 8f, alignment: VStackAlignment.Leading)
.padding(16)
.background(Color.white)
.cornerRadius(12)
.shadow(new Color(0, 0, 0, 0.12f), radius: 4, x: 0, y: -2);`)
              },
              {
                title: "ログインフォーム",
                body: code(`
VStack(() =>
{
    TextField("Email", text: email, prompt: Text("email@example.com"))
        .textFieldStyle(fieldStyle);

    SecureField("Password", text: password, prompt: Text("Password"))
        .textFieldStyle(fieldStyle);

    Button("Sign in", SignIn)
        .frame(infiniteWidth: true, height: 44)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f, alignment: VStackAlignment.Leading);`)
              },
              {
                title: "設定行",
                body: code(`
HStack(() =>
{
    VStack(() =>
    {
        Text("Notifications").bold();
        Text("Receive updates").fontSize(12).foregroundColor(Color.gray);
    }, spacing: 2f, alignment: VStackAlignment.Leading);

    Spacer();
    Toggle("On", notifications);
})
.padding(12)
.background(Color.white)
.cornerRadius(10);`)
              },
              {
                title: "空状態",
                body: code(`
VStack(() =>
{
    Image(emptyIcon)
        .resizable()
        .scaledToFit()
        .frame(width: 64, height: 64)
        .opacity(0.65f);

    Text("No items yet").bold();
    Text("Create your first item to get started.")
        .fontSize(13)
        .foregroundColor(Color.gray)
        .multilineTextAlignment(TextAlignmentOptions.Center);

    Button("Create", CreateItem)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f)
.frame(infiniteWidth: true, infiniteHeight: true);`)
              },
              {
                title: "HUD",
                body: code(`
HStack(() =>
{
    Text(() => $"HP {hp.Value}", new State[] { hp }).bold();
    Spacer();
    Text(() => $"{gold.Value} G", new State[] { gold }).bold();
})
.padding(12)
.background(new Color(0f, 0f, 0f, 0.55f))
.foregroundColor(Color.white)
.cornerRadius(10);`)
              }
            ],
            related: ["layout", "controls", "styles", "animation"]
          }
        ]
      },
      {
        title: "リファレンス",
        pages: [
          {
            id: "modifier-parameters",
            title: "modifier引数リファレンス",
            kind: "Reference",
            summary: "各modifierに渡す型と、使い分けの目安をまとめます。",
            keywords: "modifier parameters frame padding background overlay tint opacity cornerRadius border shadow input scroll",
            sections: [
              {
                title: "レイアウト",
                body: referenceTable("ja", [
                  ["frame(width:height:)", "固定サイズ", "ボタン、アイコン、カード"],
                  ["frame(infiniteWidth:infiniteHeight:)", "親の空間を使う", "横いっぱい、画面いっぱい"],
                  ["frame(minWidth:maxWidth:minHeight:maxHeight:)", "サイズ制約", "伸縮するUIの上限/下限"],
                  ["padding", "内側余白", "カード、行、ボタン"],
                  ["fixedSize", "内容サイズを優先", "伸びてほしくないラベル"],
                  ["layoutPriority", "空間配分の優先度", "同じStack内の競合調整"],
                  ["aspectRatio", "比率維持", "画像、サムネイル"]
                ])
              },
              {
                title: "見た目",
                body: referenceTable("ja", [
                  ["background", "背景色/背景View", "カード、ピル、パネル"],
                  ["overlay", "前面に重ねる", "バッジ、カスタム枠"],
                  ["foregroundColor", "前景色", "Textや対応要素"],
                  ["tint", "アクセント色", "Control、template Image、TextField"],
                  ["opacity", "透明度", "表示状態、アニメーション"],
                  ["cornerRadius", "角丸", "カード、入力欄"],
                  ["border / shadow", "枠線/影", "装飾、階層表現"],
                  ["clipped / clipShape", "切り抜き", "画像、アバター"]
                ])
              },
              {
                title: "Textと入力",
                body: referenceTable("ja", [
                  ["font / fontSize", "フォントとサイズ", "見出し、本文"],
                  ["bold / italic / underline / strikethrough", "文字スタイル", "強調、リンク風表示"],
                  ["lineLimit", "行数制限", "説明文、入力欄"],
                  ["multilineTextAlignment", "複数行の揃え", "説明文、TextField"],
                  ["focused", "フォーカスState", "フォーム制御"],
                  ["contentMargins", "TextField内側余白", "入力欄の見た目"],
                  ["caretColor / textSelectionColor", "入力中の色", "ブランド調整"],
                  ["textContentType / keyboardType", "入力種別", "メール、パスワード、モバイル"]
                ])
              },
              {
                title: "動きと操作",
                body: referenceTable("ja", [
                  ["offset", "見た目だけ移動", "バッジ、アニメーション"],
                  ["position", "配置位置指定", "自由配置"],
                  ["rotationEffect / scaleEffect", "回転/拡大", "押下フィードバック"],
                  ["disabled", "操作不可", "ロード中、条件未達"],
                  ["hidden", "非表示", "一時的な表示制御"],
                  ["allowsHitTesting", "入力受付制御", "オーバーレイ"],
                  ["onAppear / onChange / update", "ライフサイクル", "初期化、監視、毎フレーム"],
                  ["animation", "State変化のアニメーション", "UIフィードバック"]
                ])
              }
            ],
            related: ["api-index", "modifiers", "text-input"]
          },
          {
            id: "api-index",
            title: "API一覧",
            kind: "Reference",
            summary: "Factory、modifier、入力、型をコンパクトに引ける一覧です。",
            keywords: "API index Text Button Image VStack HStack State Animation modifier reference",
            sections: [
              { title: "Factory", body: referenceTable("ja", sharedReference.factories.map(row => [row[0], row[1], row[2]])) },
              { title: "Modifier", body: referenceTable("ja", sharedReference.modifiers.map(row => [row[0], row[1], row[2]])) },
              { title: "TextField modifier", body: referenceTable("ja", sharedReference.input.map(row => [row[0], row[1], row[2]])) },
              { title: "型", body: referenceTable("ja", sharedReference.types.map(row => [row[0], row[1], row[2]])) }
            ],
            related: ["modifier-parameters", "overview", "setup"]
          }
        ]
      }
    ]
  },
  en: {
    chrome: {
      search: "Search",
      toc: "On This Page",
      noResults: "No results found.",
      docsTitle: "Documentation",
      menu: "Menu",
      theme: "Theme",
      themeSystem: "System",
      themeLight: "Light",
      themeDark: "Dark"
    },
    groups: [
      {
        title: "Start Here",
        pages: [
          {
            id: "overview",
            title: "UniftUI at a Glance",
            kind: "Guide",
            summary: "Build Unity Canvas UI from C# with declarative elements, observable State, and chainable modifiers.",
            keywords: "overview start beginner Unity Canvas uGUI State modifier",
            sections: [
              {
                title: "What you build with",
                body: `
                  <p>UniftUI creates uGUI objects from C# view trees. You compose factories such as <code>Text</code>, <code>Button</code>, and <code>VStack</code>, store changing values in <code>State&lt;T&gt;</code>, and adjust layout or appearance with modifiers.</p>
                  ${cards([
                    { kicker: "1", title: "Factories", body: "Create views with Text, Button, VStack, and similar calls.", href: "#/api-index" },
                    { kicker: "2", title: "State", body: "Put changing values in State<T> so UI can follow them.", href: "#/state" },
                    { kicker: "3", title: "Modifiers", body: "Chain padding, background, fontSize, tint, and more.", href: "#/modifiers" }
                  ])}
                `
              },
              {
                title: "Small useful example",
                body: code(`
using UniftUI;
using UnityEngine;

public sealed class CounterView : UniftView
{
    private readonly State<int> count = new State<int>(0);

    private void Start()
    {
        VStack(() =>
        {
            Text(() => $"Count: {count.Value}", new State[] { count })
                .fontSize(28)
                .bold();

            Button("Increment", () => count.Value++)
                .padding(12)
                .background(new Color(0.15f, 0.38f, 0.9f))
                .foregroundColor(Color.white)
                .cornerRadius(10);
        }, spacing: 12f)
        .padding(24)
        .Build(GetComponent<Canvas>());
    }
}`)
              },
              {
                title: "Recommended path",
                body: steps([
                  { title: "Install", body: "Get a view rendering into a Canvas." },
                  { title: "State", body: "Learn how values update UI." },
                  { title: "Layout", body: "Use stacks, Spacer, frame, and padding." },
                  { title: "Controls", body: "Connect buttons, fields, pickers, and sliders to State." },
                  { title: "Reference", body: "Use the API index and modifier parameter pages when you need details." }
                ])
              },
              {
                title: "Best fit",
                body: cards([
                  { kicker: "UI", title: "Game menus", body: "Settings, status, inventory, debug UI, and other Canvas screens." },
                  { kicker: "TOOLS", title: "Internal tools", body: "Parameter panels, previews, and editor-like runtime utilities." },
                  { kicker: "FAST", title: "Prototypes", body: "Try screen structure in C# before committing to prefabs." }
                ])
              }
            ],
            related: ["setup", "state", "layout"]
          },
          {
            id: "setup",
            title: "Install and Render",
            kind: "Guide",
            summary: "Install the package, prepare a Canvas, and render your first view.",
            keywords: "install setup Package Manager Canvas Build TMP font",
            sections: [
              {
                title: "Package Manager",
                body: `
                  <p>In Unity Package Manager, choose <strong>Add package from git URL</strong> and enter:</p>
                  ${code("https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI")}
                  ${callout("To pin a version, append a tag such as <code>#v0.1.0</code>.")}
                `
              },
              {
                title: "Scene setup",
                body: steps([
                  { title: "Create a Canvas", body: "Add a Canvas GameObject to the scene." },
                  { title: "Inherit UniftView", body: "Attach your script to the same GameObject as the Canvas." },
                  { title: "Build", body: "Call <code>.Build(GetComponent&lt;Canvas&gt;())</code> after composing the view." }
                ])
              },
              {
                title: "Starter script",
                body: code(`
using UniftUI;
using UnityEngine;

public sealed class MainMenuView : UniftView
{
    private void Start()
    {
        VStack(() =>
        {
            Text("Main Menu").fontSize(32).bold();
            Button("Start", StartGame);
            Button("Options", OpenOptions);
        }, spacing: 12f)
        .frame(infiniteWidth: true, infiniteHeight: true)
        .Build(GetComponent<Canvas>());
    }
}`)
              },
              {
                title: "Troubleshooting",
                body: `
                  <ul>
                    <li>Make sure the script is on the same GameObject as the Canvas.</li>
                    <li>Make sure <code>Build</code> is called.</li>
                    <li>Check the Console for exceptions.</li>
                    <li>If text is missing, check your TextMesh Pro font setup.</li>
                  </ul>
                `
              }
            ],
            related: ["overview", "state", "text"]
          }
        ]
      },
      {
        title: "Core Concepts",
        pages: [
          {
            id: "state",
            title: "State and Updates",
            kind: "Basics",
            summary: "Use State for changing values, text input, controls, callbacks, and animations.",
            declaration: "public class State<T> : State",
            keywords: "State Value reactive onChange BatchUpdate binding update",
            sections: [
              {
                title: "State in one minute",
                body: `
                  <p><code>State&lt;T&gt;</code> is an observable value. Change <code>Value</code>, and dependent UI updates.</p>
                  ${code(`
private readonly State<int> score = new State<int>(0);

Text(() => $"Score: {score.Value}", new State[] { score });
Button("+10", () => score.Value += 10);`)}
                `
              },
              {
                title: "Two-way text input",
                body: code(`
private readonly State<string> playerName = new State<string>("");

TextField("Name", text: playerName, prompt: Text("Player name"));
Text(() => $"Hello, {playerName.Value}", new State[] { playerName });`)
              },
              {
                title: "Run code on change",
                body: code(`
Slider(volume, 0, 100)
    .onChange(volume, value =>
    {
        AudioListener.volume = value / 100f;
    });`)
              },
              {
                title: "Batch updates",
                body: code(`
using (State.BatchUpdate())
{
    hp.Value = maxHp.Value;
    mana.Value = maxMana.Value;
    selectedTab.Value = 0;
}`)
              }
            ],
            related: ["controls", "text-input", "animation"]
          },
          {
            id: "layout",
            title: "Layout",
            kind: "Basics",
            summary: "Arrange views with stacks, Spacer, frame, padding, grids, lazy stacks, and GeometryReader.",
            keywords: "layout VStack HStack ZStack Spacer frame padding fixedSize Grid LazyVStack GeometryReader",
            sections: [
              { title: "Stacks", body: code(`
VStack(() =>
{
    Text("Profile").fontSize(24).bold();

    HStack(() =>
    {
        Text("Level");
        Spacer();
        Text("12").bold();
    });
}, spacing: 12f, alignment: VStackAlignment.Leading);`) },
              { title: "Full-width card", body: code(`
Text("Quest accepted")
    .padding(16)
    .frame(infiniteWidth: true)
    .background(new Color(0.95f, 0.97f, 1f))
    .cornerRadius(12);`) },
              { title: "Lists", body: code(`
ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, quests.Count, i =>
        {
            Text(quests[i].Title)
                .padding(12)
                .frame(infiniteWidth: true);
        });
    }, spacing: 4f);
});`) },
              { title: "Grid", body: code(`
Grid(() =>
{
    GridRow(() => { Text("Stat").bold(); Text("Value").bold(); });
    GridRow(() => { Text("HP"); Text("120"); });
    GridRow(() => { Text("Attack"); Text("34"); });
}, horizontalSpacing: 16f, verticalSpacing: 8f);`) },
              { title: "GeometryReader", body: code(`
GeometryReader(proxy =>
{
    return Text($"Size: {proxy.Size.x:0} x {proxy.Size.y:0}")
        .frame(infiniteWidth: true, infiniteHeight: true);
})
.frame(infiniteWidth: true, height: 80);`) }
            ],
            related: ["modifiers", "scroll-tabs", "api-index"]
          },
          {
            id: "text",
            title: "Text and Fonts",
            kind: "Basics",
            summary: "Display static, State-bound, and computed text with TextMesh Pro styling.",
            declaration: "Text(string text)\nText(State<string> text)\nText(Func<string> content, State[] dependencyStates)",
            keywords: "Text font fontSize TMP_FontAsset bold italic lineLimit multilineTextAlignment foregroundColor",
            sections: [
              { title: "Text forms", body: code(`
Text("Static");
Text(titleState);
Text(() => $"HP: {hp.Value}", new State[] { hp });`) },
              { title: "Style", body: code(`
Text("Warning")
    .fontSize(20)
    .bold()
    .foregroundColor(new Color(0.9f, 0.18f, 0.12f));`) },
              { title: "Fonts", body: code(`
UIContext.SetDefaultFont(defaultFont);

Text("Custom font")
    .font(titleFont)
    .fontSize(28);`) },
              { title: "Wrapping", body: code(`
Text("Long description that can wrap onto multiple lines.")
    .lineLimit(2)
    .multilineTextAlignment(TextAlignmentOptions.TopLeft)
    .frame(width: 240);`) }
            ],
            related: ["text-input", "modifiers", "api-index"]
          },
          {
            id: "controls",
            title: "Buttons and Controls",
            kind: "Basics",
            summary: "Create actions and state-bound controls: buttons, toggles, sliders, steppers, progress, and pickers.",
            keywords: "Button Toggle Slider Stepper ProgressView Picker buttonStyle pickerStyle controls",
            sections: [
              { title: "Button", body: code(`
Button("Save", Save)
    .frame(infiniteWidth: true, height: 44)
    .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));`) },
              { title: "Custom button label", body: code(`
Button(
    HStack(() =>
    {
        Image(saveIcon).frame(width: 18, height: 18);
        Text("Save").bold();
    }, spacing: 8f),
    Save);`) },
              { title: "State-bound controls", body: code(`
Toggle("Enabled", isEnabled);
Slider(volume, 0, 100);
Stepper("Quantity", quantity, 0, 9);
ProgressView(downloadProgress, total: 1);`) },
              { title: "Picker", body: code(`
Picker(difficulty, "Easy", "Normal", "Hard")
    .pickerStyle(PickerStyle.Segmented)
    .tint(accent);`) }
            ],
            related: ["state", "text-input", "styles"]
          },
          {
            id: "text-input",
            title: "Text Input",
            kind: "Basics",
            summary: "Build TextField, SecureField, TextEditor, focus behavior, caret styling, and input rules.",
            declaration: "TextField(\"Title\", text: state, prompt: Text(\"Placeholder\"))\nSecureField(\"Password\", text: state, prompt: Text(\"Password\"))\nTextEditor(state)",
            keywords: "TextField SecureField TextEditor focused prompt contentMargins caretColor onSubmit keyboardType textInputLimit",
            sections: [
              { title: "Basic field", body: code(`
TextField("Email", text: email, prompt: Text("email@example.com"))
    .lineLimit(1)
    .contentMargins(horizontal: 12, vertical: 8)
    .textContentType(TMP_InputField.ContentType.EmailAddress)
    .keyboardType(TouchScreenKeyboardType.EmailAddress);`) },
              { title: "Style the prompt", body: code(`
TextField(
    "Nickname",
    text: nickname,
    prompt: Text("Optional")
        .italic()
        .foregroundColor(Color.gray));`) },
              { title: "Password and multiline", body: code(`
SecureField("Password", text: password, prompt: Text("Password"));

TextField("Notes", text: notes, axis: Axis.Vertical, prompt: Text("Notes"))
    .lineLimit(4)
    .frame(infiniteWidth: true, height: 100);

TextEditor(notes)
    .frame(infiniteWidth: true, height: 140);`) },
              { title: "Focus and submit", body: code(`
TextField("Search", text: query, prompt: Text("Search"))
    .focused(searchFocused)
    .selectAllOnFocus()
    .onSubmit(value => Search(value))
    .onEditingChanged(isEditing => Debug.Log(isEditing));`) },
              { title: "Caret and selection", body: code(`
TextField("Name", text: name)
    .caretColor(accent)
    .caretWidth(2)
    .caretBlinkRate(0.8f)
    .textSelectionColor(new Color(0f, 0.45f, 1f, 0.25f));`) }
            ],
            related: ["styles", "state", "modifier-parameters"]
          }
        ]
      },
      {
        title: "Advanced",
        pages: [
          {
            id: "modifiers",
            title: "Modifier Design",
            kind: "Guide",
            summary: "Understand modifier order, backgrounds, overlays, rounded corners, clipping, and interaction state.",
            keywords: "modifier order background overlay border shadow cornerRadius clipped clipShape opacity tint disabled",
            sections: [
              { title: "Order matters", body: `
                <p>Modifiers apply in order. If <code>background</code> comes after <code>padding</code>, the background includes that padding.</p>
                ${code(`
Text("Badge")
    .padding(Edge.Horizontal, 12)
    .padding(Edge.Vertical, 6)
    .background(Capsule(new Color(1f, 0.88f, 0.35f)))
    .foregroundColor(new Color(0.2f, 0.12f, 0f));`)}
              ` },
              { title: "Background and overlay", body: code(`
Text("Inbox")
    .padding(12)
    .background(RoundedRectangle(10, Color.white))
    .overlay(
        Circle(Color.red).frame(width: 8, height: 8),
        ZStackAlignment.TopTrailing);`) },
              { title: "Clipping", body: code(`
Image(avatar)
    .resizable()
    .scaledToFill()
    .frame(width: 64, height: 64)
    .clipShape(UniftUIClipShape.Circle);`) },
              { title: "Interaction state", body: code(`
Button("Submit", Submit)
    .disabled(isSubmitting)
    .allowsHitTesting(canTap)
    .opacity(isSubmitting.Value ? 0.6f : 1f);`) }
            ],
            related: ["modifier-parameters", "styles", "animation"]
          },
          {
            id: "styles",
            title: "Styles and Reuse",
            kind: "Guide",
            summary: "Use ButtonStyles, TextFieldStyles, and shared theme values to keep screens consistent.",
            keywords: "ButtonStyles TextFieldStyles style reuse theme tint foregroundColor",
            sections: [
              { title: "ButtonStyles", body: code(`
Button("Continue", Continue)
    .buttonStyle(ButtonStyles.Filled(
        backgroundColor: accent,
        foregroundColor: Color.white,
        cornerRadius: 10));

Button("Cancel", Cancel)
    .buttonStyle(ButtonStyles.Plain(accent));`) },
              { title: "TextFieldStyles", body: code(`
var fieldStyle = TextFieldStyles.Chrome(
    backgroundColor: Color.white,
    focusedBackgroundColor: new Color(0.94f, 0.97f, 1f),
    textColor: Color.black,
    tintColor: accent,
    contentMargins: new Vector4(12, 12, 8, 8),
    cornerRadius: 8);

TextField("First name", text: firstName).textFieldStyle(fieldStyle);
TextField("Last name", text: lastName).textFieldStyle(fieldStyle);`) },
              { title: "Theme values", body: code(`
private readonly Color accent = new Color(0.16f, 0.38f, 0.9f);
private readonly Color panel = new Color(0.96f, 0.97f, 0.99f);

VStack(() =>
{
    Text("Settings").bold();
    Toggle("Sound", soundEnabled).tint(accent);
})
.padding(16)
.background(panel)
.cornerRadius(12);`) }
            ],
            related: ["controls", "text-input", "recipes"]
          },
          {
            id: "animation",
            title: "Animation",
            kind: "Guide",
            summary: "Animate State-driven size, color, corner radius, opacity, movement, rotation, and scale.",
            keywords: "Animation WithAnimation animation spring easeInOut opacity offset scaleEffect rotationEffect cornerRadius",
            sections: [
              { title: "WithAnimation", body: code(`
WithAnimation(Animation.easeInOut(0.25f), () =>
{
    expanded.Value = !expanded.Value;
});`) },
              { title: "Bind animation to one State", body: code(`
Text("Tap")
    .scaleEffect(isPressed.Value ? 0.96f : 1f)
    .animation(Animation.easeOut(0.12f), isPressed);`) },
              { title: "Common animations", body: code(`
Animation.linear(0.2f)
Animation.easeIn(0.2f)
Animation.easeOut(0.2f)
Animation.easeInOut(0.25f)
Animation.spring()
Animation.bouncy(0.5f)
Animation.easeInOut(0.25f).delay(0.1f).repeatCount(2)`) },
              { title: "Good animation targets", body: referenceTable("en", [
                ["opacity", "Alpha", "Fade in and out"],
                ["offset / position", "Movement", "Open/close, notices, drag-like motion"],
                ["frame", "Size", "Expand and collapse"],
                ["cornerRadius", "Corners", "Card to pill transitions"],
                ["foregroundColor / background", "Color", "State feedback"],
                ["rotationEffect / scaleEffect", "Transform", "Tap feedback and emphasis"]
              ]) }
            ],
            related: ["state", "modifiers", "recipes"]
          },
          {
            id: "scroll-tabs",
            title: "ScrollView and TabView",
            kind: "Guide",
            summary: "Handle long content, horizontal/vertical scrolling, scroll position binding, and tab switching.",
            keywords: "ScrollView TabView scrollIndicators scrollPositionX scrollPositionY Tab LazyVStack",
            sections: [
              { title: "Vertical scrolling", body: code(`
ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, 50, i => Text($"Row {i}").padding(10));
    });
}, horizontal: false, vertical: true)
.scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical);`) },
              { title: "Bind scroll position", body: code(`
private readonly State<float> scrollY = new State<float>(1f);

ScrollView(() =>
{
    LazyVStack(() => { /* rows */ });
})
.scrollPositionY(scrollY, twoWay: true);`) },
              { title: "Tabs", body: code(`
private readonly State<int> selectedTab = new State<int>(0);

TabView(() =>
{
    Tab("Home", () => HomePage());
    Tab("Settings", () => SettingsPage());
}, selectedTab)
.frame(infiniteWidth: true, infiniteHeight: true);`) }
            ],
            related: ["layout", "state", "api-index"]
          },
          {
            id: "performance",
            title: "Implementation and Performance",
            kind: "Advanced",
            summary: "Think about State granularity, rebuild scope, lazy stacks, and Unity Canvas costs.",
            keywords: "performance advanced rebuild LazyVStack State granularity Unity Canvas layout",
            sections: [
              { title: "Keep State focused", body: `
                <p>Prefer several focused State values over one large screen state. It makes dependencies and updates easier to reason about.</p>
                ${code(`
private readonly State<int> hp = new State<int>(100);
private readonly State<int> gold = new State<int>(0);
private readonly State<bool> menuOpen = new State<bool>(false);`)}
              ` },
              { title: "Use lazy stacks for lists", body: code(`
ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, items.Count, i => ItemRow(items[i]));
    });
});`) },
              { title: "Separate responsibilities", body: code(`
Button("Buy", Buy)
    .frame(infiniteWidth: true, height: 44)   // layout
    .buttonStyle(primaryButtonStyle)          // appearance
    .disabled(!canBuy.Value);                 // behavior`) },
              { title: "Unity UI notes", body: `
                <ul>
                  <li>Large Canvas layout changes still have Unity-side cost.</li>
                  <li>Make fast-changing values dependencies only where needed.</li>
                  <li>Use <code>update</code> sparingly for per-frame work.</li>
                  <li>Pair <code>scaledToFill</code> with <code>clipped</code> or <code>clipShape</code> for image crops.</li>
                </ul>
              ` }
            ],
            related: ["state", "layout", "api-index"]
          }
        ]
      },
      {
        title: "Recipes",
        pages: [
          {
            id: "recipes",
            title: "Common UI Patterns",
            kind: "Recipes",
            summary: "Copyable patterns for cards, forms, settings rows, empty states, and HUDs.",
            keywords: "recipes card form settings row empty state hud badge",
            sections: [
              { title: "Card", body: code(`
VStack(() =>
{
    Text("Storage").bold().fontSize(18);
    Text("42 GB used").fontSize(13).foregroundColor(Color.gray);

    ProgressView(storageUsed, total: 1)
        .tint(accent)
        .frame(infiniteWidth: true, height: 8);
}, spacing: 8f, alignment: VStackAlignment.Leading)
.padding(16)
.background(Color.white)
.cornerRadius(12)
.shadow(new Color(0, 0, 0, 0.12f), radius: 4, x: 0, y: -2);`) },
              { title: "Login form", body: code(`
VStack(() =>
{
    TextField("Email", text: email, prompt: Text("email@example.com"))
        .textFieldStyle(fieldStyle);

    SecureField("Password", text: password, prompt: Text("Password"))
        .textFieldStyle(fieldStyle);

    Button("Sign in", SignIn)
        .frame(infiniteWidth: true, height: 44)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f, alignment: VStackAlignment.Leading);`) },
              { title: "Settings row", body: code(`
HStack(() =>
{
    VStack(() =>
    {
        Text("Notifications").bold();
        Text("Receive updates").fontSize(12).foregroundColor(Color.gray);
    }, spacing: 2f, alignment: VStackAlignment.Leading);

    Spacer();
    Toggle("On", notifications);
})
.padding(12)
.background(Color.white)
.cornerRadius(10);`) },
              { title: "Empty state", body: code(`
VStack(() =>
{
    Image(emptyIcon)
        .resizable()
        .scaledToFit()
        .frame(width: 64, height: 64)
        .opacity(0.65f);

    Text("No items yet").bold();
    Text("Create your first item to get started.")
        .fontSize(13)
        .foregroundColor(Color.gray)
        .multilineTextAlignment(TextAlignmentOptions.Center);

    Button("Create", CreateItem)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f)
.frame(infiniteWidth: true, infiniteHeight: true);`) },
              { title: "HUD", body: code(`
HStack(() =>
{
    Text(() => $"HP {hp.Value}", new State[] { hp }).bold();
    Spacer();
    Text(() => $"{gold.Value} G", new State[] { gold }).bold();
})
.padding(12)
.background(new Color(0f, 0f, 0f, 0.55f))
.foregroundColor(Color.white)
.cornerRadius(10);`) }
            ],
            related: ["layout", "controls", "styles", "animation"]
          }
        ]
      },
      {
        title: "Reference",
        pages: [
          {
            id: "modifier-parameters",
            title: "Modifier Parameters",
            kind: "Reference",
            summary: "Parameter types and practical guidance for the modifiers you use most often.",
            keywords: "modifier parameters frame padding background overlay tint opacity cornerRadius border shadow input scroll",
            sections: [
              { title: "Layout", body: referenceTable("en", [
                ["frame(width:height:)", "Fixed preferred size", "Buttons, icons, cards"],
                ["frame(infiniteWidth:infiniteHeight:)", "Use available parent space", "Full-width or full-screen layout"],
                ["frame(minWidth:maxWidth:minHeight:maxHeight:)", "Size limits", "Flexible UI with bounds"],
                ["padding", "Inner spacing", "Cards, rows, buttons"],
                ["fixedSize", "Prefer intrinsic size", "Labels that should not stretch"],
                ["layoutPriority", "Space priority", "Sibling layout conflicts"],
                ["aspectRatio", "Preserve ratio", "Images and thumbnails"]
              ]) },
              { title: "Appearance", body: referenceTable("en", [
                ["background", "Color or background view", "Cards, pills, panels"],
                ["overlay", "Foreground layer", "Badges and custom borders"],
                ["foregroundColor", "Foreground color", "Text and supported views"],
                ["tint", "Accent color", "Controls, template images, text fields"],
                ["opacity", "Alpha", "Visibility and animation"],
                ["cornerRadius", "Rounded corners", "Cards and input fields"],
                ["border / shadow", "Outline and depth", "Decoration and hierarchy"],
                ["clipped / clipShape", "Masking", "Images and avatars"]
              ]) },
              { title: "Text and input", body: referenceTable("en", [
                ["font / fontSize", "Font and size", "Headings and labels"],
                ["bold / italic / underline / strikethrough", "Text style", "Emphasis and link-like UI"],
                ["lineLimit", "Line limit", "Descriptions and input fields"],
                ["multilineTextAlignment", "Multiline alignment", "Descriptions and TextField"],
                ["focused", "Focus State", "Form control"],
                ["contentMargins", "TextField inset", "Input chrome"],
                ["caretColor / textSelectionColor", "Editing colors", "Brand polish"],
                ["textContentType / keyboardType", "Input type", "Email, password, mobile"]
              ]) },
              { title: "Motion and behavior", body: referenceTable("en", [
                ["offset", "Visual movement", "Badges and animation"],
                ["position", "Explicit placement", "Free layout"],
                ["rotationEffect / scaleEffect", "Transform", "Tap feedback"],
                ["disabled", "Disable interaction", "Loading and invalid forms"],
                ["hidden", "Hide", "Temporary visibility"],
                ["allowsHitTesting", "Pointer input", "Overlays"],
                ["onAppear / onChange / update", "Lifecycle", "Setup, observation, per-frame work"],
                ["animation", "Animate State changes", "UI feedback"]
              ]) }
            ],
            related: ["api-index", "modifiers", "text-input"]
          },
          {
            id: "api-index",
            title: "API Index",
            kind: "Reference",
            summary: "A compact index of factories, modifiers, input helpers, and types.",
            keywords: "API index Text Button Image VStack HStack State Animation modifier reference",
            sections: [
              { title: "Factories", body: referenceTable("en", sharedReference.factories) },
              { title: "Modifiers", body: referenceTable("en", sharedReference.modifiers) },
              { title: "TextField modifiers", body: referenceTable("en", sharedReference.input) },
              { title: "Types", body: referenceTable("en", sharedReference.types) }
            ],
            related: ["modifier-parameters", "overview", "setup"]
          }
        ]
      }
    ]
  }
};

const aliases = {
  home: "overview",
  start: "overview",
  install: "setup",
  textfield: "text-input",
  securefield: "text-input",
  texteditor: "text-input",
  font: "text",
  fontsize: "text",
  vstack: "layout",
  hstack: "layout",
  zstack: "layout",
  grid: "layout",
  button: "controls",
  toggle: "controls",
  slider: "controls",
  picker: "controls",
  image: "modifiers",
  scrollview: "scroll-tabs",
  tabview: "scroll-tabs",
  withanimation: "animation",
  modifiers: "modifiers",
  reference: "api-index"
};

const symbolList = document.getElementById("symbol-list");
const article = document.getElementById("article");
const tocList = document.getElementById("toc-list");
const searchInput = document.getElementById("search-input");
const languageButtons = [...document.querySelectorAll("[data-lang]")];
const themeButtons = [...document.querySelectorAll("[data-theme-choice]")];
const themeToggle = document.querySelector(".theme-toggle");
const mobileMenuButton = document.querySelector(".mobile-menu-button");
const mobileScrim = document.querySelector(".mobile-scrim");
const systemDarkQuery = window.matchMedia("(prefers-color-scheme: dark)");
const reduceMotionQuery = window.matchMedia("(prefers-reduced-motion: reduce)");
let revealObserver;

function readStorage(key) {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
}

function writeStorage(key, value) {
  try {
    localStorage.setItem(key, value);
  } catch {
    // Theme and language should still work for the current session.
  }
}

function initialLanguage() {
  const stored = readStorage(languageStorageKey);
  if (stored === "ja" || stored === "en") return stored;
  return navigator.language && navigator.language.toLowerCase().startsWith("ja") ? "ja" : "en";
}

function initialTheme() {
  const stored = readStorage(themeStorageKey);
  return themeChoices.includes(stored) ? stored : "system";
}

function resolvedTheme(theme) {
  return theme === "system"
    ? (systemDarkQuery.matches ? "dark" : "light")
    : theme;
}

let currentLanguage = initialLanguage();
let currentTheme = initialTheme();

function flatPages(lang = currentLanguage) {
  return docs[lang].groups.flatMap(group => group.pages.map(page => ({ ...page, group: group.title })));
}

function pageById(id, lang = currentLanguage) {
  return flatPages(lang).find(page => page.id === id);
}

function currentId() {
  const raw = location.hash.replace(/^#\/?/, "") || "overview";
  return aliases[raw.toLowerCase()] || raw;
}

function stripHtml(value) {
  return String(value).replace(/<[^>]*>/g, " ");
}

function matchesQuery(page, query) {
  if (!query) return true;
  const haystack = [
    page.title,
    page.kind,
    page.summary,
    page.keywords || "",
    ...(page.sections || []).map(section => `${section.title} ${stripHtml(section.body)}`)
  ].join(" ").toLowerCase();
  return haystack.includes(query);
}

function renderSidebar() {
  const query = searchInput.value.trim().toLowerCase();
  symbolList.innerHTML = "";

  for (const group of docs[currentLanguage].groups) {
    const pages = group.pages.filter(page => matchesQuery(page, query));
    if (!pages.length) continue;

    const section = document.createElement("section");
    section.className = "symbol-group";
    section.innerHTML = `<div class="symbol-heading">${group.title}</div>`;

    for (const page of pages) {
      const link = document.createElement("a");
      link.className = "symbol-link";
      link.href = `#/${page.id}`;
      link.textContent = page.title;
      if (currentId() === page.id) link.classList.add("active");
      section.appendChild(link);
    }

    symbolList.appendChild(section);
  }

  if (!symbolList.children.length) {
    symbolList.innerHTML = `<div class="empty">${docs[currentLanguage].chrome.noResults}</div>`;
  }
}

function setMobileNavOpen(open) {
  document.body.classList.toggle("nav-open", open);
  if (mobileMenuButton) {
    mobileMenuButton.setAttribute("aria-expanded", open ? "true" : "false");
  }
  if (mobileScrim) {
    mobileScrim.hidden = !open;
  }
}

function inferCodeLanguage(text) {
  const value = text.trim();
  return value.startsWith("http://") || value.startsWith("https://")
    ? "language-none"
    : "language-csharp";
}

function highlightCodeBlocks(root) {
  root.querySelectorAll("pre code").forEach(block => {
    if (![...block.classList].some(className => className.startsWith("language-"))) {
      block.classList.add(inferCodeLanguage(block.textContent));
    }
    const languageClass = [...block.classList].find(className => className.startsWith("language-"));
    if (languageClass) block.closest("pre")?.classList.add(languageClass);
  });

  if (window.Prism) window.Prism.highlightAllUnder(root);
}

function setupReveal(root) {
  const targets = [...root.querySelectorAll("section, .declaration, .topic-card, .callout, .step, pre")];

  if (revealObserver) {
    revealObserver.disconnect();
    revealObserver = null;
  }

  if (reduceMotionQuery.matches) {
    targets.forEach(target => target.classList.add("is-visible"));
    return;
  }

  revealObserver = new IntersectionObserver(entries => {
    entries.forEach(entry => {
      if (!entry.isIntersecting) return;
      entry.target.classList.add("is-visible");
      revealObserver.unobserve(entry.target);
    });
  }, {
    rootMargin: "0px 0px -10% 0px",
    threshold: 0.08
  });

  targets.forEach(target => {
    target.classList.add("reveal");
    revealObserver.observe(target);
  });
}

function renderToc(page) {
  const links = [];
  if (page.declaration) links.push(["declaration", currentLanguage === "ja" ? "宣言" : "Declaration"]);
  for (const section of page.sections || []) links.push([slug(section.title), section.title]);
  if (page.related && page.related.length) links.push(["related", currentLanguage === "ja" ? "次に読む" : "Next"]);
  tocList.innerHTML = links.map(([id, title]) => `<a href="#${id}">${title}</a>`).join("");
}

function slug(value) {
  return String(value).toLowerCase().replace(/[^a-z0-9\u3040-\u30ff\u3400-\u9fff]+/g, "-").replace(/^-|-$/g, "") || "section";
}

function renderArticle() {
  const page = pageById(currentId()) || pageById("overview");
  const chrome = docs[currentLanguage].chrome;
  const declaration = page.declaration
    ? `<section class="declaration" id="declaration"><h2 class="declaration-title">${currentLanguage === "ja" ? "宣言" : "Declaration"}</h2><pre><code class="language-csharp">${escapeHtml(page.declaration)}</code></pre></section>`
    : "";

  const sectionHtml = (page.sections || []).map(section => `
    <section id="${slug(section.title)}">
      <h2 class="section-title">${section.title}</h2>
      ${section.body}
    </section>
  `).join("");

  const relatedHtml = page.related && page.related.length
    ? `<section id="related">
        <h2 class="section-title">${currentLanguage === "ja" ? "次に読む" : "Next"}</h2>
        <div class="topics">
          ${page.related.map(id => pageById(id)).filter(Boolean).map(related => `
            <div class="topic-card">
              <div><span class="signature">${related.kind}</span><a href="#/${related.id}">${related.title}</a></div>
              <p>${related.summary}</p>
            </div>
          `).join("")}
        </div>
      </section>`
    : "";

  article.innerHTML = `<article class="article">
    <p class="eyebrow">${page.group}</p>
    <h1>${page.title}</h1>
    <p class="abstract">${page.summary}</p>
    <div class="availability">
      <span class="badge kind">${page.kind}</span>
      <span class="badge">Unity 2022.3+</span>
      <span class="badge">uGUI</span>
      <span class="badge">TextMesh Pro</span>
    </div>
    ${declaration}
    ${sectionHtml}
    ${relatedHtml}
  </article>`;

  renderToc(page);
  renderSidebar();
  highlightCodeBlocks(article);
  setupReveal(article);
  document.title = `${page.title} | UniftUI ${chrome.docsTitle}`;
}

function applyTheme() {
  document.documentElement.dataset.theme = currentTheme;
  document.documentElement.dataset.resolvedTheme = resolvedTheme(currentTheme);
}

function renderThemeToggle() {
  const chrome = docs[currentLanguage].chrome;
  const labels = {
    system: chrome.themeSystem,
    light: chrome.themeLight,
    dark: chrome.themeDark
  };

  themeToggle.setAttribute("aria-label", chrome.theme);
  themeButtons.forEach(button => {
    const choice = button.dataset.themeChoice;
    button.textContent = labels[choice];
    button.classList.toggle("active", choice === currentTheme);
    button.setAttribute("aria-pressed", choice === currentTheme ? "true" : "false");
  });
}

function renderChrome() {
  const chrome = docs[currentLanguage].chrome;
  document.documentElement.lang = currentLanguage;
  searchInput.placeholder = chrome.search;
  document.querySelector(".toc-title").textContent = chrome.toc;
  document.querySelector(".sidebar-title").textContent = "UniftUI";
  if (mobileMenuButton) {
    mobileMenuButton.setAttribute("aria-label", chrome.menu);
    mobileMenuButton.setAttribute("title", chrome.menu);
  }
  languageButtons.forEach(button => {
    const active = button.dataset.lang === currentLanguage;
    button.classList.toggle("active", active);
    button.setAttribute("aria-pressed", active ? "true" : "false");
  });
  renderThemeToggle();
}

function setLanguage(lang) {
  currentLanguage = lang;
  writeStorage(languageStorageKey, lang);
  renderChrome();
  renderArticle();
}

function setTheme(theme) {
  currentTheme = themeChoices.includes(theme) ? theme : "system";
  writeStorage(themeStorageKey, currentTheme);
  applyTheme();
  renderThemeToggle();
}

languageButtons.forEach(button => {
  button.addEventListener("click", () => setLanguage(button.dataset.lang));
});

themeButtons.forEach(button => {
  button.addEventListener("click", () => setTheme(button.dataset.themeChoice));
});

if (mobileMenuButton) {
  mobileMenuButton.addEventListener("click", () => {
    setMobileNavOpen(!document.body.classList.contains("nav-open"));
  });
}

if (mobileScrim) {
  mobileScrim.addEventListener("click", () => setMobileNavOpen(false));
}

symbolList.addEventListener("click", event => {
  if (event.target.closest("a")) setMobileNavOpen(false);
});

window.addEventListener("keydown", event => {
  if (event.key === "Escape") setMobileNavOpen(false);
});

if (systemDarkQuery.addEventListener) {
  systemDarkQuery.addEventListener("change", applyTheme);
} else {
  systemDarkQuery.addListener(applyTheme);
}

searchInput.addEventListener("input", renderSidebar);
window.addEventListener("hashchange", () => {
  setMobileNavOpen(false);
  renderArticle();
  document.getElementById("main").focus({ preventScroll: true });
  window.scrollTo({ top: 0, behavior: "auto" });
});

applyTheme();
renderChrome();
renderArticle();
