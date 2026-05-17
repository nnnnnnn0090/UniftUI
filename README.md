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

With tag or branch:

```text
https://github.com/nnnnnnn0090/UniftUI.git?path=UniftUI#v0.1.0
```

## Usage

Subclass `UniftView`, build UI with `VStack`, `HStack`, `Text`, `Button`, etc. `using UniftUI;`  

Docs: https://nnnnnnn0090.github.io/UniftUI/docs/  
Local docs: [docs/index.html](./docs/index.html)

## License

[MIT License](./UniftUI/LICENSE.md).  

**Third-party:** Rounded corners from [Unity UI Rounded Corners](https://github.com/kirevdokimov/Unity-UI-Rounded-Corners) (MIT, © 2019 Kirill Evdokimov). Retain that notice when redistributing those files.
