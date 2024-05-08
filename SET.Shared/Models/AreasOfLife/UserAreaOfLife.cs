using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;

public class UserAreaOfLife
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Priority { get; set; }
    public string Description { get; set; }
    public string ColorName { get; set; }
    public User User { get; set; }
    public Guid UserId { get; set; }
    public ICollection<UserAreaOfLifeUserHabit> Habits { get; set; }
}
