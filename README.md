<p align="center">
  <img src="./docs/assets/uniftui-docs-icon.png" alt="UniftUI" width="128">
</p>

# UniftUI

[日本語](./README-ja.md)

SwiftUI-style declarative UI for Unity **uGUI** (Canvas).
**Package:** `com.unift.ui` · **Author:** nnnnnnn0090

## Requirements

Unity **2022.3** or newer (including Unity 6). Pulls in `com.unity.ugui` and `com.unity.textmeshpro` via this package.

## Install

**Window → Package Manager → + → Add package from git URL**

```text
https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI
```

With a branch:

```text
https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI#main
```

For released versions, use the version tag shown on GitHub Releases.

## Usage

Subclass `UniftView`, build UI with `VStack`, `HStack`, `Text`, `Button`, etc. `using UniftUI;`

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

## Compatibility & Tests

UniftUI keeps the public factories and fluent modifier aliases compatible across patch releases. Runtime tests live under `UniftUI/Tests/Runtime` and can be run with Unity Test Runner after opening the `Example` project.

Docs: https://nnnnnnn0090.github.io/UniftUI/

## License

[MIT License](./LICENSE).

**Third-party:** Rounded corners from [Unity UI Rounded Corners](https://github.com/kirevdokimov/Unity-UI-Rounded-Corners) (MIT, © 2019 Kirill Evdokimov). Retain that notice when redistributing those files.
