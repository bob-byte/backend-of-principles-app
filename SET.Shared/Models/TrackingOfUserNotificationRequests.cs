using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class TrackingOfUserNotificationRequests
{
    public long Id { get; set; }
    public int MaxNotificationRequestId { get; set; }
    public long UserId { get; set; }
    public User User { get; set; }
}
