using System;

namespace SET.WebAPI.Models;

public record SaveUserNameRequest(string UserName);
public record SaveUserMainSloganRequest(string MainSlogan);
public record GetAiAnswerRequest(string Prompt);
