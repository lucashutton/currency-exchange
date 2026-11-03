namespace CurrencyExchange.Core;

public record ExchangeRate(string From, string To, decimal Rate, DateOnly Date);