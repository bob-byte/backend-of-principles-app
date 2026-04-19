namespace SET.WebAPI.Models;

public class SyncConflictResponse
{
    public string EntityType { get; set; }
    public long EntityId { get; set; }
    public DateTime ServerLastModified { get; set; }
    public DateTime ClientLastModified { get; set; }
}
