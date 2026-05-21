using System.Diagnostics;

namespace Nexus.CLI;

internal static class ProcessRunner
{
    public static async Task<int> RunAsync(
        string workingDirectory,
        string fileName,
        string arguments,
        bool waitForExit)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
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
}
