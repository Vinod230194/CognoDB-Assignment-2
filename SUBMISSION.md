Submission checklist and instructions — CognoDB Demo

This document prepares the final deliverables required by the assessment (sections 6, 7, 8, 9).

Deliverables included in this repository
- Full source code: application, `Seeder.cs`, `WebServer.cs`, `Program.cs`.
- Data-loading scripts: `Seeder.cs`, `seed.ps1`, `seed.sh`.
- Cypher queries: `queries.cypher`.
- README: `README.md` (use case, "Why a graph database?", setup and run instructions).
- Minimal web UI (local): `WebServer.cs` and embedded SPA (http://localhost:5000).
- Demo helpers: `demo.ps1`, `demo.sh`.
- Screencast script: `screencast.md`.
- CI workflow: `.github/workflows/ci.yml`.

What remains (mandatory per assessment)
- Hosted demo link (public): a live URL where the app is running. You must deploy the app to a hosting provider and add the URL below.
- A short screen recording (see `screencast.md`) showing seeding, UI and queries.

How to produce the hosted demo (quick options)
- Azure App Service (Linux): publish the .NET project as a self-contained app or framework-dependent deployment and point the app to your CognODB instance. Use GitHub Actions for CI/CD.
- Render / Fly / Railway: these providers can run .NET 9 apps; create a new service and set environment variables.

Before publishing: security checklist
- Remove secrets from repo (do NOT commit `COGNODB_PASSWORD`).
- Add any `.env` files to `.gitignore` (already added).
- Rotate the CognODB password if you accidentally committed it.

Required submission items and locations
1. Repository URL: (paste here after pushing)
   - https://github.com/<your-username>/<repo-name>
2. Hosted demo URL: (paste here)
   - https://<your-demo-host>
3. Short screencast: attach or provide a link (YouTube unlisted or cloud storage)
4. Contact email to send: hr@wexa.ai with subject "CognoDB Assignment 2 — <Your Name>"

Keep your CognODB instance running
- Keep the instance provisioned until you hear from the assessors. If you plan to stop the instance, include instructions how to repro locally with a dump or seed script.

Support preparation
- Save answers to the following in this repo (as `SUPPORT.md` or include below):
  - Short explanation of why a graph database for the use case.
  - Main Cypher queries and rationale (already in `queries.cypher`).
  - How to reproduce the seed locally.

Paste hosted demo URL here after deployment:
- DEMO_URL_PLACEHOLDER

Paste repository URL here after pushing:
- REPO_URL_PLACEHOLDER
