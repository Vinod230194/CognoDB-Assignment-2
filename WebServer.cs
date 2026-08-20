using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Neo4j.Driver;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;

static class WebServer
{
    private static IHost? _host;

    public static Task StartAsync(IDriver driver)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/", async ctx =>
                        {
                            ctx.Response.ContentType = "text/html";
                            await ctx.Response.WriteAsync(HtmlPage);
                        });

                        endpoints.MapGet("/api/people", async ctx =>
                        {
                            await using var session = driver.AsyncSession();
                            var res = await session.RunAsync("MATCH (p:Person) RETURN p.name AS name, p.age AS age LIMIT 100");
                            var list = new List<object>();
                            await foreach (var r in res)
                            {
                                list.Add(new { name = r["name"].As<string>(), age = r["age"].As<int>() });
                            }
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsync(JsonSerializer.Serialize(list));
                        });

                        endpoints.MapGet("/api/friends/{name}", async ctx =>
                        {
                            var name = (string?)ctx.Request.RouteValues["name"] ?? string.Empty;
                            await using var session = driver.AsyncSession();
                            var cursor = await session.RunAsync(
                                "MATCH (a:Person {name:$name})-[:KNOWS]->(b:Person) RETURN b.name AS friendName, b.age AS friendAge",
                                new { name }
                            );
                            var list = new List<object>();
                            await foreach (var r in cursor)
                            {
                                list.Add(new { name = r["friendName"].As<string>(), age = r["friendAge"].As<int>() });
                            }
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsync(JsonSerializer.Serialize(list));
                        });

                        endpoints.MapGet("/api/graph", async ctx =>
                        {
                            var graph = await Seeder.GetGraphAsync(driver);
                            // Convert node props and relationship props (IReadOnlyDictionary) to plain dictionaries for JSON
                            var nodes = ((IEnumerable<object>)graph.GetType().GetProperty("nodes")!.GetValue(graph)!).Select(n => {
                                var id = (long)n.GetType().GetProperty("id")!.GetValue(n)!;
                                var labels = (IEnumerable<string>)n.GetType().GetProperty("labels")!.GetValue(n)!;
                                var props = (IReadOnlyDictionary<string, object>)n.GetType().GetProperty("props")!.GetValue(n)!;
                                return new { id, labels, props = props.ToDictionary(kv => kv.Key, kv => kv.Value) };
                            });
                            var rels = ((IEnumerable<object>)graph.GetType().GetProperty("relationships")!.GetValue(graph)!).Select(r => {
                                var id = (long)r.GetType().GetProperty("id")!.GetValue(r)!;
                                var type = (string)r.GetType().GetProperty("type")!.GetValue(r)!;
                                var start = (long)r.GetType().GetProperty("start")!.GetValue(r)!;
                                var end = (long)r.GetType().GetProperty("end")!.GetValue(r)!;
                                var props = (IReadOnlyDictionary<string, object>)r.GetType().GetProperty("props")!.GetValue(r)!;
                                return new { id, type, start, end, props = props.ToDictionary(kv => kv.Key, kv => kv.Value) };
                            });

                            var payload = new { nodes, relationships = rels };
                            ctx.Response.ContentType = "application/json";
                            await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
                        });
                    });
                });
                webBuilder.UseUrls("http://localhost:5000");
            })
            .Build();

        return _host.StartAsync();
    }

    public static async Task StopAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
            _host = null;
        }
    }

    private const string HtmlPage = """
<!doctype html>
<html>
<head>
  <meta charset="utf-8">
  <title>CognoDB Explorer</title>
  <style>
    body { font-family: Arial, Helvetica, sans-serif; padding: 1rem; }
    input, button { padding: 0.5rem; margin: 0.2rem; }
    #people { margin-top: 1rem; }
    #graph { margin-top: 1rem; max-height: 400px; overflow: auto; border: 1px solid #ddd; padding: 0.5rem; }
  </style>
</head>
<body>
  <h1>CognoDB Explorer</h1>
  <div>
    <button id="reload">Reload people</button>
    <button id="reloadGraph">Reload graph</button>
  </div>
  <div id="people">Loading...</div>
  <h2>Graph</h2>
  <div id="graph">Loading graph...</div>
  <h2>Friends</h2>
  <input id="name" placeholder="Person name (e.g. Alice)" />
  <button id="friends">Get Friends</button>
  <div id="friendsList"></div>

  <script>
    async function loadPeople(){
      const res = await fetch('/api/people');
      const list = await res.json();
      const div = document.getElementById('people');
      div.innerHTML = list.map(p => `<div>${p.name} (age ${p.age})</div>`).join('');
    }
    async function loadGraph(){
      const res = await fetch('/api/graph');
      const data = await res.json();
      const div = document.getElementById('graph');
      div.innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
    }
    document.getElementById('reload').addEventListener('click', loadPeople);
    document.getElementById('reloadGraph').addEventListener('click', loadGraph);
    document.getElementById('friends').addEventListener('click', async () =>{
      const name = document.getElementById('name').value;
      const res = await fetch('/api/friends/' + encodeURIComponent(name));
      const list = await res.json();
      document.getElementById('friendsList').innerHTML = list.length ? list.map(f => `<div>${f.name} (age ${f.age})</div>`).join('') : 'No friends found.';
    });
    loadPeople();
    loadGraph();
  </script>
</body>
</html>
""";
}
