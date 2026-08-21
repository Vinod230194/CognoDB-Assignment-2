# PowerShell helper to run the app locally
$env:ASPNETCORE_URLS = 'http://0.0.0.0:5000'
if (-not $env:COGNODB_URI) { $env:COGNODB_URI = 'bolt+s://db-af2a5606.bravo.databases.cognodb.com' }
if (-not $env:COGNODB_USER) { $env:COGNODB_USER = 'cognodb' }
if (-not $env:COGNODB_PASSWORD) { Write-Host 'Please set COGNODB_PASSWORD environment variable to your password'; exit 1 }

dotnet run --project ./Neo4jDemo.csproj
