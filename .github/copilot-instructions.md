# Copilot Instructions — EdgeLogger

These instructions define how Copilot should understand and contribute to the EdgeLogger system.  
They describe architecture, boundaries, conventions, and decision history.  
For implementation details, consult the code and ADRs.

---

## System Overview

EdgeLogger is a distributed, edge‑to‑cloud logging and diagnostics platform consisting of:

- **EdgeLogger.ApiService** — headless service running on Raspberry Pi Zero 2 W  
- **EdgeLogger.AppHost** — Aspire host for local orchestration  
- **EdgeLogger.Web** — Blazor web UI for configuration and visualization  
- **EdgeLogger.Mobile** — .NET MAUI mobile app  
- **EdgeLogger.ServiceDefaults** — shared Aspire defaults  
- **EdgeLogger.Tests** — automated tests

The system ingests data from edge devices, buffers it locally, forwards it through NATS, and exposes configuration and diagnostics through web and mobile frontends.

---

## Architectural Principles

- **Clear subsystem boundaries**  
  - ApiService handles ingestion, buffering, provisioning, and local storage.  
  - Web and Mobile handle user interaction and visualization.  
  - AppHost orchestrates services using Aspire.  
  - ServiceDefaults defines shared hosting conventions.

- **Edge‑first reliability**  
  The Pi service must operate offline, degrade gracefully, and recover without manual intervention.

- **Message‑driven ingestion**  
  NATS is the primary transport for logs and telemetry.

- **Local durability**  
  ApiService maintains a local datastore for offline buffering and replay.

- **Cross‑platform UI consistency**  
  Web and Mobile follow shared patterns for navigation, state management, and API usage.

- **Small, composable modules**  
  Prefer focused classes and functions over monolithic services.

---

## Code Conventions

- **Language:** C# across all projects  
- **Async:** Use async/await consistently; avoid blocking calls  
- **Dependency Injection:** Use constructor injection; avoid service locator patterns  
- **Logging:** Use structured logging via `ILogger<T>`  
- **Naming:**  
  - Services: `XyzService`  
  - Background tasks: `XyzWorker`  
  - Models: `XyzRecord`, `XyzDto`  
  - Channels/queues: `xyzChannel`  
- **Error handling:**  
  - Fail fast on configuration errors  
  - Fail soft on network/transient errors  
  - Use retries with backoff where appropriate

---

## Subsystem Guidelines

### ApiService (Raspberry Pi)
- Runs as a headless background service.  
- Uses a state machine to manage provisioning, connectivity, and ingestion.  
- Uses NATS for message forwarding.  
- Uses a local datastore for buffering.  
- Must remain resilient to power loss and intermittent connectivity.

### Web (Blazor)
- Uses component‑based architecture.  
- Follows existing patterns for dependency injection, routing, and state.  
- Avoid business logic in components; prefer services.

### Mobile (MAUI)
- Mirrors Web’s conceptual model.  
- Uses MVVM patterns where appropriate.  
- Shares DTOs and API contracts with Web.

### AppHost (Aspire)
- Defines service composition and environment configuration.  
- Centralizes connection strings, secrets, and service wiring.

---

## ADR Workflow

All significant architectural decisions are documented as Architecture Decision Records (ADRs) in the [architecture/adr/](architecture/adr/) directory. Always refer to these documents when making changes that affect the system structure or core technologies.

Create or update an ADR when a change:

- introduces a new subsystem or dependency  
- modifies provisioning, ingestion, or storage strategy  
- changes cross‑cutting conventions  
- affects deployment or orchestration  
- alters boundaries between projects  
- introduces a one‑way‑door decision

---

## When in Doubt

- Maintain subsystem boundaries.  
- Prefer explicit, readable code over clever abstractions.  
- Follow existing patterns before introducing new ones.  
- If a change affects multiple projects or long‑term behavior, write an ADR.

---
