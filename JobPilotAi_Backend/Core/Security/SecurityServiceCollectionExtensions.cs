namespace JobPilotAi_Backend.Core.Security;

public static class SecurityServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationSecurity(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddHttpContextAccessor();
        services.AddScoped<AccessTokenService>();
        services.AddScoped<CurrentUserAccessor>();

        return services;
    }
}
