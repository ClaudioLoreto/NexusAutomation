using System.Text.Json;
using Microsoft.Playwright;

namespace Nexus.Scraper.Storyblocks;

internal static class CookiePersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public static bool Exists(string cookiePath) => File.Exists(cookiePath);

    public static async Task LoadAsync(IBrowserContext context, string cookiePath, CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(cookiePath, cancellationToken);
        var cookies = JsonSerializer.Deserialize<Cookie[]>(json, JsonOptions)
            ?? throw new InvalidOperationException($"Cookie file is empty or invalid: {cookiePath}");
        await context.AddCookiesAsync(cookies);
    }

    public static async Task SaveAsync(IBrowserContext context, string cookiePath, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(cookiePath));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var cookies = await context.CookiesAsync();
        var json = JsonSerializer.Serialize(cookies, JsonOptions);
        await File.WriteAllTextAsync(cookiePath, json, cancellationToken);
    }
}
