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
        bool isEmailAlreadyRegistered = await m_context.Users.AnyAsync( x => x.Email == userRegister.Email ).DefaultConfigureAwait();
        if (isEmailAlreadyRegistered)
        {
            throw new InvalidOperationException( message: $"User with {userRegister.Email} email already exists" );
        }

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

    public async Task<User> LoginAsync( UserLogin userLogin )
    {
        User user = await m_context.Users.FirstOrDefaultAsync( u => u.Email == userLogin.Email ).DefaultConfigureAwait();

        return user is not null && PasswordHelper.VerifyPasswordHash( userLogin.Password, user.Password )
            ? user
            : throw new InvalidOperationException( message: "Email or password is incorrect" );
    }

    private Task RegisterDefaultAreasOfLifeAsync( User user )
    {
        //TODO: change colors when it is needed
        List<UserAreaOfLife> areasOfLife = new()
        {
            new()
            {
                Name = "Spirituality",
                Description = "SpiritualityAreaOfLifeDescription",
                Priority = 1,
                ColorName = "#5e93ff",
                User = user
            },
            new()
            {
                Name = "Character",
                Description = "CharacterAreaOfLifeDescription",
                Priority = 2,
                ColorName = "#e0123a",
                User = user
            },
            new()
            {
                Name = "Mentality",
                Description = "MentalityAreaOfLifeDescription",
                Priority = 3,
                ColorName = "#ED2939",
                User = user
            },
            new()
            {
                Name = "Health",
                Description = "HealthAreaOfLifeDescription",
                Priority = 4,
                ColorName = "#36ca3a",
                User = user
            },
            new()
            {
                Name = "Career",
                Description = "CareerAreaOfLifeDescription",
                Priority = user.Gender is Gender.Man ? 5 : 7,
                ColorName = "#749fff",
                User = user
            },
            new()
            {
                Name = "Family",
                Description = "FemilyAreaOfLifeDescription",
                Priority = user.Gender is Gender.Man ? 6 : 5,
                ColorName = "#c09fff",
                User = user
            },
            new()
            {
                Name = "Relationships",
                Description = "RelationshipsAreaOfLifeDescription",
                Priority = user.Gender is Gender.Man ? 7 : 6,
                ColorName = "#ffb53e",
                User = user
            },
            new()
            {
                Name = "Sociality",
                Description = "SocialityAreaOfLifeDescription",
                Priority = 8,
                ColorName = "#ff72e9",
                User = user
            }
        };

        return m_context.UserAreasOfLife.AddRangeAsync( areasOfLife );
    }
}
