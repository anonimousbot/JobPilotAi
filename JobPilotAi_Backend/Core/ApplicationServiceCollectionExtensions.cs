using System.Net;
using System.Security.Authentication;
using JobPilotAi_Backend.Core.Ai;
using JobPilotAi_Backend.Core.Files;
using JobPilotAi_Backend.Core.Usage;
using Microsoft.Extensions.Options;

namespace JobPilotAi_Backend.Core;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationCore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AiProviderOptions>(configuration.GetSection(AiProviderOptions.SectionName));
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.Configure<AnthropicOptions>(configuration.GetSection(AnthropicOptions.SectionName));
        services.Configure<FileUploadOptions>(configuration.GetSection(FileUploadOptions.SectionName));
        services.AddScoped<IResumeStorage, LocalResumeStorage>();
        services.AddScoped<DeterministicAiProvider>();
        services.AddHttpClient<OpenAiProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds + 5);
            client.DefaultRequestVersion = HttpVersion.Version11;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            SslOptions =
            {
                EnabledSslProtocols = SslProtocols.Tls12
            },
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        });

        services.AddHttpClient<GeminiProvider>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds + 5);
            client.DefaultRequestVersion = HttpVersion.Version11;
            client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower;
        })
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            SslOptions =
            {
                EnabledSslProtocols = SslProtocols.Tls12
            },
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        });

        services.AddScoped<IAiProvider>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<AiProviderOptions>>().Value;
            var provider = options.Provider;

            // Enforce Gemini/Gemma as the main API, overriding local User Secrets that may default to OpenAI
            if (string.Equals(provider, "openai", StringComparison.OrdinalIgnoreCase))
            {
                provider = "gemini";
            }

            return provider.ToLowerInvariant() switch
            {
                "openai" => serviceProvider.GetRequiredService<OpenAiProvider>(),
                "gemini" => serviceProvider.GetRequiredService<GeminiProvider>(),
                "gemma" => serviceProvider.GetRequiredService<GeminiProvider>(),
                "deterministic" => serviceProvider.GetRequiredService<DeterministicAiProvider>(),
                "anthropic" => throw new NotSupportedException("Anthropic provider is reserved for a future implementation behind IAiProvider."),
                _ => throw new NotSupportedException($"AI provider '{options.Provider}' is not supported.")
            };
        });
        services.AddScoped<QuotaService>();

        return services;
    }
}
