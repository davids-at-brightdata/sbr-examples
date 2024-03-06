﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;

class Scraper
{

    private string _auth;

    public Scraper(string auth)
    {
        _auth = auth;
    }

    private WebDriver Connect()
    {
        if (_auth == "USER:PASS")
        {
            throw new Exception("Provide Scraping Browsers credentials in AUTH"
                    + " environment variable or update the script.");
        }
        var uri = new Uri($"https://{_auth}@brd.superproxy.io:9515");
        var executor = new HttpCommandExecutor(uri, TimeSpan.FromSeconds(60));
        var cdpCommand = new HttpCommandInfo(HttpCommandInfo.PostCommand,
                "/session/{sessionId}/goog/cdp/execute");
        executor.TryAddCommand("cdp", cdpCommand);
        var capabilities = new ChromeOptions().ToCapabilities();
        return new RemoteWebDriver(executor, capabilities);
    }

    public void Scrape(string url)
    {
        Log("Connecting to Browser...");
        var driver = Connect();
        try {
            Log($"Connected! Navigating to {url}...");
            driver.Navigate().GoToUrl(url);
            Log("Navigated! Waiting captcha to detect and solve...");
            var result = (Dictionary<string, object>) driver.ExecuteCustomDriverCommand("cdp", new ()
            {
                {"cmd", "Captcha.solve"},
                {"params", new Dictionary<string, object>(){
                    {"detectTimeout", 10000},
                }},
            }) as Dictionary<string, object>;
            var status = (string) result!["status"];
            Log($"Captcha status: {status}");
        } finally {
            driver.Quit();
        }
    }

    private static string Env(string name, string defaultValue)
    {
        return Environment.GetEnvironmentVariable(name) ?? defaultValue;
    }

    private static void Log(string message)
    {
        Console.WriteLine(message);
    }

    public static void Main()
    {
        var auth = Env("AUTH", "USER:PASS");
        var url = Env("TARGET_URL", "https://example.com");
        var scraper = new Scraper(auth);
        scraper.Scrape(url);
    }

}
