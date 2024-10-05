using Microsoft.Playwright;

string auth = Env("AUTH", "USER:PASS");
string url = Env("TARGET_URL", "https://example.com");
IPlaywright pw = await Playwright.CreateAsync();
await using IBrowser browser = await ConnectAsync(pw, auth);
await ScrapeAsync(browser, url);

/// <summary>
/// Connect to remote browser instance
/// </summary>
/// <param name="playwright">The playwright engine</param>
/// <param name="auth">Authentication string - i.e USER:PASS</param>
static async Task<IBrowser> ConnectAsync(IPlaywright playwright, string auth)
{
    if (auth == "USER:PASS")
    {
        throw new ArgumentException("Provide Scraping Browsers credentials in AUTH environment variable or update the script.");
    }
    Console.WriteLine("Connecting to Browser...");
    string endpointURL = $"wss://{auth}@brd.superproxy.io:9222";
    return await playwright.Chromium.ConnectOverCDPAsync(endpointURL);
}

/// <summary>
/// Scrape website with the browser instance
/// </summary>
/// <param name="browser">The browser instance</param>
/// <param name="url">The url to go to</param>
static async Task ScrapeAsync(IBrowser browser, string url)
{
    try
    {
        Console.WriteLine($"Navigating to {url}...");
        IPage page = await browser.NewPageAsync();
        _ = await page.GotoAsync(url, new() { Timeout = 2 * 60 * 1000 });
        await Task.Delay(TimeSpan.FromMinutes(2));
        Console.WriteLine("Navigated! Scraping page content...");
        string content = await page.ContentAsync();
        Console.WriteLine($"Scraped! Data: {content}");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e.Message);
    }
}

/// <summary>
/// Load a variable from the environment if it exists, else use a default value.
/// </summary>
/// <param name="browser">The browser instance</param>
/// <param name="url">The url to go to</param>
static string Env(string name, string defaultValue)
{
    return Environment.GetEnvironmentVariable(name) ?? defaultValue;
}