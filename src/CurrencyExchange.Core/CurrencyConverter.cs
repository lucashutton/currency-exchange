namespace CurrencyExchange.Core;
public class CurrencyConverter(IExchangeRateProvider rateProvider) : ICurrencyConverter
{
    public Task<ConversionResult> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}