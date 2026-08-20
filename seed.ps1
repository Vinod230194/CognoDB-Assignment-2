# PowerShell seed helper — fill the password before running
param(
    [string]$uri = $env:COGNODB_URI,
    [string]$user = $env:COGNODB_USER,
    [string]$password = $env:COGNODB_PASSWORD
)

if (-not $uri -or -not $user -or -not $password) {
    Write-Error "Please set COGNODB_URI, COGNODB_USER, and COGNODB_PASSWORD environment variables or pass them as parameters. Example: .\seed.ps1 -password 'mypw'"
    return
}

$env:COGNODB_URI = $uri
$env:COGNODB_USER = $user
$env:COGNODB_PASSWORD = $password

Write-Host "Running seeder against $uri as $user"

dotnet run
