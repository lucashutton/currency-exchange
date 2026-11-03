namespace CurrencyExchange.Shared;

// analogous to ConversionResult in Core to be used by API
public record ConversionResponse(
    string From,
    string To,
    decimal Amount,
    decimal Rate,
    decimal ConvertedAmount,
    DateOnly RateDate
);