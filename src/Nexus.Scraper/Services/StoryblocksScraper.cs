using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using Nexus.Core.Configuration;
using Nexus.Core.DTOs;
using Nexus.Core.Enums;
using Nexus.Core.Interfaces;

namespace Nexus.Scraper.Services;

public class StoryblocksScraper : IMediaScraper, IAsyncDisposable
{
    private readonly ScraperOptions _options;
    private readonly ILogger<StoryblocksScraper> _logger;
    private readonly Random _random = new();

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;

    public StoryblocksScraper(
        IOptions<ScraperOptions> options,
        ILogger<StoryblocksScraper> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        _playwright = await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = _options.HeadlessBrowser,
            Args = new[]
            {
                "--disable-blink-features=AutomationControlled",
                "--no-sandbox",
                "--disable-dev-shm-usage"
            }
        };

        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);

        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            Locale = "en-US"
        };

        if (File.Exists(_options.CookieFilePath))
        {
            _context = await _browser.NewContextAsync(contextOptions);
            await LoadCookiesAsync();
            _logger.LogInformation("Loaded existing cookies from {Path}", _options.CookieFilePath);
        }
        else
        {
            _context = await _browser.NewContextAsync(contextOptions);
        }
    }

    public async Task<bool> ValidateSessionAsync(CancellationToken ct = default)
    {
        if (_context == null) return false;

        try
        {
            var page = await _context.NewPageAsync();
            await page.GotoAsync("https://www.storyblocks.com/video", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            await HumanDelay();

            var isLoggedIn = await page.Locator("[data-testid='user-menu'], .user-avatar, .account-menu")
                .CountAsync() > 0;

            await page.CloseAsync();
            return isLoggedIn;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Session validation failed");
            return false;
        }
    }

    public async Task<MediaDownloadResult> DownloadVerticalVideoAsync(
        NicheType niche,
        string[] searchKeywords,
        CancellationToken ct = default)
    {
        if (_context == null)
            await InitializeAsync(ct);

        try
        {
            var isValid = await ValidateSessionAsync(ct);
            if (!isValid)
            {
                var loginResult = await AttemptLoginAsync(ct);
                if (!loginResult)
                {
                    return new MediaDownloadResult
                    {
                        Success = false,
                        ErrorMessage = "Login failed — human intervention required",
                        RequiresHumanIntervention = true
                    };
                }
            }

            var page = await _context!.NewPageAsync();
            var keyword = searchKeywords[_random.Next(searchKeywords.Length)];

            _logger.LogInformation("Searching Storyblocks for: {Keyword} (niche: {Niche})", keyword, niche);

            await page.GotoAsync("https://www.storyblocks.com/video", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });
            await HumanDelay();

            if (!string.IsNullOrEmpty(_options.Selectors.SearchBar))
            {
                await page.FillAsync(_options.Selectors.SearchBar, keyword);
                await page.PressAsync(_options.Selectors.SearchBar, "Enter");
                await HumanDelay();
            }

            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await HumanDelay();

            var downloadDir = Path.Combine(_options.DownloadDirectory, niche.ToString());
            Directory.CreateDirectory(downloadDir);

            var filePath = Path.Combine(downloadDir, $"{Guid.NewGuid()}.mp4");

            if (!string.IsNullOrEmpty(_options.Selectors.DownloadButton))
            {
                var downloadButton = page.Locator(_options.Selectors.DownloadButton).First;
                if (await downloadButton.CountAsync() > 0)
                {
                    var download = await page.RunAndWaitForDownloadAsync(async () =>
                    {
                        await downloadButton.ClickAsync();
                    });

                    await download.SaveAsAsync(filePath);
                }
            }

            await SaveCookiesAsync();
            await page.CloseAsync();

            var tags = ExtractTagsFromKeyword(keyword);

            return new MediaDownloadResult
            {
                Success = true,
                FilePath = filePath,
                Tags = tags
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download media for niche {Niche}", niche);

            if (IsCaptchaDetected(ex))
            {
                _logger.LogWarning("CAPTCHA detected — switching to headed mode for human intervention");
                return await HandleCaptchaFallbackAsync(ct);
            }

            return new MediaDownloadResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private async Task<bool> AttemptLoginAsync(CancellationToken ct)
    {
        if (_context == null) return false;

        try
        {
            var page = await _context.NewPageAsync();
            await page.GotoAsync("https://www.storyblocks.com/login", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });
            await HumanDelay();

            if (string.IsNullOrEmpty(_options.Selectors.EmailInput) ||
                string.IsNullOrEmpty(_options.Selectors.PasswordInput) ||
                string.IsNullOrEmpty(_options.Selectors.LoginButton))
            {
                _logger.LogError("Login selectors not configured — cannot attempt automated login");
                return false;
            }

            _logger.LogInformation("Attempting automated login...");

            await page.CloseAsync();
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login attempt failed");
            return false;
        }
    }

    private async Task<MediaDownloadResult> HandleCaptchaFallbackAsync(CancellationToken ct)
    {
        _logger.LogWarning("╔══════════════════════════════════════════════════════════╗");
        _logger.LogWarning("║  HUMAN INTERVENTION REQUIRED: CAPTCHA detected.         ║");
        _logger.LogWarning("║  A headed browser window will open.                     ║");
        _logger.LogWarning("║  Please solve the CAPTCHA and complete the login.       ║");
        _logger.LogWarning("╚══════════════════════════════════════════════════════════╝");

        if (_browser != null)
        {
            await _browser.CloseAsync();
        }

        _browser = await _playwright!.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false
        });

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
        });

        var page = await _context.NewPageAsync();
        await page.GotoAsync("https://www.storyblocks.com/login");

        _logger.LogWarning("Waiting for human to complete login (max 5 minutes)...");
        try
        {
            await page.WaitForURLAsync("**/storyblocks.com/**", new PageWaitForURLOptions
            {
                Timeout = 300000
            });
            await SaveCookiesAsync();
            _logger.LogInformation("Human login completed. Cookies saved.");
            await page.CloseAsync();
        }
        catch
        {
            _logger.LogError("Timed out waiting for human intervention");
        }

        return new MediaDownloadResult
        {
            Success = false,
            ErrorMessage = "CAPTCHA resolved — retry the download",
            RequiresHumanIntervention = true
        };
    }

    private static bool IsCaptchaDetected(Exception ex) =>
        ex.Message.Contains("captcha", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("challenge", StringComparison.OrdinalIgnoreCase);

    private static string[] ExtractTagsFromKeyword(string keyword) =>
        keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private async Task HumanDelay()
    {
        var delay = _random.Next(_options.MinDelayMs, _options.MaxDelayMs);
        await Task.Delay(delay);
    }

    private async Task SaveCookiesAsync()
    {
        if (_context == null) return;
        var cookies = await _context.CookiesAsync();
        var json = JsonSerializer.Serialize(cookies, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_options.CookieFilePath, json);
    }

    private async Task LoadCookiesAsync()
    {
        if (_context == null || !File.Exists(_options.CookieFilePath)) return;
        var json = await File.ReadAllTextAsync(_options.CookieFilePath);
        var cookies = JsonSerializer.Deserialize<List<Cookie>>(json);
        if (cookies != null)
        {
            await _context.AddCookiesAsync(cookies);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null) await _context.DisposeAsync();
        if (_browser != null) await _browser.DisposeAsync();
        _playwright?.Dispose();
    }
}
