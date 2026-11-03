# Currency Exchange
This is a currency exchange web app made using C# with an ASP.NET Core Web API and a Blazor WebAssembly frontend, hosted on Azure. ECB reference rate data is provided by the free open-source Frankfurter API.

## Live URLs
**Website**<br>
https://wonderful-bay-0a6624903.6.azurestaticapps.net

**API**<br>
https://currency-exchange-api-lucas-bbh4fhcyh9cpaxbb.ukwest-01.azurewebsites.net<br>
This can be hit directly with /api/currencies and /api/convert, e.g.:<br>
https://currency-exchange-api-lucas-bbh4fhcyh9cpaxbb.ukwest-01.azurewebsites.net/api/convert?from=GBP&to=USD&amount=100


## Run Locally
### Prerequisites
.NET 10 SDK
### Commands
First, clone the repository.
```bash
git clone https://github.com/lucashutton/currency-exchange.git
cd currency-exchange
```

Then, run the API and frontend on separate terminals.

**Terminal 1 – API** (http://localhost:5115):<br>
```bash
dotnet run --project src/CurrencyExchange.Api --launch-profile http
```

**Terminal 2 – frontend** (http://localhost:5098):<br>
```bash
dotnet run --project src/CurrencyExchange.Web --launch-profile http
```

Then open at http://localhost:5098.
(Note: `http` profile is used to prevent mixed content calls to the API being blocked; the API only allows the frontend's local origins through CORS)

**Testing**<br>
All tests can be run with
```bash
dotnet test
```

## Architecture
The app is built across five projects: 
- Core (domain logic and interfaces)
- Infrastructure (Frankfurter client)
- Api (endpoints)
- Shared (DTOs used by API and frontend)
- Web (Blazor UI)

A conversion request flows: page -> `CurrencyApiClient` -> API endpoint -> `CurrencyConverter` -> `IExchangeRateProvider` -> Frankfurter.

Core has no dependencies to allow for testing with fakes and the rate source to be swapped.

## Design Decisions
- `decimal` is used for money and rates to avoid rounding errors.
- Converted amounts are rounded to 2dp with `MidpointRounding.AwayFromZero`.
- Currency codes are validated against the provider's supported list. Any non-positive amount is rejected, and both return HTTP 400.
- The currency list is cached for 24 hours, rates are fetched live on every conversion.
- On a fail or timeout from Frankfurter, the API returns HTTP 503 with a dedicated message.
- CORS origins come from configuration; production is set through an App Service setting.

## Testing & Deployment / CI
The app includes unit testing for the `CurrencyConverter` using NSubstitute fakes. Additional integration tests use `WebApplicationFactory` to host the real API in memory. `IExchangeRateProvider` is replaced by an NSubstitute fake so that no tests call Frankfurter.

The app is deployed with an App Service and Static Web App, with the workflow gating deployment on the tests. GitHub Actions run on every push to `master`.

## AI Usage
See [AI_USAGE.md](AI_USAGE.md) for where and how AI was used.