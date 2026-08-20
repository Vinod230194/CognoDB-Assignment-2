Support notes for interview follow-up

1) Why a graph database?
- Natural representation of relationships (friends, co-workers, topics).
- Efficient multi-hop traversals and neighborhood queries.
- Cypher is expressive for pattern matching and path queries.

2) Seed reproduction
- Use `seed.ps1` or `seed.sh` with environment variables set, or run `dotnet run` after setting `COGNODB_*` env vars.

3) Main queries
- See `queries.cypher` for 1-hop, multi-hop, parameterized, and shortest-path examples.

4) Troubleshooting
- If connectivity fails: verify `COGNODB_URI`, DNS resolution, TLS scheme (`bolt+s://`), and credentials.
- Check driver version compatibility (Neo4j.Driver 6.x with Bolt 5+).

5) Architecture
- Single .NET 9 console app owns the driver and seeds data. Minimal web server is started from the console for the demo.
- All configuration from environment variables; no secrets in repo.

6) What to expect in interview
- Be ready to explain the seed data model, why the chosen model benefits from a graph, and walk through Cypher queries.

