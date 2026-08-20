#!/usr/bin/env bash
# Simple seed helper for Unix-like shells
if [ -z "$COGNODB_URI" ] || [ -z "$COGNODB_USER" ] || [ -z "$COGNODB_PASSWORD" ]; then
  echo "Please set COGNODB_URI, COGNODB_USER, and COGNODB_PASSWORD"
  echo "Example: COGNODB_URI=bolt+s://db-af2a5606.bravo.databases.cognodb.com COGNODB_USER=cognodb COGNODB_PASSWORD=xxx dotnet run"
  exit 1
fi

echo "Running seeder against $COGNODB_URI as $COGNODB_USER"

dotnet run
