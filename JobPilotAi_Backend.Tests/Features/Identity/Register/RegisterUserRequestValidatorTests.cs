using FluentAssertions;
using JobPilotAi_Backend.Features.Identity.Register;

namespace JobPilotAi_Backend.Tests.Features.Identity.Register;

public sealed class RegisterUserRequestValidatorTests
{
    private readonly RegisterUserRequestValidator _validator = new();

    [Fact]
    public async Task ValidateAsync_WhenRequestIsValid_ShouldPass()
    {
        var request = new RegisterUserRequest("candidate@example.com", "SecurePass123");

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("short1A")]
    [InlineData("lowercaseonly1")]
    [InlineData("UPPERCASEONLY1")]
    [InlineData("NoNumberHere")]
    public async Task ValidateAsync_WhenPasswordViolatesPolicy_ShouldFail(string password)
    {
        var request = new RegisterUserRequest("candidate@example.com", password);

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RegisterUserRequest.Password));
    }
}
