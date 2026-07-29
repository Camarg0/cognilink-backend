using CogniLink.Application.Common.Interfaces;
using CogniLink.Infrastructure.Email;
using CogniLink.Infrastructure.Persistence.Firestore;
using CogniLink.Infrastructure.Security;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CogniLink.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var projectId = configuration["Firestore:ProjectId"]
            ?? throw new InvalidOperationException("Firestore:ProjectId não configurado.");

        services.AddSingleton(_ => FirestoreDb.Create(projectId));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IUserRepository, FirestoreUserRepository>();
        services.AddScoped<IRefreshTokenRepository, FirestoreRefreshTokenRepository>();
        services.AddScoped<IPasswordResetTokenRepository, FirestorePasswordResetTokenRepository>();
        services.AddScoped<IDeckRepository, FirestoreDeckRepository>();
        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }
}
