using UnitConversionApi.Common;

namespace UnitConversionApi.Conversion;

public static class ConversionErrors
{
    public const string UnsupportedUnitCode = "Conversion.UnsupportedUnit";
    public const string IncompatibleUnitsCode = "Conversion.IncompatibleUnits";
    public const string InvalidValueCode = "Conversion.InvalidValue";
    public const string InvalidRequestCode = "Conversion.InvalidRequest";

    public static Error UnsupportedUnit(string unit) =>
        Error.Validation(UnsupportedUnitCode, $"The unit '{unit}' is not supported.");

    public static Error IncompatibleUnits(string from, string to) =>
        Error.Validation(IncompatibleUnitsCode, $"Cannot convert from '{from}' to '{to}' because they measure different quantities.");

    public static Error InvalidValue() =>
        Error.Validation(InvalidValueCode, "The value must be a finite number.");

    public static Error InvalidRequest(string message) =>
        Error.Validation(InvalidRequestCode, message);
}
