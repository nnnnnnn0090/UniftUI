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

Code examples: [Example.md](./Example.md) · [日本語](./Example-ja.md)

## License

[MIT License](./UniftUI/LICENSE.md).  

**Third-party:** Rounded corners from [Unity UI Rounded Corners](https://github.com/kirevdokimov/Unity-UI-Rounded-Corners) (MIT, © 2019 Kirill Evdokimov). Retain that notice when redistributing those files.
