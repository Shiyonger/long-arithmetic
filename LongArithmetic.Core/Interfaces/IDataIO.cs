using System.Collections.Generic;
using LongArithmetic.Core.Models;

namespace LongArithmetic.Core.Interfaces;

public interface IDataIO
{
    List<OperationRequest> ReadOperations(string path);
    void WriteResults(string path, List<OperationResult> results);
}
