using System;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class TrainingProgram
{
    public Guid Id { get; set; }
    [Required]
    public string Name { get; set; }
    [Required]
    public string DescriptionUri { get; set; }
    [Required]
    public TimeSpan TimeExecuteForOneTraining { get; set; }

    [Required]
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; }
}