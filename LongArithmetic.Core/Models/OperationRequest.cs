using System.Collections.Generic;

namespace LongArithmetic.Core.Models;

public class OperationRequest
{
    public string Id { get; set; }
    public string Operation { get; set; }
    public string Method { get; set; }
    public List<string> Operands { get; set; }
}
