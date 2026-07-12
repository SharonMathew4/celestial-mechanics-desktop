# Observation Catalog System

This document outlines the stellar and planetary catalogs integrated into Observation Mode.

## 1. Supported Catalogs

The application will read and index major astronomical catalogs:
- **Hipparcos (HIP)**: Main dataset for nearby high-brightness stars (~118,000 entries).
- **Gaia DR3 / DR4**: Massive database for deeper explorations (billions of stars; will require specialized tile querying).
- **Yale Bright Star Catalog (BSC)**: Used to quickly render the night sky visible to the naked eye.
- **Messier Catalog**: Deep sky nebulae and star clusters.

## 2. Ingestion Format
- Convert raw catalog formats into a standardized internal SQLite or binary layout.
