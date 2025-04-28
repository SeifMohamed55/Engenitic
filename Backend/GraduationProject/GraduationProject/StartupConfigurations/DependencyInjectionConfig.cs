using CloudinaryDotNet;
using GraduationProject.Application.Services;
using GraduationProject.Application.Services.Interfaces;
using GraduationProject.Domain.Models;
using GraduationProject.Infrastructure.Data;
using GraduationProject.Infrastructure.Data.Interfaces;
using GraduationProject.Infrastructure.Data.Repositories;
using GraduationProject.Infrastructure.Data.Repositories.Base;
using GraduationProject.Infrastructure.Data.Repositories.Base.Interfaces;
using GraduationProject.Infrastructure.Data.Repositories.interfaces;

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

            services.AddScoped<IUnitOfWork, DictionaryUnitOfWork>();
            //services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBulkRepository<QuizAnswer, int>, BulkRepository<QuizAnswer, int>>();
            services.AddScoped<IUserRepository, UsersRepository>();
            services.AddScoped<IQuizRepository, QuizRepository>();
            services.AddScoped<ICourseRepository, CoursesRepository>();
            services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            services.AddScoped<IUserLoginRepository, UserLoginRepository>();
            services.AddScoped<ITokenRepository, TokenRepository>();
            services.AddScoped<ITagsRepository, TagsRepository>();
            services.AddScoped<IFileHashRepository, FileHashRepository>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IQuizQuestionRepository, QuizQuestionRepository>();
            services.AddScoped<IReviewRepository, ReviewsRepository>();


            services.AddScoped<IUploadingService, UploadingService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICoursesService, CoursesService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IReviewService, ReviewService>();

            return services;
        }

    }
}
