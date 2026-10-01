using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class UserGoal
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Notes { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsCompleted { get; set; }
    public ICollection<UserHabit>? UserHabits { get; set; }
}
