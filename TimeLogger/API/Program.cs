using System.Text;
using SkillAllocationTracker.Infrastructure.DbContexts;
using SkillAllocationTracker.Infrastructure;
using SkillAllocationTracker.Application.Services;
using SkillAllocationTracker.Application.Validators;
using SkillAllocationTracker.API.Middleware;
using Serilog;
using Microsoft.EntityFrameworkCore;
using SkillAllocationTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using FluentValidation.AspNetCore;
using FluentValidation;
using SkillAllocationTracker.API;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=SkillAlocDb;Trusted_Connection=True;TrustServerCertificate=True;";

// EF Core
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));

// UnitOfWork / Repositories / Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ITopicService, TopicService>();

// Authentication - JWT (symmetric)
var jwtKey = configuration["Jwt:Key"] ?? "REPLACE_WITH_A_STRONG_KEY";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

// Enable MVC views and Razor Pages
builder.Services.AddControllersWithViews().AddJsonOptions(opts => { opts.JsonSerializerOptions.PropertyNamingPolicy = null; });
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<TopicDtoValidator>();

var app = builder.Build();

// Ensure DB created and seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await SeedData.EnsureSeedData(scope.ServiceProvider);
}

app.UseSerilogRequestLogging();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// API controllers (attribute routed)
app.MapControllers();

// Default MVC route — default controller is Topics (TopicsController)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Topics}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();