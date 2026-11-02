namespace CurrencyExchange.Core;
public record ConversionResult(
    string From, 
    string To, 
    decimal Amount, 
    decimal Rate, 
    decimal ConvertedAmount, 
    DateOnly RateDate
);