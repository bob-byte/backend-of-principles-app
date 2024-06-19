using SET.DataAccess;

using SET.Shared.Helpers;
using SET.Shared.Models;
using SET.Shared.Models.Auth;
using SET.Shared.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic;

public class AuthService : IAuthService
{
    private readonly AppDbContext m_context;

    public AuthService( AppDbContext context )
    {
        m_context = context;
    }

    public async Task<User> RegisterAsync( UserRegister userRegister )
    {
        //Id will set during execution of SaveChangesAsync
        var user = new User
        {
            Email = userRegister.Email,
            Name = userRegister.Name,
            Gender = userRegister.Gender,
            Password = PasswordHelper.CreatePasswordHash( userRegister.Password ),
            MainSlogan = userRegister.MainSlogan,
            Mission = userRegister.Mission
        };

        await m_context.Users.AddAsync( user ).DefaultConfigureAwait();

        await RegisterDefaultAreasOfLifeAsync( user ).DefaultConfigureAwait();

        await m_context.SaveChangesAsync().DefaultConfigureAwait();

        return user;
    }

    public async Task<(User? foundUser, string? errorMsg)> LoginAsync( UserLogin userLogin )
    {
        User? user = await m_context.Users.FirstOrDefaultAsync( u => u.Email == userLogin.Email ).DefaultConfigureAwait();
        (User? user, string? errorMsg) result;
        if (user is null)
        {
            result = (null, "EmailIsIncorrect");
        }
        else
        {
            bool isCorrectPassword = PasswordHelper.VerifyPasswordHash( userLogin.Password, user.Password );
            result = isCorrectPassword ? (user, null) : (user, "PasswordIsIncorrect" );
        }

        return result;
    }

    private Task RegisterDefaultAreasOfLifeAsync( User user )
    {
        List<UserAreaOfLife> areasOfLife = new()
        {
            new()
            {
                Name = "Spirituality",
                User = user
            },
            new()
            {
                Name = "Character",
                User = user
            },
            new()
            {
                Name = "Mentality",
                User = user
            },
            new()
            {
                Name = "Health",
                User = user
            },
            new()
            {
                Name = "Career",
                User = user
            },
            new()
            {
                Name = "HouseholdChores",
                User = user
            },
            new()
            {
                Name = "Family",
                User = user
            },
            new()
            {
                Name = "Relationships",
                User = user
            },
            new()
            {
                Name = "Sociality",
                User = user
            }
        };

        return m_context.UserAreasOfLife.AddRangeAsync( areasOfLife );
    }
}
