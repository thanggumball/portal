using Audit.Core;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using StudentPortal.API.Extensions;
using StudentPortal.API.Middlewares;
using StudentPortal.Common.Constants;
using StudentPortal.Common.DTOs.Shared;
using StudentPortal.Repository.Interfaces;
using StudentPortal.Repository.Implementations;
using StudentPortal.Service;
using StudentPortal.Service.Implementations;
using StudentPortal.Service.Interfaces;
using StudentPortal.Service.Validations.Auth;
using System.Security.Claims;


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


builder.Services.AddHttpClient<IMailServiceClient, MailServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["MailService:BaseUrl"]!);
    });

var connectionString = builder.Configuration.GetConnectionString(
    "DefaultConnection")
    ?? throw new InvalidOperationException(
        "Missing ConnectionStrings:DefaultConnection in appsettings.");

builder.Services.AddServiceLayer(connectionString);

// The audit custom action below needs it to know who is making the change
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// IHttpContextAccessor is a SINGLETON that always resolves the current request, so it is
// safe to capture here. NEVER capture a scoped service instead: the custom action lives
// for the whole application lifetime and would pin the very first request's user forever.
var httpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();

Audit.Core.Configuration.AddCustomAction(ActionType.OnScopeCreated, scope =>
{
    var httpContext = httpContextAccessor.HttpContext;
    if (httpContext is null)
    {
        return;   // a change made outside an HTTP request (background job...): no user to record
    }

    var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? httpContext.User.FindFirstValue("sub");

    scope.SetCustomField(AuditFields.UserId, userId);
    scope.SetCustomField(AuditFields.IpAddress, httpContext.Connection.RemoteIpAddress?.ToString());
});

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
