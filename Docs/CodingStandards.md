# Celestial Mechanics - Coding Standards

This document establishes the C# and design patterns expected throughout the Celestial Mechanics solution.

## 1. Language & Platform Guidelines

* **Target SDK**: .NET 8.0 / C# 12
* **Nullable Context**: Enabled (`<Nullable>enable</Nullable>`)
* **Implicit Usings**: Enabled where appropriate, but prefer explicit declarations in core mathematical assemblies to ensure clarity.
* **Warning Level**: Treat warnings as errors in release pipelines to maintain code hygiene.

## 2. Naming Conventions

* **Namespaces**: Match folder structures exactly (e.g., `namespace CelestialMechanics.Math.Vector`).
* **Classes & Structs**: `PascalCase`
* **Interfaces**: Prefix with `I` (e.g., `IIntegrator`)
* **Methods**: `PascalCase`
* **Properties**: `PascalCase`
* **Fields**:
  - Private/Protected: CamelCase with prefix underscore (e.g., `_particleCount`).
  - Public/Internal: PascalCase (or wrap in properties).
* **Local Variables**: `camelCase`

## 3. Architecture & Class Design

### Dependency Injection
- Always register services via `Microsoft.Extensions.DependencyInjection` where applicable.
- Constructor injection is the preferred method. Avoid static locator patterns.

### Mutability
- Prefer immutable data models for snapshot representations, orbital parameter structures, and data transfer states.
- High-frequency simulation matrices or coordinates can use mutable buffers to prevent heavy Garbage Collector allocations.

### Exception Handling
- Avoid swallowing exceptions.
- Provide custom Exception classes in each domain (e.g., `SimulationException`, `PhysicsCalculationException`).
- Catch specific exceptions rather than `System.Exception`.
