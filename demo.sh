#!/usr/bin/env bash
# Demo helper script for Unix-like systems
if [ -z "$COGNODB_URI" ] || [ -z "$COGNODB_USER" ] || [ -z "$COGNODB_PASSWORD" ]; then
  echo "Please set COGNODB_URI, COGNODB_USER and COGNODB_PASSWORD"
  echo "Example: COGNODB_URI=bolt+s://... COGNODB_USER=cognodb COGNODB_PASSWORD=... ./demo.sh"
  exit 1
fi

dotnet run &
sleep 2
xdg-open http://localhost:5000 || open http://localhost:5000 || echo "Open http://localhost:5000 in your browser"

echo "Demo launched. Use the console to start the web server with command 'web' if it hasn't started."