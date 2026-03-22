using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using LongArithmetic.Core;
using LongArithmetic.Core.Models;
using LongArithmetic.DataLayer;

var options = LoadTestOptions.Parse(args);
var runner = new LoadTestRunner();
var result = runner.Run(options);

Console.WriteLine(LoadTestReportFormatter.ToMarkdown(result));

file sealed record LoadTestOptions(
    string Algorithm,
    string Profile,
    int Digits,
    int BatchSize,
    string OutputDirectory)
{
    public static LoadTestOptions Parse(string[] args)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var arg in args)
        {
            var parts = arg.Split('=', 2, StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                values[parts[0].TrimStart('-', '/')] = parts[1];
            }
        }

        return new LoadTestOptions(
            Algorithm: values.GetValueOrDefault("algorithm", "classic"),
            Profile: values.GetValueOrDefault("profile", "dense"),
            Digits: int.Parse(values.GetValueOrDefault("digits", "120"), CultureInfo.InvariantCulture),
            BatchSize: int.Parse(values.GetValueOrDefault("batch", "100"), CultureInfo.InvariantCulture),
            OutputDirectory: values.GetValueOrDefault("out", Path.Combine(Environment.CurrentDirectory, "load-test-artifacts")));
    }
}

file sealed class LoadTestRunner
{
    public LoadTestRunResult Run(LoadTestOptions options)
    {
        Directory.CreateDirectory(options.OutputDirectory);

        var dataIo = new DataIO();
        var inputPath = Path.Combine(
            options.OutputDirectory,
            $"input-{options.Algorithm}-{options.Profile}-{options.Digits}d-{options.BatchSize}b.json");
        var outputPath = Path.Combine(
            options.OutputDirectory,
            $"output-{options.Algorithm}-{options.Profile}-{options.Digits}d-{options.BatchSize}b.json");

        ForceGc();
        var beforeGenerationBytes = GC.GetTotalMemory(true);
        var generationWatch = Stopwatch.StartNew();
        var operations = GenerateOperations(options.Profile, options.Digits, options.BatchSize, options.Algorithm);
        generationWatch.Stop();
        var afterGenerationBytes = GC.GetTotalMemory(true);

        var jsonInput = BuildOperationJson(operations);
        var inputWriteWatch = Stopwatch.StartNew();
        File.WriteAllText(inputPath, jsonInput);
        inputWriteWatch.Stop();

        ForceGc();
        var loadWatch = Stopwatch.StartNew();
        var loadedOperations = dataIo.ReadOperations(inputPath);
        loadWatch.Stop();

        ForceGc();
        var beforeSolveBytes = GC.GetTotalMemory(true);
        var solveWatch = Stopwatch.StartNew();
        var results = new List<OperationResult>(loadedOperations.Count);
        foreach (var operation in loadedOperations)
        {
            var product = BigMath.Multiply(
                new BigNumber(operation.Operands[0]),
                new BigNumber(operation.Operands[1]),
                operation.Method);

            results.Add(new OperationResult
            {
                Id = operation.Id,
                Result = product.Value
            });
        }

        solveWatch.Stop();
        var afterSolveBytes = GC.GetTotalMemory(true);

        var saveWatch = Stopwatch.StartNew();
        dataIo.WriteResults(outputPath, results);
        saveWatch.Stop();

        return new LoadTestRunResult(
            options.Algorithm,
            options.Profile,
            options.Digits,
            options.BatchSize,
            generationWatch.Elapsed,
            loadWatch.Elapsed,
            solveWatch.Elapsed,
            saveWatch.Elapsed,
            inputWriteWatch.Elapsed,
            afterGenerationBytes - beforeGenerationBytes,
            afterSolveBytes - beforeSolveBytes,
            inputPath,
            outputPath,
            results.Count);
    }

    private static List<OperationRequest> GenerateOperations(string profile, int digits, int batchSize, string algorithm)
    {
        var operations = new List<OperationRequest>(batchSize);
        for (var i = 0; i < batchSize; i++)
        {
            operations.Add(new OperationRequest
            {
                Id = $"op-{i + 1}",
                Operation = "multiply",
                Method = algorithm,
                Operands =
                [
                    GenerateOperand(profile, digits, i, reverse: false),
                    GenerateOperand(profile, digits, i, reverse: true)
                ]
            });
        }

        return operations;
    }

    private static string GenerateOperand(string profile, int digits, int index, bool reverse)
    {
        var content = profile.Equals("sparse", StringComparison.OrdinalIgnoreCase)
            ? GenerateSparseDigits(digits, index)
            : GenerateDenseDigits(digits, index);

        if (reverse)
        {
            content = new string(content.Reverse().ToArray());
        }

        if (content[0] == '0')
        {
            content = "7" + content[1..];
        }

        return content;
    }

    private static string GenerateDenseDigits(int digits, int index)
    {
        var pattern = index % 2 == 0 ? "98765432101234567890" : "31415926535897932384";
        return RepeatPattern(pattern, digits);
    }

    private static string GenerateSparseDigits(int digits, int index)
    {
        var builder = new char[digits];
        for (var i = 0; i < digits; i++)
        {
            builder[i] = (i + index) % 19 == 0 ? '9' : '0';
        }

        return new string(builder);
    }

    private static string RepeatPattern(string pattern, int digits)
    {
        var buffer = new char[digits];
        for (var i = 0; i < digits; i++)
        {
            buffer[i] = pattern[i % pattern.Length];
        }

        return new string(buffer);
    }

    private static string BuildOperationJson(List<OperationRequest> operations)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        writer.WriteStartArray();
        foreach (var operation in operations)
        {
            writer.WriteStartObject();
            writer.WriteString("Id", operation.Id);
            writer.WriteString("Operation", operation.Operation);
            writer.WriteString("Method", operation.Method);
            writer.WritePropertyName("Operands");
            writer.WriteStartArray();
            foreach (var operand in operation.Operands)
            {
                writer.WriteStringValue(operand);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.Flush();

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void ForceGc()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }
}

file sealed record LoadTestRunResult(
    string Algorithm,
    string Profile,
    int Digits,
    int BatchSize,
    TimeSpan GenerationTime,
    TimeSpan LoadTime,
    TimeSpan SolveTime,
    TimeSpan SaveTime,
    TimeSpan InputWriteTime,
    long GenerationMemoryDeltaBytes,
    long SolveMemoryDeltaBytes,
    string InputPath,
    string OutputPath,
    int ProcessedOperations)
{
    public TimeSpan TotalTime => GenerationTime + InputWriteTime + LoadTime + SolveTime + SaveTime;
}

file static class LoadTestReportFormatter
{
    public static string ToMarkdown(LoadTestRunResult result)
    {
        return string.Join(
            Environment.NewLine,
            $"# Load test result",
            $"",
            $"| Parameter | Value |",
            $"|---|---|",
            $"| Algorithm | {result.Algorithm} |",
            $"| Profile | {result.Profile} |",
            $"| Digits per operand | {result.Digits} |",
            $"| Batch size | {result.BatchSize} |",
            $"| Processed operations | {result.ProcessedOperations} |",
            $"| Generation time, ms | {result.GenerationTime.TotalMilliseconds:F2} |",
            $"| Input write time, ms | {result.InputWriteTime.TotalMilliseconds:F2} |",
            $"| Load time, ms | {result.LoadTime.TotalMilliseconds:F2} |",
            $"| Solve time, ms | {result.SolveTime.TotalMilliseconds:F2} |",
            $"| Save time, ms | {result.SaveTime.TotalMilliseconds:F2} |",
            $"| Total time, ms | {result.TotalTime.TotalMilliseconds:F2} |",
            $"| Generation memory delta, bytes | {result.GenerationMemoryDeltaBytes} |",
            $"| Solve memory delta, bytes | {result.SolveMemoryDeltaBytes} |",
            $"| Input path | {result.InputPath} |",
            $"| Output path | {result.OutputPath} |");
    }
}
