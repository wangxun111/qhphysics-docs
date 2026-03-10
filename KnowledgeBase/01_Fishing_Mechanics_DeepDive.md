// ...existing code...
### Why this works?
It introduces **Hysteresis (滞回)** in both *Time* (Lock) and *Magnitude* (Integral), filtering out the rapid `1.1772` friction noise.

## Root Cause Analysis (2026-03-05)
**Diagnosis**: "The bouncing of the fishing line on the surface (封层/Water/Ice) affects the reeling force fluctuation."
- **Physics Origin**: When the line's physics nodes interact with the surface boundary (Collision or Drag change), it creates high-frequency force spikes.
- **Symptom**: This correlates with the `friction=1.1772` 2.5Hz jitter observed in logs.
- **Solution Validation**: Scheme A is effective here because "bounces" are transient. They generate force spikes but fail to accumulate enough **Integral Score** to trigger a state flip, thus stabilizing the UI.

