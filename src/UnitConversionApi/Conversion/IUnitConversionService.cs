using UnitConversionApi.Common;

namespace UnitConversionApi.Conversion;

public interface IUnitConversionService
{
    Result<ConversionResponse> Convert(ConversionRequest? request);
}
