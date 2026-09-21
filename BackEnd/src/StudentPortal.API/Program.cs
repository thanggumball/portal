using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Extensions;
using StudentPortal.API.Middlewares;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Service;
using StudentPortal.Service.Validations.Auth;

var builder = WebApplication.CreateBuilder(args);

// log4net.config uses %property{LogRoot} to write logs into Logs/ at the content root,
// because log4net resolves relative paths against AppDomain.BaseDirectory (the build
// output folder) by default, not wherever `dotnet run` is invoked from.
log4net.GlobalContext.Properties["LogRoot"] = builder.Environment.ContentRootPath;

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
app.UseMiddleware<ExceptionHandlingMiddleware>();   // outermost
app.UseMiddleware<RequestLoggingMiddleware>();

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
