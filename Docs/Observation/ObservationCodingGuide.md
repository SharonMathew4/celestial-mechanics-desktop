# Observation Mode Coding Guide

This document provides developer guidelines for writing code in the Observation Mode modules.

## 1. Threading Rules

- Never perform file system reads or catalog database queries on the OpenGL thread. Use the `Task` asynchronous pattern.
- Coordinate updates from database results should be scheduled on the Main Dispatcher thread for rendering sync.

## 2. Float Precision Protection
- Always use high-precision variables (`double`) for planetary position offsets relative to the Solar System barycenter before converting to camera-relative single-precision vectors (`float`) for GPU pipelines.
