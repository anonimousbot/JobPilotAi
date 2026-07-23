using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Tests.TestSupport;

namespace JobPilotAi_Backend.Tests.Features.Analyses;

public sealed class CreateAnalysisEndpointTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Fact]
    public async Task PostAnalysis_WhenResumeIdIsNotGuid_ShouldReturnStandardErrorResponse()
    {
        using var client = factory.CreateClient();
        var request = new
        {
            resumeId = "not-a-resume-id",
            jobDescription = "Build clean backend APIs."
        };

        var response = await client.PostAsJsonAsync("/api/analyses", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        body.Should().NotBeNull();
        body!.Success.Should().BeFalse();
        body.Errors.Should().Contain("ResumeId must be a valid resume id.");
    }
}
