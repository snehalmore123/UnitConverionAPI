namespace UnitConversionApi.Common;

/// <summary>Category of an error, used by the API layer to choose an HTTP status code.</summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure
}
