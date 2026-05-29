@echo off
REM Double-click launcher for the Nexus.CLI developer menu.
REM Anchors to this script's directory so it works regardless of where it's invoked from.

setlocal
cd /d "%~dp0"

dotnet run --project src\Nexus.CLI\Nexus.CLI.csproj
set EXIT_CODE=%ERRORLEVEL%

REM Keep the window open after the CLI exits so output stays readable.
REM Especially important when "dotnet" is missing or the build fails.
echo.
echo --- Nexus.CLI exited with code %EXIT_CODE% ---
pause
endlocal & exit /b %EXIT_CODE%
