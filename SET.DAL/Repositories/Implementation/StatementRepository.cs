using SET.DataAccess;
using SET.DataAccess.Repositories.Interfaces;
using SET.Shared.Models;
using SET.Shared.Services.Interfaces;
using System;
using System.Linq;

namespace SET.DataAccess.Repositories.Implementation;

public class StatementRepository : IStatementRepository
{
    private readonly AppDbContext _context;
    private readonly IRandomService _randomService;

    public StatementRepository(AppDbContext context, IRandomService randomService)
    {
        _context = context;
        _randomService = randomService;
    }

    public Statement GetRandom()
    {
        int toSkip = Convert.ToInt32(Math.Floor(_context.Statements.Count() * _randomService.Next()));

        return _context.Statements.Skip(toSkip).First();
    }
}
