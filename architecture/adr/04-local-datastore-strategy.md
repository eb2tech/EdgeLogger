# 0004 – Local datastore strategy

- **Status:** Accepted  
- **Date:** 2026‑02‑26  

## Context
EdgeLogger must operate offline and buffer logs until connectivity is restored. The datastore must be lightweight, durable, and easy to manage on a Pi Zero 2 W.

## Decision
Use a local file‑based datastore (initially simple JSON or binary records; future option for LiteDB or SQLite).

## Consequences
- **Positive:**  
  - Simple implementation  
  - Durable across reboots  
  - Easy to inspect and debug  
- **Negative:**  
  - Requires careful management of file size and rotation  
  - May need migration to SQLite as features grow

