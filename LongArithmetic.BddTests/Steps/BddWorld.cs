using System;
using System.Collections.Generic;
using System.Linq;
using LongArithmetic.Core.Interfaces;
using LongArithmetic.Core.Models;

namespace LongArithmetic.BddTests.Steps;

public sealed class BddWorld
{
    public string FirstOperand { get; set; } = "0";
    public string SecondOperand { get; set; } = "0";
    public string BaseOperand { get; set; } = "0";
    public int Exponent { get; set; }
    public string Method { get; set; } = string.Empty;
    public string LastResult { get; set; } = string.Empty;
    public Dictionary<string, string> AlgorithmResults { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, long> AlgorithmDurationsMs { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string BenchmarkProfile { get; set; } = string.Empty;
    public int BenchmarkDigits { get; set; }
    public List<int> CandidateSizes { get; } = [];
    public int SelectedSafeSize { get; set; }
    public long TotalExperimentMilliseconds { get; set; }
    public TestConsoleIO Console { get; } = new();
    public TestDataIO Data { get; } = new();
}

public sealed class TestConsoleIO : IConsoleIO
{
    public bool UseFileInput { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public OperationRequest ConsoleRequest { get; set; } = new()
    {
        Id = "default",
        Operation = "add",
        Method = string.Empty,
        Operands = ["0", "0"]
    };

    public bool WriteToFile { get; set; }
    public string OutputFilePath { get; set; } = string.Empty;
    public List<OperationResult> DisplayedResults { get; } = [];

    public bool AskUseFileInput() => UseFileInput;
    public string AskFilePath(string prompt) => FilePath;
    public OperationRequest AskOperationFromConsole() => ConsoleRequest;
    public void ShowResult(OperationResult result) => DisplayedResults.Add(result);
    public bool AskWriteToFile() => WriteToFile;
    public string AskOutputFilePath() => OutputFilePath;
}

public sealed class TestDataIO : IDataIO
{
    public List<OperationRequest> OperationsToRead { get; set; } = [];
    public string? ReadPath { get; private set; }
    public string? WrittenPath { get; private set; }
    public List<OperationResult> WrittenResults { get; private set; } = [];

    public void Reset()
    {
        OperationsToRead = [];
        ReadPath = null;
        WrittenPath = null;
        WrittenResults = [];
    }

    public List<OperationRequest> ReadOperations(string path)
    {
        ReadPath = path;
        return OperationsToRead;
    }

    public void WriteResults(string path, List<OperationResult> results)
    {
        WrittenPath = path;
        WrittenResults = results
            .Select(r => new OperationResult
            {
                Id = r.Id,
                Result = r.Result
            })
            .ToList();
    }
}
