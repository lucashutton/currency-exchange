namespace CurrencyExchange.Core;
public interface ICurrencyConverter
{
    Task<ConversionResult> ConvertAsync(string from, string to, decimal amount, CancellationToken ct = default);
}