param(
    [switch]$UseTimestampFolder
)

$targetProj = "src\CelestialMechanics.Desktop\CelestialMechanics.Desktop.csproj"
$publishDir = "publish"

if ($UseTimestampFolder) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $publishDir = "publish_$timestamp"
    Write-Host "Publishing to timestamped folder: $publishDir" -ForegroundColor Cyan
}

$exePath = "$PWD\$publishDir\CelestialMechanics.Desktop.exe"

try {
    # Attempt to publish. 
    # Note: CelestialMechanics.Desktop.csproj now contains a pre-publish target that will automatically attempt to kill the running executable.
    Write-Host "Running dotnet publish..." -ForegroundColor Cyan
    $process = Start-Process -FilePath "dotnet" -ArgumentList "publish", "$targetProj", "-c", "Release", "-o", "$publishDir" -Wait -NoNewWindow -PassThru
    
    if ($process.ExitCode -ne 0) {
        Write-Host "Publish failed." -ForegroundColor Red
        if (-Not $UseTimestampFolder) {
            Write-Host "Lock may be persistent. Retrying publish in a timestamped fallback folder..." -ForegroundColor Yellow
            & $PSCommandPath -UseTimestampFolder
        }
        exit $process.ExitCode
    } else {
        Write-Host "Publish succeeded to $publishDir" -ForegroundColor Green
    }
}
catch {
    Write-Host "Error executing dotnet publish." -ForegroundColor Red
}
