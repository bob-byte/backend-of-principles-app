using AutoMapper;

using SET.DataAccess;

using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BusinessLogic;

public class ChallengeService : IChallengeService
{
    private readonly AppDbContext m_context;
    private readonly IMapper m_mapper;

    public ChallengeService( AppDbContext context, IMapper mapper )
    {
        m_context = context;
        m_mapper = mapper;
    }

    public async Task SetChallenge( ChallengeDto challengeDTO, Guid userId )
    {
        Challenge challenge = m_mapper.Map<Challenge>( challengeDTO );

        challenge.UserId = userId;

        m_context.Challenges.Add( challenge );
        m_context.SaveChanges();

        await Task.CompletedTask;
    }

    public IEnumerable<Challenge> GetChallengesByUserId( Guid userId )
    {
        return m_context.Challenges.Where( x => x.UserId == userId );
    }
}
