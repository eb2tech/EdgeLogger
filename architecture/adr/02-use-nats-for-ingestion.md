# 0002 – Use NATS for ingestion

- **Status:** Accepted  
- **Date:** 2026‑02‑26  

## Context
EdgeLogger requires a lightweight, reliable, message‑driven transport for logs and telemetry. MQTT was considered but is less suited for high‑frequency, low‑latency ingestion.

## Decision
Use NATS as the primary ingestion and forwarding mechanism.

## Consequences
- **Positive:**  
  - High throughput and low latency  
  - Simple client libraries  
  - Easy to deploy locally and in the cloud  
- **Negative:**  
  - Requires a running NATS server  
  - Adds operational complexity for cloud deployment

