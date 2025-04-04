using GraduationProject.Application.Services;
using GraduationProject.Common.Middlewares;
using GraduationProject.Domain.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddDbContextPool<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("POSTGRES_GRAD_ONLINE"))
               .EnableServiceProviderCaching());

builder.Services
    .AddIdentity<AppUser, Role>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 5;

        options.User.RequireUniqueEmail = true;

        // Working
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

/*builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});*/

builder.Services.AddHttpClient<IVqaService, VqaService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8000/"); // Python FastAPI server
});



var vqaSection = builder.Configuration.GetSection("VQA");
var jwtSection = builder.Configuration.GetSection("Jwt");
var mailingSection = builder.Configuration.GetSection("Authentication").GetSection("Mailing");

if (!jwtSection.Exists() || !vqaSection.Exists() || !mailingSection.Exists())
{
    throw new InvalidOperationException("Configuration section is missing.");
}

builder.Services.Configure<VqaApiKeyOption>(vqaSection);
builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.Configure<MailingOptions>(mailingSection);


builder.Services.AddMemoryCache();

builder.Services.AddDependencies(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});



builder.Services.AddRateLimiting(); // Add Rate Limiting Configuration

var app = builder.Build();



app.UseMiddleware<TokenBlacklistMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CheckingRefreshTokenAfterAuthorizationMiddleware>();

app.MapControllers();

app.Run();



