using System.Net;
using System.Net.Http.Json;
using CurrencyExchange.Core;
using CurrencyExchange.Shared;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CurrencyExchange.Api.Tests;

public class CurrencyEndpointsTests
{
    // start the API in memory using a rate provider given in construction (mockery for testing purposes)
    private static WebApplicationFactory<Program> CreateFactory(IExchangeRateProvider provider) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton(provider)));

    [Fact]
    public async Task GetCurrencies_ReturnsCurrenciesFromProvider()
    {
        // mock exchange rate provider
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Currency> { new("TST", "Test Currency") });
        await using var factory = CreateFactory(provider);

        // http client
        var client = factory.CreateClient();


        var currencies = await client.GetFromJsonAsync<List<CurrencyDto>>("/api/currencies");
        var currency = Assert.Single(currencies!);

        Assert.Equal(new CurrencyDto("TST", "Test Currency"), currency);
    }

    [Fact]
    public async Task Convert_ValidRequest_ReturnsConvertedAmount()
    {
        // mock exchange rate provider
        // we have to mock the GetCurrencies method as well because the check is done by the endpoint that the currency codes are valid from the API's list
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Currency> { new("GBP", "Great British Pound"), new("USD", "United States Dollar") });
        provider.GetRateAsync("GBP", "USD", Arg.Any<CancellationToken>())
            .Returns(new ExchangeRate("GBP", "USD", 1.5m, new DateOnly(2026, 11, 3)));
        await using var factory = CreateFactory(provider);

        // http client
        var client = factory.CreateClient();

        var response = await client.GetFromJsonAsync<ConversionResponse>("/api/convert?from=GBP&to=USD&amount=100");
        
        Assert.Equal(new ConversionResponse("GBP", "USD", 100, 1.5m, 150, new DateOnly(2026, 11, 3)), response);
    }

    [Fact]
    public async Task Convert_ValidRequest_SucceedsOnLowercaseCodes()
    {
        // mock exchange rate provider
        // we have to mock the GetCurrencies method as well because the check is done by the endpoint that the currency codes are valid from the API's list
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Currency> { new("GBP", "Great British Pound"), new("USD", "United States Dollar") });
        provider.GetRateAsync("GBP", "USD", Arg.Any<CancellationToken>())
            .Returns(new ExchangeRate("GBP", "USD", 1.5m, new DateOnly(2026, 11, 3)));
        await using var factory = CreateFactory(provider);

        // http client
        var client = factory.CreateClient();

        var response = await client.GetFromJsonAsync<ConversionResponse>("/api/convert?from=gbp&to=usd&amount=100");
        
        Assert.Equal(new ConversionResponse("GBP", "USD", 100, 1.5m, 150, new DateOnly(2026, 11, 3)), response);
    }

    [Fact]
    public async Task Convert_InvalidRequest_FailsOnNegativeAmounts()
    {
        // mock exchange rate provider
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Currency> { new("GBP", "Great British Pound"), new("USD", "United States Dollar") });
        await using var factory = CreateFactory(provider);

        // http client
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/convert?from=GBP&to=USD&amount=-100");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Convert_InvalidRequest_FailsOnInvalidCode()
    {
        // mock exchange rate provider
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Currency> { new("GBP", "Great British Pound"), new("USD", "United States Dollar") });
        await using var factory = CreateFactory(provider);

        // http client
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/convert?from=XYZ&to=USD&amount=100");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Currencies_ProviderDown_Returns503()
    {
        // mock exchange rate provider
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new ExchangeRateUnavailableException("Provider down"));
        await using var factory = CreateFactory(provider);

        // http client
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/currencies");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Convert_ProviderDown_Returns503()
    {
        // mock exchange rate provider
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetCurrenciesAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Currency> { new("GBP", "Great British Pound"), new("USD", "United States Dollar") });
        provider.GetRateAsync("GBP", "USD", Arg.Any<CancellationToken>())
            .ThrowsAsync(new ExchangeRateUnavailableException("Provider down"));
        await using var factory = CreateFactory(provider);

        // http client
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/convert?from=GBP&to=USD&amount=100");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

}