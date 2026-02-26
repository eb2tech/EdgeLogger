# 0001 – Use Aspire for orchestration

- **Status:** Accepted  
- **Date:** 2026‑02‑26  

## Context
EdgeLogger consists of multiple coordinated services (ApiService, Web, Mobile, ServiceDefaults). Local development requires consistent wiring, configuration, and diagnostics.

## Decision
Use .NET Aspire as the orchestration layer for local development and service composition.

## Consequences
- **Positive:**  
  - Unified dashboard for logs, metrics, and service health  
  - Simplified wiring of connection strings and secrets  
  - Consistent hosting model across services  
- **Negative:**  
  - Adds a dependency on Aspire tooling  
  - Requires developers to understand Aspire conventions

