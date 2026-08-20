# CognoDB Demo — Graph App

[![CI](.github/workflows/ci.yml/badge.svg)](https://github.com/your/repo/actions)

This repository contains a small .NET 9 demo app that connects to a CognoDB (Neo4j-compatible) instance, seeds a sample dataset, and provides a minimal web UI.

Goals (matching the assessment)
- Thoughtful graph data model with labeled nodes, typed relationships and properties
- Seed script to load realistic sample data
- Cypher queries that exercise the graph including multi-hop traversal
- Parameterized queries via the official Neo4j .NET driver
- Clear instructions and environment-based configuration (no secrets committed)

Data model (text diagram)

- Node label: `Person` { name, age }
- Relationship: `(:Person)-[:KNOWS { since }]->(:Person)`

Example (ASCII):

  (Alice:Person {name: "Alice", age:30})
            |
       KNOWS{since:2020}
            |
  (Bob:Person {name: "Bob", age:28})
            |
       KNOWS{since:2018}
            |
  (Carol:Person {name: "Carol", age:35})

Why a graph database?
- Graphs naturally model relationships; queries like "friends-of-friends", multi-hop traversals, shortest paths and neighborhood analysis are expressive and efficient in a graph model compared to relational joins.

Setup & run
1. Set connection details as environment variables (do NOT commit real secrets):
   - `COGNODB_URI` (e.g. `bolt+s://db-af2a5606.bravo.databases.cognodb.com`)
   - `COGNODB_USER` (e.g. `cognodb`)
   - `COGNODB_PASSWORD`

2. Local development (PowerShell example):
   - `$env:COGNODB_URI = 'bolt+s://db-af2a5606.bravo.databases.cognodb.com'`
   - `$env:COGNODB_USER = 'cognodb'`
   - `$env:COGNODB_PASSWORD = '<your-password>'`
   - `dotnet run`

The app will:
- Verify connectivity
- Seed sample data (see `Seeder.cs`)
- Run example queries and print results

Files added
- `Seeder.cs` — seeds `Person` nodes and `KNOWS` relationships using parameterized Cypher
- `queries.cypher` — collection of queries used and additional examples
- `seed.ps1` / `seed.sh` — helper scripts to set env vars and run the seeder locally
- `README.md` — this file
- `.env.example` — example env file to use locally (not containing real secrets)

Demo

- Use `demo.ps1` (Windows) or `demo.sh` (Unix) to launch the app and open the web UI. Make sure environment variables are set:
  - `COGNODB_URI`, `COGNODB_USER`, `COGNODB_PASSWORD`

Deploying to Render (recommended quick path)

1. Create a free account at https://dashboard.render.com and connect your GitHub repository.
2. Create a new Web Service, or add `render.yaml` to the repo and use "Create a new service from repo".
   - Build Command: `dotnet publish -c Release -o out`
   - Start Command: `dotnet out/Neo4jDemo.dll`
3. Set environment variables in the Render service settings:
   - `COGNODB_URI` (e.g. `bolt+s://db-af2a5606.bravo.databases.cognodb.com`)
   - `COGNODB_USER` = `cognodb`
   - `COGNODB_PASSWORD` = `<your-password>`
4. (Optional) To automate deployments via GitHub Actions:
   - Add the repository secrets: `RENDER_API_KEY` and `RENDER_SERVICE_ID` in your GitHub repo settings -> Secrets.
   - Push to `main`; the workflow `.github/workflows/deploy-render.yml` will publish and call the Render deploy API.

Security
- Do NOT commit passwords. Use Render's environment variables or GitHub secrets.

Submission checklist

- Ensure secrets are not committed. Remove password from `Properties/launchSettings.json` before pushing.
- Push to a GitHub repository and email the repo URL and demo link to the assessors.

Next steps I can do for you (select any):
- Add a minimal web UI (single-page app + minimal API) to explore the graph
- Expand the seed dataset and add more realistic domain (movies, products, social network)
- Produce a short screencast script or automated demo script
