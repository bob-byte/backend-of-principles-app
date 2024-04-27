using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SET.Shared.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public UserType UserType { get; set; }

    public string Email { get; set; }
    public byte[] Password { get; set; }

    public Gender Gender { get; set; }

    public string MainSlogan { get; set; }
    public string Mission { get; set; }
    public ICollection<UserAreaOfLife> AreasOfLife { get; set; }
    public ICollection<UserHabit> Habits { get; set; }

    public ICollection<Goal> Goals { get; set; }

    public ICollection<Challenge>? Challenges { get; set; }

    public Guid? DevelopmentPlanId { get; set; }
    public DevelopmentPlan DevelopmentPlan { get; set; }

}