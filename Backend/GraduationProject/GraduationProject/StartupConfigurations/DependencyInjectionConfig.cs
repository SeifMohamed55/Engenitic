using CloudinaryDotNet;
using GraduationProject.Application.Services;
using GraduationProject.Infrastructure.Data;
using GraduationProject.Infrastructure.Data.Repositories;
using static GraduationProject.Application.Services.ICoursesService;

namespace GraduationProject.StartupConfigurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddSingleton<IHashingService, FileHashingService>();


            Cloudinary cloudinary = new Cloudinary(configuration["GraduationProject:CLOUDINARY_URL"]);
            cloudinary.Api.Secure = true;
            services.AddSingleton(cloudinary);
            services.AddSingleton<ICloudinaryService, CloudinaryService>();


            services.AddScoped<IGmailServiceHelper, GmailServiceHelper>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IUserRepository, UsersRepository>();
            services.AddScoped<IQuizRepository, QuizRepository>();
            services.AddScoped<ICourseRepository, CoursesRepository>();
            services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            services.AddScoped<IUserLoginRepository, UserLoginRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<ITagsRepository, TagsRepository>();
            services.AddScoped<IFileHashRepository, FileHashRepository>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            services.AddScoped<IUploadingService, UploadingService>();
            services.AddScoped<ILoginRegisterService, LoginRegisterService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped <ICoursesService, CoursesService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IStudentService, StudentService>();

            return services;
        }

    }
}
