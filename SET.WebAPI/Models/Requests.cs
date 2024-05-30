using System;

namespace SET.WebAPI.Models;

public record SaveUserNameRequest(string UserName);
public record SaveUserMainSloganRequest(string MainSlogan);
public record GetAiAnswerRequest(string Prompt);
public record SaveLogRequest( long UserId, string DeviceOs, string DeviceModelName, string DeviceType, string DeviceManufacturer, string AppVersion, string LogType, string LogMessage, string? StackTrace );
