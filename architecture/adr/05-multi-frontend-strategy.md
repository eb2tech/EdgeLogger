# 0005 – Multi‑frontend strategy (Web + Mobile)

- **Status:** Accepted  
- **Date:** 2026‑02‑26  

## Context
EdgeLogger requires both a desktop‑friendly UI and a mobile‑friendly UI. Blazor WebAssembly and .NET MAUI share a common codebase and ecosystem.

## Decision
Use Blazor for the web frontend and .NET MAUI for the mobile frontend, sharing DTOs and API contracts.

## Consequences
- **Positive:**  
  - Shared C# models and services  
  - Consistent UX patterns  
  - Faster development across platforms  
- **Negative:**  
  - Requires maintaining two UI projects  
  - Some platform‑specific differences must be handled

