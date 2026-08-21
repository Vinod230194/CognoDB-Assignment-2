using System;
using System.Net;
using System.Threading.Tasks;
using System.Linq;
using Neo4j.Driver;
using Microsoft.Extensions.Configuration;

class Program
{
    static async Task Main(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var uri = config["COGNODB_URI"];
        var user = config["COGNODB_USER"] ?? "cognodb";
        var password = config["COGNODB_PASSWORD"];

        IDriver? driver = null;

        if (string.IsNullOrWhiteSpace(uri))
        {
            Console.Error.WriteLine("COGNODB_URI not set. Starting in offline demo mode. The web UI will show seeded demo data. To connect to a real database set COGNODB_URI/COGNODB_USER/COGNODB_PASSWORD.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.Error.WriteLine("COGNODB_PASSWORD not set. Cannot connect to database.");
                return;
            }

            if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsed) || string.IsNullOrEmpty(parsed.Host))
                throw new ArgumentException("Invalid URI for Neo4j driver", nameof(uri));

            // Diagnostic: check DNS resolution before driver attempts to connect
            try
            {
                Console.WriteLine($"Attempting DNS resolution for host: {parsed.Host}");
                var addresses = await Dns.GetHostAddressesAsync(parsed.Host);
                Console.WriteLine($"Resolved {parsed.Host} to: {string.Join(", ", addresses.Select(a => a.ToString()).ToArray())}");
            }
            catch (Exception dnsEx)
            {
                Console.Error.WriteLine($"DNS resolution failed for host '{parsed.Host}': {dnsEx.Message}");
                Console.Error.WriteLine("Ensure the hostname is correct and reachable from this environment.");
                return;
            }

            // Create driver and verify connectivity with clearer error messages
            driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
            try
            {
                await driver.VerifyConnectivityAsync();
                Console.WriteLine("✅ Connected successfully to CognoDB!");

                // Seed sample data and run example queries
                try
                {
                    await Seeder.SeedAsync(driver);
                    await Seeder.RunSampleQueriesAsync(driver);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Seeding or sample queries failed: " + ex);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Connectivity verification failed.");
                Console.Error.WriteLine("Exception: " + ex.GetType().FullName + " - " + ex.Message);
                if (ex.InnerException != null)
                    Console.Error.WriteLine("Inner: " + ex.InnerException.GetType().FullName + " - " + ex.InnerException.Message);

                Console.Error.WriteLine("Checklist:");
                Console.Error.WriteLine(" - Is COGNODB_URI correct and does the host resolve from this environment?");
                Console.Error.WriteLine(" - Are you using the correct scheme (bolt://, bolt+s://, neo4j://, neo4j+s://) matching the server TLS config?");
                Console.Error.WriteLine(" - Is the username/password correct and not expired?");
                Console.Error.WriteLine(" - If running in container or cloud, check firewall and DNS configuration.");

                // Fall back to offline mode instead of exiting
                Console.Error.WriteLine("Falling back to offline demo mode. The web UI will show sample data.");
                driver = null;
            }
        }

        // Interactive loop
        Console.WriteLine();
        Console.WriteLine("Interactive mode: type a command or 'help' for options. Type 'exit' to quit.");

        bool webRunning = false;
        Task webTask = Task.CompletedTask;

        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
                continue;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting...");
                break;
            }

            if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Commands:");
                Console.WriteLine("  friends <name>   - Query friends of a person (parameterized)");
                Console.WriteLine("  seed             - Re-run the seeder to refresh sample data (only when connected to DB)");
                Console.WriteLine("  web              - Start minimal web UI on the configured port");
                Console.WriteLine("  exit             - Quit the program");
                continue;
            }

            var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            var cmd = parts[0].ToLowerInvariant();
            var arg = parts.Length > 1 ? parts[1] : null;

            switch (cmd)
            {
                case "friends":
                    if (driver != null)
                        await Seeder.QueryFriendsByNameAsync(driver, arg);
                    else
                        Seeder.QueryFriendsOffline(arg);
                    break;
                case "seed":
                    if (driver != null)
                    {
                        await Seeder.SeedAsync(driver);
                        Console.WriteLine("Seeding complete.");
                    }
                    else
                    {
                        Console.WriteLine("Cannot seed: no database connection (offline mode).");
                    }
                    break;
                case "web":
                    if (!webRunning)
                    {
                        webRunning = true;
                        webTask = WebServer.StartAsync(driver);
                        Console.WriteLine("Web UI starting on the configured port. Use 'exit' to quit this program.");
                    }
                    else
                    {
                        Console.WriteLine("Web UI already running.");
                    }
                    break;
                default:
                    Console.WriteLine($"Unknown command: {cmd}. Type 'help' for options.");
                    break;
            }
        }

        // Stop web server if running
        if (webRunning)
        {
            await WebServer.StopAsync();
            await webTask;
        }

        if (driver != null)
            await ((IAsyncDisposable)driver).DisposeAsync();

        Console.WriteLine("Done.");
    }
}
