using SET.Shared.Models;

namespace SET.DataAccess.Repositories.Interfaces;

public interface IStatementRepository
{
    Statement GetRandom();        
}
