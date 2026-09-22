using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Extensions;
using StudentPortal.API.Middlewares;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Service;
using StudentPortal.Service.Validations.Auth;

var builder = WebApplication.CreateBuilder(args);

//file log will appear next program.cs
log4net.GlobalContext.Properties["LogDir"] =
    Path.Combine(builder.Environment.ContentRootPath, "Logs");

builder.Logging.ClearProviders();
builder.Logging.AddLog4Net("log4net.config");

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // [ApiController] returns its own default 400 ProblemDetails before reaching the
        // action - it never goes through ExceptionHandlingMiddleware. Wrap it here so every
        // error response follows the ApiResponse<T> convention (see section 6).
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .SelectMany(entry =>
                    entry.Value!.Errors.Select(error => error.ErrorMessage))
                .ToList();

            var message = errors.Count > 0
                ? string.Join(" ", errors)
                : "The submitted data is invalid.";

            return new BadRequestObjectResult(
                ApiResponse<object?>.Fail(message));
        };
    });

builder.Services.AddSwaggerDocs();

builder.Services.AddJwtAuthentication(
    builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddCorsPolicy(
    builder.Configuration);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

var connectionString = builder.Configuration.GetConnectionString(
    "DefaultConnection")
    ?? throw new InvalidOperationException(
        "Missing ConnectionStrings:DefaultConnection in appsettings.");

builder.Services.AddServiceLayer(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();   // outermost


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCorsPolicy();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
