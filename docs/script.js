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

function table(headers, rows) {
  return `<table class="api-table">
    <thead><tr>${headers.map(header => `<th>${escapeHtml(header)}</th>`).join("")}</tr></thead>
    <tbody>
      ${rows.map(row => `<tr>${row.map(cell => `<td>${cell}</td>`).join("")}</tr>`).join("")}
    </tbody>
  </table>`;
}

function api(name) {
  return `<code>${escapeHtml(name)}</code>`;
}

const apiRows = {
  factories: [
    ["Text", "Text, reactive text"],
    ["Button", "Actions"],
    ["TextField, SecureField, TextEditor", "Text input"],
    ["Toggle, Slider, Stepper, ProgressView, Picker", "Controls"],
    ["Image, Label, Divider", "Media and utility views"],
    ["Rectangle, Circle, Capsule, RoundedRectangle", "Shapes"],
    ["VStack, HStack, ZStack, Spacer", "Basic layout"],
    ["LazyVStack, LazyHStack, Grid, GridRow, GeometryReader", "Advanced layout"],
    ["ScrollView, TabView, Tab", "Navigation and containers"],
    ["ForEach", "Repeat content"]
  ],
  modifiers: [
    ["frame, padding, fixedSize, layoutPriority", "Size and spacing"],
    ["background, overlay, border, shadow", "Layers and decoration"],
    ["foregroundColor, tint, opacity", "Color"],
    ["font, fontSize, bold, italic, underline, strikethrough", "Text"],
    ["lineLimit, multilineTextAlignment", "Text wrapping"],
    ["cornerRadius, clipped, clipShape, aspectRatio", "Shape and clipping"],
    ["offset, position, rotationEffect, scaleEffect", "Transforms"],
    ["disabled, hidden, allowsHitTesting", "Interaction"],
    ["onAppear, onChange, update", "Lifecycle"],
    ["animation", "Implicit animation"],
    ["buttonStyle, textFieldStyle", "Styles"]
  ],
  input: [
    ["focused", "Bind focus state"],
    ["onEditingChanged, onSubmit", "Input events"],
    ["selectAllOnFocus", "Selection behavior"],
    ["contentMargins", "Inner text padding"],
    ["textSelectionColor, caretColor, caretWidth, caretBlinkRate", "Caret and selection"],
    ["textContentType, textInputLimit, keyboardType", "Input rules"]
  ],
  imageScroll: [
    ["resizable, scaledToFit, scaledToFill, renderingMode", "Image"],
    ["pickerStyle", "Picker"],
    ["scrollIndicators, scrollBounce, scrollSensitivity", "Scroll"],
    ["scrollMovementType, scrollPositionX, scrollPositionY", "Scroll binding"]
  ],
  types: [
    ["State<T>", "Observable value"],
    ["Animation", "Animation definition"],
    ["Axis, Edge, RectCorner", "Layout and modifier options"],
    ["PickerStyle, ImageRenderingMode, ImageResizingMode", "Control and image options"],
    ["AspectRatioContentMode, UniftUIClipShape", "Shape options"],
    ["ScrollIndicatorVisibility, UniftUIScrollAxis", "Scroll options"],
    ["VStackAlignment, HStackAlignment, ZStackAlignment", "Alignment options"],
    ["GeometryProxy", "GeometryReader value"],
    ["ButtonStyles, TextFieldStyles", "Built-in style factories"]
  ]
};

const apiRowsJa = {
  factories: [
    ["Text", "文字表示。固定文字、State、計算した文字列に対応"],
    ["Button", "クリック時の処理"],
    ["TextField, SecureField, TextEditor", "テキスト入力"],
    ["Toggle, Slider, Stepper, ProgressView, Picker", "状態に紐づくコントロール"],
    ["Image, Label, Divider", "画像、ラベル、区切り線"],
    ["Rectangle, Circle, Capsule, RoundedRectangle", "単純な図形"],
    ["VStack, HStack, ZStack, Spacer", "基本レイアウト"],
    ["LazyVStack, LazyHStack, Grid, GridRow, GeometryReader", "一覧、表、サイズ参照"],
    ["ScrollView, TabView, Tab", "スクロールとタブ"],
    ["ForEach", "繰り返し表示"]
  ],
  modifiers: [
    ["frame, padding, fixedSize, layoutPriority", "サイズと余白"],
    ["background, overlay, border, shadow", "背景、重ね合わせ、装飾"],
    ["foregroundColor, tint, opacity", "色と透明度"],
    ["font, fontSize, bold, italic, underline, strikethrough", "文字の見た目"],
    ["lineLimit, multilineTextAlignment", "行数と複数行の揃え"],
    ["cornerRadius, clipped, clipShape, aspectRatio", "角丸、切り抜き、比率"],
    ["offset, position, rotationEffect, scaleEffect", "移動、位置、回転、拡大"],
    ["disabled, hidden, allowsHitTesting", "操作と表示"],
    ["onAppear, onChange, update", "ライフサイクル"],
    ["animation", "State変化のアニメーション"],
    ["buttonStyle, textFieldStyle", "コントロールのスタイル"]
  ],
  input: [
    ["focused", "フォーカス状態のバインド"],
    ["onEditingChanged, onSubmit", "入力開始/終了、送信イベント"],
    ["selectAllOnFocus", "フォーカス時に全選択"],
    ["contentMargins", "入力欄内側の余白"],
    ["textSelectionColor, caretColor, caretWidth, caretBlinkRate", "選択色とキャレット"],
    ["textContentType, textInputLimit, keyboardType", "入力種別、文字数制限、モバイルキーボード"]
  ],
  imageScroll: [
    ["resizable, scaledToFit, scaledToFill, renderingMode", "画像のリサイズと表示方式"],
    ["pickerStyle", "Pickerの見た目"],
    ["scrollIndicators, scrollBounce, scrollSensitivity", "スクロールバーとスクロール感"],
    ["scrollMovementType, scrollPositionX, scrollPositionY", "ScrollRectの動きと位置バインド"]
  ],
  types: [
    ["State<T>", "監視できる値"],
    ["Animation", "アニメーション定義"],
    ["Axis, Edge, RectCorner", "入力方向、余白方向、角の指定"],
    ["PickerStyle, ImageRenderingMode, ImageResizingMode", "Pickerと画像の指定"],
    ["AspectRatioContentMode, UniftUIClipShape", "比率と切り抜き形状"],
    ["ScrollIndicatorVisibility, UniftUIScrollAxis", "スクロール表示の指定"],
    ["VStackAlignment, HStackAlignment, ZStackAlignment", "揃え位置"],
    ["GeometryProxy", "GeometryReaderで受け取るサイズ情報"],
    ["ButtonStyles, TextFieldStyles", "組み込みスタイル"]
  ]
};

const modifierParamsJa = {
  layout: [
    ["frame(width:height:)", `${api("width")} / ${api("height")}`, "固定サイズを指定します。どちらかだけ指定しても使えます。"],
    ["frame(infiniteWidth:infiniteHeight:)", `${api("bool?")}`, "親の空き幅/高さをできるだけ使います。横いっぱいのボタンや画面いっぱいのコンテナに使います。"],
    ["frame(minWidth:maxWidth:minHeight:maxHeight:)", `${api("float?")}`, "最小/最大サイズを指定します。伸び縮みは許すが限界を持たせたいときに使います。"],
    ["padding(_)", `${api("int")} / ${api("State<int>")} / ${api("RectOffset")}`, "全方向、State連動、または Unity の RectOffset で余白を指定します。"],
    ["padding(_:_:)", `${api("Edge")} + ${api("int")}`, "特定方向の余白です。例: <code>Edge.Horizontal</code> は左右、<code>Edge.Vertical</code> は上下。"],
    ["padding(top:bottom:left:right:)", `${api("float?")}`, "上下左右を個別に指定します。指定しない方向は 0 のままです。"],
    ["fixedSize(horizontal:vertical:)", `${api("bool")}`, "親に伸ばされず、内容に近いサイズを優先します。"],
    ["layoutPriority(_)", `${api("float")}`, "同じ親の中で、空間を優先してもらいたい度合いです。値が大きいほど優先されます。"],
    ["aspectRatio(_:contentMode:)", `${api("float")} + ${api("AspectRatioContentMode")}`, "指定した比率を保つレイアウトです。<code>Fit</code> は収める、<code>Fill</code> は埋めます。"]
  ],
  visual: [
    ["background(_)", `${api("Color")} / ${api("State<Color>")}`, "単色背景を付けます。<code>padding</code> の後に置くと余白込みで背景が付きます。"],
    ["background(_)", `${api("UIElement")} / ${api("Action")}`, "背景に図形や独自の View を置きます。角丸の Capsule 背景などに使います。"],
    ["overlay(_:alignment:)", `${api("UIElement")} / ${api("Action")} + ${api("ZStackAlignment")}`, "上に View を重ねます。バッジや枠線表現に使います。"],
    ["foregroundColor(_)", `${api("Color")} / ${api("State<Color>")}`, "Text や対応する表示要素の前景色です。"],
    ["tint(_)", `${api("Color")} / ${api("State<Color>")}`, "Button、Picker、TextField のキャレット、Image template などのアクセント色です。"],
    ["opacity(_)", `${api("float")} / ${api("State<float>")}`, "透明度です。0 で透明、1 で不透明です。"],
    ["cornerRadius(_)", `${api("float")} / ${api("State<float>")}`, "全角を丸めます。State を渡すと角丸アニメーションに使えます。"],
    ["cornerRadius(_:_:_:_:)", `${api("float topLeft, topRight, bottomRight, bottomLeft")}`, "4つの角を個別に丸めます。"],
    ["cornerRadius(_:_:)", `${api("RectCorner")} + ${api("float")}`, "指定した角だけ丸めます。例: <code>RectCorner.Top</code>。"],
    ["border(_:width:)", `${api("Color")} / ${api("State<Color>")} + ${api("float")}`, "矩形の枠線を描きます。丸い枠が必要なら <code>overlay</code> で図形を重ねます。"],
    ["shadow(color:radius:x:y:)", `${api("Color?")} + ${api("float radius")} + ${api("x/y")}`, "影を付けます。<code>x</code> と <code>y</code> は影のずれです。"],
    ["clipped()", "なし", "子要素を親の矩形範囲で切り抜きます。"],
    ["clipShape(_:cornerRadius:)", `${api("UniftUIClipShape")} + ${api("float")}`, "Circle、Capsule、RoundedRectangle などの形で切り抜きます。"]
  ],
  text: [
    ["font(_)", `${api("TMP_FontAsset")}`, "TextMesh Pro のフォントアセットを指定します。"],
    ["fontSize(_)", `${api("float")}`, "文字サイズです。"],
    ["bold() / italic()", "なし", "太字/斜体にします。placeholder の <code>Text</code> にも使えます。"],
    ["underline() / strikethrough()", "なし", "下線/取り消し線を付けます。"],
    ["lineLimit(_)", `${api("int?")}`, "表示行数の上限です。<code>null</code> または省略で制限なしです。"],
    ["multilineTextAlignment(_)", `${api("TextAlignmentOptions")}`, "複数行テキストの揃えです。TextField の複数行入力にも使います。"]
  ],
  transform: [
    ["offset(x:y:)", `${api("float")} / ${api("State<float>")}`, "レイアウト位置はそのまま、見た目だけずらします。"],
    ["offset(_)", `${api("Vector2")} / ${api("State<Vector2>")}`, "2Dベクトルで見た目のずれを指定します。"],
    ["position(x:y:)", `${api("float")} / ${api("State<float>")}`, "RectTransform の位置を指定します。自由配置したいとき向けです。"],
    ["rotationEffect(_)", `${api("float degrees")} / ${api("State<float>")}`, "Z軸回転の角度です。単位は度です。"],
    ["rotationEffect(x:y:z:)", `${api("float")} / ${api("State<Vector3>")}`, "3軸の回転を指定します。"],
    ["scaleEffect(_)", `${api("float")} / ${api("State<float>")}`, "全方向の拡大率です。1 が等倍です。"],
    ["scaleEffect(x:y:)", `${api("float")} / ${api("State<float>")}`, "横と縦の拡大率を別々に指定します。"]
  ],
  behavior: [
    ["disabled(_)", `${api("bool")} / ${api("State<bool>")}`, "操作できない状態にします。ボタンや入力を無効化したいときに使います。"],
    ["hidden(_)", `${api("bool")}`, "非表示にします。入力も受けません。"],
    ["allowsHitTesting(_)", `${api("bool")} / ${api("State<bool>")}`, "クリックやタップを受けるかどうかを制御します。"],
    ["onAppear(_)", `${api("Action")} / ${api("Func<Task>")}`, "要素が表示されたときに呼ばれます。"],
    ["onChange(_:_:)", `${api("State")} + ${api("Action")}`, "State が変わったときに呼ばれます。"],
    ["update(_)", `${api("Action")}`, "毎フレーム呼ばれます。必要なときだけ使います。"],
    ["animation(_:value:)", `${api("Animation")} + ${api("State")}`, "指定した State が変わったとき、この modifier までの見た目変更をアニメーションします。"]
  ],
  input: [
    ["focused(_)", `${api("State<bool>")}`, "TextField のフォーカス状態を State と同期します。"],
    ["onEditingChanged(_)", `${api("Action<bool>")}`, "編集開始時に true、終了時に false が渡ります。"],
    ["onSubmit(_)", `${api("Action<string>")}`, "送信時に現在の文字列が渡ります。"],
    ["selectAllOnFocus(_)", `${api("bool")}`, "フォーカス時に全選択するかどうかです。デフォルトは true。"],
    ["contentMargins(horizontal:vertical:)", `${api("float")} + ${api("float")}`, "TextField 内側の左右/上下余白です。"],
    ["contentMargins(left:right:top:bottom:)", `${api("float")}`, "TextField 内側の余白を個別に指定します。"],
    ["textSelectionColor(_)", `${api("Color")} / ${api("State<Color>")}`, "選択範囲の色です。透明度を 0.2〜0.35 程度にすると見やすいです。"],
    ["caretColor(_)", `${api("Color")} / ${api("State<Color>")}`, "キャレットの色です。"],
    ["caretWidth(_)", `${api("int")}`, "キャレットの太さです。"],
    ["caretBlinkRate(_)", `${api("float")}`, "キャレット点滅速度です。"],
    ["textContentType(_)", `${api("TMP_InputField.ContentType")}`, "Email、Password など TMP 側の入力種別です。"],
    ["textInputLimit(_)", `${api("int")}`, "最大文字数です。"],
    ["keyboardType(_)", `${api("TouchScreenKeyboardType")}`, "モバイルキーボードの種類です。"]
  ],
  mediaScroll: [
    ["resizable(_)", `${api("ImageResizingMode")}`, "Image をリサイズ対象にします。Stretch または Tile を指定できます。"],
    ["scaledToFit()", "なし", "画像全体がフレーム内に収まるように表示します。"],
    ["scaledToFill()", "なし", "フレームを埋めるように表示します。はみ出しは <code>clipped()</code> と組み合わせます。"],
    ["renderingMode(_)", `${api("ImageRenderingMode")}`, "Original または Template を指定します。Template は tint と組み合わせます。"],
    ["pickerStyle(_)", `${api("PickerStyle")}`, "Picker の表示形式です。現在は Segmented が主な用途です。"],
    ["scrollIndicators(_:)", `${api("ScrollIndicatorVisibility")} / ${api("UniftUIScrollAxis")}`, "スクロールバーの表示と対象軸を指定します。"],
    ["scrollBounce(_)", `${api("bool")}`, "端で弾む動きを許可するかどうかです。"],
    ["scrollSensitivity(_)", `${api("float")}`, "ホイール/スクロール操作の感度です。"],
    ["scrollMovementType(_)", `${api("ScrollRect.MovementType")}`, "Unity の ScrollRect の移動方式です。"],
    ["scrollPositionX/Y(_:twoWay:)", `${api("State<float>")} + ${api("bool")}`, "正規化スクロール位置を State と同期します。twoWay が true ならユーザー操作も State へ戻します。"]
  ]
};

const modifierParamsEn = {
  layout: [
    ["frame(width:height:)", `${api("width")} / ${api("height")}`, "Sets a fixed preferred size. You can provide only one axis."],
    ["frame(infiniteWidth:infiniteHeight:)", `${api("bool?")}`, "Uses as much available parent width or height as possible."],
    ["frame(minWidth:maxWidth:minHeight:maxHeight:)", `${api("float?")}`, "Sets flexible size limits."],
    ["padding(_)", `${api("int")} / ${api("State<int>")} / ${api("RectOffset")}`, "Adds padding on all sides, binds padding to State, or uses a Unity RectOffset."],
    ["padding(_:_:)", `${api("Edge")} + ${api("int")}`, "Adds padding to selected edges. <code>Edge.Horizontal</code> means left and right."],
    ["padding(top:bottom:left:right:)", `${api("float?")}`, "Sets per-side padding. Omitted sides stay at zero."],
    ["fixedSize(horizontal:vertical:)", `${api("bool")}`, "Prefers intrinsic content size instead of stretching on selected axes."],
    ["layoutPriority(_)", `${api("float")}`, "Hints which sibling should receive flexible space first."],
    ["aspectRatio(_:contentMode:)", `${api("float")} + ${api("AspectRatioContentMode")}`, "Preserves a ratio. <code>Fit</code> contains, <code>Fill</code> covers."]
  ],
  visual: [
    ["background(_)", `${api("Color")} / ${api("State<Color>")}`, "Adds a solid background. Put it after padding to include padding in the fill."],
    ["background(_)", `${api("UIElement")} / ${api("Action")}`, "Places a view behind the element, such as a Capsule or RoundedRectangle."],
    ["overlay(_:alignment:)", `${api("UIElement")} / ${api("Action")} + ${api("ZStackAlignment")}`, "Places a view above the element, often for badges or custom borders."],
    ["foregroundColor(_)", `${api("Color")} / ${api("State<Color>")}`, "Sets foreground color for Text and supported views."],
    ["tint(_)", `${api("Color")} / ${api("State<Color>")}`, "Sets accent color for controls, template images, and text field carets."],
    ["opacity(_)", `${api("float")} / ${api("State<float>")}`, "Sets alpha. 0 is transparent, 1 is opaque."],
    ["cornerRadius(_)", `${api("float")} / ${api("State<float>")}`, "Rounds all corners. Bind to State for animated radius changes."],
    ["cornerRadius(_:_:_:_:)", `${api("float topLeft, topRight, bottomRight, bottomLeft")}`, "Rounds each corner separately."],
    ["cornerRadius(_:_:)", `${api("RectCorner")} + ${api("float")}`, "Rounds only selected corners, such as <code>RectCorner.Top</code>."],
    ["border(_:width:)", `${api("Color")} / ${api("State<Color>")} + ${api("float")}`, "Draws a rectangular border."],
    ["shadow(color:radius:x:y:)", `${api("Color?")} + ${api("float radius")} + ${api("x/y")}`, "Adds a shadow. x and y are the shadow offset."],
    ["clipped()", "None", "Clips children to the element rectangle."],
    ["clipShape(_:cornerRadius:)", `${api("UniftUIClipShape")} + ${api("float")}`, "Clips to Rectangle, RoundedRectangle, Circle, or Capsule."]
  ],
  text: [
    ["font(_)", `${api("TMP_FontAsset")}`, "Sets the TextMesh Pro font asset."],
    ["fontSize(_)", `${api("float")}`, "Sets text size."],
    ["bold() / italic()", "None", "Applies bold or italic style. Also works for prompt Text."],
    ["underline() / strikethrough()", "None", "Applies underline or strikethrough."],
    ["lineLimit(_)", `${api("int?")}`, "Limits visible lines. Omit or pass null for no limit."],
    ["multilineTextAlignment(_)", `${api("TextAlignmentOptions")}`, "Sets multiline text alignment, including multiline text fields."]
  ],
  transform: [
    ["offset(x:y:)", `${api("float")} / ${api("State<float>")}`, "Moves the rendered view without changing the surrounding layout."],
    ["offset(_)", `${api("Vector2")} / ${api("State<Vector2>")}`, "Moves the rendered view by a 2D vector."],
    ["position(x:y:)", `${api("float")} / ${api("State<float>")}`, "Sets anchored position for free placement."],
    ["rotationEffect(_)", `${api("float degrees")} / ${api("State<float>")}`, "Rotates around Z in degrees."],
    ["rotationEffect(x:y:z:)", `${api("float")} / ${api("State<Vector3>")}`, "Sets 3-axis rotation."],
    ["scaleEffect(_)", `${api("float")} / ${api("State<float>")}`, "Uniform scale. 1 is normal size."],
    ["scaleEffect(x:y:)", `${api("float")} / ${api("State<float>")}`, "Separate horizontal and vertical scale."]
  ],
  behavior: [
    ["disabled(_)", `${api("bool")} / ${api("State<bool>")}`, "Disables interaction for supported controls."],
    ["hidden(_)", `${api("bool")}`, "Hides the element and prevents interaction."],
    ["allowsHitTesting(_)", `${api("bool")} / ${api("State<bool>")}`, "Controls whether the element receives pointer events."],
    ["onAppear(_)", `${api("Action")} / ${api("Func<Task>")}`, "Runs when the element appears."],
    ["onChange(_:_:)", `${api("State")} + ${api("Action")}`, "Runs when a State changes."],
    ["update(_)", `${api("Action")}`, "Runs every frame while the element is alive."],
    ["animation(_:value:)", `${api("Animation")} + ${api("State")}`, "Animates visual changes up to this modifier when the State changes."]
  ],
  input: [
    ["focused(_)", `${api("State<bool>")}`, "Binds TextField focus to State."],
    ["onEditingChanged(_)", `${api("Action<bool>")}`, "Receives true when editing begins and false when editing ends."],
    ["onSubmit(_)", `${api("Action<string>")}`, "Receives the current string when the field submits."],
    ["selectAllOnFocus(_)", `${api("bool")}`, "Selects all text when focused. Defaults to true."],
    ["contentMargins(horizontal:vertical:)", `${api("float")} + ${api("float")}`, "Sets horizontal and vertical inner padding."],
    ["contentMargins(left:right:top:bottom:)", `${api("float")}`, "Sets per-side inner padding."],
    ["textSelectionColor(_)", `${api("Color")} / ${api("State<Color>")}`, "Sets selected-text highlight color."],
    ["caretColor(_)", `${api("Color")} / ${api("State<Color>")}`, "Sets caret color."],
    ["caretWidth(_)", `${api("int")}`, "Sets caret width."],
    ["caretBlinkRate(_)", `${api("float")}`, "Sets caret blink rate."],
    ["textContentType(_)", `${api("TMP_InputField.ContentType")}`, "Sets TMP input content type, such as EmailAddress or Password."],
    ["textInputLimit(_)", `${api("int")}`, "Sets max character count."],
    ["keyboardType(_)", `${api("TouchScreenKeyboardType")}`, "Sets mobile keyboard type."]
  ],
  mediaScroll: [
    ["resizable(_)", `${api("ImageResizingMode")}`, "Makes an Image resizable using Stretch or Tile."],
    ["scaledToFit()", "None", "Shows the whole image inside the frame."],
    ["scaledToFill()", "None", "Fills the frame. Combine with <code>clipped()</code> to hide overflow."],
    ["renderingMode(_)", `${api("ImageRenderingMode")}`, "Uses Original or Template rendering. Template is useful with tint."],
    ["pickerStyle(_)", `${api("PickerStyle")}`, "Sets Picker presentation, usually Segmented."],
    ["scrollIndicators(_:)", `${api("ScrollIndicatorVisibility")} / ${api("UniftUIScrollAxis")}`, "Sets scrollbar visibility and target axes."],
    ["scrollBounce(_)", `${api("bool")}`, "Enables or disables elastic bounce at the edges."],
    ["scrollSensitivity(_)", `${api("float")}`, "Sets wheel or gesture sensitivity."],
    ["scrollMovementType(_)", `${api("ScrollRect.MovementType")}`, "Sets the underlying Unity ScrollRect movement mode."],
    ["scrollPositionX/Y(_:twoWay:)", `${api("State<float>")} + ${api("bool")}`, "Binds normalized scroll position. With twoWay true, user scrolling writes back to State."]
  ]
};

function localizedApiTable(lang, rows) {
  const headers = lang === "ja" ? ["API", "用途"] : ["API", "Use"];
  return table(headers, rows.map(([name, description]) => [api(name), escapeHtml(description)]));
}

function modifierParamTable(lang, rows) {
  const headers = lang === "ja" ? ["modifier", "引数", "意味"] : ["Modifier", "Parameters", "Meaning"];
  return table(headers, rows.map(([name, parameters, meaning]) => [api(name), parameters, meaning]));
}

const docs = {
  ja: {
    chrome: {
      search: "検索",
      toc: "このページ",
      noResults: "見つかりませんでした。",
      docsTitle: "ドキュメント",
      theme: "テーマ",
      themeSystem: "デバイス",
      themeLight: "ライト",
      themeDark: "ダーク"
    },
    groups: [
      {
        title: "はじめに",
        pages: [
          {
            id: "overview",
            title: "UniftUIとは",
            kind: "ガイド",
            summary: "Unity の Canvas を、SwiftUI に近い書き方で組み立てるためのライブラリです。",
            keywords: "UniftUI Unity Canvas uGUI SwiftUI Build",
            sections: [
              {
                title: "何ができるか",
                body: `
                  <p>UniftUI は、C# のコードで UI の見た目と状態をまとめて書くための小さなフレームワークです。Unity の uGUI と TextMesh Pro を使って、実際の <code>GameObject</code> と <code>RectTransform</code> を作ります。</p>
                  <div class="callout"><p>最初は <code>UniftView</code> を継承して、<code>VStack</code> の中に <code>Text</code> や <code>Button</code> を置くところから始めれば大丈夫です。</p></div>
                `
              },
              {
                title: "3つの基本",
                body: `
                  <ul>
                    <li><strong>Factory</strong>: <code>Text</code>、<code>Button</code>、<code>VStack</code> などで部品を作ります。</li>
                    <li><strong>State</strong>: 変わる値は <code>State&lt;T&gt;</code> に入れます。</li>
                    <li><strong>Modifier</strong>: <code>.padding()</code>、<code>.background()</code>、<code>.fontSize()</code> のように後ろへつなげて見た目を変えます。</li>
                  </ul>
                  ${code(`
Text("Save")
    .fontSize(16)
    .padding(12)
    .background(Color.white)
    .cornerRadius(8);`)}
                `
              },
              {
                title: "最小の例",
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
                .background(new Color(0.2f, 0.45f, 0.95f))
                .foregroundColor(Color.white)
                .cornerRadius(12);
        }, spacing: 12f)
        .padding(24)
        .Build(GetComponent<Canvas>());
    }
}`)
              },
              {
                title: "読む順番",
                body: `
                  <div class="steps">
                    <div class="step"><strong>インストールと準備</strong><br>Canvas に <code>UniftView</code> を付けるところまで確認します。</div>
                    <div class="step"><strong>State</strong><br>値が変わったら UI が変わる仕組みを覚えます。</div>
                    <div class="step"><strong>レイアウト</strong><br><code>VStack</code>、<code>HStack</code>、<code>Spacer</code>、<code>frame</code> を使います。</div>
                    <div class="step"><strong>コントロールと入力</strong><br><code>Button</code>、<code>TextField</code>、<code>Picker</code> などを追加します。</div>
                  </div>
                `
              }
            ],
            related: ["setup", "state", "layout"]
          },
          {
            id: "setup",
            title: "インストールと準備",
            kind: "ガイド",
            summary: "Package Manager で追加し、Canvas にビューを表示するまでの準備です。",
            keywords: "install Package Manager Canvas TMP TextMeshPro Build README",
            sections: [
              {
                title: "インストール",
                body: `
                  <p>Unity Package Manager の <strong>Add package from git URL</strong> に以下を入れます。</p>
                  ${code("https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI")}
                  <p>タグを指定する場合は、末尾に <code>#v0.1.0</code> のように付けます。</p>
                `
              },
              {
                title: "シーンの準備",
                body: `
                  <div class="steps">
                    <div class="step">シーンに <strong>Canvas</strong> を置きます。</div>
                    <div class="step">Canvas と同じ GameObject に、<code>UniftView</code> を継承したスクリプトを付けます。</div>
                    <div class="step"><code>Start()</code> の最後で <code>.Build(GetComponent&lt;Canvas&gt;())</code> を呼びます。</div>
                  </div>
                `
              },
              {
                title: "表示されないとき",
                body: `
                  <ul>
                    <li>スクリプトが Canvas と同じ GameObject に付いているか確認します。</li>
                    <li>Console に例外が出ていないか確認します。</li>
                    <li>TextMesh Pro のフォントが必要な場合は、<code>UIContext.SetDefaultFont(...)</code> を使います。</li>
                  </ul>
                `
              },
              {
                title: "デフォルトフォントを設定する",
                body: `
                  <p>プロジェクトで同じ TMP フォントを使うなら、ビューを作る前に一度設定します。</p>
                  ${code(`
private TMP_FontAsset font;

private void Start()
{
    font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");
    UIContext.SetDefaultFont(font);

    VStack(() =>
    {
        Text("こんにちは");
    })
    .Build(GetComponent<Canvas>());
}`)}
                `
              }
            ],
            related: ["overview", "text", "api-index"]
          },
          {
            id: "state",
            title: "StateでUIを更新する",
            kind: "基本",
            summary: "値を <code>State&lt;T&gt;</code> に入れると、値の変更にあわせて UI を更新できます。",
            declaration: "public class State<T> : State",
            keywords: "State Value reactive onChange BatchUpdate",
            sections: [
              {
                title: "Stateの考え方",
                body: `
                  <p><code>State&lt;T&gt;</code> は UI に見せたい値を入れる箱です。<code>Value</code> を変えると、その State を使っている表示や modifier が更新されます。</p>
                  ${code(`
private readonly State<int> count = new State<int>(0);

Text(() => $"Count: {count.Value}", new State[] { count });
Button("+1", () => count.Value++);`)}
                `
              },
              {
                title: "入力とState",
                body: code(`
private readonly State<string> name = new State<string>("");

TextField("Name", text: name, prompt: Text("Required"));
Text(() => $"Hello, {name.Value}", new State[] { name });`)
              },
              {
                title: "変更を受け取る",
                body: `
                  <p>値の変更に合わせて処理したいときは <code>.onChange(...)</code> を使います。表示の更新だけなら、依存する <code>State</code> を <code>Text</code> へ渡すだけで十分です。</p>
                  ${code(`
Text(() => $"Volume: {volume.Value}", new State[] { volume })
    .onChange(volume, value =>
    {
        Debug.Log($"volume changed: {value}");
    });`)}
                `
              },
              {
                title: "変更をまとめる",
                body: code(`
using (State.BatchUpdate())
{
    firstName.Value = "Ada";
    lastName.Value = "Lovelace";
}`)
              }
            ],
            related: ["text-input", "animation", "api-index"]
          }
        ]
      },
      {
        title: "UIを作る",
        pages: [
          {
            id: "layout",
            title: "レイアウト",
            kind: "基本",
            summary: "画面の並びは <code>VStack</code>、<code>HStack</code>、<code>ZStack</code>、<code>Spacer</code> から始めます。",
            keywords: "VStack HStack ZStack Spacer frame padding Grid LazyVStack LazyHStack GeometryReader ForEach layoutPriority",
            sections: [
              {
                title: "縦・横・重ねる",
                body: code(`
VStack(() =>
{
    Text("Title").fontSize(24).bold();

    HStack(() =>
    {
        Text("Left");
        Spacer();
        Text("Right");
    });
}, spacing: 12f, alignment: VStackAlignment.Leading);`)
              },
              {
                title: "よくある1行レイアウト",
                body: `
                  <p><code>Spacer()</code> は、左右の要素を離したいときに使います。設定画面やリスト行でよく使う形です。</p>
                  ${code(`
HStack(() =>
{
    VStack(() =>
    {
        Text("Notifications").bold();
        Text("Receive product updates")
            .fontSize(12)
            .foregroundColor(Color.gray);
    }, spacing: 2f, alignment: VStackAlignment.Leading);

    Spacer();

    Toggle("On", notificationsEnabled);
}, spacing: 12f, alignment: HStackAlignment.Center)
.padding(12)
.background(Color.white)
.cornerRadius(10);`)}
                `
              },
              {
                title: "サイズと余白",
                body: code(`
Text("Card")
    .padding(16)
    .frame(infiniteWidth: true)
    .background(Color.white)
    .cornerRadius(10);`)
              },
              {
                title: "繰り返しとグリッド",
                body: code(`
LazyVStack(() =>
{
    ForEach(0, 20, i => Text($"Row {i}"));
});

Grid(() =>
{
    GridRow(() => { Text("Name"); Text("Value"); });
    GridRow(() => { Text("HP"); Text("100"); });
});`)
              }
            ],
            related: ["modifiers", "scroll-tabs", "api-index"]
          },
          {
            id: "text",
            title: "Textとフォント",
            kind: "基本",
            summary: "文字の表示、フォント、サイズ、太字、斜体、行数を設定します。",
            declaration: "Text(string text)\nText(State<string> text)\nText(Func<string> content, State[] dependencyStates)",
            keywords: "Text font fontSize bold italic underline strikethrough foregroundColor lineLimit multilineTextAlignment TMP_FontAsset",
            sections: [
              {
                title: "固定文字とStateの文字",
                body: code(`
Text("Hello");

Text(titleState);

Text(() => $"Score: {score.Value}", new State[] { score });`)
              },
              {
                title: "フォントと装飾",
                body: code(`
Text("Title")
    .font(myTmpFontAsset)
    .fontSize(24)
    .bold()
    .italic()
    .foregroundColor(Color.white);`)
              },
              {
                title: "複数行",
                body: code(`
Text("Long text that wraps into two lines.")
    .lineLimit(2)
    .multilineTextAlignment(TextAlignmentOptions.TopLeft);`)
              }
            ],
            related: ["text-input", "modifiers", "api-index"]
          },
          {
            id: "controls",
            title: "ボタンとコントロール",
            kind: "基本",
            summary: "クリック、オンオフ、スライダー、選択、進捗表示を作ります。",
            keywords: "Button Toggle Slider Stepper ProgressView Picker pickerStyle buttonStyle",
            sections: [
              {
                title: "Button",
                body: code(`
Button("Save", Save)
    .buttonStyle(ButtonStyles.Filled(
        backgroundColor: accent,
        foregroundColor: Color.white,
        cornerRadius: 10));`)
              },
              {
                title: "中身を自由に作るButton",
                body: `
                  <p>文字だけでなく、<code>HStack</code> や <code>Image</code> を Button の中身にできます。</p>
                  ${code(`
Button(
    HStack(() =>
    {
        Image(saveIcon).frame(width: 18, height: 18);
        Text("Save").bold();
    }, spacing: 8f),
    Save)
    .padding(10)
    .background(accent)
    .foregroundColor(Color.white)
    .cornerRadius(10);`)}
                `
              },
              {
                title: "Stateにバインドするコントロール",
                body: code(`
Toggle("Enabled", isEnabled);

Slider(volume, 0, 100)
    .frame(infiniteWidth: true, height: 38);

Stepper("Quantity", quantity, 0, 8);

ProgressView(progress, total: 1)
    .tint(accent);`)
              },
              {
                title: "Picker",
                body: code(`
Picker(selection, "Small", "Medium", "Large")
    .pickerStyle(PickerStyle.Segmented)
    .tint(accent);`)
              }
            ],
            related: ["state", "modifiers", "api-index"]
          },
          {
            id: "text-input",
            title: "TextFieldと入力",
            kind: "基本",
            summary: "一行入力、複数行入力、パスワード、placeholder、キャレット、選択色を設定します。",
            declaration: "TextField(\"Title\", text: state, prompt: Text(\"Placeholder\"))\nSecureField(\"Password\", text: state, prompt: Text(\"Password\"))\nTextEditor(state)",
            keywords: "TextField SecureField TextEditor prompt placeholder contentMargins focused selectAllOnFocus onSubmit onEditingChanged caretColor caretWidth caretBlinkRate textSelectionColor textContentType textInputLimit keyboardType textFieldStyle",
            sections: [
              {
                title: "基本のTextField",
                body: code(`
TextField(
    "Name",
    text: name,
    prompt: Text("Required").italic().foregroundColor(Color.gray))
    .lineLimit(1)
    .contentMargins(horizontal: 12, vertical: 8)
    .textFieldStyle(TextFieldStyles.Chrome(
        backgroundColor: Color.white,
        tintColor: accent,
        cornerRadius: 8));`)
              },
              {
                title: "placeholderを装飾する",
                body: `
                  <p>placeholder は <code>prompt: Text(...)</code> に渡します。斜体や色は通常の <code>Text</code> と同じ modifier で指定できます。</p>
                  ${code(`
TextField(
    "Nickname",
    text: nickname,
    prompt: Text("Optional")
        .italic()
        .foregroundColor(new Color(0.45f, 0.45f, 0.5f)));`)}
                `
              },
              {
                title: "複数行",
                body: code(`
TextField("Notes", text: notes, axis: Axis.Vertical, prompt: Text("Notes"))
    .lineLimit(3)
    .multilineTextAlignment(TextAlignmentOptions.TopLeft)
    .frame(infiniteWidth: true, height: 82);

TextEditor(notes)
    .frame(infiniteWidth: true, height: 120);`)
              },
              {
                title: "パスワード入力",
                body: code(`
SecureField(
    "Password",
    text: password,
    prompt: Text("Password").foregroundColor(Color.gray))
    .lineLimit(1)
    .contentMargins(horizontal: 12, vertical: 8)
    .textFieldStyle(TextFieldStyles.Chrome(
        backgroundColor: Color.white,
        tintColor: accent,
        cornerRadius: 8));`)
              },
              {
                title: "入力の細かい制御",
                body: code(`
TextField("Email", text: email, prompt: Text("email@example.com"))
    .textContentType(TMP_InputField.ContentType.EmailAddress)
    .keyboardType(TouchScreenKeyboardType.EmailAddress)
    .textInputLimit(40)
    .focused(isFocused)
    .selectAllOnFocus()
    .textSelectionColor(new Color(0f, 0.4f, 0.85f, 0.25f))
    .caretColor(accent)
    .caretWidth(2)
    .caretBlinkRate(0.85f)
    .onSubmit(value => Debug.Log(value));`)
              },
              {
                title: "TextFieldの見た目をまとめる",
                body: `
                  <p>同じ見た目を何度も使うときは <code>TextFieldStyles.Chrome(...)</code> に背景、フォーカス色、余白、角丸、キャレットなどをまとめます。</p>
                  ${code(`
var fieldStyle = TextFieldStyles.Chrome(
    backgroundColor: Color.white,
    focusedBackgroundColor: new Color(0.94f, 0.97f, 1f),
    textColor: Color.black,
    tintColor: accent,
    contentMargins: new Vector4(12, 12, 8, 8),
    cornerRadius: 8);

TextField("First name", text: firstName).textFieldStyle(fieldStyle);
TextField("Last name", text: lastName).textFieldStyle(fieldStyle);`)}
                `
              }
            ],
            related: ["state", "text", "api-index"]
          },
          {
            id: "image-shape",
            title: "画像と図形",
            kind: "基本",
            summary: "Sprite、単純な図形、区切り線、ラベルを表示します。",
            keywords: "Image resizable scaledToFit scaledToFill renderingMode Label Divider Rectangle Circle Capsule RoundedRectangle clipped clipShape",
            sections: [
              {
                title: "Image",
                body: code(`
Image(icon)
    .resizable()
    .scaledToFit()
    .renderingMode(ImageRenderingMode.Template)
    .tint(Color.white)
    .frame(width: 32, height: 32);`)
              },
              {
                title: "図形",
                body: code(`
RoundedRectangle(12, new Color(0.93f, 0.96f, 1f))
    .frame(width: 180, height: 44);

Circle(Color.red)
    .frame(width: 24, height: 24);`)
              },
              {
                title: "LabelとDivider",
                body: code(`
Label("Status", Circle(Color.green).frame(width: 10, height: 10));

Divider(new Color(0.82f, 0.84f, 0.88f), thickness: 1);`)
              }
            ],
            related: ["modifiers", "api-index"]
          }
        ]
      },
      {
        title: "仕上げる",
        pages: [
          {
            id: "modifiers",
            title: "見た目を変えるmodifier",
            kind: "基本",
            summary: "余白、背景、色、角丸、影、重ね合わせ、無効化などをチェーンで指定します。",
            keywords: "modifier frame padding background overlay foregroundColor tint opacity cornerRadius border shadow clipped clipShape aspectRatio fixedSize layoutPriority disabled hidden allowsHitTesting",
            sections: [
              {
                title: "modifierは順番が大事",
                body: `
                  <p>modifier は上から順番に適用されます。たとえば、<code>padding</code> のあとに <code>background</code> を置くと、余白込みで背景が付きます。</p>
                  ${code(`
Text("Card")
    .padding(16)
    .background(Color.white)
    .cornerRadius(10)
    .shadow(new Color(0, 0, 0, 0.18f), radius: 4, x: 0, y: -2);`)}
                `
              },
              {
                title: "背景にViewを置く",
                body: `
                  <p>単色だけなら <code>.background(Color)</code>、図形を使いたいときは <code>.background(...)</code> に View を渡します。</p>
                  ${code(`
Text("Premium")
    .bold()
    .padding(Edge.Horizontal, 12)
    .padding(Edge.Vertical, 6)
    .background(Capsule(new Color(1f, 0.9f, 0.45f)))
    .foregroundColor(new Color(0.2f, 0.14f, 0.02f));`)}
                `
              },
              {
                title: "重ねる",
                body: code(`
Text("Inbox")
    .padding(12)
    .background(RoundedRectangle(10, Color.white))
    .overlay(
        Circle(Color.red).frame(width: 8, height: 8),
        ZStackAlignment.TopTrailing);`)
              },
              {
                title: "表示と操作",
                body: code(`
Button("Submit", Submit)
    .disabled(isSubmitting)
    .allowsHitTesting(canTap)
    .hidden(shouldHide);`)
              }
            ],
            related: ["layout", "modifier-parameters", "recipes", "animation", "api-index"]
          },
          {
            id: "animation",
            title: "アニメーション",
            kind: "基本",
            summary: "<code>State</code> の変化にあわせて、サイズ、色、角丸、透明度、移動、回転、拡大を動かします。",
            keywords: "Animation WithAnimation animation easeInOut spring bouncy delay speed repeatCount repeatForever offset position rotationEffect scaleEffect onAppear onChange update",
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
                title: "特定のStateに反応する",
                body: code(`
RoundedRectangle(radius.Value)
    .frame(width: width, height: height)
    .animation(Animation.easeInOut(0.25f), radius);`)
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
              }
            ],
            related: ["state", "modifiers", "api-index"]
          },
          {
            id: "scroll-tabs",
            title: "ScrollViewとTabView",
            kind: "基本",
            summary: "スクロールできる領域と、タブで切り替える画面を作ります。",
            keywords: "ScrollView scrollIndicators scrollBounce scrollSensitivity scrollMovementType scrollPositionX scrollPositionY TabView Tab",
            sections: [
              {
                title: "ScrollView",
                body: code(`
ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, 50, i => Text($"Row {i}"));
    });
})
.scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical)
.scrollBounce(true)
.scrollPositionY(scrollY, twoWay: true);`)
              },
              {
                title: "TabView",
                body: code(`
TabView(() =>
{
    Tab("Home", () => Text("Home"));
    Tab("Settings", () => Text("Settings"));
}, selectedTab);`)
              }
            ],
            related: ["layout", "api-index"]
          },
          {
            id: "recipes",
            title: "よく使うUIパターン",
            kind: "レシピ",
            summary: "カード、フォーム、ツールバー、空状態など、実際の画面でよく使う組み合わせです。",
            keywords: "recipes card form toolbar empty state settings row validation badge",
            sections: [
              {
                title: "カード",
                body: code(`
VStack(() =>
{
    Text("Storage").bold().fontSize(18);
    Text("42 GB used")
        .fontSize(13)
        .foregroundColor(Color.gray);

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
                title: "フォーム",
                body: code(`
VStack(() =>
{
    TextField("Email", text: email, prompt: Text("email@example.com"))
        .textContentType(TMP_InputField.ContentType.EmailAddress)
        .keyboardType(TouchScreenKeyboardType.EmailAddress)
        .textFieldStyle(fieldStyle);

    SecureField("Password", text: password, prompt: Text("Password"))
        .textFieldStyle(fieldStyle);

    Button("Sign in", SignIn)
        .frame(infiniteWidth: true, height: 44)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f, alignment: VStackAlignment.Leading);`)
              },
              {
                title: "ツールバー",
                body: code(`
HStack(() =>
{
    Button("Cancel", Cancel)
        .buttonStyle(ButtonStyles.Plain(accent));

    Spacer();

    Text("Edit Profile").bold();

    Spacer();

    Button("Done", Save)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 8));
}, spacing: 8f, alignment: HStackAlignment.Center)
.padding(12)
.background(Color.white);`)
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
              }
            ],
            related: ["layout", "controls", "text-input", "modifiers"]
          }
        ]
      },
      {
        title: "リファレンス",
        pages: [
          {
            id: "modifier-parameters",
            title: "modifierの引数",
            kind: "リファレンス",
            summary: "各 modifier に渡す引数の型と意味をまとめています。どの値を渡せばよいか迷ったときに見るページです。",
            keywords: "modifier parameter 引数 frame padding background overlay foregroundColor tint opacity cornerRadius border shadow clipped clipShape font fontSize lineLimit multilineTextAlignment offset position rotationEffect scaleEffect disabled hidden allowsHitTesting onAppear onChange update animation focused contentMargins caretColor textSelectionColor keyboardType scrollIndicators",
            sections: [
              {
                title: "サイズとレイアウト",
                body: `
                  <p><code>frame</code> はサイズ、<code>padding</code> は内側の余白です。横いっぱいにしたいときは <code>frame(infiniteWidth: true)</code> を使います。</p>
                  ${modifierParamTable("ja", modifierParamsJa.layout)}
                  ${code(`
Text("Primary action")
    .padding(Edge.Horizontal, 16)
    .padding(Edge.Vertical, 10)
    .frame(infiniteWidth: true)
    .background(Color.white);`)}
                `
              },
              {
                title: "見た目とレイヤー",
                body: `
                  <p><code>background</code> と <code>overlay</code> は、単色だけでなく View も渡せます。modifier の順番で結果が変わります。</p>
                  ${modifierParamTable("ja", modifierParamsJa.visual)}
                `
              },
              {
                title: "Text",
                body: modifierParamTable("ja", modifierParamsJa.text)
              },
              {
                title: "移動・回転・拡大",
                body: `
                  <p><code>offset</code> は見た目だけをずらし、<code>position</code> は配置位置を指定します。通常のレイアウトでは <code>offset</code> の方が扱いやすいです。</p>
                  ${modifierParamTable("ja", modifierParamsJa.transform)}
                `
              },
              {
                title: "操作・ライフサイクル・アニメーション",
                body: modifierParamTable("ja", modifierParamsJa.behavior)
              },
              {
                title: "TextField",
                body: `
                  <p>TextField 系の modifier は、見た目の chrome と入力挙動を分けて考えると扱いやすいです。背景や余白は <code>textFieldStyle</code> / <code>contentMargins</code>、入力ルールは <code>textContentType</code> / <code>keyboardType</code> に寄せます。</p>
                  ${modifierParamTable("ja", modifierParamsJa.input)}
                `
              },
              {
                title: "Image / Picker / ScrollView",
                body: modifierParamTable("ja", modifierParamsJa.mediaScroll)
              }
            ],
            related: ["modifiers", "text-input", "api-index"]
          },
          {
            id: "api-index",
            title: "API一覧",
            kind: "リファレンス",
            summary: "UI を作るときに使う factory、modifier、style、型をまとめた一覧です。",
            keywords: "Text Button Image Rectangle Circle Capsule RoundedRectangle Color Divider ProgressView Stepper Picker Label VStack HStack LazyVStack LazyHStack ZStack Grid GridRow GeometryReader Spacer Slider ScrollView Toggle TextField TextEditor SecureField TabView Tab ForEach Build frame background foregroundColor tint padding fixedSize bold italic underline strikethrough lineLimit multilineTextAlignment opacity fontSize font cornerRadius shadow border overlay offset aspectRatio clipped clipShape disabled hidden layoutPriority allowsHitTesting onChange onAppear update animation rotationEffect scaleEffect position buttonStyle textFieldStyle focused onEditingChanged onSubmit selectAllOnFocus textSelectionColor contentMargins textContentType textInputLimit caretColor caretWidth caretBlinkRate keyboardType resizable scaledToFit scaledToFill renderingMode pickerStyle scrollBounce scrollSensitivity scrollMovementType scrollPositionY scrollPositionX scrollIndicators State Animation Axis Edge RectCorner",
            sections: [
              { title: "Factory", body: localizedApiTable("ja", apiRowsJa.factories) },
              { title: "Modifier", body: localizedApiTable("ja", apiRowsJa.modifiers) },
              { title: "TextField modifier", body: localizedApiTable("ja", apiRowsJa.input) },
              { title: "Image / Picker / Scroll", body: localizedApiTable("ja", apiRowsJa.imageScroll) },
              { title: "型", body: localizedApiTable("ja", apiRowsJa.types) }
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
            title: "What is UniftUI?",
            kind: "Guide",
            summary: "A SwiftUI-style way to build Unity Canvas UI with C#.",
            keywords: "UniftUI Unity Canvas uGUI SwiftUI Build",
            sections: [
              {
                title: "What it does",
                body: `
                  <p>UniftUI lets you describe uGUI screens in C#. You build a tree with views such as <code>Text</code>, <code>Button</code>, and <code>VStack</code>, bind it to <code>State&lt;T&gt;</code>, and UniftUI creates the Unity UI objects.</p>
                  <div class="callout"><p>Start with a class that inherits <code>UniftView</code>, put views inside a stack, then call <code>.Build(canvas)</code>.</p></div>
                `
              },
              {
                title: "Three basics",
                body: `
                  <ul>
                    <li><strong>Factories</strong>: Create views with <code>Text</code>, <code>Button</code>, <code>VStack</code>, and similar calls.</li>
                    <li><strong>State</strong>: Store changing values in <code>State&lt;T&gt;</code>.</li>
                    <li><strong>Modifiers</strong>: Chain calls such as <code>.padding()</code>, <code>.background()</code>, and <code>.fontSize()</code> to change layout and appearance.</li>
                  </ul>
                  ${code(`
Text("Save")
    .fontSize(16)
    .padding(12)
    .background(Color.white)
    .cornerRadius(8);`)}
                `
              },
              {
                title: "Smallest useful example",
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
                .background(new Color(0.2f, 0.45f, 0.95f))
                .foregroundColor(Color.white)
                .cornerRadius(12);
        }, spacing: 12f)
        .padding(24)
        .Build(GetComponent<Canvas>());
    }
}`)
              },
              {
                title: "Recommended order",
                body: `
                  <div class="steps">
                    <div class="step"><strong>Install and prepare</strong><br>Add the package and build into a Canvas.</div>
                    <div class="step"><strong>State</strong><br>Learn how values update UI.</div>
                    <div class="step"><strong>Layout</strong><br>Use <code>VStack</code>, <code>HStack</code>, <code>Spacer</code>, and <code>frame</code>.</div>
                    <div class="step"><strong>Controls and input</strong><br>Add <code>Button</code>, <code>TextField</code>, <code>Picker</code>, and more.</div>
                  </div>
                `
              }
            ],
            related: ["setup", "state", "layout"]
          },
          {
            id: "setup",
            title: "Install and Prepare",
            kind: "Guide",
            summary: "Add the package and show your first view on a Canvas.",
            keywords: "install Package Manager Canvas TMP TextMeshPro Build README",
            sections: [
              {
                title: "Install",
                body: `
                  <p>In Unity Package Manager, choose <strong>Add package from git URL</strong> and enter:</p>
                  ${code("https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI")}
                `
              },
              {
                title: "Scene setup",
                body: `
                  <div class="steps">
                    <div class="step">Add a <strong>Canvas</strong> to the scene.</div>
                    <div class="step">Attach a script that inherits <code>UniftView</code> to the same GameObject as the Canvas.</div>
                    <div class="step">Call <code>.Build(GetComponent&lt;Canvas&gt;())</code> at the end of <code>Start()</code>.</div>
                  </div>
                `
              },
              {
                title: "If nothing appears",
                body: `
                  <ul>
                    <li>Check that the script is on the same GameObject as the Canvas.</li>
                    <li>Check the Console for exceptions.</li>
                    <li>If you need a TMP font, set it with <code>UIContext.SetDefaultFont(...)</code>.</li>
                  </ul>
                `
              },
              {
                title: "Set a default font",
                body: `
                  <p>If your project uses one TMP font across screens, set it before building the view.</p>
                  ${code(`
private TMP_FontAsset font;

private void Start()
{
    font = Resources.Load<TMP_FontAsset>("Fonts/NotoSansJP-Medium SDF");
    UIContext.SetDefaultFont(font);

    VStack(() =>
    {
        Text("Hello");
    })
    .Build(GetComponent<Canvas>());
}`)}
                `
              }
            ],
            related: ["overview", "text", "api-index"]
          },
          {
            id: "state",
            title: "Update UI with State",
            kind: "Basics",
            summary: "Put changing values in <code>State&lt;T&gt;</code> so the UI can react to them.",
            declaration: "public class State<T> : State",
            keywords: "State Value reactive onChange BatchUpdate",
            sections: [
              {
                title: "The idea",
                body: `
                  <p><code>State&lt;T&gt;</code> stores a value that the UI can observe. Change <code>Value</code>, and any dependent UI updates.</p>
                  ${code(`
private readonly State<int> count = new State<int>(0);

Text(() => $"Count: {count.Value}", new State[] { count });
Button("+1", () => count.Value++);`)}
                `
              },
              {
                title: "Input and state",
                body: code(`
private readonly State<string> name = new State<string>("");

TextField("Name", text: name, prompt: Text("Required"));
Text(() => $"Hello, {name.Value}", new State[] { name });`)
              },
              {
                title: "React to changes",
                body: `
                  <p>Use <code>.onChange(...)</code> when a state change should run code. If you only need the UI to update, passing the state as a dependency is enough.</p>
                  ${code(`
Text(() => $"Volume: {volume.Value}", new State[] { volume })
    .onChange(volume, value =>
    {
        Debug.Log($"volume changed: {value}");
    });`)}
                `
              },
              {
                title: "Batch changes",
                body: code(`
using (State.BatchUpdate())
{
    firstName.Value = "Ada";
    lastName.Value = "Lovelace";
}`)
              }
            ],
            related: ["text-input", "animation", "api-index"]
          }
        ]
      },
      {
        title: "Build UI",
        pages: [
          {
            id: "layout",
            title: "Layout",
            kind: "Basics",
            summary: "Arrange views with stacks, spacers, frames, lazy stacks, grids, and GeometryReader.",
            keywords: "VStack HStack ZStack Spacer frame padding Grid LazyVStack LazyHStack GeometryReader ForEach layoutPriority",
            sections: [
              {
                title: "Stacks",
                body: code(`
VStack(() =>
{
    Text("Title").fontSize(24).bold();

    HStack(() =>
    {
        Text("Left");
        Spacer();
        Text("Right");
    });
}, spacing: 12f, alignment: VStackAlignment.Leading);`)
              },
              {
                title: "Common row layout",
                body: `
                  <p><code>Spacer()</code> pushes content apart. It is useful for settings screens and list rows.</p>
                  ${code(`
HStack(() =>
{
    VStack(() =>
    {
        Text("Notifications").bold();
        Text("Receive product updates")
            .fontSize(12)
            .foregroundColor(Color.gray);
    }, spacing: 2f, alignment: VStackAlignment.Leading);

    Spacer();

    Toggle("On", notificationsEnabled);
}, spacing: 12f, alignment: HStackAlignment.Center)
.padding(12)
.background(Color.white)
.cornerRadius(10);`)}
                `
              },
              {
                title: "Size and spacing",
                body: code(`
Text("Card")
    .padding(16)
    .frame(infiniteWidth: true)
    .background(Color.white)
    .cornerRadius(10);`)
              },
              {
                title: "Repeated and grid content",
                body: code(`
LazyVStack(() =>
{
    ForEach(0, 20, i => Text($"Row {i}"));
});

Grid(() =>
{
    GridRow(() => { Text("Name"); Text("Value"); });
    GridRow(() => { Text("HP"); Text("100"); });
});`)
              }
            ],
            related: ["modifiers", "scroll-tabs", "api-index"]
          },
          {
            id: "text",
            title: "Text and Fonts",
            kind: "Basics",
            summary: "Display text and set font, size, weight, color, and line behavior.",
            declaration: "Text(string text)\nText(State<string> text)\nText(Func<string> content, State[] dependencyStates)",
            keywords: "Text font fontSize bold italic underline strikethrough foregroundColor lineLimit multilineTextAlignment TMP_FontAsset",
            sections: [
              {
                title: "Text forms",
                body: code(`
Text("Hello");

Text(titleState);

Text(() => $"Score: {score.Value}", new State[] { score });`)
              },
              {
                title: "Font and style",
                body: code(`
Text("Title")
    .font(myTmpFontAsset)
    .fontSize(24)
    .bold()
    .italic()
    .foregroundColor(Color.white);`)
              },
              {
                title: "Multiple lines",
                body: code(`
Text("Long text that wraps into two lines.")
    .lineLimit(2)
    .multilineTextAlignment(TextAlignmentOptions.TopLeft);`)
              }
            ],
            related: ["text-input", "modifiers", "api-index"]
          },
          {
            id: "controls",
            title: "Buttons and Controls",
            kind: "Basics",
            summary: "Create actions, toggles, sliders, steppers, progress views, and pickers.",
            keywords: "Button Toggle Slider Stepper ProgressView Picker pickerStyle buttonStyle",
            sections: [
              {
                title: "Button",
                body: code(`
Button("Save", Save)
    .buttonStyle(ButtonStyles.Filled(
        backgroundColor: accent,
        foregroundColor: Color.white,
        cornerRadius: 10));`)
              },
              {
                title: "Button with custom content",
                body: `
                  <p>A button label can be any UniftUI element, not just a string.</p>
                  ${code(`
Button(
    HStack(() =>
    {
        Image(saveIcon).frame(width: 18, height: 18);
        Text("Save").bold();
    }, spacing: 8f),
    Save)
    .padding(10)
    .background(accent)
    .foregroundColor(Color.white)
    .cornerRadius(10);`)}
                `
              },
              {
                title: "State-bound controls",
                body: code(`
Toggle("Enabled", isEnabled);

Slider(volume, 0, 100)
    .frame(infiniteWidth: true, height: 38);

Stepper("Quantity", quantity, 0, 8);

ProgressView(progress, total: 1)
    .tint(accent);`)
              },
              {
                title: "Picker",
                body: code(`
Picker(selection, "Small", "Medium", "Large")
    .pickerStyle(PickerStyle.Segmented)
    .tint(accent);`)
              }
            ],
            related: ["state", "modifiers", "api-index"]
          },
          {
            id: "text-input",
            title: "Text Input",
            kind: "Basics",
            summary: "Create single-line input, multiline input, passwords, placeholders, carets, and selection styling.",
            declaration: "TextField(\"Title\", text: state, prompt: Text(\"Placeholder\"))\nSecureField(\"Password\", text: state, prompt: Text(\"Password\"))\nTextEditor(state)",
            keywords: "TextField SecureField TextEditor prompt placeholder contentMargins focused selectAllOnFocus onSubmit onEditingChanged caretColor caretWidth caretBlinkRate textSelectionColor textContentType textInputLimit keyboardType textFieldStyle",
            sections: [
              {
                title: "Basic TextField",
                body: code(`
TextField(
    "Name",
    text: name,
    prompt: Text("Required").italic().foregroundColor(Color.gray))
    .lineLimit(1)
    .contentMargins(horizontal: 12, vertical: 8)
    .textFieldStyle(TextFieldStyles.Chrome(
        backgroundColor: Color.white,
        tintColor: accent,
        cornerRadius: 8));`)
              },
              {
                title: "Style the placeholder",
                body: `
                  <p>The placeholder is the <code>prompt: Text(...)</code> value. Style it with normal <code>Text</code> modifiers.</p>
                  ${code(`
TextField(
    "Nickname",
    text: nickname,
    prompt: Text("Optional")
        .italic()
        .foregroundColor(new Color(0.45f, 0.45f, 0.5f)));`)}
                `
              },
              {
                title: "Multiline input",
                body: code(`
TextField("Notes", text: notes, axis: Axis.Vertical, prompt: Text("Notes"))
    .lineLimit(3)
    .multilineTextAlignment(TextAlignmentOptions.TopLeft)
    .frame(infiniteWidth: true, height: 82);

TextEditor(notes)
    .frame(infiniteWidth: true, height: 120);`)
              },
              {
                title: "Password input",
                body: code(`
SecureField(
    "Password",
    text: password,
    prompt: Text("Password").foregroundColor(Color.gray))
    .lineLimit(1)
    .contentMargins(horizontal: 12, vertical: 8)
    .textFieldStyle(TextFieldStyles.Chrome(
        backgroundColor: Color.white,
        tintColor: accent,
        cornerRadius: 8));`)
              },
              {
                title: "Input behavior",
                body: code(`
TextField("Email", text: email, prompt: Text("email@example.com"))
    .textContentType(TMP_InputField.ContentType.EmailAddress)
    .keyboardType(TouchScreenKeyboardType.EmailAddress)
    .textInputLimit(40)
    .focused(isFocused)
    .selectAllOnFocus()
    .textSelectionColor(new Color(0f, 0.4f, 0.85f, 0.25f))
    .caretColor(accent)
    .caretWidth(2)
    .caretBlinkRate(0.85f)
    .onSubmit(value => Debug.Log(value));`)
              },
              {
                title: "Reuse TextField chrome",
                body: `
                  <p>Use <code>TextFieldStyles.Chrome(...)</code> to keep background, focus color, margins, corner radius, caret, and selection styling consistent.</p>
                  ${code(`
var fieldStyle = TextFieldStyles.Chrome(
    backgroundColor: Color.white,
    focusedBackgroundColor: new Color(0.94f, 0.97f, 1f),
    textColor: Color.black,
    tintColor: accent,
    contentMargins: new Vector4(12, 12, 8, 8),
    cornerRadius: 8);

TextField("First name", text: firstName).textFieldStyle(fieldStyle);
TextField("Last name", text: lastName).textFieldStyle(fieldStyle);`)}
                `
              }
            ],
            related: ["state", "text", "api-index"]
          },
          {
            id: "image-shape",
            title: "Images and Shapes",
            kind: "Basics",
            summary: "Display sprites, simple shapes, labels, and dividers.",
            keywords: "Image resizable scaledToFit scaledToFill renderingMode Label Divider Rectangle Circle Capsule RoundedRectangle clipped clipShape",
            sections: [
              {
                title: "Image",
                body: code(`
Image(icon)
    .resizable()
    .scaledToFit()
    .renderingMode(ImageRenderingMode.Template)
    .tint(Color.white)
    .frame(width: 32, height: 32);`)
              },
              {
                title: "Shapes",
                body: code(`
RoundedRectangle(12, new Color(0.93f, 0.96f, 1f))
    .frame(width: 180, height: 44);

Circle(Color.red)
    .frame(width: 24, height: 24);`)
              },
              {
                title: "Label and Divider",
                body: code(`
Label("Status", Circle(Color.green).frame(width: 10, height: 10));

Divider(new Color(0.82f, 0.84f, 0.88f), thickness: 1);`)
              }
            ],
            related: ["modifiers", "api-index"]
          }
        ]
      },
      {
        title: "Polish",
        pages: [
          {
            id: "modifiers",
            title: "Visual Modifiers",
            kind: "Basics",
            summary: "Change spacing, backgrounds, colors, rounded corners, shadows, overlays, and interaction.",
            keywords: "modifier frame padding background overlay foregroundColor tint opacity cornerRadius border shadow clipped clipShape aspectRatio fixedSize layoutPriority disabled hidden allowsHitTesting",
            sections: [
              {
                title: "Order matters",
                body: `
                  <p>Modifiers apply in order. For example, putting <code>background</code> after <code>padding</code> includes the padding in the background.</p>
                  ${code(`
Text("Card")
    .padding(16)
    .background(Color.white)
    .cornerRadius(10)
    .shadow(new Color(0, 0, 0, 0.18f), radius: 4, x: 0, y: -2);`)}
                `
              },
              {
                title: "Use a view as a background",
                body: `
                  <p>Use <code>.background(Color)</code> for a flat fill. Pass a view when you want a shape.</p>
                  ${code(`
Text("Premium")
    .bold()
    .padding(Edge.Horizontal, 12)
    .padding(Edge.Vertical, 6)
    .background(Capsule(new Color(1f, 0.9f, 0.45f)))
    .foregroundColor(new Color(0.2f, 0.14f, 0.02f));`)}
                `
              },
              {
                title: "Layers",
                body: code(`
Text("Inbox")
    .padding(12)
    .background(RoundedRectangle(10, Color.white))
    .overlay(
        Circle(Color.red).frame(width: 8, height: 8),
        ZStackAlignment.TopTrailing);`)
              },
              {
                title: "Visibility and interaction",
                body: code(`
Button("Submit", Submit)
    .disabled(isSubmitting)
    .allowsHitTesting(canTap)
    .hidden(shouldHide);`)
              }
            ],
            related: ["layout", "modifier-parameters", "recipes", "animation", "api-index"]
          },
          {
            id: "animation",
            title: "Animation",
            kind: "Basics",
            summary: "Animate size, color, corner radius, opacity, movement, rotation, and scale from State changes.",
            keywords: "Animation WithAnimation animation easeInOut spring bouncy delay speed repeatCount repeatForever offset position rotationEffect scaleEffect onAppear onChange update",
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
                title: "Animate one State",
                body: code(`
RoundedRectangle(radius.Value)
    .frame(width: width, height: height)
    .animation(Animation.easeInOut(0.25f), radius);`)
              },
              {
                title: "Common animations",
                body: code(`
Animation.linear(0.2f)
Animation.easeIn(0.2f)
Animation.easeOut(0.2f)
Animation.easeInOut(0.25f)
Animation.spring()
Animation.bouncy(0.5f)
Animation.easeInOut(0.25f).delay(0.1f).repeatCount(2)`)
              }
            ],
            related: ["state", "modifiers", "api-index"]
          },
          {
            id: "scroll-tabs",
            title: "ScrollView and TabView",
            kind: "Basics",
            summary: "Create scrollable content and tab-based screens.",
            keywords: "ScrollView scrollIndicators scrollBounce scrollSensitivity scrollMovementType scrollPositionX scrollPositionY TabView Tab",
            sections: [
              {
                title: "ScrollView",
                body: code(`
ScrollView(() =>
{
    LazyVStack(() =>
    {
        ForEach(0, 50, i => Text($"Row {i}"));
    });
})
.scrollIndicators(ScrollIndicatorVisibility.Visible, UniftUIScrollAxis.Vertical)
.scrollBounce(true)
.scrollPositionY(scrollY, twoWay: true);`)
              },
              {
                title: "TabView",
                body: code(`
TabView(() =>
{
    Tab("Home", () => Text("Home"));
    Tab("Settings", () => Text("Settings"));
}, selectedTab);`)
              }
            ],
            related: ["layout", "api-index"]
          },
          {
            id: "recipes",
            title: "Common UI Patterns",
            kind: "Recipes",
            summary: "Copyable patterns for cards, forms, toolbars, and empty states.",
            keywords: "recipes card form toolbar empty state settings row validation badge",
            sections: [
              {
                title: "Card",
                body: code(`
VStack(() =>
{
    Text("Storage").bold().fontSize(18);
    Text("42 GB used")
        .fontSize(13)
        .foregroundColor(Color.gray);

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
                title: "Form",
                body: code(`
VStack(() =>
{
    TextField("Email", text: email, prompt: Text("email@example.com"))
        .textContentType(TMP_InputField.ContentType.EmailAddress)
        .keyboardType(TouchScreenKeyboardType.EmailAddress)
        .textFieldStyle(fieldStyle);

    SecureField("Password", text: password, prompt: Text("Password"))
        .textFieldStyle(fieldStyle);

    Button("Sign in", SignIn)
        .frame(infiniteWidth: true, height: 44)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 10));
}, spacing: 12f, alignment: VStackAlignment.Leading);`)
              },
              {
                title: "Toolbar",
                body: code(`
HStack(() =>
{
    Button("Cancel", Cancel)
        .buttonStyle(ButtonStyles.Plain(accent));

    Spacer();

    Text("Edit Profile").bold();

    Spacer();

    Button("Done", Save)
        .buttonStyle(ButtonStyles.Filled(accent, Color.white, 8));
}, spacing: 8f, alignment: HStackAlignment.Center)
.padding(12)
.background(Color.white);`)
              },
              {
                title: "Empty state",
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
              }
            ],
            related: ["layout", "controls", "text-input", "modifiers"]
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
            summary: "A detailed guide to modifier parameter types and what each value means.",
            keywords: "modifier parameter frame padding background overlay foregroundColor tint opacity cornerRadius border shadow clipped clipShape font fontSize lineLimit multilineTextAlignment offset position rotationEffect scaleEffect disabled hidden allowsHitTesting onAppear onChange update animation focused contentMargins caretColor textSelectionColor keyboardType scrollIndicators",
            sections: [
              {
                title: "Size and layout",
                body: `
                  <p><code>frame</code> controls size. <code>padding</code> adds inner spacing. Use <code>frame(infiniteWidth: true)</code> for full-width controls.</p>
                  ${modifierParamTable("en", modifierParamsEn.layout)}
                  ${code(`
Text("Primary action")
    .padding(Edge.Horizontal, 16)
    .padding(Edge.Vertical, 10)
    .frame(infiniteWidth: true)
    .background(Color.white);`)}
                `
              },
              {
                title: "Appearance and layers",
                body: `
                  <p><code>background</code> and <code>overlay</code> can receive colors or views. Modifier order changes the result.</p>
                  ${modifierParamTable("en", modifierParamsEn.visual)}
                `
              },
              {
                title: "Text",
                body: modifierParamTable("en", modifierParamsEn.text)
              },
              {
                title: "Movement, rotation, and scale",
                body: `
                  <p><code>offset</code> moves the rendered view without changing layout. <code>position</code> places the RectTransform directly.</p>
                  ${modifierParamTable("en", modifierParamsEn.transform)}
                `
              },
              {
                title: "Interaction, lifecycle, and animation",
                body: modifierParamTable("en", modifierParamsEn.behavior)
              },
              {
                title: "TextField",
                body: `
                  <p>For text fields, separate visual chrome from input behavior. Use <code>textFieldStyle</code> and <code>contentMargins</code> for appearance, and <code>textContentType</code> or <code>keyboardType</code> for input rules.</p>
                  ${modifierParamTable("en", modifierParamsEn.input)}
                `
              },
              {
                title: "Image / Picker / ScrollView",
                body: modifierParamTable("en", modifierParamsEn.mediaScroll)
              }
            ],
            related: ["modifiers", "text-input", "api-index"]
          },
          {
            id: "api-index",
            title: "API Index",
            kind: "Reference",
            summary: "A compact list of factories, modifiers, styles, and types you use when building UI.",
            keywords: "Text Button Image Rectangle Circle Capsule RoundedRectangle Color Divider ProgressView Stepper Picker Label VStack HStack LazyVStack LazyHStack ZStack Grid GridRow GeometryReader Spacer Slider ScrollView Toggle TextField TextEditor SecureField TabView Tab ForEach Build frame background foregroundColor tint padding fixedSize bold italic underline strikethrough lineLimit multilineTextAlignment opacity fontSize font cornerRadius shadow border overlay offset aspectRatio clipped clipShape disabled hidden layoutPriority allowsHitTesting onChange onAppear update animation rotationEffect scaleEffect position buttonStyle textFieldStyle focused onEditingChanged onSubmit selectAllOnFocus textSelectionColor contentMargins textContentType textInputLimit caretColor caretWidth caretBlinkRate keyboardType resizable scaledToFit scaledToFill renderingMode pickerStyle scrollBounce scrollSensitivity scrollMovementType scrollPositionY scrollPositionX scrollIndicators State Animation Axis Edge RectCorner",
            sections: [
              { title: "Factories", body: localizedApiTable("en", apiRows.factories) },
              { title: "Modifiers", body: localizedApiTable("en", apiRows.modifiers) },
              { title: "TextField modifiers", body: localizedApiTable("en", apiRows.input) },
              { title: "Image / Picker / Scroll", body: localizedApiTable("en", apiRows.imageScroll) },
              { title: "Types", body: localizedApiTable("en", apiRows.types) }
            ],
            related: ["modifier-parameters", "overview", "setup"]
          }
        ]
      }
    ]
  }
};

const aliases = {
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
  picker: "controls",
  image: "image-shape",
  shapes: "image-shape",
  scrollview: "scroll-tabs",
  tabview: "scroll-tabs",
  withanimation: "animation",
  modifiers: "modifiers"
};

const symbolList = document.getElementById("symbol-list");
const article = document.getElementById("article");
const tocList = document.getElementById("toc-list");
const searchInput = document.getElementById("search-input");
const languageButtons = [...document.querySelectorAll("[data-lang]")];
const themeButtons = [...document.querySelectorAll("[data-theme-choice]")];
const themeToggle = document.querySelector(".theme-toggle");
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
    ...(page.sections || []).map(section => section.title)
  ].join(" ").toLowerCase();
  return stripHtml(haystack).includes(query);
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
  return String(value).toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "") || "section";
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

if (systemDarkQuery.addEventListener) {
  systemDarkQuery.addEventListener("change", applyTheme);
} else {
  systemDarkQuery.addListener(applyTheme);
}

searchInput.addEventListener("input", renderSidebar);
window.addEventListener("hashchange", () => {
  renderArticle();
  document.getElementById("main").focus({ preventScroll: true });
  window.scrollTo({ top: 0, behavior: "auto" });
});

applyTheme();
renderChrome();
renderArticle();
