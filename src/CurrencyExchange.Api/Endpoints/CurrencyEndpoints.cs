using CurrencyExchange.Core;
using CurrencyExchange.Shared;

namespace CurrencyExchange.Api.Endpoints;

public static class CurrencyEndpoints
{
    public static void MapCurrencyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/currencies", GetCurrencies);
        app.MapGet("/api/convert", ConvertCurrency);
    }

    private static async Task<IResult> GetCurrencies(IExchangeRateProvider provider, CancellationToken ct)
    {
        try
        {
            // convert provider's currency list into list of DTO objects
            IReadOnlyList<Currency> currencies = await provider.GetCurrenciesAsync(ct);
            List<CurrencyDto> currencyDtos = currencies.Select(c => new CurrencyDto(c.Code, c.Name)).ToList();
            return Results.Ok(currencyDtos);
        }
        catch (ExchangeRateUnavailableException ex)
        {
            // float error message up with status 503
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        
    }

    private static async Task<IResult> ConvertCurrency(
        string from, string to, decimal amount, ICurrencyConverter converter, IExchangeRateProvider provider, CancellationToken ct)
    {
        try
        {
            // uppercases code; invariant to avoid server language dependence
            from = from.ToUpperInvariant();
            to = to.ToUpperInvariant();
            
            // check that the currency code is valid and supported by the provider
            List<string> codes = new() { from, to };
            IReadOnlyList<Currency> currencies = await provider.GetCurrenciesAsync(ct);
            foreach (string code in codes)
            {
                if (!currencies.Any(c => c.Code == code))
                {
                    return Results.Problem(detail: $"Unknown currency code: {code}.", statusCode: StatusCodes.Status400BadRequest);
                }
            }
            

            // convert converter's result into API response object
            ConversionResult result = await converter.ConvertAsync(from, to, amount, ct);
            ConversionResponse response = new ConversionResponse(result.From, result.To, result.Amount, result.Rate, result.ConvertedAmount, result.RateDate);
            return Results.Ok(response);
        }
        catch (ExchangeRateUnavailableException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (OverflowException ex)
        {
            return Results.Problem(detail: "Amount is too large.", statusCode: StatusCodes.Status400BadRequest);
        }
    }
}