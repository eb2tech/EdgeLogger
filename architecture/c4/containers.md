
---

# **C4 Level 2 — Container Diagram (`containers.md`)**

```markdown
# C4 Level 2 — Container Diagram: EdgeLogger

```mermaid
C4Container
    title EdgeLogger — Container Diagram

    Person(user, "User")
    Person(technician, "Technician")

    System_Boundary(edgeLogger, "EdgeLogger") {

        Container(apiService, "ApiService", "C# .NET", "Runs on Raspberry Pi Zero 2 W. Handles provisioning, ingestion, buffering, and forwarding via NATS.")
        Container(webApp, "Web App", "Blazor WebAssembly", "Configuration UI and log visualization.")
        Container(mobileApp, "Mobile App", ".NET MAUI", "Mobile configuration and diagnostics.")
        Container(appHost, "AppHost", "Aspire", "Local orchestration, dashboards, wiring.")
        Container(serviceDefaults, "ServiceDefaults", "C# Library", "Shared hosting/logging conventions.")
        ContainerDb(localStore, "Local Datastore", "File-based / SQLite (future)", "Durable offline buffer on Pi.")
        Container(nats, "NATS Server", "NATS", "Message broker for logs and telemetry.")
    }

    Rel(user, webApp, "Uses")
    Rel(user, mobileApp, "Uses")
    Rel(technician, apiService, "Installs, monitors")

    Rel(apiService, localStore, "Reads/Writes logs")
    Rel(apiService, nats, "Publishes logs/telemetry")
    Rel(webApp, apiService, "Calls HTTP API")
    Rel(mobileApp, apiService, "Calls HTTP API")
    Rel(appHost, apiService, "Orchestrates")
    Rel(appHost, nats, "Orchestrates")

