# System Patterns

*   **Architecture:** Model-View-ViewModel (MVVM) is the primary architectural pattern, typical for WPF applications.
    *   **Models:** Represent application data (`SmartStore/Models/`).
    *   **Views:** Define the UI structure and layout (XAML files in `SmartStore/Views/`).
    *   **ViewModels:** Contain presentation logic, state, and expose data/commands to the Views (`SmartStore/ViewModels/`).
*   **Key Technical Decisions:**
    *   **WPF:** Chosen for desktop UI development on Windows.
    *   **MVVM:** Standard pattern for WPF, promoting separation of concerns and testability.
    *   **WebSockets:** For real-time communication (`WebSocketService.cs`).
    *   **REST API:** For backend interaction (`ApiService.cs`).
    *   **Secure Storage:** For sensitive data like credentials (`SecureStorageHelper.cs`).
*   **Component Relationships:**
    *   `App.xaml.cs`: Application entry point, likely sets up initial navigation or services.
    *   `MainWindow.xaml`/`.cs`: Main application shell, hosts different views.
    *   `NavigationService.cs`: Manages transitions between different Views/ViewModels.
    *   `ApiService.cs`: Centralizes communication with the backend API.
    *   `ViewModelBase.cs`: Provides common functionality for ViewModels (e.g., `INotifyPropertyChanged`).
    *   `RelayCommand.cs`: Standard implementation for `ICommand` used in MVVM.
*   **Critical Implementation Paths:**
    *   **Login Flow:** `LoginView` -> `LoginViewModel` -> `ApiService` -> `NavigationService` (on success).
    *   **Order Creation:** `OrderView` -> `OrderViewModel` -> `ApiService`.
    *   **Real-time Updates:** `WebSocketService` -> Updates relevant ViewModels (e.g., `MainViewModel`, `OrderViewModel`).

*(This file will be updated as the system's design and patterns become clearer through code analysis.)*
