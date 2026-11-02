namespace CurrencyExchange.Core;
public class CurrencyConverter(IExchangeRateProvider rateProvider) : ICurrencyConverter
{
    public async Task<ConversionResult> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default)
    {
        decimal rate;
        DateOnly date;

        // reject non-positive amounts
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than 0.");
        }
        // if the currencies are the same, shortcut a lookup
        if (from == to)
        {
            rate = 1;
            date = DateOnly.FromDateTime(DateTime.UtcNow);
        } 
        // otherwise, call the rateProvider to get the exchange rate
        else
        {
            ExchangeRate exchangeRate = await rateProvider.GetRateAsync(from, to, ct);
            rate = exchangeRate.Rate;
            date = exchangeRate.Date;
        }

        // compute the converted currency
        decimal convertedAmount = Math.Round(rate * amount, 2, MidpointRounding.AwayFromZero);

        ConversionResult result = new ConversionResult(from, to, amount, rate, convertedAmount, date);
        return result;
    }
}