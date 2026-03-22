using System;
using System.Collections.Generic;
using System.Linq;
using LongArithmetic.Core;
using LongArithmetic.Core.Interfaces;
using LongArithmetic.Core.Models;

namespace LongArithmetic;

public class CalculatorService(IConsoleIO console, IDataIO data)
{
    private readonly IConsoleIO _console = console;
    private readonly IDataIO _data = data;

    public void Run()
    {
        if (_console.AskUseFileInput())
        {
            var path = _console.AskFilePath("Enter JSON input file path:");
            var ops = _data.ReadOperations(path);
            var results = new List<OperationResult>();
            foreach (var op in ops)
            {
                var r = ProcessOperation(op);
                results.Add(new OperationResult { Id = op.Id, Result = r });
            }

            var outPath = _console.AskOutputFilePath();
            _data.WriteResults(outPath, results);
            foreach (var res in results) _console.ShowResult(res);
            return;
        }

        var request = _console.AskOperationFromConsole();
        var result = ProcessOperation(request);
        var opRes = new OperationResult { Id = request.Id, Result = result };
        _console.ShowResult(opRes);
        if (_console.AskWriteToFile())
        {
            var outPath = _console.AskOutputFilePath();
            _data.WriteResults(outPath, [opRes]);
        }
    }

    private string ProcessOperation(OperationRequest req)
    {
        if (req.Operands == null || req.Operands.Count < 2 ||
            req.Operands.Any(o => !IsPositiveInteger(o)))
            return "Некорректные данные";

        try
        {
            switch (req.Operation.ToLower())
            {
                case "add":
                    return BigMath.Add(new BigNumber(req.Operands[0]), new BigNumber(req.Operands[1])).Value;
                case "subtract":
                    return BigMath.Subtract(new BigNumber(req.Operands[0]), new BigNumber(req.Operands[1])).Value;
                case "multiply":
                    return BigMath.Multiply(new BigNumber(req.Operands[0]), new BigNumber(req.Operands[1]), req.Method)
                        .Value;
                case "divide":
                    return BigMath.Divide(new BigNumber(req.Operands[0]), new BigNumber(req.Operands[1]), req.Method)
                        .Value;
                case "pow":
                    var exp = int.Parse(req.Operands[1]);
                    return BigMath.Pow(new BigNumber(req.Operands[0]), exp, req.Method).Value;
                default:
                    return "Unsupported operation";
            }
        }
        catch (Exception e)
        {
            return "Error: " + e.Message;
        }
    }

    private static bool IsPositiveInteger(string s)
    {
        return !string.IsNullOrWhiteSpace(s) && s.All(char.IsDigit);
    }
}