<p align="center">
  <img src="./docs/assets/uniftui-docs-icon.png" alt="UniftUI" width="128">
</p>

# UniftUI

[English](./README.md)

Unity **uGUI**（Canvas）上で、SwiftUI 風に宣言的に UI を組み立てるライブラリです。
**パッケージ:** `com.unift.ui` · **作者:** nnnnnnn0090

## 要件

Unity **2022.3** 以降（Unity 6 含む）。`com.unity.ugui` と `com.unity.textmeshpro` は本パッケージの依存として解決されます。

## インストール

**ウィンドウ → Package Manager → + → Add package from git URL**

```text
https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI
```

タグやブランチを指定する場合:

```text
https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI#v0.1.0
```

## 使い方

`UniftView` を継承し、`VStack` / `HStack` / `Text` / `Button` などで UI を記述。`using UniftUI;`

```csharp
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
}
```

## 互換性とテスト

UniftUI は public factory と fluent modifier alias の互換性を patch release で維持します。Runtime テストは `UniftUI/Tests/Runtime` にあり、`Example` プロジェクトを Unity で開いて Test Runner から実行できます。

Docs: https://nnnnnnn0090.github.io/UniftUI/

## ライセンス

[MIT License](./UniftUI/LICENSE.md).

**サードパーティ:** 角丸 UI は [Unity UI Rounded Corners](https://github.com/kirevdokimov/Unity-UI-Rounded-Corners)（MIT, © 2019 Kirill Evdokimov）由来。該当ファイルを再配布する際は上流の表示を残してください。
