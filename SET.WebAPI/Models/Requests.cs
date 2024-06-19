using System;

namespace SET.WebAPI.Models;

public record SaveUserNameRequest(string UserName);
public record GenerateCodeRequest( string EmailWhereSendCode );
public record SaveUserMainSloganRequest(string MainSlogan);
public record GetAiAnswerRequest(string Prompt);

public class SaveLogRequest
{
    public long UserId { get; set; }
    public string DeviceOs { get; set; }
    public string? DeviceModelName { get; set; }
    public string DeviceType { get; set; }
    public string DeviceManufacturer { get; set; }
    public string AppVersion { get; set; }
    public string LogType { get; set; }
    public string LogMessage { get; set; }
    public string? StackTrace { get; set; }
}
