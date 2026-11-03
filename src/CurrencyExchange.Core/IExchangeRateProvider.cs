namespace CurrencyExchange.Core;

public interface IExchangeRateProvider
{
    Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken ct = default);
    Task<ExchangeRate> GetRateAsync(string from, string to, CancellationToken ct = default);
}