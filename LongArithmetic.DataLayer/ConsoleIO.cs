using LongArithmetic.Core.Interfaces;
using LongArithmetic.Core.Models;

namespace LongArithmetic.DataLayer;

public class ConsoleIO : IConsoleIO
{
    public bool AskUseFileInput()
    {
        Console.WriteLine("Use JSON file input? (y/n)");
        var k = Console.ReadLine();
        return k != null && k.Trim().StartsWith("y", StringComparison.CurrentCultureIgnoreCase);
    }

    public string AskFilePath(string prompt)
    {
        Console.WriteLine(prompt);
        return Console.ReadLine() ?? string.Empty;
    }

    public OperationRequest AskOperationFromConsole()
    {
        Console.WriteLine("Only positive integer operands are allowed.");
        Console.WriteLine("Enter operation (add, subtract, multiply, divide, pow):");
        var op = Console.ReadLine() ?? "";

        var method = "";
        switch (op)
        {
            case "multiply":
                Console.WriteLine("Available methods: classic, karatsuba");
                Console.WriteLine("Enter method name (or leave empty for auto):");
                method = Console.ReadLine() ?? "";
                break;
            case "divide":
                Console.WriteLine("Available methods: longdivision, binary");
                Console.WriteLine("Enter method name (or leave empty for auto):");
                method = Console.ReadLine() ?? "";
                break;
            case "pow":
                Console.WriteLine("Available methods: iterative, binary");
                Console.WriteLine("Enter method name (or leave empty for auto):");
                method = Console.ReadLine() ?? "";
                break;
        }

        var operands = new List<string>();
        if (op == "pow")
        {
            Console.WriteLine("Enter base:");
            operands.Add(Console.ReadLine() ?? "0");
            Console.WriteLine("Enter exponent (int):");
            operands.Add(Console.ReadLine() ?? "0");
        }
        else
        {
            Console.WriteLine("Enter first operand:");
            operands.Add(Console.ReadLine() ?? "0");
            Console.WriteLine("Enter second operand:");
            operands.Add(Console.ReadLine() ?? "0");
        }

        return new OperationRequest
        {
            Id = Guid.NewGuid().ToString(),
            Operation = op,
            Method = method,
            Operands = operands
        };
    }

    public void ShowResult(OperationResult result)
    {
        Console.WriteLine($"Result for {result.Id}: {result.Result}");
    }

    public bool AskWriteToFile()
    {
        Console.WriteLine("Write results to file? (y/n)");
        var k = Console.ReadLine();
        return k != null && k.Trim().StartsWith("y", StringComparison.CurrentCultureIgnoreCase);
    }

    public string AskOutputFilePath()
    {
        Console.WriteLine("Enter output file path:");
        return Console.ReadLine() ?? string.Empty;
    }
}
