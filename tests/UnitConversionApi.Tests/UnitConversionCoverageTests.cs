using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitConversionApi.Conversion;

namespace UnitConversionApi.Tests;

public class UnitConversionCoverageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private const double Tolerance = 1e-9;
    private readonly UnitConversionService _service = new(new UnitRegistry());
    private readonly WebApplicationFactory<Program> _factory;

    public UnitConversionCoverageTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Theory]
    [InlineData(1, "kilometer", "mile", 0.621371192237334)]
    [InlineData(1, "mile", "feet", 5280)]
    [InlineData(0, "kilometer", "mile", 0)]
    [InlineData(-10, "mile", "kilometer", -16.09344)]
    [InlineData(1, "pound", "kilogram", 0.45359237)]
    [InlineData(2, "kilogram", "gram", 2000)]
    [InlineData(-2.5, "kilogram", "gram", -2500)]
    [InlineData(0, "pound", "gram", 0)]
    [InlineData(273.15, "kelvin", "celsius", 0)]
    [InlineData(0, "kelvin", "celsius", -273.15)]
    [InlineData(-459.67, "fahrenheit", "kelvin", 0)]
    [InlineData(0, "kelvin", "fahrenheit", -459.67)]
    [InlineData(0, "celsius", "fahrenheit", 32)]
    [InlineData(-10, "celsius", "fahrenheit", 14)]
    public void Convert_RepresentativeValues_ReturnsExpectedWithinTolerance(double value, string from, string to, double expected)
    {
        var result = _service.Convert(new ConversionRequest(value, from, to));

        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.Result, expected - 1e-6, expected + 1e-6);
    }

    [Theory]
    [InlineData("meter")]
    [InlineData("feet")]
    [InlineData("kilometer")]
    [InlineData("mile")]
    [InlineData("celsius")]
    [InlineData("fahrenheit")]
    [InlineData("kelvin")]
    [InlineData("kilogram")]
    [InlineData("pound")]
    [InlineData("gram")]
    public void Convert_IdentityForEverySupportedUnit_ReturnsInputExactly(string unit)
    {
        var result = _service.Convert(new ConversionRequest(-12.3456789, unit, unit));

        Assert.True(result.IsSuccess);
        Assert.Equal(-12.3456789, result.Value.Result);
    }

    [Theory]
    [InlineData("meter", "feet")]
    [InlineData("kilometer", "mile")]
    [InlineData("celsius", "fahrenheit")]
    [InlineData("kelvin", "celsius")]
    [InlineData("kilogram", "pound")]
    [InlineData("gram", "kilogram")]
    public void Convert_RoundTrip_ReturnsOriginalWithinTolerance(string a, string b)
    {
        var forward = _service.Convert(new ConversionRequest(123.456, a, b));
        var back = _service.Convert(new ConversionRequest(forward.Value.Result, b, a));

        Assert.InRange(back.Value.Result, 123.456 - Tolerance * 1000, 123.456 + Tolerance * 1000);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("KM")]
    [InlineData("KG")]
    [InlineData(" meters ")]
    public void Convert_AliasesAreCaseAndWhitespaceInsensitive_Succeed(string unit)
    {
        Assert.True(_service.Convert(new ConversionRequest(1, unit, unit)).IsSuccess);
    }

    [Fact]
    public void Convert_BelowAbsoluteZero_IsAcceptedByCurrentContract()
    {
        // The documented API contract does not reject physically impossible temperatures.
        var result = _service.Convert(new ConversionRequest(-300, "celsius", "kelvin"));

        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.Result, -26.85 - Tolerance, -26.85 + Tolerance);
    }

    [Fact]
    public void Convert_UnsupportedUnit_ErrorMessageNamesUnit()
    {
        var result = _service.Convert(new ConversionRequest(1, "parsec", "meter"));

        Assert.Contains("parsec", result.Error.Message);
    }

    [Fact]
    public async Task Get_ConversionsRoute_ReturnsMethodNotAllowed()
    {
        var response = await _factory.CreateClient().GetAsync("/api/conversions");

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [Fact]
    public async Task Post_UnknownRoute_ReturnsNotFound()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/unknown", new { });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_UnsupportedMediaType_Returns415()
    {
        var content = new StringContent("value=1", Encoding.UTF8, "text/plain");
        var response = await _factory.CreateClient().PostAsync("/api/conversions", content);

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task Post_EmptyBody_Returns400()
    {
        var content = new StringContent("", Encoding.UTF8, "application/json");
        var response = await _factory.CreateClient().PostAsync("/api/conversions", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
