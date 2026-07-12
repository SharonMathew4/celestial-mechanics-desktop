# Future Development Plans

This document outlines long-term development proposals for Celestial Mechanics that are beyond the immediate scope of standard milestones.

## 1. General Relativity (Relativistic Solver)
- **Concept**: Add a post-Newtonian correction toggle to the physics integrators.
- **Application**: Allows simulation of Mercury's orbital precession and light deflection around massive black holes.
- **Impact**: Higher math/physics processing requirements. Will require native CUDA solvers.

## 2. Head-Mounted VR Support
- **Concept**: Add OpenXR bindings to the Rendering layer.
- **Application**: Allows immersive space tours, standing on the surface of Mars, or scaling planets relative to the solar system in virtual space.

## 3. Collaborative Universe Sharing
- **Concept**: Server-side user scenario library.
- **Application**: Save a sandbox state online and share a short URL code for others to open the simulation layout.
