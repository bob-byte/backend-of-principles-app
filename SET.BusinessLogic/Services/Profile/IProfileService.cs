namespace BusinessLogic;

public interface IProfileService
{
    Models.Profile GetProfile( User user );

    Task<ServiceResult> SaveNameAsync( User user, string userName );

    Task SaveMainSloganAsync( User user, string mainSlogan );

    Task<ServiceResult> SaveGenderAsync( User user, Gender gender );

    Task SaveHasSeenRoadGuideAsync( User user, bool hasSeenRoadGuide );

    /// <summary>Also renames reminders whose title/description was the previous mission.</summary>
    Task SaveMissionAsync( User user, string mission );
}
