# Tech Context

*   **Core Technologies:**
    *   **.NET Framework / .NET Core/Standard:** (Exact version needs confirmation from `.csproj` file) - The underlying runtime and base class library.
    *   **C#:** Primary programming language.
    *   **WPF (Windows Presentation Foundation):** UI framework for building the desktop application.
    *   **XAML:** Markup language used to define WPF UI structure and appearance.
*   **Key Libraries/Packages (Inferred/Expected):**
    *   **Newtonsoft.Json or System.Text.Json:** For JSON serialization/deserialization (interacting with APIs).
    *   **Microsoft.Xaml.Behaviors.Wpf (or similar):** Often used for attaching behaviors to UI elements in MVVM.
    *   **AForge.NET / Emgu CV / Accord.NET (or similar):** Likely used for camera interaction (`CameraSelectorViewModel`, `ImageHelper`). Needs confirmation.
    *   **QRCoder / ZXing.Net:** For QR code generation/reading (`QrCodeGenerator.cs`). Needs confirmation.
    *   **System.Net.WebSockets.Client:** For WebSocket communication (`WebSocketService.cs`).
    *   **System.Security.Cryptography:** Used by `SecureStorageHelper.cs` for data protection.
*   **Development Environment:**
    *   **IDE:** Visual Studio (implied by `.sln` and `.csproj` files).
    *   **Build System:** MSBuild (via `build.cmd` or Visual Studio).
    *   **Version Control:** Git (implied by `.gitattributes`, `.gitignore`).
*   **Technical Constraints:**
    *   Requires Windows operating system.
    *   Depends on .NET runtime being installed.
    *   Network connectivity required for API and WebSocket features.
*   **Tool Usage Patterns:**
    *   Visual Studio for development, debugging, and building.
    *   Git for source code management.

*(This file needs verification and refinement by inspecting the `SmartStorePOS.csproj` file for specific package references and the target .NET framework version.)*
