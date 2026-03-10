# Spring Flow (Line-Hook-Rod)

This flow focuses on the spring module and how it links the line, hook, and rod in the physics update.

```mermaid
flowchart TD
    A[Frame start] --> B[Collect active masses]
    B --> C[Integrate masses (Verlet)]
    C --> D[Build constraints]

    D --> S0[Spring constraints]
    S0 --> S1[Resolve line segments as springs]
    S1 --> S2[Hook spring constraint]
    S2 --> S3[Rod tip spring constraint]

    S1 --> S1a[Fetch line mass pairs]
    S1a --> S1b[Delta = p2 - p1]
    S1b --> S1c[Length = |Delta|]
    S1c --> S1d[Stretch = Length - RestLength]
    S1d --> S1e[EffectiveMass = invMass1 + invMass2]
    S1e --> S1f[ImpulseScalar = -Stretch * Stiffness / EffectiveMass]
    S1f --> S1g[Apply position correction]

    S2 --> S2a[Hook attached to line end mass]
    S2a --> S2b[Delta = hookPos - endMassPos]
    S2b --> S2c[Compute stretch vs rest]
    S2c --> S2d[Apply correction to hook and end mass]

    S3 --> S3a[Rod tip mass anchors line start]
    S3a --> S3b[Delta = tipPos - startMassPos]
    S3b --> S3c[Compute stretch vs rest]
    S3c --> S3d[Apply correction to tip and start mass]

    S1g --> SD1[Damping along spring axis]
    S2d --> SD1
    S3d --> SD1

    SD1 --> E[Other constraints]
    E --> F[Finalize positions]
    F --> G[Render]
```

Notes:
- Springs are resolved as position-based constraints; damping reduces oscillation along the spring axis.
- Line is modeled as a chain of spring-connected masses; hook and rod tip add terminal constraints.
