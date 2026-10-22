# UnitConverionAPI

An ASP.NET Core (.NET 8) Web API that converts values between units of length, temperature and mass. Expected failures are modelled with a Result Pattern and returned as RFC 7807 `ProblemDetails` with a stable application error code.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (`global.json` pins SDK 8.0.x)

## Setup, build, test, run

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/UnitConversionApi   # Swagger UI (Development): http://localhost:5169/swagger
```

## Project layout

- `src/UnitConversionApi/Common` – `Result`, `Result<T>`, `Error`, `ErrorType`
- `src/UnitConversionApi/Conversion` – HTTP-agnostic conversion service, unit registry, DTOs, error codes
- `src/UnitConversionApi/Api` – result → HTTP mapping and centralized exception handler
- `src/UnitConversionApi/Controllers` – thin controller
- `tests/UnitConversionApi.Tests` – xUnit unit and integration tests

## API

### `POST /api/conversions`

Request:

```json
{ "value": 10, "fromUnit": "meter", "toUnit": "feet" }
```

Response `200 OK`:

```json
{ "inputValue": 10, "fromUnit": "meter", "toUnit": "feet", "result": 32.80839895013123 }
```

Failure response (`400 Bad Request`, `application/problem+json`):

```json
{
  "title": "Bad Request",
  "status": 400,
  "detail": "Cannot convert from 'meter' to 'celsius' because they measure different quantities.",
  "errorCode": "Conversion.IncompatibleUnits"
}
```

## Supported units

Unit names are case-insensitive; the response returns the canonical name.

| Category    | Units (aliases)                                                                 |
|-------------|----------------------------------------------------------------------------------|
| Length      | meter (meters, m), feet (foot, ft), kilometer (kilometers, km), mile (miles, mi) |
| Temperature | celsius (c), fahrenheit (f), kelvin (k)                                          |
| Mass        | kilogram (kilograms, kg), pound (pounds, lb, lbs), gram (grams, g)               |

Results are not rounded; `double` is used throughout. Identity conversions return the input unchanged.

## Error handling

| HTTP | `errorCode`                      | Cause                                                           |
|------|----------------------------------|-----------------------------------------------------------------|
| 400  | `Conversion.InvalidRequest`      | Missing body/value/units, malformed JSON or non-numeric value   |
| 400  | `Conversion.InvalidValue`        | NaN/Infinity input, or a result that is not finite (overflow)   |
| 400  | `Conversion.UnsupportedUnit`     | Unknown unit                                                    |
| 400  | `Conversion.IncompatibleUnits`   | Units from different categories                                 |
| 500  | `Server.UnexpectedError`         | Unhandled exception (sanitized; details are only logged)        |

Error categories (`ErrorType`) map to HTTP statuses centrally in `ResultExtensions`: `Validation` → 400, `NotFound` → 404, `Conflict` → 409, `Failure` → 500. The conversion API currently only produces validation errors; the other mappings are ready for future endpoints. Unexpected exceptions are handled by `GlobalExceptionHandler`, which returns a generic 500 `ProblemDetails` and never exposes messages or stack traces.

## Result Pattern rationale

Unsupported units or incompatible categories are normal outcomes, not exceptional ones. `Result`/`Result<T>` make the possibility of failure explicit in service signatures, avoid `null` as a failure signal and avoid exceptions for control flow. Accessing `Result<T>.Value` on a failure (or `Error` on a success) throws `InvalidOperationException`, so a failure cannot be silently used as a value. Services stay free of HTTP concerns; only the API layer maps errors to status codes.

## Design trade-offs and extension

- `double` is fast and adequate for general conversions but is not exact decimal arithmetic; switch to `decimal` if exact base-10 results are needed.
- Conversions go through a base unit (meter, kilogram, kelvin), so N units need N definitions instead of N² formulas, at the cost of small floating point error.
- To add a unit: register a `UnitDefinition` (with aliases) in `UnitRegistry.Build`. To add a category: add a `UnitCategory` value and register its units.
- Errors are plain `ProblemDetails` with an `errorCode` extension rather than a `success/data/error` envelope, so successful responses stay simple DTOs.
