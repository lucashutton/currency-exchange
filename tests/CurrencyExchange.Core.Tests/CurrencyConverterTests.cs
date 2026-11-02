using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace CurrencyExchange.Core.Tests;

public class CurrencyConverterTests
{
    [Fact]
    public async Task ConvertAsync_ValidInput_ReturnsConvertedAmount()
    {
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetRateAsync("GBP", "USD", Arg.Any<CancellationToken>())
            .Returns(new ExchangeRate("GBP", "USD", 1.34m, new DateOnly(2026, 11, 2)));

        var converter = new CurrencyConverter(provider);

        var results = await converter.ConvertAsync("GBP", "USD", 100m);

        Assert.Equal(134m, results.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertAsync_ValidInput_ReturnsRoundedAmount()
    {
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetRateAsync("GBP", "USD", Arg.Any<CancellationToken>())
            .Returns(new ExchangeRate("GBP", "USD", 1.125m, new DateOnly(2026, 11, 2)));

        var converter = new CurrencyConverter(provider);

        var results = await converter.ConvertAsync("GBP", "USD", 1m);

        Assert.Equal(1.13m, results.ConvertedAmount);
    }

    [Fact]
    public async Task ConvertAsync_SameCurrency_ProviderNeverCalled()
    {
        var provider = Substitute.For<IExchangeRateProvider>();
        var converter = new CurrencyConverter(provider);

        var results = await converter.ConvertAsync("GBP", "GBP", 100m);

        await provider.DidNotReceive().GetRateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task ConvertAsync_InvalidInput_ThrowsError(decimal amount)
    {
        var provider = Substitute.For<IExchangeRateProvider>();
        var converter = new CurrencyConverter(provider);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => converter.ConvertAsync("GBP", "USD", amount));
    }

    [Fact]
    public async Task ConvertAsync_ProviderFails_PassesExceptionToCaller()
    {
        var provider = Substitute.For<IExchangeRateProvider>();
        provider.GetRateAsync("GBP", "USD", Arg.Any<CancellationToken>()).ThrowsAsync(new ExchangeRateUnavailableException("Rates are unavailable."));

        var converter = new CurrencyConverter(provider);

        await Assert.ThrowsAsync<ExchangeRateUnavailableException>(() => converter.ConvertAsync("GBP", "USD", 100m));
    }
}