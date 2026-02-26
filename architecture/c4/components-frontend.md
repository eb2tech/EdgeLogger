
---

# **C4 Level 3 — Component Diagram: Web + Mobile (`components-frontend.md`)**

```markdown
# C4 Level 3 — Component Diagram: Web + Mobile

```mermaid
C4Component
    title Web + Mobile Frontends — Component Diagram

    Container(webApp, "Web App", "Blazor WebAssembly")
    Container(mobileApp, "Mobile App", ".NET MAUI")

    Component(webUi, "UI Components", "Razor Components", "Pages and components for configuration and visualization.")
    Component(webServices, "Web Services", "C#", "HTTP clients, DTOs, state management.")

    Component(mobileUi, "Mobile UI", "XAML + C#", "Screens and views for diagnostics and configuration.")
    Component(mobileViewModels, "ViewModels", "C#", "MVVM state and logic.")
    Component(mobileServices, "Mobile Services", "C#", "HTTP clients, DTOs, shared logic.")

    ComponentShared(sharedDtos, "Shared DTOs", "C#", "Shared models across Web, Mobile, and ApiService.")

    Rel(webUi, webServices, "Uses")
    Rel(webServices, sharedDtos, "Uses")
    Rel(mobileUi, mobileViewModels, "Uses")
    Rel(mobileViewModels, mobileServices, "Uses")
    Rel(mobileServices, sharedDtos, "Uses")

    Rel(webServices, apiService, "Calls HTTP API")
    Rel(mobileServices, apiService, "Calls HTTP API")

