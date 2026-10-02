namespace BusinessLogic;

/// <summary>
/// One-off tasks. Writes wake the user's other devices (except <c>originDeviceId</c>) after commit.
/// </summary>
public interface ITaskService
{
    Task<ServiceResult<TaskItemDto>> CreateAsync( long userId, TaskItemDto request, string? originDeviceId );

    Task<ServiceResult<List<TaskItemDto>>> GetByDateAsync( long userId, DateOnly? date );

    Task<List<TaskItemDto>> GetAllAsync( long userId );

    /// <summary>Open tasks without a date or due today or later.</summary>
    Task<List<TaskItemDto>> GetInboxAsync( long userId );

    Task<ServiceResult<TaskItemDto>> UpdateStatusAsync( long userId, long id, UpdateTaskStatusDto request, string? originDeviceId );

    Task<ServiceResult<TaskItemDto>> UpdateAsync( long userId, long id, TaskItemDto request, string? originDeviceId );

    Task<ServiceResult> DeleteAsync( long userId, long id, string? originDeviceId );
}
