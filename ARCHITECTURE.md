# EdgeLogger Architecture

EdgeLogger is a distributed diagnostics and logging platform designed for edge devices with intermittent connectivity. It consists of multiple coordinated projects that together provide ingestion, buffering, visualization, and orchestration.

---

## System Context

EdgeLogger ingests logs and telemetry from edge devices, buffers them locally on a Raspberry Pi Zero 2 W, forwards them through NATS, and exposes configuration and diagnostics through web and mobile frontends. Aspire orchestrates the system during development.

---

## Subsystems

### EdgeLogger.ApiService (Edge Device)
- Runs headless on Raspberry Pi Zero 2 W  
- Manages WiFi provisioning via balena wifi‑connect  
- Maintains a local datastore for offline durability  
- Buffers logs using an internal `Channel<T>`  
- Forwards messages to NATS when available  
- Implements a state machine for provisioning, connectivity, and ingestion  
- Exposes a minimal HTTP API for diagnostics

### EdgeLogger.Web (Blazor)
- Provides configuration UI and visualization  
- Communicates with ApiService and cloud endpoints  
- Uses component‑based architecture with DI and shared DTOs

### EdgeLogger.Mobile (MAUI)
- Provides mobile access to configuration and diagnostics  
- Shares API contracts and DTOs with Web  
- Uses MVVM patterns where appropriate

### EdgeLogger.AppHost (Aspire)
- Defines service composition for local development  
- Manages environment configuration, secrets, and wiring  
- Provides dashboards and diagnostics

### EdgeLogger.ServiceDefaults
- Centralizes hosting, logging, and DI conventions  
- Ensures consistency across all services

### EdgeLogger.Tests
- Contains automated tests for core logic and API behavior

---

## Cross‑Cutting Concerns

- **Logging:** Structured logging via `ILogger<T>`  
- **Configuration:** Strongly typed options with validation  
- **Networking:** Resilient retry patterns for NATS and HTTP  
- **Storage:** Local durable store on Pi; cloud storage optional  
- **Provisioning:** WiFi provisioning via wifi‑connect  
- **Deployment:** Pi uses systemd; Aspire used for local orchestration

---

## Decision History

Architectural decisions are recorded as ADRs under `/architecture/adr`.

Initial ADRs:

- `0001-use-aspire-for-orchestration.md`  
- `0002-use-nats-for-ingestion.md`  
- `0003-use-wifi-connect-for-provisioning.md`  
- `0004-local-datastore-strategy.md`  
- `0005-multi-frontend-strategy.md`

---

## Future Directions

- Cloud ingestion pipeline  
- Long‑term storage and analytics  
- Enhanced mobile UI  
- OTA updates for ApiService  
- Additional edge device integrations

---

