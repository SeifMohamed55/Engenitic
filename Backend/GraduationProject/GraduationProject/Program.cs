using GraduationProject.Models;
using GraduationProject.StartupConfigurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GraduationProject.Services;
using GraduationProject.Repositories;
using GraduationProject.Middlewares;
using GraduationProject;
using GraduationProject.Models.DTOs;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Configuration.AddEnvironmentVariables();


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
builder.Services.AddScoped<IQuizRepository, QuizRepository>();


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
        stopwatch.Restart();*//*
}*/


app.Run();


