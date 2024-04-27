using SET.Shared.Models;
using SET.Shared.Models.Auth;
using System.Threading.Tasks;

namespace BusinessLogic;

public interface IProgressOfHabitService
{
    double ComputeScore( double frequency, double previousScore, double checkmarkValue, int complexity );
}
