# Observation Navigation System

This document outlines the navigation algorithms and coordinate conversions required for Observation Mode.

## 1. Coordinate Systems

Navigation and camera targeting must support real-time conversions between:
- **Equatorial Coordinate System**: Right Ascension (RA) and Declination (Dec).
- **Ecliptic Coordinate System**: Ecliptic longitude and latitude.
- **Horizontal Coordinate System**: Altitude and Azimuth.

## 2. Interactive Navigation Targets
- Double-clicking objects triggers smooth camera transitions (using slerp and log scale curves) to prevent sudden camera jumps.
