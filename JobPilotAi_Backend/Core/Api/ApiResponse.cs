namespace JobPilotAi_Backend.Core.Api;

public sealed record ApiResponse<T>(bool Success, T Data)
{
    public static ApiResponse<T> Ok(T data) => new(true, data);
}
