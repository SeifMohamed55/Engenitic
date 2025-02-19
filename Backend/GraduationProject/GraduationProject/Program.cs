using GraduationProject.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Services;
using GraduationProject.Repositories;
using GraduationProject.Middlewares;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddDbContextPool<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("POSTGRES_GRAD"))
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

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});



builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>();

builder.Services.AddScoped<IUserRepository, AppUsersRepository>();
builder.Services.AddScoped<ILoginRegisterService, LoginRegisterService>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<ICourseRepository, CoursesRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();


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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<CheckingRefreshTokenAfterAuthorizationMiddleware>();

app.MapControllers();

app.Run();




/*using (var scope = app.Services.CreateScope())
{

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var QuizRepo = scope.ServiceProvider.GetRequiredService<IQuizRepository>();
    //AppDbSeeder.Seed(dbContext);

    dbContext.Roles.Add(new Role() { Name = "Superadmin"});
    await dbContext.SaveChangesAsync();
    *//*
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var quiz = await QuizRepo.GetQuizByCourseAndPosition(1, 1);

        stopwatch.Stop();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
        stopwatch.Restart();

        var quiz2 = await QuizRepo.GetQuizByCourseAndPosition(1, 1);

        stopwatch.Stop();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");

        var quiz3 = await dbContext.Quizzes
                    .Include(x => x.Questions)
                        .ThenInclude(q => q.Answers)
                    .Where(q => q.CourseId == 1 && q.Position == 1)
                    .Select(q => new QuizDTO()
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Position = q.Position,
                        Questions = q.Questions.OrderBy(x => x.Position).Select(qq => new QuestionDTO()
                        {
                            Id = qq.Id,
                            QuestionText = qq.QuestionText,
                            Position = qq.Position,
                            Answers = qq.Answers.OrderBy(x => x.Position).Select(a => new AnswerDTO()
                            {
                                Id = a.Id,
                                AnswerText = a.AnswerText,
                                IsCorrect = a.IsCorrect,
                                Position = a.Position
                            }).ToList()
                        }).ToList()
                    })
                    .AsSingleQuery()
                    .FirstOrDefaultAsync();

        stopwatch.Stop();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
        stopwatch.Restart();
}*/


