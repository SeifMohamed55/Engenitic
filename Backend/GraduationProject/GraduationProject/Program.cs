using GraduationProject.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Services;
using GraduationProject.Repositories;
using GraduationProject.Middlewares;
using GraduationProject;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddEnvironmentVariables();


builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("POSTGRES_GRAD")));

builder.Services
    .AddIdentity<AppUser, Role>(options =>
    {
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 5;
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<Role>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

builder.Services.AddScoped<IUserRepository, AppUsersRepository>();
builder.Services.AddScoped<ILoginRegisterService, LoginRegisterService>();


builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<TokenBlacklistMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

/*using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    *//*AppDbSeeder.Seed(dbContext);*//*
    var quiz1InCourse1 = await dbContext.Quizzes
        .Include(x => x.Questions)
            .ThenInclude(q => q.Answers)
        .Where(q => q.CourseId == 1 && q.Position == 1) 
        .Select(x=> new
        {
            x.Id,
            x.Title,
            x.Position,
            Questions = x.Questions
            .Select(q => new
            {
                q.Id,
                q.QuestionText,
                Answers = q.Answers.Select(a => new
                {
                    a.Id,
                    a.AnswerText,
                    a.IsCorrect
                })
            })
        }).FirstOrDefaultAsync();

}*/


app.Run();


