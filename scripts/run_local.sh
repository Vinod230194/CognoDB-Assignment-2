#!/usr/bin/env bash
# Helper to run the app from repo root using dotnet run on Linux/macOS
export ASPNETCORE_URLS="http://0.0.0.0:5000"
export COGNODB_URI="${COGNODB_URI:-bolt+s://db-af2a5606.bravo.databases.cognodb.com}"
export COGNODB_USER="${COGNODB_USER:-cognodb}"
export COGNODB_PASSWORD="${COGNODB_PASSWORD:-<replace-with-real-password>}"

dotnet run --project ./Neo4jDemo.csproj
