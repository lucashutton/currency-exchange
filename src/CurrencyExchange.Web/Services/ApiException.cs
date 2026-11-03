namespace CurrencyExchange.Web.Services;

public class ApiException(string message) : Exception(message);