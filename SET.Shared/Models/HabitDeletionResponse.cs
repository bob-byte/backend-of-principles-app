using System.Collections.Generic;

namespace SET.Shared.Models;

public class HabitDeletionResponse
{
    public record NotificationRequest( int Id );
    public List<NotificationRequest> DeletedNotifications { get; set; }
}
