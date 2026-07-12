# Observation Mode Roadmap

This document outlines the milestones for Observation Mode implementation.

## Milestone 1: Library & Coordinates
- [ ] Create `CelestialMechanics.Observation` project.
- [ ] Implement right ascension/declination to XYZ coordinate transforms.
- [ ] Parse Hipparcos catalog binary files.

## Milestone 2: Telescope Viewport
- [ ] Set up geocentric camera tracking.
- [ ] Render 100,000+ stars as OpenGL point sprites.
- [ ] Implement ray-cast star picker.

## Milestone 3: Real-Time Integrations
- [ ] Connect JPL Horizons client.
- [ ] Implement proper motion calculations for timeline scrubbers.
