using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic;

public interface IChallengeService
{
    IEnumerable<Challenge> GetChallengesByUserId( Guid userId );
    Task SetChallenge( ChallengeDto challengeDTO, Guid userId );
}