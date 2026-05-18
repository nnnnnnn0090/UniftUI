<p align="center">
  <img src="../docs/assets/uniftui-docs-icon.png" alt="UniftUI" width="128">
</p>

# UniftUI

SwiftUI-style declarative UI for Unity uGUI.

## Requirements

- Unity 2022.3 or newer
- TextMesh Pro
- Unity UI (`com.unity.ugui`)

Dependencies are declared in `package.json`.

## Install

In Unity Package Manager, choose **Add package from git URL** and enter:

```text
https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI
```

For a tagged release:

```text
https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI#v0.1.0
```

## Documentation

Docs:

```text
https://nnnnnnn0090.github.io/UniftUI/docs/
```

Or open the documentation site from the repository root:

```text
docs/index.html
```

## Compatibility & Tests

Public factories (`Text`, `Button`, `VStack`, etc.) and fluent modifier aliases (`.frame`, `.background`, `.foregroundColor`, etc.) are kept source-compatible across patch releases.

Runtime tests are under `Tests/Runtime`. Open the repository's `Example` project in Unity and run them from Test Runner.

## Quick Start

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
                .background(new Color(0.2f, 0.45f, 0.95f))
                .cornerRadius(12)
                .foregroundColor(Color.white);
        }, spacing: 12f)
        .padding(24)
        .Build(GetComponent<Canvas>());
    }
}
```

## License

MIT. See `LICENSE.md`.

Rounded-corner UI support under `Runtime/UiRoundedCorners/` is based on
[Unity UI Rounded Corners](https://github.com/kirevdokimov/Unity-UI-Rounded-Corners)
by Kirill Evdokimov, also MIT licensed.
