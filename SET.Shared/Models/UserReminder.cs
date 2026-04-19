using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class UserReminder
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TimeOnly Time { get; set; }
    public bool IsEnabled { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
    public int UserNotificationRequestId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
