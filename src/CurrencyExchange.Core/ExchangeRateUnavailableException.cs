namespace CurrencyExchange.Core;

public class ExchangeRateUnavailableException(string message, Exception? innerException = null) : Exception(message, innerException);