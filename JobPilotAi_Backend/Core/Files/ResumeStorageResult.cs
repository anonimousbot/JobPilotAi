namespace JobPilotAi_Backend.Core.Files;

public sealed record ResumeStorageResult(string StoragePath, string ContentHash, string? ExtractedText);
