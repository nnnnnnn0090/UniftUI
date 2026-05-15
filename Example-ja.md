# UniftUI — コード例

[English](./Example.md)

はじめて画面を作る人向けの手順です。C# で UI の「設計図」を書き、最後に `.Build(canvas)` で Unity の uGUI オブジェクトにします。

## はじめる前に（Unity エディタ）

1. **Canvas** があるシーンを用意する（Screen Space で問題ありません）。
2. 空の GameObject（例: `MyUI`）を作り、**`UniftView` を継承したスクリプト**をアタッチする。
3. スクリプト先頭に `using UniftUI;` を書く。
4. `Start()` の中で UI を書き、**同じ GameObject 上の Canvas** に対して **`.Build(GetComponent<Canvas>())`** で確定する。

何も表示されないときは、スクリプトが **Canvas と同じ GameObject** に付いているか、Console にエラーが出ていないかを確認してください。

## 基本の流れ

どの画面も次の 3 ステップです。

1. **`UniftView` を継承** — `VStack` / `Text` / `Button` などのヘルパーが使える。
2. **レイアウトの `{ }` の中に UI を書く** — 例: `VStack(() => { ... })`。
3. **`.Build(GetComponent<Canvas>())`** — 設計図から実際の `RectTransform` / uGUI を生成する。

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

**ヒント:** `VStack` の中に `HStack`、`HStack` の中に `Text`…と入れ子にできます。

**上級者向け:** `UniftView` の外では `UIElements.VStack(...)` も使えますが、`UIContext` の親子ルールが必要です。慣れるまでは `UniftView` だけで十分です。

---

## レイアウト（スタック）

子要素を縦・横に並べる、または重ねるコンテナです。

### VStack — 上から下

メニュー、フォーム、縦に並べた一覧向け。

| 引数 | 意味 |
|------|------|
| `spacing` | 子ども同士の間隔（ピクセル） |
| `alignment` | 横方向の揃え（例: `Leading` = 左寄せ） |

```csharp
VStack(() =>
{
    Text("タイトル");
    Text("サブタイトル");
    Button("次へ", () => Debug.Log("next"));
}, spacing: 12f, alignment: VStackAlignment.Leading);
```

### HStack — 左から右

ツールバー、横並びボタン、ラベル＋値の 1 行向け。

```csharp
HStack(() =>
{
    Text("左");
    Spacer(8);   // 余白があるとき、右側の要素を端に押しやすくする
    Text("右");
}, spacing: 16f, alignment: HStackAlignment.Center);
```

### ZStack — 重ねる（奥 → 手前）

背景の上に文字、バッジをアイコンに載せる、など。

```csharp
ZStack(() =>
{
    Image(mySprite).Opacity(0.5f);   // 奥
    Text("手前");                     // 前
}, alignment: ZStackAlignment.Center);
```

---

## Grid（表）

**行（`GridRow`）** を **`Grid`** の中に並べます。電卓のキー配置や、列を揃えた表向け。

- `GridRow` は **必ず `Grid` の内側** で使う。
- 列幅は行どうしで揃います。

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

## Text（テキスト）

| 書き方 | 使う場面 |
|--------|----------|
| `Text("固定")` | 文字列が変わらない |
| `Text(myStringState)` | `State<string>` に文字列を保持 |
| `Text(() => "…", new State[] { x })` | `x` が変わったとき表示を更新 |

```csharp
// シンプル
Text("こんにちは");

// 状態に連動（score が変わると文言を更新）
var score = new State<int>(0);
Text(() => $"スコア: {score.Value}", new State[] { score });
```

**見た目** — `Text(...)` のあとにチェーン:

```csharp
Text("タイトル")
    .FontSize(24)
    .Bold()
    .ForegroundColor(Color.white)
    .Font(myTmpFontAsset);   // TMP のフォントアセット
```

---

## Button（ボタン）

第 1 引数: ラベルまたは任意の UI。第 2 引数: 押したときの処理。

```csharp
Button("OK", () => Debug.Log("clicked"));

// 修飾子なしで色だけ指定
Button("削除", () => { }, backgroundColor: Color.red, textColor: Color.white);

// アイコンボタン: ラベル部分に任意の UIElement
Button(
    Image(iconSprite).Frame(width: 24, height: 24),
    () => Debug.Log("icon tap"));
```

---

## Image（画像）

**Sprite** が必要です（Inspector で割り当てるか `Resources` から読み込み）。

```csharp
Image(sprite)
    .TintColor(Color.white)
    .ScaleMode(ImageScaleMode.AspectFit)   // はみ出さずに収める
    .Opacity(0.9f);
```

---

## Spacer（伸びる空白）

スタック内の「空き」を埋める（`Spacer`）。

- **`Spacer(8)`** — 主軸方向の最小 8。親に余裕があれば **それ以上に伸びる**。
- **常に 8px だけ空けたい** 場合は `.Padding()` や `.Frame()` を使う。

---

## TabView（タブ）

上にタブ、選んだタブの内容だけ表示。

```csharp
var selectedTab = new State<int>(0);   // 0 = 最初のタブ

TabView(() =>
{
    Tab("ホーム", () => { Text("ホームの内容"); });
    Tab("設定", () => { Text("設定の内容"); });
}, selectedTab);
```

---

## Slider（スライダー）

**`State<int>`**（渡した State）と連動。ドラッグで値が変わり、バインドした UI が更新されます。

```csharp
var volume = new State<int>(50);
Slider(volume, minValue: 0, maxValue: 100);
```

---

## Toggle（トグル）

オン/オフを **`State<bool>`** で保持（チェックボックス風）。

```csharp
var soundOn = new State<bool>(true);
Toggle(soundOn, "サウンド", isOn => Debug.Log($"Sound: {isOn}"));
```

---

## TextField（入力欄）

入力テキストは **`State<string>`** に入ります。

```csharp
var name = new State<string>("");
TextField(name, "名前を入力…", value => Debug.Log(value))
    .FontSize(18)
    .LineLimit(3);   // 最大行数（任意）
```

---

## ScrollView（スクロール）

内容が表示領域より大きいとき。

```csharp
ScrollView(() =>
{
    for (int i = 0; i < 40; i++)
        Text($"行 {i}");
}, horizontal: false, vertical: true)
    .ScrollIndicators(ScrollIndicatorVisibility.Hidden)
    .ScrollBounce(true);
```

---

## ForEach — 同じ UI を繰り返す

### 閉区間（両端を含む）

`ForEach(0, 9, …)` → `i` は 0, 1, …, **9**（10 個）。

```csharp
ForEach(0, 9, i =>
{
    Button(i.ToString(), () => Debug.Log(i));
});
```

### 半開区間（C# の `Range`）

`ForEach(0..10, …)` → `i` は 0 … **9**（10 個）。`for (int i = 0; i < 10; i++)` と同じ。

```csharp
ForEach(0..10, i =>
{
    Button(i.ToString(), () => { });
});
```

### リストや配列

```csharp
ForEach(items, item =>
{
    Text(item.Name);
});
```

---

## State とアニメーション

### `State<T>` とは？

値を入れておく箱。**`state.Value = …`** で書き換えると、それに依存する UI（`State[]` やバインド）が更新されます。

```csharp
var counter = new State<int>(0);

Button("+1", () => counter.Value++);

Text(() => $"回数: {counter.Value}", new State[] { counter });
```

### 変化をアニメーション

**`WithAnimation`** で囲む:

```csharp
var width = new State<float>(100f);

WithAnimation(Animation.easeInOut(0.25f), () =>
{
    width.Value = 200f;
});

// または: width が変わったときだけこの要素をアニメート
SomeElement()
    .Animation(Animation.Default, width);
```

---

## よく使う修飾子（要素の後ろにチェーン）

戻り値が要素（またはラッパー）なので、つなげて書けます:

```csharp
Text("カード")
    .Padding(16)                              // 内側の余白
    .Background(Color.blue)                   // 背景色
    .CornerRadius(8)                          // 角丸（UniftUI）
    .Frame(width: 200, height: 48)            // 希望サイズ
    .Offset(0, -4);                           // 位置をずらす

Text("影付き")
    .Shadow(Color.black, radius: 2f, x: 1f, y: -1f);

Text("傾ける")
    .ScaleEffect(1.1f)
    .RotationEffect(5f);

Text("フェード")
    .Opacity(0.85f)
    .OnAppear(() => Debug.Log("表示された"));
```

**`FixedSize`** — 親に引き伸ばされず、中身のサイズに合わせる:

```csharp
Text("内容に合わせる").FixedSize(horizontal: true, vertical: true);
```

---

## 早見表

| やりたいこと | API |
|--------------|-----|
| 縦に並べる | `VStack` |
| 横に並べる | `HStack` |
| 重ねる | `ZStack` |
| 表 | `Grid` + `GridRow` |
| タップ | `Button` |
| 文字入力 | `TextField` + `State<string>` |
| 長い一覧 | `ScrollView` |
| 動的な表示 | `State<T>` + `Text(() => …, new State[] { … })` |
| 見た目 | `.Padding` `.Background` `.Frame` `.FontSize` など |
