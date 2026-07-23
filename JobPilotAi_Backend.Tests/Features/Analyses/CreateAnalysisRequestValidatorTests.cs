using FluentValidation.TestHelper;
using JobPilotAi_Backend.Features.Analyses;

namespace JobPilotAi_Backend.Tests.Features.Analyses;

public sealed class CreateAnalysisRequestValidatorTests
{
    private readonly CreateAnalysisRequestValidator _validator = new();

    [Fact]
    public void Validate_WhenResumeIdIsNotGuid_ShouldReturnValidationError()
    {
        var request = new CreateAnalysisRequest("not-a-resume-id", null);

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(item => item.ResumeId)
            .WithErrorMessage("ResumeId must be a valid resume id.");
    }

    [Fact]
    public void Validate_WhenResumeIdIsGuid_ShouldNotReturnResumeIdValidationError()
    {
        var request = new CreateAnalysisRequest(Guid.NewGuid().ToString(), null);

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(item => item.ResumeId);
    }
}
