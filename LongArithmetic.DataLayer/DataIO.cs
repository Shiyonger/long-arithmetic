using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LongArithmetic.Core.Interfaces;
using LongArithmetic.Core.Models;

namespace LongArithmetic.DataLayer;

public class DataIO : IDataIO
{
    public List<OperationRequest> ReadOperations(string path)
    {
        var json = File.ReadAllText(path);
        var ops = JsonSerializer.Deserialize<List<OperationRequest>>(json);
        return ops ?? [];
    }

    public void WriteResults(string path, List<OperationResult> results)
    {
        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }
}
