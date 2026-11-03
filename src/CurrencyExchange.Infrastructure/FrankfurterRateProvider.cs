using CurrencyExchange.Core;
using System.Net.Http.Json;

namespace CurrencyExchange.Infrastructure;

// exchange rate provider using the free open-source Frankfurter API
public class FrankfurterRateProvider(HttpClient client) : IExchangeRateProvider
{
    public async Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken ct = default)
    {
        string url = $"currencies";

        // currencies are returned as code : full name
        Dictionary<String, String> rawCurrencies;

        try
        {
            rawCurrencies = await client.GetFromJsonAsync<Dictionary<String, String>>(url, ct)
                ?? throw new ExchangeRateUnavailableException("Frankfurter API returned an empty response.");
        }
        catch (HttpRequestException ex)
        {
            throw new ExchangeRateUnavailableException("Frankfurter API could not be reached.", ex);
        }
        // ignore case where user cancelled the request themselves
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
        // ignore case where user cancelled the request themselves
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            throw new ExchangeRateUnavailableException("Frankfurter API timed out.", ex);
        }
        
        // convert to ExchangeRate object
        ExchangeRate rate = new ExchangeRate(from, to, response.Rates[to], response.Date);

        return rate;
    }
}