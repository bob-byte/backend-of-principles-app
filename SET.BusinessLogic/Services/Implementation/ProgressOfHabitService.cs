using SET.Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BusinessLogic;

public class ProgressOfHabitService : IProgressOfHabitService
{
    


    public double ComputeScore( double frequency, double previousScore, double checkmarkValue, int complexity )
    {
        double coefOfComplexity = complexity switch
        {
            1 => 5.6,
            2 => 3.33,
            3 => 2.3,
            4 => 1.7,
            5 => 1.525,
            6 => 1.4,
            7 => 1.0,
            8 => 0.77,
            9 => 0.524,
            10 => 0.392,
            _ => throw new ArgumentException( message: $"Complexity {complexity} of habit is not supported",
            paramName: nameof( complexity ) )
        };

        double exponentialSmoothingFactor = 0.5;
        double scalingFactor = 13.0;
        double decayFactor = Math.Pow( exponentialSmoothingFactor, Math.Sqrt( frequency ) * coefOfComplexity / scalingFactor );

        double score = previousScore * decayFactor;
        score += checkmarkValue * (1 - decayFactor);
        return score;
    }
}

