using System;
namespace SET.Shared.Models;

public class ClientLog
{
    public long Id { get; set; }
    public User? User { get; set; }
    public long? UserId { get; set; }
    public string DeviceOs { get; set; }
    public string? DeviceModelName { get; set; }
    public string DeviceType { get; set; }
    public string DeviceManufacturer { get; set; }
    public string AppVersion { get; set; }
    public string LogType { get; set; }
    public string LogMessage { get; set; }
    public string? StackTrace { get; set; }
}
