# Project Roadmap

This roadmap details the planned release milestones for the Celestial Mechanics system.

## Phase 1: Core Consolidation & Native Interop (Current)
- [x] Consolidate C# WPF IDE UI.
- [x] Standardize `.cesim` file schema.
- [ ] Implement C++ interop layers for the native `engine` library.
- [ ] Connect CUDA-accelerated N-body solvers for massive scenarios (>100k bodies).

## Phase 2: Observation Mode Foundations
- [ ] Create `CelestialMechanics.Observation` project structure.
- [ ] Integrate NASA/JPL Horizon API clients for planetary ephemerides.
- [ ] Implement database integration for star catalog databases (e.g., Gaia, Hipparcos).
- [ ] Establish tile-based stellar skybox loading.

## Phase 3: Advanced Rendering & Navigation
- [ ] High-Dynamic Range (HDR) exposure controls for realistic stellar brightness.
- [ ] Multi-scale zoom camera allowing navigation from local satellites to outer galaxies.
- [ ] Physics-based light scattering for planetary atmospheres.

## Phase 4: Production Launcher & Deployment
- [ ] Create unified Launcher application with mode selection dashboard.
- [ ] Setup deterministic dependency installer.
- [ ] Release production bundle.
