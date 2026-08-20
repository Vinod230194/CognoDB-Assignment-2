# Demo helper script — runs seed, opens browser to web UI (PowerShell)
param(
    [string]$password
)

if (-not $env:COGNODB_URI) { Write-Error "Set COGNODB_URI env var first"; exit 1 }
if (-not $env:COGNODB_USER) { Write-Error "Set COGNODB_USER env var first"; exit 1 }
if (-not $password -and -not $env:COGNODB_PASSWORD) { Write-Error "Provide password parameter or set COGNODB_PASSWORD"; exit 1 }

if ($password) { $env:COGNODB_PASSWORD = $password }

# Run the app which seeds and starts interactive mode
Start-Process -NoNewWindow -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory .

Start-Sleep -Seconds 2
Start-Process "http://localhost:5000"

Write-Host "Demo launched. Use the console to start the web server with command 'web' if it hasn't started."