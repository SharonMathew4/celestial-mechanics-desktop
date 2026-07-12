# Observation Data Pipeline

This document explains the ingestion pipelines for raw astronomical datasets.

## 1. Offline Processing Pipeline

1. **Extraction**: Download catalog sources from NASA/JPL/Gaia repositories.
2. **Filtering**: Parse out irrelevant fields to reduce catalog sizes.
3. **Verification**: Verify coordinate conversions against standard ephemeris models.
4. **Export**: Export to compressed binary chunks indexed by galactic coordinates.

## 2. Real-time Lookup APIs
- Establish background HTTP clients connecting to JPL Horizons for current planetary coordinates.
