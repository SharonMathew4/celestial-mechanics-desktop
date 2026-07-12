# Observation Camera Controls

This document details the camera system requirements for Observation Mode.

## 1. Camera Modes

- **Geocentric Gaze**: The camera is bound to Earth, tracking stars and planets across the sky dome based on current latitude and longitude.
- **Heliocentric Orbit**: Camera orbits stars/planets at astronomical scales.
- **Free Interstellar Flight**: Continuous navigation with velocity adjustments scaled logarithmically.

## 2. Mathematical Scaling
- Implementing a **Logarithmic Depth Buffer** in rendering shaders to prevent near/far clipping issues when transiting from small spacecraft dimensions to light-year intervals.
