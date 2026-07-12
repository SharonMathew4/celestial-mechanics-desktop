# Project Dependencies

This document details the libraries, packages, and system requirements for Celestial Mechanics.

## 1. Managed C# NuGet Dependencies

The managed runtime uses modern .NET libraries locked to deterministic version trees.

| Package Name | Pinned Version | Scope | Purpose |
|---|---|---|---|
| **Silk.NET.OpenGL** | `2.23.0` | Desktop, Renderer, App | Low-level OpenGL bindings |
| **Silk.NET.Windowing** | `2.23.0` | App | OS window management |
| **Silk.NET.Input** | `2.23.0` | App | Keyboard and mouse input |
| **Silk.NET.Maths** | `2.23.0` | App | Native Silk vectors and matrix utilities |
| **ImGui.NET** | `1.91.6.1` | App | In-application UI overlays |
| **CommunityToolkit.Mvvm** | `8.4.0` | Desktop | WPF MVVM commands, messengers, and viewmodels |
| **LiveChartsCore.SkiaSharpView.WPF** | `2.0.0-rc3.3` | Desktop | Real-time orbital metrics telemetry charts |
| **Microsoft.Extensions.DependencyInjection** | `8.0.0` | Desktop | IoC Container setup |

## 2. Native Engine Dependencies

The C++ engine uses modern C++ build infrastructure.

* **Compiler**: C++20 compliant compiler (MSVC 2022 / Clang / GCC 12+)
* **CMake**: Version 3.20 or higher
* **CUDA Toolkit**: Version 12.0+ (Optional, for GPU-accelerated solvers)
