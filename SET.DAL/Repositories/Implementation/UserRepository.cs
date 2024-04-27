using SET.DataAccess;
using SET.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using SET.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SET.DataAccess.Repositories.Implementation;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }
}
