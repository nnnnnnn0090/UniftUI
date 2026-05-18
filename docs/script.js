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

function sampleImage(alt) {
  const escapedAlt = escapeHtml(alt);
  return `<figure class="sample-shot">
    <img src="./assets/counter-sample.svg" alt="${escapedAlt}" loading="lazy">
    <figcaption>${escapedAlt}</figcaption>
  </figure>`;
}

function referenceTable(lang, rows) {
  return table(
    lang === "ja" ? ["名前", "用途", "よく使う場面"] : ["Name", "Purpose", "Use it for"],
    rows
  );
}

function parameterTable(lang, rows) {
  return table(
    lang === "ja" ? ["API", "引数", "型", "意味"] : ["API", "Parameter", "Type", "Meaning"],
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

const factoryParametersJa = {
  basics: [
    ["Text(string text)", "text", "string", "表示する固定文字列です。"],
    ["Text(string text, State[] dependencyStates)", "dependencyStates", "State[]", "固定文字列でも、指定したState変更時にTextを再生成したい場合に渡します。"],
    ["Text(State&lt;string&gt; text)", "text", "State&lt;string&gt;", "Stateの値をそのまま表示します。値が変わるとTextも更新されます。"],
    ["Text(State&lt;string&gt; text, State[] additionalStates)", "additionalStates", "State[]", "text以外にも更新のきっかけにしたいStateです。"],
    ["Text(Func&lt;string&gt; content, State[] dependencyStates)", "content", "Func&lt;string&gt;", "表示文字列を計算する関数です。"],
    ["Text(Func&lt;string&gt; content, State[] dependencyStates)", "dependencyStates", "State[]", "このTextを再計算するきっかけになるStateです。"],
    ["Button(string label, Action onClick)", "label", "string", "ボタンに表示する文字です。"],
    ["Button(string label, Action onClick)", "onClick", "Action", "クリック/タップ時に実行する処理です。"],
    ["Button(Action action, string label)", "action / label", "Action / string", "処理を先に書くオーバーロードです。意味はButton(label, onClick)と同じです。"],
    ["Button(UIElement content, Action onClick)", "content", "UIElement", "TextやHStackなど、ボタン内に置くカスタム表示です。"],
    ["Button(Action action, UIElement label)", "action / label", "Action / UIElement", "カスタム表示を使い、処理を先に書くオーバーロードです。"],
    ["Toggle(string title, State&lt;bool&gt; isOn)", "title", "string", "トグル横に表示する文字です。"],
    ["Toggle(string title, State&lt;bool&gt; isOn)", "isOn", "State&lt;bool&gt;", "オン/オフ状態です。ユーザー操作で値も変わります。"],
    ["Toggle(..., Action&lt;bool&gt; onValueChanged)", "onValueChanged", "Action&lt;bool&gt;", "値が変わった直後に新しいboolを受け取る処理です。"],
    ["Label(string title, Sprite icon)", "title / icon", "string / Sprite", "文字とSpriteアイコンを横に並べます。"],
    ["Label(string title, UIElement icon)", "icon", "UIElement", "Imageや図形など、任意の要素をアイコンとして使います。"]
  ],
  layout: [
    ["VStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "content", "Action", "中に並べる要素を書く場所です。"],
    ["VStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "states", "State[]", "中身を作り直すきっかけになるStateです。不要なら省略できます。"],
    ["VStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "spacing", "float", "子要素同士の間隔です。"],
    ["VStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "alignment", "VStackAlignment", "横方向の揃えです。Leading、Center、Trailingなど。"],
    ["LazyVStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "content / states", "Action / State[]", "Build時やState変更時に縦Stackの子要素を作ります。大量表示や遅延生成向けです。"],
    ["LazyHStack(Action content, State[] states, float spacing, HStackAlignment alignment)", "content / states", "Action / State[]", "Build時やState変更時に横Stackの子要素を作ります。"],
    ["HStack(Action content, State[] states, float spacing, HStackAlignment alignment)", "alignment", "HStackAlignment", "縦方向の揃えです。Top、Center、Bottom、FirstTextBaselineなど。"],
    ["ZStack(Action content, State[] states, ZStackAlignment alignment)", "alignment", "ZStackAlignment", "重ねる子要素をどこに寄せるかです。"],
    ["Grid(Action content, State[] states, float horizontalSpacing, float verticalSpacing, HStackAlignment rowAlignment)", "horizontalSpacing", "float", "列同士の間隔です。"],
    ["Grid(Action content, State[] states, float horizontalSpacing, float verticalSpacing, HStackAlignment rowAlignment)", "verticalSpacing", "float", "行同士の間隔です。"],
    ["Grid(Action content, State[] states, float horizontalSpacing, float verticalSpacing, HStackAlignment rowAlignment)", "rowAlignment", "HStackAlignment", "各行の縦方向の揃えです。"],
    ["GridRow(Action content)", "content", "Action", "Grid内で1行分の要素を宣言します。"],
    ["GeometryReader(Func&lt;GeometryProxy, UIElement&gt; content)", "content", "Func&lt;GeometryProxy, UIElement&gt;", "親から提案されたサイズを受け取り、返したUIElementを配置します。"],
    ["Spacer(float minLength)", "minLength", "float", "Spacerが最低限確保する長さです。"],
    ["ScrollView(Action content, State[] states, bool horizontal, bool vertical)", "content", "Action", "スクロール内に置く要素です。"],
    ["ScrollView(Action content, State[] states, bool horizontal, bool vertical)", "states", "State[]", "スクロール内容を作り直すきっかけです。"],
    ["ScrollView(Action content, State[] states, bool horizontal, bool vertical)", "horizontal / vertical", "bool", "横/縦スクロールを有効にするかです。"],
    ["ForEach(int fromInclusive, int toInclusive, Action&lt;int&gt; content)", "fromInclusive / toInclusive", "int", "繰り返す範囲です。両端を含みます。"],
    ["ForEach(Range range, Action&lt;int&gt; content)", "range", "Range", "0..10 のようなC# Rangeです。こちらは終端を含みません。"],
    ["ForEach&lt;T&gt;(IEnumerable&lt;T&gt; data, Action&lt;T&gt; content)", "data", "IEnumerable&lt;T&gt;", "配列やListなどをそのまま繰り返します。"]
  ],
  shapes: [
    ["Image(Sprite sprite)", "sprite", "Sprite", "表示する画像です。Resources.LoadやInspector参照などで渡します。"],
    ["Rectangle(Color color)", "color", "Color", "矩形の塗り色です。省略時は白です。"],
    ["Color(Color color)", "color", "Color", "指定色で塗ったRectangleElementを作ります。"],
    ["Circle(Color color)", "color", "Color", "円の塗り色です。省略時は白です。"],
    ["Capsule(Color color)", "color", "Color", "カプセル形状の塗り色です。省略時は白です。"],
    ["RoundedRectangle(float cornerRadius, Color color)", "cornerRadius", "float", "角丸の半径です。"],
    ["RoundedRectangle(float cornerRadius, Color color)", "color", "Color", "角丸矩形の塗り色です。省略時は白です。"],
    ["Divider(Color color, float thickness)", "color", "Color", "区切り線の色です。"],
    ["Divider(Color color, float thickness)", "thickness", "float", "区切り線の太さです。"]
  ],
  input: [
    ["TextField(string title, State&lt;string&gt; text, TextElement prompt)", "title", "string", "入力欄の識別用タイトルです。"],
    ["TextField(string title, State&lt;string&gt; text, TextElement prompt)", "text", "State&lt;string&gt;", "入力値です。ユーザー入力で値も更新されます。"],
    ["TextField(string title, State&lt;string&gt; text, TextElement prompt)", "prompt", "TextElement", "未入力時に出るplaceholderです。Textのmodifierで装飾できます。"],
    ["TextField(..., Action&lt;string&gt; onTextChanged)", "onTextChanged", "Action&lt;string&gt;", "入力が変わるたびに現在の文字列を受け取ります。"],
    ["TextField(string title, State&lt;string&gt; text, Axis axis, TextElement prompt)", "axis", "Axis", "Verticalにすると複数行入力として扱います。"],
    ["SecureField(string title, State&lt;string&gt; text, TextElement prompt)", "text", "State&lt;string&gt;", "パスワード文字列です。表示は伏せられます。"],
    ["SecureField(..., Action&lt;string&gt; onTextChanged)", "onTextChanged", "Action&lt;string&gt;", "パスワード入力が変わったときに呼ばれます。"],
    ["TextEditor(State&lt;string&gt; text)", "text", "State&lt;string&gt;", "複数行編集用の文字列です。"],
    ["TextEditor(State&lt;string&gt; text, Action&lt;string&gt; onTextChanged)", "onTextChanged", "Action&lt;string&gt;", "複数行テキストが変わったときに呼ばれます。"]
  ],
  media: [
    ["ProgressView(State&lt;float&gt; value, float total)", "value", "State&lt;float&gt;", "現在値です。"],
    ["ProgressView(State&lt;float&gt; value, float total)", "total", "float", "最大値です。value / total が進捗になります。"],
    ["Stepper(State&lt;int&gt; value, int minValue, int maxValue, int step)", "value", "State&lt;int&gt;", "増減する整数値です。"],
    ["Stepper(State&lt;int&gt; value, int minValue, int maxValue, int step)", "minValue / maxValue", "int", "値の下限/上限です。"],
    ["Stepper(State&lt;int&gt; value, int minValue, int maxValue, int step)", "step", "int", "1回の操作で増減する量です。"],
    ["Stepper(string label, ...)", "label", "string", "Stepper横に表示する文字です。"],
    ["Slider(State&lt;float&gt; value, float minValue, float maxValue)", "value", "State&lt;float&gt;", "現在値です。ユーザー操作で値も更新されます。"],
    ["Slider(State&lt;int&gt; value, float minValue, float maxValue)", "value", "State&lt;int&gt;", "整数値として扱うSliderです。"],
    ["Slider(State&lt;float&gt; value, float minValue, float maxValue)", "minValue / maxValue", "float", "スライダーの下限/上限です。"],
    ["Picker(State&lt;int&gt; selection, params string[] options)", "selection", "State&lt;int&gt;", "選択中のindexです。"],
    ["Picker(State&lt;int&gt; selection, params string[] options)", "options", "string[]", "表示する選択肢です。"],
    ["TabView(Action content, State&lt;int&gt; selectedIndex)", "content", "Action", "中にTab(...)を並べます。"],
    ["TabView(Action content, State&lt;int&gt; selectedIndex)", "selectedIndex", "State&lt;int&gt;", "選択中タブのindexです。省略時は内部状態で管理します。"],
    ["Tab(string title, Action content)", "title", "string", "タブ見出しの文字です。"],
    ["Tab(Action titleContent, Action content)", "titleContent", "Action", "タブ見出しを任意のUIで作るための処理です。"],
    ["Tab(..., Action content)", "content", "Action", "そのタブを選択したときに表示する中身です。"]
  ]
};

const factoryParametersEn = {
  basics: [
    ["Text(string text)", "text", "string", "Static text to display."],
    ["Text(string text, State[] dependencyStates)", "dependencyStates", "State[]", "States that rebuild the Text even when the literal string is fixed."],
    ["Text(State&lt;string&gt; text)", "text", "State&lt;string&gt;", "Displays the State value and updates when it changes."],
    ["Text(State&lt;string&gt; text, State[] additionalStates)", "additionalStates", "State[]", "Extra states that should also trigger refresh."],
    ["Text(Func&lt;string&gt; content, State[] dependencyStates)", "content", "Func&lt;string&gt;", "Function that computes the visible string."],
    ["Text(Func&lt;string&gt; content, State[] dependencyStates)", "dependencyStates", "State[]", "States that cause the text to recompute."],
    ["Button(string label, Action onClick)", "label", "string", "Text shown inside the button."],
    ["Button(string label, Action onClick)", "onClick", "Action", "Code to run when the button is clicked."],
    ["Button(Action action, string label)", "action / label", "Action / string", "Action-first overload with the same meaning as Button(label, onClick)."],
    ["Button(UIElement content, Action onClick)", "content", "UIElement", "Custom label content, such as Text or HStack."],
    ["Button(Action action, UIElement label)", "action / label", "Action / UIElement", "Action-first overload for custom label content."],
    ["Toggle(string title, State&lt;bool&gt; isOn)", "title", "string", "Label shown next to the toggle."],
    ["Toggle(string title, State&lt;bool&gt; isOn)", "isOn", "State&lt;bool&gt;", "On/off value. User interaction writes back to it."],
    ["Toggle(..., Action&lt;bool&gt; onValueChanged)", "onValueChanged", "Action&lt;bool&gt;", "Callback that receives the new bool after a change."],
    ["Label(string title, Sprite icon)", "title / icon", "string / Sprite", "Places text beside a Sprite icon."],
    ["Label(string title, UIElement icon)", "icon", "UIElement", "Uses any element, such as Image or a shape, as the icon."]
  ],
  layout: [
    ["VStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "content", "Action", "Where you declare child elements."],
    ["VStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "states", "State[]", "States that rebuild the stack content. Optional."],
    ["VStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "spacing", "float", "Space between child elements."],
    ["VStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "alignment", "VStackAlignment", "Horizontal alignment: Leading, Center, Trailing, and similar values."],
    ["LazyVStack(Action content, State[] states, float spacing, VStackAlignment alignment)", "content / states", "Action / State[]", "Creates vertical stack children at build time or state refresh time."],
    ["LazyHStack(Action content, State[] states, float spacing, HStackAlignment alignment)", "content / states", "Action / State[]", "Creates horizontal stack children at build time or state refresh time."],
    ["HStack(Action content, State[] states, float spacing, HStackAlignment alignment)", "alignment", "HStackAlignment", "Vertical alignment: Top, Center, Bottom, FirstTextBaseline, and similar values."],
    ["ZStack(Action content, State[] states, ZStackAlignment alignment)", "alignment", "ZStackAlignment", "Where layered children are placed."],
    ["Grid(Action content, State[] states, float horizontalSpacing, float verticalSpacing, HStackAlignment rowAlignment)", "horizontalSpacing", "float", "Space between columns."],
    ["Grid(Action content, State[] states, float horizontalSpacing, float verticalSpacing, HStackAlignment rowAlignment)", "verticalSpacing", "float", "Space between rows."],
    ["Grid(Action content, State[] states, float horizontalSpacing, float verticalSpacing, HStackAlignment rowAlignment)", "rowAlignment", "HStackAlignment", "Vertical alignment for each grid row."],
    ["GridRow(Action content)", "content", "Action", "Declares one row inside Grid content."],
    ["GeometryReader(Func&lt;GeometryProxy, UIElement&gt; content)", "content", "Func&lt;GeometryProxy, UIElement&gt;", "Receives the proposed parent size and returns the element to place."],
    ["Spacer(float minLength)", "minLength", "float", "Minimum length the spacer should reserve."],
    ["ScrollView(Action content, State[] states, bool horizontal, bool vertical)", "content", "Action", "Elements placed inside the scroll view."],
    ["ScrollView(Action content, State[] states, bool horizontal, bool vertical)", "states", "State[]", "States that rebuild scroll content."],
    ["ScrollView(Action content, State[] states, bool horizontal, bool vertical)", "horizontal / vertical", "bool", "Enables horizontal and/or vertical scrolling."],
    ["ForEach(int fromInclusive, int toInclusive, Action&lt;int&gt; content)", "fromInclusive / toInclusive", "int", "Inclusive integer range. Both ends are included."],
    ["ForEach(Range range, Action&lt;int&gt; content)", "range", "Range", "C# range such as 0..10. The end is excluded."],
    ["ForEach&lt;T&gt;(IEnumerable&lt;T&gt; data, Action&lt;T&gt; content)", "data", "IEnumerable&lt;T&gt;", "Iterates arrays, lists, and other enumerable data directly."]
  ],
  shapes: [
    ["Image(Sprite sprite)", "sprite", "Sprite", "Sprite to display."],
    ["Rectangle(Color color)", "color", "Color", "Fill color. Defaults to white."],
    ["Color(Color color)", "color", "Color", "Creates a RectangleElement filled with the color."],
    ["Circle(Color color)", "color", "Color", "Circle fill color. Defaults to white."],
    ["Capsule(Color color)", "color", "Color", "Capsule fill color. Defaults to white."],
    ["RoundedRectangle(float cornerRadius, Color color)", "cornerRadius", "float", "Corner radius."],
    ["RoundedRectangle(float cornerRadius, Color color)", "color", "Color", "Fill color. Defaults to white."],
    ["Divider(Color color, float thickness)", "color", "Color", "Divider color."],
    ["Divider(Color color, float thickness)", "thickness", "float", "Divider thickness."]
  ],
  input: [
    ["TextField(string title, State&lt;string&gt; text, TextElement prompt)", "title", "string", "Identifier/title for the input field."],
    ["TextField(string title, State&lt;string&gt; text, TextElement prompt)", "text", "State&lt;string&gt;", "Input value. User edits write back to it."],
    ["TextField(string title, State&lt;string&gt; text, TextElement prompt)", "prompt", "TextElement", "Placeholder shown when empty. Style it with Text modifiers."],
    ["TextField(..., Action&lt;string&gt; onTextChanged)", "onTextChanged", "Action&lt;string&gt;", "Called with the current string whenever input changes."],
    ["TextField(string title, State&lt;string&gt; text, Axis axis, TextElement prompt)", "axis", "Axis", "Use Vertical for multiline input."],
    ["SecureField(string title, State&lt;string&gt; text, TextElement prompt)", "text", "State&lt;string&gt;", "Password value. Displayed as hidden input."],
    ["SecureField(..., Action&lt;string&gt; onTextChanged)", "onTextChanged", "Action&lt;string&gt;", "Called when password input changes."],
    ["TextEditor(State&lt;string&gt; text)", "text", "State&lt;string&gt;", "Multiline editable text."],
    ["TextEditor(State&lt;string&gt; text, Action&lt;string&gt; onTextChanged)", "onTextChanged", "Action&lt;string&gt;", "Called when multiline text changes."]
  ],
  media: [
    ["ProgressView(State&lt;float&gt; value, float total)", "value", "State&lt;float&gt;", "Current progress value."],
    ["ProgressView(State&lt;float&gt; value, float total)", "total", "float", "Maximum progress. value / total becomes the fill amount."],
    ["Stepper(State&lt;int&gt; value, int minValue, int maxValue, int step)", "value", "State&lt;int&gt;", "Integer value to increment or decrement."],
    ["Stepper(State&lt;int&gt; value, int minValue, int maxValue, int step)", "minValue / maxValue", "int", "Lower and upper bounds."],
    ["Stepper(State&lt;int&gt; value, int minValue, int maxValue, int step)", "step", "int", "Amount changed per action."],
    ["Stepper(string label, ...)", "label", "string", "Label shown beside the stepper."],
    ["Slider(State&lt;float&gt; value, float minValue, float maxValue)", "value", "State&lt;float&gt;", "Current slider value. User interaction writes back to it."],
    ["Slider(State&lt;int&gt; value, float minValue, float maxValue)", "value", "State&lt;int&gt;", "Slider value stored as an integer."],
    ["Slider(State&lt;float&gt; value, float minValue, float maxValue)", "minValue / maxValue", "float", "Slider lower and upper bounds."],
    ["Picker(State&lt;int&gt; selection, params string[] options)", "selection", "State&lt;int&gt;", "Selected option index."],
    ["Picker(State&lt;int&gt; selection, params string[] options)", "options", "string[]", "Visible option labels."],
    ["TabView(Action content, State&lt;int&gt; selectedIndex)", "content", "Action", "Declare Tab(...) calls inside."],
    ["TabView(Action content, State&lt;int&gt; selectedIndex)", "selectedIndex", "State&lt;int&gt;", "Selected tab index. Omit it to use internal state."],
    ["Tab(string title, Action content)", "title", "string", "Visible tab title."],
    ["Tab(Action titleContent, Action content)", "titleContent", "Action", "Builds a custom tab title UI."],
    ["Tab(..., Action content)", "content", "Action", "Content shown when this tab is selected."]
  ]
};

const modifierParametersJa = {
  layout: [
    ["frame(width:height:)", "width / height", "float?", "指定した軸の推奨サイズです。nullならその軸は指定しません。"],
    ["frame(State&lt;float&gt; width, State&lt;float&gt; height)", "width / height", "State&lt;float&gt;", "Stateに合わせてサイズを更新します。"],
    ["frame(infiniteWidth:infiniteHeight:)", "infiniteWidth / infiniteHeight", "bool?", "親の空き領域をできるだけ使うかどうかです。"],
    ["frame(minWidth:maxWidth:minHeight:maxHeight:)", "minWidth / maxWidth / minHeight / maxHeight", "float?", "伸縮できる範囲の下限/上限です。"],
    ["layoutPriority(float priority)", "priority", "float", "Stack内で余白を配分するときの優先度です。大きいほど優先されます。"],
    ["padding(int padding)", "padding", "int", "上下左右すべてに入れる余白です。"],
    ["padding(State&lt;int&gt; padding)", "padding", "State&lt;int&gt;", "Stateに合わせて余白を更新します。"],
    ["padding(RectOffset padding)", "padding", "RectOffset", "left/right/top/bottomをまとめて指定します。"],
    ["padding(Edge edges, int length)", "edges", "Edge", "余白を入れる方向です。Horizontal、Vertical、Allなど。"],
    ["padding(Edge edges, int length)", "length", "int", "余白のピクセル量です。"],
    ["padding(top:bottom:left:right:)", "top / bottom / left / right", "float?", "必要な辺だけ個別に余白を指定します。"],
    ["fixedSize(horizontal:vertical:)", "horizontal / vertical", "bool", "その軸で内容サイズを優先するかです。"],
    ["aspectRatio(float ratio, AspectRatioContentMode contentMode)", "ratio", "float", "幅 ÷ 高さの比率です。2なら横長、1なら正方形です。"],
    ["aspectRatio(float ratio, AspectRatioContentMode contentMode)", "contentMode", "AspectRatioContentMode", "Fitは収める、Fillは埋める方向で扱います。"]
  ],
  visual: [
    ["background(Color color)", "color", "Color / State&lt;Color&gt;", "背景色です。Stateを渡すと色変更に追従します。"],
    ["background(UIElement background, ZStackAlignment alignment)", "background", "UIElement", "背景として置くViewです。CapsuleやRoundedRectangleなど。"],
    ["background(UIElement background, ZStackAlignment alignment)", "alignment", "ZStackAlignment", "背景Viewの配置です。通常はCenterで十分です。"],
    ["overlay(UIElement overlay, ZStackAlignment alignment)", "overlay", "UIElement", "前面に重ねるViewです。バッジや装飾に使います。"],
    ["overlay(UIElement overlay, ZStackAlignment alignment)", "alignment", "ZStackAlignment", "重ねる位置です。TopTrailingなど。"],
    ["border(Color color, float width)", "color", "Color / State&lt;Color&gt;", "枠線の色です。"],
    ["border(Color color, float width)", "width", "float", "枠線の太さです。"],
    ["foregroundColor(Color color)", "color", "Color / State&lt;Color&gt;", "Textや対応Viewの前景色です。"],
    ["tint(Color color)", "color", "Color / State&lt;Color&gt;", "Controlやtemplate Imageのアクセント色です。"],
    ["opacity(float opacity)", "opacity", "float / State&lt;float&gt;", "0が透明、1が不透明です。"],
    ["fontSize(float size)", "size", "float", "Textや対応Viewの文字サイズです。"],
    ["font(TMP_FontAsset font)", "font", "TMP_FontAsset", "TextMesh Proのフォントアセットです。"],
    ["lineLimit(int? limit)", "limit", "int?", "表示する最大行数です。nullなら制限しません。"],
    ["multilineTextAlignment(TextAlignmentOptions alignment)", "alignment", "TextAlignmentOptions", "複数行テキストの揃えです。"],
    ["cornerRadius(float radius)", "radius", "float / State&lt;float&gt;", "角丸の半径です。"],
    ["cornerRadius(topLeft, topRight, bottomRight, bottomLeft)", "各corner", "float", "四隅の角丸を個別に指定します。"],
    ["cornerRadius(RectCorner corners, float radius)", "corners / radius", "RectCorner / float", "指定した角だけ丸めます。"],
    ["shadow(Color? color, float radius, float x, float y)", "color", "Color?", "影の色です。nullなら標準色を使います。"],
    ["shadow(Color? color, float radius, float x, float y)", "radius / x / y", "float", "ぼかし量とオフセットです。"],
    ["clipped()", "-", "-", "要素を矩形でクリップします。"],
    ["clipShape(UniftUIClipShape shape, float cornerRadius)", "shape", "UniftUIClipShape", "Rectangle、RoundedRectangle、Circle、Capsuleから選びます。"],
    ["clipShape(UniftUIClipShape shape, float cornerRadius)", "cornerRadius", "float", "RoundedRectangleで使う角丸量です。"],
    ["resizable(ImageResizingMode resizingMode)", "resizingMode", "ImageResizingMode", "StretchまたはTileでImageのリサイズ方法を指定します。"],
    ["renderingMode(ImageRenderingMode mode)", "mode", "ImageRenderingMode", "OriginalまたはTemplateを指定します。Templateはtintで着色できます。"],
    ["pickerStyle(PickerStyle style)", "style", "PickerStyle", "Pickerの表示スタイルです。"]
  ],
  behavior: [
    ["disabled(bool disabled)", "disabled", "bool / State&lt;bool&gt;", "trueなら操作不可にします。"],
    ["hidden(bool hidden)", "hidden", "bool", "trueなら非表示にします。"],
    ["allowsHitTesting(bool enabled)", "enabled", "bool / State&lt;bool&gt;", "クリックやタップを受けるかどうかです。"],
    ["onAppear(Action action)", "action", "Action", "Build後に一度実行する処理です。"],
    ["onAppear(Func&lt;Task&gt; action)", "action", "Func&lt;Task&gt;", "非同期で実行するonAppear処理です。"],
    ["update(Action action)", "action", "Action", "フレーム更新で実行する処理です。"],
    ["onChange(State state, Action action)", "state", "State", "監視するStateです。"],
    ["onChange(State state, Action action)", "action", "Action", "State変更時に呼ばれる処理です。"],
    ["onChange(State&lt;T&gt; state, Action&lt;T&gt; action)", "action", "Action&lt;T&gt;", "変更後の値を受け取る処理です。"],
    ["animation(float duration)", "duration", "float", "標準イージングで動かす時間です。"],
    ["animation(AnimationEasing easing, float duration)", "easing / duration", "AnimationEasing / float", "イージング種類と時間です。"],
    ["animation(Animation animation, State value)", "animation", "Animation", "使うアニメーションです。"],
    ["animation(Animation animation, State value)", "value", "State", "このStateが変わったときに動かします。"],
    ["animation(Animation animation, State value0, State value1)", "value0 / value1", "State", "複数Stateのどちらが変わっても動かします。"]
  ],
  input: [
    ["contentMargins(float horizontal, float vertical)", "horizontal / vertical", "float", "TextField内側の左右/上下余白です。"],
    ["contentMargins(float left, float right, float top, float bottom)", "left / right / top / bottom", "float", "TextField内側の余白を辺ごとに指定します。"],
    ["focused(State&lt;bool&gt; isFocused)", "isFocused", "State&lt;bool&gt;", "TextFieldのフォーカス状態と同期します。"],
    ["onEditingChanged(Action&lt;bool&gt; action)", "action", "Action&lt;bool&gt;", "編集開始/終了をboolで受け取ります。"],
    ["onSubmit(Action&lt;string&gt; action)", "action", "Action&lt;string&gt;", "送信時に現在の入力文字列を受け取ります。"],
    ["selectAllOnFocus(bool enabled)", "enabled", "bool", "フォーカス時に全選択するかです。"],
    ["textSelectionColor(Color color)", "color", "Color / State&lt;Color&gt;", "選択範囲の色です。"],
    ["caretColor(Color color)", "color", "Color / State&lt;Color&gt;", "カーソル色です。"],
    ["caretWidth(int width)", "width", "int", "カーソル幅です。"],
    ["caretBlinkRate(float rate)", "rate", "float", "カーソル点滅速度です。"],
    ["textContentType(TMP_InputField.ContentType type)", "type", "TMP_InputField.ContentType", "Password、EmailAddressなどの入力用途です。"],
    ["textInputLimit(int limit)", "limit", "int", "入力できる最大文字数です。"],
    ["keyboardType(TouchScreenKeyboardType type)", "type", "TouchScreenKeyboardType", "モバイルで出すキーボード種別です。"]
  ],
  transform: [
    ["offset(float x, float y)", "x / y", "float", "現在位置からずらす量です。"],
    ["offset(Vector2 offset)", "offset", "Vector2 / State&lt;Vector2&gt;", "2Dベクトルで指定するオフセットです。"],
    ["position(float x, float y)", "x / y", "float / State&lt;float&gt;", "親内での絶対配置位置です。"],
    ["rotationEffect(float degrees)", "degrees", "float / State&lt;float&gt;", "Z軸回転の角度です。"],
    ["rotationEffect(float x, float y, float z)", "x / y / z", "float / State&lt;float&gt;", "各軸の回転角度です。"],
    ["rotationEffect(State&lt;Vector3&gt; euler)", "euler", "State&lt;Vector3&gt;", "Stateでまとめて回転角を管理します。"],
    ["scaleEffect(float scale)", "scale", "float / State&lt;float&gt;", "全軸に同じ倍率をかけます。"],
    ["scaleEffect(float x, float y, float z)", "x / y / z", "float / State&lt;float&gt;", "軸ごとの拡大率です。"],
    ["scaleEffect(Vector3 scale)", "scale", "Vector3 / State&lt;Vector3&gt;", "3Dベクトルで拡大率を指定します。"]
  ],
  scroll: [
    ["scrollBounce(bool elastic)", "elastic", "bool", "端でバウンドするElastic挙動にするかです。"],
    ["scrollSensitivity(float sensitivity)", "sensitivity", "float", "ホイール/ドラッグ入力の感度です。"],
    ["scrollMovementType(ScrollRect.MovementType type)", "type", "ScrollRect.MovementType", "Elastic、Clamped、Unrestrictedなどの移動制限です。"],
    ["scrollPositionY(State&lt;float&gt; normalized, bool twoWay)", "normalized", "State&lt;float&gt;", "縦スクロール位置です。0から1の正規化値です。"],
    ["scrollPositionY/X(..., bool twoWay)", "twoWay", "bool", "trueならユーザー操作もStateへ書き戻します。"],
    ["scrollPositionX(State&lt;float&gt; normalized, bool twoWay)", "normalized", "State&lt;float&gt;", "横スクロール位置です。0から1の正規化値です。"],
    ["scrollIndicators(ScrollIndicatorVisibility visibility)", "visibility", "ScrollIndicatorVisibility", "スクロールバーを表示する条件です。"],
    ["scrollIndicators(ScrollIndicatorVisibility visibility, UniftUIScrollAxis axes)", "axes", "UniftUIScrollAxis", "縦/横/両方のどの軸に適用するかです。"]
  ],
  styles: [
    ["buttonStyle(IButtonStyle style)", "style", "IButtonStyle", "ButtonStyles.Filled/Plainなどで作ったButtonスタイルです。"],
    ["textFieldStyle(ITextFieldStyle style)", "style", "ITextFieldStyle", "TextFieldStyles.RoundedBorder/Plain/Chromeなどで作ったTextFieldスタイルです。"],
    ["ButtonStyles.Filled(Color backgroundColor, Color foregroundColor, float cornerRadius)", "backgroundColor / foregroundColor", "Color", "塗り色と文字色です。"],
    ["ButtonStyles.Filled(..., float cornerRadius)", "cornerRadius", "float", "ボタン背景の角丸です。"],
    ["ButtonStyles.Plain(Color foregroundColor)", "foregroundColor", "Color", "背景なしボタンの文字色です。"],
    ["TextFieldStyles.RoundedBorder(...)", "backgroundColor / focusedBackgroundColor", "Color", "通常時/フォーカス時の背景色です。"],
    ["TextFieldStyles.RoundedBorder(...)", "textColor / tintColor", "Color", "入力文字色とアクセント色です。"],
    ["TextFieldStyles.Chrome(...)", "contentMargins", "Vector4?", "left/right/top/bottomの内側余白です。"],
    ["TextFieldStyles.Chrome(...)", "caretWidth / caretBlinkRate", "int? / float?", "カーソル幅と点滅速度です。"]
  ],
  animations: [
    ["WithAnimation(Animation animation, Action changes)", "animation", "Animation", "changes内のState変更に使うアニメーションです。"],
    ["WithAnimation(Animation animation, Action changes)", "changes", "Action", "アニメーションさせたいState変更をまとめて書きます。"],
    ["WithAnimation(Action changes)", "changes", "Action", "標準アニメーションで実行するState変更です。"],
    ["Animation.linear(float duration)", "duration", "float", "一定速度で動く時間です。"],
    ["Animation.easeIn/easeOut/easeInOut(float duration)", "duration", "float", "各イージングで動く時間です。"],
    ["Animation.spring(float response, float dampingFraction)", "response", "float", "反応速度です。小さいほど速く反応します。"],
    ["Animation.spring(float response, float dampingFraction)", "dampingFraction", "float", "減衰量です。小さいほど弾みます。"],
    ["Animation.interactiveSpring(float response, float dampingFraction)", "response / dampingFraction", "float", "操作追従向けのばね設定です。"],
    ["Animation.bouncy(float duration)", "duration", "float", "弾むプリセットの時間です。"]
  ]
};

const modifierParametersEn = {
  layout: [
    ["frame(width:height:)", "width / height", "float?", "Preferred size for each axis. null leaves that axis unspecified."],
    ["frame(State&lt;float&gt; width, State&lt;float&gt; height)", "width / height", "State&lt;float&gt;", "Updates size from State values."],
    ["frame(infiniteWidth:infiniteHeight:)", "infiniteWidth / infiniteHeight", "bool?", "Whether the view should use available parent space."],
    ["frame(minWidth:maxWidth:minHeight:maxHeight:)", "minWidth / maxWidth / minHeight / maxHeight", "float?", "Lower and upper flexible size bounds."],
    ["layoutPriority(float priority)", "priority", "float", "Priority used when stack space is distributed. Larger values win more space."],
    ["padding(int padding)", "padding", "int", "Padding applied to all edges."],
    ["padding(State&lt;int&gt; padding)", "padding", "State&lt;int&gt;", "Reactive padding amount."],
    ["padding(RectOffset padding)", "padding", "RectOffset", "left/right/top/bottom padding in one value."],
    ["padding(Edge edges, int length)", "edges", "Edge", "Edges that receive padding: Horizontal, Vertical, All, and similar values."],
    ["padding(Edge edges, int length)", "length", "int", "Padding amount in pixels."],
    ["padding(top:bottom:left:right:)", "top / bottom / left / right", "float?", "Set only the edges you need."],
    ["fixedSize(horizontal:vertical:)", "horizontal / vertical", "bool", "Whether that axis should prefer intrinsic content size."],
    ["aspectRatio(float ratio, AspectRatioContentMode contentMode)", "ratio", "float", "Width divided by height. 2 is wide, 1 is square."],
    ["aspectRatio(float ratio, AspectRatioContentMode contentMode)", "contentMode", "AspectRatioContentMode", "Fit contains; Fill covers."]
  ],
  visual: [
    ["background(Color color)", "color", "Color / State&lt;Color&gt;", "Background color. State makes it reactive."],
    ["background(UIElement background, ZStackAlignment alignment)", "background", "UIElement", "View placed behind this element, such as Capsule or RoundedRectangle."],
    ["background(UIElement background, ZStackAlignment alignment)", "alignment", "ZStackAlignment", "Placement for the background view. Center is common."],
    ["overlay(UIElement overlay, ZStackAlignment alignment)", "overlay", "UIElement", "View placed above this element, often for badges or decoration."],
    ["overlay(UIElement overlay, ZStackAlignment alignment)", "alignment", "ZStackAlignment", "Placement for the overlay, such as TopTrailing."],
    ["border(Color color, float width)", "color", "Color / State&lt;Color&gt;", "Border color."],
    ["border(Color color, float width)", "width", "float", "Border thickness."],
    ["foregroundColor(Color color)", "color", "Color / State&lt;Color&gt;", "Foreground color for Text and supported views."],
    ["tint(Color color)", "color", "Color / State&lt;Color&gt;", "Accent color for controls and template images."],
    ["opacity(float opacity)", "opacity", "float / State&lt;float&gt;", "0 is transparent, 1 is opaque."],
    ["fontSize(float size)", "size", "float", "Text size for Text and supported views."],
    ["font(TMP_FontAsset font)", "font", "TMP_FontAsset", "TextMesh Pro font asset."],
    ["lineLimit(int? limit)", "limit", "int?", "Maximum visible line count. null means unlimited."],
    ["multilineTextAlignment(TextAlignmentOptions alignment)", "alignment", "TextAlignmentOptions", "Alignment for multiline text."],
    ["cornerRadius(float radius)", "radius", "float / State&lt;float&gt;", "Corner radius."],
    ["cornerRadius(topLeft, topRight, bottomRight, bottomLeft)", "each corner", "float", "Sets each corner radius individually."],
    ["cornerRadius(RectCorner corners, float radius)", "corners / radius", "RectCorner / float", "Rounds only the selected corners."],
    ["shadow(Color? color, float radius, float x, float y)", "color", "Color?", "Shadow color. null uses the default."],
    ["shadow(Color? color, float radius, float x, float y)", "radius / x / y", "float", "Blur radius and offset."],
    ["clipped()", "-", "-", "Clips the element to a rectangle."],
    ["clipShape(UniftUIClipShape shape, float cornerRadius)", "shape", "UniftUIClipShape", "Rectangle, RoundedRectangle, Circle, or Capsule."],
    ["clipShape(UniftUIClipShape shape, float cornerRadius)", "cornerRadius", "float", "Radius used by RoundedRectangle."],
    ["resizable(ImageResizingMode resizingMode)", "resizingMode", "ImageResizingMode", "Stretch or Tile resizing for Image."],
    ["renderingMode(ImageRenderingMode mode)", "mode", "ImageRenderingMode", "Original or Template. Template can be tinted."],
    ["pickerStyle(PickerStyle style)", "style", "PickerStyle", "Picker display style."]
  ],
  behavior: [
    ["disabled(bool disabled)", "disabled", "bool / State&lt;bool&gt;", "true disables interaction."],
    ["hidden(bool hidden)", "hidden", "bool", "true hides the view."],
    ["allowsHitTesting(bool enabled)", "enabled", "bool / State&lt;bool&gt;", "Whether the view receives pointer input."],
    ["onAppear(Action action)", "action", "Action", "Runs once after build."],
    ["onAppear(Func&lt;Task&gt; action)", "action", "Func&lt;Task&gt;", "Async onAppear action."],
    ["update(Action action)", "action", "Action", "Runs during frame updates."],
    ["onChange(State state, Action action)", "state", "State", "State to observe."],
    ["onChange(State state, Action action)", "action", "Action", "Callback invoked after the State changes."],
    ["onChange(State&lt;T&gt; state, Action&lt;T&gt; action)", "action", "Action&lt;T&gt;", "Callback that receives the changed value."],
    ["animation(float duration)", "duration", "float", "Duration using the default easing."],
    ["animation(AnimationEasing easing, float duration)", "easing / duration", "AnimationEasing / float", "Easing type and duration."],
    ["animation(Animation animation, State value)", "animation", "Animation", "Animation to use."],
    ["animation(Animation animation, State value)", "value", "State", "State whose changes trigger animation."],
    ["animation(Animation animation, State value0, State value1)", "value0 / value1", "State", "Animates when either state changes."]
  ],
  input: [
    ["contentMargins(float horizontal, float vertical)", "horizontal / vertical", "float", "TextField horizontal and vertical inset."],
    ["contentMargins(float left, float right, float top, float bottom)", "left / right / top / bottom", "float", "TextField inset per edge."],
    ["focused(State&lt;bool&gt; isFocused)", "isFocused", "State&lt;bool&gt;", "Synchronizes TextField focus."],
    ["onEditingChanged(Action&lt;bool&gt; action)", "action", "Action&lt;bool&gt;", "Receives true/false as editing begins or ends."],
    ["onSubmit(Action&lt;string&gt; action)", "action", "Action&lt;string&gt;", "Receives current text when submitted."],
    ["selectAllOnFocus(bool enabled)", "enabled", "bool", "Whether focus selects all text."],
    ["textSelectionColor(Color color)", "color", "Color / State&lt;Color&gt;", "Text selection highlight color."],
    ["caretColor(Color color)", "color", "Color / State&lt;Color&gt;", "Caret color."],
    ["caretWidth(int width)", "width", "int", "Caret width."],
    ["caretBlinkRate(float rate)", "rate", "float", "Caret blink speed."],
    ["textContentType(TMP_InputField.ContentType type)", "type", "TMP_InputField.ContentType", "Input purpose, such as Password or EmailAddress."],
    ["textInputLimit(int limit)", "limit", "int", "Maximum input length."],
    ["keyboardType(TouchScreenKeyboardType type)", "type", "TouchScreenKeyboardType", "Mobile keyboard type."]
  ],
  transform: [
    ["offset(float x, float y)", "x / y", "float", "Position delta from the current placement."],
    ["offset(Vector2 offset)", "offset", "Vector2 / State&lt;Vector2&gt;", "Offset as a 2D vector."],
    ["position(float x, float y)", "x / y", "float / State&lt;float&gt;", "Absolute position inside the parent."],
    ["rotationEffect(float degrees)", "degrees", "float / State&lt;float&gt;", "Z-axis rotation in degrees."],
    ["rotationEffect(float x, float y, float z)", "x / y / z", "float / State&lt;float&gt;", "Rotation per axis."],
    ["rotationEffect(State&lt;Vector3&gt; euler)", "euler", "State&lt;Vector3&gt;", "Reactive Euler rotation."],
    ["scaleEffect(float scale)", "scale", "float / State&lt;float&gt;", "Uniform scale."],
    ["scaleEffect(float x, float y, float z)", "x / y / z", "float / State&lt;float&gt;", "Scale per axis."],
    ["scaleEffect(Vector3 scale)", "scale", "Vector3 / State&lt;Vector3&gt;", "Scale as a 3D vector."]
  ],
  scroll: [
    ["scrollBounce(bool elastic)", "elastic", "bool", "Whether the scroll view uses elastic bounce at the edges."],
    ["scrollSensitivity(float sensitivity)", "sensitivity", "float", "Wheel/drag input sensitivity."],
    ["scrollMovementType(ScrollRect.MovementType type)", "type", "ScrollRect.MovementType", "Movement restriction: Elastic, Clamped, Unrestricted, and similar values."],
    ["scrollPositionY(State&lt;float&gt; normalized, bool twoWay)", "normalized", "State&lt;float&gt;", "Vertical scroll position from 0 to 1."],
    ["scrollPositionY/X(..., bool twoWay)", "twoWay", "bool", "When true, user scrolling writes back into State."],
    ["scrollPositionX(State&lt;float&gt; normalized, bool twoWay)", "normalized", "State&lt;float&gt;", "Horizontal scroll position from 0 to 1."],
    ["scrollIndicators(ScrollIndicatorVisibility visibility)", "visibility", "ScrollIndicatorVisibility", "When scrollbars should be visible."],
    ["scrollIndicators(ScrollIndicatorVisibility visibility, UniftUIScrollAxis axes)", "axes", "UniftUIScrollAxis", "Which axis receives the setting."]
  ],
  styles: [
    ["buttonStyle(IButtonStyle style)", "style", "IButtonStyle", "Button style from ButtonStyles.Filled/Plain."],
    ["textFieldStyle(ITextFieldStyle style)", "style", "ITextFieldStyle", "TextField style from TextFieldStyles.RoundedBorder/Plain/Chrome."],
    ["ButtonStyles.Filled(Color backgroundColor, Color foregroundColor, float cornerRadius)", "backgroundColor / foregroundColor", "Color", "Fill and text colors."],
    ["ButtonStyles.Filled(..., float cornerRadius)", "cornerRadius", "float", "Button background corner radius."],
    ["ButtonStyles.Plain(Color foregroundColor)", "foregroundColor", "Color", "Text color for a plain button."],
    ["TextFieldStyles.RoundedBorder(...)", "backgroundColor / focusedBackgroundColor", "Color", "Normal and focused background colors."],
    ["TextFieldStyles.RoundedBorder(...)", "textColor / tintColor", "Color", "Input text color and accent color."],
    ["TextFieldStyles.Chrome(...)", "contentMargins", "Vector4?", "left/right/top/bottom inset."],
    ["TextFieldStyles.Chrome(...)", "caretWidth / caretBlinkRate", "int? / float?", "Caret width and blink speed."]
  ],
  animations: [
    ["WithAnimation(Animation animation, Action changes)", "animation", "Animation", "Animation used for State changes inside changes."],
    ["WithAnimation(Animation animation, Action changes)", "changes", "Action", "State changes to animate together."],
    ["WithAnimation(Action changes)", "changes", "Action", "State changes run with the default animation."],
    ["Animation.linear(float duration)", "duration", "float", "Duration for constant-speed motion."],
    ["Animation.easeIn/easeOut/easeInOut(float duration)", "duration", "float", "Duration for each easing curve."],
    ["Animation.spring(float response, float dampingFraction)", "response", "float", "Response speed. Smaller values react faster."],
    ["Animation.spring(float response, float dampingFraction)", "dampingFraction", "float", "Damping amount. Smaller values bounce more."],
    ["Animation.interactiveSpring(float response, float dampingFraction)", "response / dampingFraction", "float", "Spring settings for interaction-driven motion."],
    ["Animation.bouncy(float duration)", "duration", "float", "Duration for the bouncy preset."]
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
}`) + sampleImage("Count: 5 と Increment ボタンを表示した Counter サンプル。")
              },
              {
                title: "読む順番",
                body: steps([
                  { title: "セットアップ", body: "Canvasに表示するところまでを確認します。" },
                  { title: "State", body: "値が変わるUIの作り方を覚えます。" },
                  { title: "Layout", body: "Stack、Spacer、frame、paddingで画面を組みます。" },
                  { title: "Controls", body: "Button、TextField、PickerなどをStateに接続します。" },
                  { title: "Reference", body: "迷ったときはAPI一覧、Factory引数、modifier引数表を引きます。" }
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
            Button("Start", () => Debug.Log("Start"));
            Button("Options", () => Debug.Log("Options"));
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
            keywords: "State Value reactive onChange binding update",
            sections: [
              {
                title: "Stateの考え方",
                body: `
                  <p><code>State&lt;T&gt;</code> は監視できる値です。<code>Value</code> を変更すると、依存しているUIが更新されます。</p>
                  ${code(`
State<int> score = new State<int>(0);

Text(() => $"Score: {score.Value}", new State[] { score });
Button("+10", () => score.Value += 10);`)}
                `
              },
              {
                title: "TextFieldと双方向更新",
                body: code(`
State<string> playerName = new State<string>("");

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
                title: "複数のStateを使う",
                body: code(`
State<int> hp = new State<int>(80);
State<int> maxHp = new State<int>(100);
State<int> selectedTab = new State<int>(0);

HStack(() =>
{
    Text(() => $"HP {hp.Value} / {maxHp.Value}", new State[] { hp, maxHp });
    Spacer();
    Button("Recover", () => hp.Value = maxHp.Value);
});

TabView(() =>
{
    Tab("Status", () => Text("Status"));
    Tab("Items", () => Text("Items"));
}, selectedTab);`)
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
string[] quests =
{
    "Find the key",
    "Open the gate",
    "Return to town"
};

ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, quests.Length - 1, i =>
        {
            Text(quests[i])
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
State<string> title = new State<string>("Status");
State<int> hp = new State<int>(100);

Text("Static");
Text(title);
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
TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");
TMP_FontAsset titleFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");

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
Color accent = new Color(0.16f, 0.38f, 0.9f);

Button("Save", () => Debug.Log("Saved"))
    .frame(infiniteWidth: true, height: 44)
    .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));`)
              },
              {
                title: "カスタムラベル",
                body: code(`
Sprite saveIcon = Resources.Load<Sprite>("Icons/Save");

Button(
    HStack(() =>
    {
        Image(saveIcon).frame(width: 18, height: 18);
        Text("Save").bold();
    }, spacing: 8f),
    () => Debug.Log("Saved"));`)
              },
              {
                title: "Stateに紐づくControl",
                body: code(`
State<bool> isEnabled = new State<bool>(true);
State<int> volume = new State<int>(50);
State<int> quantity = new State<int>(1);
State<float> downloadProgress = new State<float>(0.35f);

Toggle("Enabled", isEnabled);
Slider(volume, 0, 100);
Stepper("Quantity", quantity, 0, 9);
ProgressView(downloadProgress, total: 1);`)
              },
              {
                title: "Picker",
                body: code(`
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<int> difficulty = new State<int>(1);

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
State<string> email = new State<string>("");

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
    text: new State<string>(""),
    prompt: Text("Optional")
        .italic()
        .foregroundColor(Color.gray));`)
              },
              {
                title: "パスワードと複数行",
                body: code(`
State<string> password = new State<string>("");
State<string> notes = new State<string>("");

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
State<string> query = new State<string>("");
State<bool> searchFocused = new State<bool>(false);

TextField("Search", text: query, prompt: Text("Search"))
    .focused(searchFocused)
    .selectAllOnFocus()
    .onSubmit(value => Debug.Log($"Search: {value}"))
    .onEditingChanged(isEditing => Debug.Log(isEditing));`)
              },
              {
                title: "キャレットと選択色",
                body: code(`
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<string> name = new State<string>("");

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
Sprite avatar = Resources.Load<Sprite>("Avatars/Player");

Image(avatar)
    .resizable()
    .scaledToFill()
    .frame(width: 64, height: 64)
    .clipShape(UniftUIClipShape.Circle);`)
              },
              {
                title: "操作状態",
                body: code(`
State<bool> isSubmitting = new State<bool>(false);
State<bool> canTap = new State<bool>(true);

Button("Submit", () => Debug.Log("Submitted"))
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
Color accent = new Color(0.16f, 0.38f, 0.9f);

Button("Continue", () => Debug.Log("Continue"))
    .buttonStyle(ButtonStyles.Filled(
        backgroundColor: accent,
        foregroundColor: Color.white,
        cornerRadius: 10));

Button("Cancel", () => Debug.Log("Cancel"))
    .buttonStyle(ButtonStyles.Plain(accent));`)
              },
              {
                title: "TextFieldStyles",
                body: code(`
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<string> firstName = new State<string>("");
State<string> lastName = new State<string>("");

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
Color accent = new Color(0.16f, 0.38f, 0.9f);
Color panel = new Color(0.96f, 0.97f, 0.99f);
State<bool> soundEnabled = new State<bool>(true);

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
State<bool> expanded = new State<bool>(false);

WithAnimation(Animation.easeInOut(0.25f), () =>
{
    expanded.Value = !expanded.Value;
});`)
              },
              {
                title: "特定のStateに紐づける",
                body: code(`
State<bool> isPressed = new State<bool>(false);

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
        ForEach(0, 49, i => Text($"Row {i}").padding(10));
    });
}, horizontal: false, vertical: true)
.scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical);`)
              },
              {
                title: "スクロール位置をStateへ",
                body: code(`
State<float> scrollY = new State<float>(1f);

ScrollView(() =>
{
    LazyVStack(() => { /* rows */ });
})
.scrollPositionY(scrollY, twoWay: true);`)
              },
              {
                title: "TabView",
                body: code(`
State<int> selectedTab = new State<int>(0);

TabView(() =>
{
    Tab("Home", () => Text("Home"));
    Tab("Settings", () => Text("Settings"));
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
State<int> hp = new State<int>(100);
State<int> gold = new State<int>(0);
State<bool> menuOpen = new State<bool>(false);`)}
                `
              },
              {
                title: "一覧はLazy系を使う",
                body: code(`
string[] items = { "Potion", "Key", "Map" };

ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, items.Length - 1, i => Text(items[i]).padding(8));
    });
});`)
              },
              {
                title: "複数変更の通知をまとめる",
                body: `
                  <p><code>State.BatchUpdate()</code> は、複数の <code>State</code> を一度に変更するときの通知をまとめるための応用機能です。初期化、リセット、セーブデータ読み込みのように、値をまとめて差し替える場面で使います。</p>
                  ${code(`
State<int> hp = new State<int>(40);
State<int> mana = new State<int>(10);
State<int> selectedTab = new State<int>(2);

void ResetPlayerUi()
{
    using (State.BatchUpdate())
    {
        hp.Value = 100;
        mana.Value = 50;
        selectedTab.Value = 0;
    }
}`)}
                `
              },
              {
                title: "modifierの責務を分ける",
                body: `
                  <p>レイアウト、見た目、操作を分けて読むと、意図が追いやすくなります。</p>
                  ${code(`
State<bool> canBuy = new State<bool>(true);
Color accent = new Color(0.16f, 0.38f, 0.9f);
IButtonStyle primaryButtonStyle = ButtonStyles.Filled(accent, Color.white, 10);

Button("Buy", () => Debug.Log("Buy"))
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
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<float> storageUsed = new State<float>(0.42f);

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
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<string> email = new State<string>("");
State<string> password = new State<string>("");
ITextFieldStyle fieldStyle = TextFieldStyles.Chrome(
    backgroundColor: Color.white,
    tintColor: accent,
    cornerRadius: 8);

VStack(() =>
{
    TextField("Email", text: email, prompt: Text("email@example.com"))
        .textFieldStyle(fieldStyle);

    SecureField("Password", text: password, prompt: Text("Password"))
        .textFieldStyle(fieldStyle);

    Button("Sign in", () => Debug.Log("Sign in"))
        .frame(infiniteWidth: true, height: 44)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f, alignment: VStackAlignment.Leading);`)
              },
              {
                title: "設定行",
                body: code(`
State<bool> notifications = new State<bool>(true);

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
Color accent = new Color(0.16f, 0.38f, 0.9f);
Sprite emptyIcon = Resources.Load<Sprite>("Icons/Empty");

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

    Button("Create", () => Debug.Log("Create"))
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f)
.frame(infiniteWidth: true, infiniteHeight: true);`)
              },
              {
                title: "HUD",
                body: code(`
State<int> hp = new State<int>(100);
State<int> gold = new State<int>(250);

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
            id: "factory-parameters",
            title: "Factory引数リファレンス",
            kind: "Reference",
            summary: "Text、Button、Stack、TextFieldなど、UIを作る関数に渡す引数を名前・型・意味で引けます。",
            keywords: "factory parameters arguments Text Button VStack HStack ZStack Grid TextField SecureField Image ProgressView Slider Picker spacing alignment states prompt content",
            sections: [
              {
                title: "基本要素",
                body: `
                  <p>FactoryはUI要素を作る入口です。まずは <code>content</code> が中身、<code>State</code> が変化する値、<code>Action</code> が実行する処理だと覚えると読みやすくなります。</p>
                  ${parameterTable("ja", factoryParametersJa.basics)}
                `
              },
              {
                title: "レイアウト",
                body: `
                  <p><code>spacing</code> は子要素同士の間隔、<code>alignment</code> は揃え位置、<code>states</code> は中身を再生成するきっかけです。</p>
                  ${parameterTable("ja", factoryParametersJa.layout)}
                `
              },
              {
                title: "画像・図形",
                body: parameterTable("ja", factoryParametersJa.shapes)
              },
              {
                title: "入力",
                body: parameterTable("ja", factoryParametersJa.input)
              },
              {
                title: "画像・Control",
                body: parameterTable("ja", factoryParametersJa.media)
              }
            ],
            related: ["modifier-parameters", "api-index", "overview"]
          },
          {
            id: "modifier-parameters",
            title: "modifier引数リファレンス",
            kind: "Reference",
            summary: "modifierに渡す引数を、引数名・型・意味で引けます。",
            keywords: "modifier parameters frame padding background overlay tint opacity cornerRadius border shadow input scroll",
            sections: [
              {
                title: "レイアウト",
                body: parameterTable("ja", modifierParametersJa.layout)
              },
              {
                title: "見た目",
                body: parameterTable("ja", modifierParametersJa.visual)
              },
              {
                title: "変形",
                body: parameterTable("ja", modifierParametersJa.transform)
              },
              {
                title: "入力",
                body: parameterTable("ja", modifierParametersJa.input)
              },
              {
                title: "スクロール",
                body: parameterTable("ja", modifierParametersJa.scroll)
              },
              {
                title: "動きと操作",
                body: parameterTable("ja", modifierParametersJa.behavior)
              },
              {
                title: "Style factory",
                body: parameterTable("ja", modifierParametersJa.styles)
              },
              {
                title: "Animation factory",
                body: parameterTable("ja", modifierParametersJa.animations)
              }
            ],
            related: ["factory-parameters", "api-index", "modifiers", "text-input"]
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
            related: ["factory-parameters", "modifier-parameters", "overview", "setup"]
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
}`) + sampleImage("Counter sample showing Count: 5 and an Increment button.")
              },
              {
                title: "Recommended path",
                body: steps([
                  { title: "Install", body: "Get a view rendering into a Canvas." },
                  { title: "State", body: "Learn how values update UI." },
                  { title: "Layout", body: "Use stacks, Spacer, frame, and padding." },
                  { title: "Controls", body: "Connect buttons, fields, pickers, and sliders to State." },
                  { title: "Reference", body: "Use the API index, factory parameters, and modifier parameters when you need details." }
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
            Button("Start", () => Debug.Log("Start"));
            Button("Options", () => Debug.Log("Options"));
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
            keywords: "State Value reactive onChange binding update",
            sections: [
              {
                title: "State in one minute",
                body: `
                  <p><code>State&lt;T&gt;</code> is an observable value. Change <code>Value</code>, and dependent UI updates.</p>
                  ${code(`
State<int> score = new State<int>(0);

Text(() => $"Score: {score.Value}", new State[] { score });
Button("+10", () => score.Value += 10);`)}
                `
              },
              {
                title: "Two-way text input",
                body: code(`
State<string> playerName = new State<string>("");

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
                title: "Use several State values",
                body: code(`
State<int> hp = new State<int>(80);
State<int> maxHp = new State<int>(100);
State<int> selectedTab = new State<int>(0);

HStack(() =>
{
    Text(() => $"HP {hp.Value} / {maxHp.Value}", new State[] { hp, maxHp });
    Spacer();
    Button("Recover", () => hp.Value = maxHp.Value);
});

TabView(() =>
{
    Tab("Status", () => Text("Status"));
    Tab("Items", () => Text("Items"));
}, selectedTab);`)
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
string[] quests =
{
    "Find the key",
    "Open the gate",
    "Return to town"
};

ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, quests.Length - 1, i =>
        {
            Text(quests[i])
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
State<string> title = new State<string>("Status");
State<int> hp = new State<int>(100);

Text("Static");
Text(title);
Text(() => $"HP: {hp.Value}", new State[] { hp });`) },
              { title: "Style", body: code(`
Text("Warning")
    .fontSize(20)
    .bold()
    .foregroundColor(new Color(0.9f, 0.18f, 0.12f));`) },
              { title: "Fonts", body: code(`
TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");
TMP_FontAsset titleFont = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");

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
Color accent = new Color(0.16f, 0.38f, 0.9f);

Button("Save", () => Debug.Log("Saved"))
    .frame(infiniteWidth: true, height: 44)
    .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));`) },
              { title: "Custom button label", body: code(`
Sprite saveIcon = Resources.Load<Sprite>("Icons/Save");

Button(
    HStack(() =>
    {
        Image(saveIcon).frame(width: 18, height: 18);
        Text("Save").bold();
    }, spacing: 8f),
    () => Debug.Log("Saved"));`) },
              { title: "State-bound controls", body: code(`
State<bool> isEnabled = new State<bool>(true);
State<int> volume = new State<int>(50);
State<int> quantity = new State<int>(1);
State<float> downloadProgress = new State<float>(0.35f);

Toggle("Enabled", isEnabled);
Slider(volume, 0, 100);
Stepper("Quantity", quantity, 0, 9);
ProgressView(downloadProgress, total: 1);`) },
              { title: "Picker", body: code(`
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<int> difficulty = new State<int>(1);

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
State<string> email = new State<string>("");

TextField("Email", text: email, prompt: Text("email@example.com"))
    .lineLimit(1)
    .contentMargins(horizontal: 12, vertical: 8)
    .textContentType(TMP_InputField.ContentType.EmailAddress)
    .keyboardType(TouchScreenKeyboardType.EmailAddress);`) },
              { title: "Style the prompt", body: code(`
TextField(
    "Nickname",
    text: new State<string>(""),
    prompt: Text("Optional")
        .italic()
        .foregroundColor(Color.gray));`) },
              { title: "Password and multiline", body: code(`
State<string> password = new State<string>("");
State<string> notes = new State<string>("");

SecureField("Password", text: password, prompt: Text("Password"));

TextField("Notes", text: notes, axis: Axis.Vertical, prompt: Text("Notes"))
    .lineLimit(4)
    .frame(infiniteWidth: true, height: 100);

TextEditor(notes)
    .frame(infiniteWidth: true, height: 140);`) },
              { title: "Focus and submit", body: code(`
State<string> query = new State<string>("");
State<bool> searchFocused = new State<bool>(false);

TextField("Search", text: query, prompt: Text("Search"))
    .focused(searchFocused)
    .selectAllOnFocus()
    .onSubmit(value => Debug.Log($"Search: {value}"))
    .onEditingChanged(isEditing => Debug.Log(isEditing));`) },
              { title: "Caret and selection", body: code(`
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<string> name = new State<string>("");

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
Sprite avatar = Resources.Load<Sprite>("Avatars/Player");

Image(avatar)
    .resizable()
    .scaledToFill()
    .frame(width: 64, height: 64)
    .clipShape(UniftUIClipShape.Circle);`) },
              { title: "Interaction state", body: code(`
State<bool> isSubmitting = new State<bool>(false);
State<bool> canTap = new State<bool>(true);

Button("Submit", () => Debug.Log("Submitted"))
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
Color accent = new Color(0.16f, 0.38f, 0.9f);

Button("Continue", () => Debug.Log("Continue"))
    .buttonStyle(ButtonStyles.Filled(
        backgroundColor: accent,
        foregroundColor: Color.white,
        cornerRadius: 10));

Button("Cancel", () => Debug.Log("Cancel"))
    .buttonStyle(ButtonStyles.Plain(accent));`) },
              { title: "TextFieldStyles", body: code(`
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<string> firstName = new State<string>("");
State<string> lastName = new State<string>("");

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
Color accent = new Color(0.16f, 0.38f, 0.9f);
Color panel = new Color(0.96f, 0.97f, 0.99f);
State<bool> soundEnabled = new State<bool>(true);

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
State<bool> expanded = new State<bool>(false);

WithAnimation(Animation.easeInOut(0.25f), () =>
{
    expanded.Value = !expanded.Value;
});`) },
              { title: "Bind animation to one State", body: code(`
State<bool> isPressed = new State<bool>(false);

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
        ForEach(0, 49, i => Text($"Row {i}").padding(10));
    });
}, horizontal: false, vertical: true)
.scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical);`) },
              { title: "Bind scroll position", body: code(`
State<float> scrollY = new State<float>(1f);

ScrollView(() =>
{
    LazyVStack(() => { /* rows */ });
})
.scrollPositionY(scrollY, twoWay: true);`) },
              { title: "Tabs", body: code(`
State<int> selectedTab = new State<int>(0);

TabView(() =>
{
    Tab("Home", () => Text("Home"));
    Tab("Settings", () => Text("Settings"));
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
State<int> hp = new State<int>(100);
State<int> gold = new State<int>(0);
State<bool> menuOpen = new State<bool>(false);`)}
              ` },
              { title: "Use lazy stacks for lists", body: code(`
string[] items = { "Potion", "Key", "Map" };

ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, items.Length - 1, i => Text(items[i]).padding(8));
    });
});`) },
              { title: "Batch several changes", body: `
                <p><code>State.BatchUpdate()</code> is an advanced helper for grouping notifications when several <code>State</code> values change together. It fits reset, initialization, and save-data loading flows.</p>
                ${code(`
State<int> hp = new State<int>(40);
State<int> mana = new State<int>(10);
State<int> selectedTab = new State<int>(2);

void ResetPlayerUi()
{
    using (State.BatchUpdate())
    {
        hp.Value = 100;
        mana.Value = 50;
        selectedTab.Value = 0;
    }
}`)}
              ` },
              { title: "Separate responsibilities", body: code(`
State<bool> canBuy = new State<bool>(true);
Color accent = new Color(0.16f, 0.38f, 0.9f);
IButtonStyle primaryButtonStyle = ButtonStyles.Filled(accent, Color.white, 10);

Button("Buy", () => Debug.Log("Buy"))
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
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<float> storageUsed = new State<float>(0.42f);

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
Color accent = new Color(0.16f, 0.38f, 0.9f);
State<string> email = new State<string>("");
State<string> password = new State<string>("");
ITextFieldStyle fieldStyle = TextFieldStyles.Chrome(
    backgroundColor: Color.white,
    tintColor: accent,
    cornerRadius: 8);

VStack(() =>
{
    TextField("Email", text: email, prompt: Text("email@example.com"))
        .textFieldStyle(fieldStyle);

    SecureField("Password", text: password, prompt: Text("Password"))
        .textFieldStyle(fieldStyle);

    Button("Sign in", () => Debug.Log("Sign in"))
        .frame(infiniteWidth: true, height: 44)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f, alignment: VStackAlignment.Leading);`) },
              { title: "Settings row", body: code(`
State<bool> notifications = new State<bool>(true);

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
Color accent = new Color(0.16f, 0.38f, 0.9f);
Sprite emptyIcon = Resources.Load<Sprite>("Icons/Empty");

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

    Button("Create", () => Debug.Log("Create"))
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f)
.frame(infiniteWidth: true, infiniteHeight: true);`) },
              { title: "HUD", body: code(`
State<int> hp = new State<int>(100);
State<int> gold = new State<int>(250);

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
            id: "factory-parameters",
            title: "Factory Parameters",
            kind: "Reference",
            summary: "Look up the parameters passed to Text, Button, stacks, TextField, controls, and other factory calls.",
            keywords: "factory parameters arguments Text Button VStack HStack ZStack Grid TextField SecureField Image ProgressView Slider Picker spacing alignment states prompt content",
            sections: [
              {
                title: "Basic elements",
                body: `
                  <p>Factories create UI elements. Read <code>content</code> as child UI, <code>State</code> as changing value, and <code>Action</code> as code to run.</p>
                  ${parameterTable("en", factoryParametersEn.basics)}
                `
              },
              {
                title: "Layout",
                body: `
                  <p><code>spacing</code> is the gap between children, <code>alignment</code> is placement, and <code>states</code> controls when content is rebuilt.</p>
                  ${parameterTable("en", factoryParametersEn.layout)}
                `
              },
              {
                title: "Images and shapes",
                body: parameterTable("en", factoryParametersEn.shapes)
              },
              {
                title: "Input",
                body: parameterTable("en", factoryParametersEn.input)
              },
              {
                title: "Media and controls",
                body: parameterTable("en", factoryParametersEn.media)
              }
            ],
            related: ["modifier-parameters", "api-index", "overview"]
          },
          {
            id: "modifier-parameters",
            title: "Modifier Parameters",
            kind: "Reference",
            summary: "Look up modifier parameters by name, type, and meaning.",
            keywords: "modifier parameters frame padding background overlay tint opacity cornerRadius border shadow input scroll",
            sections: [
              { title: "Layout", body: parameterTable("en", modifierParametersEn.layout) },
              { title: "Appearance", body: parameterTable("en", modifierParametersEn.visual) },
              { title: "Transform", body: parameterTable("en", modifierParametersEn.transform) },
              { title: "Input", body: parameterTable("en", modifierParametersEn.input) },
              { title: "Scroll", body: parameterTable("en", modifierParametersEn.scroll) },
              { title: "Motion and behavior", body: parameterTable("en", modifierParametersEn.behavior) },
              { title: "Style factories", body: parameterTable("en", modifierParametersEn.styles) },
              { title: "Animation factories", body: parameterTable("en", modifierParametersEn.animations) }
            ],
            related: ["factory-parameters", "api-index", "modifiers", "text-input"]
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
            related: ["factory-parameters", "modifier-parameters", "overview", "setup"]
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
  reference: "api-index",
  arguments: "factory-parameters",
  args: "factory-parameters",
  parameters: "factory-parameters",
  factoryparams: "factory-parameters",
  factoryparameters: "factory-parameters",
  modifierparams: "modifier-parameters",
  modifierparameters: "modifier-parameters"
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
