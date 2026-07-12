# Observation Mode Architecture

This document outlines the architectural plan for the future Observation Mode.

## 1. System Overview

Observation Mode will render realistic real-world astronomical data. It must run independently of Simulation Mode, utilizing shared core engine libraries only.

## 2. Design Guidance

- **Data-Driven Architecture**: The system must load astronomical coordinates, stellar classifications, and velocities dynamically from catalog files instead of maintaining editable local state files.
- **Service Isolation**: Databases and APIs (NASA JPL, Gaia databases) should be accessed via dedicated proxy/repository classes that do not couple with the presentation layers.
- **Scene Hierarchy**: Utilize an octree-based spatial partitioning model to handle astronomical scales without precision issues.
