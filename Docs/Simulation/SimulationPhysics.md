# Simulation Physics Engine

This document details the equations, integrators, and settings used to calculate gravitational physics in Simulation Mode.

## 1. Physics Model

Simulation Mode resolves classical Newtonian gravitational forces for $N$ bodies. The gravitational force $\vec{F}_{ij}$ acting on body $i$ due to body $j$ is defined by:

$$\vec{F}_{ij} = -G \frac{m_i m_j}{|\vec{r}_{ij}|^2 + \epsilon^2} \hat{r}_{ij}$$

Where:
- $G$ is the gravitational constant.
- $m_i, m_j$ are the masses of the bodies.
- $\vec{r}_{ij}$ is the displacement vector from body $i$ to body $j$.
- $\epsilon$ is a softening factor to prevent infinite forces during close encounters.

## 2. Supported Integrators

The physics engine supports multiple integration algorithms configured at runtime:
1. **Euler**: First-order integrator. Computationally cheap but suffers from energy drift.
2. **Verlet**: Second-order symplectic integrator. Excellent energy conservation characteristics, ideal for orbital dynamics.
3. **Runge-Kutta 4th Order (RK4)**: Fourth-order integrator. Highly accurate but requires four force evaluations per step.
