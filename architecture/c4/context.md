# C4 Level 1 — System Context: EdgeLogger

```mermaid
C4Context
    title EdgeLogger — System Context Diagram

    Person(user, "User", "Configures devices, views logs, uses mobile and web apps.")
    Person(technician, "Technician", "Installs and maintains edge devices.")

    System(edgeLogger, "EdgeLogger System", "Distributed edge-to-cloud logging and diagnostics platform.")

    System_Ext(homeAssistant, "Home Assistant (Optional)", "May consume logs or device state.")
    System_Ext(cloud, "Cloud Storage / Analytics (Future)", "Long-term storage and analytics pipeline.")

    Rel(user, edgeLogger, "Configures, views logs via Web/Mobile")
    Rel(technician, edgeLogger, "Installs devices, monitors health")

    Rel(edgeLogger, homeAssistant, "Publishes device state (optional)")
    Rel(edgeLogger, cloud, "Uploads logs/telemetry (future)")

