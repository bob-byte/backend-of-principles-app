using System;

namespace SET.WebAPI.Models;

public record RegisterResponse(string Message, string Token);
public record LoginResponse(string Message, string Token, long UserId);
public record GenerateCodeResponse( int Code );
