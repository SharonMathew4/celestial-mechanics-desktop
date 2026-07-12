# Observation Selection System

This document outlines the ray-cast picking and selection logic for Observation Mode.

## 1. Ray Cast Ray Picking

- Clicking in the viewport projects a ray from the camera lens into world coordinate space.
- Calculate bounding cylinder intersections for stars using a minimum distance threshold to allow picking stars that are pixel-sized.

## 2. Selection Sync
- Sync selected entity context with target telemetry panels and UI overlays.
