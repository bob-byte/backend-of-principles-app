using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class Workout
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }

    public ICollection<TrainingProgram> AvailableTrainingPrograms { get; set; }
    public ICollection<Rd71> Rd71s { get; set; }
}