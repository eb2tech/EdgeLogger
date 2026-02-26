
---

# **C4 Level 3 — Component Diagram: ApiService (`components-apiservice.md`)**

This is the most important Level 3 diagram because ApiService is the architectural “center of gravity.”

```markdown
# C4 Level 3 — Component Diagram: ApiService

```mermaid
C4Component
    title ApiService — Component Diagram

    Container(apiService, "ApiService", "C# .NET")

    Component(stateMachine, "Provisioning & Connectivity State Machine", "C#", "Manages WiFi provisioning, connectivity, and service lifecycle.")
    Component(ingestionWorker, "Ingestion Worker", "C#", "Consumes logs from Channel<T>, batches, forwards to NATS.")
    Component(channelBuffer, "Channel Buffer", "C#", "In-memory buffer to decouple producers from NATS.")
    Component(localStore, "Local Datastore Adapter", "C#", "Durable offline storage (file-based or SQLite).")
    Component(natsClient, "NATS Client", "C#", "Publishes messages to NATS.")
    Component(httpApi, "HTTP API", "C#", "Diagnostics and configuration endpoints.")
    Component(wifiConnect, "wifi-connect Integration", "Shell/Docker", "Handles WiFi provisioning via SoftAP.")

    Rel(stateMachine, wifiConnect, "Triggers provisioning")
    Rel(stateMachine, ingestionWorker, "Controls lifecycle")
    Rel(ingestionWorker, channelBuffer, "Consumes")
    Rel(ingestionWorker, natsClient, "Publishes logs")
    Rel(ingestionWorker, localStore, "Reads/Writes offline logs")
    Rel(httpApi, stateMachine, "Exposes state")
    Rel(httpApi, localStore, "Reads logs")

