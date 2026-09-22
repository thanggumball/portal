using MailService.API.Data;
using MailService.API.Repositories;
using MailService.API.Services;
using MailServiceImplementation = MailService.API.Services.MailService;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<MailDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Mail services
builder.Services.AddScoped<IMailService, MailServiceImplementation>();
builder.Services.AddScoped<IMailRepository, MailRepository>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentMailUser, CurrentMailUser>();

// Mail account services
builder.Services.AddScoped<
    IMailAccountRepository,
    MailAccountRepository>();

builder.Services.AddScoped<
    IMailAccountService,
    MailAccountService>();

// Cookie Authentication
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "mailservice_auth";
        options.LoginPath = "/api/mail-account/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

