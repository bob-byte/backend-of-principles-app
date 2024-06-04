using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Shared.Models;
public class PrincipleProgress
{
    public long Id { get; set; }
    public int Value { get; set; }
    public DateOnly Date { get; set; }
    public long PrincipleId { get; set; }
    public UserPrinciple Principle { get; set; }
}
