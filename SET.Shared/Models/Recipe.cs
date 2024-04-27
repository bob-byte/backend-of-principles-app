using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class Recipe
{
    public Guid Id { get; set; }
    [Required]
    public string DescriptionUri { get; set; }

    public ICollection<Diet> FitDiets { get; set; }
}