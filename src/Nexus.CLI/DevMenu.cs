namespace Nexus.CLI;

internal sealed class DevMenu
{
    private readonly string _root;

    public DevMenu(string root) => _root = root;

    public async Task<int> RunAsync()
    {
        while (true)
        {
            PrintMenu();
            Console.Write("Select option: ");
            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    await RunMigrationsAsync();
                    break;
                case "2":
                    await StartBackendAsync();
                    break;
                case "3":
                    await StartFrontendAsync();
                    break;
                case "4":
                    CleanTempFolders();
                    break;
                case "5":
                    await InstallPlaywrightBrowsersAsync();
                    break;
                case "0":
                case "q":
                case "Q":
                    return 0;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }

            Console.WriteLine();
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║     Nexus Shorts — Developer CLI     ║");
        Console.WriteLine("╠══════════════════════════════════════╣");
        Console.WriteLine("║  1. Run EF migrations (update DB)    ║");
        Console.WriteLine("║  2. Start Backend / Web API          ║");
        Console.WriteLine("║  3. Start Angular frontend           ║");
        Console.WriteLine("║  4. Clean download/render temp dirs  ║");
        Console.WriteLine("║  5. Install Playwright browsers      ║");
        Console.WriteLine("║  0. Exit                             ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
    }

    private async Task RunMigrationsAsync()
    {
        Console.WriteLine("Applying EF Core migrations...");
        await ProcessRunner.RunAsync(_root, "dotnet", "tool restore", waitForExit: true);
        var code = await ProcessRunner.RunAsync(
            _root,
            "dotnet",
            "ef database update --project src/Nexus.Data/Nexus.Data.csproj --startup-project src/Nexus.API/Nexus.API.csproj",
            waitForExit: true);

        if (code != 0)
            Console.WriteLine($"Migration failed (exit {code}). Ensure PostgreSQL is running and ConnectionStrings:PostgreSQL is set.");
        else
            Console.WriteLine("Database updated successfully.");
    }

    private async Task StartBackendAsync()
    {
        Console.WriteLine("Starting Nexus.API (Ctrl+C to stop)...");
        await ProcessRunner.RunAsync(
            _root,
            "dotnet",
            "run --project src/Nexus.API/Nexus.API.csproj",
            waitForExit: true);
    }

    private async Task StartFrontendAsync()
    {
        var dashboard = Path.Combine(_root, "client", "nexus-dashboard");
        if (!Directory.Exists(dashboard))
        {
            Console.WriteLine($"Dashboard not found: {dashboard}");
            return;
        }

        if (!Directory.Exists(Path.Combine(dashboard, "node_modules")))
        {
            Console.WriteLine("node_modules missing — running npm install first...");
            var installCode = await ProcessRunner.RunAsync(dashboard, "npm", "install", waitForExit: true);
            if (installCode != 0)
                return;
        }

        Console.WriteLine("Starting Angular dashboard (Ctrl+C to stop)...");
        await ProcessRunner.RunAsync(dashboard, "npm", "start", waitForExit: true);
    }

    private void CleanTempFolders()
    {
        var targets = new[]
        {
            Path.Combine(_root, "data", "downloads"),
            Path.Combine(_root, "data", "temp"),
            Path.Combine(_root, "output"),
            Path.Combine(_root, "temp")
        };

        foreach (var path in targets)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine($"Skip (not found): {path}");
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                try { File.Delete(file); }
                catch (Exception ex) { Console.WriteLine($"  Could not delete {file}: {ex.Message}"); }
            }

            foreach (var dir in Directory.EnumerateDirectories(path).OrderByDescending(d => d.Length))
            {
                try { Directory.Delete(dir, recursive: true); }
                catch (Exception ex) { Console.WriteLine($"  Could not delete {dir}: {ex.Message}"); }
            }

            Console.WriteLine($"Cleaned: {path}");
        }
    }

    private async Task InstallPlaywrightBrowsersAsync()
    {
        Console.WriteLine("Installing Playwright Chromium (required for Storyblocks scraper)...");

        Console.WriteLine("Step 1/2: Building Nexus.Scraper...");
        var buildCode = await ProcessRunner.RunAsync(
            _root,
            "dotnet",
            "build src/Nexus.Scraper/Nexus.Scraper.csproj",
            waitForExit: true);

        if (buildCode != 0)
        {
            Console.WriteLine($"Build failed (exit {buildCode}). Aborting Playwright install.");
            return;
        }

        Console.WriteLine("Step 2/2: Installing Playwright Chromium...");
        var scraperOutputDir = Path.Combine(_root, "src", "Nexus.Scraper", "bin", "Debug", "net8.0");
        var playwrightScript = Path.Combine(scraperOutputDir, "playwright.ps1");

        if (!File.Exists(playwrightScript))
        {
            Console.WriteLine($"Playwright bootstrap script not found at: {playwrightScript}");
            Console.WriteLine("Ensure the Microsoft.Playwright NuGet package is referenced in Nexus.Scraper.csproj.");
            return;
        }

        var installCode = await ProcessRunner.RunAsync(
            _root,
            "powershell",
            $"-NoProfile -ExecutionPolicy Bypass -File \"{playwrightScript}\" install chromium",
            waitForExit: true);

        if (installCode == 0)
        {
            Console.WriteLine("Playwright browsers ready.");
        }
        else
        {
            Console.WriteLine($"Playwright install failed (exit {installCode}). Run manually:");
            Console.WriteLine($"  powershell -NoProfile -ExecutionPolicy Bypass -File \"{playwrightScript}\" install chromium");
        }
    }
}
