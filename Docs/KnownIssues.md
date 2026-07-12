# Known Issues & Workarounds

This document tracks known issues, bugs, and technical limitations within the Celestial Mechanics project.

## 1. Active Diagnostics & Workarounds

### 1.1 Silk.NET Thread Lock in WPF (stuttering frames)
- **Problem**: Periodic rendering lag occurs when moving WPF panels or resizing the main window while the simulation is running.
- **Root Cause**: The OpenGL context is run within a custom WPF hosting wrapper. Main UI thread resize calculations conflict with Silk.NET render cycles.
- **Workaround**: Pause simulation before doing extensive panel reorganization.

### 1.2 NuGet Vulnerability Warning NU1900
- **Problem**: Building via CLI throws warnings that NuGet package vulnerability lookup failed.
- **Root Cause**: Build systems operating in offline or restricted-network environments cannot query the remote NuGet database.
- **Workaround**: Ignored during local offline compilation. Can be bypassed using standard MSBuild flags if needed (`/p:ReportAnalyzer=None`).

## 2. Math & Physics Edge Cases

### 2.1 Infinite Forces on Collisions
- **Problem**: If two bodies come extremely close (distance approaches zero), gravitational force values approach infinity.
- **Solution**: The physics solver uses a softening factor ($\epsilon = 0.01$) to prevent division by zero errors. Ensure this factor is tuned based on body diameter.
