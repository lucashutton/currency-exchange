using CurrencyExchange.Core;
using CurrencyExchange.Infrastructure;
using CurrencyExchange.Api.Endpoints;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// using Frankfurter currency exchange API
builder.Services.Configure<FrankfurterOptions>(
    builder.Configuration.GetSection(FrankfurterOptions.SectionName));

// registering Microsft memory cache
builder.Services.AddMemoryCache();

// registering Frankfurter as exchange rate provider with HttpClient using defined defaults
builder.Services.AddHttpClient<IExchangeRateProvider, FrankfurterRateProvider>((services, client) =>
    {
        // returns actual FrankfurterOptions out of IOptions
        FrankfurterOptions options = services.GetRequiredService<IOptions<FrankfurterOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    }
);

// registering CurrencyConverter as default (passing above rate provider)
builder.Services.AddScoped<ICurrencyConverter, CurrencyConverter>();

// allows for detailed problem messages from endpoints
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapCurrencyEndpoints();

app.Run();

