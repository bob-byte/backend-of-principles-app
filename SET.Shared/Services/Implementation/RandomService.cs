using SET.Shared.Services.Interfaces;
using System;

namespace SET.Shared.Services.Implementation;

public class RandomService : IRandomService
{
    private Random _random = new Random();

    public double Next()
    {
        return _random.NextDouble();
    }
}
