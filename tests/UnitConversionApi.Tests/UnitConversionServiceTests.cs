using UnitConversionApi.Conversion;

namespace UnitConversionApi.Tests;

public class UnitConversionServiceTests
{
    private readonly UnitConversionService _service = new(new UnitRegistry());

    [Theory]
    [InlineData(10, "meter", "feet", 32.8084, 4)]
    [InlineData(1, "kilometer", "meters", 1000, 9)]
    [InlineData(1, "mile", "kilometer", 1.609344, 9)]
    [InlineData(1, "feet", "meter", 0.3048, 9)]
    [InlineData(100, "celsius", "fahrenheit", 212, 9)]
    [InlineData(32, "Fahrenheit", "Celsius", 0, 9)]
    [InlineData(0, "celsius", "kelvin", 273.15, 9)]
    [InlineData(-40, "celsius", "fahrenheit", -40, 9)]
    [InlineData(-273.15, "celsius", "kelvin", 0, 9)]
    [InlineData(1, "kilogram", "pounds", 2.2046226, 6)]
    [InlineData(1500, "grams", "kilograms", 1.5, 9)]
    [InlineData(0, "meter", "mile", 0, 9)]
    [InlineData(-5, "meter", "feet", -16.40420, 5)]
    public void Convert_ValidRequests_ReturnsExpected(double value, string from, string to, double expected, int precision)
    {
        var result = _service.Convert(new ConversionRequest(value, from, to));
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Result, precision);
        Assert.Equal(value, result.Value.InputValue);
    }

    [Theory]
    [InlineData("meter")]
    [InlineData("celsius")]
    [InlineData("gram")]
    public void Convert_Identity_ReturnsSameValue(string unit)
    {
        var result = _service.Convert(new ConversionRequest(123.456789012345, unit, unit));
        Assert.True(result.IsSuccess);
        Assert.Equal(123.456789012345, result.Value.Result);
    }

    [Fact]
    public void Convert_UnsupportedUnit_Fails()
    {
        Assert.Equal("Conversion.UnsupportedUnit", _service.Convert(new ConversionRequest(1, "parsec", "meter")).Error.Code);
        Assert.Equal("Conversion.UnsupportedUnit", _service.Convert(new ConversionRequest(1, "meter", "parsec")).Error.Code);
    }

    [Theory]
    [InlineData("meter", "celsius")]
    [InlineData("kilogram", "feet")]
    [InlineData("kelvin", "gram")]
    public void Convert_IncompatibleUnits_Fails(string from, string to)
    {
        var result = _service.Convert(new ConversionRequest(1, from, to));
        Assert.True(result.IsFailure);
        Assert.Equal("Conversion.IncompatibleUnits", result.Error.Code);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Convert_NonFiniteValue_Fails(double value) =>
        Assert.Equal("Conversion.InvalidValue", _service.Convert(new ConversionRequest(value, "meter", "feet")).Error.Code);

    [Fact]
    public void Convert_OverflowingResult_Fails() =>
        Assert.Equal("Conversion.InvalidValue", _service.Convert(new ConversionRequest(double.MaxValue, "kilometer", "meter")).Error.Code);

    [Fact]
    public void Convert_InvalidRequests_Fail()
    {
        Assert.Equal("Conversion.InvalidRequest", _service.Convert(null).Error.Code);
        Assert.Equal("Conversion.InvalidRequest", _service.Convert(new ConversionRequest(null, "meter", "feet")).Error.Code);
        Assert.Equal("Conversion.InvalidRequest", _service.Convert(new ConversionRequest(1, null, "feet")).Error.Code);
        Assert.Equal("Conversion.InvalidRequest", _service.Convert(new ConversionRequest(1, "meter", " ")).Error.Code);
    }
}
