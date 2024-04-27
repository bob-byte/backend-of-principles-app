using System;
using System.ComponentModel.DataAnnotations;

namespace SET.Shared.Models;

public class Rd71
{
    public Guid Id { get; set; }
    [Required]
    public int CompletedDays { get; set; }
    /// <summary>
    /// if (IsEasyVersion) then Workout.
    /// </summary>
    [Required]
    public bool IsEasyVersion { get; set; }
    [Required]
    public DateTime MinTimeWorkoutExecute { get; set; }

    [Required]
    public Guid ReadingId { get; set; }
    public Reading Reading { get; set; }
    public Guid? DietId { get; set; }
    /// <summary>
    /// If IsEasyVersion then it is null
    /// </summary>
    public Diet Diet { get; set; }
    [Required]
    public Guid WorkoutId { get; set; }
    public Workout Workout { get; set; }
    [Required]
    public Guid UserTaskId { get; set; }
    public UserTask UserTask { get; set; }

    public Guid ChallengeId { get; set; }
    public Challenge Challenge { get; set; }
}