using AutoMapper;

using SET.DataAccess;

using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic;

public class GoalService : IGoalService
{
    private readonly AppDbContext m_context;
    private readonly IMapper m_mapper;

    public GoalService( AppDbContext context, IMapper mapper )
    {
        m_context = context;
        m_mapper = mapper;
    }

    public async Task SetGoal( GoalDto goalDTO, Guid userId )
    {
        Goal goal = m_mapper.Map<Goal>( goalDTO );

        goal.UserId = userId;

        m_context.Goals.Add( goal );
        m_context.SaveChanges();

        await Task.CompletedTask;
    }

    public IEnumerable<Goal> GetGoalsByUserId( Guid userId )
    {
        return m_context.Goals.Where( x => x.UserId == userId ).ToList();
    }
}
