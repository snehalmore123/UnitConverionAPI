using UnitConversionApi.Common;

namespace UnitConversionApi.Conversion;

public sealed class UnitConversionService : IUnitConversionService
{
    private readonly UnitRegistry _registry;

    public UnitConversionService(UnitRegistry registry) => _registry = registry;

    public Result<ConversionResponse> Convert(ConversionRequest? request)
    {
        if (request is null)
        {
            return Result.Failure<ConversionResponse>(ConversionErrors.InvalidRequest("A request body is required."));
        }

        if (string.IsNullOrWhiteSpace(request.FromUnit) || string.IsNullOrWhiteSpace(request.ToUnit))
        {
            return Result.Failure<ConversionResponse>(ConversionErrors.InvalidRequest("Both 'fromUnit' and 'toUnit' are required."));
        }

        if (request.Value is null)
        {
            return Result.Failure<ConversionResponse>(ConversionErrors.InvalidRequest("'value' is required."));
        }

        var value = request.Value.Value;
        if (!double.IsFinite(value))
        {
            return Result.Failure<ConversionResponse>(ConversionErrors.InvalidValue());
        }

        if (!_registry.TryGet(request.FromUnit, out var from))
        {
            return Result.Failure<ConversionResponse>(ConversionErrors.UnsupportedUnit(request.FromUnit));
        }

        if (!_registry.TryGet(request.ToUnit, out var to))
        {
            return Result.Failure<ConversionResponse>(ConversionErrors.UnsupportedUnit(request.ToUnit));
        }

        if (from.Category != to.Category)
        {
            return Result.Failure<ConversionResponse>(ConversionErrors.IncompatibleUnits(from.Name, to.Name));
        }

        var converted = from == to ? value : to.FromBase(from.ToBase(value));
        if (!double.IsFinite(converted))
        {
            return Result.Failure<ConversionResponse>(ConversionErrors.InvalidValue());
        }

        return Result.Success(new ConversionResponse(value, from.Name, to.Name, converted));
    }
}
