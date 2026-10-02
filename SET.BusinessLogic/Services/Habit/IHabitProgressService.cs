namespace BusinessLogic;

public interface IHabitProgressService
{
    Task<ServiceResult<ProgressSavedResponse>> UpdateProgressAsync( long userId, UpdateProgressDto progressDto );
}
