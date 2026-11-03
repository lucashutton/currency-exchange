namespace CurrencyExchange.Shared;

// Data Transfer Object (DTO) to protect frontend from changes to Core
public record CurrencyDto(string Code, string Name);