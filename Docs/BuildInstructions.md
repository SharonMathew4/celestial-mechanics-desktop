# Build Instructions

This document explains how to restore, build, and test the Celestial Mechanics projects.

## 1. Managed C# Application

### Prerequisites
- .NET 8.0 SDK installed.

### Standard Build Commands

To restore and build the solution:
```powershell
dotnet restore CelestialMechanics.sln
dotnet build CelestialMechanics.sln -c Debug
```

To run unit tests:
```powershell
dotnet test CelestialMechanics.sln -c Debug --no-build
```

### Running the Launchers

- To run the WPF desktop IDE launcher:
  ```powershell
  dotnet run --project src/CelestialMechanics.Desktop/CelestialMechanics.Desktop.csproj
  ```

- To run the direct Silk.NET interactive window launcher:
  ```powershell
  dotnet run --project src/CelestialMechanics.App/CelestialMechanics.App.csproj
  ```

## 2. Native C++ Engine Build

### Prerequisites
- CMake 3.20+
- Visual Studio 2022 / MSVC
- CUDA Toolkit 12.0+ (Optional)

### Compilation Steps
1. Navigate to the `engine` directory.
2. Generate CMake build files:
   ```bash
   cmake -B build -S .
   ```
3. Compile the binaries:
   ```bash
   cmake --build build --config Release
   ```
