# 0003 – Use wifi‑connect for provisioning

- **Status:** Accepted  
- **Date:** 2026‑02‑26  

## Context
Early prototypes used a custom BLE provisioning flow. This added complexity and maintenance burden. The Pi Zero 2 W supports balena wifi‑connect, which provides a stable SoftAP provisioning experience.

## Decision
Use balena wifi‑connect for WiFi provisioning on the Pi.

## Consequences
- **Positive:**  
  - Mature, stable provisioning flow  
  - Less custom code to maintain  
  - Works well with headless devices  
- **Negative:**  
  - Requires Docker‑based setup on the Pi  
  - Less control over provisioning UX

