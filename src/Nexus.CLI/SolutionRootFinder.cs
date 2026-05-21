namespace Nexus.CLI;

internal static class SolutionRootFinder
{
    public static string? Find()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "NexusAutomation.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        var cwd = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (cwd is not null)
        {
            if (File.Exists(Path.Combine(cwd.FullName, "NexusAutomation.sln")))
                return cwd.FullName;
            cwd = cwd.Parent;
        }

        return null;
    }
}
