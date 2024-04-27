namespace SET.Shared.Models;

public class ChallengeDto
{
    public ChallengeName Name { get; set; }

    public UserStatusInChallenge UserStatusInChallenge { get; set; }

    public string ShortDescription { get; set; }

    public string UrlWithFullDescription { get; set; }

    public string Foundator { get; set; }
}
