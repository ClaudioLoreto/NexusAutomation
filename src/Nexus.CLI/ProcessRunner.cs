using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Nexus.CLI;

internal static class ProcessRunner
{
    public static async Task<int> RunAsync(
        string workingDirectory,
        string fileName,
        string arguments,
        bool waitForExit)
    {
        var resolvedFileName = ResolveExecutable(fileName);

        var psi = new ProcessStartInfo
        {
            FileName = resolvedFileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false
        };

        using var process = Process.Start(psi);
        if (process is null)
            return -1;

        if (!waitForExit)
            return 0;

        await process.WaitForExitAsync();
        return process.ExitCode;
    }

    // On Windows, Process.Start("npm") fails because the real file is "npm.cmd"
    // (same for npx, yarn, pnpm, ng, etc.). .NET's process launcher does not
    // probe PATHEXT the way cmd.exe does. We resolve the executable ourselves:
    // if the caller passes a bare name with no extension, look it up on PATH
    // trying every PATHEXT variant. If nothing is found, fall back to the
    // original name so the original error surface is preserved.
    private static string ResolveExecutable(string fileName)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return fileName;

        if (Path.IsPathRooted(fileName) || fileName.Contains(Path.DirectorySeparatorChar) || fileName.Contains(Path.AltDirectorySeparatorChar))
            return fileName;

        if (Path.HasExtension(fileName))
            return fileName;

        var pathExt = Environment.GetEnvironmentVariable("PATHEXT")
                      ?? ".COM;.EXE;.BAT;.CMD";
        var extensions = pathExt
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim())
            .Where(e => e.Length > 0)
            .ToArray();

        var pathDirs = (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var dir in pathDirs)
        {
            foreach (var ext in extensions)
            {
                string candidate;
                try
                {
                    candidate = Path.Combine(dir.Trim(), fileName + ext);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                if (File.Exists(candidate))
                    return candidate;
            }
        }

        return fileName;
    }
}
