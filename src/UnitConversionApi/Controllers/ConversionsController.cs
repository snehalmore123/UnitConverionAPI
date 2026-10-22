using Microsoft.AspNetCore.Mvc;
using UnitConversionApi.Api;
using UnitConversionApi.Conversion;

namespace UnitConversionApi.Controllers;

[ApiController]
[Route("api/conversions")]
public sealed class ConversionsController : ControllerBase
{
    private readonly IUnitConversionService _service;

    public ConversionsController(IUnitConversionService service) => _service = service;

    [HttpPost]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public IActionResult Convert([FromBody] ConversionRequest? request) =>
        _service.Convert(request).ToActionResult();
}
