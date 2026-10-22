namespace UnitConversionApi.Conversion;

public sealed record ConversionRequest(double? Value, string? FromUnit, string? ToUnit);

public sealed record ConversionResponse(double InputValue, string FromUnit, string ToUnit, double Result);
