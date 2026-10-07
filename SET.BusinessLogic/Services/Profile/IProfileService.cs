namespace BusinessLogic;

public interface IProfileService
{
    Models.Profile GetProfile( User user );

    Task<ServiceResult> SaveNameAsync( User user, string userName );

    Task SaveMainSloganAsync( User user, string mainSlogan );

    Task<ServiceResult> SaveGenderAsync( User user, Gender gender );

    Task SaveHasSeenRoadGuideAsync( User user, bool hasSeenRoadGuide );

    /// <summary>
    /// Stores the later of the existing and incoming calendar day (max-wins).
    /// No-op when the incoming day is not newer.
    /// </summary>
    Task SaveLastAppOpenAsync( User user, DateTime lastAppOpen );

    /// <summary>Also renames reminders whose title/description was the previous mission.</summary>
    Task SaveMissionAsync( User user, string mission );
}
