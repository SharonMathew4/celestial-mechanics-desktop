# Simulation Rendering Pipeline

This document describes the rendering structures and graphics techniques used to display Simulation Mode.

## 1. OpenGL Pipeline

The rendering layer is implemented using **Silk.NET.OpenGL** inside a custom control wrapper.

### Key Render Stages:
1. **Background Pass**: Draws the deep space/nebula background skybox quad using a dedicated fragment shader.
2. **Orbit Trail Pass**: Draws historical position buffers using line-strip shaders. Trails are updated dynamically from ring buffers to limit memory allocations.
3. **Planet Mesh Pass**: Renders 3D sphere meshes for celestial bodies, applying diffuse textures and lighting.
4. **Overlay Pass**: Draws UI handles, velocity vectors, selection wireframes, and orbits using simple lines.
5. **ImGui Pass**: Renders overlay performance stats and dashboard HUDs.

## 2. Shaders
- **Shaders/skybox.vert / skybox.frag**: Renders background deep space fields.
- **Shaders/planet.vert / planet.frag**: Main sphere mesh lighting and texturing.
- **Shaders/trail.vert / trail.frag**: Dynamic alpha fading for orbital trail lines.
