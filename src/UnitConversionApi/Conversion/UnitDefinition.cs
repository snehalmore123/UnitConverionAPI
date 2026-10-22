namespace UnitConversionApi.Conversion;

/// <summary>A unit with functions converting to and from its category's base unit.</summary>
public sealed record UnitDefinition(
    string Name,
    UnitCategory Category,
    Func<double, double> ToBase,
    Func<double, double> FromBase)
{
    /// <summary>Creates a unit related to the base unit by a simple multiplicative factor.</summary>
    public static UnitDefinition Linear(string name, UnitCategory category, double factorToBase) =>
        new(name, category, v => v * factorToBase, v => v / factorToBase);
}
