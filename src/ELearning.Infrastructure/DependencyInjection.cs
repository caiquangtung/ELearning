using ELearning.Application.Common.Interfaces;
using ELearning.Application.Common.Options;
using ELearning.Infrastructure.Ai;
using ELearning.Infrastructure.Caching;
using ELearning.Core.Abstractions;
using ELearning.Infrastructure.Commerce;
using ELearning.Infrastructure.Certificates;
using ELearning.Infrastructure.Courses;
using ELearning.Infrastructure.Identity;
using ELearning.Infrastructure.Licenses;
using ELearning.Infrastructure.Notifications;
using ELearning.Infrastructure.Orders;
using ELearning.Infrastructure.Promotions;
using ELearning.Infrastructure.TrainingClasses;
using ELearning.Infrastructure.Videos;
using ELearning.Infrastructure.Zoom;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Payments;
using ELearning.Infrastructure.Quizzes;
using ELearning.Infrastructure.Reports;
using ELearning.Infrastructure.Reviews;
using ELearning.Infrastructure.Security;
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
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(2),
                    errorCodesToAdd: null)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<ITrainingClassRepository, TrainingClassRepository>();
        services.AddScoped<ILicensePoolRepository, LicensePoolRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOrderPaymentRepository, OrderPaymentRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<ICheckoutReservationRepository, CheckoutReservationRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponRedemptionRepository, CouponRedemptionRepository>();
        services.AddScoped<ICouponUsageReservationRepository, CouponUsageReservationRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IQuizAttemptRepository, QuizAttemptRepository>();
        services.AddScoped<ICertificateRepository, CertificateRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IReportingReadService, ReportingReadService>();
        services.AddScoped<IVideoAssetRepository, VideoAssetRepository>();
        services.AddScoped<IWatchEventRepository, WatchEventRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IAiRequestLogRepository, AiRequestLogRepository>();

        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.AddSingleton<IRedisConnectionProvider, RedisConnectionProvider>();
        services.AddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();
        services.AddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
        services.AddSingleton<IRateLimitStore, RedisRateLimitStore>();

        services.Configure<PaymentOptions>(configuration.GetSection(PaymentOptions.SectionName));
        services.AddSingleton<IPaymentService, NoOpPaymentService>();
        services.AddSingleton<ICertificatePdfService, SimpleCertificatePdfService>();
        services.AddSingleton<IEmailService, NoOpEmailService>();
        services.AddSingleton<HttpClient>();
        services.AddScoped<OpenAiCompatibleChatClient>();
        services.AddScoped<GoogleAiStudioChatClient>();
        services.AddScoped<OllamaChatClient>();
        services.AddSingleton<LocalQuizQuestionGenerator>();
        services.AddSingleton<LocalEssayGradingService>();
        services.AddScoped<LocalLearningPathService>();
        services.AddScoped<OpenAiCompatibleQuizQuestionGenerator>();
        services.AddScoped<OpenAiCompatibleEssayGradingService>();
        services.AddScoped<OpenAiCompatibleLearningPathService>();
        services.AddScoped<OpenAiCompatibleTextEmbeddingService>();
        services.AddScoped<GoogleAiStudioTextEmbeddingService>();
        services.AddScoped<AiKnowledgeChunker>();
        services.AddScoped<IAiKnowledgeIndexingService, AiKnowledgeIndexingService>();
        services.AddScoped<IAiKnowledgeRetriever, AiKnowledgeRetriever>();
        services.AddScoped<AiChatIntentGate>();
        services.AddScoped<IAiQueryRouter, AiQueryRouter>();
        services.AddScoped<IAiQueryRewriter, AiQueryRewriter>();
        services.AddScoped<IAiRagChatService, AiRagChatService>();
        services.AddScoped<IAiRagEvaluationService, AiRagEvaluationService>();
        services.AddScoped<IAiKnowledgeReindexQueue, AiKnowledgeReindexQueue>();
        services.AddHostedService<AiKnowledgeReindexWorker>();
        services.AddScoped<IAiQuizQuestionGenerator, ConfigurableAiQuizQuestionGenerator>();
        services.AddScoped<IAiEssayGradingService, ConfigurableAiEssayGradingService>();
        services.AddSingleton<IAiEmbeddingService, LocalEmbeddingService>();
        services.AddSingleton<LocalDenseTextEmbeddingService>();
        services.AddScoped<IAiTextEmbeddingService, ConfigurableAiTextEmbeddingService>();
        services.AddScoped<IAiCourseRecommendationService, LocalCourseRecommendationService>();
        services.AddScoped<IAiLearnerRiskService, LocalLearnerRiskService>();
        services.AddScoped<IAiSemanticSearchService, LocalSemanticSearchService>();
        services.AddScoped<IAiLearningPathService, ConfigurableAiLearningPathService>();

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
