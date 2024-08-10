using SET.Shared.Helpers;
using SET.Shared.Models.Auth;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.Services;

namespace BusinessLogic;

public class AuthService : IAuthService
{
    private readonly AppDbContext m_context;
    private readonly IConfiguration m_configuration;
    private readonly IJwtTokenService m_jwtTokenService;

    public AuthService( IServiceProvider serviceProvider )
    {
        m_context = serviceProvider.GetRequiredService<AppDbContext>();
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
        m_jwtTokenService = serviceProvider.GetRequiredService<IJwtTokenService>();
    }

    public async Task<GoogleAuthResponse> GoogleAuthAsync( string idToken, string accessToken )
    {
        GoogleJsonWebSignature.Payload payload = await ValidateGoogleTokenAsync( idToken ).DefaultConfigureAwait();

        User? user = await m_context.
            Users.
            FirstOrDefaultAsync( u => u.Email.ToLower() == payload.Email.ToLower() ).
            DefaultConfigureAwait();

        if (user is null)
        {
            Gender gender;
            try
            {
                gender = await GoogleUserGenderAsync( accessToken ).DefaultConfigureAwait();
            }
            catch (Exception ex) //an user may not provide access to his/her gender
            {
                Log.Error( ex, ex.Message );
                gender = Gender.Other;
            }

            var userRegister = new UserRegister()
            {
                Email = payload.Email,
                Name = payload.GivenName,
                Gender = gender
            };

            user = await RegisterAsync( userRegister ).DefaultConfigureAwait();
        }

        string token = m_jwtTokenService.GetToken( user );

        GoogleAuthResponse response = new( user.Id, token );
        return response;
    }

    public async Task<User> RegisterAsync( UserRegister userRegister )
    {
        //Id will set during execution of SaveChangesAsync
        User user = new()
        {
            Email = userRegister.Email,
            Name = userRegister.Name,
            Gender = userRegister.Gender,
            Password = userRegister.Password is null ? null : PasswordHelper.CreatePasswordHash( userRegister.Password ),
            MainSlogan = userRegister.MainSlogan,
            Mission = userRegister.Mission
        };

        await m_context.Users.AddAsync( user ).DefaultConfigureAwait();

        await RegisterDefaultAreasOfLifeAsync( user ).DefaultConfigureAwait();

        await m_context.SaveChangesAsync().DefaultConfigureAwait();

        Log.Information( "Successfully registered new user" );

        return user;
    }

    public async Task<(User? foundUser, string? errorMsg)> LoginAsync( UserLogin userLogin )
    {
        User? user = await m_context.Users.FirstOrDefaultAsync( u => u.Email.ToLower() == userLogin.Email.ToLower() ).DefaultConfigureAwait();
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

    private Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync( string accessToken )
    {
        string[] audience = new string[2];
        audience[0] = m_configuration["Google:AndroidClientId"]!;
        audience[1] = m_configuration["Google:iOSClientId"]!;

        GoogleJsonWebSignature.ValidationSettings validationSettings = new()
        {
            Audience = audience
        };

        return GoogleJsonWebSignature.ValidateAsync(
            accessToken,
            validationSettings
        );
    }

    private async Task<Gender> GoogleUserGenderAsync( string accessToken )
    {
        string genderScope = "https://www.googleapis.com/auth/user.gender.read";
        GoogleCredential googleCredential = GoogleCredential.
            FromAccessToken( accessToken ).
            CreateScoped( genderScope );

        var service = new PeopleServiceService( new BaseClientService.Initializer
        {
            HttpClientInitializer = googleCredential,
            ApplicationName = "Principles"
        } );

        PeopleResource.GetRequest request = service.People.Get( "people/me" );
        request.PersonFields = "genders";
        Google.Apis.PeopleService.v1.Data.Person response = await request.ExecuteAsync().DefaultConfigureAwait();
        string? gender = (response.Genders?.FirstOrDefault()?.Value)
            ?? throw new InvalidOperationException( message: "ReceivedGenderFromGoogleIsNull" );

        Gender result = gender.ToLower() switch
        {
            "male" => Gender.Man,
            "female" => Gender.Woman,
            _ => Gender.Other
        };

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
