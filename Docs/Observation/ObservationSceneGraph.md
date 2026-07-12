# Observation Scene Graph

This document details the spatial management layout for astronomical data.

## 1. Scale Hierarchy

Due to floating-point precision limitations (single-precision vs double-precision on GPU), the scene graph must use a multi-tiered scale hierarchy:
1. **Local System Coordinate Space** (AU units)
2. **Interstellar Space** (Light Year units)
3. **Galactic / Deep Space** (Parsec units)

## 2. Spatial Partitioning
- Use dynamic octree structures to store, search, and cull stars in real-time.
