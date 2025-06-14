using GraduationProject.API.Responses;
using GraduationProject.API.Responses.ActionResult;
using GraduationProject.Application.Services;
using GraduationProject.Application.Services.HttpClientServices;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Common.Middlewares;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var vars = DotNetEnv.Env.Load();

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddDbContextPool<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("POSTGRES_GRAD_ONLINE"))
               .EnableServiceProviderCaching());

builder.Services
    .AddIdentity<AppUser, Role>(options =>
    {
        options.User.RequireUniqueEmail = true;

        // Working
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddHttpClient<IGrammarCorrectionService, GrammarCorrectionService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8001/");
});

builder.Services.AddHttpClient<IVqaService, VqaService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8000/");
});

builder.Services.AddHttpClient<ITextToSpeechService, TextToSpeechService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8002/");
});

builder.Services.AddHttpClient<IMediaValidator, MediaValidator>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "MediaValidator");
    client.Timeout = TimeSpan.FromSeconds(10); // Note: Increase timeout for cloud services
});

var vqaSection = builder.Configuration.GetSection("VQA");
var jwtSection = builder.Configuration.GetSection("Jwt");
var mailingSection = builder.Configuration.GetSection("Authentication").GetSection("Mailing");

if (!jwtSection.Exists() || !vqaSection.Exists() || !mailingSection.Exists())
{
    throw new InvalidOperationException("Configuration section is missing.");
}

builder.Services.Configure<ApiKeyOption>(vqaSection);
builder.Services.Configure<JwtOptions>(jwtSection);
builder.Services.Configure<MailingOptions>(mailingSection);


builder.Services.AddMemoryCache();

builder.Services.AddDependencies(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errorMessage = context.ModelState.Aggregate("", (accumlator, modelEntry) =>
        {
            if (modelEntry.Value is null)
                return accumlator;
            foreach (var item in modelEntry.Value.Errors)
            {
                accumlator += $"{modelEntry.Key}: {item.ErrorMessage}\n";
            }
            return accumlator;
        }).TrimEnd('\n');

        return new BadRequestObjectResult(new ErrorResponse()
        {
            Code = System.Net.HttpStatusCode.BadRequest,
            Message = errorMessage,
        });
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .WithOrigins("https://localhost:4200")
                .WithOrigins("https://localhost:443")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});


builder.Services.AddRateLimiting();

builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "wwwroot";
});

var app = builder.Build();

await AppDbSeeder.SeedAsync(app.Services);

app.UseMiddleware<TokenBlacklistMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();     // Serve index.html by default
app.UseStaticFiles();      // Serve static files from wwwroot
app.UseRouting();

app.UseCors("AllowSpecificOrigin");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html"); // Required for Angular routing


app.UseSpa(spa =>
{
    spa.Options.SourcePath = "wwwroot";
});

app.Run();


