namespace BusinessLogic;

public interface IGoalService
{
    Task<List<UserGoalDto>> GetActiveAsync( long userId );

    Task<List<ArchivedGoalResponse>> GetArchivedAsync( long userId );

    Task<ServiceResult> SetArchiveStatusAsync( long userId, GoalArchiveStatus goalArchiveStatus, string? originDeviceId );

    /// <summary>Unlinks the goal's habits and writes a sync tombstone before deleting.</summary>
    Task<ServiceResult> DeleteAsync( long userId, long goalId, string? originDeviceId );

    /// <summary>Creates the goal when <c>Id</c> is 0; otherwise updates it and renames matching reminders.</summary>
    Task<ServiceResult<DtoWithId>> SaveAsync( User user, UserGoalDto userGoal, string? originDeviceId );
}
