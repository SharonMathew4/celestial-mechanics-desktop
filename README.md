# Celestial Mechanics Desktop 🌌

Welcome to **Celestial Mechanics Desktop**, a high-performance 3D gravitational physics simulation engine and interactive desktop application. 

This project allows users to simulate, visualize, and interact with complex orbital mechanics, N-body physics, relativistic effects, and real-time accretion disk formations natively on Windows.

## 🚀 Features

- **N-Body Gravity Simulation:** High-performance, deterministic simulation capable of handling thousands of interacting bodies.
- **Hardware Acceleration:** Pluggable physics backends, including SIMD Single-Threaded, Multi-Threaded, and Native GPU/CUDA backends for massive scale simulations.
- **Interactive 3D Viewport:** Advanced camera controls (pan/orbit/zoom), interactive body placement, and real-time gravitational lens rendering.
- **Relativistic Physics:** Barnes-Hut O(n log n) tree implementations and Post-Newtonian corrections for high-accuracy simulations.
- **Extensible Architecture:** Designed with maintainability in mind using Modern C# / .NET 7+. 

---

## 🛠️ Tech Stack & Prerequisites

Before you can build and run this application locally, ensure you have the following installed:

- **[.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)** (or later, if configured in `global.json`)
- **[Git](https://git-scm.com/downloads)**
- **[Visual Studio 2022](https://visualstudio.microsoft.com/vs/)** (Recommended) with the **".NET desktop development"** workload.
- **C++ Build Tools / CUDA Toolkit** (Optional, required only if you are building the native hardware-accelerated backend locally).

---

## 💻 Local Setup strictly for Collaborators

Follow these instructions to clone, build, and run the project strictly via the command line or Visual Studio.

### 1. Clone the Repository

```powershell
git clone https://github.com/<username>/celestial-mechanics-desktop.git
cd celestial-mechanics-desktop
```

### 2. Restore Dependencies

Use the .NET CLI to restore all required NuGet packages for the solution:

```powershell
dotnet restore
```

*(Note: Ensure you have network access to standard NuGet package sources. If any C++ native interop issues arise, ensure you have your MSVC tools correctly installed.)*

### 3. Build the Solution

Build the entire solution in Debug mode (default):

```powershell
dotnet build
```

*For maximum performance during testing, you can build in Release mode:*
```powershell
dotnet build -c Release
```

### 4. Run the Application

The main entry point is the WPF Desktop Application (`CelestialMechanics.App`). You can launch it using:

```powershell
dotnet run --project src/CelestialMechanics.App/CelestialMechanics.App.csproj
```

Alternatively, you can open `CelestialMechanics.sln` in **Visual Studio 2022**, set `CelestialMechanics.App` as the **Startup Project**, and press `F5` to debug.

---

## 🧪 Running Tests

We strictly enforce deterministic tests, math accuracy, and physical correctness across our solvers. To run the full suite of unit and integration tests:

```powershell
dotnet test
```

Please ensure all tests pass (especially `DeterminismTests` and `SIMDParityTests`) before submitting a Pull Request!

---

## 🤝 Contribution Guidelines

- **Branching:** Please branch off `main` for any new feature or bugfix (e.g., `feature/barnes-hut-optimization` or `fix/camera-viewport`).
- **Code Style:** We follow standard Microsoft C# naming conventions. Keep your commits atomic.
- **Check-ins:** Ensure you do not accidentally commit generated folders such as `bin/`, `obj/`, `.vs/`, or huge test dump files.
- **Pull Requests:** All PRs will undergo a code review and automated checks for determinism regression.

## 📄 License

*(Insert your chosen license here, e.g., MIT, GPL-3.0, or Proprietary if closed-source.)*

---
*If you run into issues such as `System.DllNotFoundException` for `OpenTK` or native interop, ensure you have the latest Visual C++ Redistributable installed.*
