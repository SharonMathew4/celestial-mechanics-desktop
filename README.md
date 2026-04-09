# Celestial Mechanics Desktop

## Detailed Execution Problem Report

A very detailed report has been prepared here:

- `EXECUTION_PROBLEM_REPORT.md`

The report includes:

1. Full root-cause analysis of the publish failure.
2. Evidence from `publish_log.txt`, `publish_log2.txt`, and build script behavior.
3. Reproduction steps for both failure and success scenarios.
4. Process-level diagnostics and lock troubleshooting commands.
5. Immediate mitigation, long-term hardening, and team workflow recommendations.

## Problem Summary (Short Version)

The failing symptom is a publish-time lock conflict:

1. `dotnet publish` reaches `GenerateBundle`.
2. Destination file `publish\CelestialMechanics.Desktop.exe` is still in use by a running process.
3. MSBuild surfaces error `MSB4018` with an inner `System.IO.IOException` for file-in-use.

This is not a source compilation issue; it is an execution-state issue.

## Troubleshooting MSB4018 Lock Issues

If you encounter `MSB4018` or `System.IO.IOException` during `dotnet publish`, it is very likely an execution lock because the app is already running.

| Symptom / Error | Common Cause | Action / Resolution |
| :--- | :--- | :--- |
| **`MSB4018`** on `GenerateBundle` | `publish\CelestialMechanics.Desktop.exe` is currently running | **Close the app**, then publish again. |
| Publish fails silently  | The app window is closed, but the process hangs | Run `.\detect_lock.ps1` to find ghost processes. |
| Cannot close the app | The simulation thread is stuck in an infinite loop | Run `Stop-Process -Name CelestialMechanics.Desktop -Force`. |
| Need a rapid local dev loop | Manual closing takes time | Use `.\publish.ps1 -UseTimestampFolder` to publish to a fresh output folder (e.g. `publish_20260409_165030`). |

**Note**: The `.csproj` has been updated to automatically terminate existing `CelestialMechanics.Desktop.exe` instances during publish natively. If you use the standard VS Code Publish tasks or `.\publish.ps1`, the lock handling and termination should be completely automatic!
