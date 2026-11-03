using CurrencyExchange.Shared;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace CurrencyExchange.Web.Services;

public class CurrencyApiClient(HttpClient http)
{
    public Task<List<CurrencyDto>> GetCurrenciesAsync(CancellationToken ct = default) 
        => GetAsync<List<CurrencyDto>>("api/currencies", ct);

    public Task<ConversionResponse> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default)
    {
        // formatted to be invariant to language settings that may affect how numbers are displayed
        string url = $"api/convert?from={from}&to={to}&amount={amount.ToString(CultureInfo.InvariantCulture)}";
        return GetAsync<ConversionResponse>(url, ct);
    }

    private async Task<T> GetAsync<T>(string url, CancellationToken ct)
    {
        try
        {
            using HttpResponseMessage response = await http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                ProblemResponse? problem = await response.Content.ReadFromJsonAsync<ProblemResponse>(ct);
                throw new ApiException(problem?.Detail ?? "Something went wrong.");
            }
            return await response.Content.ReadFromJsonAsync<T>(ct) ?? throw new ApiException("The server returned an unexpected response.");
        }
        catch (HttpRequestException)
        {
            throw new ApiException("Couldn't reach the server.");
        }
        catch (JsonException)
        {
            throw new ApiException("The server returned an unexpected response.");
        }
    }

    private record ProblemResponse(string? Detail);
    
}