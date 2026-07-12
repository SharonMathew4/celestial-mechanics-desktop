# Observation Time Controller

This document outlines the time scrubber calculations for Observation Mode.

## 1. Astronomical Time Standards

- System updates coordinates based on **Julian Date (JD)** or **Barycentric Dynamical Time (TDB)**.
- Scrubber scales from real-time seconds up to thousands of years per step.

## 2. Proper Motion Updates
- When scrubbing through millennia, apply proper motion coordinates ($\mu_{\alpha}, \mu_{\delta}$) from star catalogs to offset star position coordinates dynamically.
