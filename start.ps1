# PowerShell launcher for the Nexus.CLI developer menu.
# Usage:
#   - Right-click -> "Run with PowerShell", or
#   - From a PowerShell prompt:  .\start.ps1
#
# Note for double-click users: Windows opens .ps1 files in an editor by default.
# If you want a true double-click experience, use start.bat (same outcome).

$ErrorActionPreference = 'Stop'

# Anchor to this script's directory so relative paths in dotnet resolve correctly.
Set-Location -LiteralPath $PSScriptRoot

try {
    dotnet run --project 'src\Nexus.CLI\Nexus.CLI.csproj'
    $exitCode = $LASTEXITCODE
}
catch {
    Write-Host ''
    Write-Host "Launcher error: $($_.Exception.Message)" -ForegroundColor Red
    $exitCode = 1
}

Write-Host ''
Write-Host "--- Nexus.CLI exited with code $exitCode ---"

# Only prompt to keep the window open if we're in a host that would otherwise
# close (e.g. launched via Explorer "Run with PowerShell"). When called from an
# existing prompt we just return naturally.
if ($Host.Name -eq 'ConsoleHost' -and -not $env:WT_SESSION) {
    Read-Host -Prompt 'Press Enter to exit'
}

exit $exitCode
