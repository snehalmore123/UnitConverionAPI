using Microsoft.AspNetCore.Mvc;
using UnitConversionApi.Api;
using UnitConversionApi.Conversion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    // Malformed bodies / binding failures use the same ProblemDetails + error code contract.
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = ResultExtensions.ToProblemDetails(
            ConversionErrors.InvalidRequestCode,
            "The request body is malformed or contains invalid values.",
            StatusCodes.Status400BadRequest,
            "Bad Request");
        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<UnitRegistry>();
builder.Services.AddScoped<IUnitConversionService, UnitConversionService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
