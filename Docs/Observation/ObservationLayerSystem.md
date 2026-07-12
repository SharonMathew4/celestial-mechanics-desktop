# Observation Layer System

This document outlines the visual layer systems planned for Observation Mode.

## 1. Layers

To maintain render performance, visual modules are separated into configurable layers:
1. **Background Layer**: Skybox and interstellar dust.
2. **Stars Layer**: Stellar point sprites.
3. **Planets Layer**: Detailed 3D planetary surfaces.
4. **Overlay Layer**: Constellations, coordinates, gridlines.
5. **HUD Layer**: Target markers and user text details.

## 2. Control Layout
- Provide a check-list UI allowing users to toggle each layer independently.
