using System;
using System.Threading.Tasks;
using Neo4j.Driver;
using System.Collections.Generic;

static class Seeder
{
    // Expanded sample model: Person -[:KNOWS]-> Person, Person -[:WORKS_AT]-> Company, Person -[:INTERESTED_IN]-> Topic
    public static async Task SeedAsync(IDriver driver)
    {
        await using var session = driver.AsyncSession();
        var tx = await session.BeginTransactionAsync();
        try
        {
            // Clear small test dataset
            await tx.RunAsync("MATCH (n) WHERE exists(n.name) OR exists(n.title) DETACH DELETE n");

            // Create sample people
            var people = new[] {
                new { name = "Alice", age = 30 },
                new { name = "Bob", age = 28 },
                new { name = "Carol", age = 35 },
                new { name = "Dave", age = 40 },
                new { name = "Eve", age = 25 }
            };

            foreach (var p in people)
            {
                await tx.RunAsync("CREATE (a:Person {name:$name, age:$age})", new { name = p.name, age = p.age });
            }

            // Companies
            var companies = new[] { "Acme Corp", "Globex", "Initech" };
            foreach (var c in companies)
            {
                await tx.RunAsync("CREATE (c:Company {title:$title})", new { title = c });
            }

            // Topics
            var topics = new[] { "AI", "Cycling", "Cooking" };
            foreach (var t in topics)
            {
                await tx.RunAsync("CREATE (t:Topic {title:$title})", new { title = t });
            }

            // Relationships: KNOWS
            await tx.RunAsync(@"MATCH (a:Person {name:$n1}), (b:Person {name:$n2}) CREATE (a)-[:KNOWS {since:2020}]->(b)", new { n1 = "Alice", n2 = "Bob" });
            await tx.RunAsync(@"MATCH (a:Person {name:$n1}), (b:Person {name:$n2}) CREATE (a)-[:KNOWS {since:2018}]->(b)", new { n1 = "Bob", n2 = "Carol" });
            await tx.RunAsync(@"MATCH (a:Person {name:$n1}), (b:Person {name:$n2}) CREATE (a)-[:KNOWS {since:2019}]->(b)", new { n1 = "Carol", n2 = "Dave" });
            await tx.RunAsync(@"MATCH (a:Person {name:$n1}), (b:Person {name:$n2}) CREATE (a)-[:KNOWS {since:2021}]->(b)", new { n1 = "Dave", n2 = "Eve" });

            // WORKS_AT
            await tx.RunAsync(@"MATCH (p:Person {name:$p}), (c:Company {title:$c}) CREATE (p)-[:WORKS_AT {since:2015}]->(c)", new { p = "Alice", c = "Acme Corp" });
            await tx.RunAsync(@"MATCH (p:Person {name:$p}), (c:Company {title:$c}) CREATE (p)-[:WORKS_AT {since:2019}]->(c)", new { p = "Bob", c = "Globex" });
            await tx.RunAsync(@"MATCH (p:Person {name:$p}), (c:Company {title:$c}) CREATE (p)-[:WORKS_AT {since:2012}]->(c)", new { p = "Carol", c = "Initech" });

            // INTERESTED_IN
            await tx.RunAsync(@"MATCH (p:Person {name:$p}), (t:Topic {title:$t}) CREATE (p)-[:INTERESTED_IN]->(t)", new { p = "Alice", t = "AI" });
            await tx.RunAsync(@"MATCH (p:Person {name:$p}), (t:Topic {title:$t}) CREATE (p)-[:INTERESTED_IN]->(t)", new { p = "Bob", t = "Cycling" });
            await tx.RunAsync(@"MATCH (p:Person {name:$p}), (t:Topic {title:$t}) CREATE (p)-[:INTERESTED_IN]->(t)", new { p = "Eve", t = "Cooking" });

            await tx.CommitAsync();
            Console.WriteLine("Seeded expanded sample data.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
        finally
        {
            await session.CloseAsync();
        }
    }

    public static async Task RunSampleQueriesAsync(IDriver driver)
    {
        await using var session = driver.AsyncSession();

        // 1-hop query
        var res1 = await session.RunAsync("MATCH (a:Person)-[:KNOWS]->(b:Person) RETURN a.name AS from, b.name AS to, b.age AS age");
        await foreach (var r in res1)
        {
            Console.WriteLine($"One-hop: {r["from"].As<string>()} -> {r["to"].As<string>()} (age {r["age"].As<int>()})");
        }

        // multi-hop: path of length 2 or more
        var res2 = await session.RunAsync(@"MATCH p=(a:Person)-[:KNOWS*2..]->(b:Person) RETURN p LIMIT 10");
        var found = false;
        await foreach (var r in res2)
        {
            found = true;
            Console.WriteLine("Found multi-hop path: " + r["p"].ToString());
        }
        if (!found) Console.WriteLine("No multi-hop paths found (that's okay for small seed data).");

        await session.CloseAsync();
    }

    public static async Task QueryFriendsByNameAsync(IDriver driver, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("No name provided.");
            return;
        }

        await using var session = driver.AsyncSession();
        var cursor = await session.RunAsync(
            "MATCH (a:Person {name:$name})-[:KNOWS]->(b:Person) RETURN b.name AS friendName, b.age AS friendAge",
            new { name }
        );

        var found = false;
        await foreach (var r in cursor)
        {
            found = true;
            Console.WriteLine($"Friend: {r["friendName"].As<string>()} (age {r["friendAge"].As<int>()})");
        }

        if (!found)
            Console.WriteLine($"No friends found for '{name}'.");

        await session.CloseAsync();
    }

    public static async Task<object> GetGraphAsync(IDriver driver, int limit = 200)
    {
        var nodes = new List<object>();
        var rels = new List<object>();

        await using var session = driver.AsyncSession();

        var nCursor = await session.RunAsync($"MATCH (n) RETURN n LIMIT {limit}");
        await foreach (var r in nCursor)
        {
            var node = r["n"].As<INode>();
            nodes.Add(new { id = node.Id, labels = node.Labels, props = node.Properties });
        }

        var rCursor = await session.RunAsync($"MATCH ()-[r]->() RETURN r LIMIT {limit}");
        await foreach (var r in rCursor)
        {
            var rel = r["r"].As<IRelationship>();
            rels.Add(new { id = rel.Id, type = rel.Type, start = rel.StartNodeId, end = rel.EndNodeId, props = rel.Properties });
        }

        return new { nodes, relationships = rels };
    }

    public static void QueryFriendsOffline(string name)
    {
        // Simple offline implementation matching seeded sample data
        var offline = new Dictionary<string, string[]>
        {
            { "Alice", new[] { "Bob" } },
            { "Bob", new[] { "Carol" } },
            { "Carol", new[] { "Dave" } },
            { "Dave", new[] { "Eve" } },
            { "Eve", Array.Empty<string>() }
        };

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("No name provided.");
            return;
        }

        if (offline.TryGetValue(name, out var friends) && friends.Length > 0)
        {
            foreach (var f in friends)
            {
                Console.WriteLine($"Friend (offline): {f}");
            }
        }
        else
        {
            Console.WriteLine($"No friends found for '{name}' (offline).");
        }
    }

    public static object GetOfflineGraph()
    {
        var nodes = new List<object>
        {
            new { id = 1L, labels = new[] { "Person" }, props = new Dictionary<string, object> { { "name", "Alice" }, { "age", 30 } } },
            new { id = 2L, labels = new[] { "Person" }, props = new Dictionary<string, object> { { "name", "Bob" }, { "age", 28 } } },
            new { id = 3L, labels = new[] { "Person" }, props = new Dictionary<string, object> { { "name", "Carol" }, { "age", 35 } } },
            new { id = 4L, labels = new[] { "Person" }, props = new Dictionary<string, object> { { "name", "Dave" }, { "age", 40 } } },
            new { id = 5L, labels = new[] { "Person" }, props = new Dictionary<string, object> { { "name", "Eve" }, { "age", 25 } } }
        };

        var rels = new List<object>
        {
            new { id = 101L, type = "KNOWS", start = 1L, end = 2L, props = new Dictionary<string, object> { { "since", 2020 } } },
            new { id = 102L, type = "KNOWS", start = 2L, end = 3L, props = new Dictionary<string, object> { { "since", 2018 } } },
            new { id = 103L, type = "KNOWS", start = 3L, end = 4L, props = new Dictionary<string, object> { { "since", 2019 } } },
            new { id = 104L, type = "KNOWS", start = 4L, end = 5L, props = new Dictionary<string, object> { { "since", 2021 } } }
        };

        return new { nodes, relationships = rels };
    }
}
