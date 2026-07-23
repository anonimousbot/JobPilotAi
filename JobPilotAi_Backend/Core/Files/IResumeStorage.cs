namespace JobPilotAi_Backend.Core.Files;

public interface IResumeStorage
{
    Task<ResumeStorageResult> SaveAsync(Guid userId, IFormFile file, CancellationToken cancellationToken);
}
