using SET.Shared.Models;

namespace BusinessLogic;
public interface IServiceOfHabit
{
    bool ShouldHabitBeFollowed( UserHabit habit, ProgressOfHabit progress, int? countOfFollowedPerSpecificInterval );
}