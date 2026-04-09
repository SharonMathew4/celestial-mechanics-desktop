# Celestial Mechanics Desktop - Detailed Execution Problem Report

## 1. Document Purpose

This report explains, in technical and operational detail, why publish operations can fail in this repository with an executable lock error, how to reproduce and diagnose the issue, and how to prevent it in local development and release workflows.

This is a documentation report only. No runtime behavior was modified while preparing this report.

---

## 2. Problem Statement

`dotnet publish` can fail with an `MSB4018` error during publish when the target executable in the publish output folder is currently running and locked by Windows.

The observed lock target is:

- `publish\CelestialMechanics.Desktop.exe`

The observed failing task is:

- `GenerateBundle`

This is an execution/process-state issue, not a compile-time code failure.

---

## 3. Evidence Inventory

### 3.1 Failed publish evidence

From `publish_log.txt`:

1. Restore succeeds.
2. Projects build successfully.
3. Publish fails at `Microsoft.NET.Publish.targets` with `MSB4018`.
4. Root exception is `System.IO.IOException` for an in-use destination file.
5. The locked destination path is `publish\CelestialMechanics.Desktop.exe`.

Representative error text:

```text
The process cannot access the file '...\publish\CelestialMechanics.Desktop.exe'
because it is being used by another process.
```

### 3.2 Successful publish evidence

From `publish_log2.txt`:

1. Restore succeeds.
2. Build succeeds.
3. Publish succeeds when output is redirected to `publish_new\`.

Interpretation: publish itself is healthy; failure is tied to destination file lock in the active output folder.

### 3.3 Build script behavior

From `build.bat`:

1. The script contains a pre-check to detect lock conditions by attempting deletion of `publish\CelestialMechanics.Desktop.exe`.
2. If deletion fails, the script prints an explicit lock warning and exits before publish.

This confirms the lock scenario is already recognized as an operational hazard.

### 3.4 Project configuration context

From `src/CelestialMechanics.Desktop/CelestialMechanics.Desktop.csproj`:

1. The project is a WPF desktop app (`OutputType=WinExe`, `TargetFramework=net8.0-windows`).
2. The file explicitly notes publish-specific flags are passed via publish command, not hardcoded in the csproj.

From `.github/workflows/release.yml`:

1. CI publish uses runtime-specific output and single-file/self-contained settings.
2. This context is compatible with `GenerateBundle` involvement in publish.

---

## 4. Root Cause Analysis

## 4.1 Immediate root cause

Windows enforces exclusive access semantics on a running executable image. If the destination `.exe` is active, publish cannot overwrite that file.

## 4.2 Why the error appears as `MSB4018`

`MSB4018` is an MSBuild task failure wrapper. In this case, the wrapped failure is I/O-level access denial due to file lock.

## 4.3 Why `GenerateBundle` appears in stack trace

The publish pipeline enters bundling/binary assembly logic where file copy/replace happens. The destination replacement fails because the destination executable is in use.

## 4.4 What this is not

This issue is not caused by:

1. C# compilation errors.
2. NuGet restore errors.
3. Missing references.
4. Unit test failures.
5. Invalid source code in the built projects.

---

## 5. Failure Surface and Impact

### 5.1 Typical trigger paths

1. Developer launches `publish\CelestialMechanics.Desktop.exe` directly.
2. Developer launches the same executable via desktop shortcut.
3. App window appears closed but process persists, or closes after delay.
4. Another publish targets the same `publish\` folder.

### 5.2 Impact

1. Publish job fails late (after successful build), wasting build time.
2. Team members may misclassify it as a code or SDK failure.
3. Local development loop slows down.
4. Repeated retries without killing process create noisy logs and confusion.

---

## 6. Reproduction Procedure

## 6.1 Reproduce failure

1. Publish to `publish\` and launch `publish\CelestialMechanics.Desktop.exe`.
2. Keep the app process running.
3. Run publish again to the same output folder.
4. Observe `MSB4018` and file-in-use exception.

## 6.2 Reproduce success path

1. Keep the same app running from `publish\`.
2. Publish to a different folder, for example `publish_new\`.
3. Observe successful publish.

---

## 7. Diagnostic Runbook

Use the following before publish if lock suspicion exists.

### 7.1 Check process presence

```powershell
Get-Process CelestialMechanics.Desktop -ErrorAction SilentlyContinue
```

### 7.2 Check by executable path (more precise)

```powershell
Get-CimInstance Win32_Process |
  Where-Object { $_.ExecutablePath -eq "$PWD\publish\CelestialMechanics.Desktop.exe" } |
  Select-Object ProcessId, Name, ExecutablePath
```

### 7.3 Stop process safely

```powershell
Get-Process CelestialMechanics.Desktop -ErrorAction SilentlyContinue | Stop-Process -Force
```

### 7.4 Retry publish

```powershell
dotnet publish src\CelestialMechanics.Desktop\CelestialMechanics.Desktop.csproj -c Release -o publish
```

---

## 8. Existing Mitigation in Repository

`build.bat` already applies a practical guard:

1. If `publish\CelestialMechanics.Desktop.exe` exists, attempt forced delete.
2. If file still exists, abort with clear message about a running process.
3. Prevents ambiguous late-stage MSBuild failure.

This is a good baseline for local reliability.

---

## 9. Recommended Operational Policy

## 9.1 Folder separation policy

Use separate folders for run-target vs publish-target:

1. `publish\` as stable runtime folder.
2. `publish_new\` or versioned folders for active build output.

Benefits:

1. Prevents lock collision between running app and fresh publish.
2. Keeps runtime binaries stable while testing new publish results.
3. Enables safe shortcut switching only after successful publish.

## 9.2 Shortcut policy

For Windows shortcuts:

1. Ensure `Target` points to intended output folder.
2. Ensure `Start in` is the same output folder.
3. Never publish over a folder currently used by a running shortcut instance.

## 9.3 Team workflow policy

Before publish to an existing output folder:

1. Confirm app process is closed.
2. Run lock-aware script (`build.bat`) instead of raw `dotnet publish` when possible.
3. If failure repeats, switch to a clean output folder immediately.

---

## 10. Optional Hardening Opportunities

These are optional and not required for immediate resolution:

1. Add a dedicated PowerShell helper script that detects path-specific process lock and prints PID + path.
2. Add a timestamped output convention (for example `publish_yyyyMMdd_HHmmss`) in local dev mode.
3. Add README troubleshooting table with lock symptom -> action mapping.
4. Add pre-publish process detection into additional scripts/IDE tasks beyond `build.bat`.

---

## 11. Final Conclusion

The problem is a deterministic file lock conflict between a running desktop executable and publish overwrite behavior in the same output folder.

Evidence is consistent across logs:

1. Publish fails only when destination executable is in use.
2. Publish succeeds when output folder changes or lock is removed.

Operationally, the issue is fully manageable through process hygiene, folder separation, and the existing lock pre-check in `build.bat`.
