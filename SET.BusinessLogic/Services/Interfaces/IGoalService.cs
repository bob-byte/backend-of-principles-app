using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic;

public interface IGoalService
{
    IEnumerable<Goal> GetGoalsByUserId( Guid userId );
    Task SetGoal( GoalDto goalDTO, Guid userId );
}