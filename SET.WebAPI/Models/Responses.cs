using System;

namespace SET.WebAPI.Models;

public record LoginResponse(string Token);
public record GenerateCodeResponse( int Code );
