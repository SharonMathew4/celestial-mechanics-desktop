# Performance Goals & Metrics

This document lists performance benchmarks and system resource budgets for the Celestial Mechanics system.

## 1. Frame-rate Target

- **Goal**: Lock rendering frame-rate at a steady 60 FPS (VSync target) or achieve 120+ FPS on G-Sync/FreeSync systems.
- **Budget**: Maximum of 16.6ms frame time.
  - Physics step: < 2.0ms (for standard 5,000 body arrays).
  - Scene graph updates: < 1.0ms.
  - OpenGL draw calls and UI render: < 10.0ms.

## 2. Memory Consumption

- **Baseline Overhead**: < 200MB RAM for minimal scenarios.
- **Heavy Sandbox scenarios**: < 1.5GB RAM for scenarios with large particle systems or high-resolution planetary textures.
- **Garbage Collection (GC)**: Eliminate runtime allocations (zero allocation loop) inside `Update()` and `Render()` threads to prevent GC spikes.

## 3. GPU/CUDA Integration Targets

- **Massive Simulations**: Support 100,000+ active bodies at >30 FPS.
- **Memory Bandwidth**: Minimize CPU-GPU memory copies. Use OpenGL interoperability with CUDA (`cudaGraphicsGLRegisterBuffer`) for direct GPU-to-GPU particle rendering.
