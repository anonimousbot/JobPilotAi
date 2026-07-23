using System.Net;
using System.Text.Json;
using FluentAssertions;
using JobPilotAi_Backend.Core.Ai;
using JobPilotAi_Backend.Modules.Resumes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace JobPilotAi_Backend.Tests.Features.Analyses;

public sealed class GeminiProviderTests
{
    private readonly Mock<ILogger<GeminiProvider>> _loggerMock = new();

    [Fact]
    public async Task AnalyzeResumeAsync_WhenPrimarySucceeds_ReturnsAnalysisAndDoesNotFallback()
    {
        // Arrange
        var options = new GeminiOptions
        {
            ApiKey = "primary-key",
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
            Model = "gemini-1.5-flash",
            FallbackApiKey = "nvidia-key",
            FallbackBaseUrl = "https://integrate.api.nvidia.com/v1",
            FallbackModel = "google/gemma-2-27b-it"
        };
        var optionsMock = new Mock<IOptions<GeminiOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var expectedResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = JsonSerializer.Serialize(new
                        {
                            keywordMatchScore = 80,
                            skillsCoverageScore = 75,
                            formattingScore = 90,
                            experienceScore = 70,
                            educationScore = 85,
                            weaknesses = new[] { "Weakness 1" },
                            recommendations = new[] { "Rec 1" },
                            missingKeywords = new[] { "dotnet" },
                            strengths = new[] { "Strength 1" }
                        })
                    }
                }
            },
            usage = new
            {
                prompt_tokens = 100,
                completion_tokens = 50
            }
        };

        var handler = new MockHttpMessageHandler((req, token) =>
        {
            req.Headers.Authorization!.Parameter.Should().Be("primary-key");
            req.RequestUri!.ToString().Should().Contain("generativelanguage.googleapis.com");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };
            return Task.FromResult(response);
        });

        var httpClient = new HttpClient(handler);
        var provider = new GeminiProvider(httpClient, optionsMock.Object, _loggerMock.Object);
        var resume = Resume.Create(Guid.NewGuid(), "test.pdf", ".pdf", 1024, "storage/test.pdf", "hash", "Extracted text content", DateTimeOffset.UtcNow);

        // Act
        var result = await provider.AnalyzeResumeAsync(resume, "Job description", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.KeywordMatchScore.Should().Be(80);
        result.Usage.TokensUsed.Should().Be(150);
        handler.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task AnalyzeResumeAsync_WhenPrimaryFails_FallsBackToNvidia()
    {
        // Arrange
        var options = new GeminiOptions
        {
            ApiKey = "primary-key",
            BaseUrl = "https://generativelanguage.googleapis.com/v1beta/openai/",
            Model = "gemini-1.5-flash",
            FallbackApiKey = "nvidia-key",
            FallbackBaseUrl = "https://integrate.api.nvidia.com/v1",
            FallbackModel = "google/gemma-2-27b-it"
        };
        var optionsMock = new Mock<IOptions<GeminiOptions>>();
        optionsMock.Setup(o => o.Value).Returns(options);

        var expectedResponse = new
        {
            choices = new[]
            {
                new
                {
                    message = new
                    {
                        content = JsonSerializer.Serialize(new
                        {
                            keywordMatchScore = 90,
                            skillsCoverageScore = 85,
                            formattingScore = 95,
                            experienceScore = 80,
                            educationScore = 90,
                            weaknesses = new[] { "Weakness 2" },
                            recommendations = new[] { "Rec 2" },
                            missingKeywords = new[] { "csharp" },
                            strengths = new[] { "Strength 2" }
                        })
                    }
                }
            },
            usage = new
            {
                prompt_tokens = 120,
                completion_tokens = 60
            }
        };

        var handler = new MockHttpMessageHandler((req, token) =>
        {
            if (req.Headers.Authorization!.Parameter == "primary-key")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            req.Headers.Authorization.Parameter.Should().Be("nvidia-key");
            req.RequestUri!.ToString().Should().Contain("integrate.api.nvidia.com");

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(expectedResponse))
            };
            return Task.FromResult(response);
        });

        var httpClient = new HttpClient(handler);
        var provider = new GeminiProvider(httpClient, optionsMock.Object, _loggerMock.Object);
        var resume = Resume.Create(Guid.NewGuid(), "test.pdf", ".pdf", 1024, "storage/test.pdf", "hash", "Extracted text content", DateTimeOffset.UtcNow);

        // Act
        var result = await provider.AnalyzeResumeAsync(resume, "Job description", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.KeywordMatchScore.Should().Be(90);
        result.Usage.TokensUsed.Should().Be(180);
        handler.CallCount.Should().Be(2); // First failed, second succeeded
    }

    private sealed class MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            return await sendAsync(request, cancellationToken);
        }
    }
}
