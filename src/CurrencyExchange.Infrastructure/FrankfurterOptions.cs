namespace CurrencyExchange.Infrastructure;

public class FrankfurterOptions
{
    public const string SectionName = "Frankfurter";

    // property declaration with defaults for when .NET creates a new object
    public string BaseUrl { get; set; } = "";
    public int TimeoutSeconds { get; set; } = 10;
}