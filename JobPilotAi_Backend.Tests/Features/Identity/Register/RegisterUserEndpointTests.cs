using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Features.Identity.Register;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Tests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace JobPilotAi_Backend.Tests.Features.Identity.Register;

public sealed class RegisterUserEndpointTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    [Fact]
    public async Task PostRegister_WhenRequestIsValid_ShouldPersistUserAndReturnCreated()
    {
        using var client = factory.CreateClient();
        var request = new RegisterUserRequest("candidate@example.com", "SecurePass123");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterUserResponse>();
        body.Should().NotBeNull();
        body!.Email.Should().Be(request.Email);
        body.Role.Should().Be("FreeUser");
        body.EmailConfirmed.Should().BeFalse();

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Users.Should().ContainSingle(user => user.Email == request.Email);
    }

    [Fact]
    public async Task PostRegister_WhenRequestIsInvalid_ShouldReturnStandardErrorResponse()
    {
        using var client = factory.CreateClient();
        var request = new RegisterUserRequest("not-an-email", "weak");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        body.Should().NotBeNull();
        body!.Success.Should().BeFalse();
        body.Errors.Should().NotBeEmpty();
    }
}
