# AI Usage

I used **Claude Code** as a tutor and reviewer throughout. I was new to C#, ASP.NET Core, Blazor and Azure, so most of my use was asking it to explain concepts (relating them to Java/Kotlin), suggest structure, and review what I'd written.

**How I worked with it**
- It did not edit any project files; I typed all of the code in this repository myself.
- Where it gave me code, I've listed it below. Everything else, I wrote, with it reviewing and pointing out errors.
- I asked it to explain every snippet before I used it.
- It also checked its own suggestions by building and running copies of the project in a scratch folder.

## By area

| Area | Provided by AI | Written by me |
|---|---|---|
| **Setup** | The list of `dotnet` commands to create the solution, projects and references; an explanation of the structure | Ran the commands; chose the naming |
| **Core models and interfaces** | Explained C# record and interface syntax  | Created the records (`Currency`, `ExchangeRate`, `ConversionResult`) and interfaces (`IExchangeRateProvider`, `ICurrencyConverter`), and `ExchangeRateUnavailableException` |
| **Conversion logic** (`CurrencyConverter`) | Reviewed my code | The `ConvertAsync` implementation: validation, the same-currency shortcut, calling the provider |
| **Core unit tests** | Explained xUnit/NSubstitute syntax | Created all tests |
| **Frankfurter provider** | Explained `HttpClient` and JSON deserialisation; gave the two `catch` blocks that map HTTP failures and timeouts to `ExchangeRateUnavailableException`; explained `IMemoryCache` | The provider implementation, the response record and the caching. **Decisions:** use Frankfurter v1; cache only the currency list (24 h), not rates |
| **API wiring** (`Program.cs`) | The DI, options-pattern, `HttpClient` timeout and CORS configuration code | The config values in `appsettings` |
| **Endpoints** | The skeleton (`MapCurrencyEndpoints` with two empty handlers); explained minimal APIs, DTOs and ProblemDetails | Handler bodies, DTO mapping and error responses. **Decisions:** validate codes against the provider's list instead of a format check; reuse Core's amount validation rather than repeating it. |
| **API integration tests** | Explained `WebApplicationFactory`; gave the helper that swaps in a fake provider | The test cases |
| **`.http` file** | Explained the format | The requests |
| **Blazor API client** (`CurrencyApiClient`) | The class outline (two public methods, one private generic helper), the `InvariantCulture` advice, and the `Program.cs` config/registration lines | The implementation and `ApiException` |
| **Converter page** (`Home.razor`) | The HTML/Bootstrap markup and the one-line scoped CSS | The `@code` logic: loading currencies, validation, swap, conversion, loading and error state. |
| **Azure and CI/CD** | Step-by-step portal instructions; the GitHub Actions workflow YAML | Created the Azure resources, secrets and settings; set the production config; debugged the deployment |