param(
    [string]$TargetPath = "$PWD\publish\CelestialMechanics.Desktop.exe"
)

Write-Host "Scanning for processes locking: $TargetPath..." -ForegroundColor Cyan

$processes = Get-CimInstance Win32_Process | Where-Object { $_.ExecutablePath -eq $TargetPath }

if ($processes) {
    Write-Host "Lock detected! The following process(es) are locking the file:" -ForegroundColor Red
    $processes | Select-Object ProcessId, Name, ExecutablePath | Format-Table
} else {
    Write-Host "No lock detected for the specified path." -ForegroundColor Green
    
    # Check if the process matches by name, but originated from another directory
    $fileName = Split-Path $TargetPath -Leaf
    $otherProcesses = Get-CimInstance Win32_Process | Where-Object { $_.Name -eq $fileName }
    
    if ($otherProcesses) {
        Write-Host "Warning: Instances of $fileName are still running, but from other paths:" -ForegroundColor Yellow
        $otherProcesses | Select-Object ProcessId, ExecutablePath | Format-Table
    }
}
