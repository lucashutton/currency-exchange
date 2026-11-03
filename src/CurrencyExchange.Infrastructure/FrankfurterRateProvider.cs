using CurrencyExchange.Core;
using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyExchange.Infrastructure;

// exchange rate provider using the free open-source Frankfurter API
public class FrankfurterRateProvider(HttpClient client, IMemoryCache memoryCache) : IExchangeRateProvider
{
    public async Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        string url = "currencies";

        // currencies are returned as code : full name
        Dictionary<string, string> rawCurrencies;

        try
        {
            // because the currency list doesn't change we cache it for faster lookup
            rawCurrencies = await memoryCache.GetOrCreateAsync("currencies", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
                    return await client.GetFromJsonAsync<Dictionary<string, string>>(url, ct)
                        ?? throw new ExchangeRateUnavailableException("Frankfurter API returned an empty response.");
                }
            // GetOrCreateAsync is a nullable method so we need this extra error throw even though it will never happen
            ) ?? throw new ExchangeRateUnavailableException("Frankfurter API returned an empty response.");

        }
        catch (HttpRequestException ex)
        {
            throw new ExchangeRateUnavailableException("Frankfurter API could not be reached.", ex);
        }
        // pass through case where user cancelled the request themselves
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new ExchangeRateUnavailableException("Frankfurter API timed out.", ex);
        }

        // convert dictionary of strings into list of Currency objects 
        List<Currency> currencies = rawCurrencies.Select(c => new Currency(c.Key, c.Value)).ToList();

        return currencies;
    }

    public async Task<ExchangeRate> GetRateAsync(string from, string to, CancellationToken ct = default)
    {
        string url = $"latest?base={from}&symbols={to}";
        FrankfurterLatestResponse response;

        try
        {
            response = await client.GetFromJsonAsync<FrankfurterLatestResponse>(url, ct)
                ?? throw new ExchangeRateUnavailableException("Frankfurter API returned an empty response.");
        }
        catch (HttpRequestException ex)
        {
            throw new ExchangeRateUnavailableException("Frankfurter API could not be reached.", ex);
        }
        // pass through case where user cancelled the request themselves
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new ExchangeRateUnavailableException("Frankfurter API timed out.", ex);
        }
        
        // catch potential cases of incorrect codes being standardised externally by the API and not matching original string
        if (!response.Rates.TryGetValue(to, out decimal rate))
        {
            throw new ExchangeRateUnavailableException($"Frankfurter API returned no rate for {to}.");
        }

        // convert to ExchangeRate object
        ExchangeRate exchangeRate = new ExchangeRate(from, to, rate, response.Date);

        return exchangeRate;
    }
}