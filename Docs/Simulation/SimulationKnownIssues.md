# Simulation Known Issues

This document lists bugs and known limitations specific to the Simulation Mode sandbox.

## 1. Physics Drift in High-Speed Orbits
- **Problem**: In close binaries or highly eccentric orbits, bodies can fly apart when using the Euler integrator.
- **Mitigation**: Instruct users to switch to the Verlet or RK4 integrator, or decrease the simulation step size using the viewport controls.

## 2. Trail Buffer Overflow
- **Problem**: Long-running simulations with massive step sizes cause orbital trails to stutter or disappear.
- **Mitigation**: Adjust the trail ring buffer size dynamically based on body velocities.
