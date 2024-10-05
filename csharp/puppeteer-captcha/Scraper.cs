﻿using PuppeteerSharp;
using System.Net.WebSockets;
using System.Text;

string auth = Env("AUTH", "USER:PASS");
string url = Env("TARGET_URL", "https://example.com");
await using IBrowser browser = await ConnectAsync(auth);
await ScrapeAsync(browser, url);

/// <summary>
/// Connect to remote browser instance
/// </summary>
/// <param name="auth">Authentication string - i.e USER:PASS</param>
static async Task<IBrowser> ConnectAsync(string auth)
{
    ConnectOptions options = new()
    {
        BrowserWSEndpoint = "wss://brd.superproxy.io:9222",
        WebSocketFactory = async (uri, options, cToken) =>
        {
            ClientWebSocket socket = new();
            byte[] authBytes = Encoding.UTF8.GetBytes(auth);
            string authHeader = "Basic " + Convert.ToBase64String(authBytes);
            socket.Options.SetRequestHeader("Authorization", authHeader);
            socket.Options.KeepAliveInterval = TimeSpan.Zero;
            await socket.ConnectAsync(uri, cToken);
            return socket;
        },
    };
    return await Puppeteer.ConnectAsync(options);
}

/// <summary>
/// Solve captcha with the browser instance
/// </summary>
/// <param name="browser">The browser instance</param>
/// <param name="url">The url to go to</param>
static async Task ScrapeAsync(IBrowser browser, string url)
{
    try
    {
        Console.WriteLine($"Navigating to {url}...");
        IPage page = await browser.NewPageAsync();
        ICDPSession session = await page.Target.CreateCDPSessionAsync();
        _ = await page.GoToAsync(url, 2 * 60 * 1000);
        Console.WriteLine("Navigated! Waiting captcha to detect and solve...");
        var result = await session.SendAsync("Captcha.waitForSolve", new
        {
            detectTimeout = 10 * 1000,
        });
        string status = (string)result["status"]!;
        Console.WriteLine($"Captcha status: {status}");
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