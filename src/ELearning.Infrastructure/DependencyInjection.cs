using ELearning.Core.Abstractions;
using ELearning.Infrastructure.Courses;
using ELearning.Infrastructure.Identity;
using ELearning.Infrastructure.TrainingClasses;
using ELearning.Infrastructure.Zoom;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ELearning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ITrainingClassRepository, TrainingClassRepository>();

        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IZoomMeetingService, NoOpZoomMeetingService>();

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
