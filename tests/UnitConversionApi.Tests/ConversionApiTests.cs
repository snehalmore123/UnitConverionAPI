using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using UnitConversionApi.Common;
using UnitConversionApi.Conversion;

namespace UnitConversionApi.Tests;

public class ConversionApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ConversionApiTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private static async Task<JsonElement> ReadJson(HttpResponseMessage response) =>
        JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

    [Fact]
    public async Task Post_ValidRequest_Returns200WithResult()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/conversions",
            new { value = 100, fromUnit = "celsius", toUnit = "fahrenheit" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJson(response);
        Assert.Equal(212, json.GetProperty("result").GetDouble(), 9);
        Assert.Equal("celsius", json.GetProperty("fromUnit").GetString());
    }

    [Theory]
    [InlineData("parsec", "meter", "Conversion.UnsupportedUnit")]
    [InlineData("meter", "celsius", "Conversion.IncompatibleUnits")]
    public async Task Post_BusinessFailure_Returns400ProblemDetailsWithCode(string from, string to, string code)
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/conversions",
            new { value = 1, fromUnit = from, toUnit = to });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var json = await ReadJson(response);
        Assert.Equal(400, json.GetProperty("status").GetInt32());
        Assert.Equal(code, json.GetProperty("errorCode").GetString());
        Assert.False(string.IsNullOrEmpty(json.GetProperty("detail").GetString()));
    }

    [Fact]
    public async Task Post_MissingValue_Returns400InvalidRequest()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/conversions",
            new { fromUnit = "meter", toUnit = "feet" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Conversion.InvalidRequest", (await ReadJson(response)).GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Post_MalformedJson_Returns400InvalidRequest()
    {
        var content = new StringContent("{ not json", Encoding.UTF8, "application/json");
        var response = await _factory.CreateClient().PostAsync("/api/conversions", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Conversion.InvalidRequest", (await ReadJson(response)).GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Post_NonNumericValue_Returns400InvalidRequest()
    {
        var content = new StringContent("""{"value":"abc","fromUnit":"meter","toUnit":"feet"}""", Encoding.UTF8, "application/json");
        var response = await _factory.CreateClient().PostAsync("/api/conversions", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Conversion.InvalidRequest", (await ReadJson(response)).GetProperty("errorCode").GetString());
    }

    [Theory]
    [InlineData(ErrorType.NotFound, HttpStatusCode.NotFound)]
    [InlineData(ErrorType.Conflict, HttpStatusCode.Conflict)]
    [InlineData(ErrorType.Failure, HttpStatusCode.InternalServerError)]
    public async Task Post_ErrorCategories_MapToStatusCodes(ErrorType type, HttpStatusCode expected)
    {
        var client = _factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            s.AddSingleton<IUnitConversionService>(new StubService(Result.Failure<ConversionResponse>(new Error("Test.Code", "msg", type))))))
            .CreateClient();

        var response = await client.PostAsJsonAsync("/api/conversions", new { value = 1, fromUnit = "m", toUnit = "ft" });

        Assert.Equal(expected, response.StatusCode);
        Assert.Equal("Test.Code", (await ReadJson(response)).GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task Post_UnexpectedException_Returns500WithoutInternalDetails()
    {
        var client = _factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            s.AddSingleton<IUnitConversionService>(new ThrowingService()))).CreateClient();

        var response = await client.PostAsJsonAsync("/api/conversions", new { value = 1, fromUnit = "m", toUnit = "ft" });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("SECRET-INTERNAL-DETAIL", body);
        Assert.DoesNotContain("ThrowingService", body);
        Assert.DoesNotContain("StackTrace", body, StringComparison.OrdinalIgnoreCase);
        var json = JsonDocument.Parse(body).RootElement;
        Assert.Equal("Server.UnexpectedError", json.GetProperty("errorCode").GetString());
        Assert.Equal(500, json.GetProperty("status").GetInt32());
    }

    private sealed class StubService(Result<ConversionResponse> result) : IUnitConversionService
    {
        public Result<ConversionResponse> Convert(ConversionRequest? request) => result;
    }

    private sealed class ThrowingService : IUnitConversionService
    {
        public Result<ConversionResponse> Convert(ConversionRequest? request) =>
            throw new InvalidOperationException("SECRET-INTERNAL-DETAIL");
    }
}
