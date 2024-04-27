using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class Diet
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string DescriptionUri { get; set; }

    public ICollection<Recipe> Recipes { get; set; }
    public ICollection<Rd71> Rd71s { get; set; }
}