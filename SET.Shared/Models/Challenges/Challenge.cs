using System;

namespace SET.Shared.Models;

///<summary>
/// Implements <a href="https://martinfowler.com/eaaCatalog/">Class Table Inheritance</a> pattern
///</summary>
public class Challenge
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    
    public UserStatusInChallenge UserStatusInChallenge { get; set; }

    public string ShortDescription { get; set; }
    
    public string UrlWithFullDescription { get; set; }

    public string Foundator { get; set; }

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public Rd71? Rd71 { get; set; }
}
