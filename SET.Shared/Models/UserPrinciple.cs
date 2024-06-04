using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class UserPrinciple
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public int Priority { get; set; }
    public string? Exceptions { get; set; }
    public ICollection<PrincipleProgress> Progresses { get; set; }
    public string ReasonToFollow { get; set; }
}
