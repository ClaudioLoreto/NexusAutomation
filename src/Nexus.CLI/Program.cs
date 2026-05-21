using Nexus.CLI;

var root = SolutionRootFinder.Find();
if (root is null)
{
    Console.Error.WriteLine("Could not locate NexusAutomation.sln. Run from the repository root.");
    return 1;
}

Console.WriteLine($"Nexus CLI — repo root: {root}");
Console.WriteLine();

var menu = new DevMenu(root);
return await menu.RunAsync();
