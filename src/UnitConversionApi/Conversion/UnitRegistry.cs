namespace UnitConversionApi.Conversion;

/// <summary>
/// Catalogue of supported units. To add a unit, add a definition (and aliases) in <see cref="Build"/>.
/// To add a category, extend <see cref="UnitCategory"/> and register its units.
/// Base units: meter (length), kelvin (temperature), kilogram (mass).
/// </summary>
public sealed class UnitRegistry
{
    private readonly Dictionary<string, UnitDefinition> _units;

    public UnitRegistry() => _units = Build();

    public bool TryGet(string name, out UnitDefinition unit) => _units.TryGetValue(name.Trim(), out unit!);

    private static Dictionary<string, UnitDefinition> Build()
    {
        var units = new Dictionary<string, UnitDefinition>(StringComparer.OrdinalIgnoreCase);

        void Add(UnitDefinition definition, params string[] aliases)
        {
            units[definition.Name] = definition;
            foreach (var alias in aliases)
            {
                units[alias] = definition;
            }
        }

        Add(UnitDefinition.Linear("meter", UnitCategory.Length, 1), "meters", "m");
        Add(UnitDefinition.Linear("feet", UnitCategory.Length, 0.3048), "foot", "ft");
        Add(UnitDefinition.Linear("kilometer", UnitCategory.Length, 1000), "kilometers", "km");
        Add(UnitDefinition.Linear("mile", UnitCategory.Length, 1609.344), "miles", "mi");

        Add(UnitDefinition.Linear("kilogram", UnitCategory.Mass, 1), "kilograms", "kg");
        Add(UnitDefinition.Linear("pound", UnitCategory.Mass, 0.45359237), "pounds", "lb", "lbs");
        Add(UnitDefinition.Linear("gram", UnitCategory.Mass, 0.001), "grams", "g");

        Add(new UnitDefinition("celsius", UnitCategory.Temperature, c => c + 273.15, k => k - 273.15), "c");
        Add(new UnitDefinition("fahrenheit", UnitCategory.Temperature, f => (f - 32) * 5 / 9 + 273.15, k => (k - 273.15) * 9 / 5 + 32), "f");
        Add(new UnitDefinition("kelvin", UnitCategory.Temperature, k => k, k => k), "k");

        return units;
    }
}
