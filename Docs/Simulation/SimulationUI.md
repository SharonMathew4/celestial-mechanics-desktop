# Simulation UI Layout

This document describes the design and components of the Simulation Mode user interface.

## 1. Interface Overview

The interface is structured to function as an IDE for sandbox creation:
1. **Scene Outliner**: Tree view tracking all bodies in the current scene graph.
2. **Properties Panel**: Edit physical characteristics of the selected body (mass, radius, velocity, position).
3. **Control Bar**: Play, pause, step, and simulation speed sliders.
4. **Interactive Viewport**: The main OpenGL screen allowing click selection and camera pan/orbit.
5. **Telemetry Panels**: Live charts showing system total energy drift, body velocities, and orbital distance changes.

## 2. WPF Controls & Styles
- Custom styles are defined in `MainWindow.xaml` and resource dictionaries.
- UI elements use standard WPF layout grids, menus, and sidebars.
