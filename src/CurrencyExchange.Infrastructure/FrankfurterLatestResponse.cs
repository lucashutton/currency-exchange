namespace CurrencyExchange.Infrastructure;

// Frankfurter returns JSON with resulting conversions in a dictionary structure, e.g.
// request:
// https://api.frankfurter.dev/v1/latest?base=AUD&symbols=GBP,USD,EUR
// result:
// {"amount":1.0,"base":"AUD","date":"2026-11-03","rates":{"EUR":0.62473,"GBP":0.52076,"USD":0.69838}}
internal record FrankfurterLatestResponse(DateOnly Date, Dictionary<string, decimal> Rates);