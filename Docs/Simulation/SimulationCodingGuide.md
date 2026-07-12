# Simulation Mode Coding Guide

This guide is designed for developers working on the Simulation Mode sandbox features.

## 1. Adding a New Physics Integrator

To add a new integration algorithm:
1. Implement the `IIntegrator` interface in `CelestialMechanics.Physics`.
2. Register the solver in the physics factory namespace.
3. Update the `SimulationSettingsViewModel` in `CelestialMechanics.Simulation` to include the new algorithm in the UI selection options.

## 2. Modifying UI Components
- All WPF Views are located in `src/CelestialMechanics.Desktop/Views`.
- ViewModels are in `src/CelestialMechanics.Simulation/ViewModels`.
- Properties and bindings must follow the standard MVVM rules using CommunityToolkit MVVM generators.
