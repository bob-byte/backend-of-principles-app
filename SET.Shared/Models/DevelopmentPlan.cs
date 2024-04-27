using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class DevelopmentPlan
{
    public Guid Id { get; set; }
    [Required]
    public string Description { get; set; }
    [Required]
    public DevelopmentPlanType DevelopmentPlanType { get; set; }
    public ICollection<User> UsersWhichUseThis { get; set; }

}