using FluentValidation;
using JobPilotAi_Backend.Core.Files;
using Microsoft.Extensions.Options;

namespace JobPilotAi_Backend.Features.Resumes;

public sealed class UploadResumeRequestValidator : AbstractValidator<UploadResumeRequest>
{
    private static readonly string[] AllowedExtensions = [".pdf", ".docx"];

    public UploadResumeRequestValidator(IOptions<FileUploadOptions> options)
    {
        RuleFor(request => request.File)
            .NotNull()
            .Must(file => file.Length > 0).WithMessage("Resume file cannot be empty.")
            .Must(file => file.Length <= options.Value.MaxResumeBytes).WithMessage("Resume file cannot exceed 10 MB.")
            .Must(file => AllowedExtensions.Contains(Path.GetExtension(file.FileName), StringComparer.OrdinalIgnoreCase))
            .WithMessage("Resume file must be a PDF or DOCX.");
    }
}
