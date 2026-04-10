# Celestial Mechanics Desktop

## Project Report (Start to Current State)

This repository has evolved from an initial prototype into a multi-layered desktop simulation platform with .NET, WPF, OpenGL rendering, advanced celestial physics, and optional native CUDA acceleration.

**Current snapshot (as of latest commit in this branch):**

- Latest commit: `283ea21` (2026-04-09) - `Add DefaultSimulationScenario class for reusable two-body orbit setup`
- SDK pin: `.NET 8.0.418` (`global.json`)
- Solution: `CelestialMechanics.sln`
- Primary app target: Windows desktop (`net8.0-windows`, WPF)

---

## 1. Technology Stack

| Area | Stack / Tooling |
| :--- | :--- |
| Application platform | .NET 8, C# |
| Desktop UI | WPF (`UseWPF=true`), MVVM (`CommunityToolkit.Mvvm`) |
| Rendering | Silk.NET (OpenGL, Input, Windowing, Maths) |
| Simulation core | Custom ECS-like simulation framework in C# |
| Physics | Custom Newtonian + advanced astrophysics modules |
| Native acceleration | C++20 + CUDA (CMake build, shared/static libs) |
| Testing | xUnit + Microsoft.NET.Test.Sdk + coverlet |
| CI | GitHub Actions (build on Win/Linux/macOS, tests + coverage on Linux) |

---

## 2. Languages and Runtime Components

1. **C# / .NET 8**: Main application, physics layers, simulation systems, data model, serialization, and tests.
2. **XAML**: WPF view definitions for desktop UI panels, modals, and layout.
3. **C++20 + CUDA**: Native engine for high-performance compute paths and interop with C#.
4. **YAML (GitHub Actions)**: CI workflows for restore/build/test/coverage pipelines.
5. **PowerShell**: Publishing and lock-diagnostics automation (`publish.ps1`, `detect_lock.ps1`).

---

## 3. Solution Architecture and Modules

The solution is split into focused projects under `src\` and `tests\`.

### Core source projects

- `CelestialMechanics.Math`: mathematical primitives and constants (`Vec3d`, `Mat4d`, `Quaterniond`, `PhysicalConstants`, `UnitConversion`)
- `CelestialMechanics.Physics`: force models, integrators, solver backends, collision systems, relativistic corrections, astrophysical extensions
- `CelestialMechanics.Simulation`: simulation engine, entity/component model, systems, events, factories, time and metric managers
- `CelestialMechanics.Renderer`: OpenGL render pipeline (camera, grid, stars, trails, sphere instancing, selection helpers, relativistic/accretion visuals)
- `CelestialMechanics.Data`: observation/catalog and template datasets (`ObservationCatalog`, `ObjectTemplates`, `GravityStrengthMap`)
- `CelestialMechanics.AppCore`: app-domain orchestration (modes, scene graph, snapshotting, serialization, determinism validation, default scenario)
- `CelestialMechanics.Desktop`: WPF desktop host, viewmodels, services, views/panels/modals, OpenGL host/render loop integration
- `CelestialMechanics.App`: app entry/wiring layer

### Test projects

- `CelestialMechanics.Math.Tests`
- `CelestialMechanics.Physics.Tests`
- `CelestialMechanics.Simulation.Tests`
- `CelestialMechanics.AppCore.Tests`

---

## 4. Physics and Scientific/Simulation Capabilities Implemented

From commit history and code structure, the implemented physics stack includes:

1. **N-body gravity foundation**
   - Newtonian gravity force model
   - Two-body orbit scenarios and reusable defaults

2. **Time integration methods**
   - Euler, Verlet, RK4
   - SoA-specific Verlet integration path

3. **Collision and merger mechanics**
   - Collision detection and resolution
   - Merge policies and collision event propagation
   - Remnant/merger-related systems in simulation layer

4. **Advanced physics enhancements**
   - Adaptive timestep behavior
   - SIMD-oriented compute path
   - Barnes-Hut acceleration structures (`OctreeNode`, `OctreePool`) and backend
   - Post-Newtonian / relativistic correction components
   - Gravitational-wave analysis buffers/models
   - Accretion disk modeling
   - Astrophysical utilities (e.g., Roche limit, Schwarzschild radius)

5. **Validation and physical consistency utilities**
   - Momentum validation
   - Force error analysis
   - Energy calculators/budget tracking
   - Determinism and serialization precision improvements (tracked in history)

---

## 5. UI/UX Work Delivered

Desktop UI has matured into a structured WPF application with simulation controls and project workflows:

- Main window + viewport panel host
- Scene outliner, body inspector, simulation settings panels
- Mode and navigation state management
- File/project modals (new project, project list, simulation/file menu)
- Service layer for simulation lifecycle, scene management, project persistence
- Runtime controls wired for play/pause/reset and live metrics
- OpenGL render loop and host integration for interactive visualization

---

## 6. Native Engine and GPU Acceleration Track

The `engine\` subtree establishes a native performance track:

- CMake project with `CXX` and `CUDA` languages enabled
- C++20 + CUDA 20 standards configured
- CUDA architectures configured for modern GPUs (`75;80;86;89;90`)
- Shared and static library outputs for interop (`celestial_engine`)
- Test and benchmark toggles in CMake options
- Native physics interop hooks in managed physics layer (`NativePhysicsInterop`, native GPU backend classes)

This indicates both managed and native execution backends were designed for scalability and performance experimentation.

---

## 7. Testing and Quality Assurance

Automated quality gates are present at project and CI levels:

- Unit tests across Math, Physics, Simulation, and AppCore layers
- Physics-focused tests cover orbit behavior, gravity, integrators, collisions, Barnes-Hut, SIMD parity, adaptive time step, relativistic behavior, and accretion/gravitational-wave related scenarios
- AppCore tests include scene graph, snapshot, serialization, and determinism coverage
- CI workflow:
  - Cross-platform build verification (`ubuntu-latest`, `windows-latest`, `macos-latest`)
  - Linux test execution with Cobertura coverage artifact export

---

## 8. Work Timeline (Chronological Progress)

Based on repository history:

| Date | Commit | Work Delivered |
| :--- | :--- | :--- |
| 2026-02-19 | `9613ea4` | Initial commit |
| 2026-02-19 | `2beaa1b` | Phase 1: core 3D engine, N-body gravity, Verlet integrator, OpenGL rendering |
| 2026-02-19 | `a5e02d5`, `6f6ace1` | README and repository onboarding/documentation refinement |
| 2026-02-21 | `5f1be1f` | Phase 4/5 hybrid physics upgrade: collisions, radius logic, adaptive dt, SIMD |
| 2026-02-21 | `f6046bf` | Relativistic physics (P6) + Barnes-Hut enhancements (P7) |
| 2026-02-22 | `f12cc22` | Determinism fixes, accretion updates in AoS path, serialization precision improvements |
| 2026-02-22 | `1c32ff8` | Native C++/CUDA engine, AppCore layer, and simulation ECS framework added |
| 2026-02-24 | `3db82d5` | GPU compute architecture and high-performance engine backbone (Phase 12-21) |
| 2026-04-06 | `cd40b99` | WPF desktop UI integration and renderer/frontend enhancements |
| 2026-04-06 | `9a5e6b2` | Runtime simulation wiring: default scene, live metrics, play/pause/reset |
| 2026-04-07 | `52a970a` | Professional README cleanup/update |
| 2026-04-08 | `1f2a4f6` | Core simulation components, engine utilities, CUDA integration, initial test infrastructure |
| 2026-04-09 | `283ea21` | Reusable `DefaultSimulationScenario` for two-body orbit setup |

---

## 9. Publishing and Operational Reliability Notes

A major operational issue documented during publish cycle was `MSB4018`/`IOException` caused by file locks when `publish\CelestialMechanics.Desktop.exe` was still running.

Relevant repository assets:

- `EXECUTION_PROBLEM_REPORT.md` (deep investigation and mitigation guidance)
- `publish.ps1` and `detect_lock.ps1` (workflow automation and diagnostics)

Practical status:

- Issue characterized as execution-state locking rather than compile-time code failure
- Lock-handling and process cleanup steps are part of current operational guidance

---

## 10. Current Overall Status

At this point, the project is a feature-rich simulation desktop application with:

- Multi-project clean architecture in .NET 8
- Working WPF + OpenGL visualization stack
- Broad celestial physics feature set (classical + advanced extensions)
- Optional native CUDA compute path
- Automated CI and substantial unit-test coverage areas
- Documented operational troubleshooting for publish reliability

In short, the repository has progressed from a foundational simulation prototype to a structured, extensible, high-performance desktop simulation platform.
