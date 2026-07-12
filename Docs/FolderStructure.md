# Folder Structure Mapping

This document describes the directory structure of the Celestial Mechanics project, mapping physical files to their logical design.

## 1. Physical Directory Tree

```
celestial-mechanics-desktop/
├── .gitignore                      # Git exclusion rules
├── CelestialMechanics.sln          # MSBuild Solution
├── Docs/                           # Version-controlled project documentation
│   ├── Simulation/                 # Simulation Mode specific docs
│   └── Observation/                # Observation Mode specific docs
├── AI/                             # Local-only AI Workspace (Git ignored)
│   └── PROMPTS/                    # Custom prompt templates
├── engine/                         # Native C++ engine (Parallel track)
├── src/                            # Managed .NET source code
│   ├── CelestialMechanics.App/     # Silk.NET interactive executable
│   ├── CelestialMechanics.AppCore/ # Core scene graph & mode orchestration
│   ├── CelestialMechanics.Data/    # Serialization & file system data layer
│   ├── CelestialMechanics.Desktop/ # WPF-based IDE desktop launcher
│   ├── CelestialMechanics.Math/    # Vector & astronomical math library
│   ├── CelestialMechanics.Physics/ # Gravitational physics engine
│   ├── CelestialMechanics.Renderer/# Silk.NET + OpenGL renderer
│   └── CelestialMechanics.Simulation/# Simulation ViewModels & UI logic
└── tests/                          # Automated test suites
    ├── CelestialMechanics.AppCore.Tests/
    ├── CelestialMechanics.Math.Tests/
    ├── CelestialMechanics.Physics.Tests/
    └── CelestialMechanics.Simulation.Tests/
```

## 2. Logical Reorganization Mapping

To align the physical structure with target guidelines without breaking active import namespaces and solution layouts, the projects correspond to target logical layers as follows:

| Target Logical Project | Physical Location | Description / Mapping Strategy |
|---|---|---|
| **CelestialMechanics.Core** | `src/CelestialMechanics.AppCore` | Core framework orchestration, selection context, and scene nodes. |
| **CelestialMechanics.Common** | Shared Interfaces / Core | Handled in Core/Math namespaces currently. |
| **CelestialMechanics.Math** | `src/CelestialMechanics.Math` | Low-level orbital mechanics & vectors. |
| **CelestialMechanics.Physics** | `src/CelestialMechanics.Physics` | Core simulator integrator algorithms. |
| **CelestialMechanics.Rendering** | `src/CelestialMechanics.Renderer` | Custom OpenGL rendering pipeline. |
| **CelestialMechanics.Resources** | `src/CelestialMechanics.Desktop/Assets` | Assets embedded in launchers and renderer. |
| **CelestialMechanics.Data** | `src/CelestialMechanics.Data` | File readers, scenario serializers (`.cesim`). |
| **CelestialMechanics.Simulation**| `src/CelestialMechanics.Simulation` | Simulation mode workspace and UI workflows. |
| **CelestialMechanics.Observation**| `Docs/Observation` (Spec) | Ready for future implementation under `src/CelestialMechanics.Observation`. |
| **CelestialMechanics.Launcher** | `src/CelestialMechanics.Desktop` & `App` | Dual launchers (WPF IDE and direct Silk.NET canvas). |
